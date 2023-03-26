using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private const string invokeCommand = "IsCommand";
		private static int labelct;

		internal static string LabelMethodName(string raw) => $"label_{raw.GetHashCode():X}_{labelct++:X}";

		internal bool IsLocalMethodReference(string name)
		{
			foreach (var method in methods[targetClass])
				if (method.Key.Equals(name, System.StringComparison.OrdinalIgnoreCase))
					return true;

			return false;
		}

		private CodeMemberMethod LocalMethod(string name)
		{
			var method = new CodeMemberMethod { Name = name, ReturnType = new CodeTypeReference(typeof(object)) };
			method.Attributes = MemberAttributes.Public | MemberAttributes.Final;

			if (typeStack.PeekOrNull() == targetClass)
				method.Attributes |= MemberAttributes.Static;

			return method;
		}

		private CodeMethodInvokeExpression LocalMethodInvoke(string name)
		{
			var invoke = new CodeMethodInvokeExpression();
			invoke.Method.MethodName = name;
			invoke.Method.TargetObject = null;
			return invoke;
		}
	}
}