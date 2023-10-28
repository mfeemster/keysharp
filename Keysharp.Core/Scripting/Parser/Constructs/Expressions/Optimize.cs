using System;
using System.CodeDom;
using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private static bool IsOptimizableExpression(CodeExpression expr)
		{
			if (!(expr is CodeMethodInvokeExpression))
				return false;

			var invoke = (CodeMethodInvokeExpression)expr;

			if (invoke.Method.MethodName == InternalMethods.Operate.MethodName && invoke.Parameters.Count == 3)
			{
				if (invoke.Parameters[0] is CodeFieldReferenceExpression cfe && cfe.FieldName != "RegEx")
					return true;
			}

			return false;
		}

		private CodeExpression OptimizeExpression(CodeExpression expr)
		{
			if (!IsOptimizableExpression(expr))
				return expr;

			var invoke = (CodeMethodInvokeExpression)expr;

			for (var i = 1; i < 3; i++)
				invoke.Parameters[i] = OptimizeExpression(invoke.Parameters[i]);

			object left = null;
			object right = null;

			try
			{
				if (invoke.Parameters[1] is CodePrimitiveExpression cpe1)
					left = cpe1.Value;
				else if (invoke.Parameters[1] is CodeSnippetExpression cse1)
					left = cse1.Value;

				if (invoke.Parameters[2] is CodePrimitiveExpression cpe2)
					right = cpe2.Value;
				else if (invoke.Parameters[2] is CodeSnippetExpression cse2)
					right = cse2.Value;

				if (left != null && right != null)
				{
					var ll = left.ParseLong(false);
					var rl = right.ParseLong(false);
					object result;

					if (ll is long l && rl is long r)
					{
						result = Script.Operate((Script.Operator)invoke.Parameters[0].UserData[RawData], l, r);

						if (result is long lresult)
							return new CodeSnippetExpression($"{lresult}L");
					}
					else
						result = Script.Operate((Script.Operator)invoke.Parameters[0].UserData[RawData], left, right);

					return new CodePrimitiveExpression(result);
				}
			}
			catch (Exception)
			{
				return expr;
			}

			return invoke;
		}

		private CodeExpression OptimizeLoneExpression(CodeExpression expr)
		{
			if (IsVarAssignment(expr))
			{
				var assign = (CodeBinaryOperatorExpression)expr;
				assign.Right = OptimizeExpression(assign.Right);
				return assign;
			}

			if (!IsOptimizableExpression(expr))
				return expr;

			var invoke = (CodeMethodInvokeExpression)expr;

			for (var i = 1; i < 3; i++)
				invoke.Parameters[i] = OptimizeExpression(invoke.Parameters[i]);

			bool left = invoke.Parameters[1] is CodePrimitiveExpression, right = invoke.Parameters[2] is CodeExpression;

			if (!left && !right)
				return null;

			if (left)
				return invoke.Parameters[2];

			return right ? invoke.Parameters[1] : expr;
		}
	}
}