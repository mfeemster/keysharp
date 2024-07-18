namespace Keysharp.Core.Common.ExtensionMethods
{
	internal static class CodeDomExtensions
	{
		internal static CodeExpression GetExpr(this CodeObject co) => co is CodeExpressionStatement ces ? ces.Expression : co as CodeExpression;

		internal static CodeBinaryOperatorExpression IsAssignExpr(this CodeExpression expr) => expr is CodeBinaryOperatorExpression cboe && cboe.Operator == CodeBinaryOperatorType.Assign ? cboe : null;

		internal static CodeSnippetExpression ParentSnippet(this CodeExpression expr)
		{
			if (expr == null)
				return null;

			if (expr.UserData.Contains("snippet") && expr.UserData["snippet"] is CodeSnippetExpression cse)
				return cse;

			return null;
		}

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