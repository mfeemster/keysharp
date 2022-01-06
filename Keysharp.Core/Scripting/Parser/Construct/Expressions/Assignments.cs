using System.CodeDom;
using System.Collections.Generic;
using static Keysharp.Core.Core;

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

		private void MergeAssignmentAt(List<object> parts, int i)
		{
			int x = i - 1, y = i + 1;
			var right = y < parts.Count;

			if ((parts[i] as CodeBinaryOperatorType? ) != CodeBinaryOperatorType.Assign)
				return;

			if (i > 0 && IsJsonObject(parts[x]))
			{
				MergeObjectAssignmentAt(parts, i);
				return;
			}
			else if (i > 0 && IsArrayExtension(parts[x]))
			{
				var extend = (CodeMethodInvokeExpression)parts[x];
				_ = extend.Parameters.Add(right ? VarMixedExpr(parts[y]) : new CodePrimitiveExpression(null));

				if (right)
					parts.RemoveAt(y);

				parts.RemoveAt(i);
				return;
			}

			var assign = new CodeBinaryOperatorExpression { Operator = CodeBinaryOperatorType.Assign };
			parts[i] = assign;

			if (assign.Left != null)
				return;

			if (parts[x] is CodeBinaryOperatorExpression binary)
			{
				assign.Left = (CodeArrayIndexerExpression)binary.Left;
			}
			else assign.Left = parts[x] is CodeVariableReferenceExpression cvre ? cvre :
								   parts[x] is CodeArrayIndexerExpression caie
								   ? caie
								   : parts[x] is CodePropertyReferenceExpression cpre ? cpre : VarId(parts[x] as CodeExpression, false);

			assign.Right = right ? VarMixedExpr(parts[y]) : new CodePrimitiveExpression(null);
			parts[x] = assign;

			if (right)
				parts.RemoveAt(y);

			parts.RemoveAt(i);
		}

		private void MergeObjectAssignmentAt(List<object> parts, int i)
		{
			int x = i - 1, y = i + 1;
			var invoke = (CodeMethodInvokeExpression)parts[x];
			CodeExpression target = null;
			var step = new List<CodeExpression>();

			while (invoke.Parameters.Count == 2 && invoke.Method.MethodName == InternalMethods.Index.MethodName)
			{
				step.Add(invoke.Parameters[1]);

				if (invoke.Parameters[0] is CodeMethodInvokeExpression inv)
					invoke = inv;
				else
				{
					target = invoke.Parameters[0];
					break;
				}
			}

			var set = (CodeMethodInvokeExpression)InternalMethods.SetObject;
			_ = set.Parameters.Add(step[0]);
			step.RemoveAt(0);
			_ = set.Parameters.Add(target);
			_ = set.Parameters.Add(new CodeArrayCreateExpression(typeof(object), step.ToArray()));

			if (y < parts.Count)
			{
				_ = set.Parameters.Add(VarMixedExpr(parts[y]));
				parts.RemoveAt(y);
			}
			else
				_ = set.Parameters.Add(new CodePrimitiveExpression(null));

			parts.RemoveAt(i);
			parts[x] = set;
		}
	}
}