using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Antlr4.Runtime.Misc;
using static MainParser;
using Microsoft.CodeAnalysis;
using System.Drawing.Imaging;
using Antlr4.Runtime;
using System.IO;
using System.Collections;
using System.Reflection;
using static System.Windows.Forms.AxHost;
using static Keysharp.Scripting.Parser;

namespace Keysharp.Scripting
{
    internal partial class VisitMain : MainParserBaseVisitor<SyntaxNode>
    {
        public ParameterSyntax VariadicParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")) // Default name for spread argument
        .WithType(SyntaxFactory.ArrayType(
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // object[]
            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory.OmittedArraySizeExpression()
                )
            ))
        ))
        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));

        public override SyntaxNode VisitClassDeclaration([NotNull] ClassDeclarationContext context)
        {
            string userDeclaredName = context.identifier().GetText();
            PushClass(Parser.NormalizeIdentifier(userDeclaredName, eNameCase.Title));
            parser.currentClass.UserDeclaredName = userDeclaredName;

            // Determine the base class (Extends clause)
            if (context.Extends() != null)
            {
                var extendsParts = context.classExtensionName().identifier();
                var baseClassName = parser.NormalizeClassIdentifier(extendsParts[0].GetText());
                for (int i = 1; i < extendsParts.Length; i++)
                {
                    baseClassName += "." + parser.NormalizeClassIdentifier(extendsParts[i].GetText());
                }
                if (extendsParts.Length == 1 && Reflections.stringToTypes.ContainsKey(baseClassName))
                {
                    baseClassName = Reflections.stringToTypes.First(pair => pair.Key.Equals(baseClassName, StringComparison.InvariantCultureIgnoreCase)).Key;
                }
                parser.currentClass.Base = baseClassName;
                parser.currentClass.BaseList.Add(SyntaxFactory.SimpleBaseType(CreateQualifiedName(baseClassName)));
            }
            else
            {
                // Default base class is KeysharpObject
                parser.currentClass.BaseList.Add(SyntaxFactory.SimpleBaseType(CreateQualifiedName("KeysharpObject")));
            }

            // Add `public static object myclass = Myclass.__Static;` global field
            /*var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName("object"),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(parser.currentClass.Name.ToLower())
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        CreateQualifiedName(
                                            string.Join(".",
                                                parser.ClassStack
                                                .Reverse()               // reverse to get outer-to-inner order
                                                .Select(cls => cls.Name) // extract the Name property
                                            ) + "." + parser.currentClass.Name
                                        ),
                                        SyntaxFactory.IdentifierName("__Static")
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
            */

            string fieldDeclarationName = parser.currentClass.Name.ToLower();

            MemberDeclarationSyntax fieldDeclaration = null;
            SyntaxToken[] fieldDeclarationModifiers = [SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)];
            var fieldDeclarationArrowClause = SyntaxFactory.ArrowExpressionClause(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            CreateQualifiedName(
                                string.Join(".",
                                    parser.ClassStack.Reverse()               // Outer-to-inner order.
                                    .Select(cls => cls.Name)
                                ) + "." + parser.currentClass.Name
                            ),
                            SyntaxFactory.IdentifierName("__Static")
                        )
                    );
            if (parser.ClassStack.Count == 1)
            {
                fieldDeclaration = SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.ParseTypeName("object"),
                    fieldDeclarationName
                )
                .AddModifiers(fieldDeclarationModifiers)
                .WithExpressionBody(fieldDeclarationArrowClause)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                parser.ClassStack.Peek().Body.Add(fieldDeclaration);
            }

            // Add the super property
            var superPropertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.TupleType(
                    SyntaxFactory.SeparatedList<TupleElementSyntax>(
                        new[]
                        {
                            SyntaxFactory.TupleElement(SyntaxFactory.IdentifierName("Type")),
                            SyntaxFactory.TupleElement(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                        }
                    )
                ),
                SyntaxFactory.Identifier("super") // Property name
            )
            .WithModifiers(
                SyntaxFactory.TokenList(
                    new[]
                    {
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.NewKeyword)
                    }
                )
            )
            .WithExpressionBody(
                SyntaxFactory.ArrowExpressionClause(
                    SyntaxFactory.TupleExpression(
                        SyntaxFactory.SeparatedList<ArgumentSyntax>(
                            new ArgumentSyntax[]
                            {
                                SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(
                                    CreateQualifiedName(parser.currentClass.Base) // typeof(MyType)
                                )),
                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()) // this
                            }
                        )
                    )
                )
            )
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            parser.currentClass.Body.Add(superPropertyDeclaration);

            // Add the constructor
            parser.currentClass.Body.Add(CreateConstructor(parser.currentClass.Name));

            // Add __Class and __Static
            AddClassProperties(userDeclaredName);

            // Process class elements
            if (context.classTail().classElement() != null)
            {
                foreach (var element in context.classTail().classElement())
                {
                    var members = Visit(element);
                    if (members == null)
                        continue;
                    if (members is MemberDeclarationSyntax member) { 
                        parser.currentClass.Body.Add(member);
                    }
                }
            }

            // Add static__Init, __Init, and static constructor method (must be after processing the elements for proper field assignments)
            AddInitMethods(parser.currentClass.Name);
            parser.currentClass.Body.Add(CreateStaticConstructor(parser.currentClass.Name));

            if (parser.currentClass.ContainsMethod("__Delete"))
            {
                parser.currentClass.Body.Add(
                    SyntaxFactory.DestructorDeclaration(SyntaxFactory.Identifier(parser.currentClass.Name))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.SingletonList<StatementSyntax>(
                                SyntaxFactory.ExpressionStatement(
                                    ((InvocationExpressionSyntax)InternalMethods.Invoke)
                                    .WithArgumentList(SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(
                                            new[] {
                                                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                                                SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("__Delete")))
                                            }
                                        )
                                    ))
                                )
                            )
                        ))
                    );
            }

            // Add the Call factory method
            if (!parser.currentClass.ContainsMethod("Call", true))
                parser.currentClass.Body.Add(CreateCallFactoryMethod(parser.currentClass.Name));

            var newClass = parser.currentClass.Assemble();

            PopClass();
            return newClass;
        }


        public override SyntaxNode VisitClassTail([NotNull] ClassTailContext context)
        {
            return base.VisitClassTail(context);
        }

        public override SyntaxNode VisitClassPropertyDeclaration([NotNull] ClassPropertyDeclarationContext context)
        {
            var propertyDefinition = context.propertyDefinition();
            var isStatic = context.Static() != null;

            // Determine if this is a regular property or an indexer
            var propertyNameSyntax = propertyDefinition.classPropertyName();

            string propertyName;
            List<ParameterSyntax> indexerParameters = null;

            if (propertyNameSyntax.formalParameterList() != null)
            {
                // Handle indexer property
                propertyName = "Item"; // The name for indexer properties
            }
            else
            {
                // Handle regular property
                propertyName = propertyNameSyntax.identifier().GetText();
            }

            // Generate getter method
            MethodDeclarationSyntax getterMethod = null;
            if (propertyDefinition.propertyGetterDefinition().Length != 0 || propertyDefinition.expression() != null)
            {
                PushFunction((isStatic ? Keywords.ClassStaticPrefix : "") + "get_" + propertyName);

                if (propertyNameSyntax.formalParameterList() != null)
                {
                    indexerParameters = ((ParameterListSyntax)Visit(propertyNameSyntax.formalParameterList())).Parameters.ToList();
                    parser.currentFunc.Params.AddRange(indexerParameters);
                }
                else
                    parser.currentFunc.Params.Add(Parser.ThisParam);

                if (propertyDefinition.propertyGetterDefinition().Length != 0)
                {
                    var getterBody = (BlockSyntax)Visit(propertyDefinition.propertyGetterDefinition(0));
                    parser.currentFunc.Body.AddRange(getterBody.Statements);
                }
                else if (propertyDefinition.expression() != null)
                {
                    var getterBody = SyntaxFactory.Block(SyntaxFactory.ReturnStatement((ExpressionSyntax)Visit(propertyDefinition.expression())));
                    parser.currentFunc.Body.AddRange(getterBody.Statements);
                }

                getterMethod = parser.currentFunc.Assemble();
                PopFunction();
                parser.currentClass.Body.Add(getterMethod);

                var initializerValue = ((InvocationExpressionSyntax)InternalMethods.Func)
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName((isStatic ? Keywords.ClassStaticPrefix : "") + "get_" + propertyName)
                            )
                        )
                    )
                );

                /*
                if (isStatic)
                    parser.currentClass.deferredStaticInitializations.Add((propertyName, "get", initializerValue));
                else
                    parser.currentClass.deferredInitializations.Add((propertyName, "get", initializerValue));
                */
            }

            // Generate setter method
            MethodDeclarationSyntax setterMethod = null;
            if (propertyDefinition.propertySetterDefinition().Length != 0)
            {
                PushFunction((isStatic ? Keywords.ClassStaticPrefix : "") + "set_" + propertyName, SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)));

                if (propertyNameSyntax.formalParameterList() != null)
                {
                    indexerParameters = ((ParameterListSyntax)Visit(propertyNameSyntax.formalParameterList())).Parameters.ToList();

                    var p = indexerParameters[^1];

                    // Check if it's a `params object[]` parameter
                    if (p.Type is ArrayTypeSyntax arrayType &&
                        arrayType.ElementType.IsKind(SyntaxKind.PredefinedType) &&
                        ((PredefinedTypeSyntax)arrayType.ElementType).Keyword.IsKind(SyntaxKind.ObjectKeyword) &&
                        p.Modifiers.Any(SyntaxKind.ParamsKeyword))
                    {
                        // Remove `params` modifier and replace with a normal `object[]`
                        indexerParameters[^1] = p.WithModifiers(SyntaxFactory.TokenList()); // Remove params
                    }
                    // Preserve indexer parameters
                    parser.currentFunc.Params.AddRange(indexerParameters);
                }
                else
                    parser.currentFunc.Params.Add(Parser.ThisParam);

                // Always add `object value` parameter for setters
                parser.currentFunc.Params.Add(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("value"))
                        .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                );

                parser.currentFunc.Void = true;
                var setterBody = (BlockSyntax)Visit(propertyDefinition.propertySetterDefinition(0));
                parser.currentFunc.Body.AddRange(setterBody.Statements);

                setterMethod = parser.currentFunc.Assemble();
                PopFunction();
                parser.currentClass.Body.Add(setterMethod);

                var initializerValue = ((InvocationExpressionSyntax)InternalMethods.Func)
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName((isStatic ? Keywords.ClassStaticPrefix : "") + "set_" + propertyName)
                            )
                        )
                    )
                );

                /*
                if (isStatic)
                    parser.currentClass.deferredStaticInitializations.Add((propertyName, "set", initializerValue));
                else
                    parser.currentClass.deferredInitializations.Add((propertyName, "set", initializerValue));
                */
            }
            return null;
        }

        public override SyntaxNode VisitClassFieldDeclaration([NotNull] ClassFieldDeclarationContext context)
        {
            parser.currentClass.isInitialization = true;
            var fieldNames = new HashSet<string> { };
            var fieldDeclarations = new List<PropertyDeclarationSyntax>();

            var isStatic = context.Static() != null;

            EqualsValueClauseSyntax initializer = null;
            ExpressionSyntax initializerValue = null;

            foreach (var fieldDefinition in context.fieldDefinition())
            {
                var fieldName = fieldDefinition.propertyName().GetText();
                fieldNames.Add(fieldName);

                if (fieldDefinition.expression() != null)
                {
                    initializerValue = (ExpressionSyntax)Visit(fieldDefinition.expression());

                    if (IsLiteralOrConstant(initializerValue))
                    {
                        initializer = SyntaxFactory.EqualsValueClause(initializerValue);
                    }

                    if (isStatic)
                        parser.currentClass.deferredStaticInitializations.Add((fieldName, "value", initializerValue));
                    else
                        parser.currentClass.deferredInitializations.Add((fieldName, "value", initializerValue));
                }

                if (parser.PropertyExistsInBuiltinBase(fieldName) != null)
                    continue;
            }
            parser.currentClass.isInitialization = false;
            return null;
        }


        public override SyntaxNode VisitPropertyGetterDefinition([Antlr4.Runtime.Misc.NotNull] PropertyGetterDefinitionContext context)
        {
            //Console.WriteLine("PropertyGetterDefinition: " + context.GetText());
            return VisitFunctionBody(context.functionBody());
        }

        public override SyntaxNode VisitPropertySetterDefinition([NotNull] PropertySetterDefinitionContext context)
        {
            return (BlockSyntax)VisitFunctionBody(context.functionBody());
        }

        public override SyntaxNode VisitClassMethodDeclaration([NotNull] ClassMethodDeclarationContext context)
        {
            var methodDefinition = context.methodDefinition();
            Visit(methodDefinition.functionHead());
            var rawMethodName = methodDefinition.functionHead().identifier().GetText();
            var methodName = parser.currentFunc.Name = parser.NormalizeClassIdentifier(rawMethodName);

            var fieldName = methodName.ToLowerInvariant();
            var isStatic = context.methodDefinition().functionHead().functionHeadPrefix()?.Static() != null;

            // parser.currentFunc.Static can't be used here, because all user-defined class methods must be non-static
            if (isStatic)
                methodName = Keywords.ClassStaticPrefix + methodName;

            // Visit method body
            BlockSyntax methodBody = (BlockSyntax)Visit(methodDefinition.functionBody());
            parser.currentFunc.Body.AddRange(methodBody.Statements);

            // Create method declaration
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Return type is object
                    SyntaxFactory.Identifier(methodName)
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new List<SyntaxToken> {
                            parser.currentFunc.Async ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default,
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword) }
                        .Where(token => token != default)
                    )
                )
                .WithParameterList(parser.currentFunc.AssembleParams())
            .WithBody(parser.currentFunc.AssembleBody());

            PopFunction();

            return methodDeclaration;
        }

        private ConstructorDeclarationSyntax CreateConstructor(string className)
        {
            return SyntaxFactory.ConstructorDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ParamsKeyword)))
                                .WithType(
                                    SyntaxFactory.ArrayType(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)))
                                        .WithRankSpecifiers(
                                            SyntaxFactory.SingletonList(
                                                SyntaxFactory.ArrayRankSpecifier(
                                                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                                        SyntaxFactory.OmittedArraySizeExpression())
                                                )
                                            )
                                        )
                                )
                        )
                    )
                )
                .WithInitializer(
                    SyntaxFactory.ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(SyntaxFactory.IdentifierName("args"))
                            )
                        )
                    )
                )
                .WithBody(SyntaxFactory.Block());
        }

        private ConstructorDeclarationSyntax CreateStaticConstructor(string className)
        {
            return SyntaxFactory.ConstructorDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        // Script.InitStaticInstance(typeof(Myclass));
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Script"),
                                    SyntaxFactory.IdentifierName("InitStaticInstance")
                                ),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.TypeOfExpression(
                                                SyntaxFactory.IdentifierName(className)
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        // __Static = Variables.Statics[typeof(Myclass)];
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.IdentifierName("__Static"),
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
                                                    SyntaxFactory.IdentifierName(className)
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        // SetPropertyValue(Variables.Prototypes[typeof(ClassName)], "__Class", "UserDeclaredClassname");
                        SyntaxFactory.ExpressionStatement(
                            ((InvocationExpressionSyntax)InternalMethods.SetPropertyValue)
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(new[]
                                    {
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.ElementAccessExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName("Variables"),
                                                    SyntaxFactory.IdentifierName("Prototypes")
                                                ),
                                                SyntaxFactory.BracketedArgumentList(
                                                    SyntaxFactory.SingletonSeparatedList(
                                                        SyntaxFactory.Argument(
                                                            SyntaxFactory.TypeOfExpression(
                                                                SyntaxFactory.IdentifierName(className)
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        ),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal("__Class")
                                            )
                                        ),
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(
                                                    parser.ClassStack.Count == 1 ? parser.currentClass.UserDeclaredName :
                                                    string.Join(".",
                                                        parser.ClassStack.Reverse().Skip(1)
                                                        .Select(cls => cls.UserDeclaredName)
                                                    ) + "." + parser.currentClass.UserDeclaredName
                                                )
                                            )
                                        )
                                    })
                                )
                            )
                        )
                    )
                );
        }

        private void AddInitMethods(string className)
        {
            // Check if instance __Init method already exists
            var instanceInitName = "__Init";
            if (!parser.currentClass.ContainsMethod(instanceInitName, default, true))
            {
                // Instance __Init method
                var instanceInitMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    instanceInitName
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(Parser.ThisParam)))
                .WithBody(
                    SyntaxFactory.Block(
                        new[] // Add the call to Invoke((object)super, "__Init") as the first statement
                        {
                    SyntaxFactory.ExpressionStatement(
                        ((InvocationExpressionSyntax)InternalMethods.Invoke)
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(
                                        parser.CreateSuperTuple()
                                    ),
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("__Init")
                                        )
                                    )
                                })
                            )
                        )
                    )
                        }.Concat(
                            parser.currentClass.deferredInitializations.Select(deferred =>
                                SyntaxFactory.ExpressionStatement(
                                ((InvocationExpressionSyntax)InternalMethods.SetPropertyValue)
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(new[] {
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("@this")),
                                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(deferred.Item1))),
                                            SyntaxFactory.Argument(deferred.Item3)
                                        })
                                    )
                                )
                                )
                            )
                        )
                    )
                );

                parser.currentClass.Body.Add(instanceInitMethod);
            }

            // Check if static __Init method already exists
            if (!parser.currentClass.ContainsMethod("__Init", true, true))
            {
                // Static __Init method
                var staticInitMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    Keywords.ClassStaticPrefix + "__Init"
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(Parser.ThisParam)))
                .WithBody(
                    SyntaxFactory.Block(
                        parser.currentClass.deferredStaticInitializations.Select(deferred =>
                            SyntaxFactory.ExpressionStatement(
                                ((InvocationExpressionSyntax)InternalMethods.SetPropertyValue)
                                                .WithArgumentList(
                                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(new[] {
                                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("@this")),
                                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(deferred.Item1))),
                                            SyntaxFactory.Argument(deferred.Item3)
                                        })
                                    )
                                )
                            )
                        )
                    )
                );

                parser.currentClass.Body.Add(staticInitMethod);
            }
        }

        private void AddClassProperties(string className)
        {
            /*
            parser.currentClass.Body.Add(
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    "__Class"
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithBody(
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ReturnStatement(
                                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(className))
                                        )
                                    )
                                )
                        )
                    )
                )
            );
            */

            parser.currentClass.Body.Add(
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.ParseTypeName("object"), // Type of the property
                    "__Static" // Property name
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.NewKeyword),
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                    )
                )
                .WithAccessorList(
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List(new[]
                        {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        })
                    )
                )
            );
        }

        private MethodDeclarationSyntax CreateCallFactoryMethod(string className)
        {
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.IdentifierName(className),
                    Keywords.ClassStaticPrefix + "Call"
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                    )
                )
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList(
                            new[] {
                                Parser.ThisParam,
                                VariadicParam
                            }
                        )
                    )
                )
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.ObjectCreationExpression(
                                SyntaxFactory.IdentifierName(className),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("args")))
                                ),
                                null
                            )
                        )
                    )
                );
        }

        private PropertyDeclarationSyntax CreateFieldDeclaration(string fieldName, bool isStatic)
        {
            if (isStatic && !fieldName.StartsWith(Keywords.ClassStaticPrefix))
                fieldName = Keywords.ClassStaticPrefix + fieldName;
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Type is object
                SyntaxFactory.Identifier(fieldName)
            )
            .WithModifiers(
                SyntaxFactory.TokenList(
                    new List<SyntaxToken>
                    {
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                    }
                    .Where(token => token != default)
                )
            )
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    SyntaxFactory.List(
                        new[]
                        {
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        }
                    )
                )
            );
        }


        private void PushClass(string className, string baseName = "KeysharpObject")
        {
            parser.ClassStack.Push(parser.currentClass);
            parser.classDepth++;
            parser.currentClass = new Class(className, baseName);
        }

        private void PopClass()
        {
            parser.currentClass = parser.ClassStack.Pop();
            parser.classDepth--;
        }
    }
}