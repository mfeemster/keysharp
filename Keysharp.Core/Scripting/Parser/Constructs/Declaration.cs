using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Keysharp.Scripting
{
	internal partial class Parser
	{
		public class Class
		{
			public int Indent = 0;
			public string Name = null;
			public string UserDeclaredName = null;
			public string Base = "KeysharpObject";
			public List<BaseTypeSyntax> BaseList = new();
			public List<MemberDeclarationSyntax> Body = new List<MemberDeclarationSyntax>();
			public List<AttributeSyntax> Attributes = new();
			public ClassDeclarationSyntax Declaration = null;

			public int lastCheckedBodyCount = 0;
			public HashSet<string> cachedFieldNames = new();

			public bool isInitialization = false;
			public readonly List<(ExpressionSyntax BaseExpr, ExpressionSyntax TargetExpr, ExpressionSyntax Initializer)> deferredInitializations = new();
			public readonly List<(ExpressionSyntax BaseExpr, ExpressionSyntax TargetExpr, ExpressionSyntax Initializer)> deferredStaticInitializations = new();

			public Class(string name, string baseName = "KeysharpObject")
			{
				if (string.IsNullOrWhiteSpace(name))
					throw new ArgumentException("Name cannot be null or empty.", nameof(name));

				Name = name;
				Declaration = SyntaxFactory.ClassDeclaration(
					modifiers: SyntaxFactory.TokenList(PredefinedKeywords.PublicToken),
					identifier: SyntaxFactory.Identifier(Name),
					attributeLists: default,
					typeParameterList: null,
					baseList: null,
					constraintClauses: default,
					members: default
					);

				if (baseName != null)
					Declaration = Declaration.WithBaseList(
						SyntaxFactory.BaseList(
							SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
								SyntaxFactory.SimpleBaseType(CreateQualifiedName(baseName))
							)
						)
					);
			}
			public AttributeListSyntax AssembleAttributes() => SyntaxFactory.AttributeList(
			SyntaxFactory.SeparatedList<AttributeSyntax>(
				Attributes));

			public ClassDeclarationSyntax Assemble()
			{
				if (UserDeclaredName != null && UserDeclaredName != Name)
				{
					var value = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(UserDeclaredName));
					var nameAttr = SyntaxFactory.Attribute(
						SyntaxFactory.IdentifierName("UserDeclaredName"),
						SyntaxFactory.AttributeArgumentList(
							SyntaxFactory.SingletonSeparatedList(
								SyntaxFactory.AttributeArgument(value)
							)
						)
					);
					Attributes.Add(nameAttr);
				}
				if (Attributes.Count > 0)
				{
					var attributeList = new SyntaxList<AttributeListSyntax>(AssembleAttributes());
					Declaration = Declaration.WithAttributeLists(attributeList);
				}
				return Declaration
					.WithBaseList(BaseList.Count > 0 ? SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(BaseList)) : default)
					.AddMembers(Body.ToArray());
			}

			public bool ContainsMethod(string methodName, bool searchStatic = false, bool caseSensitive = false)
			{
				if (Body == null) throw new ArgumentNullException(nameof(Body));
				if (string.IsNullOrEmpty(methodName)) throw new ArgumentException("Method name cannot be null or empty", nameof(methodName));

				// Adjust string comparison based on case-sensitivity
				var stringComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

				// Search for methods
				foreach (var member in Body)
				{
					if (member is MethodDeclarationSyntax method)
					{
						var methName = method.Identifier.Text;
						bool isStatic = false;
						if (methName.StartsWith(Keywords.ClassStaticPrefix))
						{
							methName = methName.Substring(Keywords.ClassStaticPrefix.Length);
							isStatic = true;
						}
						// Check method name
						if (string.Equals(methName, methodName, stringComparison))
						{
							if (isStatic == searchStatic) return true;
						}
					}
				}

				return false;
			}
		}
		public void PushClass(string className, string baseName = "KeysharpObject")
		{
			ClassStack.Push(currentClass);
			classDepth++;
			currentClass = new Class(className, baseName);
		}

		public void PopClass()
		{
			currentClass = ClassStack.Pop();
			classDepth--;
		}
	}
}
