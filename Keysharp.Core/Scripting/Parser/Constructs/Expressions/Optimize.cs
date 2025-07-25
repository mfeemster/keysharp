namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private static bool IsOptimizableExpression(CodeExpression expr)
		{
			if (expr is CodeMethodInvokeExpression invoke)
			{
				if (invoke.Method.MethodName == InternalMethods.Operate.MethodName && invoke.Parameters.Count == 3)
				{
					if (invoke.Parameters[0] is CodeFieldReferenceExpression cfe && cfe.FieldName != "RegEx")
						return true;
				}
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
				if (invoke.Parameters[1] is CodeSnippetExpression cse1)//Longs.
				{
					var ll = cse1.Value.ParseLong(false);

					if (ll != null)
						left = ll;
				}
				else if (invoke.Parameters[1] is CodePrimitiveExpression cpe1)//Strings and doubles.
					left = cpe1.Value;

				if (invoke.Parameters[2] is CodeSnippetExpression cse2)
				{
					var rl = cse2.Value.ParseLong(false);

					if (rl != null)
						right = rl;
				}
				else if (invoke.Parameters[2] is CodePrimitiveExpression cpe2)
					right = cpe2.Value;

				if (left != null && right != null)
				{
					object result;

					if (left is long l && right is long r)
					{
						result = Script.Operate((Script.Operator)invoke.Parameters[0].UserData[RawData], l, r);

						if (result is long lresult)
							return new CodeSnippetExpression($"{lresult}L");
					}
					else
					{
						result = Script.Operate((Script.Operator)invoke.Parameters[0].UserData[RawData], left, right);

						if (result is long lresult)
							return new CodeSnippetExpression($"{lresult}L");

						return new CodePrimitiveExpression(result);
					}
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
				var assign = expr.WasCboeAssign();
				assign.Right = OptimizeExpression(assign.Right);
				return assign;
			}

			if (!IsOptimizableExpression(expr))
				return expr;

			var invoke = (CodeMethodInvokeExpression)expr;

			for (var i = 1; i < 3; i++)
				invoke.Parameters[i] = OptimizeExpression(invoke.Parameters[i]);

			return invoke;
		}
	}
}