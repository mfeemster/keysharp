using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Antlr4.Runtime.Misc;
using static Keysharp.Core.Scripting.Parser.Antlr.Helper;
using static MainParser;
using Microsoft.CodeAnalysis;
using System.Drawing.Imaging;
using Antlr4.Runtime;
using System.IO;
using System.Collections;
using System.Reflection;

namespace Keysharp.Core.Scripting.Parser.Antlr
{
    public partial class MainVisitor : MainParserBaseVisitor<SyntaxNode>
    {
        public override SyntaxNode VisitClassDeclaration([NotNull] ClassDeclarationContext context)
        {
            var prevClass = state.currentClass;
            state.currentClass = new Class(NormalizeIdentifier(context.identifier().GetText(), NameCase.Title));

            // Determine the base class (Extends clause)
            BaseListSyntax baseList;
            if (context.classTail().Extends() != null)
            {
                var extendsParts = context.classTail().identifier();
                var baseClassName = NormalizeClassIdentifier(extendsParts[0].GetText(), NameCase.Title);
                if (Reflections.stringToTypes.ContainsKey(baseClassName)) {
                    baseClassName = Reflections.stringToTypes.First(pair =>  pair.Key.Equals(baseClassName, StringComparison.InvariantCultureIgnoreCase)).Key;
                    state.currentClass.Base = baseClassName;
                }
                for (int i = 1; i < extendsParts.Length; i++)
                {
                    baseClassName += "." + NormalizeClassIdentifier(extendsParts[i].GetText(), NameCase.Title);
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
                        SyntaxFactory.VariableDeclarator(state.currentClass.Name.ToLower())
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName(state.currentClass.Name),
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

            state.mainClass = state.mainClass.AddMembers(fieldDeclaration);

            // Add the constructor
            state.currentClass.Body.Add(CreateConstructor(state.currentClass.Name));

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
                        state.currentClass.Body.Add(member);
                    }
                }
            }

            // Add static__Init, __Init, and static constructor method (must be after processing the elements for proper field assignments)
            AddInitMethods(state.currentClass.Name);
            state.currentClass.Body.Add(CreateStaticConstructor(state.currentClass.Name));

            // Add the Call factory method
            if (!state.currentClass.ContainsMethod(Helper.Keywords.ClassStaticPrefix + "Call"))
                state.currentClass.Body.Add(CreateCallFactoryMethod(state.currentClass.Name));

            var newClass = state.currentClass.Declaration
                .WithBaseList(baseList)
                .WithMembers(SyntaxFactory.List(state.currentClass.Body));
            state.currentClass = prevClass;
            return newClass;
        }


        public override SyntaxNode VisitClassTail([NotNull] ClassTailContext context)
        {
            return base.VisitClassTail(context);
        }

        public override SyntaxNode VisitClassExpression([NotNull] ClassExpressionContext context)
        {
            return base.VisitClassExpression(context);
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
                propertyName = NormalizeClassIdentifier(propertyNameSyntax.identifier().GetText());
            }

            // Visit getter
            AccessorDeclarationSyntax getter = null;
            if (propertyDefinition.propertyGetterDefinition().Length != 0)
            {
                PushFunction("get_" + propertyNameSyntax.identifier().GetText());
                var getterBody = (BlockSyntax)Visit(propertyDefinition.propertyGetterDefinition(0));
                state.currentFunc.Body = EnsureReturnStatement(state.currentFunc.Body.AddStatements(getterBody.Statements.ToArray()));
                getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(state.currentFunc.Body);
                PopFunction();
            } else if (propertyDefinition.singleExpression() != null)
            {
                PushFunction("get_" + propertyNameSyntax.identifier().GetText());
                var getterBody = SyntaxFactory.Block(SyntaxFactory.ReturnStatement((ExpressionSyntax)Visit(propertyDefinition.singleExpression())));
                state.currentFunc.Body = state.currentFunc.Body.AddStatements(getterBody.Statements.ToArray());
                getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(state.currentFunc.Body);
                PopFunction();
            }

            // Visit setter
            AccessorDeclarationSyntax setter = null;
            if (propertyDefinition.propertySetterDefinition().Length != 0)
            {
                PushFunction("set_" + propertyNameSyntax.identifier().GetText());
                var setterBody = (BlockSyntax)Visit(propertyDefinition.propertySetterDefinition(0));
                state.currentFunc.Body = state.currentFunc.Body.AddStatements(setterBody.Statements.ToArray());
                setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(state.currentFunc.Body);
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
            var fieldDefinition = context.fieldDefinition();

            var fieldNames = new HashSet<string> { };

            foreach (var property in fieldDefinition.propertyName())
                fieldNames.Add(NormalizeClassIdentifier(property.GetText()));

            var isStatic = context.Static() != null;

            // Visit initializer if present
            EqualsValueClauseSyntax initializer = null;
            ExpressionSyntax initializerValue = null;
            if (fieldDefinition.initializer() != null)
            {
                initializerValue = (ExpressionSyntax)Visit(fieldDefinition.initializer().singleExpression());

                if (IsLiteralOrConstant(initializerValue))
                {
                    initializer = SyntaxFactory.EqualsValueClause(initializerValue);
                }
            }

            // Create property declarations for all fields
            var fieldDeclarations = new List<PropertyDeclarationSyntax>();
            foreach (var fieldName in fieldNames)
            {
                // Defer initialization to __Init
                if (initializerValue != null)
                {
                    if (isStatic)
                        state.currentClass.deferredStaticInitializations.Add((fieldName, initializerValue));
                    else
                        state.currentClass.deferredInitializations.Add((fieldName, initializerValue));
                }

                if (PropertyExistsInBuiltinBase(fieldName) != null)
                    continue;

                var propertyDeclaration = CreateFieldDeclaration(fieldName, isStatic);

                state.currentClass.Body.Add(propertyDeclaration);
                //fieldDeclarations.Add(propertyDeclaration);
            }
            return null;
        }


        public override SyntaxNode VisitPropertyGetterDefinition([Antlr4.Runtime.Misc.NotNull] PropertyGetterDefinitionContext context)
        {
            //Console.WriteLine("PropertyGetterDefinition: " + context.GetText());
            return VisitLambdaFunctionBody(context.lambdaFunctionBody());
        }

        public override SyntaxNode VisitPropertySetterDefinition([NotNull] PropertySetterDefinitionContext context)
        {
            return EnsureNoReturnStatement((BlockSyntax)VisitLambdaFunctionBody(context.lambdaFunctionBody()));
        }

        public override SyntaxNode VisitClassMethodDeclaration([NotNull] ClassMethodDeclarationContext context)
        {
            var methodDefinition = context.methodDefinition();
            var rawMethodName = methodDefinition.propertyName().GetText();
            var methodName = NormalizeClassIdentifier(rawMethodName, NameCase.Title);
            PushFunction(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(rawMethodName));
            var fieldName = methodName.ToLowerInvariant();
            var isStatic = context.Static() != null;
            var isAsync = methodDefinition.Async() != null;

            if (isStatic)
                methodName = Helper.Keywords.ClassStaticPrefix + methodName;

            // Visit formal parameters
            ParameterListSyntax parameters = SyntaxFactory.ParameterList();
            if (methodDefinition.formalParameterList() != null)
            {
                parameters = (ParameterListSyntax)Visit(methodDefinition.formalParameterList());
            }

            // Visit method body
            BlockSyntax methodBody = (BlockSyntax)Visit(methodDefinition.lambdaFunctionBody());
            methodBody = EnsureReturnStatement(methodBody);

            // Create method declaration
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), // Return type is object
                    SyntaxFactory.Identifier(methodName)
                )
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        new List<SyntaxToken> {
                            isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default,
                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)}
                        .Where(token => token != default)
                    )
                )
                .WithParameterList(parameters)
            .WithBody(methodBody);

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
            if (!state.currentClass.ContainsMethod(instanceInitName, default, true))
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
                            state.currentClass.deferredInitializations.Select(deferred =>
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

                state.currentClass.Body.Add(instanceInitMethod);
            }

            // Check if static __Init method already exists
            if (!state.currentClass.ContainsMethod(Helper.Keywords.ClassStaticPrefix + "__Init", default, true))
            {
                // Static __Init method
                var staticInitMethod = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    Helper.Keywords.ClassStaticPrefix + "__Init"
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        state.currentClass.deferredStaticInitializations.Select(deferred =>
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

                state.currentClass.Body.Add(staticInitMethod);
            }
        }

        private void AddClassProperties(string className)
        {
            state.currentClass.Body.Add(
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

            state.currentClass.Body.Add(
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
                    Helper.Keywords.ClassStaticPrefix + "Call"
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