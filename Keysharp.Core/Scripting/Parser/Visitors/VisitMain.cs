using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using static MainParser;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using static Keysharp.Scripting.Parser;
using System.Reflection;
using System.Xml.Linq;

namespace Keysharp.Scripting
{
    internal partial class VisitMain : MainParserBaseVisitor<SyntaxNode>
    {
        Keysharp.Scripting.Parser parser;
        public VisitMain(Keysharp.Scripting.Parser _parser) : base()
        {
            parser = _parser;
        }

        public override SyntaxNode VisitProgram([NotNull] ProgramContext context)
        {
            // Add CompilationUnit usings 
            var usingSyntaxTree = CSharpSyntaxTree.ParseText(CompilerHelper.UsingStr);
            var root = usingSyntaxTree.GetRoot() as CompilationUnitSyntax;
            var usingDirectives = root?.Usings ?? new SyntaxList<UsingDirectiveSyntax>();

            parser.AddAssembly("Keysharp.Scripting.AssemblyBuildVersionAttribute", Accessors.A_AhkVersion);

            parser.compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectives.ToArray());

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
                usings.Add(parser.CreateUsingDirective(import));
            }

            parser.namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(CreateQualifiedName("Keysharp.CompiledMain"))
                .AddUsings(usings.ToArray());

            parser.currentClass = new Parser.Class(Keywords.MainClassName, null);
            parser.mainClass = parser.currentClass;

            var mainFunc = new Function("Main", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            var mainFuncParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                .WithType(PredefinedTypes.StringArray);

            var staThreadAttribute = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("System.STAThreadAttribute"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList());

            var mainMethodSeparatedAttributeList = new SeparatedSyntaxList<AttributeSyntax>();
            mainMethodSeparatedAttributeList = mainMethodSeparatedAttributeList.Add(staThreadAttribute);
            var mainMethodAttributeList = SyntaxFactory.AttributeList(mainMethodSeparatedAttributeList);

            mainFunc.Params.Add(mainFuncParam);

            parser.mainFuncInitial.Add($"string name = @\"{Path.GetFullPath(parser.fileName)}\";");
            foreach (var (p, s) in parser.reader.PreloadedDlls)
            {
                parser.mainFuncInitial.Add($"Keysharp.Scripting.Script.Variables.AddPreLoadedDll(\"{p}\", {s.ToString().ToLower()});");
            }

            string mainBodyCode = $$"""
			{
					try
					{
						{{String.Join(Environment.NewLine, parser.mainFuncInitial)}}
						Keysharp.Scripting.Script.Variables.InitGlobalVars(typeof({{Keywords.MainClassName}}));
						Keysharp.Scripting.Script.SetName(name);
						if (Keysharp.Scripting.Script.HandleSingleInstance(name, eScriptInstance.{{System.Enum.GetName(typeof(eScriptInstance), parser.reader.SingleInstance)}}))
						{
							return 0;
						}
						Keysharp.Core.Env.HandleCommandLineParams(args);
						Keysharp.Scripting.Script.CreateTrayMenu();
						Keysharp.Scripting.Script.RunMainWindow(name, {{Keywords.AutoExecSectionName}}, false);
						Keysharp.Scripting.Script.WaitThreads();
					}
                    catch (Keysharp.Core.Flow.UserRequestedExitException)
                    {
                    }
					catch (Keysharp.Core.Error kserr)
					{
						if (ErrorOccurred(kserr))
						{
							var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
							MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
							Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
						}
						Keysharp.Core.Flow.ExitApp(1);
					}
					catch (System.Exception mainex)
					{
						var ex = mainex.InnerException ?? mainex;

						if (ex is Keysharp.Core.Error kserr)
						{
							if (ErrorOccurred(kserr))
							{
								var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
								MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
								Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
							}
						}
						else
						{
							var (_ks_pushed, _ks_btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
							MsgBox("Uncaught exception:\r\n" + "Message: " + ex.Message + "\r\nStack: " + ex.StackTrace, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
							Keysharp.Core.Common.Threading.Threads.EndThread(_ks_pushed);
						}
						Keysharp.Core.Flow.ExitApp(1);
					}
				return Environment.ExitCode;
			}
""";

            var mainBodyBlock = SyntaxFactory.ParseStatement(mainBodyCode) as BlockSyntax;
            mainFunc.Body = mainBodyBlock.Statements.ToList();

            parser.autoExecFunc = new Function(Keywords.AutoExecSectionName, SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)));
            parser.currentFunc = parser.autoExecFunc;
            parser.autoExecFunc.Scope = eScope.Global;
            parser.autoExecFunc.Method = parser.autoExecFunc.Method
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            // Map out all user class types and also any built-in types they derive from
            var allClassDeclarations = GetClassDeclarationsRecursive(context);
            foreach (var classDeclaration in allClassDeclarations)
            {
                var className = classDeclaration.identifier().GetText();
                var classBase = classDeclaration.classExtensionName()?.GetText() ?? "KeysharpObject";
                parser.UserTypes[className] = classBase;
                parser.AllTypes[className] = classBase;
                while (Reflections.stringToTypes.TryGetValue(classBase, out Type baseType))
                {
                    classBase = parser.AllTypes[baseType.Name] = baseType.BaseType.Name;
                }
            }

            // Create global FuncObj variables for all functions here, because otherwise during parsing
            // we might not know how to case the name.
            var scopeFunctionDeclarations = GetScopeFunctions(context);
            foreach (var funcName in scopeFunctionDeclarations)
            {
                parser.UserFuncs.Add(funcName.Name);
                parser.AddGlobalFuncObjVariable(funcName.Name.ToLowerInvariant());
            }

            if (context.sourceElements() != null)
                VisitSourceElements(context.sourceElements());

            GenerateGeneralDirectiveStatements();

            if (parser.persistent)
				parser.DHHR.Add(SyntaxFactory.ExpressionStatement(
	                SyntaxFactory.InvocationExpression(
		                SyntaxFactory.MemberAccessExpression(
			                SyntaxKind.SimpleMemberAccessExpression,
			                CreateQualifiedName("Keysharp.Core.Flow"),
			                SyntaxFactory.IdentifierName("Persistent")
		                )
	                )
                ));

			// Merge positional directives, hotkeys, hotstrings to the beginning of the auto-execute section
			parser.DHHR.Add(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Core.Common.Keyboard.HotkeyDefinition"),
                        SyntaxFactory.IdentifierName("ManifestAllHotkeysHotstringsHooks")
                    )
                )
            ));

            parser.autoExecFunc.Body = 
                parser.generalDirectiveStatements.Concat(parser.DHHR)
                .Concat(parser.autoExecFunc.Body)
                .ToList();
    
            // Return "" by default
            parser.autoExecFunc.Body.Add(SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(""))));
            parser.autoExecFunc.Method = parser.autoExecFunc.Assemble();

            mainFunc.Method = mainFunc.Assemble().AddAttributeLists(mainMethodAttributeList);
            // Add the Main function to the beginning, and AutoExecSection to the end. Keyview requires Main to be at the beginning
            parser.mainClass.Body.Insert(0, mainFunc.Method);
            parser.mainClass.Body.Add(parser.autoExecFunc.Method);
            parser.namespaceDeclaration = parser.namespaceDeclaration.AddMembers(parser.mainClass.Assemble());

            var attributeList = SyntaxFactory.AttributeList(parser.assemblies)
                .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)));

            // Using tabs as indentation rather than spaces seems to be more performant.
            // Not normalizing whitespaces is even faster though.
            parser.compilationUnit = parser.compilationUnit
                .AddMembers(parser.namespaceDeclaration)
                .AddAttributeLists(attributeList)
                .NormalizeWhitespace("\t");

            return parser.compilationUnit;
        }

        public override SyntaxNode VisitSourceElements([NotNull] SourceElementsContext context)
        {
            List<StatementSyntax> autoExecStatements = [];
            HashSet<string> classNames = new();

            for (var i = 0; i < context.ChildCount; i++)
            {
                var child = context.GetChild(i).GetChild(0);

                if (child is not IRuleNode ruleNode)
                    continue; // EOL or WS

                //var txt = child.GetText();
                SyntaxNode element = Visit(child);

                if (element == null)
                {
                    switch (ruleNode.RuleContext.RuleIndex)
                    {
                        case RULE_positionalDirective:
                        case RULE_remap:
                        case RULE_hotkey:
                        case RULE_hotstring:
                            continue;
                        default:
                            throw new NoNullAllowedException();
                    }
                }

                var ruleContext = ruleNode.RuleContext;

                if (element is MemberDeclarationSyntax)
                {
                    parser.mainClass.Body.Add((MemberDeclarationSyntax)element);
                    continue;
                }

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

                        parser.mainClass.Body.Add((MemberDeclarationSyntax)element);
                        break;
                }
            }

            parser.autoExecFunc.Body.AddRange(autoExecStatements);
            return parser.mainClass.Declaration;
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
                } 
                else if (visited is MemberDeclarationSyntax memberDeclarationSyntax)
                {
                    // This shouldn't happen anywhere else but the auto-execute section
                    // when the function declaration is inside a block for example
                    parser.mainClass.Body.Add(memberDeclarationSyntax);
				}
				else if (visited is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    // In case a class is declared inside a function (such as some unit tests)
                    parser.mainClass.Body.Add(classDeclarationSyntax);
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
            if (context.iterationStatement() != null)
            {
                var prevLabel = parser.loopLabel;
                parser.loopDepth++;
                // May label an iterationStatement
                LabeledStatementSyntax label = null;
                if (context.labelledStatement() != null)
                {
                    parser.loopLabel = context.labelledStatement().Identifier().GetText();
                    label = (LabeledStatementSyntax)VisitLabelledStatement(context.labelledStatement());
                }
                else
                    parser.loopLabel = parser.loopDepth.ToString();

                BlockSyntax result = (BlockSyntax)Visit(context.iterationStatement());
                if (label != null)
                    result = result.WithStatements(result.Statements.Insert(0, label));
                parser.loopDepth--;
                parser.loopLabel = prevLabel;
                return result;
            }

            return Visit(context.GetChild(0));
        }

        // This should always return the identifier in the exact case needed
        // Built-in properties: a_scriptdir -> A_ScriptDir
        // Variables are turned lowercase: HellO -> hello
        // Static variables get the function name added in upper-case: a -> FUNCNAME_a
        // Special keywords do not get @ added here
        public override SyntaxNode VisitIdentifier([NotNull] IdentifierContext context)
        {
            var text = context.GetText();

            switch (text.ToLowerInvariant())
            {
                case "this":
                    return SyntaxFactory.IdentifierName("@this");
                case "base":
                    return parser.currentClass.Name == Keywords.MainClassName ? SyntaxFactory.IdentifierName("@base") : SyntaxFactory.BaseExpression();
                case "super":
                    return parser.CreateSuperTuple();
            }

            // Handle special built-ins
            if (parser.IsVarDeclaredGlobally(text) == null)
            {
                switch (text.ToLowerInvariant())
                {
                    case "a_linenumber":
                        var contextLineNumber = context.Start.Line;
                        return SyntaxFactory.CastExpression(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(contextLineNumber))
                        );
                    case "a_linefile":
                        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(context.Start.InputStream.SourceName));

                    case "a_scriptfullpath":
                        parser.mainClass.Body.Add(CreatePublicConstant("A_ScriptFullPath", typeof(string), Path.GetFullPath(parser.fileName)));
                        break;
                }
            }

            return HandleIdentifierName(text);
        }

        private SyntaxNode HandleIdentifierName(string text)
        {
            text = parser.NormalizeFunctionIdentifier(text);

            parser.MaybeAddClassStaticVariable(text, false); // This needs to be before FuncObj one, because "object" can be both
            parser.MaybeAddGlobalFuncObjVariable(text, false);

            var vr = parser.IsVarRef(text);
            if (vr != null)
            {
                var debug = parser.currentFunc;
                // If it's a VarRef, access the __Value member
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ParenthesizedExpression(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.IdentifierName("VarRef"),
                            SyntaxFactory.IdentifierName(vr))), // Identifier name
                    SyntaxFactory.IdentifierName("__Value")       // Member access
                );
            }

            return SyntaxFactory.IdentifierName(text);
        }

        public override SyntaxNode VisitReservedWord([NotNull] ReservedWordContext context)
        {
            return SyntaxFactory.IdentifierName(context.GetText().ToLowerInvariant());
        }

        public override SyntaxNode VisitKeyword([NotNull] KeywordContext context)
        {
            return HandleIdentifierName(context.GetText().ToLower());
        }

        public override SyntaxNode VisitIdentifierName([NotNull] IdentifierNameContext context)
        {
            if (context.identifier() != null)
                return VisitIdentifier(context.identifier());
            return SyntaxFactory.IdentifierName(context.GetText().ToLower());
        }

        public override SyntaxNode VisitVarRefExpression([NotNull] VarRefExpressionContext context)
        {
            // Visit the singleExpression to determine its type
            var targetExpression = Visit(context.primaryExpression()) as ExpressionSyntax;

            if (targetExpression == null)
                throw new InvalidOperationException("Unsupported singleExpression type for VarRefExpression.");

            return parser.ConstructVarRef(targetExpression);
        }


        public override SyntaxNode VisitDynamicIdentifierExpression([NotNull] DynamicIdentifierExpressionContext context)
        {
            return Visit(context.dynamicIdentifier());
        }

        public override SyntaxNode VisitDynamicIdentifier([NotNull] DynamicIdentifierContext context)
        {
            var dynVar = (ExpressionSyntax)CreateDynamicVariableString(context);

            parser.currentFunc.HasDerefs = true;

            // In this case we have a identifier composed of identifier parts and dereference expressions
            // such as a%b%. CreateDynamicVariableAccessor will return string.Concat<object>(new object[] {"a", b), so
            // to turn it into an identifier we need to wrap it in Keysharp.Scripting.Script.Vars[]
            return SyntaxFactory.ElementAccessExpression(
                parser.currentFunc.Name == Keywords.AutoExecSectionName 
                    ? SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        CreateQualifiedName("Keysharp.Scripting.Script"),
                        SyntaxFactory.IdentifierName("Vars")
                      )
                    : SyntaxFactory.IdentifierName(InternalPrefix + "Derefs")
                ,
                SyntaxFactory.BracketedArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(dynVar)
                    )
                )
            );
        }

        public override SyntaxNode VisitVariableStatement([NotNull] VariableStatementContext context)
        {
            var prevScope = parser.currentFunc.Scope;
            
            switch (context.GetChild(0).GetText().ToLower())
            {
                case "local":
                    parser.currentFunc.Scope = eScope.Local;
                    break;
                case "global":
                    parser.currentFunc.Scope = eScope.Global;
                    break;
                case "static":
                    parser.currentFunc.Scope = eScope.Static;
                    break;
            }

            if (context.variableDeclarationList() != null && context.variableDeclarationList().ChildCount > 0) {
                var result = VisitVariableDeclarationList(context.variableDeclarationList());
                parser.currentFunc.Scope = prevScope;
                return result;
            }

            if (prevScope == eScope.Global && prevScope != parser.currentFunc.Scope)
                throw new Error("Multiple differing scope declarations are not allowed");

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
            SyntaxNode node = Visit(context.assignable());
            if (!(node is IdentifierNameSyntax))
            {
                throw new Error();
            }
            IdentifierNameSyntax identifier = (IdentifierNameSyntax)node;

            var name = identifier.Identifier.Text;

            parser.MaybeAddVariableDeclaration(name);

            if (parser.currentFunc.Scope == eScope.Global)
            {
                parser.currentFunc.Locals.Remove(name);
                parser.currentFunc.Globals.Add(name);
                if (context.assignmentOperator() != null)
                {
                    var initializerValue = (ExpressionSyntax)Visit(context.expression());

                    return SyntaxFactory.ExpressionStatement(HandleAssignment(
                        identifier,
                        initializerValue,
                        context.assignmentOperator().GetText()));
                }
            } else if (parser.currentFunc.Scope == eScope.Local) {
                parser.currentFunc.Globals.Remove(name);
                // MaybeAddVariableDeclaration added the Locals entry
            }

            // Check if there is an initializer (e.g., ':= singleExpression')
            if (context.assignmentOperator() != null)
            {
                var initializerValue = (ExpressionSyntax)Visit(context.expression());

                if (parser.currentFunc.Scope == eScope.Static)
                {
                    return parser.CreateStaticVariableInitializer(identifier, initializerValue);
                }

                return SyntaxFactory.ExpressionStatement(HandleAssignment(
                    identifier,
                    initializerValue,
                    context.assignmentOperator().GetText()));
            }
            else if (context.op != null)
            {
                return SyntaxFactory.ExpressionStatement(HandleCompoundAssignment(identifier, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1L)), context.op.Text == "++" ? "+=" : "-=", isPostFix: true));
            }

            // Return null if no assignment is needed
            return null;
        }


        public override SyntaxNode VisitArguments([NotNull] ArgumentsContext context)
        {

            var arguments = new List<SyntaxNode>();
            bool lastIsComma = true;
            bool containsSpread = false;
            int lastDefinedElement = 0;
            for (var i = 0; i < context.ChildCount; i++)
            {
                var child = context.GetChild(i);
                bool isComma = false;
                if (child is ITerminalNode node)
                {
                    if (node.Symbol.Type == MainParser.EOL || node.Symbol.Type == MainParser.WS)
                        continue;
                    else if (node.Symbol.Type == MainParser.Comma)
                        isComma = true;
                }

                if (isComma)
                {
                    if (lastIsComma)
                        arguments.Add(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));

                    goto ShouldVisitNextChild;
                }
                SyntaxNode arg = VisitArgument((ArgumentContext)child);
                if (arg != null)
                {
                    if (arg is ExpressionSyntax)
                        arguments.Add(arg);
                    else if (arg is SpreadElementSyntax)
                    {
                        arguments.Add(arg);
                        containsSpread = true;
                    }
                    else
                        throw new Error("Unknown argument type");

                    lastDefinedElement = arguments.Count;
                }
                else
                    throw new Error("Unknown function argument");

                ShouldVisitNextChild:
                lastIsComma = isComma;
            }

            if (arguments.Count > lastDefinedElement)
                arguments.RemoveRange(lastDefinedElement, arguments.Count - lastDefinedElement);

            if (!containsSpread)
            {
                // No spread elements present, wrap all elements in ArgumentSyntax and return as ArgumentListSyntax
                var normalArguments = arguments
                    .Select(expr => SyntaxFactory.Argument((ExpressionSyntax)expr))
                    .ToList();

                return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(normalArguments));
            }

            // If spread elements are present, convert all elements into CollectionElements
            var collectionElements = new List<CollectionElementSyntax>();

            foreach (var node in arguments)
            {
                if (node is ExpressionSyntax expr)
                {
                    collectionElements.Add(SyntaxFactory.ExpressionElement(expr));
                }
                else if (node is SpreadElementSyntax spread)
                {
                    collectionElements.Add(spread);
                }
            }

            // Create a CollectionExpressionSyntax
            var collectionExpression = SyntaxFactory.CollectionExpression(SyntaxFactory.SeparatedList(collectionElements));

            // Wrap in a single argument and return
            return SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(collectionExpression)
                )
            );
        }

        public override SyntaxNode VisitArgument([NotNull] ArgumentContext context)
        {
            ExpressionSyntax arg = null;
            if (context.expression() != null)
                arg = (ExpressionSyntax)Visit(context.expression());
            else if (context.primaryExpression() != null)
                arg = (ExpressionSyntax)Visit(context.primaryExpression());

            if (arg != null)
            {
                if (context.Multiply() == null)
                    return arg;
                else
                {
                    var invocationExpression = ((InvocationExpressionSyntax)InternalMethods.FlattenParam)
                        .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(arg) // Passing `arg` as the function argument
                            )
                        )
                    );

                    // Add the spread operator `..`
                    return SyntaxFactory.SpreadElement(invocationExpression);
                }
            }

            throw new Error("VisitArgument failed: unknown context type");
        }
        public override SyntaxNode VisitAssignmentOperator([NotNull] AssignmentOperatorContext context)
        {
            //Console.WriteLine("AssignmentOperator: " + context.GetText());
            return base.VisitAssignmentOperator(context);
        }

        public override SyntaxNode VisitBoolean([NotNull] BooleanContext context)
        {
            bool.TryParse(context.GetText(), out bool result);
            return SyntaxFactory.LiteralExpression(
                result ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression
            );
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
            else if (context.boolean() != null)
            {
                return Visit(context.boolean());
            }
            else if (context.StringLiteral() != null)
            {
                var str = context.StringLiteral().GetText();
                str = str.Substring(1, str.Length - 2); // Remove quotes
                if (str.Contains('\n') || str.Contains('\r'))
                {
                    List<string> processedSections = new();
                    int currentLine = context.Start.Line;

                    using (var reader = new StringReader(str))
                    {
                        StringBuilder sectionBuilder = new();
                        bool inContinuation = false; // Track whether we're inside a continuation section

                        while (reader.Peek() != -1)
                        {
                            var line = reader.ReadLine() ?? "";

                            // Check for the start of a continuation section
                            var trimmedLine = line.Trim();
                            if (trimmedLine.StartsWith("("))
                            {
                                sectionBuilder.AppendLine(trimmedLine);
                                inContinuation = true;
                            }
                            // Check for the end of a continuation section
                            else if (inContinuation && trimmedLine == ")")
                            {
                                sectionBuilder.Append(trimmedLine);
                                inContinuation = false;

                                var newstr = sectionBuilder.ToString();

                                processedSections.Add(MultilineString(sectionBuilder.ToString(), currentLine, "TODO"));
                                sectionBuilder.Clear();

                                continue; // Skip this line
                            }
                            else if (inContinuation)
                                sectionBuilder.AppendLine(line);
                        }
                    }

                    // Combine all processed sections
                    str = string.Join("", processedSections);
                }
                
                str = EscapedString(str, false);

                return SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(str)
                );
            }
            else if (context.numericLiteral() != null)
            {
                return NumericLiteralExpression(context.numericLiteral().GetText());
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

            throw new ValueError($"Unknown literal: {context.GetText()}");
        }

        public override SyntaxNode VisitNumericLiteral([NotNull] NumericLiteralContext context)
        {
            return NumericLiteralExpression(context.GetText());
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
            var propertyIdentifier = Visit(context.memberIdentifier());
            if (propertyIdentifier is IdentifierNameSyntax memberIdentifierName)
            {
                propertyIdentifier = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(memberIdentifierName.Identifier.Text));
            }
            // Keysharp.Scripting.Script.Vars[expression] should extract expression
            else if (propertyIdentifier is ElementAccessExpressionSyntax memberElementAccess)
            {
                propertyIdentifier = memberElementAccess.ArgumentList.Arguments.FirstOrDefault().Expression;
            }
            else
                throw new Error("Invalid property name expression identifier");

            var propertyValue = (ExpressionSyntax)Visit(context.expression());

            // Return an initializer combining the property name and value
            return SyntaxFactory.InitializerExpression(
                SyntaxKind.ComplexElementInitializerExpression,
                SyntaxFactory.SeparatedList(new[] { (ExpressionSyntax)propertyIdentifier, propertyValue })
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
                (StatementSyntax)Visit(context.flowBlock())
            );

            var elseProduction = (BlockSyntax)Visit(context.elseProduction());
            if (elseProduction != null)
                ifStatement = ifStatement.WithElse(SyntaxFactory.ElseClause(elseProduction));

            return ifStatement;
        }

        public override SyntaxNode VisitReturnStatement([NotNull] ReturnStatementContext context)
        {
            ExpressionSyntax returnExpression;

            if (context.expression() != null)
            {
                returnExpression = (ExpressionSyntax)Visit(context.expression());
            }
            else
            {
                if (parser.currentFunc.Void)
                    return SyntaxFactory.ReturnStatement();

                returnExpression = SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal("")
                );
            }

            if (parser.currentFunc.Void)
                return SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        EnsureValidStatementExpression(returnExpression)
                    ),
                    SyntaxFactory.ReturnStatement()
                );

            return SyntaxFactory.ReturnStatement(returnExpression);
        }

        public override SyntaxNode VisitThrowStatement([NotNull] ThrowStatementContext context)
        {
            if (context.singleExpression() == null)
                return SyntaxFactory.ThrowStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.IdentifierName("Error")
                    )
                );

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
            else
                expression = SyntaxFactory.ParenthesizedExpression(expression);

            // Otherwise, return a normal throw statement
            return SyntaxFactory.ThrowStatement(SyntaxFactory.CastExpression(SyntaxFactory.ParseTypeName("KeysharpException"), expression));
        }

        private SyntaxNode HandleTernaryCondition(ExpressionSyntax condition, ExpressionSyntax trueExpression, ExpressionSyntax falseExpression)
        {
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
        public override SyntaxNode VisitTernaryExpression([NotNull] TernaryExpressionContext context)
        {
            return HandleTernaryCondition((ExpressionSyntax)Visit(context.ternCond), (ExpressionSyntax)Visit(context.ternTrue), (ExpressionSyntax)Visit(context.ternFalse));
        }
        public override SyntaxNode VisitTernaryExpressionDuplicate([NotNull] TernaryExpressionDuplicateContext context)
        {
            return HandleTernaryCondition((ExpressionSyntax)Visit(context.ternCond), (ExpressionSyntax)Visit(context.ternTrue), (ExpressionSyntax)Visit(context.ternFalse));
        }

        public override SyntaxNode VisitFormalParameterArg([Antlr4.Runtime.Misc.NotNull] FormalParameterArgContext context)
        {
            // Treat as a regular parameter
            var parameterName = ToValidIdentifier(context.identifier().GetText().Trim().ToLowerInvariant());

            TypeSyntax parameterType;
            if (context.BitAnd() == null)
                parameterType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
            else
            {
                parser.currentFunc.VarRefs.Add(parameterName);
                parameterType = SyntaxFactory.ParseTypeName("VarRef");
            }

            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                .WithType(parameterType);

            // Handle default value assignment (:=) or optional parameter (QuestionMark)
            if (context.expression() != null)
            {
                var defaultValue = (ExpressionSyntax)Visit(context.expression());

                // Add [Optional] and [DefaultParameterValue(defaultValue)] attributes
                parameter = parser.AddOptionalParamValue(parameter, defaultValue);
            }
            // Handle optional parameter
            else if (context.QuestionMark() != null)
            {
                // If QuestionMark is present, mark the parameter as optional with null default value
                parameter = parser.AddOptionalParamValue(parameter, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
            }

            return parameter;
        }

        public override SyntaxNode VisitLastFormalParameterArg([NotNull] LastFormalParameterArgContext context)
        {
            ParameterSyntax parameter;
            if (context.Multiply() != null)
            {
                var parameterName = ToValidIdentifier(context.identifier()?.GetText().ToLowerInvariant().Trim() ?? "args");

                // Handle 'Multiply' for variadic arguments (params object[])
                parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(PredefinedTypes.ObjectArray)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));
            }
            else if (context.formalParameterArg() != null)
            {
                parameter = (ParameterSyntax)VisitFormalParameterArg(context.formalParameterArg());
            }
            else
                throw new Error("Unknown last formal parameter type");
            return parameter;
        }

        public override SyntaxNode VisitAssignable([Antlr4.Runtime.Misc.NotNull] AssignableContext context)
        {
            //Console.WriteLine("Assignable: " + context.GetText());
            //Console.WriteLine(context.children[0].GetText());
            if (parser.currentFunc.Scope == eScope.Static)
                return SyntaxFactory.IdentifierName(parser.CreateStaticIdentifier(context.GetText()));
            return base.VisitAssignable(context);
        }

        public override SyntaxNode VisitInitializer([Antlr4.Runtime.Misc.NotNull] InitializerContext context)
        {
            //Console.WriteLine("Initializer: " + context.GetText());
            return base.VisitInitializer(context);
        }

        public override SyntaxNode VisitFunctionStatement([Antlr4.Runtime.Misc.NotNull] FunctionStatementContext context)
        {
            // Visit the singleExpression (the method to be called)
            ExpressionSyntax targetExpression = (ExpressionSyntax)Visit(context.primaryExpression());

            if (!(targetExpression is IdentifierNameSyntax || targetExpression is IdentifierNameSyntax
                || targetExpression is MemberAccessExpressionSyntax 
                || (targetExpression is InvocationExpressionSyntax ies && 
                    ((ies.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text.Equals("GetPropertyValue", StringComparison.OrdinalIgnoreCase))
                    || ies.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text.Equals("GetPropertyValue", StringComparison.OrdinalIgnoreCase)
                    ))))
                return SyntaxFactory.ExpressionStatement(EnsureValidStatementExpression(targetExpression));

            string methodName = ExtractMethodName(targetExpression);

            // Get the argument list
            ArgumentListSyntax argumentList;
            if (context.arguments() != null)
                argumentList = (ArgumentListSyntax)VisitArguments(context.arguments());
            else
                argumentList = SyntaxFactory.ArgumentList();

            return SyntaxFactory.ExpressionStatement(parser.GenerateFunctionInvocation(targetExpression, argumentList, methodName));
        }

        private void PushFunction(string funcName, TypeSyntax returnType = null)
        {
            parser.FunctionStack.Push((parser.currentFunc, parser.UserFuncs));

            // Create shallow copy
            parser.UserFuncs = new HashSet<string>(parser.UserFuncs, parser.UserFuncs.Comparer);

            parser.functionDepth++;

            parser.currentFunc = new Function(funcName, returnType);
        }

        private void PopFunction()
        {
            (parser.currentFunc, parser.UserFuncs) = parser.FunctionStack.Pop();
            parser.functionDepth--;
        }

        public override SyntaxNode VisitFunctionHead([NotNull] FunctionHeadContext context)
        {
            PushFunction(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(context.identifierName().GetText()));

            VisitFunctionHeadPrefix(context.functionHeadPrefix());

            parser.currentFunc.Params.AddRange(((ParameterListSyntax)VisitFormalParameterList(context.formalParameterList())).Parameters);

            return null;
        }

        public override SyntaxNode VisitFunctionHeadPrefix([NotNull] FunctionHeadPrefixContext context)
        {
            parser.currentFunc.Async = context?.Async() != null && context.Async().Length > 0;
            parser.currentFunc.Public = parser.functionDepth < 2;
            parser.currentFunc.Static = !(parser.functionDepth > 1 && (context?.Static() == null || context.Static().Length == 0));
            parser.currentFunc.UserStatic = context?.Static() != null;

			return null;
        }

        public override SyntaxNode VisitFunctionExpressionHead([NotNull] FunctionExpressionHeadContext context)
        {
            if (context.functionHead() != null)
                return Visit(context.functionHead());

            PushFunction(Keywords.AnonymousLambdaPrefix + ++parser.lambdaCount);

            VisitFunctionHeadPrefix(context.functionHeadPrefix());

            parser.currentFunc.Params.AddRange(((ParameterListSyntax)VisitFormalParameterList(context.formalParameterList())).Parameters);

            return null;
        }

        public override SyntaxNode VisitFatArrowExpressionHead([NotNull] FatArrowExpressionHeadContext context)
        {
            if (context.functionExpressionHead() != null)
                return Visit(context.functionExpressionHead());

            PushFunction(Keywords.AnonymousFatArrowLambdaPrefix + ++parser.lambdaCount);
            VisitFunctionHeadPrefix(context.functionHeadPrefix());

            var parameterName = ToValidIdentifier(context.identifierName()?.GetText().ToLowerInvariant().Trim() ?? "args");
            ParameterSyntax parameter;
            if (context.Multiply() != null)
            {
                // Handle 'Multiply' for variadic arguments (params object[])
                parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(PredefinedTypes.ObjectArray)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));
            }
            else
            {
                TypeSyntax parameterType;
                if (context.BitAnd() != null)
                {
                    parser.currentFunc.VarRefs.Add(parameterName);
                    parameterType = SyntaxFactory.ParseTypeName("VarRef");
                }
                else
                    parameterType = PredefinedTypes.Object;

                parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(parameterType);

                if (context.QuestionMark() != null)
                    parameter = parser.AddOptionalParamValue(parameter, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
            }

            parser.currentFunc.Params.Add(parameter);

            return null;
        }

        public SyntaxNode FunctionExpressionCommon(MethodDeclarationSyntax methodDeclaration, ParserRuleContext context)
        {
            // Case 1: If we are inside the auto-execute section and this is the only expression in the expression sequence
            // then consider it a top-level function. The method declaration will be added to the main
            // class in VisitExpressionSequence.
            if (parser.currentFunc.Name == Keywords.AutoExecSectionName &&
                context.Parent is ExpressionSequenceContext esc && esc.Parent is ExpressionStatementContext && esc.ChildCount == 1)
            {
                return methodDeclaration;
            }
            // Case 2: If we are in the main class (not inside a class declaration)
            // OR we are inside any method declaration besides the auto-execute one then add it as a closure.
            // Function expressions inside the auto-execute section are added as static nested functions.
            if (parser.currentClass.Name == Keywords.MainClassName || parser.currentFunc.Name != Keywords.AutoExecSectionName)
            {
                var methodName = methodDeclaration.Identifier.Text;
                var variableName = methodName.ToLowerInvariant();

                var modifiers = methodDeclaration.Modifiers;

                // Function expressions in AutoExec section should be static
                if (parser.currentFunc.Name == Keywords.AutoExecSectionName)
                {
                    // Remove 'public' from the existing modifiers
                    var updatedModifiers = modifiers
                        .Where(m => !m.IsKind(SyntaxKind.PublicKeyword))
                        .ToList();

                    // Ensure 'static' is included if not already present
                    if (!updatedModifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                        updatedModifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

                    modifiers = SyntaxFactory.TokenList(updatedModifiers);
                }

                // Create a delegate or closure and add it to the current function's body
                var delegateSyntax = SyntaxFactory.LocalFunctionStatement(
                        methodDeclaration.ReturnType,
                        methodDeclaration.Identifier)
                    .WithParameterList(methodDeclaration.ParameterList)
                    .WithBody(methodDeclaration.Body)
                    .WithModifiers(modifiers);

                var isStatic = modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

				InvocationExpressionSyntax funcObj = CreateFuncObj(
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.IdentifierName("Delegate"),
                        SyntaxFactory.IdentifierName(methodName)
                    ),
                    !isStatic
                );

                parser.currentFunc.Body.Add(delegateSyntax);

                // If we are creating a closure in the auto-execute section then add a global
                // variable for the delegate and assign it's value at the beginning of AutoExecSection
                if (parser.currentFunc.Name == Keywords.AutoExecSectionName)
                {
                    parser.MaybeAddGlobalVariableDeclaration(variableName, true);
                    parser.currentFunc.Body.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(variableName), // Target variable
                            funcObj // Value to assign
                        )
                    ));
                }
                else if (isStatic)
                {
					ExpressionSyntax initializerValue = CreateFuncObj(
	                    SyntaxFactory.CastExpression(
		                    SyntaxFactory.IdentifierName("Delegate"),
		                    SyntaxFactory.IdentifierName(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(methodName))
	                    )
                    );

                    variableName = parser.CreateStaticIdentifier(variableName);

					parser.currentFunc.Body.Add(parser.CreateStaticVariableInitializer(
						SyntaxFactory.IdentifierName(variableName),
						initializerValue));
				}
                else
                {
                    var nullVariableDeclaration = SyntaxFactory.LocalDeclarationStatement(
                        CreateNullObjectVariable(variableName)
                    );

                    // Add the variable declaration to the beginning of the current function body
                    parser.currentFunc.Locals[variableName] = nullVariableDeclaration;

                    // Add the assignment statement to the `statements` list
                    parser.currentFunc.Body.Add(SyntaxFactory.ExpressionStatement(
                        CreateSimpleAssignment(variableName, funcObj)
                    ));
                }

                // Return the FuncObj variable wrapping the delegate
                return SyntaxFactory.IdentifierName(variableName).WithAdditionalAnnotations(new SyntaxAnnotation("FunctionDeclaration"));
            }
            else

            // Case 3: If inside a class declaration and not inside a method, for example
            // a class field is being assigned a fat arrow function
            {
                // Transform the method into an anonymous lambda function
                var isAsync = methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword);

                // Wrap the method body in a lambda
                var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression()
                    .WithAsyncKeyword(isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default)
                    .WithParameterList(methodDeclaration.ParameterList)
                    .WithBlock(methodDeclaration.Body);

                // Return the Func invocation
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("Func"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(lambdaExpression))
                    )
                );
            }
        }

        public override SyntaxNode VisitFunctionDeclaration([NotNull] FunctionDeclarationContext context)
        {
            Visit(context.functionHead());

            BlockSyntax functionBody = (BlockSyntax)VisitFunctionBody(context.functionBody());
            parser.currentFunc.Body.AddRange(functionBody.Statements.ToArray());

            /*
            if (parser.functionDepth > 1)
            {
                var block = SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        CreateFuncObjDelegateVariable(parser.currentFunc.Name)
                    ),
                    SyntaxFactory.LocalFunctionStatement(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Assuming return type is void
                        SyntaxFactory.Identifier(parser.currentFunc.Name) // Function name
                    )
                    .WithParameterList(parser.currentFunc.Params)
                    .WithBody(parser.currentFunc.Body)
                    .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                    )).WithAdditionalAnnotations(new SyntaxAnnotation("MergeStart"))
                );
                PopFunction();
                return block;
            }
            */

            var methodDeclaration = parser.currentFunc.Assemble();
            PopFunction();

            return methodDeclaration;
        }

        public override SyntaxNode VisitFunctionExpression([NotNull] FunctionExpressionContext context)
        {
            Visit(context.functionExpressionHead());

            HandleScopeFunctions(context);

            VisitVariableStatements(context.block());

            BlockSyntax functionBody = (BlockSyntax)Visit(context.block());
            parser.currentFunc.Body.AddRange(functionBody.Statements.ToArray());

            var methodDeclaration = parser.currentFunc.Assemble();
            PopFunction();

            return FunctionExpressionCommon(methodDeclaration, context);
        }

        public override SyntaxNode VisitFatArrowExpression([Antlr4.Runtime.Misc.NotNull] FatArrowExpressionContext context)
        {
            Visit(context.fatArrowExpressionHead());

            var returnExpression = (ExpressionSyntax)Visit(context.expression());

            BlockSyntax functionBody;

            if (parser.currentFunc.Void)
            {
                functionBody = SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(EnsureValidStatementExpression(returnExpression)),
                    SyntaxFactory.ReturnStatement()
                 );
                
            } else
                functionBody = SyntaxFactory.Block(SyntaxFactory.ReturnStatement(returnExpression));

            parser.currentFunc.Body.AddRange(functionBody.Statements.ToArray());

            var methodDeclaration = parser.currentFunc.Assemble();
            PopFunction();

            return FunctionExpressionCommon(methodDeclaration, context);
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

            if (parser.currentClass.Name != Keywords.MainClassName && parser.FunctionStack.Count == 1 && !parser.currentClass.isInitialization)
            {
                parameters.Add(Parser.ThisParam);
            }

            if (context != null)
            {
                foreach (var formalParameter in context.formalParameterArg())
                {
                    var parameter = (ParameterSyntax)VisitFormalParameterArg(formalParameter);

                    // Add the parameter to the list
                    parameters.Add(parameter);
                }
            }

            // Handle the last formal parameter argument if it exists
            if (context?.lastFormalParameterArg() != null)
            {
                var parameter = (ParameterSyntax)VisitLastFormalParameterArg(context.lastFormalParameterArg());
                if (context.lastFormalParameterArg().Multiply() != null)
                {
                    var identifier = parameter.Identifier.Text;
                    var substitute = Keywords.InternalPrefix + identifier.TrimStart('@');
                    parameter = parameter.WithIdentifier(SyntaxFactory.Identifier(substitute));

                    var statement = SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
                    )
                    .WithVariables(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(identifier))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(
                                        SyntaxFactory.IdentifierName("Array")
                                    )
                                    .WithArgumentList(
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.IdentifierName(substitute)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );

                    parser.currentFunc.Body.Add(statement);
                }
                parameters.Add(parameter);
            }

            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
        }

        public void HandleScopeFunctions(ParserRuleContext context)
        {
			var scopeFunctionDeclarations = GetScopeFunctions(context);

			foreach (var fi in scopeFunctionDeclarations)
			{
				if (parser.currentFunc.Name != Keywords.AutoExecSectionName)
				{
					if (fi.Static)
					{
						var staticName = parser.CreateStaticIdentifier(fi.Name);
						// Declare the variable in the containing class
						parser.currentFunc.Statics.Add(staticName);
						var prevScope = parser.currentFunc.Scope;
						parser.currentFunc.Scope = eScope.Static;
						parser.AddVariableDeclaration(staticName);
						parser.currentFunc.Scope = prevScope;
					}
					else
					{
						var variableName = fi.Name.ToLowerInvariant();
						var nullVariableDeclaration = SyntaxFactory.LocalDeclarationStatement(
							CreateNullObjectVariable(variableName)
						);
						// Add the variable declaration to the beginning of the current function body
						parser.currentFunc.Locals[variableName] = nullVariableDeclaration;
					}
				}
				else
					parser.UserFuncs.Add(fi.Name);
			}
		}

        public override SyntaxNode VisitFunctionBody([NotNull] FunctionBodyContext context)
        {
            HandleScopeFunctions(context);

			VisitVariableStatements(context);
            if (context.expression() != null)
            {
                var expression = (ExpressionSyntax)Visit(context.expression());
                if (parser.currentFunc.Void)
                    return SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(EnsureValidStatementExpression(expression)),
                        SyntaxFactory.ReturnStatement()
                    );
                return SyntaxFactory.Block(SyntaxFactory.ReturnStatement(expression));
            }
            else if (context.statementList() == null || context.statementList().ChildCount == 0)
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

        private void VisitVariableStatements(ParserRuleContext context)
        {
            foreach (var child in context.children)
            {
                switch (child)
                {
                    case VariableStatementContext varStmt:
                        var prevScope = parser.currentFunc.Scope;

                        switch (varStmt.GetChild(0).GetText().ToLower())
                        {
                            case "local":
                                parser.currentFunc.Scope = eScope.Local;
                                break;
                            case "global":
                                parser.currentFunc.Scope = eScope.Global;
                                break;
                            case "static":
                                parser.currentFunc.Scope = eScope.Static;
                                break;
                        }

                        if (varStmt.variableDeclarationList() != null && varStmt.variableDeclarationList().ChildCount > 0)
                        {
                            foreach (var varDecl in varStmt.variableDeclarationList().variableDeclaration())
                            {
                                SyntaxNode node = Visit(varDecl.assignable());
                                if (!(node is IdentifierNameSyntax))
                                {
                                    throw new Error();
                                }
                                string name = ((IdentifierNameSyntax)node).Identifier.Text;

                                if (parser.currentFunc.Scope == eScope.Static)
                                {
                                    parser.currentFunc.Statics.Add(name);
                                }

                                if (parser.currentFunc.Scope == eScope.Global)
                                {
                                    parser.MaybeAddVariableDeclaration(name);
                                    parser.currentFunc.Locals.Remove(name);
                                    parser.currentFunc.Globals.Add(name);
                                }
                                else
                                {
                                    parser.AddVariableDeclaration(name);
                                    if (parser.currentFunc.Scope == eScope.Local)
                                    {
                                        parser.currentFunc.Globals.Remove(name);
                                        // AddVariableDeclaration added the Locals entry
                                    }
                                }
                            }

                            parser.currentFunc.Scope = prevScope;
                        }

                        VisitVariableStatement(varStmt); // Collect variable statement
                        break;

                    case AssignmentExpressionContext assignStmt:
                        if (assignStmt.left is IdentifierExpressionContext iec && Visit(iec) is IdentifierNameSyntax ins)
                        {
                            parser.MaybeAddVariableDeclaration(ins.Identifier.Text);
                        }
                        break;
                    case FunctionStatementContext:
                    case FunctionExpressionContext:
                    case FatArrowExpressionContext:
                        continue; // Skip nested function bodies entirely

                    // Recursively search inside blocks, loops, and conditionals
                    case BlockContext:
                    case IterationStatementContext:
                    case TryStatementContext:
                    case FlowBlockContext:
                    case StatementListContext:
                    case MainParser.StatementContext:
                    case ExpressionStatementContext:
                    case ExpressionSequenceContext:
                    case MainParser.ExpressionDummyContext:
                    case OperatorExpressionDummyContext:
                    case ParenthesizedExpressionContext:
                    case VarRefExpressionContext:
                        VisitVariableStatements((ParserRuleContext)child);
                        break;

                    default:
                        break; // Ignore other nodes
                }
            }
        }

        public override SyntaxNode VisitArrayLiteral(ArrayLiteralContext context)
        {
            // Visit the arrayElementList to get all the elements
            var elementsArgList = context.arguments() == null
                ? SyntaxFactory.ArgumentList()
                : (ArgumentListSyntax)Visit(context.arguments());
            var elementsInitializer = SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList(
                        elementsArgList.Arguments.Select(arg => arg.Expression)
                    )
                );

            // Wrap the array initializer in a call to 'new Keysharp.Core.Array(...)'
            var keysharpArrayCreation = SyntaxFactory.ObjectCreationExpression(
                CreateQualifiedName("Keysharp.Core.Array"), // Class name: Keysharp.Core.Array
                elementsArgList,
                null // No object initializers
            );

            return keysharpArrayCreation;
        }

        public override SyntaxNode VisitMapLiteral([NotNull] MapLiteralContext context)
        {
            ArgumentListSyntax argumentList = (ArgumentListSyntax)Visit(context.mapElementList());

            return SyntaxFactory.InvocationExpression(
                CreateQualifiedName("Keysharp.Core.Collections.Map"),
                argumentList
            );
        }

        public override SyntaxNode VisitMapElementList([NotNull] MapElementListContext context)
        {
            var expressions = new List<ArgumentSyntax>();

            if (context == null || context.ChildCount == 0)
            {
                return SyntaxFactory.ArgumentList();
            }
            foreach (var mapElementContext in context.mapElement())
            {
                expressions.Add(SyntaxFactory.Argument((ExpressionSyntax)(Visit(mapElementContext.key))));
                expressions.Add(SyntaxFactory.Argument((ExpressionSyntax)(Visit(mapElementContext.value))));
            }

            // Wrap the expressions in an InitializerExpressionSyntax
            return SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(expressions)
            );
        }
    }
}