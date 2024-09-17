using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class AssemblyBuildVersionAttribute : Attribute
	{
		public string Version { get; }

		public AssemblyBuildVersionAttribute(string v) => Version = v;
	}

	public partial class Parser
	{
		private void AddAssemblyAttribute(Type attribute, object value)
		{
			var type = new CodeTypeReference(attribute);
			type.UserData.Add(RawData, attribute);
			var arg = new CodeAttributeArgument(new CodePrimitiveExpression(value));
			var dec = new CodeAttributeDeclaration(type, arg);
			_ = assemblyAttributes.Add(dec);
		}

		private void ParseDirective(CodeLine codeLine, string code)
		{
			if (code.Length < 2)
				throw new ParseException(ExUnknownDirv, codeLine);

			var parts = code.Split(directiveDelims, 2);

			if (parts.Length != 2)
				parts = [parts[0], string.Empty];

			parts[1] = StripComment(parts[1]).Trim(Spaces);
			var value = 0u;
			bool numeric;
			string[] sub;

			if (parts[1].Length == 0)
			{
				numeric = false;
				sub = [string.Empty, string.Empty];
			}
			else
			{
				numeric = uint.TryParse(parts[1], out value);
				var split = parts[1].Split([Multicast], 2);
				sub = [split[0].Trim(Spaces), split.Length > 1 ? split[1].Trim(Spaces) : string.Empty];
			}

			var cmd = parts[0].Substring(1);
			const string res = "__IFWIN";
			/*
			    Warn
			*/
			var upper = cmd.ToUpperInvariant();

			switch (upper)
			{
				case "ASSEMBLYTITLE":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyTitleAttribute), parts[1]);

					break;

				case "ASSEMBLYDESCRIPTION":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyDescriptionAttribute), parts[1]);

					break;

				case "ASSEMBLYCONFIGURATION":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyConfigurationAttribute), parts[1]);

					break;

				case "ASSEMBLYCOMPANY":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyCompanyAttribute), parts[1]);

					break;

				case "ASSEMBLYPRODUCT":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyProductAttribute), parts[1]);

					break;

				case "ASSEMBLYCOPYRIGHT":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyCopyrightAttribute), parts[1]);

					break;

				case "ASSEMBLYTRADEMARK":
					if (!string.IsNullOrEmpty(parts[1]))
						AddAssemblyAttribute(typeof(AssemblyTrademarkAttribute), parts[1]);

					break;

				case "ASSEMBLYVERSION":
					if (!string.IsNullOrEmpty(parts[1]))
					{
						AddAssemblyAttribute(typeof(AssemblyVersionAttribute), parts[1]);
						AddAssemblyAttribute(typeof(AssemblyFileVersionAttribute), parts[1]);
					}

					break;

				//Note that ASSEMBLYCULTURE is not supported because if you include it, you will get this error: "Executables cannot be satellite assemblies; culture should always be empty".
				case "CLIPBOARDTIMEOUT":
				{
					if (numeric)
					{
						Accessors.A_ClipboardTimeout = value;
						var clipvar = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_ClipboardTimeout");
						var clipset = new CodeAssignStatement(clipvar, new CodePrimitiveExpression(value));
						initial.Insert(0, clipset);
					}
					else
						throw new ParseException($"#{upper} directive must be followed by numerical value.", codeLine);
				}
				break;

				case "INPUTLEVEL":
				{
					Accessors.A_InputLevel = numeric ? value : 0L;
					var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_InputLevel");
					var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(Accessors.A_InputLevel));
					_ = parent.Add(propset);
				}
				break;

				case "SUSPENDEXEMPT":
				{
					var val = parts[1].Length > 0 ? (Options.OnOff(parts[1]) ?? false) : true;
					var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_SuspendExempt");
					var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(val));
					_ = parent.Add(propset);
				}
				break;

				case "WINACTIVATEFORCE":
				{
					Script.WinActivateForce = true;
					var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), "WinActivateForce");
					var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(Script.WinActivateForce));
					initial.Insert(0, propset);
				}
				break;

				case "HOTIFTIMEOUT":
				{
					if (numeric)
					{
						Accessors.A_HotIfTimeout = value;
						var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_HotIfTimeout");
						var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(Accessors.A_HotIfTimeout));
						initial.Insert(0, propset);
					}
					else
						throw new ParseException($"#{upper} directive must be followed by numerical value.", codeLine);
				}
				break;

				case "HOTSTRING":
				{
					var splits = parts[1].Split(' ', 2);

					if (splits.Length > 0)
					{
						var p1 = splits[0].ToUpperInvariant();

						switch (p1)
						{
							case "NOMOUSE":
							{
								var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), "HotstringNoMouse");
								var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(true));
								initial.Insert(0, propset);
							}
							break;

							case "ENDCHARS":
							{
								if (splits.Length > 1)
								{
									var p2 = EscapedString(splits[1], false);
									var cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Keysharp.Core.Keyboard"), "Hotstring", [new CodePrimitiveExpression("ENDCHARS"), new CodePrimitiveExpression(p2)]);
									initial.Insert(0, new CodeExpressionStatement(cmie));
								}
							}
							break;
								//default:
								//{
								//  var p2 = parts[1];
								//  var cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Keysharp.Core.Keyboard"), "Hotstring", new CodeExpression[] { new CodePrimitiveExpression(p2) });
								//  parent.Add(new CodeExpressionStatement(cmie));
								//}
								//break;
						}
					}
				}
				break;

				case "ERRORSTDOUT":
				{
					ErrorStdOut = true;
				}
				break;

				case "USEHOOK":
				{
					var val = parts[1].Length > 0 ? (Options.OnOff(parts[1]) ?? false) : true;
					var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_UseHook");
					var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(val));
					_ = parent.Add(propset);
				}
				break;

				case "MAXTHREADS":
				{
					if (numeric)
					{
						var val = Math.Clamp(value, 1u, 255u);
						var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), "MaxThreadsTotal");
						var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(val));
						initial.Insert(0, propset);
					}
					else
						throw new ParseException($"#{upper} directive must be followed by numerical value.", codeLine);
				}
				break;

				case "MAXTHREADSBUFFER":
				{
					var val = parts[1].Length > 0 ? (Options.OnOff(parts[1]) ?? false) : true;
					var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_MaxThreadsBuffer");
					var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(val));
					_ = parent.Add(propset);
				}
				break;

				case "MAXTHREADSPERHOTKEY":
				{
					if (numeric)
					{
						var val = Math.Clamp(value, 1u, 255u);
						var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "A_MaxThreadsPerHotkey");
						var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(val));
						_ = parent.Add(propset);
					}
					else
						throw new ParseException($"#{upper} directive must be followed by numerical value.", codeLine);
				}
				break;

				case "NOTRAYICON":
				{
					NoTrayIcon = true;
					var prop = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), "NoTrayIcon");
					var propset = new CodeAssignStatement(prop, new CodePrimitiveExpression(NoTrayIcon));
					initial.Insert(0, propset);
				}
				break;

				case res:
					var cond = (CodeMethodInvokeExpression)InternalMethods.Hotkey;
					_ = cond.Parameters.Add(new CodePrimitiveExpression(cmd));
					_ = cond.Parameters.Add(new CodePrimitiveExpression(sub[0]));
					_ = cond.Parameters.Add(new CodePrimitiveExpression(sub[1]));
					_ = prepend.Add(cond);
					break;

				default:
					throw new ParseException(ExUnknownDirv, codeLine);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public sealed class PublicForTestOnly : Attribute
	{
		public PublicForTestOnly()
		{ }
	}

	// This always writes to the parent console window and also to a redirected stdout if there is one.
	// It would be better to do the relevant thing (eg write to the redirected file if there is one, otherwise
	// write to the console) but it doesn't seem possible.
	/*  public class GUIConsoleWriter// : IConsoleWriter
	    {
	    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
	    private static extern bool AttachConsole(int dwProcessId);

	    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
	    static extern bool FreeConsole();

	    private const int ATTACH_PARENT_PROCESS = -1;

	    StreamWriter _stdOutWriter;

	    // this must be called early in the program
	    public GUIConsoleWriter()
	    {
	        // this needs to happen before attachconsole.
	        // If the output is not redirected we still get a valid stream but it doesn't appear to write anywhere
	        // I guess it probably does write somewhere, but nowhere I can find out about
	        var stdout = Console.OpenStandardOutput();
	        _stdOutWriter = new StreamWriter(stdout);
	        _stdOutWriter.AutoFlush = true;
	        AttachConsole(ATTACH_PARENT_PROCESS);
	    }

	    ~GUIConsoleWriter()
	    {
	        FreeConsole();
	    }

	    public void WriteLine(string line)
	    {
	        _stdOutWriter.WriteLine(line);
	        Console.WriteLine(line);
	    }
	    }*/

	public enum eScriptInstance
	{
		Force,
		Ignore,
		Prompt,
		Off
	}
}