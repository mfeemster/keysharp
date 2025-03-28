using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static MainParser;
using static System.Windows.Forms.AxHost;
using Keysharp.Core;
using System.Text.RegularExpressions;
using System.IO;

namespace Keysharp.Scripting
{
    public partial class Parser
    {
        static Parser()
        {
            var anyType = typeof(Any);
            foreach (var type in Reflections.stringToTypes.Values
                    .Where(type => type.IsClass && !type.IsAbstract && anyType.IsAssignableFrom(type)))
            {
                BuiltinTypes[type.Name] = type;
            }
        }
        public void AddAssembly(string assemblyName, string value)
        {
            assemblies = assemblies.Add(SyntaxFactory.Attribute(
                CreateQualifiedName(assemblyName))
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(value)))));
            return;
        }

        public IdentifierNameSyntax PushTempVar()
        {
            tempVarCount++;
            var tempVarName = Keywords.TempVariablePrefix + tempVarCount.ToString();
            var lastScope = currentFunc.Scope;
            currentFunc.Scope = eScope.Local;
            MaybeAddVariableDeclaration(tempVarName);
            currentFunc.Scope = lastScope;
            return SyntaxFactory.IdentifierName(tempVarName);
        }

        public void PopTempVar() => tempVarCount--;

        public static List<ClassDeclarationContext> GetClassDeclarationsRecursive(ParserRuleContext context)
        {
            if (context == null || context.children == null) return new List<ClassDeclarationContext>();
            return context.children
                .OfType<ClassDeclarationContext>()
                .Concat(
                    context.children
                        .OfType<ParserRuleContext>()
                        .SelectMany(GetClassDeclarationsRecursive)
                )
                .ToList();
        }

        public static List<string> GetScopeFunctions(ParserRuleContext context)
        {
            var result = new List<string>();

            if (context == null || context.children == null)
                return result;

            foreach (var child in context.children)
            {
                // Add FunctionDeclarationContext directly
                if (child is FatArrowExpressionContext fatArrow)
                {
                    var head = fatArrow.fatArrowExpressionHead();
                    if (head.functionExpressionHead() != null && head.functionExpressionHead().functionHead() != null)
                        result.Add(head.functionExpressionHead().functionHead().identifier().GetText());

                }
                else if (child is FunctionExpressionContext func && func.functionExpressionHead().functionHead() != null)
                {
                    result.Add(func.functionExpressionHead().functionHead().identifier().GetText());
                }
                else if (child is FunctionDeclarationContext funcdecl)
                {
                    result.Add(funcdecl.functionHead().identifier().GetText());
                }
                // Recurse into children only if the current node is not a FunctionDeclarationContext
                else if (child is ParserRuleContext parserRuleContext && child is not FunctionDeclarationContext && child is not FatArrowExpressionContext && child is not FunctionExpressionContext && child is not ClassDeclarationContext)
                {
                    result.AddRange(GetScopeFunctions(parserRuleContext));
                }
            }

            return result;
        }

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
        public UsingDirectiveSyntax CreateUsingDirective(string usingName)
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

        public string IsLocalVar(string name, bool caseSense = true)
        {
            if (caseSense)
            {
                if (currentFunc.Locals.ContainsKey(name) || currentFunc.Statics.Contains(name))
                    return name;
            }
            else
            {
                string match = currentFunc.Locals.Keys.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                    return match;
                match = currentFunc.Statics.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (match != null)
                    return match;
            }

            if (!currentFunc.Static)
            {
                // Skip the UserMainFunction function in the stack and check the rest
                foreach (var (func, _) in FunctionStack.SkipLast(1))
                {
                    if (caseSense)
                    {
                        if (func.Locals.ContainsKey(name) || func.Statics.Contains(name))
                            return name;
                    }
                    else
                    {
                        string match = func.Locals.Keys.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                        if (match != null)
                            return match;
                        match = func.Statics.FirstOrDefault(v => v.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                        if (match != null)
                            return match;
                    }
                }
            }

            return null;
        }

        public string IsVarRef(string name)
        {
            string match = null;
            if (currentFunc.VarRefs.TryGetValue(name, out match))
                return match;

            foreach (var (func, _) in FunctionStack.SkipLast(1))
            {
                if (func.VarRefs.TryGetValue(name, out match))
                    return match;
            }
            return null;
        }

        public string IsStaticVar(string name)
        {
            if (currentFunc.Statics.TryGetValue(name, out var match))
                return match;
            return null;
        }

        public string IsVarDeclaredLocally(string name, bool caseSense = true)
        {
            if (currentFunc.Body == null)
                return null;

            if (currentFunc.Params != null)
            {
                var parameterMatch = currentFunc.Params.FirstOrDefault(param =>
                    caseSense ? param.Identifier.Text == name
                              : param.Identifier.Text.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (parameterMatch != null)
                    return parameterMatch.Identifier.Text;
            }

            var localMatch = IsLocalVar(name, caseSense);
            if (localMatch != null) return localMatch;

            var variableDeclarations = currentFunc.Body.OfType<LocalDeclarationStatementSyntax>();
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

            if (IsStaticDefinedInThisOrParent(name) is string staticName && staticName != null)
                return staticName;

            return null;
        }

        public string IsGlobalVar(string name, bool caseSense = true)
        {
            //if (UserFuncs.Contains(name) || UserTypes.ContainsKey(name))
            //    return name.ToLowerInvariant();

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

        public string IsVarDeclaredInClass(Class cls, string name, bool caseSense = true)
        {
            var variableDeclarations = cls.Body
                .OfType<FieldDeclarationSyntax>();
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

        public string IsVarDeclaredGlobally(string name, bool caseSense = true)
        {
            return IsVarDeclaredInClass(mainClass, name, caseSense);
        }

        public string IsBuiltIn(string name, bool caseSense = false)
        {
            var builtin = IsBuiltInMethod(name, caseSense);
            if (builtin != null) return builtin;

            builtin = IsBuiltInProperty(name, caseSense);
            if (builtin != null) return builtin;

            if (Reflections.stringToTypes.ContainsKey(name))
                return caseSense ? ((Reflections.stringToTypes.FirstOrDefault(item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Key) == name ? name : null) : name.ToLower();

            return null;
        }

        public string IsBuiltInProperty(string name, bool caseSense = false)
        {
            KeyValuePair<string, PropertyInfo> match;
            if (caseSense && Reflections.flatPublicStaticProperties.ContainsKey(name))
                return name;
            else
                match = Reflections.flatPublicStaticProperties.FirstOrDefault(v => v.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (match.Key == null
                && currentClass != null
                && Reflections.stringToTypeProperties.ContainsKey(name)
                && Reflections.stringToTypeProperties[name].Keys.Any(item => item.Name == currentClass.Base || item.Name == "KeysharpObject"))
            {
                return Reflections.stringToTypeProperties.FirstOrDefault(item => item.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Key;
            }
            return match.Key;
        }

        public string IsBuiltInMethod(string name, bool caseSense = false)
        {
            MethodPropertyHolder mph = Reflections.FindBuiltInMethod(name, -1);
            if (mph?.mi == null || (caseSense && mph.mi.Name != name))
                return null;
            return mph.mi.Name;
        }

        public string MaybeAddGlobalVariableDeclaration(string name, bool caseSense = true)
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

            mainClass.Body.Add(fieldDeclaration);

            return name;
        }

        public string MaybeAddVariableDeclaration(string name, bool caseSense = true)
        {
            string match;
            if (currentFunc.Scope == eScope.Static)
            {
                match = IsVarDeclaredInClass(currentClass, name);
                if (match != null) return match;
            }
            if (currentFunc.Scope == eScope.Local || currentFunc.Scope == eScope.Static)
            {
                // If the variable is supposed to be global then don't add a local declaration
                match = IsGlobalVar(name, caseSense);
                if (match != null) return match;

                // Otherwise if a local declaration is present then return it
                match = IsVarDeclaredLocally(name, caseSense);
                if (match != null) return match;
            }
            else if (currentFunc.Scope == eScope.Global)
            {
                // If the variable is supposed to be local then return it
                match = IsLocalVar(name);
                if (match != null)
                    return match;

                match = IsVarDeclaredGlobally(name, caseSense);
                if (match != null) return match;
            }

            if (IsBuiltIn(name, caseSense) is string builtin && builtin != null)
                return builtin;

            return AddVariableDeclaration(name, caseSense);
        }

        public string AddVariableDeclaration(string name, bool caseSense = true)
        {
            if (!caseSense)
                name = name.ToLowerInvariant();

            if (currentFunc.Scope == eScope.Static && !currentFunc.Statics.Contains(name))
            {
                currentFunc.Statics.Add(name);
            }

            var variableDeclaration = SyntaxFactory.VariableDeclaration(
            SyntaxFactory.ParseTypeName("object"))
            .AddVariables(
                SyntaxFactory.VariableDeclarator(name)
                .WithInitializer(
                    SyntaxFactory.EqualsValueClause(
                        SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                    )
                )
            );

            if (currentFunc.Scope == eScope.Local)
            {
                var localDeclaration = SyntaxFactory.LocalDeclarationStatement(variableDeclaration);
                currentFunc.Locals[name] = (StatementSyntax)localDeclaration;
                return name;
            }

            var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(
            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
            SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            if (currentFunc.Statics.Contains(name) || currentFunc.Scope == eScope.Static)
            {
                currentClass.Body.Add(fieldDeclaration);
            }
            else
                mainClass.Body.Add(fieldDeclaration);

            return name;
        }

        public string MaybeAddClassStaticVariable(string className, bool caseSense = false)
        {
            className = className.TrimStart('@');
            string name = IsVarDeclaredGlobally(className, caseSense);
            if (name != null) return name;

            // Only add built-in classes, because user-defined classes are handled in the constructor
            if (!Parser.BuiltinTypes.ContainsKey(className))
                return null;
            string casedName = Parser.BuiltinTypes.SingleOrDefault(kv => kv.Key.Equals(className, StringComparison.OrdinalIgnoreCase)).Key;

            // Add `public static object myclass = Myclass.__Static;` global field
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName("object"),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(className.ToLower())
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ElementAccessExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("Variables"),
                                            SyntaxFactory.IdentifierName("Statics")
                                        ),
                                        SyntaxFactory.BracketedArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.TypeOfExpression(
                                                        SyntaxFactory.IdentifierName(casedName)
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
            .AddModifiers(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword)
            );

            mainClass.Body.Add(fieldDeclaration);

            return className.ToLower();
        }

        public string MaybeAddGlobalFuncObjVariable(string functionName, bool caseSense = true)
        {
            functionName = functionName.TrimStart('@');
            string name = IsVarDeclaredGlobally(ToValidIdentifier(functionName), caseSense);
            if (name != null) return name;

            // Only add built-in functions, because user-defined functions are handled in the constructor
            name = IsBuiltInMethod(functionName, caseSense);
            if (name == null) return caseSense ? functionName : functionName.ToLowerInvariant();

            functionName = name;

            return AddGlobalFuncObjVariable(functionName, caseSense);
        }

        public static VariableDeclarationSyntax CreateNullObjectVariable(string variableName)
        {
            return SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                .WithVariables(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(variableName))
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)
                            )
                        )
                    )
                );
        }

        public static ExpressionSyntax CreateFuncObjAssignment(string variableName, string functionName)
        {
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(variableName),
                CreateFuncObj(
                    SyntaxFactory.CastExpression(
                        SyntaxFactory.IdentifierName("Delegate"),
                        SyntaxFactory.IdentifierName(functionName)
                    )
                )
            );
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
                                CreateFuncObj(
                                    SyntaxFactory.CastExpression(
                                        SyntaxFactory.IdentifierName("Delegate"),
                                        SyntaxFactory.IdentifierName(functionName)
                                    )
                                )
                            )
                        )
                    )
                );
        }

        public static InvocationExpressionSyntax CreateFuncObj(ExpressionSyntax funcArg)
        {
            return ((InvocationExpressionSyntax)InternalMethods.Func)
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                funcArg
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
                            SyntaxFactory.Identifier(Parser.ToValidIdentifier(functionName))
                        )
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                CreateFuncObj(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(functionName)
                                    )
                                )
                            )
                        )
                    )
                );
        }

        public string AddGlobalFuncObjVariable(string functionName, bool caseSense = false)
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

            mainClass.Body.Add(funcObjVariable);
            return ToValidIdentifier(functionName);
        }

        public ExpressionSyntax GenerateFunctionInvocation(
            ExpressionSyntax targetExpression,
            ArgumentListSyntax argumentList,
            string methodName)
        {
            // 1. Built-in functions: Directly invoke the built-in method
            // except in the case of a variadic function call
            if (!string.IsNullOrEmpty(methodName) &&
                Reflections.FindBuiltInMethod(methodName, -1) is MethodPropertyHolder mph && mph.mi != null && !UserFuncs.Contains(methodName))
            {
                if (argumentList.Arguments.Count > 0 && argumentList.Arguments.First().Expression is CollectionExpressionSyntax)
                {
                    targetExpression = ((InvocationExpressionSyntax)InternalMethods.Func)
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[]
                            {
                                SyntaxFactory.Argument(targetExpression)
                            })
                        )
                    );
                } else
                    // Fully qualified method invocation
                    return SyntaxFactory.InvocationExpression(
                        CreateQualifiedName($"{mph.mi.DeclaringType}.{mph.mi.Name}"),
                        argumentList
                    );
            }

            // 2. Handle UserTypes
            if (targetExpression is IdentifierNameSyntax identifierName &&
                UserTypes.ContainsKey(identifierName.Identifier.Text))
            {
                // Convert to Invoke(targetExpression, "Call", arguments)
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("Invoke"),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.Argument(targetExpression),
                            SyntaxFactory.Argument(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal("Call")
                                )
                            )
                        }.Concat(argumentList.Arguments)) // Include additional arguments
                    )
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

                    // Generate Script.Invoke(obj, prop, args)
                    return ((InvocationExpressionSyntax)InternalMethods.Invoke)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(baseExpression),
                                    SyntaxFactory.Argument(propertyNameExpression)
                                }.Concat(argumentList.Arguments)) // Pass additional arguments (args)
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
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("Invoke"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(targetExpression),
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal("Call")
                            )
                        )
                    }.Concat(argumentList.Arguments)) // Include additional arguments
                )
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

        public string NormalizeClassIdentifier(string name, eNameCase nameCase = eNameCase.Title)
        {
            // Define the reserved keywords that should retain their original case

            if (nameCase == eNameCase.Title && ClassReservedKeywords.Contains(name))
                return ClassReservedKeywords.First(keyword => string.Equals(keyword, name, StringComparison.InvariantCultureIgnoreCase));

            var builtinProp = PropertyExistsInBuiltinBase(name);
            if (builtinProp != null) return builtinProp;

            return NormalizeIdentifier(name, nameCase);
        }

        public string CreateStaticIdentifier(string name)
        {
            if (currentFunc == null) return null;
            return currentFunc.Name.ToUpper() + "_" + name.ToLowerInvariant().TrimStart('@');
        }

        public string IsStaticDefinedInThisOrParent(string name)
        {
            if (currentFunc == null) return null;
            name = name.ToLowerInvariant();
            string staticName = CreateStaticIdentifier(name);

            if (currentFunc.Statics.Contains(staticName)) return staticName;
            if (currentFunc.Locals.ContainsKey(name)) return null;

            if (!currentFunc.Static)
            {
                foreach (var (f, _) in FunctionStack)
                {
                    staticName = f.Name.ToUpper() + "_" + name.TrimStart('@');
                    if (f.Statics.Contains(staticName)) return staticName;
                    if (f.Locals.ContainsKey(name)) return null;
                }
            }
            return null;
        }

        public string NormalizeFunctionIdentifier(string name, eNameCase nameCase = eNameCase.Lower)
        {
            name = name.Trim('"', '\'');
            var normalizedName = NormalizeIdentifier(name, nameCase);

            // First check whether it's a static variable or should be a static variable,
            // then check if it's a built-in method, property, or a type

            if (IsVarDeclaredLocally(normalizedName, true) is string localName && localName != null)
                return localName;

            // This should be handled by IsVarDeclaredLocally
            //if (IsStaticDefinedInThisOrParent(normalizedName) is string staticName && staticName != null)
            //    return staticName;

            if (IsVarDeclaredGlobally(normalizedName, true) is string globalName && globalName != null)
                return globalName;

            if (UserTypes.ContainsKey(normalizedName))
                return normalizedName;

            var builtin = IsBuiltInProperty(name);
            if (builtin != null) return builtin;

            // Normalize before checking these, because method and type identifiers will all be lower-case
            builtin = IsBuiltInMethod(normalizedName);
            if (builtin != null) return normalizedName;

            if (Reflections.stringToTypes.ContainsKey(normalizedName))
                return normalizedName;

            if (currentFunc.Scope == eScope.Static)
                return CreateStaticIdentifier(normalizedName);

            return normalizedName;
        }

        public static string NormalizeIdentifier(string name, eNameCase nameCase = eNameCase.Lower)
        {
            name = name.Trim('"', '\'');

            if (Parser.TypeNameAliases.ContainsKey(name) && Reflections.stringToTypes.ContainsKey(Parser.TypeNameAliases[name]))
                name = Parser.TypeNameAliases[name];

            if (nameCase == eNameCase.Lower)
                return ToValidIdentifier(name.ToLowerInvariant());
            else if (nameCase == eNameCase.Title)
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            else
                return name.ToUpperInvariant();
        }

        public static string ToValidIdentifier(string text)
        {
            if (SyntaxFacts.IsKeywordKind(SyntaxFactory.ParseToken(text).Kind()))
                return "@" + text;
            return text;
        }

        public string PropertyExistsInBuiltinBase(string name)
        {
            if (Reflections.stringToTypeProperties.TryGetValue(name, out var dttp))
            {
                string className = currentClass.Name;
                while (AllTypes.TryGetValue(className, out string classBase))
                {
                    className = classBase;
                    if (dttp.Any(t => t.Key.Name == className))
                        return Reflections.stringToTypeProperties.Keys
                            .FirstOrDefault(key => string.Equals(key, name, StringComparison.OrdinalIgnoreCase));
                }
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

        public BlockSyntax RemoveLocalVariable(BlockSyntax block, string variableName)
        {
            var variableDeclaration = block.Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .FirstOrDefault(declaration =>
                    declaration.Declaration.Variables.Any(v => v.Identifier.Text == variableName));

            if (variableDeclaration != null)
                return block.RemoveNode(variableDeclaration, SyntaxRemoveOptions.KeepNoTrivia);

            return block;
        }

        public bool RemoveGlobalVariable(string variableName, bool local)
        {
            var fieldDeclaration = mainClass.Body
                .OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(declaration =>
                    declaration.Declaration.Variables.Any(v => v.Identifier.Text == variableName));

            if (fieldDeclaration != null)
            {
                mainClass.Body.Remove(fieldDeclaration);
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

        public ExpressionSyntax CreateSuperTuple()
        {
            return SyntaxFactory.CastExpression(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                    SyntaxFactory.TupleExpression(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            new ArgumentSyntax[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(
                                    CreateQualifiedName(currentClass.Base) // typeof(MyType)
                                )),
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("@this"))
                            }
                        )
                    )
            );
        }

        public ParameterSyntax AddOptionalParamValue(ParameterSyntax parameter, ExpressionSyntax value)
        {
            // Extract the parameter name
            var parameterName = parameter.Identifier.Text;

            // Determine if the value is a constant
            bool isConstant = value is LiteralExpressionSyntax;

            // Check if the parameter type is VarRef
            var isVarRefType = parameter.Type is IdentifierNameSyntax typeSyntax &&
                               typeSyntax.Identifier.Text.Equals("VarRef", StringComparison.OrdinalIgnoreCase);

            StatementSyntax initializationStatement = null;

            if (isVarRefType)
            {
                // Add the initialization statement to the function body
                initializationStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.CoalesceAssignmentExpression,
                        SyntaxFactory.IdentifierName(parameterName),
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.IdentifierName("VarRef"),
                            SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(value))),
                            null
                        )
                    )
                );

                // Set the default value to null
                value = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            else if (!isConstant)
            {
                // Add the coalesce assignment to the function body if the value is non-constant
                initializationStatement = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.CoalesceAssignmentExpression,
                        SyntaxFactory.IdentifierName(parameterName),
                        value
                    )
                );

                // Set the default value to null
                value = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }

            if (initializationStatement != null)
            {
                // Add the initialization statement to the beginning of the function body
                currentFunc.Body.Insert(0, initializationStatement);
            }

            // Add the attributes for Optional and DefaultParameterValue
            return parameter.WithAttributeLists(SyntaxFactory.SingletonList(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Optional")),
                        SyntaxFactory.Attribute(
                            SyntaxFactory.IdentifierName("DefaultParameterValue"),
                            SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(value)
                                )
                            )
                        )
                    })
                )
            ));
        }
    }
}
