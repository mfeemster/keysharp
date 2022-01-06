using System.CodeDom;
using System.Collections.Generic;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private CodeExpression StringConcat(params CodeExpression[] parts)
		{
			var list = new List<CodeExpression>(parts.Length);

			foreach (var part in parts)
			{
				if (part is CodePrimitiveExpression cpe && cpe.Value is string s)
				{
					if (string.IsNullOrEmpty(s))
						continue;
				}

				list.Add(part);
			}

			if (list.Count == 1)
				return list[0];

			var str = typeof(object);// typeof(string);
			var method = (CodeMethodReferenceExpression)InternalMethods.StringConcat;
			var all = new CodeArrayCreateExpression(str, list.ToArray());
			return new CodeMethodInvokeExpression(method, all);
		}
	}
}