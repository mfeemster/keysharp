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
	}
}