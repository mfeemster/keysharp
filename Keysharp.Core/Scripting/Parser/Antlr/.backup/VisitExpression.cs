using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static MainParser;
using static Keysharp.Scripting.Parser;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Keysharp.Core.Scripting.Parser.Antlr.Helper;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.AccessControl;
using System.Configuration;

namespace Keysharp.Core.Scripting.Parser.Antlr
{
    public partial class MainVisitor : MainParserBaseVisitor<SyntaxNode>
    {
        // Converts identifiers such as a%b% to
        // Keysharp.Scripting.Script.Vars[string.Concat<object>(new object[] {"a", b)]
        public SyntaxNode CreateDynamicVariableString(ParserRuleContext context)
        {
            // Collect the parts of the dereference expression
            var parts = new List<ExpressionSyntax>();

            foreach (var child in context.children)
            {
                if (child is ITerminalNode || child is IdentifierContext)
                    parts.Add(SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(child.GetText().Trim())
                    ));
                else
                    parts.Add((ExpressionSyntax)Visit(child));
            }

            // Determine if there is only a single part (no string concatenation needed)
            ExpressionSyntax combinedExpression;
            if (parts.Count == 1)
                combinedExpression = parts[0];
            else
            {
                // Create string.Concat<object>(new object[] { ... }) for multiple parts
                combinedExpression = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("System.String"), // Roslyn doesn't like lowercase "string"
                        SyntaxFactory.IdentifierName("Concat")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.ArrayCreationExpression(
                                    SyntaxFactory.ArrayType(
                                        SyntaxFactory.PredefinedType(
                                            SyntaxFactory.Token(SyntaxKind.ObjectKeyword)
                                        ),
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.ArrayRankSpecifier(
                                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                    SyntaxFactory.OmittedArraySizeExpression()
                                                )
                                            )
                                        )
                                    ),
                                    SyntaxFactory.InitializerExpression(
                                        SyntaxKind.ArrayInitializerExpression,
                                        SyntaxFactory.SeparatedList(parts)
                                    )
                                )
                            )
                        )
                    )
                );
            }

            // Wrap the combined expression in Keysharp.Scripting.Script.Vars[...]
            return combinedExpression;
        }

        public override SyntaxNode VisitDerefContinuation([NotNull] DerefContinuationContext context)
        {
            if (context.singleExpression() != null)
                return Visit(context.singleExpression());
            else
                return SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(context.GetText())
                    );
        }

        public override SyntaxNode VisitDereference([NotNull] DereferenceContext context)
        {
            return Visit(context.singleExpression());
        }

        public override SyntaxNode VisitMemberIdentifier([NotNull] MemberIdentifierContext context)
        {
            if (context.dynamicIdentifier() == null)
                return SyntaxFactory.IdentifierName(context.GetText().ToLowerInvariant());
            return base.VisitMemberIdentifier(context);
        }

        public override SyntaxNode VisitIdentifierExpression([NotNull] IdentifierExpressionContext context)
        {
            var result = Visit(context.identifier());
            return result;
        }

        public override SyntaxNode VisitPropertyName([NotNull] PropertyNameContext context)
        {
            return SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(context.GetText().Trim('"', ' '))
                    );
        }

        public override SyntaxNode VisitFunctionCallExpression([NotNull] FunctionCallExpressionContext context)
        {
            ExpressionSyntax targetExpression = null;
            // Visit the singleExpression (the method to be called)
            string methodName = context.primary().GetText();
            if (!state.UserFuncs.Contains(methodName) && Reflections.FindBuiltInMethod(methodName, -1) is MethodPropertyHolder mph && mph.mi != null)
                targetExpression = CreateQualifiedName($"{mph.mi.DeclaringType}.{mph.mi.Name}");
            else
            {
                targetExpression = (ExpressionSyntax)Visit(context.primary());
                methodName = ExtractMethodName(targetExpression);
            }

            // Get the argument list
            ArgumentListSyntax argumentList;
            if (context.functionCallArguments()?.arguments() != null)
                argumentList = (ArgumentListSyntax)VisitArguments(context.functionCallArguments().arguments());
            else
                argumentList = SyntaxFactory.ArgumentList();

            return state.GenerateFunctionInvocation(targetExpression, argumentList, methodName);
        }

        private ExpressionSyntax GenerateMemberIndexAccess(PrimaryContext singleExpression, MemberIndexArgumentsContext memberIndexArguments, bool isOptional = false)
        {
            // Visit the main expression (e.g., c in c[1])
            var targetExpression = (ExpressionSyntax)Visit(singleExpression);

            // Visit the expressionSequence to generate an ArgumentListSyntax
            var exprArgSeqContext = memberIndexArguments.expressionSequence();
            var argumentList = exprArgSeqContext == null ? SyntaxFactory.ArgumentList() : (ArgumentListSyntax)VisitExpressionSequence(exprArgSeqContext);

            // Prepend the targetExpression as the first argument
            var fullArgumentList = argumentList.WithArguments(
                argumentList.Arguments.Insert(0, SyntaxFactory.Argument(targetExpression))
            );

            // Generate the invocation: Keysharp.Scripting.Script.Index(target, index)
            var indexInvocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"), // Class name
                    SyntaxFactory.IdentifierName("Index") // Method name
                ),
                fullArgumentList
            );

            // Handle optional chaining: if '? ' is present
            if (isOptional)
            {
                return SyntaxFactory.ConditionalExpression(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        targetExpression,
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    ),
                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
                    indexInvocation
                );
            }

            return indexInvocation;
        }

        public override SyntaxNode VisitMemberIndexExpression([NotNull] MemberIndexExpressionContext context)
        {
            return GenerateMemberIndexAccess(context.primary(), context.memberIndexArguments(), context.GetChild(1)?.GetText() == "?");
        }

        public override SyntaxNode VisitExpressionStatement([Antlr4.Runtime.Misc.NotNull] ExpressionStatementContext context)
        {
            var argumentList = (ArgumentListSyntax)base.VisitExpressionStatement(context);
            ExpressionSyntax singleExpression = null;
            ExpressionSyntax functionTargetExpression = null;
            if (argumentList.Arguments.Count == 0)
                throw new Error("Expression count can't be 0");

            singleExpression = argumentList.Arguments[0].Expression;

            // Check whether this might qualify for a function call statement, and if yes then turn it into an invocation instead
            if ((singleExpression is IdentifierNameSyntax || singleExpression is IdentifierNameSyntax
                || singleExpression is MemberAccessExpressionSyntax) && singleExpression.GetAnnotatedNodes("FunctionDeclaration").FirstOrDefault() == null)
            {
                // The first argument matches a function call statement pattern, so extract the method name
                string methodName = ExtractMethodName(singleExpression);
                // Also, if the original expression list contains more arguments then the first argument will be null
                // For example the input might have been `MsgBox , "world"`
                argumentList = argumentList.Arguments.Count == 1 
                    ? SyntaxFactory.ArgumentList() 
                    : SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)) }.Concat(argumentList.Arguments))); 

                return SyntaxFactory.ExpressionStatement(state.GenerateFunctionInvocation(singleExpression, argumentList, methodName));
            }
            // The other function call case is a Concat expression in cases like `MsgBox "hello"`, so check for that
            else if (singleExpression is InvocationExpressionSyntax invocation)
            {
                // Get the argument list of the invocation
                var invocationArgumentList = invocation.ArgumentList;
                if (invocationArgumentList.Arguments.Count >= 3)
                {
                    // Check if the first argument is "Concat"
                    var firstArgument = invocationArgumentList.Arguments[0].Expression as QualifiedNameSyntax;
                    if (firstArgument != null && firstArgument.Right is IdentifierNameSyntax identifier && identifier.Identifier.Text == "Concat")
                    {
                        // Extract the second and third arguments
                        var secondArgument = invocationArgumentList.Arguments[1].Expression;
                        var thirdArgument = invocationArgumentList.Arguments[2].Expression;

                        // The first expression in the Concat call matches a function call expression
                        if (secondArgument is IdentifierNameSyntax || secondArgument is IdentifierNameSyntax
                            || secondArgument is MemberAccessExpressionSyntax)
                        {
                            string methodName = ExtractMethodName(secondArgument);
                            argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(thirdArgument) }.Concat(argumentList.Arguments.Skip(1))));

                            return SyntaxFactory.ExpressionStatement(state.GenerateFunctionInvocation(secondArgument, argumentList, methodName));
                        }
                    }
                }
            }

            if (argumentList.Arguments.Count == 1)
            {
                // Validate and convert the expression if necessary
                singleExpression = EnsureValidStatementExpression(singleExpression);

                return SyntaxFactory.ExpressionStatement(singleExpression);
            }
            else
            {
                 // Wrap each expression if needed and create a block
                return SyntaxFactory.Block(argumentList.Arguments
                    .Select(arg =>
                    {
                        var expression = EnsureValidStatementExpression(arg.Expression);
                        return SyntaxFactory.ExpressionStatement(expression);
                    })
                    .ToList());
            }
        }

        private ExpressionSyntax EnsureValidStatementExpression(ExpressionSyntax expression)
        {
            // Check if the expression is valid as a statement
            if (expression is InvocationExpressionSyntax ||
                expression is AssignmentExpressionSyntax ||
                expression is PostfixUnaryExpressionSyntax ||
                expression is PrefixUnaryExpressionSyntax ||
                expression is AwaitExpressionSyntax ||
                expression is ObjectCreationExpressionSyntax)
            {
                return expression; // It's valid, return as-is
            }

            // If not valid, wrap it in a dummy call
            
            return ((InvocationExpressionSyntax)InternalMethods.MultiStatement)
                .WithArgumentList(
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.Argument(expression) 
                    })));
        }

        public override SyntaxNode VisitParenthesizedExpression([NotNull] ParenthesizedExpressionContext context)
        {
            ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(context.expressionSequence());
            if (argumentList.Arguments.Count == 1)
                return argumentList.Arguments[0].Expression;

            return ((InvocationExpressionSyntax)InternalMethods.MultiStatement)
                .WithArgumentList(argumentList);
        }

        public override SyntaxNode VisitExpressionSequence(ExpressionSequenceContext context)
        {
            var arguments = new List<ArgumentSyntax>();

            var lastChildText = ",";
            for (var i = 0; i < context.ChildCount; i++)
            {
                var child = context.GetChild(i);
                if (child is ITerminalNode node && node.Symbol.Type == EOL)
                    continue;
                var childText = child.GetText();

                if (childText == ",")
                {
                    if (lastChildText == ",")
                        arguments.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));

                    goto ShouldVisitNextChild;
                }
                arguments.Add(SyntaxFactory.Argument((ExpressionSyntax)Visit((SingleExpressionContext)child)));

                ShouldVisitNextChild:
                lastChildText = childText;
            }

            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
        }

        private int counter = 0;
        public override SyntaxNode VisitImplicitConcatenateExpression([NotNull] ImplicitConcatenateExpressionContext context)
        {
            var arguments = new List<ExpressionSyntax> { 
                (ExpressionSyntax)Visit(context.singleExpression(0)),
                (ExpressionSyntax)Visit(context.singleExpression(1))
            };

            return ConcatenateExpressions(arguments);
        }

        /*
        public override SyntaxNode VisitConcatenateExpression([NotNull] ConcatenateExpressionContext context)
        {
            return ConcatenateExpressions(
                new List<ExpressionSyntax> {
                    (ExpressionSyntax)Visit(context.singleExpression()),
                    (ExpressionSyntax)Visit(context.singleExpressionConcatenation())
            });
        }
        */

        public ExpressionSyntax ConcatenateExpressions(List<ExpressionSyntax> expressions)
        {
            ExpressionSyntax finalExpression = expressions[0];
            if (expressions.Count == 1)
                return finalExpression;

            for (var i = 1; i < expressions.Count; i++)
            {
                finalExpression = CreateBinaryOperatorExpression(MainParser.Dot, finalExpression, expressions[i]);
            }
            return finalExpression;
        }

        public override SyntaxNode VisitPreIncrementExpression([NotNull] PreIncrementExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());
            return HandleCompoundAssignment(expression, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)), "+=");
        }

        public override SyntaxNode VisitPreDecreaseExpression([NotNull] PreDecreaseExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());
            return HandleCompoundAssignment(expression, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)), "-=");
        }

        public override SyntaxNode VisitPostIncrementExpression([NotNull] PostIncrementExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());
            return HandleCompoundAssignment(expression, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)), "+=", isPostFix: true);
        }

        public override SyntaxNode VisitPostDecreaseExpression([NotNull] PostDecreaseExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());
            return HandleCompoundAssignment(expression, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)), "-=", isPostFix: true);
        }

        public override SyntaxNode VisitPowerExpression([NotNull] PowerExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitUnaryMinusExpression([NotNull] UnaryMinusExpressionContext context)
        {
            return HandleUnaryExpressionVisit(context);
        }

        public override SyntaxNode VisitUnaryPlusExpression([NotNull] UnaryPlusExpressionContext context)
        {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryPlusExpression, (ExpressionSyntax)Visit(context.GetChild(1)));
        }

        public override SyntaxNode VisitBitNotExpression([NotNull] BitNotExpressionContext context)
        {
            return HandleUnaryExpressionVisit(context);
        }

        public SyntaxNode HandleBinaryExpressionVisit([NotNull] ParserRuleContext context)
        {
            List<IParseTree> rules = new List<IParseTree>();
            foreach (var child in context.children)
            {
                if (child is ITerminalNode childNode && childNode != null && childNode.Symbol.Type == EOL)
                    continue;
                rules.Add(child);
            } 
            if (rules[1] is ITerminalNode node && node != null)
            {
                return CreateBinaryOperatorExpression(node.Symbol.Type, (ExpressionSyntax)Visit(rules[0]), (ExpressionSyntax)Visit(rules[2]));
            }
            throw new ValueError("Invalid operand: " + context.GetChild(2).GetText());
        }

        public SyntaxNode HandlePureBinaryExpressionVisit([NotNull] ParserRuleContext context)
        {
            if (context.GetChild(1) is ITerminalNode node && node != null)
                return SyntaxFactory.BinaryExpression(
                    Helper.pureBinaryOperators[node.Symbol.Type],
                    (ExpressionSyntax)Visit(context.GetChild(0)),
                    (ExpressionSyntax)Visit(context.GetChild(2)));
            throw new ValueError("Invalid operand: " + context.GetChild(1).GetText());
        }

        public override SyntaxNode VisitMultiplicativeExpression([NotNull] MultiplicativeExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitAdditiveExpression([NotNull] AdditiveExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitBitShiftExpression([NotNull] BitShiftExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitBitAndExpression([NotNull] BitAndExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitBitXOrExpression([NotNull] BitXOrExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitBitOrExpression([NotNull] BitOrExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitDotConcatenateExpression([NotNull] DotConcatenateExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitRegExMatchExpression([NotNull] RegExMatchExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitRelationalExpression([NotNull] RelationalExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitEqualityExpression([NotNull] EqualityExpressionContext context)
        {
            return HandleBinaryExpressionVisit(context);
        }

        public override SyntaxNode VisitIsExpression([NotNull] IsExpressionContext context)
        {
            var leftExpression = (ExpressionSyntax)Visit(context.singleExpression(0));
            // Ensure the classExpression is treated as a string (e.g., "KeysharpObject")
            var classAsRawString = context.singleExpression(1).GetText().Trim();

            if (classAsRawString == "unset" || classAsRawString == "null")
            {
                var nullComparison = SyntaxFactory.BinaryExpression(
                    SyntaxKind.EqualsExpression,
                    leftExpression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                );
                return nullComparison;
            }

            var classAsString = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(classAsRawString) // Convert class to its string representation
            );

            return CreateBinaryOperatorExpression(
                MainLexer.Is,
                leftExpression,
                classAsString);
        }

        public override SyntaxNode VisitLogicalAndExpression([NotNull] LogicalAndExpressionContext context)
        {
            var left = (ExpressionSyntax)Visit(context.singleExpression(0));
            var right = (ExpressionSyntax)Visit(context.singleExpression(1));

            return SyntaxFactory.ConditionalExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("ForceBool")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(left))
                    )
                ),
                right,  // If left is truthy, return right
                left    // If left is falsy, return left
            );
        }

        public override SyntaxNode VisitLogicalOrExpression([NotNull] LogicalOrExpressionContext context)
        {
            var left = (ExpressionSyntax)Visit(context.singleExpression(0));
            var right = (ExpressionSyntax)Visit(context.singleExpression(1));

            return SyntaxFactory.ConditionalExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("ForceBool")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(left))
                    )
                ),
                left,   // If left is truthy, return left
                right   // If left is falsy, return right
            );
        }

        public override SyntaxNode VisitCoalesceExpression([NotNull] CoalesceExpressionContext context)
        {
            var left = (ExpressionSyntax)Visit(context.singleExpression(0));
            var right = (ExpressionSyntax)Visit(context.singleExpression(1));
            return SyntaxFactory.BinaryExpression(
                SyntaxKind.CoalesceExpression,
                left,
                right
            );
        }

        public SyntaxNode HandleUnaryExpressionVisit([NotNull] ParserRuleContext context)
        {
            if (context.GetChild(0) is ITerminalNode node && node != null)
            {
                var arguments = new List<ExpressionSyntax>() {
                CreateQualifiedName("Keysharp.Scripting.Script.Operator." + Helper.unaryOperators[node.Symbol.Type]),
                (ExpressionSyntax)Visit(context.GetChild(1))
            };
                var argumentList = SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(arguments.Select(arg => SyntaxFactory.Argument(arg)))
                );
                return SyntaxFactory.InvocationExpression(Helper.ScriptOperateUnaryName, argumentList);
            }
            throw new ValueError("Invalid operand: " + context.GetChild(1).GetText());
        }

        public override SyntaxNode VisitNotExpression([NotNull] NotExpressionContext context)
        {
            return HandleUnaryExpressionVisit(context);
        }

        public override SyntaxNode VisitArrayLiteralExpression([NotNull] ArrayLiteralExpressionContext context)
        {
            return base.VisitArrayLiteralExpression(context);
        }

        public override SyntaxNode VisitAssignmentOperatorExpression([NotNull] AssignmentOperatorExpressionContext context)
        {
            // Visit the left-hand side of the assignment
            var leftExpression = (ExpressionSyntax)Visit(context.primary());
            if (leftExpression is IdentifierNameSyntax name)
                state.MaybeAddVariableDeclaration(name.Identifier.Text);
            var rightExpression = (ExpressionSyntax)Visit(context.singleExpression());
            string assignmentOperator = context.assignmentOperator().GetText();

            return HandleAssignment(leftExpression, rightExpression, assignmentOperator);
        }

        private ExpressionSyntax HandleAssignment(ExpressionSyntax leftExpression, ExpressionSyntax rightExpression, string assignmentOperator)
        {
            
            // Handle static member assignment
            if (leftExpression is InvocationExpressionSyntax invocationExpression &&
                IsStaticMemberAccessInvocation(invocationExpression))
            {
                return HandleStaticMemberAssignment(invocationExpression, rightExpression, assignmentOperator);
            }

            // Handle ElementAccessExpression for array or indexed assignments
            else if (leftExpression is ElementAccessExpressionSyntax elementAccess)
            {
                return HandleElementAccessAssignment(elementAccess, rightExpression, assignmentOperator);
            }

            // Handle MemberAccessExpression for property or field assignments
            else if (leftExpression is MemberAccessExpressionSyntax memberAccess)
            {
                return HandleMemberAccessAssignment(memberAccess, rightExpression, assignmentOperator);
            }

            // Handle other cases (e.g., property assignments, index access)
            if (assignmentOperator == ":=" || assignmentOperator == "??=")
            {
                var assignmentKind = (assignmentOperator == ":=") ? SyntaxKind.SimpleAssignmentExpression : SyntaxKind.CoalesceAssignmentExpression;
                if ((leftExpression is ObjectCreationExpressionSyntax objectExpression)
                && objectExpression.Type is IdentifierNameSyntax objectName
                && objectName.Identifier.Text == "VarRef")
                {
                    var varRefExpression = leftExpression;
                    leftExpression = state.PushTempVar();
                    var result = ((InvocationExpressionSyntax)InternalMethods.MultiStatement)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.AssignmentExpression(
                                            assignmentKind,
                                            leftExpression,
                                            varRefExpression
                                        )
                                    ),
                                    SyntaxFactory.Argument(
                                        ((InvocationExpressionSyntax)InternalMethods.SetPropertyValue)
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList(new[]
                                                {
                                                    SyntaxFactory.Argument(leftExpression),
                                                    SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("__Value")
                                                    )),
                                                    SyntaxFactory.Argument(rightExpression)
                                                })
                                            )
                                        )
                                    ),
                                    SyntaxFactory.Argument(varRefExpression)
                                })
                            )
                        );
                    state.PopTempVar();
                    return result;
                }
                if (leftExpression is InvocationExpressionSyntax getPropertyInvocation &&
                    IsGetPropertyInvocation(getPropertyInvocation))
                {
                    return HandlePropertyAssignment(getPropertyInvocation, rightExpression);
                }
                else if (leftExpression is InvocationExpressionSyntax indexAccessInvocation &&
                         IsIndexAccessInvocation(indexAccessInvocation))
                {
                    return HandleIndexAssignment(indexAccessInvocation, rightExpression);
                }
                else if (leftExpression is IdentifierNameSyntax identifierNameSyntax)
                {
                    state.MaybeAddVariableDeclaration(identifierNameSyntax.Identifier.Text);
                    leftExpression = identifierNameSyntax;
                    return SyntaxFactory.AssignmentExpression(
                        assignmentKind,
                        leftExpression,
                        rightExpression
                    );
                }
                else
                {
                    throw new Error("Unknown left expression type");
                }
            }

            // Handle compound assignments
            return HandleCompoundAssignment(leftExpression, rightExpression, assignmentOperator);
        }

        private ExpressionSyntax HandleStaticMemberAssignment(
            InvocationExpressionSyntax staticMemberInvocation,
            ExpressionSyntax rightExpression,
            string assignmentOperator)
        {
            if (assignmentOperator == "??=")
            {
                var getStaticMemberValue = staticMemberInvocation;
                var setStaticMemberValue = CreateSetStaticMemberInvocation(getStaticMemberValue, rightExpression);

                // Return: left ?? (SetStaticMemberValueT<typeName>(member, right))
                return SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    getStaticMemberValue,
                    setStaticMemberValue
                );
            }

            if (assignmentOperator == ":=")
            {
                return CreateSetStaticMemberInvocation(staticMemberInvocation, rightExpression);
            }

            // Handle compound assignments (e.g., "+=", "-=")
            string binaryOperator = MapAssignmentOperatorToBinaryOperator(assignmentOperator);
            var binaryOperation = Helper.CreateBinaryOperatorExpression(
                GetOperatorToken(binaryOperator),
                staticMemberInvocation,
                rightExpression
            );

            return CreateSetStaticMemberInvocation(staticMemberInvocation, binaryOperation);
        }


        private ExpressionSyntax HandleElementAccessAssignment(
            ElementAccessExpressionSyntax elementAccess,
            ExpressionSyntax rightExpression,
            string assignmentOperator)
        {
            if (assignmentOperator == ":=")
            {
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("SetObject")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(rightExpression),
                            SyntaxFactory.Argument(elementAccess.Expression)
                        }.Concat(elementAccess.ArgumentList.Arguments))
                    )
                );
            }

            if (assignmentOperator == "??=")
            {
                var getIndexValue = CreateElementAccessGetterInvocation(elementAccess);
                var setIndexValue = (ExpressionSyntax)HandleElementAccessAssignment(elementAccess, rightExpression, ":=");

                // Return: left ?? (SetObject(base, index, right))
                return SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    getIndexValue,
                    setIndexValue
                );
            }

            // Handle compound assignments
            string binaryOperator = MapAssignmentOperatorToBinaryOperator(assignmentOperator);
            var binaryOperation = Helper.CreateBinaryOperatorExpression(
                GetOperatorToken(binaryOperator),
                CreateElementAccessGetterInvocation(elementAccess),
                rightExpression
            );

            return HandleElementAccessAssignment(elementAccess, binaryOperation, ":=");
        }

        private ExpressionSyntax HandleMemberAccessAssignment(
            MemberAccessExpressionSyntax memberAccess,
            ExpressionSyntax rightExpression,
            string assignmentOperator)
        {
            if (assignmentOperator == ":=")
            {
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("SetPropertyValue")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(memberAccess.Expression),
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(memberAccess.Name.Identifier.Text)
                            )),
                            SyntaxFactory.Argument(rightExpression)
                        })
                    )
                );
            }

            if (assignmentOperator == "??=")
            {
                var getPropertyValue = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("GetPropertyValue")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(memberAccess.Expression),
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(memberAccess.Name.Identifier.Text)
                            ))
                        })
                    )
                );

                var setPropertyValue = (ExpressionSyntax)HandleMemberAccessAssignment(memberAccess, rightExpression, ":=");

                // Return: left ?? (SetPropertyValue(base, member, right))
                return SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    getPropertyValue,
                    setPropertyValue
                );
            }

            // Handle compound assignments
            string binaryOperator = MapAssignmentOperatorToBinaryOperator(assignmentOperator);
            var binaryOperation = Helper.CreateBinaryOperatorExpression(
                GetOperatorToken(binaryOperator),
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("GetPropertyValue")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(memberAccess.Expression),
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(memberAccess.Name.Identifier.Text)
                            ))
                        })
                    )
                ),
                rightExpression
            );

            return HandleMemberAccessAssignment(memberAccess, binaryOperation, ":=");
        }

        private ExpressionSyntax CreateElementAccessGetterInvocation(ElementAccessExpressionSyntax elementAccess)
        {
            var baseExpression = elementAccess.Expression;
            var indexExpression = elementAccess.ArgumentList.Arguments.First().Expression;

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("Index")
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                SyntaxFactory.Argument(baseExpression),
                SyntaxFactory.Argument(indexExpression)
                    })
                )
            );
        }

        private ExpressionSyntax HandlePropertyAssignment(InvocationExpressionSyntax getPropertyInvocation, ExpressionSyntax rightExpression)
        {
            var baseExpression = getPropertyInvocation.ArgumentList.Arguments[0].Expression;
            var memberExpression = getPropertyInvocation.ArgumentList.Arguments[1].Expression;

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("SetPropertyValue")
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(baseExpression),
                        SyntaxFactory.Argument(memberExpression),
                        SyntaxFactory.Argument(rightExpression)
                    })
                )
            );
        }

        private ExpressionSyntax HandleIndexAssignment(InvocationExpressionSyntax indexAccessInvocation, ExpressionSyntax rightExpression)
        {
            var baseExpression = indexAccessInvocation.ArgumentList.Arguments[0].Expression;

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("SetObject")
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(rightExpression),
                        SyntaxFactory.Argument(baseExpression)
                    }.Concat(indexAccessInvocation.ArgumentList.Arguments.Skip(1)))
                )
            );
        }

        private ExpressionSyntax HandleCompoundAssignment(ExpressionSyntax leftExpression, ExpressionSyntax rightExpression, string assignmentOperator, bool isPostFix = false)
        {
            string binaryOperator = MapAssignmentOperatorToBinaryOperator(assignmentOperator);
            InvocationExpressionSyntax binaryOperation;
            InvocationExpressionSyntax result = null;

            // In the case of member or index access, buffer the base and member and then get+set to avoid multiple evaluations
            if (!(leftExpression is IdentifierNameSyntax))
            {
                var baseTemp = state.PushTempVar();
                var memberTemp = state.PushTempVar();
                IdentifierNameSyntax resultTemp = null;
                IdentifierNameSyntax varRefTemp = null;
                ExpressionSyntax assignmentExpression = null;
                ExpressionSyntax baseExpression = null;
                ExpressionSyntax memberExpression = null;
                ExpressionSyntax varRefExpression = null;

                if ((leftExpression is ObjectCreationExpressionSyntax objectExpression)
                    && objectExpression.Type is IdentifierNameSyntax objectName
                    && objectName.Identifier.Text == "VarRef")
                {
                    varRefTemp = state.PushTempVar();
                    varRefExpression = leftExpression;
                    leftExpression = SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.ParenthesizedExpression(
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.IdentifierName("VarRef"),
                                    varRefTemp
                                )
                            ),
                            SyntaxFactory.IdentifierName("__Value")
                        );
                }
                if (leftExpression is InvocationExpressionSyntax getPropertyInvocation 
                    && IsGetPropertyInvocation(getPropertyInvocation))
                {
                    baseExpression = getPropertyInvocation.ArgumentList.Arguments[0].Expression;
                    memberExpression = getPropertyInvocation.ArgumentList.Arguments[1].Expression;

                    var propValue = ((InvocationExpressionSyntax)InternalMethods.GetPropertyValue)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(baseTemp),
                                    SyntaxFactory.Argument(memberTemp)
                                })
                            )
                        );

                    if (isPostFix)
                    {
                        resultTemp = state.PushTempVar();
                        binaryOperation = Helper.CreateBinaryOperatorExpression(
                            GetOperatorToken(binaryOperator),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                resultTemp,
                                propValue
                            ),
                            rightExpression
                        );
                        state.PopTempVar();
                    }
                    else
                    {
                        binaryOperation = Helper.CreateBinaryOperatorExpression(
                            GetOperatorToken(binaryOperator),
                                propValue,
                            rightExpression
                        );
                    }

                    assignmentExpression = ((InvocationExpressionSyntax)InternalMethods.SetPropertyValue)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(baseTemp),
                                    SyntaxFactory.Argument(memberTemp),
                                    SyntaxFactory.Argument(binaryOperation)
                                })
                            )
                    );
                }
                else if (leftExpression is InvocationExpressionSyntax indexAccessInvocation 
                    && IsIndexAccessInvocation(indexAccessInvocation))
                {
                    baseExpression = indexAccessInvocation.ArgumentList.Arguments[0].Expression;
                    memberExpression = indexAccessInvocation.ArgumentList.Arguments[1].Expression;

                    var propValue = ((InvocationExpressionSyntax)InternalMethods.Index)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(baseTemp),
                                    SyntaxFactory.Argument(memberTemp)
                                })
                            )
                        );

                    if (isPostFix)
                    {
                        resultTemp = state.PushTempVar();
                        binaryOperation = Helper.CreateBinaryOperatorExpression(
                            GetOperatorToken(binaryOperator),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                resultTemp, 
                                propValue),
                            rightExpression
                        );
                        state.PopTempVar();
                    } else
                    {
                        binaryOperation = Helper.CreateBinaryOperatorExpression(
                            GetOperatorToken(binaryOperator),
                            propValue,
                            rightExpression
                        );
                    }

                    assignmentExpression = ((InvocationExpressionSyntax)InternalMethods.SetObject)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(binaryOperation),
                                    SyntaxFactory.Argument(baseTemp),
                                    SyntaxFactory.Argument(memberTemp)
                                })
                            )
                        );
                }
                else if (leftExpression is ElementAccessExpressionSyntax elementAccess)
                {
                    baseExpression = elementAccess.Expression;
                    var indices = elementAccess.ArgumentList.Arguments;

                    resultTemp = state.PushTempVar();

                    // Assign each index to a temporary variable to avoid repeated evaluation
                    var indexTemps = new List<IdentifierNameSyntax>();
                    foreach (var _ in indices)
                        indexTemps.Add(state.PushTempVar());
                    // Create assignment expressions for each index and tempIndex
                    var indexTempAssigns = indices.Zip(indexTemps, (indexArg, tempIndex) =>
                        SyntaxFactory.Argument(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                tempIndex, // The temporary variable
                                indexArg.Expression // The original index expression
                            )
                        )
                    ).ToList();

                    // Creates an access in the form Keysharp.Scripting.Script.Vars[_ks_temp2 = x]
                    // This will get assigned to baseTemp: _ks_temp1 = Keysharp.Scripting.Script.Vars[_ks_temp2 = x]
                    baseExpression = elementAccess
                        .WithArgumentList(SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(indexTempAssigns)));

                    // Keysharp.Scripting.Script.Vars[_ks_temp2]
                    memberExpression = elementAccess
                        .WithArgumentList(SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(indexTemps.Select(index => SyntaxFactory.Argument(index)).ToList())));

                    // Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Add, _ks_temp1, rightExpression)
                    binaryOperation = Helper.CreateBinaryOperatorExpression(
                        GetOperatorToken(binaryOperator),
                        resultTemp,
                        rightExpression
                    );

                    assignmentExpression = SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        memberExpression,
                        binaryOperation
                    );

                    var argumentList = new List<ArgumentSyntax> {
                        SyntaxFactory.Argument(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                resultTemp,
                                baseExpression
                            )
                        ),
                        SyntaxFactory.Argument(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                memberExpression,
                                binaryOperation
                            )
                        )
                    };

                    result = ((InvocationExpressionSyntax)InternalMethods.MultiStatement)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(argumentList.ToArray())
                        )
                    );

                    // Clean up pushed temporaries for indices
                    foreach (var temp in indexTemps)
                    {
                        state.PopTempVar();
                    }

                    state.PopTempVar(); // For the resultTemp variable
                }
                else
                    throw new Error("Unknown compound assignment left operand");

                state.PopTempVar();
                state.PopTempVar();

                if (result == null)
                result = ((InvocationExpressionSyntax)InternalMethods.MultiStatement)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                            SyntaxFactory.Argument(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    baseTemp,
                                    baseExpression
                                )
                            ),
                            SyntaxFactory.Argument(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    memberTemp,
                                    memberExpression
                                )
                            ),
                            SyntaxFactory.Argument(assignmentExpression)
                            })
                        )
                    );

                if (varRefTemp != null)
                {
                    result = result.WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                new[] { SyntaxFactory.Argument(
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        varRefTemp, // The temporary variable
                                        varRefExpression // The VarRef expression
                                    )
                                ) }
                                .Concat(result.ArgumentList.Arguments)
                                .Concat(new[] { SyntaxFactory.Argument(varRefTemp) })
                            )
                        )
                    );
                    state.PopTempVar();
                }

                if (isPostFix && resultTemp != null)
                {
                    return result.WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(
                                result.ArgumentList.Arguments.Concat(new[] { SyntaxFactory.Argument(resultTemp) })
                            )
                        )
                    );
                }

                return result;
            }

            binaryOperation = Helper.CreateBinaryOperatorExpression(
                GetOperatorToken(binaryOperator),
                leftExpression,
                rightExpression
            );

            if (isPostFix)
            {
                var tempVar = state.PushTempVar(); // Create a temporary variable

                // Create a MultiStatement:
                // temp = x, x = x + 1, temp
                result = ((InvocationExpressionSyntax)InternalMethods.MultiStatement)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        tempVar,
                                        leftExpression
                                    )
                                ),
                                SyntaxFactory.Argument(
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        leftExpression,
                                        binaryOperation
                                    )
                                ),
                                SyntaxFactory.Argument(tempVar)
                            })
                        )
                    );
                state.PopTempVar();
                return result;
            }

            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                leftExpression,
                binaryOperation
            );
        }

        private bool IsStaticMemberAccessInvocation(InvocationExpressionSyntax invocation)
        {
            return (invocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text.Equals("GetStaticMemberValueT", StringComparison.OrdinalIgnoreCase));
        }

        private InvocationExpressionSyntax CreateSetStaticMemberInvocation(
            InvocationExpressionSyntax getStaticMemberInvocation,
            ExpressionSyntax newValue)
        {
            if (getStaticMemberInvocation.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name is GenericNameSyntax genericName)
            {
                // Extract the type argument (e.g., `typeName` in `GetStaticMemberValueT<typeName>`)
                var typeArgument = genericName.TypeArgumentList.Arguments.First();

                // Extract the member argument (e.g., `member`)
                var memberArgument = getStaticMemberInvocation.ArgumentList.Arguments.First();

                // Create SetStaticMemberValueT<typeName>(member, newValue)
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.GenericName("SetStaticMemberValueT")
                        .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(new[] { typeArgument }))),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                    memberArgument,
                    SyntaxFactory.Argument(newValue)
                        })
                    )
                );
            }

            throw new InvalidOperationException("Invalid static member access invocation");
        }

        private bool IsGetPropertyInvocation(InvocationExpressionSyntax invocation)
        {
            return (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "GetPropertyValue");
        }

        private bool IsIndexAccessInvocation(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                   memberAccess.Name.Identifier.Text == "Index";
        }

        // Helper function to map assignment operators to binary operators
        private string MapAssignmentOperatorToBinaryOperator(string assignmentOperator)
        {
            return assignmentOperator switch
            {
                "+=" => "Add",
                "-=" => "Minus",
                "*=" => "Multiply",
                "/=" => "Divide",
                "%=" => "Modulus",
                "//=" => "FloorDivide",
                ".=" => "Concat",
                "|=" => "BitwiseOr",
                "&=" => "BitwiseAnd",
                "^=" => "BitwiseXor",
                "<<=" => "BitShiftLeft",
                ">>=" => "BitShiftRight",
                ">>>=" => "LogicalBitShiftRight",
                "**=" => "Power",
                "??=" => "Coalesce",
                _ => throw new InvalidOperationException($"Unsupported assignment operator: {assignmentOperator}")
            };
        }

        // Helper function to map binary operator names to tokens
        private int GetOperatorToken(string binaryOperator)
        {
            return Helper.binaryOperators.FirstOrDefault(kvp => kvp.Value == binaryOperator).Key;
        }

        private InvocationExpressionSyntax GenerateMemberDotAccess(PrimaryContext baseIdentifier, MemberIdentifierContext memberIdentifier)
        {
            // Visit the base expression (e.g., `arr` in `arr.Length`)
            var baseExpression = (ExpressionSyntax)Visit(baseIdentifier);

            // Determine the property or method being accessed
            ExpressionSyntax memberExpression = (ExpressionSyntax)Visit(memberIdentifier);

            // Simple identifier should be converted to string literal
            if (memberExpression is IdentifierNameSyntax memberIdentifierName)
            {
                memberExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(memberIdentifierName.Identifier.Text));
            }
            // Keysharp.Scripting.Script.Vars[expression] should extract expression
            else if (memberExpression is ElementAccessExpressionSyntax memberElementAccess)
            {
                memberExpression = memberElementAccess.ArgumentList.Arguments.FirstOrDefault().Expression;
            }
            else
                throw new Error("Invalid member dot access expression member");

            if (baseExpression is IdentifierNameSyntax identifierName)
            {
                if (Reflections.stringToTypes.ContainsKey(identifierName.Identifier.Text) || identifierName.Identifier.Text.ToLower() == "program")
                {
                    // Generate Keysharp.Scripting.Script.GetStaticMemberValueT<baseExpression>(memberExpression)
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Scripting.Script"),
                            SyntaxFactory.GenericName("GetStaticMemberValueT")
                                .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.IdentifierName(identifierName.Identifier.Text))
                                ))
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(memberExpression)
                            })
                        )
                    );
                }
                else
                    state.MaybeAddGlobalFuncObjVariable(identifierName.Identifier.Text);
            }

            // Generate the call to Keysharp.Scripting.Script.GetPropertyValue(base, member)
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("GetPropertyValue")
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(baseExpression),
                        SyntaxFactory.Argument(memberExpression)
                    })
                )
            );
        }

        public override SyntaxNode VisitMemberDotExpression([Antlr4.Runtime.Misc.NotNull] MemberDotExpressionContext context)
        {
            return GenerateMemberDotAccess(context.primary(), context.memberIdentifier());
        }

        public override SyntaxNode VisitObjectLiteralExpression([NotNull] ObjectLiteralExpressionContext context)
        {
            return base.VisitObjectLiteralExpression(context);
        }

        public override SyntaxNode VisitThisExpression([NotNull] ThisExpressionContext context)
        {
            return state.currentClass.Name == "program" ? SyntaxFactory.IdentifierName("@this") : SyntaxFactory.ThisExpression();
        }

        public override SyntaxNode VisitBaseExpression([NotNull] BaseExpressionContext context)
        {
            return state.currentClass.Name == "program" ? SyntaxFactory.IdentifierName("@base") : SyntaxFactory.BaseExpression();
        }

        public override SyntaxNode VisitSuperExpression([NotNull] SuperExpressionContext context)
        {
            return SyntaxFactory.CastExpression(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), 
                            SyntaxFactory.IdentifierName("Super")
                   );
        }

    }
}
