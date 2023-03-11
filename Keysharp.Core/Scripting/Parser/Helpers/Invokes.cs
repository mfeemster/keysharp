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
		private List<CodeMethodInvokeExpression> invokes = new List<CodeMethodInvokeExpression>();
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

		private void StdLib()
		{
			var search = new StringBuilder();
			_ = search.Append(Environment.GetEnvironmentVariable(LibEnv));
			_ = search.Append(Path.PathSeparator);
			_ = search.Append(Path.Combine(Assembly.GetExecutingAssembly().Location, LibDir));

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				_ = search.Append(Path.PathSeparator);
				_ = search.Append(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.Combine("AutoHotkey", LibDir)));
			}
			else if (Path.DirectorySeparatorChar == '/' && Environment.OSVersion.Platform == PlatformID.Unix)
			{
				_ = search.Append(Path.PathSeparator);
				_ = search.Append("/usr/" + LibDir + "/" + LibExt);
				_ = search.Append(Path.PathSeparator);
				_ = search.Append("/usr/local/" + LibDir + "/" + LibExt);
				_ = search.Append(Path.PathSeparator);
				_ = search.Append(Path.Combine(Environment.GetEnvironmentVariable("HOME"), Path.Combine(LibDir, LibExt)));
			}

			var paths = search.ToString().Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
			search = null;

			foreach (var invoke in invokes)
			{
				var name = invoke.Method.MethodName;

				if (IsLocalMethodReference(name))
				{
					//var obj = new CodeArrayCreateExpression { Size = invoke.Parameters.Count, CreateType = new CodeTypeReference(typeof(object)) };
					//obj.Initializers.AddRange(invoke.Parameters);
					//invoke.Parameters.Clear();
					//_ = invoke.Parameters.Add(obj);
					continue;
				}

				if (invoke.Method.TargetObject != null)
					continue;

				var z = name.IndexOf(LibSeperator);

				if (z != -1)
					name = name.Substring(0, z);

				foreach (var dir in paths)
				{
					if (!Directory.Exists(dir))
						continue;

					var file = Path.Combine(dir, string.Concat(name, ".", LibExt));

					if (File.Exists(file))
					{
						_ = Parse(new StreamReader(file), Path.GetFileName(file));
						return;
					}
				}
			}

			invokes.Clear();
		}
	}
}