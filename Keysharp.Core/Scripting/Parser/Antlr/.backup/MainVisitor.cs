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
using static Keysharp.Core.Misc;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;
using System.Reflection;

namespace Keysharp.Core.Scripting.Parser.Antlr
{
    public partial class MainVisitor : MainParserBaseVisitor<SyntaxNode>
    {
        public override SyntaxNode VisitProgram([NotNull] ProgramContext context)
        {
            var t = Keysharp.Core.Common.Invoke.Reflections.stringToTypes["Map"];

            // Add CompilationUnit usings 
            var usingSyntaxTree = CSharpSyntaxTree.ParseText(CompilerHelper.UsingStr);
            var root = usingSyntaxTree.GetRoot() as CompilationUnitSyntax;
            var usingDirectives = root?.Usings ?? new SyntaxList<UsingDirectiveSyntax>();

            state.AddAssembly("Keysharp.Scripting.AssemblyBuildVersionAttribute", Accessors.A_AhkVersion);

            state.compilationUnit = SyntaxFactory.CompilationUnit()
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
                usings.Add(CreateUsingDirective(import));
            }

            state.namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(CreateQualifiedName("Keysharp.CompiledMain"))
                .AddUsings(usings.ToArray());

            state.currentClass = new Class("program", null);
            state.mainClass = state.currentClass;

            var mainFunc = new Helper.Function("Main", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            var mainFuncParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                .WithType(Helper.Types.StringArray);

            var staThreadAttribute = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("System.STAThreadAttribute"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList());

            var mainMethodSeparatedAttributeList = new SeparatedSyntaxList<AttributeSyntax>();
            mainMethodSeparatedAttributeList = mainMethodSeparatedAttributeList.Add(staThreadAttribute);
            var mainMethodAttributeList = SyntaxFactory.AttributeList(mainMethodSeparatedAttributeList);

            mainFunc.Params.Add(mainFuncParam);

            string mainBodyCode = @"
			{
					try
					{
						string name = SCRIPT_NAME_PLACEHOLDER;
						Keysharp.Scripting.Script.Variables.InitGlobalVars();
						Keysharp.Scripting.Script.SetName(name);
						if (Keysharp.Scripting.Script.HandleSingleInstance(name, eScriptInstance.SINGLEINSTANCE_PLACEHOLDER))
						{
							return 0;
						}
						Keysharp.Core.Env.HandleCommandLineParams(args);
						Keysharp.Scripting.Script.CreateTrayMenu();
						Keysharp.Scripting.Script.RunMainWindow(name, _ks_UserMainCode, false);
						Keysharp.Scripting.Script.WaitThreads();
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
					}
				return Environment.ExitCode;
			}
		";
            mainBodyCode = mainBodyCode.ReplaceFirst("SCRIPT_NAME_PLACEHOLDER", "@\"" + Path.GetFullPath(state.fileName) + "\"");
            mainBodyCode = mainBodyCode.ReplaceFirst("SINGLEINSTANCE_PLACEHOLDER", System.Enum.GetName(typeof(eScriptInstance), state.reader.SingleInstance));

            var mainBodyBlock = SyntaxFactory.ParseStatement(mainBodyCode) as BlockSyntax;
            mainFunc.Body = mainBodyBlock.Statements.ToList();

            state.autoExecFunc = new Helper.Function("_ks_UserMainCode", SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)));
            state.currentFunc = state.autoExecFunc;
            state.autoExecFunc.Scope = Scope.Global;
            state.autoExecFunc.Method = state.autoExecFunc.Method
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            var allClassDeclarations = GetClassDeclarationsRecursive(context);
            foreach (var classDeclaration in allClassDeclarations)
            {
                var className = classDeclaration.identifier().GetText();
                var classBase = classDeclaration.classTail().identifier(0)?.GetText() ?? "KeysharpObject";
                state.UserTypes[className] = classBase;
                state.AllTypes[className] = classBase;
                while (Reflections.stringToTypes.TryGetValue(classBase, out Type baseType))
                {
                    classBase = state.AllTypes[baseType.Name] = baseType.BaseType.Name;
                }
            }

            var scopeFunctionDeclarations = GetScopeFunctions(context);
            foreach (var scopeFunctionDeclaration in scopeFunctionDeclarations)
            {
                var funcName = scopeFunctionDeclaration.identifier().GetText();
                state.UserFuncs.Add(funcName);
                state.AddGlobalFuncObjVariable(funcName.ToLowerInvariant());
            }

            if (context.sourceElements() != null)
                VisitSourceElements(context.sourceElements());

            // Merge positional directives, hotkeys, hotstrings to the beginning of the auto-execute section
            state.autoExecFunc.Body = 
                state.generalDirectives.Concat(state.DHHR)
                .Concat(state.autoExecFunc.Body)
                .Append(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                CreateQualifiedName("Keysharp.Core.Common.Keyboard.HotkeyDefinition"),
                                SyntaxFactory.IdentifierName("ManifestAllHotkeysHotstringsHooks")
                            )
                        )
                    )
                ).ToList();

            if (!state.isPersistent)
            {
                state.autoExecFunc.Body.Add(
                    SyntaxFactory.ExpressionStatement(((InvocationExpressionSyntax)InternalMethods.ExitApp)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.Argument(Helper.NumericLiteralExpression("0")) })))));
            }
            state.autoExecFunc.Body.Add(SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(""))));
            state.autoExecFunc.Method = state.autoExecFunc.Assemble();

            mainFunc.Method = mainFunc.Assemble().AddAttributeLists(mainMethodAttributeList);
            state.mainClass.Declaration = state.mainClass.Declaration.AddMembers(mainFunc.Method, state.autoExecFunc.Method).AddMembers(state.mainClass.Body.ToArray());
            state.namespaceDeclaration = state.namespaceDeclaration.AddMembers(state.mainClass.Declaration);

            var attributeList = SyntaxFactory.AttributeList(state.assemblies)
                .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword)));

            state.compilationUnit = state.compilationUnit
                .AddMembers(state.namespaceDeclaration)
                .AddAttributeLists(attributeList)
                .NormalizeWhitespace();

            return state.compilationUnit;
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
                {
                    switch (ruleNode.RuleContext.RuleIndex)
                    {
                        case RULE_generalDirective:
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

            state.autoExecFunc.Body = state.autoExecFunc.Body.Concat(autoExecStatements).ToList();
            state.mainClass.Declaration = state.mainClass.Declaration.AddMembers(memberList.ToArray());
            return state.mainClass.Declaration;
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
                else if (visited is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    // In case a class is declared inside a function (such as some unit tests)
                    state.mainClass.Declaration = state.mainClass.Declaration.AddMembers(classDeclarationSyntax);
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
                var prevLabel = state.loopLabel;
                state.loopDepth++;
                // May label an iterationStatement
                LabeledStatementSyntax label = null;
                if (context.labelledStatement() != null)
                {
                    state.loopLabel = context.labelledStatement().Identifier().GetText();
                    label = (LabeledStatementSyntax)VisitLabelledStatement(context.labelledStatement());
                }
                else
                    state.loopLabel = state.loopDepth.ToString();

                BlockSyntax result = (BlockSyntax)Visit(context.iterationStatement());
                if (label != null)
                    result = result.WithStatements(result.Statements.Insert(0, label));
                state.loopDepth--;
                state.loopLabel = prevLabel;
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

            state.MaybeAddGlobalFuncObjVariable(text, false);

            // Handle special built-ins
            if (state.IsVarDeclaredGlobally(text) == null)
            {
                switch (text.ToLowerInvariant())
                {
                    case "a_linenumber":
                        var contextLineNumber = context.Start.Line;
                        var realLineNumber = state.codeLines[contextLineNumber - 1].LineNumber;
                        return SyntaxFactory.CastExpression(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(realLineNumber))
                        );
                    case "a_linefile":
                        return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(state.codeLines[context.Start.Line].FileName));

                    case "a_scriptfullpath":
                        state.mainClass.Declaration = state.mainClass.Declaration.AddMembers(CreatePublicConstant("A_ScriptFullPath", typeof(string), Path.GetFullPath(state.fileName)));
                        break;
                }
            }

            return HandleIdentifierName(text);
        }

        private SyntaxNode HandleIdentifierName(string text)
        {
            text = NormalizeFunctionIdentifier(text);

            if (state.currentFunc.VarRefs.Contains(text))
            {
                var debug = state.currentFunc;
                // If it's a VarRef, access the __Value member
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ParenthesizedExpression(
                        SyntaxFactory.CastExpression(
                            SyntaxFactory.IdentifierName("VarRef"),
                            SyntaxFactory.IdentifierName(state.currentFunc.VarRefs.First(item => string.Equals(item, text, StringComparison.InvariantCultureIgnoreCase))))), // Identifier name
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
            var targetExpression = Visit(context.singleExpression());

            // Handle the different cases of singleExpression
            if (targetExpression is IdentifierNameSyntax identifierName)
            {
                // Case: Variable identifier
                state.MaybeAddVariableDeclaration(identifierName.Identifier.Text);
                return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("VarRef"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                    // Getter lambda: () => identifier
                    SyntaxFactory.Argument(
                        SyntaxFactory.ParenthesizedLambdaExpression(
                            SyntaxFactory.ParameterList(),
                            identifierName
                        )
                    ),
                    // Setter lambda: value => identifier = value
                    SyntaxFactory.Argument(
                        SyntaxFactory.ParenthesizedLambdaExpression(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                )
                            ),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                identifierName,
                                SyntaxFactory.IdentifierName("value")
                            )
                        )
                    )
                        })
                    ),
                    null
                );
            }
            else if (targetExpression is MemberAccessExpressionSyntax memberAccess)
            {
                // Case: MemberDotExpression
                return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("VarRef"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                    // Getter lambda: () => obj.property
                    SyntaxFactory.Argument(
                        SyntaxFactory.ParenthesizedLambdaExpression(
                            SyntaxFactory.ParameterList(),
                            memberAccess
                        )
                    ),
                    // Setter lambda: value => obj.property = value
                    SyntaxFactory.Argument(
                        SyntaxFactory.ParenthesizedLambdaExpression(
                            SyntaxFactory.ParameterList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                )
                            ),
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                memberAccess,
                                SyntaxFactory.IdentifierName("value")
                            )
                        )
                    )
                        })
                    ),
                    null
                );
            }
            else if (targetExpression is ElementAccessExpressionSyntax elementAccess)
            {
                // Case: MemberIndexExpression
                var baseExpression = elementAccess.Expression;
                var indexExpression = elementAccess.ArgumentList.Arguments.First().Expression;

                return SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("VarRef"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            // Getter lambda: () => obj[index]
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression(
                                    SyntaxFactory.ParameterList(),
                                    SyntaxFactory.ElementAccessExpression(baseExpression)
                                        .WithArgumentList(
                                            SyntaxFactory.BracketedArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(indexExpression)
                                                )
                                            )
                                        )
                                )
                            ),
                            // Setter lambda: value => obj[index] = value
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression(
                                    SyntaxFactory.ParameterList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                        )
                                    ),
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.ElementAccessExpression(baseExpression)
                                            .WithArgumentList(
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(indexExpression)
                                                    )
                                                )
                                            ),
                                        SyntaxFactory.IdentifierName("value")
                                    )
                                )
                            )
                        })
                    ),
                    null
                );
            }
            else if (targetExpression is InvocationExpressionSyntax invocationExpression)
            {
                // Handle Keysharp.Scripting.Script.Index(varname, index)
                if (invocationExpression.Expression is MemberAccessExpressionSyntax invocationMemberAccess &&
                    invocationMemberAccess.Name.Identifier.Text == "Index" &&
                    invocationExpression.ArgumentList.Arguments.Count == 2)
                {
                    var varName = invocationExpression.ArgumentList.Arguments[0].Expression;
                    var index = invocationExpression.ArgumentList.Arguments[1].Expression;

                    return SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName("VarRef"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                        // Getter lambda: () => Keysharp.Scripting.Script.Index(varname, index)
                        SyntaxFactory.Argument(
                            SyntaxFactory.ParenthesizedLambdaExpression(
                                SyntaxFactory.ParameterList(),
                                ((InvocationExpressionSyntax)InternalMethods.Index)
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(new[]
                                        {
                                            SyntaxFactory.Argument(varName),
                                            SyntaxFactory.Argument(index)
                                        })
                                    )
                                )
                            )
                        ),
                        // Setter lambda: value => Keysharp.Scripting.Script.SetObject(value, varname, index)
                        SyntaxFactory.Argument(
                            SyntaxFactory.ParenthesizedLambdaExpression(
                                SyntaxFactory.ParameterList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                    )
                                ),
                                ((InvocationExpressionSyntax)InternalMethods.SetObject)
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(new[]
                                        {
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")),
                                            SyntaxFactory.Argument(varName),
                                            SyntaxFactory.Argument(index)
                                        })
                                    )
                                )
                            )
                        )
                            })
                        ),
                        null
                    );
                }
                // Handle Keysharp.Scripting.Script.GetPropertyValue(obj, field)
                else if (invocationExpression.Expression is MemberAccessExpressionSyntax getPropertyAccess &&
                         getPropertyAccess.Name.Identifier.Text == "GetPropertyValue" &&
                         invocationExpression.ArgumentList.Arguments.Count == 2)
                {
                    var obj = invocationExpression.ArgumentList.Arguments[0].Expression;
                    var field = invocationExpression.ArgumentList.Arguments[1].Expression;

                    return SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName("VarRef"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                        // Getter lambda: () => Keysharp.Scripting.Script.GetPropertyValue(obj, field)
                        SyntaxFactory.Argument(
                            SyntaxFactory.ParenthesizedLambdaExpression(
                                SyntaxFactory.ParameterList(),
                                ((InvocationExpressionSyntax)InternalMethods.GetPropertyValue)
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(new[]
                                        {
                                            SyntaxFactory.Argument(obj),
                                            SyntaxFactory.Argument(field)
                                        })
                                    )
                                )
                            )
                        ),
                        // Setter lambda: value => Keysharp.Scripting.Script.SetPropertyValue(obj, field, value)
                        SyntaxFactory.Argument(
                            SyntaxFactory.ParenthesizedLambdaExpression(
                                SyntaxFactory.ParameterList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                                    )
                                ),
                                ((InvocationExpressionSyntax)InternalMethods.SetPropertyValue)
                                .WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(new[]
                                        {
                                            SyntaxFactory.Argument(obj),
                                            SyntaxFactory.Argument(field),
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value"))
                                        })
                                    )
                                )
                            )
                        )
                            })
                        ),
                        null
                    );
                }
                else if (invocationExpression.ArgumentList.Arguments.Count == 3 &&
                         invocationExpression.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax argumentName &&
                         argumentName.Token.Text == "\"__Value\"")
                {
                    return invocationExpression;
                }
            }

            throw new InvalidOperationException("Unsupported singleExpression type for VarRefExpression.");
        }


        public override SyntaxNode VisitDynamicIdentifierExpression([NotNull] DynamicIdentifierExpressionContext context)
        {
            return Visit(context.dynamicIdentifier());
        }

        public override SyntaxNode VisitDynamicIdentifier([NotNull] DynamicIdentifierContext context)
        {
            // In this case we have a identifier composed of identifier parts and dereference expressions
            // such as a%b%. CreateDynamicVariableAccessor will return string.Concat<object>(new object[] {"a", b), so
            // to turn it into an identifier we need to wrap it in Keysharp.Scripting.Script.Vars[]
            return SyntaxFactory.ElementAccessExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    CreateQualifiedName("Keysharp.Scripting.Script"),
                    SyntaxFactory.IdentifierName("Vars")
                ),
                SyntaxFactory.BracketedArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument((ExpressionSyntax)CreateDynamicVariableString(context))
                    )
                )
            );
        }

        public override SyntaxNode VisitDynamicPropertyName([NotNull] DynamicPropertyNameContext context)
        {
            return CreateDynamicVariableString(context);
        }

        public override SyntaxNode VisitVariableStatement([NotNull] VariableStatementContext context)
        {
            var prevScope = state.currentFunc.Scope;
            
            switch (context.GetChild(0).GetText().ToLower())
            {
                case "local":
                    state.currentFunc.Scope = Scope.Local;
                    break;
                case "global":
                    state.currentFunc.Scope = Scope.Global;
                    break;
                case "static":
                    state.currentFunc.Scope = Scope.Static;
                    break;
            }

            if (context.variableDeclarationList() != null && context.variableDeclarationList().ChildCount > 0) {
                var result = VisitVariableDeclarationList(context.variableDeclarationList());
                state.currentFunc.Scope = prevScope;
                return result;
            }

            if (prevScope == Scope.Global && prevScope != state.currentFunc.Scope)
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

            if (state.currentFunc.Scope == Scope.Static)
            {
                state.currentFunc.Statics.Add(identifier.Identifier.Text);
                var normalizedName = NormalizeFunctionIdentifier(identifier.Identifier.Text);
                state.currentFunc.Statics.Add(normalizedName);
                identifier = SyntaxFactory.IdentifierName(normalizedName);
            }

            var name = identifier.Identifier.Text;

            state.MaybeAddVariableDeclaration(name);

            if (state.currentFunc.Scope == Scope.Global)
            {
                state.currentFunc.Locals.Remove(name);
                state.currentFunc.Globals.Add(name);
                if (context.assignmentOperator() != null)
                {
                    var initializerValue = (ExpressionSyntax)Visit(context.singleExpression());

                    return SyntaxFactory.ExpressionStatement(HandleAssignment(
                        identifier,
                        initializerValue,
                        context.assignmentOperator().GetText()));
                }
            } else if (state.currentFunc.Scope == Scope.Local) {
                state.currentFunc.Globals.Remove(name);
                // MaybeAddVariableDeclaration added the Locals entry
            }

            // Check if there is an initializer (e.g., ':= singleExpression')
            if (context.assignmentOperator() != null)
            {
                var initializerValue = (ExpressionSyntax)Visit(context.singleExpression());

                if (state.currentFunc.Scope == Scope.Static)
                {
                    var invocationExpression = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Scripting.Script"),
                            SyntaxFactory.IdentifierName("InitStaticVariable")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                // Argument: ref identifier
                                SyntaxFactory.Argument(identifier)
                                    .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.RefKeyword)),

                                // Argument: identifier as a string literal
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(state.currentClass.Name + "_" + identifier.Identifier.Text)
                                    )
                                ),

                                // Argument: () => initializerValue
                                SyntaxFactory.Argument(
                                    SyntaxFactory.ParenthesizedLambdaExpression(initializerValue)
                                )
                            })
                        )
                    );
                    return SyntaxFactory.ExpressionStatement(invocationExpression);
                }

                return SyntaxFactory.ExpressionStatement(HandleAssignment(
                    identifier,
                    initializerValue,
                    context.assignmentOperator().GetText()));
            }

            // Return null if no assignment is needed
            return null;
        }


        public override SyntaxNode VisitArguments([NotNull] ArgumentsContext context)
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
                        SyntaxFactory.IdentifierName("FlattenParam"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(arg)
                            )
                        )
                    );
                    return SyntaxFactory.Argument(flattenedArg);
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
            var propertyName = (ExpressionSyntax)Visit(context.dynamicPropertyName());
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
            // Treat as a regular parameter
            var parameterName = Helper.ToValidIdentifier(context.assignable().GetText().Trim().ToLowerInvariant());

            TypeSyntax parameterType;
            if (context.BitAnd() == null)
                parameterType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
            else
            {
                state.currentFunc.VarRefs.Add(parameterName);
                parameterType = SyntaxFactory.ParseTypeName("VarRef");
            }

            var parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                .WithType(parameterType);

            // Handle default value assignment (:=) or optional parameter (QuestionMark)
            if (context.singleExpression() != null)
            {
                var defaultValue = (ExpressionSyntax)Visit(context.singleExpression());

                // Add [Optional] and [DefaultParameterValue(defaultValue)] attributes
                parameter = state.AddOptionalParamValue(parameter, defaultValue);
            }
            // Handle optional parameter
            else if (context.QuestionMark() != null)
            {
                // If QuestionMark is present, mark the parameter as optional with null default value
                parameter = state.AddOptionalParamValue(parameter, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
            }

            return parameter;
        }

        public override SyntaxNode VisitLastFormalParameterArg([NotNull] LastFormalParameterArgContext context)
        {
            ParameterSyntax parameter;
            if (context.Multiply() != null)
            {
                var lastFormalParam = context.formalParameterArg();
                var parameterName = Helper.ToValidIdentifier(lastFormalParam?.assignable()?.GetText().ToLowerInvariant().Trim() ?? "args");

                // Handle 'Multiply' for variadic arguments (params object[])
                parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(Helper.Types.ObjectArray)
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
            return base.VisitAssignable(context);
        }

        public override SyntaxNode VisitLambdaFunctionBody([Antlr4.Runtime.Misc.NotNull] LambdaFunctionBodyContext context)
        {
            //Console.WriteLine("ArrowFunctionBody: " + context.GetText());
            return context.singleExpression() == null ? (BlockSyntax)VisitFunctionBody(context.functionBody()) : SyntaxFactory.Block(SyntaxFactory.ReturnStatement((ExpressionSyntax)Visit(context.singleExpression())));
        }
        public override SyntaxNode VisitInitializer([Antlr4.Runtime.Misc.NotNull] InitializerContext context)
        {
            //Console.WriteLine("Initializer: " + context.GetText());
            return base.VisitInitializer(context);
        }

        public override SyntaxNode VisitFunctionStatement([Antlr4.Runtime.Misc.NotNull] FunctionStatementContext context)
        {
            // Visit the singleExpression (the method to be called)
            ExpressionSyntax targetExpression = (ExpressionSyntax)Visit(context.singleExpression());

            if (!(targetExpression is IdentifierNameSyntax || targetExpression is IdentifierNameSyntax
                || targetExpression is MemberAccessExpressionSyntax))
                return SyntaxFactory.ExpressionStatement(targetExpression);

            string methodName = ExtractMethodName(targetExpression);

            // Get the argument list
            ArgumentListSyntax argumentList;
            if (context.arguments() != null)
                argumentList = (ArgumentListSyntax)VisitArguments(context.arguments());
            else
                argumentList = SyntaxFactory.ArgumentList();

            return SyntaxFactory.ExpressionStatement(state.GenerateFunctionInvocation(targetExpression, argumentList, methodName));
        }

        private void PushFunction(string funcName)
        {
            state.FunctionStack.Push((state.currentFunc, state.UserFuncs));

            // Create shallow copy
            state.UserFuncs = new HashSet<string>(state.UserFuncs, state.UserFuncs.Comparer);

            state.functionDepth++;

            state.currentFunc = new Helper.Function(funcName);
        }

        private void PopFunction()
        {
            (state.currentFunc, state.UserFuncs) = state.FunctionStack.Pop();
            state.functionDepth--;
        }

        public override SyntaxNode VisitFunctionDeclaration([NotNull] FunctionDeclarationContext context)
        {
            PushFunction(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(context.identifier().GetText()));

            var scopeFunctionDeclarations = GetScopeFunctions(context);
            foreach (var scopeFunctionDeclaration in scopeFunctionDeclarations)
                state.UserFuncs.Add(scopeFunctionDeclaration.identifier().GetText());

            if (context.formalParameterList() != null)
                state.currentFunc.Params.AddRange(((ParameterListSyntax)VisitFormalParameterList(context.formalParameterList())).Parameters);

            BlockSyntax functionBody = (BlockSyntax)VisitLambdaFunctionBody(context.lambdaFunctionBody());
            state.currentFunc.Body.AddRange(functionBody.Statements.ToArray());

            /*
            if (state.functionDepth > 1)
            {
                var block = SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        CreateFuncObjDelegateVariable(state.currentFunc.Name)
                    ),
                    SyntaxFactory.LocalFunctionStatement(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Assuming return type is void
                        SyntaxFactory.Identifier(state.currentFunc.Name) // Function name
                    )
                    .WithParameterList(state.currentFunc.Params)
                    .WithBody(state.currentFunc.Body)
                    .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                    )).WithAdditionalAnnotations(new SyntaxAnnotation("MergeStart"))
                );
                PopFunction();
                return block;
            }
            */

            var methodDeclaration = state.currentFunc.Assemble();
            PopFunction();
            return methodDeclaration;
        }

        public override SyntaxNode VisitFunctionExpression([NotNull] FunctionExpressionContext context)
        {
            return Visit(context.lambdaFunction());
        }

        public override SyntaxNode VisitNamedLambdaFunction([NotNull] NamedLambdaFunctionContext context)
        {
            // Visit the function declaration
            var methodDeclaration = (MethodDeclarationSyntax)VisitFunctionDeclaration(context.functionDeclaration());

            // Case 1: If we are not inside a class declaration OR we are inside a class
            // declaration method declaration then add it as a closure
            if (state.currentClass.Name == "program" || state.currentFunc.Name != "_ks_UserMainCode")
            {
                var methodName = methodDeclaration.Identifier.Text;
                var variableName = methodName.ToLowerInvariant();
                // Create a delegate or closure and add it to the current function's body
                var delegateSyntax = SyntaxFactory.LocalFunctionStatement(
                        methodDeclaration.ReturnType,
                        methodDeclaration.Identifier)
                    .WithParameterList(methodDeclaration.ParameterList)
                    .WithBody(methodDeclaration.Body);

                InvocationExpressionSyntax funcObj = CreateFuncObj(
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.IdentifierName("Delegate"),
                        SyntaxFactory.IdentifierName(methodName)
                    )
                );

                state.currentFunc.Body.Add(delegateSyntax);

                // If we are creating a closure in the auto-execute section then add a global
                // variable for the delegate and assign it's value at the beginning of _ks_UserMainCode
                if (state.currentFunc.Name == "_ks_UserMainCode")
                {
                    state.MaybeAddGlobalVariableDeclaration(variableName, true);
                    state.currentFunc.Body.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(variableName), // Target variable
                            funcObj // Value to assign
                        )
                    ));
                } 
                else
                {
                    var nullVariableDeclaration = SyntaxFactory.LocalDeclarationStatement(
                        CreateNullObjectVariable(methodName.ToLowerInvariant())
                    );

                    // Add the variable declaration to the beginning of the current function body
                    state.currentFunc.Locals[methodName] = nullVariableDeclaration;

                    // Add the assignment statement to the `statements` list
                    state.currentFunc.Body.Add(SyntaxFactory.ExpressionStatement(
                        CreateFuncObjAssignment(methodName.ToLowerInvariant(), methodName)
                    ));
                }

                // Return the FuncObj variable wrapping the delegate
                return SyntaxFactory.IdentifierName(variableName).WithAdditionalAnnotations(new SyntaxAnnotation("FunctionDeclaration"));
            } else

            // Case 2: If inside a class declaration and not inside a method
            {
                // Transform the method into an anonymous lambda function
                var isAsync = methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword);

                // Wrap the method body in a lambda
                var lambdaExpression = SyntaxFactory.ParenthesizedLambdaExpression()
                    .WithAsyncKeyword(isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default)
                    .WithParameterList(methodDeclaration.ParameterList)
                    .WithBlock(methodDeclaration.Body);

                // Return the FuncObj invocation
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("FuncObj"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(lambdaExpression))
                    )
                );
            }
        }

        public override SyntaxNode VisitAnonymousLambdaFunction([NotNull] AnonymousLambdaFunctionContext context)
        {
            PushFunction(Helper.Keywords.AnonymousLambdaPrefix + state.functionDepth.ToString());
            var isAsync = context.Async() != null;

            // Visit parameters
            if (context.formalParameterList() != null)
                state.currentFunc.Params.AddRange(((ParameterListSyntax)VisitFormalParameterList(context.formalParameterList())).Parameters);

            // Determine the arrow function body
            if (context.lambdaFunctionBody().functionBody() != null)
            {
                // If it's a function body, wrap it in a block
                state.currentFunc.Body.AddRange(((BlockSyntax)VisitFunctionBody(context.lambdaFunctionBody().functionBody())).Statements);
            }
            else
            {
                // If it's a single expression, directly visit it
                var lambdaExpression = (ExpressionSyntax)Visit(context.lambdaFunctionBody().singleExpression());
                state.currentFunc.Body.Add(SyntaxFactory.ReturnStatement(lambdaExpression));
            }

            var result = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("FuncObj"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression()
                                    .WithAsyncKeyword(isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default)
                                    .WithParameterList(state.currentFunc.AssembleParams())
                                    .WithBlock(state.currentFunc.AssembleBody())
                            )
                        )
                    )
                );
            PopFunction();
            return result;
        }

        public override SyntaxNode VisitAnonymousFatArrowLambdaFunction([NotNull] AnonymousFatArrowLambdaFunctionContext context)
        {
            PushFunction(Helper.Keywords.AnonymousFatArrowLambdaPrefix + state.functionDepth.ToString());
            var isAsync = context.Async() != null;

            var parameterName = Helper.ToValidIdentifier(context.assignable()?.GetText().ToLowerInvariant().Trim() ?? "args");
            ParameterSyntax parameter;
            if (context.Multiply() != null)
            {
                // Handle 'Multiply' for variadic arguments (params object[])
                parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(Helper.Types.ObjectArray)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));
            }
            else
            {
                TypeSyntax parameterType;
                if (context.BitAnd() != null)
                {
                    state.currentFunc.VarRefs.Add(parameterName);
                    parameterType = SyntaxFactory.ParseTypeName("VarRef");
                } else
                    parameterType = Helper.Types.Object;

                parameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(parameterType);

                if (context.QuestionMark() != null)
                    parameter = state.AddOptionalParamValue(parameter, SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
            }

            state.currentFunc.Params.Add(parameter);

            var lambdaExpression = (ExpressionSyntax)Visit(context.singleExpression());
            state.currentFunc.Body.Add(SyntaxFactory.ReturnStatement(lambdaExpression));

            var result = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("FuncObj"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.ParenthesizedLambdaExpression()
                                    .WithAsyncKeyword(isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default)
                                    .WithParameterList(state.currentFunc.AssembleParams())
                                    .WithBlock(state.currentFunc.AssembleBody())
                            )
                        )
                    )
                );

            PopFunction();
            return result;
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

        private BlockSyntax EnsureNoReturnStatement(BlockSyntax functionBody)
        {
            var updatedStatements = functionBody.Statements.Select(statement =>
            {
                if (statement is ReturnStatementSyntax returnStatement && returnStatement.Expression != null)
                {
                    // Replace the return statement with its expression
                    return SyntaxFactory.ExpressionStatement(returnStatement.Expression);
                }

                return statement;
            });

            return SyntaxFactory.Block(updatedStatements);
        }

        public override SyntaxNode VisitFormalParameterList([NotNull] FormalParameterListContext context)
        {
            var parameters = new List<ParameterSyntax>();

            foreach (var formalParameter in context.formalParameterArg())
            {
                var parameter = (ParameterSyntax)VisitFormalParameterArg(formalParameter);

                // Add the parameter to the list
                parameters.Add(parameter);
            }

            // Handle the last formal parameter argument if it exists
            if (context.lastFormalParameterArg() != null)
            {
                var parameter = (ParameterSyntax)VisitLastFormalParameterArg(context.lastFormalParameterArg());
                parameters.Add(parameter);
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
                            SyntaxFactory.CollectionExpression(
                            SyntaxFactory.SeparatedList<CollectionElementSyntax>(
                                elementsInitializer.Expressions.Select(expression =>
                                {
                                    return expression is CollectionExpressionSyntax spread
                                        ? spread.Elements.First()
                                        : SyntaxFactory.ExpressionElement((ExpressionSyntax)expression);
                                })
                            )
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
            if (context.children == null)
            {
                return SyntaxFactory.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>()
                );
            }
            foreach (var child in context.children)
            {
                var childText = child.GetText();
                if (child is ArrayElementContext arrayElementContext)
                {
                    // Visit non-empty array elements
                    var element = VisitArrayElement(arrayElementContext);
                    expressions.Add((ExpressionSyntax)element);
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

                // Add the spread operator `..`
                return SyntaxFactory.CollectionExpression(
                    SyntaxFactory.SeparatedList(
                        new[] { (CollectionElementSyntax)SyntaxFactory.SpreadElement(flattenInvocation) }
                    )
                );
            }

            // Return the element as is
            return element;
        }
    }
}