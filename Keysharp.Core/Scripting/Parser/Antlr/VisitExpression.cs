using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static MainParser;
using static Keysharp.Scripting.Parser;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Keysharp.Core.Scripting.Parser.Antlr.Helper;
using System.Linq.Expressions;

namespace Keysharp.Core.Scripting.Parser.Antlr
{
    public partial class MainVisitor : MainParserBaseVisitor<SyntaxNode>
    {
        // Converts identifiers such as a%b% to
        // Keysharp.Scripting.Script.Vars[string.Concat<object>(new object[] {"a", b)]
        public SyntaxNode CreateDynamicVariableAccessor(ParserRuleContext context)
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
            return SyntaxFactory.ElementAccessExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("Vars")
                ),
                SyntaxFactory.BracketedArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(combinedExpression)
                    )
                )
            );
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

        public override SyntaxNode VisitIdentifierExpression([NotNull] IdentifierExpressionContext context)
        {
            return Visit(context.identifierName());
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
            // Visit the singleExpression (the method to be called)
            var targetExpression = (ExpressionSyntax)Visit(context.singleExpression());

            // Get the argument list
            ArgumentListSyntax argumentList;
            if (context.functionCallArguments()?.arguments() != null)
                argumentList = (ArgumentListSyntax)VisitArguments(context.functionCallArguments().arguments());
            else
                argumentList = SyntaxFactory.ArgumentList();

            string methodName = ExtractMethodName(targetExpression);

            return GenerateFunctionInvocation(targetExpression, argumentList, methodName);
        }

        public override SyntaxNode VisitMemberIndexExpression([NotNull] MemberIndexExpressionContext context)
        {
            // Visit the main expression (e.g., c in c[1])
            var targetExpression = (ExpressionSyntax)Visit(context.singleExpression());

            // Visit the expressionSequence to generate an ArgumentListSyntax
            var argumentList = (ArgumentListSyntax)VisitExpressionSequence(context.memberIndexArguments().expressionSequence());

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
            if (context.GetChild(1)?.GetText() == "?")
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

        public override SyntaxNode VisitExpressionStatement([Antlr4.Runtime.Misc.NotNull] ExpressionStatementContext context)
        {
            var argumentList = (ArgumentListSyntax)base.VisitExpressionStatement(context);
            if (argumentList.Arguments.Count == 0)
                throw new Error("Expression count can't be 0?");
            else if (argumentList.Arguments.Count == 1)
                return SyntaxFactory.ExpressionStatement(argumentList.Arguments[0].Expression);
            else
            {
                return SyntaxFactory.Block(argumentList.Arguments
                                .Select(arg => SyntaxFactory.ExpressionStatement(arg.Expression))
                                .ToList());
            }
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

            foreach (var expr in context.singleExpression())
            {
                arguments.Add(SyntaxFactory.Argument((ExpressionSyntax)Visit(expr)));
            }
            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
        }

        public override SyntaxNode VisitImplicitConcatenateExpression([NotNull] ImplicitConcatenateExpressionContext context)
        {
            if (context.singleExpressionConcatenation().Length == 0)
                return Visit(context.singleExpression());
            var arguments = new List<ExpressionSyntax> { 
                (ExpressionSyntax)Visit(context.singleExpression()) 
            };

            foreach (var expr in context.singleExpressionConcatenation())
            {
                arguments.Add((ExpressionSyntax)Visit(expr));
            }
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

            // Assign the result of the operation back to the expression
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                expression,
                CreateBinaryOperatorExpression(
                    MainParser.Plus,
                    expression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L))
                )
            );
        }

        public override SyntaxNode VisitPreDecreaseExpression([NotNull] PreDecreaseExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());

            // Assign the result of the operation back to the expression
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                expression,
                CreateBinaryOperatorExpression(
                    MainParser.Minus,
                    expression,
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L))
                )
            );
        }

        public override SyntaxNode VisitPostIncrementExpression([NotNull] PostIncrementExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());

            // Generate: x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Subtract, x, 1L)
            var incrementOperation = CreateBinaryOperatorExpression(
                MainParser.Plus,
                expression,
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)));

            var assignmentExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, expression, incrementOperation);

            // Generate the full expression: Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, (x = ...), 1L)
            return CreateBinaryOperatorExpression(
                MainParser.Minus,
                SyntaxFactory.ParenthesizedExpression(assignmentExpression),
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L))
            );
        }

        public override SyntaxNode VisitPostDecreaseExpression([NotNull] PostDecreaseExpressionContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());

            // Generate: x = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Subtract, x, 1L)
            var decrementOperation = CreateBinaryOperatorExpression(
                MainParser.Minus,
                expression,
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)));

            var assignmentExpression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, expression, decrementOperation);

            // Generate the full expression: Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Minus, (x = ...), 1L)
            return CreateBinaryOperatorExpression(
                MainParser.Plus,
                SyntaxFactory.ParenthesizedExpression(assignmentExpression),
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L))
            );
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
                MainParser.Is,
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
            var leftExpression = (ExpressionSyntax)Visit(context.singleExpression(0));
            var rightExpression = (ExpressionSyntax)Visit(context.singleExpression(1));

            string assignmentOperator = context.assignmentOperator().GetText();

            // Handle static member assignment
            if (leftExpression is InvocationExpressionSyntax staticMemberInvocation &&
                IsStaticMemberAccessInvocation(staticMemberInvocation))
            {
                // Handle "??=" for static members
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

                // Handle simple assignment ":="
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

            // Other existing cases (e.g., property assignments, index access)
            if (assignmentOperator == ":=")
            {
                if (leftExpression is InvocationExpressionSyntax getPropertyInvocation &&
                    IsGetPropertyInvocation(getPropertyInvocation))
                {
                    return HandlePropertyAssignment(getPropertyInvocation, rightExpression);
                } else if (leftExpression is InvocationExpressionSyntax indexAccessInvocation &&
                    IsIndexAccessInvocation(indexAccessInvocation))
                {
                    return HandleIndexAssignment(indexAccessInvocation, rightExpression);
                } else if (leftExpression is IdentifierNameSyntax identifierNameSyntax)
                {
                    leftExpression = MaybeWrapVariableDeclaration(identifierNameSyntax);
                    return SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        leftExpression,
                        rightExpression
                    );
                } else
                {
                    throw new Error("Unknown left expression type");
                }
            }

            // Handle compound assignments
            return HandleCompoundAssignment(leftExpression, rightExpression, assignmentOperator);
        }

        private SyntaxNode HandlePropertyAssignment(InvocationExpressionSyntax getPropertyInvocation, ExpressionSyntax rightExpression)
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

        private SyntaxNode HandleIndexAssignment(InvocationExpressionSyntax indexAccessInvocation, ExpressionSyntax rightExpression)
        {
            var baseExpression = indexAccessInvocation.ArgumentList.Arguments[0].Expression;
            var indexExpression = indexAccessInvocation.ArgumentList.Arguments[1].Expression;

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
                SyntaxFactory.Argument(baseExpression),
                SyntaxFactory.Argument(indexExpression)
                    })
                )
            );
        }

        private SyntaxNode HandleCompoundAssignment(ExpressionSyntax leftExpression, ExpressionSyntax rightExpression, string assignmentOperator)
        {
            string binaryOperator = MapAssignmentOperatorToBinaryOperator(assignmentOperator);

            var binaryOperation = Helper.CreateBinaryOperatorExpression(
                GetOperatorToken(binaryOperator),
                leftExpression,
                rightExpression
            );

            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                leftExpression,
                binaryOperation
            );
        }

        private ExpressionSyntax MaybeWrapVariableDeclaration(IdentifierNameSyntax identifierName)
        {
            return SyntaxFactory.IdentifierName(MaybeAddVariableDeclaration(identifierName.Identifier.Text));
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

        public override SyntaxNode VisitMemberDotExpression([Antlr4.Runtime.Misc.NotNull] MemberDotExpressionContext context)
        {
            // Visit the base expression (e.g., `arr` in `arr.Length`)
            var baseExpression = (ExpressionSyntax)Visit(context.singleExpression());

            // Determine the property or method being accessed
            ExpressionSyntax memberExpression;
            if (context.dynamicPropertyName() != null)
            {
                // If the member is a single expression
                memberExpression = (ExpressionSyntax)Visit(context.dynamicPropertyName());
                if (memberExpression is IdentifierNameSyntax)
                {
                    // If the member is an identifier name, treat it as a string literal
                    var memberName = context.dynamicPropertyName().GetText().Trim();
                    memberExpression = SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(memberName)
                    );
                }
                else if (memberExpression is ElementAccessExpressionSyntax eaes)
                {
                    // If the member is not an IdentifierName then it's a deref expression, 
                    // so extract the target string from the argument list.
                    memberExpression = eaes.ArgumentList.Arguments[0].Expression;
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid member access in MemberDotExpression.");
            }

            if (baseExpression is IdentifierNameSyntax identifierName)
            {
                if (UserTypes.Contains(identifierName.Identifier.Text))
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
                    MaybeAddGlobalFuncObjVariable(identifierName.Identifier.Text);
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

        public override SyntaxNode VisitObjectLiteralExpression([NotNull] ObjectLiteralExpressionContext context)
        {
            return base.VisitObjectLiteralExpression(context);
        }

        public override SyntaxNode VisitThisExpression([NotNull] ThisExpressionContext context)
        {
            return SyntaxFactory.ThisExpression();
        }

        public override SyntaxNode VisitBaseExpression([NotNull] BaseExpressionContext context)
        {
            return SyntaxFactory.BaseExpression();
        }

        public override SyntaxNode VisitSuperExpression([NotNull] SuperExpressionContext context)
        {
            return SyntaxFactory.BaseExpression(); //TODO?
        }

    }
}
