using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Antlr4.Runtime.Misc;
using static Keysharp.Scripting.Parser;
using static MainParser;
using Microsoft.CodeAnalysis;
using System.Drawing.Imaging;
using Antlr4.Runtime;
using System.IO;
using System.Collections;
using System.Xml.Linq;
using System.Data.Common;
using System.Reflection;
using Antlr4.Runtime.Tree;

namespace Keysharp.Scripting
{
    internal partial class VisitMain : MainParserBaseVisitor<SyntaxNode>
    {
        private SyntaxNode VisitLoopGeneric(
            ExpressionSyntax loopExpression,
            SyntaxNode loopBodyNode,
            ExpressionSyntax untilCondition,
            SyntaxNode elseNode,
            string loopType,
            string loopEnumeratorName,
            string enumeratorType,
            string enumeratorMethodName,
            List<string> backupVariableNames,
            params ExpressionSyntax[] enumeratorArguments)
        {
            // Generate the enumerator initialization
            var loopFunction = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Loops"),
                        SyntaxFactory.IdentifierName(enumeratorMethodName)
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(
                            enumeratorArguments.Select(SyntaxFactory.Argument)
                        )
                    )
                );
            var enumeratorVariable = SyntaxFactory.IdentifierName(loopEnumeratorName);
            var enumeratorDeclaration = SyntaxFactory.VariableDeclaration(
                CreateQualifiedName(enumeratorType),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        enumeratorVariable.Identifier,
                        null,
                        SyntaxFactory.EqualsValueClause(
                            enumeratorMethodName == "MakeEnumerator" ? loopFunction :
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    loopFunction, 
                                    SyntaxFactory.IdentifierName("GetEnumerator")
                                )
                            )
                        )
                    )
                )
            );

            // Generate the Push statement with the loopType
            var pushStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Loops"),
                        SyntaxFactory.IdentifierName("Push")
                    ),
                    loopType == null ? SyntaxFactory.ArgumentList() :
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    CreateQualifiedName("Keysharp.Core.LoopType"),
                                    SyntaxFactory.IdentifierName(loopType)
                                )
                            )
                        )
                    )
                )
            );

            // Generate the loop condition
            var loopCondition = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("IsTrueAndRunning"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    enumeratorVariable,
                                    SyntaxFactory.IdentifierName("MoveNext")
                                )
                            )
                        )
                    )
                )
            );

            // Ensure the loop body is a block
            BlockSyntax loopBody = EnsureBlockSyntax(loopBodyNode);

            // Add the `Until` condition, if provided
            StatementSyntax untilStatement = untilCondition != null
                ? SyntaxFactory.IfStatement(
                    SyntaxFactory.InvocationExpression(
                        CreateQualifiedName("Keysharp.Scripting.Script.IfTest"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(untilCondition))
                        )
                    ),
                    SyntaxFactory.Block(SyntaxFactory.BreakStatement())
                )
                : SyntaxFactory.EmptyStatement();

            // Add the loop continuation label `_ks_eX_next:`
            loopBody = loopBody.AddStatements(
                SyntaxFactory.LabeledStatement(loopEnumeratorName + "_next", untilStatement)
            );

            // Generate the `for` loop
            var forLoop = SyntaxFactory.ForStatement(
                null,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(), // No initializer
                loopCondition,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(), // No incrementor
                loopBody
            );

            // Generate the `_ks_eX_end:` label
            var endLabel = SyntaxFactory.LabeledStatement(
                loopEnumeratorName + "_end",
                SyntaxFactory.EmptyStatement()
            );

            // Generate the Pop() call
            var popStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Loops"),
                        SyntaxFactory.IdentifierName("Pop")
                    )
                )
            );

            // Handle the `Else` clause
            StatementSyntax elseClause = null;
            if (elseNode != null)
            {
                BlockSyntax elseBody = EnsureBlockSyntax(elseNode);

                // Wrap the else body in an if-check
                elseClause = SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    CreateQualifiedName("Keysharp.Core.Loops"),
                                    SyntaxFactory.IdentifierName("Pop")
                                )
                            ),
                            SyntaxFactory.IdentifierName("index")
                        ),
                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0L))
                    ),
                    elseBody
                );
            }
            else
                elseClause = popStatement;

            LocalDeclarationStatementSyntax backupDeclaration = null;
            ExpressionStatementSyntax restoreBackupVariables = null;

            if (backupVariableNames != null)
            {
                var uniqueVariableNames = backupVariableNames.Distinct().ToList();
                var backupIdentifier = SyntaxFactory.IdentifierName(loopEnumeratorName + "_backup");

                backupDeclaration = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ArrayType(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // object[]
                            SyntaxFactory.SingletonList(
                                SyntaxFactory.ArrayRankSpecifier(
                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                        SyntaxFactory.OmittedArraySizeExpression() // Allow dynamic array initialization
                                    )
                                )
                            )
                        )
                    ).WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(backupIdentifier.Identifier)
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ArrayCreationExpression(
                                        SyntaxFactory.ArrayType(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                                            SyntaxFactory.SingletonList(
                                                SyntaxFactory.ArrayRankSpecifier(
                                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(uniqueVariableNames.Count))
                                                    )
                                                )
                                            )
                                        )
                                    ).WithInitializer(
                                        SyntaxFactory.InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression,
                                            SyntaxFactory.SeparatedList<ExpressionSyntax>(
                                                uniqueVariableNames.Select(name => SyntaxFactory.IdentifierName(name))
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );

                if (uniqueVariableNames.Count == 1)
                    restoreBackupVariables = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(uniqueVariableNames[0]),
                            SyntaxFactory.ElementAccessExpression(backupIdentifier)
                                .WithArgumentList(
                                    SyntaxFactory.BracketedArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    SyntaxFactory.Literal(0)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        );
                else
                    restoreBackupVariables = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.TupleExpression(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    uniqueVariableNames.Select(name =>
                                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(name))
                                    )
                                )
                            ),
                            SyntaxFactory.TupleExpression(
                                SyntaxFactory.SeparatedList<ArgumentSyntax>(
                                    uniqueVariableNames.Select((_, index) =>
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.ElementAccessExpression(backupIdentifier)
                                            .WithArgumentList(
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.LiteralExpression(
                                                                SyntaxKind.NumericLiteralExpression,
                                                                SyntaxFactory.Literal(index)
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                );
            }

            // Create the finally block with Pop() and end label
            var finallyBlock = backupVariableNames == null ? SyntaxFactory.Block(elseClause) : SyntaxFactory.Block(restoreBackupVariables, elseClause);

            // Wrap the loop in a try-finally statement
            var tryFinallyStatement = SyntaxFactory.TryStatement(
                SyntaxFactory.Block(forLoop), // Try block containing the loop
                SyntaxFactory.List<CatchClauseSyntax>(), // No catch clauses
                SyntaxFactory.FinallyClause(finallyBlock) // Finally block
            );

            return backupDeclaration == null 
                ? SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(enumeratorDeclaration),
                    pushStatement,
                    tryFinallyStatement,
                    endLabel
                )
                : SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(enumeratorDeclaration),
                    backupDeclaration,
                    pushStatement,
                    tryFinallyStatement,
                    endLabel
                );
        }

        public override SyntaxNode VisitElseProduction([NotNull] ElseProductionContext context)
        {
            return context.Else() != null
                ? EnsureBlockSyntax(Visit(context.statement()))
                : null;
        }

        public override SyntaxNode VisitUntilProduction([NotNull] UntilProductionContext context)
        {
            return context.Until() != null
                ? Visit(context.singleExpression())
                : null;
        }

        public override SyntaxNode VisitFlowBlock([NotNull] FlowBlockContext context)
        {
            if (context.block() != null)
                return Visit(context.block());
            return EnsureBlockSyntax(Visit(context.statement()));
        }

        public override SyntaxNode VisitLoopStatement([NotNull] LoopStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;
            // Determine the loop expression (or -1 for infinite loops)
            ExpressionSyntax loopExpression = context.singleExpression() != null
                ? (ExpressionSyntax)Visit(context.singleExpression())
                : SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(-1));

            // Determine the `Until` condition, if present
            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)VisitUntilProduction(context.untilProduction()) : null;

            // Visit the loop body
            SyntaxNode loopBodyNode = Visit(context.flowBlock());

            // Visit the `Else` clause, if present
            SyntaxNode elseNode = Visit(context.elseProduction());
            // Invoke the generic loop handler
            return VisitLoopGeneric(
                loopExpression,
                loopBodyNode,
                untilCondition,
                elseNode,
                "Normal",
                loopEnumeratorName,
                "System.Collections.IEnumerator",
                "Loop",
                null,
                loopExpression
            );
        }

        public override SyntaxNode VisitLoopParseStatement([NotNull] LoopParseStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;
            var singleExprCount = context.singleExpression().Length;
            // Determine the `Until` condition
            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)Visit(context.untilProduction()) : null;

            // Visit the string literals
            var String = (ExpressionSyntax)Visit(context.singleExpression(0));
            var DelimiterChars = singleExprCount > 1
                ? (ExpressionSyntax)Visit(context.singleExpression(1))
                : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            var OmitChars = singleExprCount > 2
                ? (ExpressionSyntax)Visit(context.singleExpression(2))
                : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

            // Visit the loop body
            SyntaxNode loopBodyNode = Visit(context.flowBlock());

            // Visit the `Else` clause, if present
            SyntaxNode elseNode = Visit(context.elseProduction());

            // Invoke the generic loop handler
            return VisitLoopGeneric(
                null,
                loopBodyNode,
                untilCondition,
                elseNode,
                "Parse",
                loopEnumeratorName,
                "System.Collections.IEnumerator",
                "LoopParse",
                null, 
                String, DelimiterChars, OmitChars
            );
        }

        public override SyntaxNode VisitLoopFilesStatement([NotNull] LoopFilesStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;
            // Determine the `Until` condition
            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)Visit(context.untilProduction()) : null;

            var FilePattern = (ExpressionSyntax)Visit(context.singleExpression(0));
            var Mode = context.singleExpression().Length != 1
                ? (ExpressionSyntax)Visit(context.singleExpression(1))
                : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

            SyntaxNode loopBodyNode = Visit(context.flowBlock());
            SyntaxNode elseNode = Visit(context.elseProduction());

            // Invoke the generic loop handler
            return VisitLoopGeneric(
                null,
                loopBodyNode,
                untilCondition,
                elseNode,
                "Directory",
                loopEnumeratorName,
                "System.Collections.IEnumerator",
                "LoopFile",
                null,
                FilePattern, Mode
            );
        }

        public override SyntaxNode VisitLoopReadStatement([NotNull] LoopReadStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;
            // Determine the `Until` condition
            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)Visit(context.untilProduction()) : null;

            var InputFile = (ExpressionSyntax)Visit(context.singleExpression(0));
            var OutputFile = context.singleExpression().Length > 1
                ? (ExpressionSyntax)Visit(context.singleExpression(1))
                : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

            SyntaxNode loopBodyNode = Visit(context.flowBlock());
            SyntaxNode elseNode = Visit(context.elseProduction());

            // Invoke the generic loop handler
            return VisitLoopGeneric(
                null,
                loopBodyNode,
                untilCondition,
                elseNode,
                "File",
                loopEnumeratorName,
                "System.Collections.IEnumerator",
                "LoopRead",
                null,
                InputFile, OutputFile
            );
        }

        public override SyntaxNode VisitLoopRegStatement([NotNull] LoopRegStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;
            // Determine the `Until` condition
            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)Visit(context.untilProduction()) : null;

            var KeyName = (ExpressionSyntax)Visit(context.singleExpression(0));
            var Mode = context.singleExpression().Length > 1
                ? (ExpressionSyntax)Visit(context.singleExpression(1))
                : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);

            SyntaxNode loopBodyNode = Visit(context.flowBlock());
            SyntaxNode elseNode = Visit(context.elseProduction());

            // Invoke the generic loop handler
            return VisitLoopGeneric(
                null,
                loopBodyNode,
                untilCondition,
                elseNode,
                "Registry",
                loopEnumeratorName,
                "System.Collections.IEnumerator",
                "LoopRegistry",
                null,
                KeyName, Mode
            );
        }

        public override SyntaxNode VisitForInStatement([NotNull] ForInStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;

            // Get the loop expression (e.g., `arr` in `for x in arr`)
            var loopExpression = (ExpressionSyntax)Visit(context.forInParameters().singleExpression());

            // Collect loop variable names (e.g., `x`, `y` in `for x, y in arr`)
            var parameters = context.forInParameters();
            List<string> variableNames = new();
            var lastParamType = MainLexer.Comma;
            foreach (var parameter in parameters.children)
            {
                var paramType = MainLexer.Identifier;
                if (parameter is ITerminalNode node)
                    paramType = node.Symbol.Type;

                if (paramType == MainLexer.OpenParen || paramType == MainLexer.CloseParen || paramType == MainLexer.EOL || paramType == MainLexer.WS)
                    continue;
                if (paramType == MainLexer.In)
                {
                    if (lastParamType == MainLexer.Comma)
                        variableNames.Add("_");
                    break;
                }
                if (paramType == MainLexer.Comma)
                {
                    if (lastParamType == MainLexer.Comma)
                        variableNames.Add("_");
                }
                else
                {
                    var paramText = parameter.GetText();
                    var normalizedName = parser.NormalizeFunctionIdentifier(paramText);
                    var variableName = parser.MaybeAddVariableDeclaration(normalizedName);
                    variableNames.Add(variableName);
                }
                lastParamType = paramType;
            }
            var variableNameCount = variableNames.Count;

            if (variableNames.Any(name => name == "_"))
                parser.MaybeAddVariableDeclaration("_");

            // Ensure the loop body is a block
            BlockSyntax loopBody = (BlockSyntax)Visit(context.flowBlock());

            var incStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Loops"),
                        SyntaxFactory.IdentifierName("Inc")
                    )
                )
            );

            // Add the `Current` assignment to the loop body
            loopBody = loopBody.WithStatements(
                SyntaxFactory.List(
                    new StatementSyntax[] { 
                        incStatement
                    }
                    .Concat(loopBody.Statements))
            );

            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)Visit(context.untilProduction()) : null;

            // Handle the `Else` clause, if present
            SyntaxNode elseNode = Visit(context.elseProduction());

            List<ExpressionSyntax> argsList = new List<ExpressionSyntax>();
            argsList.Add(loopExpression);

            foreach (string variableName in variableNames)
            {
                ExpressionSyntax varRefExpr = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Misc"),
                        SyntaxFactory.IdentifierName("MakeVarRef")
                    ),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            // Getter lambda: () => variableName
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression(
                                    SyntaxFactory.ParameterList(),
                                    SyntaxFactory.IdentifierName(variableName)
                                )
                            ),
                            // Setter lambda: (Val) => variableName = Val
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression(
                                    SyntaxFactory.ParameterList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("Val"))
                                        )
                                    ),
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName(variableName),
                                        SyntaxFactory.IdentifierName("Val")
                                    )
                                )
                            )
                        })
                    )
                );

                argsList.Add(varRefExpr);
            }

            // Generate the final loop structure using `VisitLoopGeneric`
            BlockSyntax loopSyntax = (BlockSyntax)VisitLoopGeneric(
                null,
                loopBody,
                untilCondition,
                elseNode,
                null,
                loopEnumeratorName,
                "var",
                "MakeEnumerable",
                variableNames,
                argsList.ToArray()
            );

            return loopSyntax;
        }


        public override SyntaxNode VisitWhileStatement([NotNull] WhileStatementContext context)
        {
            var loopEnumeratorName = LoopEnumeratorBaseName + parser.loopLabel;

            // Visit the singleExpression (loop condition)
            var conditionExpression = (ExpressionSyntax)Visit(context.singleExpression());

            // Wrap the condition in IfTest
            var conditionWrapped = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("IfTest")
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(conditionExpression))
                )
            );

            // Generate the loop condition: IsTrueAndRunning(IfTest(...))
            var loopCondition = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("IsTrueAndRunning"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(conditionWrapped))
                )
            );

            // Generate the loop body
            BlockSyntax loopBody = (BlockSyntax)Visit(context.flowBlock());

            var incStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Core.Loops"),
                            SyntaxFactory.IdentifierName("Inc")
                        )
                    )
                );

            // Add Keysharp.Core.Loops.Inc() at the start of the loop body
            if (loopBody.Statements.Count > 0)
                loopBody = loopBody.InsertNodesBefore(
                    loopBody.Statements.FirstOrDefault(), new[] { incStatement }
                );
            else
                loopBody = SyntaxFactory.Block(incStatement);

            StatementSyntax untilStatement;
            ExpressionSyntax untilCondition = context.untilProduction() != null ? (ExpressionSyntax)Visit(context.untilProduction()) : null;
            if (untilCondition != null)
            {
                untilStatement = SyntaxFactory.IfStatement(
                    SyntaxFactory.InvocationExpression(
                        CreateQualifiedName("Keysharp.Scripting.Script.IfTest"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(untilCondition))
                        )
                    ),
                    SyntaxFactory.Block(
                        SyntaxFactory.BreakStatement()
                    )
                );
            }
            else
                untilStatement = SyntaxFactory.EmptyStatement();

            // Add the loop continuation label `_ks_e1_next:`
            loopBody = loopBody.AddStatements(
                SyntaxFactory.LabeledStatement(
                    loopEnumeratorName + "_next",
                    untilStatement
                )
            );

            // Generate the `for` loop structure
            var forLoop = SyntaxFactory.ForStatement(
                null, // No initializer
                SyntaxFactory.SeparatedList<ExpressionSyntax>(), // No initializer
                loopCondition,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(), // No incrementor
                loopBody
            );

            // Generate `_ks_e1_end:` label and Pop statement
            var endLabel = SyntaxFactory.LabeledStatement(
                loopEnumeratorName + "_end",
                SyntaxFactory.EmptyStatement()
            );

            // Generate the Pop() call
            var popStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Loops"),
                        SyntaxFactory.IdentifierName("Pop")
                    )
                )
            );

            // Handle the `Else` clause if present
            StatementSyntax elseClause = null;
            BlockSyntax elseBody = (BlockSyntax)Visit(context.elseProduction());
            if (elseBody != null)
            {
                // Wrap the else body in an if statement checking Pop().index == 0L
                elseClause = SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    CreateQualifiedName("Keysharp.Core.Loops"),
                                    SyntaxFactory.IdentifierName("Pop")
                                )
                            ),
                            SyntaxFactory.IdentifierName("index")
                        ),
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            SyntaxFactory.Literal(0L)
                        )
                    ),
                    elseBody
                );
            }

            var startLoopStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Loops"),
                        SyntaxFactory.IdentifierName("Push")
                    )
                )
            );

            // Create the finally block
            var finallyBlock = elseClause == null
                ? SyntaxFactory.Block(popStatement)
                : SyntaxFactory.Block(elseClause);

            // Wrap the for loop in a try-finally statement
            var tryFinallyStatement = SyntaxFactory.TryStatement(
                SyntaxFactory.Block(forLoop), // Try block containing the loop
                SyntaxFactory.List<CatchClauseSyntax>(), // No catch clauses
                SyntaxFactory.FinallyClause(finallyBlock) // Finally block
            );

            // Wrap everything in a block and return
            return SyntaxFactory.Block(startLoopStatement, tryFinallyStatement, endLabel);
        }


        public override SyntaxNode VisitContinueStatement([NotNull] ContinueStatementContext context)
        {
            var targetLabel = parser.loopLabel;
            if (context.propertyName() != null)
                targetLabel = context.propertyName().GetText().Trim('"');
            targetLabel = LoopEnumeratorBaseName + targetLabel + "_next";

            // Generate the goto statement
            return SyntaxFactory.GotoStatement(
                SyntaxKind.GotoStatement,
                SyntaxFactory.IdentifierName(targetLabel)
            );
        }

        public override SyntaxNode VisitBreakStatement([NotNull] BreakStatementContext context)
        {
            var targetLabel = parser.loopDepth.ToString();
            if (context.propertyName() != null)
            {
                targetLabel = context.propertyName().GetText().Trim('"');
                if (int.TryParse(targetLabel, out int result) && result <= parser.loopDepth && result > 0)
                {
                    targetLabel = (parser.loopDepth + 1 - result).ToString();
                }
            }
            targetLabel = LoopEnumeratorBaseName + targetLabel + "_end";

            // Generate the goto statement
            return SyntaxFactory.GotoStatement(
                SyntaxKind.GotoStatement,
                SyntaxFactory.IdentifierName(targetLabel)
            );
        }

        private string exceptionIdentifierName;

        public override SyntaxNode VisitTryStatement([NotNull] TryStatementContext context)
        {
            parser.tryDepth++;
            var prevExceptionIdentifierName = exceptionIdentifierName;
            var elseClaudeIdentifier = "_ks_trythrew_" + parser.tryDepth.ToString();
            // Generate the try block
            var tryBlock = EnsureBlockSyntax(Visit(context.statement()));

            // Generate the Else block (if present)
            StatementSyntax elseCondition = null;
            LocalDeclarationStatementSyntax exceptionVariableDeclaration = null;
            if (context.elseProduction().Else() != null)
            {
                var elseBlock = (BlockSyntax)Visit(context.elseProduction());

                // Declare the `exception` variable
                exceptionVariableDeclaration = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(elseClaudeIdentifier)
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))
                                )
                        )
                    )
                );

                // Add `exception = false;` at the end of the try block
                tryBlock = tryBlock.AddStatements(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(elseClaudeIdentifier),
                            SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)
                        )
                    )
                );

                // Create `if (!exception) { ... }` for the Else block
                elseCondition = SyntaxFactory.IfStatement(
                    SyntaxFactory.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        SyntaxFactory.IdentifierName(elseClaudeIdentifier)
                    ),
                    elseBlock
                );
            }

            // Generate Finally block
            FinallyClauseSyntax finallyClause = null;
            if (context.finallyProduction() != null || elseCondition != null)
            {
                var finallyStatements = new List<StatementSyntax>();
                if (elseCondition != null)
                {
                    finallyStatements.Add(elseCondition);
                }

                if (context.finallyProduction() != null)
                {
                    finallyStatements.AddRange(((BlockSyntax)VisitFinallyProduction(context.finallyProduction())).Statements);
                }

                finallyClause = SyntaxFactory.FinallyClause(SyntaxFactory.Block(finallyStatements));
            }

            // Generate Catch clauses
            
            var catchClauses = new List<CatchClauseSyntax>();
            uint i = 0;
            foreach (var catchProduction in context.catchProduction())
            {
                i++;
                exceptionIdentifierName = "_ks_ex_" + parser.tryDepth.ToString() + "_" + i.ToString();
                catchClauses.Add((CatchClauseSyntax)VisitCatchProduction(catchProduction));
            }

            // Ensure a catch clause for `Keysharp.Core.Error` exists
            if (!catchClauses.Any(c =>
                    c.Declaration != null &&
                    c.Declaration.Type.ToString().Equals("Keysharp.Core.Error", StringComparison.InvariantCultureIgnoreCase)))
            {
                var keysharpErrorCatch = SyntaxFactory.CatchClause()
                    .WithDeclaration(
                        SyntaxFactory.CatchDeclaration(
                            SyntaxFactory.ParseTypeName("Keysharp.Core.Error"),
                            SyntaxFactory.Identifier("_ks_ex_" + parser.tryDepth.ToString() + "_0")
                        )
                    )
                    .WithBlock(SyntaxFactory.Block());

                catchClauses.Add(keysharpErrorCatch);
            }

            catchClauses.Sort(
                (c1, c2) =>
                  {
                      var t1 = Type.GetType(c1.Declaration?.Type.ToString() ?? "", false, true);
                      var t2 = Type.GetType(c2.Declaration?.Type.ToString() ?? "", false, true);

                      if (t1 == t2)
                          return 0;

                      var d1 = Keysharp.Scripting.Parser.TypeDistance(t1, typeof(KeysharpException));
                      var d2 = Keysharp.Scripting.Parser.TypeDistance(t2, typeof(KeysharpException));

                      if (d1 == d2)
                          return 0;
                      else if (d1 < d2)
                          return 1;
                      else
                          return -1;
                  });

            // Construct the TryStatementSyntax
            var tryStatement = SyntaxFactory.TryStatement()
                .WithBlock(tryBlock)
                .WithCatches(SyntaxFactory.List(catchClauses));

            if (finallyClause != null)
            {
                tryStatement = tryStatement.WithFinally(finallyClause);
            }

            parser.tryDepth--;
            exceptionIdentifierName = prevExceptionIdentifierName;

            if (exceptionVariableDeclaration != null)
            {
                return SyntaxFactory.Block(new SyntaxList<StatementSyntax> { exceptionVariableDeclaration, tryStatement });
            }

            return tryStatement;
        }

        public override SyntaxNode VisitCatchProduction([NotNull] CatchProductionContext context)
        {
            // Visit the Catch block
            var block = (BlockSyntax)(Visit(context.flowBlock()));

            var catchAssignable = context.catchAssignable();
            if (catchAssignable != null)
            {
                // Handle optional `As` and `identifier`
                if (catchAssignable.identifier() != null)
                {
                    var catchVarName = parser.NormalizeFunctionIdentifier(catchAssignable.identifier().GetText());
                    if (parser.IsVarDeclaredLocally(catchVarName) != null)
                    {
                        var assignmentStatement = SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName(catchVarName),
                                SyntaxFactory.IdentifierName(exceptionIdentifierName)
                            )
                        );
                        block = SyntaxFactory.Block(
                            block.Statements.Insert(0, assignmentStatement)
                        );
                    } else
                        exceptionIdentifierName = catchVarName;
                }
                
                SyntaxToken exceptionIdentifier = SyntaxFactory.Identifier(exceptionIdentifierName);

                TypeSyntax exceptionType = SyntaxFactory.ParseTypeName("Keysharp.Core.Error");
                var typeConditions = new List<ExpressionSyntax>();

                if (catchAssignable.catchClasses() != null)
                {
                    string catchClassText = null;
                    foreach (var catchClass in catchAssignable.catchClasses().identifier())
                    {
                        catchClassText = catchClass.GetText();
                        if (Reflections.stringToTypes.TryGetValue(catchClassText, out var t))
                            catchClassText = t.FullName;
                        else
                            catchClassText = "Keysharp.Core." + catchClassText;

                        // Create condition: `ex is IndexError`
                        typeConditions.Add(
                            SyntaxFactory.IsPatternExpression(
                                SyntaxFactory.IdentifierName(exceptionIdentifierName),
                                SyntaxFactory.TypePattern(SyntaxFactory.ParseTypeName(catchClassText))
                            )
                        );
                    }
                    if (typeConditions.Count == 1)
                        exceptionType = SyntaxFactory.ParseTypeName(catchClassText);
                } 
                else
                {
                    typeConditions.Add(
                        SyntaxFactory.IsPatternExpression(
                            SyntaxFactory.IdentifierName(exceptionIdentifierName),
                            SyntaxFactory.TypePattern(SyntaxFactory.ParseTypeName("Keysharp.Core.Error"))
                        )
                    );
                }

                ExpressionSyntax conditionExpression = typeConditions[0];

                for (int i = 1; i < typeConditions.Count; i++)
                {
                    conditionExpression = SyntaxFactory.BinaryExpression(
                        SyntaxKind.LogicalOrExpression,
                        conditionExpression,
                        typeConditions[i]
                    );
                }

                CatchFilterClauseSyntax? whenClause = typeConditions.Count > 1
                    ? SyntaxFactory.CatchFilterClause(conditionExpression)
                    : null;

                return SyntaxFactory.CatchClause(
                        SyntaxFactory.CatchDeclaration(exceptionType, exceptionIdentifier),
                        whenClause,
                        block
                    );
            }

            // Catch-all clause
            return SyntaxFactory.CatchClause()
                .WithDeclaration(
                    SyntaxFactory.CatchDeclaration(
                        SyntaxFactory.ParseTypeName("Keysharp.Core.Error"),
                        SyntaxFactory.Identifier(exceptionIdentifierName)
                    )
                )
                .WithBlock(block);
        }

        public override SyntaxNode VisitFinallyProduction([NotNull] FinallyProductionContext context)
        {
            return EnsureBlockSyntax(Visit(context.flowBlock()));
        }

        private bool switchCaseSense = true;
        private uint caseClauseCount = 0;
        private bool switchValueExists = true;
        public override SyntaxNode VisitSwitchStatement([NotNull] SwitchStatementContext context)
        {
            // Extract the switch value (SwitchValue)
            switchValueExists = context.singleExpression() != null;
            var switchValue = switchValueExists
                ? (ExpressionSyntax)Visit(context.singleExpression())
                : SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);

            // Extract case sensitivity (CaseSense)
            LiteralExpressionSyntax caseSense = context.literal() != null
                ? (LiteralExpressionSyntax)Visit(context.literal())
                : SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1)); // Default: case-sensitive
            switchCaseSense = (caseSense.Token.Text == "1L" || caseSense.Token.Text == "1" || caseSense.Token.Text.Equals("on", StringComparison.InvariantCultureIgnoreCase));

            // Visit the case block
            var caseBlock = (SwitchStatementSyntax)VisitCaseBlock(context.caseBlock());

            ExpressionSyntax switchValueToString;
            if (switchValueExists)
            {
                switchValueToString = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        switchValue,
                        SyntaxFactory.IdentifierName("ToString")
                    )
                );
                switchValueToString = !switchCaseSense
                    ? SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            switchValueToString,
                            SyntaxFactory.IdentifierName("ToLower")
                        )
                    )
                    : switchValueToString;
            }
            else
                switchValueToString = switchValue;

            var switchInvocation = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var"),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier("caseIndex"),
                            null,
                            SyntaxFactory.EqualsValueClause(switchValueToString)
                        )
                    )
                )
            );

            // Combine the SwitchHelper invocation and the case block
            return caseBlock.WithExpression(switchValueToString);
        }

        public override SyntaxNode VisitCaseBlock([NotNull] CaseBlockContext context)
        {
            var sections = new List<SwitchSectionSyntax>();
            var caseExpressions = new List<ExpressionSyntax>();

            // Process case clauses
            if (context.caseClauses() != null)
            {
                var allCaseClauses = context.caseClauses(0)?.caseClause()
                    .Concat(context.caseClauses(1)?.caseClause() ?? Enumerable.Empty<CaseClauseContext>())
                    .ToList() ?? Enumerable.Empty<CaseClauseContext>();
                foreach (var caseClause in allCaseClauses)
                {
                    var caseSection = (SwitchSectionSyntax)VisitCaseClause(caseClause);
                    sections.Add(caseSection);

                    // Collect case expressions
                    var clauseExpressions = caseClause.expressionSequence()
                        .expression()
                        .Select(expr => (ExpressionSyntax)Visit(expr));
                    caseExpressions.AddRange(clauseExpressions);
                }
            }

            // Process default clause
            if (context.defaultClause() != null)
            {
                sections.Add((SwitchSectionSyntax)VisitDefaultClause(context.defaultClause()));
            }

            // Return the switch statement
            return SyntaxFactory.SwitchStatement(
                SyntaxFactory.IdentifierName("caseIndex"),
                SyntaxFactory.List(sections)
            );
        }

        public override SyntaxNode VisitCaseClause([NotNull] CaseClauseContext context)
        {
            // Visit the case expressions and generate case labels
            var caseLabels = context.expressionSequence()
                .expression()
                .Select(expr =>
                {
                    ++caseClauseCount;
                    var exprSyntax = (ExpressionSyntax)Visit(expr);

                    // Convert the case expression to string
                    var toStringExpr = switchValueExists ? SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            exprSyntax,
                            SyntaxFactory.IdentifierName("ToString")
                        )
                    ) : exprSyntax;

                    // Handle case-sensitivity by applying ToLower if switchCaseSense is false
                    var comparisonExpr = !switchCaseSense
                        ? SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                toStringExpr,
                                SyntaxFactory.IdentifierName("ToLower")
                            )
                        )
                        : toStringExpr;

                    // Generate either `case true when ...` or `case ...` based on `switchValueExists`
                    if (!switchValueExists)
                    {
                        // Create a `case true when ...` label
                        return SyntaxFactory.CasePatternSwitchLabel(
                            SyntaxFactory.Token(SyntaxKind.CaseKeyword), // case keyword
                            SyntaxFactory.ConstantPattern(
                                SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                            ), // case true
                            SyntaxFactory.WhenClause(
                                SyntaxFactory.CastExpression(
                                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                    ((InvocationExpressionSyntax)InternalMethods.ForceBool)
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList(new[] {
                                                SyntaxFactory.Argument(comparisonExpr) }))
                                    )
                                )
                            ),
                            SyntaxFactory.Token(SyntaxKind.ColonToken) // colon
                        );
                    }
                    else
                    {
                        // Create a condition for the `when` clause
                        var whenClause = SyntaxFactory.WhenClause(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("_ks_string_" + caseClauseCount),
                                    SyntaxFactory.IdentifierName("Equals")
                                ),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(comparisonExpr))
                                )
                            )
                        );

                        // Create a `case string s when ...` label
                        return SyntaxFactory.CasePatternSwitchLabel(
                            SyntaxFactory.Token(SyntaxKind.CaseKeyword), // case keyword
                            SyntaxFactory.DeclarationPattern(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier("_ks_string_" + caseClauseCount))
                            ),
                            whenClause, // when condition
                            SyntaxFactory.Token(SyntaxKind.ColonToken) // colon
                        );
                    }
                })
                .ToArray();

            // Visit the statement list, if present
            var statements = context.statementList() != null
                ? (StatementSyntax)Visit(context.statementList())
                : SyntaxFactory.Block();

            // Ensure a break statement is appended
            statements = EnsureBreakStatement(statements);

            // Return a switch section for this case
            return SyntaxFactory.SwitchSection(
                SyntaxFactory.List<SwitchLabelSyntax>(caseLabels),
                SyntaxFactory.List<StatementSyntax>(
                    statements is BlockSyntax blockSyntax
                        ? blockSyntax.Statements // Unwrap block statements
                        : new[] { statements }   // Wrap single statement in an array
                )
            );
        }


        public override SyntaxNode VisitDefaultClause([NotNull] DefaultClauseContext context)
        {
            // Visit the statement list, if present
            var statements = context.statementList() != null
                ? (StatementSyntax)Visit(context.statementList())
                : SyntaxFactory.Block();

            statements = EnsureBreakStatement(statements);

            // Return a switch section for this default clause
            return SyntaxFactory.SwitchSection(
                SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                    SyntaxFactory.DefaultSwitchLabel()
                ),
                SyntaxFactory.List<StatementSyntax>(
                    statements is BlockSyntax blockSyntax
                        ? blockSyntax.Statements // Unwrap block statements
                        : new[] { statements }   // Wrap single statement in an array
                )
            );
        }

        public override SyntaxNode VisitLabelledStatement([NotNull] LabelledStatementContext context)
        {
            // Get the label identifier
            var labelName = context.Identifier().GetText().Trim('"');

            // Return a labeled statement with an empty statement as the body
            return SyntaxFactory.LabeledStatement(
                SyntaxFactory.Identifier(labelName),
                SyntaxFactory.EmptyStatement()
            );
        }

        public override SyntaxNode VisitGotoStatement([NotNull] GotoStatementContext context)
        {
            // Get the target label
            var labelName = context.propertyName()?.GetText().Trim('"');

            if (labelName == null)
            {
                throw new ArgumentException("Goto target label is missing.");
            }

            // Return the Goto statement
            return SyntaxFactory.GotoStatement(SyntaxKind.GotoStatement, SyntaxFactory.IdentifierName(labelName));
        }


    }
}