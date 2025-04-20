using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static MainParser;
using System.Data.Common;
using System.Linq.Expressions;

namespace Keysharp.Scripting
{
    internal partial class Parser
    {
        internal static Dictionary<int, SyntaxKind> pureBinaryOperators = new Dictionary<int, SyntaxKind>()
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

        internal static Dictionary<int, string> binaryOperators = new Dictionary<int, string>()
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

        internal static Dictionary<int, string> unaryOperators = new Dictionary<int, string>()
        {
            {MainParser.Minus, "Minus"},
            {MainParser.Not, "LogicalNot"},
            {MainParser.VerbalNot, "LogicalNot"},
            {MainParser.BitNot, "BitwiseNot"}
        };

        static Parser()
        {
            var anyType = typeof(Any);
            foreach (var type in Reflections.stringToTypes.Values
                    .Where(type => type.IsClass && !type.IsAbstract && anyType.IsAssignableFrom(type)))
            {
                BuiltinTypes[type.Name] = type;
            }
        }
        internal void AddAssembly(string assemblyName, string value)
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

        // Used to create temporary variables (eg for-loop vars) which can also be reused later
        internal IdentifierNameSyntax PushTempVar()
        {
            tempVarCount++;
            var tempVarName = Keywords.TempVariablePrefix + tempVarCount.ToString();
            var lastScope = currentFunc.Scope;
            currentFunc.Scope = eScope.Local;
            MaybeAddVariableDeclaration(tempVarName);
            currentFunc.Scope = lastScope;
            return SyntaxFactory.IdentifierName(tempVarName);
        }

        internal void PopTempVar() => tempVarCount--;

        internal static List<ClassDeclarationContext> GetClassDeclarationsRecursive(ParserRuleContext context)
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

        public struct FunctionInfo
        {
            public string Name;
            public bool Static;
        }

        internal static List<FunctionInfo> GetScopeFunctions(ParserRuleContext context)
        {
            var result = new List<FunctionInfo>();

            if (context == null || context.children == null)
                return result;

            foreach (var child in context.children)
            {
                FunctionHeadContext head = null;
				// Add FunctionDeclarationContext directly
				if (child is FatArrowExpressionContext fatArrow)
                {
                    head = fatArrow.fatArrowExpressionHead()?.functionExpressionHead()?.functionHead();
                }
                else if (child is FunctionExpressionContext func && func.functionExpressionHead().functionHead() != null)
                {
                    head = func.functionExpressionHead().functionHead();
                }
                else if (child is FunctionDeclarationContext funcdecl)
                {
                    head = funcdecl.functionHead();
                }
                // Recurse into children only if the current node is not a FunctionDeclarationContext
                else if (child is ParserRuleContext parserRuleContext && child is not FunctionDeclarationContext && child is not FatArrowExpressionContext && child is not FunctionExpressionContext && child is not ClassDeclarationContext)
                {
                    result.AddRange(GetScopeFunctions(parserRuleContext));
                }

                if (head != null)
                {
					var f = new FunctionInfo();
                    f.Name = head.identifierName().GetText();
                    f.Static = head.functionHeadPrefix()?.Static() != null;
                    result.Add(f);
				}
            }

            return result;
        }

        // Creating an identifier for something like "Keysharp.Program" results in a single identifier,
        // not a namespace/class access. This function splits the name by "." and then joins it one-by-one.
        internal static NameSyntax CreateQualifiedName(string qualifiedString)
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
        internal UsingDirectiveSyntax CreateUsingDirective(string usingName)
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

        internal static LiteralExpressionSyntax NumericLiteralExpression(string value)
        {
            if (value.Contains("."))
            {
                double.TryParse(value, CultureInfo.InvariantCulture, out double result);
				value = value.EndsWith("d", StringComparison.OrdinalIgnoreCase)
	                ? value
	                : value + "d";
				return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value, result));
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

        internal static InvocationExpressionSyntax CreateBinaryOperatorExpression(int op, ExpressionSyntax exprL, ExpressionSyntax exprR)
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

        // Checks whether a local or static variable is in the current function or any parent functions.
        // See also: IsVarDeclaredLocally
        internal string IsLocalVar(string name, bool caseSense = true)
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

        // Whether the variable was a by-reference parameter
        internal string IsVarRef(string name)
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

        internal string IsStaticVar(string name)
        {
            if (currentFunc.Statics.TryGetValue(name, out var match))
                return match;
            return null;
        }

        // Checks whether a local or static variable has been declared in the C# function body
        internal string IsVarDeclaredLocally(string name, bool caseSense = true)
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

        internal string IsStaticDefinedInThisOrParent(string name)
        {
            if (currentFunc == null) return null;
            name = name.ToLowerInvariant();
            string staticName = CreateStaticIdentifier(name);

            if (currentFunc.Statics.Contains(staticName)) return staticName;
            if (currentFunc.Locals.ContainsKey(name)) return null;

            foreach (var (f, _) in FunctionStack)
            {
                if (f.Name == Keywords.AutoExecSectionName)
                    break;

                staticName = f.Name.ToUpper() + "_" + name.TrimStart('@');
                if (f.Statics.Contains(staticName)) return staticName;
                if (f.Locals.ContainsKey(name)) return null;
            }
            return null;
        }

        // Checks whether the variable has been declared as global 
        internal string IsGlobalVar(string name, bool caseSense = true)
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

        // Checks for a field declaration in a specific class
        internal string IsVarDeclaredInClass(Class cls, string name, bool caseSense = true)
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

        internal string IsVarDeclaredGlobally(string name, bool caseSense = true)
        {
            return IsVarDeclaredInClass(mainClass, name, caseSense);
        }

        internal string IsBuiltIn(string name, bool caseSense = false)
        {
            var builtin = IsBuiltInMethod(name, caseSense);
            if (builtin != null) return builtin;

            builtin = IsBuiltInProperty(name, caseSense, true);
            if (builtin != null) return builtin;

            if (Reflections.stringToTypes.ContainsKey(name))
                return caseSense ? ((Reflections.stringToTypes.FirstOrDefault(item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Key) == name ? name : null) : name.ToLower();

            return null;
        }

        internal string IsBuiltInProperty(string name, bool caseSense = false, bool ignoreExtensionClass = false)
        {
            KeyValuePair<string, PropertyInfo> match;
            if (caseSense && Reflections.flatPublicStaticProperties.ContainsKey(name))
                return name;
            else
                match = Reflections.flatPublicStaticProperties.FirstOrDefault(v => v.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (!ignoreExtensionClass
                && match.Key == null
                && currentClass != null
                && Reflections.stringToTypeProperties.ContainsKey(name))
            {
                var baseName = currentClass.Base;
                while (UserTypes.ContainsKey(baseName))
                {
                    if (baseName == UserTypes[baseName])
                        break;
                    baseName = UserTypes[baseName];
                }

                if (Reflections.stringToTypeProperties[name].Keys.Any(item => item.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
                    return Reflections.stringToTypeProperties.FirstOrDefault(item => item.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;
            }
            return match.Key;
        }

        internal string IsBuiltInMethod(string name, bool caseSense = false)
        {
            MethodPropertyHolder mph = Reflections.FindBuiltInMethod(name, -1);
            if (mph?.mi == null || (caseSense && mph.mi.Name != name))
                return null;
            return mph.mi.Name;
        }

        // Either adds a new global variable declaration, or returns a previously declared ones name
        internal string MaybeAddGlobalVariableDeclaration(string name, bool caseSense = true)
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

        // Either adds a new local, static, or global variable declaration (depending on scope),
        // or returns a previously declared variable name
        internal string MaybeAddVariableDeclaration(string name, bool caseSense = true)
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

        // Unconditionally adds a static, local, or global variable declaration
        internal string AddVariableDeclaration(string name, bool caseSense = true)
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

        // // Adds `public static object myclass = Myclass.__Static;` as a global variable (the class static instance declaration)
        internal string MaybeAddClassStaticVariable(string className, bool caseSense = false)
        {
            className = className.TrimStart('@');
            string name = IsVarDeclaredGlobally(className, caseSense);
            if (name != null) return name;

            // Only add built-in classes, because user-defined classes are handled in the constructor
            if (!Parser.BuiltinTypes.ContainsKey(className))
                return null;
            string casedName = Parser.BuiltinTypes.SingleOrDefault(kv => kv.Key.Equals(className, StringComparison.OrdinalIgnoreCase)).Key;

            // Add `internal static object myclass = Myclass.__Static;` global field
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

        // Adds a global FuncObj variable if one is not already present,
        // eg `public static object msgbox = FuncObj(MsgBox);`
        internal string MaybeAddGlobalFuncObjVariable(string functionName, bool caseSense = true)
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

        internal string AddGlobalFuncObjVariable(string functionName, bool caseSense = false)
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

        // Used to create function object variables for closures. The closure is cast to a Delegate
        // and then the FuncObj is assigned to a lower-cased variable of the closure name.
        internal static VariableDeclarationSyntax CreateFuncObjDelegateVariable(string functionName)
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

        internal static InvocationExpressionSyntax CreateFuncObj(ExpressionSyntax funcArg, bool asClosure = false)
        {
            return (asClosure ? (InvocationExpressionSyntax)InternalMethods.Closure : (InvocationExpressionSyntax)InternalMethods.Func)
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

        internal static VariableDeclarationSyntax CreateFuncObjVariable(string functionName)
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

        // Creates a VariableDeclarationSyntax for `object var = null;`
        internal static VariableDeclarationSyntax CreateNullObjectVariable(string variableName)
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

        internal static ExpressionSyntax CreateSimpleAssignment(string variableName, ExpressionSyntax value)
        {
            return SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(variableName),
                value
            );
        }

        /*
        Converts an ExpressionSyntax to a function InvocationExpressionSyntax. 
        1. Built-in functions get called with the fully qualified name, eg MsgBox -> Keysharp.Core.Dialogs.MsgBox
        EXCEPT if the argument list contains variadic arguments, in which case Invoke is used.
        2. User-declared classes are converted to Invoke(targetExpression, "Call", arguments)
        3. Property access such as `obj.propName()` is converted to Invoke(obj, propName, args)
        4. GetStaticMemberValueT gets converted to Invoke(GetStaticMethodT<typeName>(methodName, -1))
        5. Anything else is converted to Invoke(targetExpression, "Call", arguments)
        */
        public ExpressionSyntax GenerateFunctionInvocation(
            ExpressionSyntax targetExpression,
            ArgumentListSyntax argumentList,
            string methodName)
        {
            // 1. Built-in functions: Directly invoke the built-in method
            // except in the case of a variadic function call
            if (!string.IsNullOrEmpty(methodName)
				&& Reflections.FindBuiltInMethod(methodName, -1) is MethodPropertyHolder mph && mph.mi != null
				&& !UserFuncs.Contains(methodName)
                && IsVarDeclaredLocally(methodName) == null
                )
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


        internal static string ExtractMethodName(ExpressionSyntax expression)
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

        internal string NormalizeClassIdentifier(string name, eNameCase nameCase = eNameCase.Title)
        {
            // Define the reserved keywords that should retain their original case

            if (nameCase == eNameCase.Title && ClassReservedKeywords.Contains(name))
                return ClassReservedKeywords.First(keyword => string.Equals(keyword, name, StringComparison.InvariantCultureIgnoreCase));

            var builtinProp = PropertyExistsInBuiltinBase(name);
            if (builtinProp != null) return builtinProp;

            return NormalizeIdentifier(name, nameCase);
        }

        // A static variable is named "UPPERCASEFUNCTIONNAME_lowercasevariablename"
        internal string CreateStaticIdentifier(string name)
        {
            if (currentFunc == null) return null;
            return currentFunc.Name.ToUpper() + "_" + name.ToLowerInvariant().TrimStart('@');
        }

        internal ExpressionStatementSyntax CreateStaticVariableInitializer(IdentifierNameSyntax identifier, ExpressionSyntax initializerValue)
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
								SyntaxFactory.Literal(currentClass.Name + "_" + identifier.Identifier.Text)
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

        internal string NormalizeFunctionIdentifier(string name, eNameCase nameCase = eNameCase.Lower)
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

            var builtin = IsBuiltInProperty(name, false, true);
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

        internal static string NormalizeIdentifier(string name, eNameCase nameCase = eNameCase.Lower)
        {
            name = name.Trim('"', '\'');

            //if (Parser.TypeNameAliases.ContainsKey(name) && Reflections.stringToTypes.ContainsKey(Parser.TypeNameAliases[name]))
            //    name = Parser.TypeNameAliases[name];

            if (nameCase == eNameCase.Lower)
                return ToValidIdentifier(name.ToLowerInvariant());
            else if (nameCase == eNameCase.Title)
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            else
                return name.ToUpperInvariant();
        }

        // Escapes keyword identifiers with @
        internal static string ToValidIdentifier(string text)
        {
            if (SyntaxFacts.IsKeywordKind(SyntaxFactory.ParseToken(text).Kind()))
                return "@" + text;
            return text;
        }

        internal string PropertyExistsInBuiltinBase(string name)
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

        internal static BlockSyntax EnsureBlockSyntax(SyntaxNode node)
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

        internal static StatementSyntax EnsureStatementSyntax(SyntaxNode node)
        {
            if (node is StatementSyntax statement)
                return statement;
            else if (node is ExpressionSyntax expression)
                return SyntaxFactory.ExpressionStatement(expression);
            else
                throw new InvalidOperationException("Unsupported node type in statement list.");
        }

        internal static StatementSyntax EnsureBreakStatement(StatementSyntax statements)
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


        internal static FieldDeclarationSyntax CreatePublicConstant(string name, Type type, object value)
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

        public static ExpressionSyntax ConstructVarRefFromIdentifier(string identifier)
        {
            var identifierName = SyntaxFactory.IdentifierName(identifier);
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

        // Creates `new VarRef(() => identifier, value => identifier = value)`
        // In the case of index or property access, it uses the appropriate get/set methods.
        public ExpressionSyntax ConstructVarRef(ExpressionSyntax targetExpression)
        {
            // Handle the different cases of singleExpression
            if (targetExpression is IdentifierNameSyntax identifierName)
            {
                // Case: Variable identifier
                var addedName = MaybeAddVariableDeclaration(identifierName.Identifier.Text);
                if (addedName != null && addedName != identifierName.Identifier.Text)
                    targetExpression = identifierName = SyntaxFactory.IdentifierName(addedName);
                return ConstructVarRefFromIdentifier(identifierName.Identifier.Text);
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
            } else if (targetExpression is ObjectCreationExpressionSyntax oces
                && oces.Type.GetLastToken().ValueText == "VarRef")
            {
                return oces;
            }

            throw new InvalidOperationException("Unsupported singleExpression type for VarRefExpression.");
        }

        internal static bool IsLiteralOrConstant(ExpressionSyntax expression)
        {
            return expression is LiteralExpressionSyntax ||
                   expression is TypeOfExpressionSyntax ||
                   expression is DefaultExpressionSyntax ||
                   expression is IdentifierNameSyntax identifier &&
                   char.IsUpper(identifier.Identifier.Text[0]); // Assume constants are upper-case
        }

        internal ExpressionSyntax CreateSuperTuple()
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

        internal static ExpressionSyntax GenerateGetPropertyValue(ExpressionSyntax baseExpression, ExpressionSyntax memberExpression)
        {
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

        // Non-constant optional parameter values get added to the top of the function body 
        // as `paramName ??= value` and the default value is set to null. This allows using
        // arbitrary expressions as default values.
        internal ParameterSyntax AddOptionalParamValue(ParameterSyntax parameter, ExpressionSyntax value)
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
