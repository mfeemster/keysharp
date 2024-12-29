using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static MainParser;
using static Keysharp.Core.Scripting.Parser.Antlr.MainVisitor;
using System.Linq.Expressions;
using System.Xml.Linq;
using static Keysharp.Scripting.Script;
using System.Text.RegularExpressions;
using Keysharp.Core.Windows;
using Keysharp.Scripting;
using Antlr4.Runtime;
using System.Data.Common;

namespace Keysharp.Core.Scripting.Parser.Antlr
{
    public enum Scope
    {
        Local,
        Global
    }
    internal class Helper
    {
        public static CompilationUnitSyntax compilationUnit;
        public static NamespaceDeclarationSyntax namespaceDeclaration;
        public static ClassDeclarationSyntax mainClass;
        public static Function mainFunc;
        public static Function autoExecFunc;
        public static Function currentFunc;

        public static Class currentClass;

        public static HashSet<string> globalVars = [];
        public static HashSet<string> accessibleVars = [];

        public static HashSet<string> UserTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> UserFuncs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static List<CodeLine> codeLines;
        public static string fileName;

        public static uint loopDepth = 0;
        public static uint functionDepth = 0;
        public static uint tryDepth = 0;

        public const string LoopEnumeratorBaseName = "_ks_e";

        public static NameSyntax ScriptOperateName = CreateQualifiedName("Keysharp.Scripting.Script.Operate");
        public static NameSyntax ScriptOperateUnaryName = CreateQualifiedName("Keysharp.Scripting.Script.OperateUnary");

        public class Function
        {

            public MethodDeclarationSyntax Method = null;
            public string Name = null;
            public BlockSyntax Body = SyntaxFactory.Block();
            public ParameterListSyntax Params = SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<ParameterSyntax>());
            public HashSet<string> Locals = new HashSet<string>();
            public HashSet<string> Globals = new HashSet<string>();
            public HashSet<string> Statics = new HashSet<string>();
            public Scope Scope = Scope.Local;

            public Function(string name, TypeSyntax returnType = null)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null or empty.", nameof(name));

                if (returnType == null)
                    returnType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));

                Name = name;
                Method = SyntaxFactory.MethodDeclaration(returnType, name);
            }
        }

        public class Class
        {
            public string Name = null;
            public List<MemberDeclarationSyntax> Body = new List<MemberDeclarationSyntax>();
            public ClassDeclarationSyntax Declaration = null;

            public readonly List<(string FieldName, ExpressionSyntax Initializer)> deferredInitializations = new();
            public readonly List<(string FieldName, ExpressionSyntax Initializer)> deferredStaticInitializations = new();

            public Class(string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be null or empty.", nameof(name));

                Name = name;
                Declaration = SyntaxFactory.ClassDeclaration(name)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                            SyntaxFactory.SimpleBaseType(CreateQualifiedName("KeysharpObject"))
                        )
                    )
                );
            }
        }

        public static List<ClassDeclarationContext> GetClassDeclarationsRecursive(ParserRuleContext context)
        {
            return context.children
                .OfType<ClassDeclarationContext>()
                .Concat(
                    context.children
                        .OfType<ParserRuleContext>()
                        .SelectMany(GetClassDeclarationsRecursive)
                )
                .ToList();
        }

        public static ParameterSyntax VariadicParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")) // Default name for spread argument
                        .WithType(SyntaxFactory.ArrayType(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // object[]
                            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.OmittedArraySizeExpression()
                                )
                            ))
                        ))
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));

        public static NameSyntax CreateQualifiedName(string qualifiedString)
        {
            NameSyntax qualifiedName = null;

            foreach (var identifier in qualifiedString.Split('.'))
            {
                var name = SyntaxFactory.IdentifierName(identifier);

                if (qualifiedName != null)
                {
                    qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, name);
                }
                else
                {
                    qualifiedName = name;
                }
            }

            return qualifiedName;
        }
        public static UsingDirectiveSyntax CreateUsingDirective(string usingName)
        {
            string alias = null;

            if (usingName.Contains("="))
            {
                // Handle alias import
                var parts = usingName.Split('=');
                alias = parts[0].Trim();
                usingName = parts[1].Trim();
            }

            var qualifiedName = CreateQualifiedName(usingName);

            if (alias != null)
                return SyntaxFactory.UsingDirective(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(alias)), qualifiedName);

            return SyntaxFactory.UsingDirective(qualifiedName);
        }

        public static LiteralExpressionSyntax NumericLiteralExpression(string value)
        {
            if (value.Contains("."))
            {
                double.TryParse(value, CultureInfo.InvariantCulture, out double result);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(result));
            }
            else if (value.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
                long.TryParse(value.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out long result);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(result));
            }
            else if (value.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
            {
                long.TryParse(value.Substring(2), NumberStyles.AllowBinarySpecifier, CultureInfo.InvariantCulture, out long result);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(result));
            }
            else
            {
                long.TryParse(value, out long result);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(result));
            }
        }

        public static ExpressionStatementSyntax MethodCallExpression(string name, params ExpressionSyntax[] args)
        {
            var identifier = CreateQualifiedName(name);
            var argumentList = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(args.Select(arg => SyntaxFactory.Argument(arg)))
            );
            var invocation = SyntaxFactory.InvocationExpression(identifier, argumentList);
            return SyntaxFactory.ExpressionStatement(invocation);
        }

        public static Dictionary<int, SyntaxKind> pureBinaryOperators = new Dictionary<int, SyntaxKind>()
        {
            {MainParser.Plus, SyntaxKind.AddExpression},
            {MainParser.Minus, SyntaxKind.SubtractExpression},
            {MainParser.Multiply, SyntaxKind.MultiplyExpression},
            {MainParser.Divide, SyntaxKind.DivideExpression},
            {MainParser.Modulus, SyntaxKind.ModuloExpression},
            {MainParser.LeftShiftArithmetic, SyntaxKind.LeftShiftExpression},
            {MainParser.RightShiftArithmetic, SyntaxKind.RightShiftExpression},
            {MainParser.RightShiftLogical, SyntaxKind.UnsignedRightShiftExpression},
            {MainParser.BitAnd, SyntaxKind.BitwiseAndExpression},
            {MainParser.BitOr, SyntaxKind.BitwiseOrExpression},
            {MainParser.BitXOr, SyntaxKind.ExclusiveOrExpression},
            {MainParser.LessThan, SyntaxKind.LessThanExpression},
            {MainParser.MoreThan, SyntaxKind.GreaterThanExpression},
            {MainParser.IdentityEquals, SyntaxKind.EqualsEqualsToken},
            {MainParser.NotEquals, SyntaxKind.NotEqualsExpression},
            {MainParser.And, SyntaxKind.LogicalAndExpression},
            {MainParser.VerbalAnd, SyntaxKind.LogicalAndExpression},
            {MainParser.Or, SyntaxKind.LogicalOrExpression},
            {MainParser.VerbalOr, SyntaxKind.LogicalOrExpression}
        };

        public static Dictionary<int, string> binaryOperators = new Dictionary<int, string>()
        {
            {MainParser.Plus, "Add"},
            {MainParser.Minus, "Minus"},
            {MainParser.Multiply, "Multiply"},
            {MainParser.Divide, "Divide"},
            {MainParser.IntegerDivide, "FloorDivide"},
            {MainParser.Modulus, "Modulus"},
            {MainParser.LeftShiftArithmetic, "BitShiftLeft"},
            {MainParser.RightShiftArithmetic, "BitShiftRight"},
            {MainParser.RightShiftLogical, "LogicalBitShiftRight"},
            {MainParser.BitAnd, "BitwiseAnd"},
            {MainParser.BitOr, "BitwiseOr"},
            {MainParser.BitXOr, "BitwiseXor"},
            {MainParser.LessThan, "LessThan"},
            {MainParser.LessThanEquals, "LessThanOrEqual"},
            {MainParser.MoreThan, "GreaterThan"},
            {MainParser.GreaterThanEquals, "GreaterThanOrEqual"},
            {MainParser.IdentityEquals, "IdentityEquality"},
            {MainParser.IdentityNotEquals, "IdentityInequality"},
            {MainParser.NotEquals, "ValueInequality"},
            {MainParser.Equals_, "ValueEquality"},
            {MainParser.And, "BooleanAnd"},
            {MainParser.VerbalAnd, "BooleanAnd"},
            {MainParser.Or, "BooleanOr"},
            {MainParser.VerbalOr, "BooleanOr"},
            {MainParser.Dot, "Concat"},
            {MainParser.DotConcat, "Concat"},
            {MainParser.RegExMatch, "RegEx"},
            {MainParser.Power, "Power"},
            {MainParser.Is, "Is"}
        };

        public static Dictionary<int, string> unaryOperators = new Dictionary<int, string>()
        {
            {MainParser.Minus, "Minus"},
            {MainParser.Not, "LogicalNot"},
            {MainParser.VerbalNot, "LogicalNot"},
            {MainParser.BitNot, "BitwiseNot"}
        };

        public static InvocationExpressionSyntax CreateBinaryOperatorExpression(int op, ExpressionSyntax exprL, ExpressionSyntax exprR)
        {
            return SyntaxFactory.InvocationExpression(
                ScriptOperateName,
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(CreateQualifiedName("Keysharp.Scripting.Script.Operator." + binaryOperators[op])),
                        SyntaxFactory.Argument(exprL),
                        SyntaxFactory.Argument(exprR)
                    })
                )
            );
        }

        public static void FlattenContext(IParseTree context, ref List<string> list)
        {
            if (context.ChildCount == 0)
                list.Add(((ITerminalNode)context).Symbol.Text);
            else
            {
                for (var i = 0; i < context.ChildCount; i++)
                {
                    FlattenContext(context.GetChild(i), ref list);
                }
            }
        }

        public static string IsLocalVar(string name, bool caseSense = false)
        {
            if (caseSense)
            {
                if (currentFunc.Locals.Contains(name) || currentFunc.Statics.Contains(name))
                    return name;
            }
            else
            {
                string match = currentFunc.Locals.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                    return match;
                match = currentFunc.Statics.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                    return match;
            }
            return null;
        }

        public static string IsVarDeclaredLocally(string name, bool caseSense = false)
        {
            if (currentFunc.Body == null)
                return null;

            var builtIn = IsBuiltInProperty(name, caseSense);
            if (builtIn != null) return builtIn;

            var variableDeclarations = currentFunc.Body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>();
            if (variableDeclarations != null)
            {
                foreach (var declaration in variableDeclarations)
                {
                    VariableDeclaratorSyntax match;
                    if (caseSense)
                        match = declaration.Declaration.Variables.FirstOrDefault(v => v.Identifier.Text == name);
                    else
                        match = declaration.Declaration.Variables.FirstOrDefault(v => v.Identifier.Text.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (match != null)
                        return match.Identifier.Text;
                }
            }
            return null;
        }

        public static string IsGlobalVar(string name, bool caseSense = false)
        {
            if (caseSense)
            {
                if (currentFunc.Globals.Contains(name))
                    return name;
            }
            else
            {
                string match = currentFunc.Globals.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                    return match;
            }
            return null;
        }

        public static string IsVarDeclaredGlobally(string name, bool caseSense = false)
        {
            var builtIn = IsBuiltInProperty(name, caseSense);
            if (builtIn != null) return builtIn;

            var variableDeclarations = mainClass.ChildNodes().OfType<FieldDeclarationSyntax>();
            if (variableDeclarations != null)
            {
                foreach (var declaration in variableDeclarations)
                {
                    VariableDeclaratorSyntax match;
                    if (caseSense)
                        match = declaration.Declaration.Variables.FirstOrDefault(v => v.Identifier.Text == name);
                    else
                        match = declaration.Declaration.Variables.FirstOrDefault(v => v.Identifier.Text.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (match != null)
                        return match.Identifier.Text;
                }
            }
            return null;
        }

        public static string IsBuiltInProperty(string name, bool caseSense = false)
        {
            KeyValuePair<string, PropertyInfo> match;
            if (caseSense && Reflections.flatPublicStaticProperties.ContainsKey(name))
                return name;
            else
                match = Reflections.flatPublicStaticProperties.FirstOrDefault(v => v.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return match.Key;
        }

        public static string MaybeAddGlobalVariableDeclaration(string name, bool caseSense = false)
        {
            string match = IsVarDeclaredGlobally(name, caseSense);
            if (match != null)
                return match;

            if (!caseSense)
                name = name.ToLowerInvariant();

            var variableDeclaration = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName("object"))
                .AddVariables(SyntaxFactory.VariableDeclarator(name));

            var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            mainClass = mainClass.AddMembers(fieldDeclaration);

            return name;
        }

        public static string MaybeAddVariableDeclaration(string name, bool caseSense = false)
        {
            string match;
            if (currentFunc.Scope == Scope.Local)
            {
                // If the variable is supposed to be global then don't add a local declaration
                match = IsGlobalVar(name, caseSense);
                if (match != null) return match;

                // Otherwise if a local declaration is present then return it
                match = IsVarDeclaredLocally(name, caseSense);
                if (match != null) return match;
            }
            else if (currentFunc.Scope == Scope.Global)
            {
                // If the variable is supposed to be local then return it
                match = IsLocalVar(name);
                if (match != null)
                    return match;

                match = IsVarDeclaredGlobally(name, caseSense);
                if (match != null) return match;
            }

            if (!caseSense)
                name = name.ToLowerInvariant();

            var variableDeclaration = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.ParseTypeName("object"))
                .AddVariables(SyntaxFactory.VariableDeclarator(name));

            if (currentFunc.Scope == Scope.Local)
            {
                var localDeclaration = SyntaxFactory.LocalDeclarationStatement(variableDeclaration);
                currentFunc.Body = currentFunc.Body.AddStatements(localDeclaration);
                return name;
            }

            var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            mainClass = mainClass.AddMembers(fieldDeclaration);

            return name;
        }

        public static string MaybeAddGlobalFuncObjVariable(string functionName, bool caseSense = false)
        {

            string name = IsVarDeclaredGlobally(functionName, caseSense);
            if (name != null) return name;

            // Only add built-in functions, because user-defined functions are handled in the constructor
            KeyValuePair<string, MethodInfo> match;
            if (caseSense)
                match = Reflections.flatPublicStaticMethods.FirstOrDefault(v => v.Key == functionName);
            else
                match = Reflections.flatPublicStaticMethods.FirstOrDefault(v => v.Key.Equals(functionName, StringComparison.InvariantCultureIgnoreCase));
            if (match.Key == null)
                return caseSense ? functionName : functionName.ToLowerInvariant();

            return AddGlobalFuncObjVariable(functionName, caseSense);
        }

        public static VariableDeclarationSyntax CreateFuncObjDelegateVariable(string functionName)
        {
            return SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(functionName.ToLowerInvariant())
                        )
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.IdentifierName("FuncObj"),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.CastExpression(
                                                    SyntaxFactory.IdentifierName("Delegate"),
                                                    SyntaxFactory.IdentifierName(functionName)
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

        public static VariableDeclarationSyntax CreateFuncObjVariable(string functionName)
        {
            return SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.Identifier(functionName)
                        )
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.IdentifierName("FuncObj"),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    SyntaxFactory.Literal(functionName)
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

        public static string AddGlobalFuncObjVariable(string functionName, bool caseSense = false)
        {
            if (!caseSense)
                functionName = functionName.ToLowerInvariant();

            var funcObjVariable = SyntaxFactory.FieldDeclaration(
                CreateFuncObjVariable(functionName)
            )
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                )
            );

            mainClass = mainClass.AddMembers(funcObjVariable);
            return functionName;
        }

        public static ExpressionSyntax GenerateFunctionInvocation(
            ExpressionSyntax targetExpression,
            ArgumentListSyntax argumentList,
            string methodName)
        {
            // 1. Built-in functions: Directly invoke the built-in method
            if (!string.IsNullOrEmpty(methodName) &&
                Reflections.flatPublicStaticMethods.TryGetValue(methodName, out var mi))
            {
                // Fully qualified method invocation
                return SyntaxFactory.InvocationExpression(
                    CreateQualifiedName($"{mi.DeclaringType}.{mi.Name}"),
                    argumentList
                );
            }

            // 2. Handle UserTypes
            if (targetExpression is IdentifierNameSyntax identifierName &&
                Helper.UserTypes.Contains(identifierName.Identifier.Text))
            {
                // Append .Call() to the invocation
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        targetExpression,
                        SyntaxFactory.IdentifierName("Call")
                    ),
                    argumentList
                );
            }

            // 3. Handle GetPropertyValue invocation
            if (targetExpression is InvocationExpressionSyntax invocationExpression &&
                invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text.Equals("GetPropertyValue", StringComparison.OrdinalIgnoreCase))
            {
                // Extract arguments of GetPropertyValue
                var propertyArguments = invocationExpression.ArgumentList.Arguments;
                if (propertyArguments.Count == 2)
                {
                    // Extract base expression and property name
                    var baseExpression = propertyArguments[0].Expression;
                    var propertyNameExpression = propertyArguments[1].Expression;

                    // Generate Script.GetMethodOrProperty(base, propertyName, -1)
                    var getMethodOrPropertyInvocation = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Scripting.Script"),
                            SyntaxFactory.IdentifierName("GetMethodOrProperty")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                        SyntaxFactory.Argument(baseExpression),
                        SyntaxFactory.Argument(propertyNameExpression),
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(-1)
                            )
                        )
                            })
                        )
                    );

                    // Wrap in Script.Invoke
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Scripting.Script"),
                            SyntaxFactory.IdentifierName("Invoke")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                        SyntaxFactory.Argument(getMethodOrPropertyInvocation)
                            }.Concat(argumentList.Arguments))
                        )
                    );
                }
            }

            // 4. Handle GetStaticMemberValueT invocation
            if (targetExpression is InvocationExpressionSyntax staticMemberInvocation &&
                staticMemberInvocation.Expression is MemberAccessExpressionSyntax staticMemberAccess &&
                staticMemberAccess.Name.Identifier.Text.Equals("GetStaticMemberValueT", StringComparison.OrdinalIgnoreCase))
            {
                // Extract the generic type and argument
                if (staticMemberAccess.Name is GenericNameSyntax genericName &&
                    staticMemberInvocation.ArgumentList.Arguments.Count == 1)
                {
                    var typeArguments = genericName.TypeArgumentList;
                    var memberNameExpression = staticMemberInvocation.ArgumentList.Arguments[0].Expression;

                    // Transform into Script.Invoke(Script.GetStaticMethodT<typeName>(methodName, -1))
                    var getStaticMethodInvocation = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Scripting.Script"),
                            SyntaxFactory.GenericName("GetStaticMethodT")
                                .WithTypeArgumentList(typeArguments)
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                        SyntaxFactory.Argument(memberNameExpression),
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(-1)
                            )
                        )
                            })
                        )
                    );

                    // Wrap in Script.Invoke
                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName("Keysharp.Scripting.Script"),
                            SyntaxFactory.IdentifierName("Invoke")
                        ),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                        SyntaxFactory.Argument(getStaticMethodInvocation)
                            }.Concat(argumentList.Arguments))
                        )
                    );
                }
            }

            // 5. Default behavior: Treat as callable object and invoke .Call
            var defaultCastExpression = SyntaxFactory.ParenthesizedExpression(
                SyntaxFactory.CastExpression(
                    SyntaxFactory.IdentifierName("ICallable"),
                    targetExpression
                )
            );

            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    defaultCastExpression,
                    SyntaxFactory.IdentifierName("Call")
                ),
                argumentList
            );
        }


        public static string ExtractMethodName(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text;
            }
            if (expression is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.Text;
            }
            return null;
        }

        public static BlockSyntax EnsureBlockSyntax(SyntaxNode node)
        {
            if (node is BlockSyntax block)
                return block;
            else if (node is StatementSyntax statement)
                return SyntaxFactory.Block(statement);
            else if (node is ExpressionSyntax expression)
                return SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(expression));
            else
                throw new InvalidOperationException("Unsupported SyntaxNode type for loop body.");
        }

        public static StatementSyntax EnsureStatementSyntax(SyntaxNode node)
        {
            if (node is StatementSyntax statement)
                return statement;
            else if (node is ExpressionSyntax expression)
                return SyntaxFactory.ExpressionStatement(expression);
            else
                throw new InvalidOperationException("Unsupported node type in statement list.");
        }

        public static StatementSyntax EnsureBreakStatement(StatementSyntax statements)
        {
            if (statements is BlockSyntax blockSyntax)
            {
                var lastStatement = blockSyntax.Statements.LastOrDefault();
                if (lastStatement == null || !(lastStatement is BreakStatementSyntax))
                {
                    // Add a break statement to the block if not present
                    return blockSyntax.AddStatements(SyntaxFactory.BreakStatement());
                }
                return blockSyntax; // Block already has a break
            }
            else if (!(statements is BreakStatementSyntax))
            {
                // Wrap single statement in a block and add a break
                return SyntaxFactory.Block(
                    new StatementSyntax[] { statements, SyntaxFactory.BreakStatement() }
                );
            }

            // Already a break statement
            return statements;
        }


        public static FieldDeclarationSyntax CreatePublicConstant(string name, Type type, object value)
        {
            return SyntaxFactory.FieldDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName(type.FullName),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(name))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(value.ToString())
                                    )
                                )
                            )
                        )
                    )
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.ConstKeyword)
                    )
                );
        }

        public static BlockSyntax RemoveLocalVariable(BlockSyntax block, string variableName)
        {
            var variableDeclaration = block.Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .FirstOrDefault(declaration =>
                    declaration.Declaration.Variables.Any(v => v.Identifier.Text == variableName));

            if (variableDeclaration != null)
                return block.RemoveNode(variableDeclaration, SyntaxRemoveOptions.KeepNoTrivia);

            return block;
        }

        public static bool RemoveGlobalVariable(string variableName, bool local)
        {
            var fieldDeclaration = mainClass.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(declaration =>
                    declaration.Declaration.Variables.Any(v => v.Identifier.Text == variableName));

            if (fieldDeclaration != null)
            {
                mainClass = mainClass.RemoveNode(fieldDeclaration, SyntaxRemoveOptions.KeepNoTrivia);
                return true;
            }

            return false;
        }

        public static bool IsLiteralOrConstant(ExpressionSyntax expression)
        {
            return expression is LiteralExpressionSyntax ||
                   expression is TypeOfExpressionSyntax ||
                   expression is DefaultExpressionSyntax ||
                   expression is IdentifierNameSyntax identifier &&
                   char.IsUpper(identifier.Identifier.Text[0]); // Assume constants are upper-case
        }

    }

}
