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
            var prevClass = currentClass;
            currentClass = new Class(context.identifier().GetText().ToLowerInvariant());

            // Determine the base class (Extends clause)
            BaseListSyntax baseList;
            if (context.classTail().Extends() != null)
            {
                var extendsParts = context.classTail().identifier();
                var baseClassName = NormalizeName(extendsParts[0].GetText());
                if (Reflections.stringToTypes.ContainsKey(baseClassName)) {
                    baseClassName = Reflections.stringToTypes.First(pair =>  pair.Key.Equals(baseClassName, StringComparison.InvariantCultureIgnoreCase)).Key;
                    currentClass.Base = baseClassName;
                }
                for (int i = 1; i < extendsParts.Length; i++)
                {
                    baseClassName += "." + NormalizeName(extendsParts[i].GetText());
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

            // Add the constructor
            currentClass.Body.Add(CreateConstructor(currentClass.Name));

            // Add the __Class property
            currentClass.Body.Add(CreateClassProperty(currentClass.Name));

            // Process class elements
            if (context.classTail().classElement() != null)
            {
                foreach (var element in context.classTail().classElement())
                {
                    var member = Visit(element) as MemberDeclarationSyntax;
                    if (member != null)
                    {
                        currentClass.Body.Add(member);
                    }
                }
            }

            // Add the __Init and static constructor method (must be after processing the elements for proper field assignments)
            currentClass.Body.Add(CreateInitMethod(context.classTail()));
            currentClass.Body.Add(CreateStaticConstructor(currentClass.Name));

            // Add the Call factory method
            currentClass.Body.Add(CreateCallFactoryMethod(currentClass.Name));

            AddFuncObjField("__init", "__Init", false);
            AddFuncObjField("call", "Call", true);

            var newClass = currentClass.Declaration
                .WithBaseList(baseList)
                .WithMembers(SyntaxFactory.List(currentClass.Body));
            currentClass = prevClass;
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
                propertyName = NormalizeName(propertyNameSyntax.identifier().GetText());
            }

            // Visit getter
            AccessorDeclarationSyntax getter = null;
            if (propertyDefinition.propertyGetterDefinition() != null)
            {
                var getterBody = (BlockSyntax)Visit(propertyDefinition.propertyGetterDefinition());
                getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(getterBody);
            }

            // Visit setter
            AccessorDeclarationSyntax setter = null;
            if (propertyDefinition.propertySetterDefinition() != null)
            {
                var setterBody = (BlockSyntax)Visit(propertyDefinition.propertySetterDefinition());
                setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(setterBody);
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

            var fieldNames = new List<string> { };

            foreach (var property in fieldDefinition.propertyName())
                fieldNames.Add(NormalizeName(property.GetText()));

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
                var propertyDeclaration = CreateFieldDeclaration(fieldName, isStatic);

                // Defer initialization to __Init
                if (initializerValue != null)
                {
                    if (isStatic)
                        currentClass.deferredStaticInitializations.Add((fieldName, initializerValue));
                    else
                        currentClass.deferredInitializations.Add((fieldName, initializerValue));
                }

                currentClass.Body.Add(propertyDeclaration);
                //fieldDeclarations.Add(propertyDeclaration);
            }
            return null;
        }


        public override SyntaxNode VisitPropertyGetterDefinition([Antlr4.Runtime.Misc.NotNull] PropertyGetterDefinitionContext context)
        {
            //Console.WriteLine("PropertyGetterDefinition: " + context.GetText());
            return VisitLambdaFunctionBody(context.lambdaFunctionBody());
        }

        public override SyntaxNode VisitClassMethodDeclaration([NotNull] ClassMethodDeclarationContext context)
        {
            var methodDefinition = context.methodDefinition();
            var methodName = NormalizeName(methodDefinition.propertyName().GetText(), NameCase.Title);
            var fieldName = methodName.ToLowerInvariant();
            var isStatic = context.Static() != null;
            var isAsync = methodDefinition.Async() != null;

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
                        new List<SyntaxToken> {isStatic ? SyntaxFactory.Token(SyntaxKind.StaticKeyword) : default,
                        isAsync ? SyntaxFactory.Token(SyntaxKind.AsyncKeyword) : default,
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)}
                        .Where(token => token != default)
                    )
                )
                .WithParameterList(parameters)
            .WithBody(methodBody);

            AddFuncObjField(fieldName.ToLowerInvariant(), methodName, isStatic);

            return methodDeclaration;
        }

        private void AddFuncObjField(string fieldName, string methodName, bool isStatic)
        {
            currentClass.Body.Add(CreateFieldDeclaration(fieldName, isStatic));

            // Defer the initialization to the correct list
            var initializationExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("FuncObj"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal(methodName)
                            )
                        ),
                        SyntaxFactory.Argument(isStatic
                            ? SyntaxFactory.TypeOfExpression(SyntaxFactory.ParseTypeName(currentClass.Name))
                            : SyntaxFactory.ThisExpression())
                    }
                    )
                )
            );

            if (isStatic)
                currentClass.deferredStaticInitializations.Insert(0, (fieldName, initializationExpression));
            else
                currentClass.deferredInitializations.Insert(0, (fieldName, initializationExpression));
        }

        private ConstructorDeclarationSyntax CreateConstructor(string className)
        {
            return SyntaxFactory.ConstructorDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            VariadicParam
                        )
                    )
                )
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName("__New"),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("args")))
                                )
                            )
                        )
                    )
                );
        }

        private ConstructorDeclarationSyntax CreateStaticConstructor(string className)
        {
            var bodyStatements = new List<StatementSyntax> { };
            // Add deferred initializations
            foreach (var (fieldName, initializer) in currentClass.deferredStaticInitializations)
            {
                bodyStatements.Add(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(fieldName),
                            initializer
                        )
                    )
                );
            }
            bodyStatements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("__StaticInit"))
                )
            );
            return SyntaxFactory.ConstructorDeclaration(className)
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                .WithBody(
                    SyntaxFactory.Block(
                        bodyStatements
                    )
                );
        }

        enum NameCase
        {
            Lower,
            Upper,
            Title
        };

        private static string NormalizeName(string name, NameCase nameCase = NameCase.Lower)
        {
            // Define the reserved keywords that should retain their original case
            var reservedKeywords = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "Call", "__New", "__Init", "__Get", "__Set", "__Item", "__Class", "__StaticInit"
            };

            name = name.Trim('"');

            if (nameCase == NameCase.Lower)
                return name.ToLowerInvariant();
            else if (nameCase == NameCase.Title)
            {
                if (reservedKeywords.Contains(name))
                {
                    return reservedKeywords.First(keyword => string.Equals(keyword, name, StringComparison.InvariantCultureIgnoreCase));
                }
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            }
            else
                return name.ToUpperInvariant();
        }


        private MethodDeclarationSyntax CreateInitMethod(ClassTailContext context)
        {
            var bodyStatements = new List<StatementSyntax>
            {
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.BaseExpression(),
                            SyntaxFactory.IdentifierName("__Init")
                        )
                    )
                )
            };

            // Add deferred initializations
            foreach (var (fieldName, initializer) in currentClass.deferredInitializations)
            {
                bodyStatements.Add(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(fieldName),
                            initializer
                        )
                    )
                );
            }

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "__Init"
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
                .WithBody(SyntaxFactory.Block(bodyStatements));
        }

        private PropertyDeclarationSyntax CreateClassProperty(string className)
        {
            return SyntaxFactory.PropertyDeclaration(
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
                );
        }

        private MethodDeclarationSyntax CreateCallFactoryMethod(string className)
        {
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.IdentifierName(className),
                    "Call"
                )
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
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