namespace Keysharp.Core.Common.ExtensionMethods
{
	/// <summary>
	/// Extension methods for various CodeDOM classes.
	/// </summary>
	internal static class CodeDomExtensions
	{
		/// <summary>
		/// Returns the Expression property of a <see cref="CodeObject"/> if it was a <see cref="CodeExpressionStatement"/>.
		/// </summary>
		/// <param name="co">The <see cref="CodeObject"/> to examine.</param>
		/// <returns>A <see cref="CodeExpression"/> if the cast succeeded, else co cast as a <see cref="CodeExpression"/>.</returns>
		internal static CodeExpression GetExpr(this CodeObject co) => co is CodeExpressionStatement ces ? ces.Expression : co as CodeExpression;

		//internal static CodeBinaryOperatorExpression IsAssignExpr(this CodeExpression expr) => expr is CodeBinaryOperatorExpression cboe && cboe.Operator == CodeBinaryOperatorType.Assign ? cboe : null;

		//internal static CodeSnippetExpression ParentSnippet(this CodeExpression expr)
		//{
		//  if (expr == null)
		//      return null;
		//
		//  if (expr.UserData.Contains("snippet") && expr.UserData["snippet"] is CodeSnippetExpression cse)
		//      return cse;
		//
		//  return null;
		//}

		/// <summary>
		/// Determines whether a <see cref="CodeExpression"/> is a <see cref="CodeBinaryOperatorExpression"/> or if its UserData
		/// property contained one.
		/// </summary>
		/// <param name="expr">The expression to examine.</param>
		/// <returns>A <see cref="CodeBinaryOperatorExpression"/> if one was found, else null.</returns>
		internal static CodeBinaryOperatorExpression WasCboe(this CodeExpression expr)
		{
			if (expr == null)
				return null;

			if (expr is CodeBinaryOperatorExpression cboe)
				return cboe;

			if (expr.UserData.Contains("orig") && expr.UserData["orig"] is CodeBinaryOperatorExpression cboe2)
				return cboe2;

			return null;
		}

		/// <summary>
		/// Determines whether a <see cref="CodeExpression"/> is a <see cref="CodeBinaryOperatorExpression"/> and its Operator
		/// property is Assign.
		/// </summary>
		/// <param name="expr">The expression to examine.</param>
		/// <returns>A <see cref="CodeBinaryOperatorExpression"/> if one was found, else null.</returns>
		internal static CodeBinaryOperatorExpression WasCboeAssign(this CodeExpression expr)
		{
			if (expr == null)
				return null;

			if (expr.WasCboe() is CodeBinaryOperatorExpression cboe && cboe.Operator == CodeBinaryOperatorType.Assign)
				return cboe;

			return null;
		}

		internal static bool IsVariadic(this CodeParameterDeclarationExpression p) => p != null && p.CustomAttributes.Cast<CodeAttributeDeclaration>().Any(cad => cad.Name == "System.ParamArrayAttribute");

		internal static bool IsOptional(this CodeParameterDeclarationExpression p) => p != null && p.CustomAttributes.Cast<CodeAttributeDeclaration>().Any(cad => cad.Name == "DefaultParameterValue");

		internal static bool IsOptionalOrVariadic(this CodeParameterDeclarationExpression p) => p != null && p.CustomAttributes.Cast<CodeAttributeDeclaration>().Any(cad => cad.Name == "Optional" || cad.Name == "System.ParamArrayAttribute");

		internal static CodeExpression DefaultValue(this CodeParameterDeclarationExpression p)
		{
			if (p.CustomAttributes.Cast<CodeAttributeDeclaration>().FirstOrDefault(cad => cad.Name == "DefaultParameterValue") is CodeAttributeDeclaration def)
			{
				if (def.Arguments[0] is CodeAttributeArgument caa)
					return caa.Value;
			}

			return null;
		}
	}
}