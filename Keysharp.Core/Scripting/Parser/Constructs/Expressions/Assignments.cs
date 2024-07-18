using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private bool IsImplicitAssignment(List<object> parts, int i)
		{
			int x = i - 1, y = i + 1;

			if (!(parts[i] is string si && si.Length == 1 && si[0] == Equal))//Is the middle object the '=' character...
				return false;

			if (x < 0 || !IsVarReference(parts[x]))//Is the first object a variable reference...
				return false;

			if (!(y < parts.Count && parts[y] is string sy && IsVariable(sy)))//Is the third object a known variable reference...
				return false;

			var z = x - 1;

			if (z < 0)//If x was the first element, and i was the second, return true.
				return true;

			if (IsVarAssignment(parts[z]))//If the code before positions x, i and y is a CodeBinaryOperatorExpression object with the operator type of Assign, return true.
				return true;

			return parts[z] is Script.Operator op && op != Script.Operator.IdentityEquality;//If the code before x was an operator and was not != or <>, return true.
		}

		private void MergeAssignmentAt(CodeLine codeLine, List<object> parts, int i)
		{
			int x = i - 1, y = i + 1;
			var right = y < parts.Count;

			if ((parts[i] as CodeBinaryOperatorType? ) != CodeBinaryOperatorType.Assign)
				return;

			if (i > 0 && IsJsonObject(parts[x]))//Unsure why anything using Index() is considered JSON, but this appears to work.
			{
				MergeObjectAssignmentAt(codeLine, parts, i);
				return;
			}
			else if (i > 0 && IsArrayExtension(parts[x]))
			{
				var extend = (CodeMethodInvokeExpression)parts[x];
				_ = extend.Parameters.Add(right ? VarMixedExpr(codeLine, parts[y]) : nullPrimitive);

				if (right)
					parts.RemoveAt(y);

				parts.RemoveAt(i);
				return;
			}

			var assign = new CodeBinaryOperatorExpression { Operator = CodeBinaryOperatorType.Assign };
			parts[i] = assign;

			if (assign.Left != null)
				return;

			if ((parts[x] as CodeExpression).WasCboeAssign() is CodeBinaryOperatorExpression binary)
			{
				assign.Left = (CodeArrayIndexerExpression)binary.Left;
			}
			else assign.Left = parts[x] is CodeVariableReferenceExpression cvre ? cvre :
								   parts[x] is CodeArrayIndexerExpression caie
								   ? caie
								   : parts[x] is CodePropertyReferenceExpression cpre ? cpre : VarId(parts[x] as CodeExpression, false);

			assign.Right = right ? (parts[y] is CodeVariableReferenceExpression cvre2 ? cvre2 : VarMixedExpr(codeLine, parts[y])) : nullPrimitive;
			parts[x] = BinOpToSnippet(assign);

			if (right)
				parts.RemoveAt(y);

			parts.RemoveAt(i);
		}

		/// <summary>
		/// When parsing an assignment to an element of a collection, it will parse to the following before this function is called:
		///     Index(myobject, 1L) := 123
		/// Which doesn't make sense, so it must be converted into a call to SetObject() like so:
		///     SetObject(1L, myobject, 123L);
		/// </summary>
		/// <param name="parts"></param>
		/// <param name="i"></param>
		private void MergeObjectAssignmentAt(CodeLine codeLine, List<object> parts, int i)
		{
			int x = i - 1, y = i + 1;
			var invoke = (CodeMethodInvokeExpression)parts[x];
			CodeExpression[] parameters;

			if (invoke.Method.MethodName == InternalMethods.Index.MethodName)
				parameters = invoke.Parameters.Cast<CodeExpression>().ToArray();
			else//Should never happen.
				parameters = new CodeExpression[0];

			var set = (CodeMethodInvokeExpression)InternalMethods.SetObject;

			if (y < parts.Count)
			{
				_ = set.Parameters.Add(VarMixedExpr(codeLine, parts[y]));
				parts.RemoveAt(y);
			}
			else
				_ = set.Parameters.Add(nullPrimitive);

			set.Parameters.AddRange(parameters);//Indexes go at the end because there can be a variable number of them.
			parts.RemoveAt(i);
			parts[x] = set;
		}
	}
}