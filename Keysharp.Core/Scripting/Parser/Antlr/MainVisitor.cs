using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static MainParser;
using static Keysharp.Scripting.Parser;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Data;
using static Keysharp.Core.Scripting.Parser.Antlr.Helper;
using System.Data.Common;
using System.Windows.Forms;
using Keysharp.Core.Windows;
using Keysharp.Scripting;
using System.ComponentModel;

namespace Keysharp.Core.Scripting.Parser.Antlr
{
    public partial class MainVisitor : MainParserBaseVisitor<SyntaxNode>
    {
        public override SyntaxNode VisitProgram([NotNull] ProgramContext context)
        {
            var t = Keysharp.Core.Common.Invoke.Reflections.stringToTypes["Map"];

            var allClassDeclarations = GetClassDeclarationsRecursive(context);
            foreach (var classDeclaration in allClassDeclarations)
                Helper.UserTypes.Add(classDeclaration.identifier().GetText());

            // Add CompilationUnit usings 
            var usingSyntaxTree = CSharpSyntaxTree.ParseText(CompilerHelper.UsingStr);
            var root = usingSyntaxTree.GetRoot() as CompilationUnitSyntax;
            var usingDirectives = root?.Usings ?? new SyntaxList<UsingDirectiveSyntax>();

            // Generating a single assembly entry is not pretty
            var assemblyAttribute = SyntaxFactory.Attribute(
                CreateQualifiedName("Keysharp.Scripting.AssemblyBuildVersionAttribute"))
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(Accessors.A_AhkVersion))));
            var separatedAttributeList = new SeparatedSyntaxList<AttributeSyntax>();
            separatedAttributeList = separatedAttributeList.Add(assemblyAttribute);
            var attributeList = SyntaxFactory.AttributeList(separatedAttributeList)
                .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)));

            compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectives.ToArray())
                .AddAttributeLists(attributeList);

            var imports = new[]
            {
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Data",
                "System.IO",
                "System.Reflection",
                "System.Runtime.InteropServices",
                "System.Text",
                "System.Threading.Tasks",
                "System.Windows.Forms",
                "Keysharp.Core",
                "Keysharp.Core.Common",
                "Keysharp.Core.Common.File",
                "Keysharp.Core.Common.Invoke",
                "Keysharp.Core.Common.ObjectBase",
                "Keysharp.Core.Common.Strings",
                "Keysharp.Core.Common.Threading",
                "Keysharp.Scripting",
                "Array = Keysharp.Core.Array",
                "Buffer = Keysharp.Core.Buffer"
            };

            // Create using directives
            var usings = new List<UsingDirectiveSyntax>();
            foreach (var import in imports)
            {
                usings.Add(CreateUsingDirective(import));
            }

            namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(CreateQualifiedName("Keysharp.CompiledMain"))
                .AddUsings(usings.ToArray());

            mainClass = SyntaxFactory.ClassDeclaration("program")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var mainFunc = new Helper.Function("Main", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            var mainFuncParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                .WithType(
                    SyntaxFactory.ArrayType(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                    .WithRankSpecifiers(
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression())))));

            var staThreadAttribute = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("System.STAThreadAttribute"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList());

            var mainMethodSeparatedAttributeList = new SeparatedSyntaxList<AttributeSyntax>();
            mainMethodSeparatedAttributeList = mainMethodSeparatedAttributeList.Add(staThreadAttribute);
            var mainMethodAttributeList = SyntaxFactory.AttributeList(mainMethodSeparatedAttributeList);


            mainFunc.Method = mainFunc.Method
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(mainFuncParam)
            .AddAttributeLists(mainMethodAttributeList);

            string mainBodyCode = @"
			{
					try
					{
						string name = ""*"";
						Keysharp.Scripting.Script.Variables.InitGlobalVars();
						Keysharp.Scripting.Script.SetName(name);
						if (Keysharp.Scripting.Script.HandleSingleInstance(name, eScriptInstance.Prompt))
						{
							return 0;
						}
						Keysharp.Core.Env.HandleCommandLineParams(args);
						Keysharp.Scripting.Script.CreateTrayMenu();
						Keysharp.Scripting.Script.RunMainWindow(name, _ks_UserMainCode, false);
						Keysharp.Scripting.Script.WaitThreads();
						return 0;
					}
					catch (Keysharp.Core.Error kserr)
					{
						if (ErrorOccurred(kserr))
						{
							var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
							MsgBox(""Uncaught Keysharp exception:\r\n"" + kserr, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
							Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
						}
						Keysharp.Core.Flow.ExitApp(1);
						return 1;
					}
					catch (System.Exception mainex)
					{
						var ex = mainex.InnerException ?? mainex;

						if (ex is Keysharp.Core.Error kserr)
						{
							if (ErrorOccurred(kserr))
							{
								var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
								MsgBox(""Uncaught Keysharp exception:\r\n"" + kserr, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
								Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
							}
						}
						else
						{
							var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
							MsgBox(""Uncaught exception:\r\n"" + ""Message: "" + ex.Message + ""\r\nStack: "" + ex.StackTrace, $""{Accessors.A_ScriptName}: Unhandled exception"", ""iconx"");
							Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
						}
		;
						Keysharp.Core.Flow.ExitApp(1);
						return 1;
					}
			}
		";

            mainFunc.Body = SyntaxFactory.ParseStatement(mainBodyCode) as BlockSyntax;

            autoExecFunc = new Helper.Function("_ks_UserMainCode", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)));
            currentFunc = autoExecFunc;
            autoExecFunc.Scope = Scope.Global;
            autoExecFunc.Method = autoExecFunc.Method
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            if (context.sourceElements() != null)
                VisitSourceElements(context.sourceElements());

            autoExecFunc.Method = autoExecFunc.Method.WithBody(autoExecFunc.Body.AddStatements(
                SyntaxFactory.ExpressionStatement(((InvocationExpressionSyntax)InternalMethods.ExitApp)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                                SyntaxFactory.Argument(Helper.NumericLiteralExpression("0")) }))))
                ,
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression, 
                        SyntaxFactory.Literal("")))));
            mainFunc.Method = mainFunc.Method.WithBody(mainFunc.Body);
            mainClass = mainClass.AddMembers(mainFunc.Method, autoExecFunc.Method);
            namespaceDeclaration = namespaceDeclaration.AddMembers(mainClass);
            compilationUnit = compilationUnit.AddMembers(namespaceDeclaration).NormalizeWhitespace();

            return compilationUnit;
        }

        public override SyntaxNode VisitSourceElements([NotNull] SourceElementsContext context)
        {
            List<MemberDeclarationSyntax> memberList = [];
            List<StatementSyntax> autoExecStatements = [];
            HashSet<string> classNames = new();

            for (var i = 0; i < context.ChildCount; i++)
            {
                var child = context.GetChild(i).GetChild(0);

                if (!(child is IRuleNode ruleNode))
                    continue; // EOL

                var txt = child.GetText();
                SyntaxNode element = Visit(child);


                if (element == null)
                    throw new NoNullAllowedException();

                var ruleContext = ruleNode.RuleContext;

                switch (ruleContext.RuleIndex)
                {
                    case RULE_statement:
                        if (element is BlockSyntax block)
                        {
                            if (block.GetAnnotatedNodes("MergeStart").FirstOrDefault() != null)
                                autoExecStatements = block.WithoutAnnotations("MergeStart").Statements.Concat(autoExecStatements).ToList();
                            else if (block.GetAnnotatedNodes("MergeEnd").FirstOrDefault() != null)
                                autoExecStatements.AddRange(block.WithoutAnnotations("MergeEnd").Statements);
                            else
                                autoExecStatements.Add(block);
                        }
                        else
                            autoExecStatements.Add((StatementSyntax)element);
                        break;
                    default:
                        if (element is ClassDeclarationSyntax classDecl)
                            classNames.Add(classDecl.Identifier.Text.ToLowerInvariant());

                        memberList.Add((MemberDeclarationSyntax)element);
                        break;
                }
            }

            autoExecFunc.Body = autoExecFunc.Body.AddStatements(autoExecStatements.ToArray());
            mainClass = mainClass.AddMembers(memberList.ToArray());
            return mainClass;
        }

        public override SyntaxNode VisitStatementList(StatementListContext context)
        {
            // Collect all visited statements
            var statements = new List<StatementSyntax>();

            foreach (var statementContext in context.statement())
            {
                var visited = Visit(statementContext);
                if (visited == null)
                {
                    if (statementContext.variableStatement() != null)
                    {
                        continue;
                    }
                }
                if (visited is BlockSyntax block)
                {
                    if (block.GetAnnotatedNodes("MergeStart").FirstOrDefault() != null)
                        statements = block.WithoutAnnotations("MergeStart").Statements.Concat(statements).ToList();
                    else if (block.GetAnnotatedNodes("MergeEnd").FirstOrDefault() != null)
                        statements.AddRange(block.WithoutAnnotations("MergeEnd").Statements);
                    else
                        statements.Add(EnsureStatementSyntax(visited));
                } else
                    statements.Add(EnsureStatementSyntax(visited));
            }

            // Return the statements as a BlockSyntax
            return SyntaxFactory.Block(statements);
        }

        public override SyntaxNode VisitSourceElement([NotNull] SourceElementContext context)
        {
            return base.VisitSourceElement(context);
        }

        public override SyntaxNode VisitStatement([NotNull] MainParser.StatementContext context)
        {
            return Visit(context.GetChild(0));
        }

        public override SyntaxNode VisitIdentifier([NotNull] IdentifierContext context)
        {
            var text = context.GetText();

            if (IsVarDeclaredGlobally(text) == null)
            {
                switch (text.ToLowerInvariant())
                {
                    case "a_linenumber":
                        var contextLineNumber = context.Start.Line;
                        var realLineNumber = Helper.codeLines[contextLineNumber - 1].LineNumber;
                        return SyntaxFactory.CastExpression(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(realLineNumber))
                        );
                    case "a_linefile":
                        mainClass = mainClass.AddMembers(CreatePublicConstant("a_linefile", typeof(string), Path.GetFullPath(Helper.fileName)));
                        break;

                    case "a_scriptdir":
                        mainClass = mainClass.AddMembers(CreatePublicConstant("A_ScriptDir", typeof(string), Path.GetDirectoryName(Path.GetFullPath(Helper.fileName))));
                        break;

                    case "a_scriptfullpath":
                        mainClass = mainClass.AddMembers(CreatePublicConstant("A_ScriptFullPath", typeof(string), Path.GetFullPath(Helper.fileName)));
                        break;

                    case "a_scriptname":
                        mainClass = mainClass.AddMembers(CreatePublicConstant("A_ScriptName", typeof(string), Path.GetFileName(Path.GetFullPath(Helper.fileName))));
                        break;

                    case "a_thisfunc":
                        //return new CodePrimitiveExpression(Scope);
                        //TODO
                        break;
                }
            }

            var name = IsBuiltInProperty(text);
            return SyntaxFactory.IdentifierName(name ?? text.ToLower());
        }

        public override SyntaxNode VisitIdentifierName([NotNull] IdentifierNameContext context)
        {
            if (context.identifier() != null)
                return VisitIdentifier(context.identifier());
            return SyntaxFactory.IdentifierName(context.GetText().ToLower());
        }

        public override SyntaxNode VisitDynamicIdentifierExpression([NotNull] DynamicIdentifierExpressionContext context)
        {
            return Visit(context.dynamicIdentifier());
        }

        public override SyntaxNode VisitDynamicIdentifier([NotNull] DynamicIdentifierContext context)
        {
            // In this case we have a identifier composed of identifier parts and dereference expressions
            // such as a%b%. CreateDynamicVariableAccessor will return Keysharp.Scripting.Script.Vars[string.Concat<object>(new object[] {"a", b)]
            return CreateDynamicVariableAccessor(context);
        }

        public override SyntaxNode VisitDynamicPropertyName([NotNull] DynamicPropertyNameContext context)
        {
            return CreateDynamicVariableAccessor(context);
        }

        public override SyntaxNode VisitVariableStatement([NotNull] VariableStatementContext context)
        {
            var prevScope = currentFunc.Scope;
            
            switch (context.GetChild(0).GetText().ToLower())
            {
                case "local":
                    currentFunc.Scope = Scope.Local;
                    break;
                case "global":
                    currentFunc.Scope = Scope.Global;
                    break;
            }

            if (context.variableDeclarationList() != null && context.variableDeclarationList().ChildCount > 0) {
                var result = VisitVariableDeclarationList(context.variableDeclarationList());
                if (((BlockSyntax)result).Statements.Count != 0)
                {
                    if (prevScope == Scope.Global && prevScope != currentFunc.Scope)
                        throw new Error("Multiple differing scope declarations are not allowed");
                    currentFunc.Scope = prevScope;
                }
                return result;
            }

            // Do nothing, but don't return null
            return SyntaxFactory.Block().WithAdditionalAnnotations(new SyntaxAnnotation("MergeEnd"));
        }

        public override SyntaxNode VisitVariableDeclarationList([NotNull] VariableDeclarationListContext context)
        {
            var declarations = new List<StatementSyntax>();

            foreach (var variableDeclaration in context.variableDeclaration())
            {
                var declaration = (ExpressionStatementSyntax)Visit(variableDeclaration);
                if (declaration != null)
                    declarations.Add(declaration);
            }

            return SyntaxFactory.Block(declarations).WithAdditionalAnnotations(new SyntaxAnnotation("MergeEnd"));
        }

        public override SyntaxNode VisitVariableDeclaration([NotNull] VariableDeclarationContext context)
        {
            var assignableText = context.assignable().GetText();
            var name = MaybeAddVariableDeclaration(assignableText);

            if (currentFunc.Scope == Scope.Global)
            {
                currentFunc.Body = RemoveLocalVariable(currentFunc.Body, name);
                currentFunc.Globals.Add(name);
                currentFunc.Locals.Remove(name);
                if (context.singleExpression() != null)
                {
                    var initializerValue = (ExpressionSyntax)Visit(context.singleExpression());

                    // Create an assignment expression: `variableName = initializerValue`
                    var assignment = SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(name ?? assignableText.ToLowerInvariant()),
                        initializerValue
                    );

                    // Return as an expression statement
                    return SyntaxFactory.ExpressionStatement(assignment);
                }
            }

            var variableName = name ?? context.assignable().GetText().ToLowerInvariant();
            currentFunc.Globals.Remove(variableName);
            currentFunc.Locals.Add(variableName);

            // Check if there is an initializer (e.g., ':= singleExpression')
            if (context.singleExpression() != null)
            {
                var initializerValue = (ExpressionSyntax)Visit(context.singleExpression());

                // Generate the assignment expression: variableName = initializerValue;
                var assignmentExpression = SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(variableName),
                    initializerValue
                );

                // Return the assignment as an expression statement
                return SyntaxFactory.ExpressionStatement(assignmentExpression);
            }

            // Return null if no assignment is needed
            return null;
        }


        public override SyntaxNode VisitArguments([NotNull] ArgumentsContext context)
        {

            var arguments = new List<ArgumentSyntax>();
            var lastChildText = ",";
            foreach (var child in context.argument())
            {
                if (child is ITerminalNode node && node.Symbol.Type == EOL)
                    
                    continue;
                var childText = child.GetText();

                if (childText == ",")
                {
                    if (lastChildText == ",")
                        arguments.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));

                    goto ShouldVisitNextChild;
                }
                SyntaxNode arg = VisitArgument((ArgumentContext)child);
                if (arg != null)
                {
                    if (arg is ArgumentSyntax)
                        arguments.Add((ArgumentSyntax)arg);
                    else
                        throw new Error("Unknown argument type");
                }
                else
                    throw new Error("Unknown function argument");

                ShouldVisitNextChild:
                lastChildText = childText;
            }

            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
        }

        public override SyntaxNode VisitArgument([NotNull] ArgumentContext context)
        {
            if (context.singleExpression() != null)
            {
                ExpressionSyntax arg = (ExpressionSyntax)Visit(context.singleExpression());
                if (context.Multiply() == null)
                    return SyntaxFactory.Argument(arg);
                else
                {
                    InvocationExpressionSyntax flattenedArg = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.IdentifierName("Flatten"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(arg)
                            )
                        )
                    );
                    SyntaxFactory.Argument(flattenedArg);
                }
            }

            throw new Error("VisitArgument failed: unknown context type");
        }
        public override SyntaxNode VisitAssignmentOperator([NotNull] AssignmentOperatorContext context)
        {
            //Console.WriteLine("AssignmentOperator: " + context.GetText());
            return base.VisitAssignmentOperator(context);
        }

        public override SyntaxNode VisitLiteral([NotNull] LiteralContext context)
        {
            if (context.NullLiteral() != null || context.Unset() != null)
            {
                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.NullLiteralExpression,
                    SyntaxFactory.Token(SyntaxKind.NullKeyword)
                );
            }
            else if (context.BooleanLiteral() != null)
            {
                bool.TryParse(context.GetText(), out bool result);
                return SyntaxFactory.LiteralExpression(
                    result ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression
                );
            }
            else if (context.StringLiteral() != null)
            {
                var str = context.StringLiteral().GetText();
                str = str.Substring(1, str.Length - 2); // Remove quotes
                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(EscapedString(str, false))
                );
            }
            else if (context.MultilineStringLiteral() != null)
            {
                // Handle multi-line string literal
                var str = context.MultilineStringLiteral().GetText();
                str = str.Substring(1, str.Length - 2); // Remove quotes
                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(MultilineString(str, 0, "TODO"))
                );
            }
            else if (context.numericLiteral() != null)
            {
                return Helper.NumericLiteralExpression(context.numericLiteral().GetText());
            }
            else if (context.bigintLiteral() != null)
            {
                var value = context.bigintLiteral().GetText();
                if (long.TryParse(value.TrimEnd('n'), out long bigint))
                {
                    return SyntaxFactory.LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(bigint)
                    );
                }
                throw new ValueError($"Invalid bigint literal: {value}");
            }
            else if (context.RegularExpressionLiteral() != null)
            {
                throw new NotImplementedException();
            }

            throw new ValueError($"Unknown literal: {context.GetText()}");
        }

        public override SyntaxNode VisitNumericLiteral([NotNull] NumericLiteralContext context)
        {
            return Helper.NumericLiteralExpression(context.GetText());
        }

        public override SyntaxNode VisitObjectLiteral([NotNull] ObjectLiteralContext context)
        {
            // Collect the property assignments
            var properties = new List<ExpressionSyntax>();
            foreach (var propertyAssignmentContext in context.propertyAssignment())
            {
                // Visit the property assignment to get key-value pairs
                var initializer = (InitializerExpressionSyntax)Visit(propertyAssignmentContext);
                properties.AddRange(initializer.Expressions);
            }

            // Create the object[] array
            var arrayExpression = SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
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
                    SyntaxFactory.SeparatedList(properties)
                )
            );

            // Wrap the array in Keysharp.Core.Objects.Object
            var objectCreationExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Core.Objects"),
                    SyntaxFactory.IdentifierName("Object")
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(arrayExpression))
                )
            );

            return objectCreationExpression;
        }

        public override SyntaxNode VisitPropertyExpressionAssignment([NotNull] PropertyExpressionAssignmentContext context)
        {
            // Visit propertyName and singleExpression
            var propertyName = (ExpressionSyntax)Visit(context.propertyName());
            var propertyValue = (ExpressionSyntax)Visit(context.singleExpression());

            // Return an initializer combining the property name and value
            return SyntaxFactory.InitializerExpression(
                SyntaxKind.ComplexElementInitializerExpression,
                SyntaxFactory.SeparatedList(new[] { propertyName, propertyValue })
            );
        }

        public override SyntaxNode VisitLiteralExpression([NotNull] LiteralExpressionContext context)
        {
            return base.VisitLiteralExpression(context);
        }

        public override SyntaxNode VisitBlock([NotNull] BlockContext context)
        {
            if (context.statementList() == null)
                return SyntaxFactory.Block();
            return Visit(context.statementList());
        }

        public override SyntaxNode VisitIfStatement([NotNull] IfStatementContext context)
        {
            var arguments = new List<ExpressionSyntax>() {
                (ExpressionSyntax)Visit(context.singleExpression())
            };
            var argumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(arguments.Select(arg => SyntaxFactory.Argument(arg)))
            );

            var ifStatement = SyntaxFactory.IfStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),          // Class name
                        SyntaxFactory.IdentifierName("IfTest")                     // Method name
                    ), 
                    argumentList),
                (StatementSyntax)Visit(context.statement(0))
            );

            if (context.statement().Length > 1 && (context.statement(1).GetText() is string e) && e != "")
                ifStatement = ifStatement.WithElse(SyntaxFactory.ElseClause((StatementSyntax)Visit(context.statement(1))));

            return ifStatement;
        }

        public override SyntaxNode VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            ExpressionSyntax returnExpression;

            if (context.singleExpression() != null)
            {
                returnExpression = (ExpressionSyntax)Visit(context.singleExpression());
            }
            else
            {
                returnExpression = SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal("")
                );
            }

            return SyntaxFactory.ReturnStatement(returnExpression);
        }

        public override SyntaxNode VisitThrowStatement([NotNull] ThrowStatementContext context)
        {
            var expression = (ExpressionSyntax)Visit(context.singleExpression());

            if (expression is LiteralExpressionSyntax)
            {
                // Wrap the literal in Keysharp.Core.Error
                return SyntaxFactory.ThrowStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.IdentifierName("Error"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expression))
                        )
                    )
                );
            }

            // Otherwise, return a normal throw statement
            return SyntaxFactory.ThrowStatement(expression);
        }


        public override SyntaxNode VisitTernaryExpression([NotNull] TernaryExpressionContext context)
        {
            var condition = (ExpressionSyntax)Visit(context.singleExpression(0));
            var trueExpression = (ExpressionSyntax)Visit(context.singleExpression(1));
            var falseExpression = (ExpressionSyntax)Visit(context.singleExpression(2));

            // Wrap the condition in Keysharp.Scripting.Script.IfTest(condition) to force a boolean
            var wrappedCondition = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),          // Class name
                    SyntaxFactory.IdentifierName("IfTest")                     // Method name
                ),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(condition))
                )
            );

            // Create a ternary conditional expression: condition ? trueExpression : falseExpression
            return SyntaxFactory.ConditionalExpression(
                wrappedCondition,          // The condition, forced to a boolean
                trueExpression,            // Expression for true branch
                falseExpression            // Expression for false branch
            );
        }

        public override SyntaxNode VisitFormalParameterArg([Antlr4.Runtime.Misc.NotNull] FormalParameterArgContext context)
        {
            //Console.WriteLine("FormalParameterArg: " + context.GetText());
            return base.VisitFormalParameterArg(context);
        }
        public override SyntaxNode VisitAssignable([Antlr4.Runtime.Misc.NotNull] AssignableContext context)
        {
            //Console.WriteLine("Assignable: " + context.GetText());
            //Console.WriteLine(context.children[0].GetText());
            return base.VisitAssignable(context);
        }

        public override SyntaxNode VisitArrowFunctionBody([Antlr4.Runtime.Misc.NotNull] ArrowFunctionBodyContext context)
        {
            //Console.WriteLine("ArrowFunctionBody: " + context.GetText());
            return context.singleExpression() == null ? VisitFunctionBody(context.functionBody()) : SyntaxFactory.Block(SyntaxFactory.ReturnStatement((ExpressionSyntax)Visit(context.singleExpression())));
        }
        public override SyntaxNode VisitInitializer([Antlr4.Runtime.Misc.NotNull] InitializerContext context)
        {
            //Console.WriteLine("Initializer: " + context.GetText());
            return base.VisitInitializer(context);
        }
        ExpressionSyntax FunctionExpressionFromArgumentContext(string methName, IParseTree child)
        {
            InvocationExpressionSyntax invocation = null;
            ArgumentListSyntax argumentList;
            if (child == null)
                argumentList = SyntaxFactory.ArgumentList();
            else
            {
                argumentList = (ArgumentListSyntax)VisitArguments((ArgumentsContext)child);
                if (argumentList == null)
                    throw new Error("Unable to get argument list");
            }

            if (Reflections.flatPublicStaticMethods.TryGetValue(methName, out var mi))
                invocation = SyntaxFactory.InvocationExpression(CreateQualifiedName(mi.DeclaringType + "." + mi.Name), argumentList);
            else
            {
                // Cast methName to ICallable: ((ICallable)methName)
                var castToICallable = SyntaxFactory.ParenthesizedExpression(
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.IdentifierName("ICallable"), // Target type: ICallable
                        SyntaxFactory.IdentifierName(methName.ToLower())     // methName
                    )
                );

                // Access the .Call method: ((ICallable)methName).Call
                var memberAccess = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    castToICallable,
                    SyntaxFactory.IdentifierName("Call")
                );

                // Final invocation: (ICallable(methName)).Call(arguments)
                invocation = SyntaxFactory.InvocationExpression(
                    memberAccess,
                    argumentList
                );
            }
            return invocation;
        }

        public override SyntaxNode VisitFunctionStatement([Antlr4.Runtime.Misc.NotNull] FunctionStatementContext context)
        {
            return SyntaxFactory.ExpressionStatement(FunctionExpressionFromArgumentContext(context.identifier().GetText(), context.arguments()));
        }

        public override SyntaxNode VisitFunctionDeclaration([NotNull] FunctionDeclarationContext context)
        {
            var prevFunc = currentFunc;
            currentFunc = new Helper.Function(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(context.identifier().GetText()));

            if (functionDepth == 0)
                Helper.AddGlobalFuncObjVariable(currentFunc.Name);

            functionDepth++;

            if (context.formalParameterList() != null)
                currentFunc.Params = (ParameterListSyntax)VisitFormalParameterList(context.formalParameterList());

            BlockSyntax functionBody = context.functionBody() == null ? (BlockSyntax)VisitArrowFunctionBody(context.arrowFunctionBody()) : (BlockSyntax)VisitFunctionBody(context.functionBody());
            currentFunc.Body = EnsureReturnStatement(currentFunc.Body.AddStatements(functionBody.Statements.ToArray()));

            functionDepth--;

            if (functionDepth > 0)
            {
                var block = SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        CreateFuncObjDelegateVariable(currentFunc.Name)
                    ),
                    SyntaxFactory.LocalFunctionStatement(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Assuming return type is void
                        SyntaxFactory.Identifier(currentFunc.Name) // Function name
                    )
                    .WithParameterList(currentFunc.Params)
                    .WithBody(currentFunc.Body)
                    .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                    )).WithAdditionalAnnotations(new SyntaxAnnotation("MergeStart"))
                );
                currentFunc = prevFunc;
                return block;
            }

            var methodDeclaration = currentFunc.Method
                .WithParameterList(currentFunc.Params)
                .WithBody(currentFunc.Body)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                    )
                );
            currentFunc = prevFunc;
            return methodDeclaration;
        }

        public override SyntaxNode VisitFunctionExpression([NotNull] FunctionExpressionContext context)
        {
            return Visit(context.anonymousFunction());
        }

        /*
        public override SyntaxNode VisitNamedFunction([NotNull] NamedFunctionContext context)
        {
            return VisitFunctionDeclaration(context.functionDeclaration());
        }

        public override SyntaxNode VisitAnonymousFunctionDecl([NotNull] AnonymousFunctionDeclContext context)
        {
            var isAsync = context.Async() != null;

            // Visit parameters
            ParameterListSyntax formalParams = context.formalParameterList() != null
                ? (ParameterListSyntax)VisitFormalParameterList(context.formalParameterList())
                : SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>());

            // Visit the function body
            BlockSyntax functionBody = (BlockSyntax)VisitFunctionBody(context.functionBody());
            functionBody = EnsureReturnStatement(functionBody);

            // Generate the anonymous method
            var lambda = SyntaxFactory.ParenthesizedLambdaExpression()
                .WithAsyncKeyword(isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default)
                .WithParameterList(formalParams)
                .WithBlock(functionBody);

            return lambda;
        }
        */

        public override SyntaxNode VisitArrowFunction([NotNull] ArrowFunctionContext context)
        {
            var isAsync = context.Async() != null;

            // Visit parameters
            ParameterListSyntax formalParams = (ParameterListSyntax)VisitArrowFunctionParameters(context.arrowFunctionParameters());

            // Determine the arrow function body
            CSharpSyntaxNode arrowBody;
            if (context.arrowFunctionBody().functionBody() != null)
            {
                // If it's a function body, wrap it in a block
                arrowBody = (BlockSyntax)VisitFunctionBody(context.arrowFunctionBody().functionBody());
                arrowBody = EnsureReturnStatement((BlockSyntax)arrowBody);
                return arrowBody;
            }
            else
            {
                // If it's a single expression, directly visit it
                arrowBody = (ExpressionSyntax)Visit(context.arrowFunctionBody().singleExpression());

                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("FuncObj"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.ParenthesizedLambdaExpression()
                                        .WithAsyncKeyword(isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default)
                                        .WithParameterList(formalParams)
                                        .WithExpressionBody((ExpressionSyntax)arrowBody)
                                )
                            )
                        )
                    );
            }
        }

        public override SyntaxNode VisitArrowFunctionParameters([NotNull] ArrowFunctionParametersContext context)
        {
            if (context.formalParameterList() != null)
                return (ParameterListSyntax)VisitFormalParameterList(context.formalParameterList());
            else
                return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>());
        }

        private BlockSyntax EnsureReturnStatement(BlockSyntax functionBody)
        {
            var statements = functionBody.Statements;

            bool hasReturn = statements.OfType<ReturnStatementSyntax>().Any();

            if (!hasReturn)
            {
                // Append a default return ""; statement
                var defaultReturn = SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""))
                );

                statements = statements.Add(defaultReturn);
            }

            return SyntaxFactory.Block(statements);
        }

        public override SyntaxNode VisitFormalParameterList([NotNull] FormalParameterListContext context)
        {
            var parameters = new List<ParameterSyntax>();

            foreach (var formalParameter in context.formalParameterArg())
            {
                var parameterName = formalParameter.assignable().GetText().Trim();

                // Create a parameter with type 'object'
                var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)));

                if (formalParameter.BitAnd() != null)
                {
                    parameter = parameter.WithModifiers(
                        SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword))
                    );
                }

                // Handle default value assignment (:=) or optional parameter (QuestionMark)
                if (formalParameter.singleExpression() != null)
                {
                    var defaultValue = (ExpressionSyntax)Visit(formalParameter.singleExpression());
                    parameter = parameter.WithDefault(SyntaxFactory.EqualsValueClause(defaultValue));
                }
                else if (formalParameter.QuestionMark() != null)
                {
                    // If QuestionMark is present, mark the parameter as optional with null default value
                    parameter = parameter.WithDefault(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                        )
                    );
                }

                // Add the parameter to the list
                parameters.Add(parameter);
            }

            // Handle the last formal parameter argument if it exists
            if (context.lastFormalParameterArg() != null)
            {
                var lastParamContext = context.lastFormalParameterArg();

                if (lastParamContext.Multiply() != null)
                {
                    // Handle 'Multiply' for variadic arguments (params object[])
                    var lastParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")) // Default name for spread argument
                        .WithType(SyntaxFactory.ArrayType(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // object[]
                            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()
                                )
                            ))
                        ))
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));

                    parameters.Add(lastParameter);
                }
                else if (lastParamContext.formalParameterArg() != null)
                {
                    // Treat as a regular parameter
                    var lastFormalParam = lastParamContext.formalParameterArg();
                    var parameterName = lastFormalParam.assignable().GetText().Trim();

                    var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                        .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)));

                    // Handle BitAnd for by-reference
                    if (lastFormalParam.BitAnd() != null)
                    {
                        parameter = parameter.WithModifiers(
                            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword))
                        );
                    }

                    // Handle optional parameter
                    if (lastFormalParam.QuestionMark() != null)
                    {
                        parameter = parameter.WithDefault(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            )
                        );
                    }

                    parameters.Add(parameter);
                }
            }

            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
        }

        public override SyntaxNode VisitFunctionBody([NotNull] FunctionBodyContext context)
        {
            if (context.statementList() == null || context.statementList().ChildCount == 0)
            {
                return SyntaxFactory.Block(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""))
                    )
                );
            }
            return VisitStatementList(context.statementList());
            /*
            var statements = new List<StatementSyntax>();

            foreach (var statementContext in context.statementList().statement())
            {
                var statement = VisitStatement(statementContext);
                if (statement == null)
                    throw new Error("Invalid statement");
                if (statement is BlockSyntax block)
                    statements.AddRange(block.Statements);
                else
                    statements.Add((StatementSyntax)statement);
            }

            return SyntaxFactory.Block(statements);
            */
        }

        public override SyntaxNode VisitArrayLiteral(ArrayLiteralContext context)
        {
            // Visit the arrayElementList to get all the elements
            var elementsInitializer = (InitializerExpressionSyntax)VisitArrayElementList(context.arrayElementList());

            // Wrap the array initializer in a call to 'new Keysharp.Core.Array(...)'
            var keysharpArrayCreation = SyntaxFactory.ObjectCreationExpression(
                CreateQualifiedName("Keysharp.Core.Array"), // Class name: Keysharp.Core.Array
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(
                            SyntaxFactory.ArrayCreationExpression(
                                SyntaxFactory.ArrayType(
                                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // object[]
                                    SyntaxFactory.SingletonList(
                                        SyntaxFactory.ArrayRankSpecifier(
                                            SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                SyntaxFactory.OmittedArraySizeExpression()
                                            )
                                        )
                                    )
                                ),
                                elementsInitializer
                            )
                        )
                    )
                ),
                null // No object initializers
            );

            return keysharpArrayCreation;
        }

        public override SyntaxNode VisitArrayElementList(ArrayElementListContext context)
        {
            var expressions = new List<ExpressionSyntax>();

            var lastText = ","; // Initialize to "," to handle leading empty slots (e.g., [,,1])
            foreach (var child in context.children)
            {
                var childText = child.GetText();
                if (child is ArrayElementContext arrayElementContext)
                {
                    // Visit non-empty array elements
                    var element = (ExpressionSyntax)VisitArrayElement(arrayElementContext);
                    expressions.Add(element);
                }
                else if (childText == ",")
                {
                    if (lastText == childText)
                        // Add an empty slot represented by `null`
                        expressions.Add(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
                }
                lastText = childText;
            }
            if (context.ChildCount != 0 && lastText == ",")
                expressions.Add(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));

            // Wrap the expressions in an InitializerExpressionSyntax
            return SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList(expressions)
                );
        }

        public override SyntaxNode VisitArrayElement(ArrayElementContext context)
        {
            // Visit the single expression
            var element = (ExpressionSyntax)Visit(context.singleExpression());

            // If the Multiply (*) is present, wrap it with FlattenParam
            if (context.Multiply() != null)
            {
                var flattenInvocation = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("FlattenParam"), // FlattenParam function
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(element) // Pass the visited expression as an argument
                        )
                    )
                );

                // Add the spread operator `..` using a PrefixUnaryExpression
                return SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.SpreadElement, // Custom Roslyn spread operator syntax kind
                    flattenInvocation
                );

            }

            // Return the element as is
            return element;
        }
    }
}