using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;
using static Keysharp.Core.Core;

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
		public static bool WinActivateForce = WinActivateForceDefault;

		internal static int HotExprTimeout = 1000;

		internal static bool MaxThreadsBuffer = false;

		internal static uint MaxThreadsPerHotkey = 1u;

		internal static uint MaxThreadsTotal = 10u;

		//private bool NoEnv;// = NoEnvDefault;
		internal static bool NoTrayIcon = NoTrayIconDefault;

		internal static bool Persistent = PersistentDefault;

		internal static List<(string, bool)> preloadedDlls = new List<(string, bool)>();
		internal static bool SuspendExempt;
		internal bool ErrorStdOut = false;

		// #SuspendExempt, applies to hotkeys and hotstrings.
		//private const bool NoEnvDefault = false;
		private const bool NoTrayIconDefault = false;

		private const bool PersistentDefault = false;
		private const bool WinActivateForceDefault = false;
		private char[] directiveDelims = Spaces.Concat(new char[] { Multicast });
		private string HotstringEndChars = string.Empty;
		private string HotstringNewOptions = string.Empty;
		internal static bool HotstringNoMouse { get; private set; } = false;
		private string IfWinActive_WinText = string.Empty;

		//private bool LTrimForced;
		private string IfWinActive_WinTitle = string.Empty;

		private string IfWinExist_WinText = string.Empty;
		private string IfWinExist_WinTitle = string.Empty;
		private string IfWinNotActive_WinText = string.Empty;
		private string IfWinNotActive_WinTitle = string.Empty;
		private string IfWinNotExist_WinText = string.Empty;
		private string IfWinNotExist_WinTitle = string.Empty;
		private eScriptInstance SingleInstance = eScriptInstance.Force;

		internal void PrintCompilerErrors(string s)
		{
			if (ErrorStdOut || Env.FindCommandLineArg("errorstdout") != null)
				Console.Error.WriteLine(s);
			else
				_ = MessageBox.Show(s, "Keysharp", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private void AddAssemblyAttribute(Type attribute, object value)
		{
			var type = new CodeTypeReference(attribute);
			type.UserData.Add(RawData, attribute);
			var arg = new CodeAttributeArgument(new CodePrimitiveExpression(value));
			var dec = new CodeAttributeDeclaration(type, arg);
			_ = assemblyAttributes.Add(dec);
		}

		private void ParseDirective(string code)
		{
			if (code.Length < 2)
				throw new ParseException(ExUnknownDirv);

			var parts = code.Split(directiveDelims, 2);

			if (parts.Length != 2)
				parts = new[] { parts[0], string.Empty };

			parts[1] = StripComment(parts[1]).Trim(Spaces);

			var value = 0u;

			bool numeric;

			string[] sub;

			if (parts[1].Length == 0)
			{
				numeric = false;
				sub = new[] { string.Empty, string.Empty };
			}
			else
			{
				numeric = uint.TryParse(parts[1], out value);
				var split = parts[1].Split(new[] { Multicast }, 2);
				sub = new[] { split[0].Trim(Spaces), split.Length > 1 ? split[1].Trim(Spaces) : string.Empty };
			}

			var cmd = parts[0].Substring(1);
			const string res = "__IFWIN\0";

			/*
			    HotIf
			    Warn
			 * */

			switch (cmd.ToUpperInvariant())
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
						var clipvar = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Keysharp.Core.Accessors"), "ClipboardTimeout");
						var clipset = new CodeAssignStatement(clipvar, new CodePrimitiveExpression(value));
						_ = initial.Add(clipset);
					}
				}
				break;

				case "INPUTLEVEL":
				{
					Script.inputLevel = Math.Clamp(parts[1].ParseUInt().Value, 0u, 100u);
				}
				break;

				case "HOTIFTIMEOUT":
				{
					HotExprTimeout = parts[1].ParseInt().Value;
				}
				break;

				case "HOTSTRING":
				{
					HotstringNewOptions = parts[1];
				}
				break;

				case "ERRORSTDOUT":
				{
					ErrorStdOut = true;
				}
				break;

				case "SUSPENDEXEMPT":
				{
					SuspendExempt = true;
				}
				break;

				case "MAXTHREADS":
				{
					if (numeric)
						MaxThreadsTotal = Math.Clamp(value, 1u, 255u);
				}
				break;

				case "MAXTHREADSBUFFER":
				{
					MaxThreadsBuffer = Keysharp.Core.Options.OnOff(parts[1]) ?? false;
				}
				break;

				case "MAXTHREADSPERHOTKEY":
				{
					if (numeric)
						MaxThreadsPerHotkey = Math.Clamp(value, 1u, 255u);
				}
				break;

				//              case "COMMENTFLAG":
				//                  if (parts[1].Length == 2 && parts[1][0] == MultiComA && parts[1][1] == MultiComB)
				//                      throw new ParseException(ExIllegalCommentFlag);

				//#if LEGACY
				//                  Comment = parts[1];
				//#else//MATT
				//                  Comment = parts[1][0];
				//#endif
				//                  break;

				//case "DEREFCHAR":
				//  Resolve = parts[1][0];
				//  break;

				//case "ESCAPECHAR":
				//  Escape = parts[1][0];
				//  break;

				//case "DELIMITER":
				//  Multicast = parts[1][0];
				//  break;

				//case "IFWINACTIVE":
				//  IfWinActive_WinTitle = sub[0];
				//  IfWinActive_WinText = sub[1];

				//goto case res;

				//case "IFWINEXIST":
				//  IfWinExist_WinTitle = sub[0];
				//  IfWinExist_WinText = sub[1];

				//goto case res;

				//case "IFWINNOTACTIVE":
				//  IfWinNotExist_WinTitle = sub[0];
				//  IfWinNotActive_WinText = sub[1];

				//goto case res;

				//case "IFWINNOTEXIST":
				//  IfWinNotExist_WinTitle = sub[0];
				//  IfWinNotExist_WinText = sub[1];

				//goto case res;

				case res:
					var cond = (CodeMethodInvokeExpression)InternalMethods.Hotkey;
					_ = cond.Parameters.Add(new CodePrimitiveExpression(cmd));
					_ = cond.Parameters.Add(new CodePrimitiveExpression(sub[0]));
					_ = cond.Parameters.Add(new CodePrimitiveExpression(sub[1]));
					_ = prepend.Add(cond);
					break;

				//case "LTRIM":
				//  switch (sub[0].ToUpperInvariant())
				//  {
				//      case "":
				//      case "ON":
				//          LTrimForced = true;
				//          break;

				//      case "OFF":
				//          LTrimForced = false;
				//          break;

				//      default:
				//          throw new ParseException("Directive parameter must be either \"on\" or \"off\"");
				//  }

				//  break;

				default:
					throw new ParseException(ExUnknownDirv);
			}
		}
	}

	public enum eScriptInstance
	{
		Force,
		Ignore,
		Prompt,
		Off
	}
}