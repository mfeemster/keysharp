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
    public partial class MainVisitor : MainParserBaseVisitor<SyntaxNode>
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
            var prevClass = parser.currentClass;
            parser.currentClass = new Parser.Class(Parser.NormalizeIdentifier(context.identifier().GetText(), eNameCase.Title));

            // Determine the base class (Extends clause)
            BaseListSyntax baseList;
            if (context.Extends() != null)
            {
                var extendsParts = context.classExtensionName().identifier();
                var baseClassName = parser.NormalizeClassIdentifier(extendsParts[0].GetText(), eNameCase.Title);
                if (Reflections.stringToTypes.ContainsKey(baseClassName)) {
                    baseClassName = Reflections.stringToTypes.First(pair =>  pair.Key.Equals(baseClassName, StringComparison.InvariantCultureIgnoreCase)).Key;
                    parser.currentClass.Base = baseClassName;
                }
                for (int i = 1; i < extendsParts.Length; i++)
                {
                    baseClassName += "." + parser.NormalizeClassIdentifier(extendsParts[i].GetText(), eNameCase.Title);
                }
                baseList = SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(CreateQualifiedName(baseClassName))
                    )
                );
            }
            else
            {
                // Default base class is KeysharpObject
                baseList = SyntaxFactory.BaseList(
                    SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                        SyntaxFactory.SimpleBaseType(CreateQualifiedName("KeysharpObject"))
                    )
                );
            }

            // Add `public static object myclass = Myclass.__Static;` global field
            var fieldDeclaration = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.ParseTypeName("object"),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(parser.currentClass.Name.ToLower())
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(parser.currentClass.Name),
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

            parser.mainClass.Declaration = parser.mainClass.Declaration.AddMembers(fieldDeclaration);

            // Add the constructor
            parser.currentClass.Body.Add(CreateConstructor(parser.currentClass.Name));

            // Add __Class and __Static
            AddClassProperties(context.identifier().GetText());

            // Process class elements
            if (context.classTail().classElement() != null)
            {
                foreach (var element in context.classTail().classElement())
                {
                    var member = Visit(element) as MemberDeclarationSyntax;
                    if (member != null)
                    {
                        parser.currentClass.Body.Add(member);
                    }
                }
            }

            // Add static__Init, __Init, and static constructor method (must be after processing the elements for proper field assignments)
            AddInitMethods(parser.currentClass.Name);
            parser.currentClass.Body.Add(CreateStaticConstructor(parser.currentClass.Name));

            // Add the Call factory method
            if (!parser.currentClass.ContainsMethod(Keywords.ClassStaticPrefix + "Call"))
                parser.currentClass.Body.Add(CreateCallFactoryMethod(parser.currentClass.Name));

            var newClass = parser.currentClass.Declaration
                .WithBaseList(baseList)
                .WithMembers(SyntaxFactory.List(parser.currentClass.Declaration.Members.Concat(parser.currentClass.Body)));
            parser.currentClass = prevClass;
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
            BracketedParameterListSyntax indexerParameters = null;

            if (propertyNameSyntax.formalParameterList() != null)
            {
                // Handle indexer property
                propertyName = "__Item"; // The name for indexer properties
                indexerParameters = SyntaxFactory.BracketedParameterList(
                    SyntaxFactory.SeparatedList<ParameterSyntax>(
                        ((ParameterListSyntax)Visit(propertyNameSyntax.formalParameterList())).Parameters
                    )
                );
            }
            else
            {
                // Handle regular property
                propertyName = parser.NormalizeClassIdentifier(propertyNameSyntax.identifier().GetText());
            }

            // Visit getter
            AccessorDeclarationSyntax getter = null;
            if (propertyDefinition.propertyGetterDefinition().Length != 0)
            {
                PushFunction("get_" + propertyNameSyntax.identifier().GetText());
                var getterBody = (BlockSyntax)Visit(propertyDefinition.propertyGetterDefinition(0));
                parser.currentFunc.Body.AddRange(getterBody.Statements.ToArray());
                getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(parser.currentFunc.AssembleBody());
                PopFunction();
            } else if (propertyDefinition.expression() != null)
            {
                PushFunction("get_" + propertyNameSyntax.identifier().GetText());
                var getterBody = SyntaxFactory.Block(SyntaxFactory.ReturnStatement((ExpressionSyntax)Visit(propertyDefinition.expression())));
                parser.currentFunc.Body.AddRange(getterBody.Statements);
                getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(parser.currentFunc.AssembleBody());
                PopFunction();
            }

            // Visit setter
            AccessorDeclarationSyntax setter = null;
            if (propertyDefinition.propertySetterDefinition().Length != 0)
            {
                PushFunction("set_" + propertyNameSyntax.identifier().GetText());
                parser.currentFunc.Void = true;
                var setterBody = (BlockSyntax)Visit(propertyDefinition.propertySetterDefinition(0));
                parser.currentFunc.Body.AddRange(setterBody.Statements);
                setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(parser.currentFunc.AssembleBody());
                PopFunction();
            }

            // Create property declaration
            if (indexerParameters != null)
            {
                // Create indexer declaration
                return SyntaxFactory.IndexerDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)) // Type is object
                    )
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new List<SyntaxToken> { isStatic ? SyntaxFactory.Token(SyntaxKind.StaticKeyword) : default,
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword) }
                            .Where(token => token != default)
                        )
                    )
                    .WithParameterList(indexerParameters)
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List(
                                new[] { getter, setter }.Where(accessor => accessor != null)
                            )
                        )
                    );
            }
            else
            {
                // Create regular property declaration
                return SyntaxFactory.PropertyDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Type is object
                        SyntaxFactory.Identifier(propertyName)
                    )
                    .WithModifiers(
                        SyntaxFactory.TokenList(
                            new List<SyntaxToken> { isStatic ? SyntaxFactory.Token(SyntaxKind.StaticKeyword) : default,
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword) }
                            .Where(token => token != default)
                        )
                    )
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List(
                                new[] { getter, setter }.Where(accessor => accessor != null)
                            )
                        )
                    );
            }
        }

        public override SyntaxNode VisitClassFieldDeclaration([NotNull] ClassFieldDeclarationContext context)
        {
            var fieldNames = new HashSet<string> { };
            var fieldDeclarations = new List<PropertyDeclarationSyntax>();

            var isStatic = context.Static() != null;

            EqualsValueClauseSyntax initializer = null;
            ExpressionSyntax initializerValue = null;

            foreach (var fieldDefinition in context.fieldDefinition())
            {
                var fieldName = fieldDefinition.propertyName().GetText();
                fieldNames.Add(parser.NormalizeClassIdentifier(fieldName));

                if (fieldDefinition.expression() != null)
                {
                    initializerValue = (ExpressionSyntax)Visit(fieldDefinition.expression());

                    if (IsLiteralOrConstant(initializerValue))
                    {
                        initializer = SyntaxFactory.EqualsValueClause(initializerValue);
                    }

                    if (isStatic)
                        parser.currentClass.deferredStaticInitializations.Add((fieldName, initializerValue));
                    else
                        parser.currentClass.deferredInitializations.Add((fieldName, initializerValue));
                }

                if (parser.PropertyExistsInBuiltinBase(fieldName) != null)
                    continue;

                var propertyDeclaration = CreateFieldDeclaration(fieldName, isStatic);

                parser.currentClass.Body.Add(propertyDeclaration);
            }

            return null;
        }


        public override SyntaxNode VisitPropertyGetterDefinition([Antlr4.Runtime.Misc.NotNull] PropertyGetterDefinitionContext context)
        {
            //Console.WriteLine("PropertyGetterDefinition: " + context.GetText());
            return VisitFunctionBody(context.functionBody());
        }

        public override SyntaxNode VisitPropertySetterDefinition([NotNull] PropertySetterDefinitionContext context)
        {
            return EnsureNoReturnStatement((BlockSyntax)VisitFunctionBody(context.functionBody()));
        }

        public override SyntaxNode VisitClassMethodDeclaration([NotNull] ClassMethodDeclarationContext context)
        {
            var methodDefinition = context.methodDefinition();
            Visit(methodDefinition.functionHead());
            var rawMethodName = methodDefinition.functionHead().identifier().GetText();
            var methodName = parser.currentFunc.Name = parser.NormalizeClassIdentifier(rawMethodName, eNameCase.Title);

            var fieldName = methodName.ToLowerInvariant();
            var isStatic = context.Static() != null;

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
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)}
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
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        new[] // Add the call to Invoke((object)super, "__Init") as the first statement
                        {
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName("Invoke"),
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SeparatedList(new[]
                                {
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.CastExpression(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                                            SyntaxFactory.IdentifierName("Super")
                                        )
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
                                    SyntaxFactory.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        SyntaxFactory.IdentifierName(deferred.Item1),
                                        deferred.Item2
                                    )
                                )
                            )
                        )
                    )
                );

                parser.currentClass.Body.Add(instanceInitMethod);
            }

            // Check if static __Init method already exists
            if (!parser.currentClass.ContainsMethod(Keywords.ClassStaticPrefix + "__Init", default, true))
            {
                // Static __Init method
                var staticInitMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    Keywords.ClassStaticPrefix + "__Init"
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        parser.currentClass.deferredStaticInitializations.Select(deferred =>
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.IdentifierName(deferred.Item1),
                                    deferred.Item2
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
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)
                    )
                )
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            VariadicParam
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
            return SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Type is object
                SyntaxFactory.Identifier(fieldName)
            )
            .WithModifiers(
                SyntaxFactory.TokenList(
                    new List<SyntaxToken>
                    {
                isStatic ? SyntaxFactory.Token(SyntaxKind.StaticKeyword) : default,
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

    }
}