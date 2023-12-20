using Keysharp.Core;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.CodeDom;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	internal class PreReader
	{
		private static int hotifcount;
		private static char[] libBrackets = new char[] { '<', '>' };
		private static string multiLineComments = new string(new[] { MultiComB, MultiComA });
		private readonly List<string> includes = new List<string>();
		private string includePath = "./";
		private static FrozenSet<string> otbFlowKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			FlowCatch,
			FlowClass,
			FlowElse,
			FlowFinally,
			FlowGosub,
			FlowIf,
			FlowLoop,
			FlowReturn,
			FlowSwitch,
			FlowTry,
			//FlowUntil,//Could  until { one : 1 } == x ever be done?
			//FlowWhile//Same: while { one : 1 } == x
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
		private Parser parser;
		internal static int NextHotIfCount => ++hotifcount;
		internal List<(string, bool)> PreloadedDlls { get; } = new List<(string, bool)>();
		internal eScriptInstance SingleInstance { get; private set; } = eScriptInstance.Force;

		internal PreReader(Parser p) => parser = p;

		internal List<CodeLine> Read(TextReader source, string name)
		{
			string code;
			var line = 0;
			var list = new List<CodeLine>();
			var extralines = new List<CodeLine>();
			var replace = new[,]//These will need to be done differently on linux.//LINUXTODO
			{
				{ "%A_AhkPath%", Accessors.A_AhkPath },
				{ "%A_AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) },
				{ "%A_AppDataCommon%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) },
				{ "%A_ComputerName%", Accessors.A_ComputerName },
				{ "%A_ComSpec%", Accessors.A_ComSpec },
				{ "%A_Desktop%", Accessors.A_Desktop },
				{ "%A_DesktopCommon%", Accessors.A_DesktopCommon },
				{ "%A_IsCompiled%", Accessors.A_IsCompiled.ToString() },
				{ "%A_KeysharpPath%", Accessors.A_KeysharpPath },
				{ "%A_LineFile%", name },
				{ "%A_MyDocuments%", Accessors.A_MyDocuments },
				{ "%A_ProgramFiles%", Accessors.A_ProgramFiles },
				{ "%A_Programs%", Accessors.A_Programs },
				{ "%A_ProgramsCommon%", Accessors.A_ProgramsCommon },
				{ "%A_ScriptDir%", Path.GetDirectoryName(parser.name) },//Note that Name, with a capital N, is the initial script file, not any of the included files.
				{ "%A_ScriptFullPath%", parser.name },
				{ "%A_ScriptName%", Accessors.A_ScriptName },
				{ "%A_Space%", Accessors.A_Space },
				{ "%A_StartMenu%", Accessors.A_StartMenu },
				{ "%A_StartMenuCommon%", Accessors.A_StartMenuCommon },
				{ "%A_Startup%", Accessors.A_Startup },
				{ "%A_StartupCommon%", Accessors.A_StartupCommon },
				{ "%A_Tab%", Accessors.A_Tab },
				{ "%A_Temp%", Accessors.A_Temp },
				{ "%A_UserName%", Accessors.A_UserName },
				{ "%A_WinDir%", Accessors.A_WinDir },
			};
			includePath = name = System.IO.File.Exists(name) ? Path.GetFullPath(name) : "./";

			if (Env.FindCommandLineArgVal("include") is string cmdinc)
			{
				if (System.IO.File.Exists(cmdinc))
				{
					if (!includes.Contains(cmdinc))
					{
						_ = includes.AddUnique(cmdinc);
						list.AddRange(Read(new StreamReader(cmdinc), cmdinc));
					}
				}
				else
					throw new ParseException($"Command line include file {cmdinc} specified with -/include not found", line, "");
			}

			while ((code = source.ReadLine()) != null)
			{
				line++;

				if (line == 1 && code.Length > 2 && code[0] == '#' && code[1] == '!')
					continue;

				code = Parser.StripComment(code).Trim(Spaces);

				if (code.Length > 1 && code[0] == MultiComA && code[1] == MultiComB)
				{
					while ((code = source.ReadLine()) != null)
					{
						line++;
						code = code.TrimStart(Spaces);

						if (code.Length > 1 && code[0] == MultiComB && code[1] == MultiComA)
						{
							code = code.Substring(2);
							break;
						}
					}

					if (code == null)
						continue;
				}

				if (code.Length > 1 && code[0] == Directive)
				{
					if (code.Length < 2)
						throw new ParseException($"{ExUnknownDirv} at line {code}", line, code);

					var delim = new char[Spaces.Length + 1];
					delim[0] = Multicast;
					Spaces.CopyTo(delim, 1);
					var sub = code.Split(delim, 2);
					var parts = new[] { sub[0], sub.Length > 1 ? sub[1] : string.Empty };
					var p1 = Parser.StripComment(parts[1]).Trim(Spaces);
					var numeric = int.TryParse(p1, out var value);
					var next = true;
					var includeOnce = false;
					var upper = parts[0].Substring(1).ToUpperInvariant();

					switch (upper)
					{
						case "INCLUDE":
							includeOnce = true;

						goto case "INCLUDEAGAIN";

						case "DLLLOAD":
						{
							var silent = false;
							p1 = p1.Trim('"');//Quotes throw off the system file/path functions, so remove them.

							if (p1.Length > 3 && p1.StartsWith("*i ", StringComparison.OrdinalIgnoreCase))
							{
								p1 = p1.Substring(3);
								silent = true;
							}

							for (var i = 0; i < replace.Length / 2; i++)
								p1 = p1.Replace(replace[i, 0], replace[i, 1]);

							PreloadedDlls.Add((p1, silent));
							//The generated code for this is handled in Parser.Parse() because it must come before the InitGlobalVars();
						}
						break;

						case "INCLUDEAGAIN":
						{
							var silent = false;
							var isLib = false;
							p1 = p1.RemoveAll("\"");//Quotes throw off the system file/path functions, so remove them.

							if (p1.StartsWith('<') && p1.EndsWith('>'))
							{
								p1 = p1.Trim(libBrackets).Split('_', StringSplitOptions.None)[0];

								if (!p1.EndsWith(".ahk"))
									p1 += ".ahk";

								isLib = true;
							}

							if (p1.StartsWith("*i ", StringComparison.OrdinalIgnoreCase))
							{
								p1 = p1.Substring(3);
								silent = true;
							}
							else if (p1.StartsWith("*i", StringComparison.OrdinalIgnoreCase))
							{
								p1 = p1.Substring(2);
								silent = true;
							}

							if (isLib)
							{
								var paths = new List<string>(6);

								if (Environment.OSVersion.Platform == PlatformID.Win32NT)
								{
									paths.Add($"{includePath}\\{p1}");//Folder relative to the script file, or as overriden.
									paths.Add($"{Accessors.A_MyDocuments}\\AutoHotkey\\{LibDir}\\{p1}");//User library.
									paths.Add($"{Accessors.A_KeysharpPath}\\{LibDir}\\{p1}");//Executable folder, standard library.
								}
								else if (Path.DirectorySeparatorChar == '/' && Environment.OSVersion.Platform == PlatformID.Unix)
								{
									paths.Add($"{includePath}/{p1}");
									paths.Add(Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("HOME"), "/AutoHotkey"), p1));
									paths.Add($"{Accessors.A_KeysharpPath}/{LibDir}/{p1}");//Three ways to get the possible executable folder.
									paths.Add($"/usr/{LibDir}/AutoHotkey/{LibDir}/{p1}");
									paths.Add($"/usr/local/{LibDir}/AutoHotkey/{LibDir}/{p1}");
								}

								var found = false;

								foreach (var dir in paths)
								{
									if (System.IO.File.Exists(dir))
									{
										found = true;

										if (includeOnce && includes.Contains(dir))
											break;

										_ = includes.AddUnique(dir);
										list.AddRange(Read(new StreamReader(dir), dir));
										break;
									}
								}

								if (!found && !silent)
									throw new ParseException($"Include file {p1} not found at any of the locations: {string.Join(Environment.NewLine, paths)}", line, code);
							}
							else
							{
								for (var i = 0; i < replace.Length / 2; i++)
									p1 = p1.Replace(replace[i, 0], replace[i, 1]);

								var path = p1;

								if (!Path.IsPathRooted(path) && Directory.Exists(includePath))
									path = Path.Combine(includePath, path);
								else if (!Path.IsPathRooted(path))
									path = Path.Combine(Path.GetDirectoryName(name), path);

								path = Path.GetFullPath(path);

								if (Directory.Exists(path))
								{
									includePath = path;
								}
								else if (System.IO.File.Exists(path))
								{
									if (includeOnce && includes.Contains(path))
										break;

									_ = includes.AddUnique(path);
									list.AddRange(Read(new StreamReader(path), path));
								}
								else
								{
									if (!silent)
										throw new ParseException($"Include file {p1} not found at location {path}", line, code);

									break;
								}
							}
						}
						break;

						case "REQUIRES":
						{
							if (p1.StartsWith("AutoHotkey") || p1.StartsWith("Keysharp"))
							{
								var splits = p1.Split(' ', StringSplitOptions.RemoveEmptyEntries);

								if (splits.Length > 1)
								{
									var ver = splits[1].Trim(new char[] { 'v', '+' });
									var plus = splits[1].EndsWith('+');
									var reqvers = Script.ParseVersionToInts(ver);

									if (!reqvers.Any(x => x != 0))
										throw new ParseException($"This script requires {p1}", line, name);

									Script.VerifyVersion(ver, plus, line, name);
									//In addition to being checked here, it must be added to the code for when it runs as a compiled exe.
									var cmie = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Keysharp.Scripting.Script"), "VerifyVersion");
									_ = cmie.Parameters.Add(new CodePrimitiveExpression(ver));
									_ = cmie.Parameters.Add(new CodePrimitiveExpression(plus));
									_ = cmie.Parameters.Add(new CodePrimitiveExpression(0));
									_ = cmie.Parameters.Add(new CodeSnippetExpression("name"));
									parser.initial.Insert(0, new CodeExpressionStatement(cmie));
									//Sub release designators such as "-alpha", "-beta" are not supported in C#. Only the assembly version is supported.
								}
							}
						}
						break;

						case "HOTIF":
							if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
							{
								var hotiffuncname = $"HotIf_{NextHotIfCount}";
								extralines.Add(new CodeLine(name, line, $"{hotiffuncname}(thehotkey)"));
								extralines.Add(new CodeLine(name, line, "{"));
								extralines.Add(new CodeLine(name, line, $"return {parts[1]}"));
								extralines.Add(new CodeLine(name, line, "}"));
								var tempcl = new CodeLine(name, line, "HotIf(FuncObj(\"" + hotiffuncname + "\"))");//Can't use interpolated string here because the AStyle formatter misinterprets it.

								if (line < list.Count)
								{
									list.Insert(line, tempcl);
								}
								else
								{
									list.Add(tempcl);
								}
							}
							else
								list.Add(new CodeLine(name, line, "HotIf(\"\")"));

							break;

						case "NODYNAMICVARS":
							parser.DynamicVars = false;
							break;

						//case "NOENV":
						//  NoEnv = true;
						//  break;

						case "PERSISTENT":
							parser.Persistent = true;
							break;

						case "SINGLEINSTANCE":
						{
							switch (p1.ToUpperInvariant())
							{
								case "FORCE":
									SingleInstance = eScriptInstance.Force;
									break;

								case "IGNORE":
									SingleInstance = eScriptInstance.Ignore;
									break;

								case "PROMPT":
									SingleInstance = eScriptInstance.Prompt;
									break;

								case "OFF":
									SingleInstance = eScriptInstance.Off;
									break;

								default:
									break;
							}
						}
						break;

						case "HOTSTRING":
						{
							if (sub.Length > 1)
							{
								var splits = sub[1].Split(' ', 2);

								if (splits.Length > 0)
								{
									p1 = splits[0].ToUpperInvariant();

									switch (p1.ToUpperInvariant())
									{
										case "NOMOUSE":
											list.Add(new CodeLine(name, line, code));
											break;

										case "ENDCHARS":
											list.Add(new CodeLine(name, line, code));
											//list.Add(new CodeLine(name, line, Parser.EscapedString(code, false)));
											break;

										default:
											list.Add(new CodeLine(name, line, "Hotstring(\"" + sub[1] + "\")"));//Can't use interpolated string here because the AStyle formatter misinterprets it.
											next = false;
											break;
									}
								}
							}
						}
						break;

						//Deprecated directives.
						case "ALLOWSAMELINECOMMENTS":
						case "HOTKEYINTERVAL":
						case "HOTKEYMODIFIERTIMEOUT":
						case "INSTALLKEYBDHOOK":
						case "INSTALLMOUSEHOOK":
						case "KEYHISTORY":
						case "MAXHOTKEYSPERINTERVAL":
						case "MAXMEM":
							break;

						//Directives that will be processed in Statements().
						case "ASSEMBLYTITLE":
						case "ASSEMBLYDESCRIPTION":
						case "ASSEMBLYCONFIGURATION":
						case "ASSEMBLYCOMPANY":
						case "ASSEMBLYPRODUCT":
						case "ASSEMBLYCOPYRIGHT":
						case "ASSEMBLYTRADEMARK":
						case "ASSEMBLYVERSION":
						case "CLIPBOARDTIMEOUT":
						case "ERRORSTDOUT":
						case "USEHOOK":
						case "MAXTHREADS":

						//case "MAXTHREADSBUFFER":
						//case "MAXTHREADSPERHOTKEY":
						case "NOTRAYICON":
						case "SUSPENDEXEMPT":
						case "WINACTIVATEFORCE":
						case Keyword_IfWin:
							list.Add(new CodeLine(name, line, code));
							break;

						default:
							next = false;
							break;
					}

					if (next)
						continue;
				}

				if (code.Length > 0 && code[0] == ParenOpen)
				{
					if (list.Count == 0)
						throw new ParseException($"{ExUnexpected} at line {code}", line, code);

					var buf = new StringBuilder(256);
					//_ = buf.Append(options[0]);// code);
					_ = buf.Append(code);
					_ = buf.Append(newlineToUse);

					while ((code = source.ReadLine()) != null)
					{
						var codeTrim = code.TrimStart(Spaces);

						if (codeTrim.Length > 0 && codeTrim[0] == ParenClose)
						{
							code = codeTrim = codeTrim.Substring(1);
							_ = buf.Append(ParenClose);
							break;
						}
						else
						{
							_ = buf.Append(code);
							_ = buf.Append(newlineToUse);
						}
					}

					var str = buf.ToString();
					var result = Parser.MultilineString(str);
					var lastlistitem = list[list.Count - 1];

					if (lastlistitem.Code.EndsWith('\''))
						lastlistitem.Code = lastlistitem.Code.Substring(0, lastlistitem.Code.Length - 1) + '\"';

					if (code.EndsWith('\''))
						code = code.Substring(0, code.Length - 1) + '\"';

					lastlistitem.Code += result + code;
					continue;
				}

				code = code.Trim(Spaces);

				if (code.StartsWith(multiLineComments))
					code = code.Substring(2);

				if (code.Length == 0 || Parser.IsCommentLine(code))
					continue;

				var parenlevels = 0;
				var bracelevels = 0;
				var bracketlevels = 0;
				var inquote = false;
				var verbatim = false;

				if (code.Length == 1 && (code[0] == BlockOpen || code[0] == BlockClose))
				{
				}
				else
				{
					var cont = Parser.IsContinuationLine(code, true);
					var peek = source.Peek();

					if (code.EndsWith('=') && peek != '(' && peek != '[')//Very special case for ending with = If the next line is not a paren or bracket, it's an empty assignment, otherwise it's the start of a continuation statement.
						cont = false;

					//Don't count hotstrings/keys because they can have brackets and braces as their trigger, which may not be balanced.
					var ll = code.Contains("::", StringComparison.OrdinalIgnoreCase) ? false : LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);

					if (cont || (ll && (((!code.IsBalanced('{', '}') || !code.IsBalanced('[', ']')) && ((code.IndexOf('(') != -1 && !code.IsBalanced('(', ')')) || code.OcurredInBalance(":=", '(', ')'))) ||//OcurredInBalance is for checking that the := was not inside of a function declaring a default parameter value like func(a := 123).
										//Non-flow statements that end in { or [, such as constructing a map or array, are also considered the start of a multiline statement.
										(code.Length > 1 &&
										 !code.Contains('(') && !code.Contains(')') &&
										 code[code.Length - 2] != ' ' &&
										 code[code.Length - 2] != '\t'
										)
									   )
								)
					   )
					{
						if (cont)
							code += SingleSpace;

						code += ParseContinuations(source, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
					}
				}

				if (Parser.IsContinuationLine(code, false))
				{
					if (list.Count == 0)
						throw new ParseException(ExUnexpected, line, code);

					var i = list.Count - 1;
					var buf = new StringBuilder(list[i].Code, list[i].Code.Length + /*newlineToUse.Length*/1 + code.Length);//Was originally using newline, but probably want space instead.
					_ = buf.Append(/*newlineToUse*/SingleSpace);
					_ = buf.Append(code);
					list[i].Code = buf.ToString();
				}
				else
				{
					Parser.Translate(ref code);

					if (code.Length != 0)
					{
						//var tempLine = line;
						var rest = code.Trim(Spaces);
						var firstImbalanced = 0;

						while (rest.Length > 0)
						{
							if (rest.StartsWith('{') || rest.StartsWith('}'))
							{
								list.Add(new CodeLine(name, line, rest[0].ToString()));
								rest = rest.Substring(1).Trim(Spaces);
							}
							else if ((firstImbalanced = rest.FindFirstImbalanced('{', '}')) != -1)
							{
								var cur = rest.Substring(0, firstImbalanced).Trim(Spaces);
								list.Add(new CodeLine(name, line, cur));
								rest = rest.Substring(firstImbalanced).Trim(Spaces);
							}
							else
							{
								var splits = rest.Split(Spaces, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

								//Only split if it's an OTB flow statement like if {
								//Do not split if it's a command statement like FileAppend {one, 1}, "*"
								if (splits.Length > 1 && splits[1][0] == '{' && otbFlowKeywords.Contains(splits[0]))
								{
									var firstOpen = rest.IndexOf('{');
									var cur = rest.Substring(0, firstOpen).Trim(Spaces);
									list.Add(new CodeLine(name, line, cur));
									rest = rest.Substring(firstOpen).Trim(Spaces);
								}
								else
								{
									list.Add(new CodeLine(name, line, rest));
									break;
								}
							}
						}
					}
				}
			}

			line = list.Count > 0 ? list[list.Count - 1].LineNumber : 0;

			foreach (var extraline in extralines)
			{
				extraline.LineNumber = ++line;
				list.Add(extraline);
			}

			return list;
		}

		private bool LineLevels(string code, ref bool inquote, ref bool verbatim, ref int parenlevels, ref int bracelevels, ref int bracketlevels)
		{
			var escape = false;

			for (var i = 0; i < code.Length; i++)
			{
				var ch = code[i];

				if (Parser.IsSpace(ch))
					continue;

				if (ch == '\'')
				{
					if (!inquote)
					{
						if (i == 0 || code[i - 1] != '`')
							inquote = verbatim = true;
					}
					else if (verbatim)
					{
						if (i == 0 || code[i - 1] != '`')
							inquote = verbatim = false;
					}
				}
				else if (ch == '\"' && !verbatim)
				{
					if (!inquote)
					{
						if (i == 0 || code[i - 1] != Escape)
							inquote = true;
					}
					else
					{
						if (i == 0 || code[i - 1] != Escape || !escape)//Checking escape accounts for ``.
							inquote = false;
					}
				}

				escape = ch == Escape ? !escape : false;

				if (!inquote)
				{
					switch (ch)
					{
						case ParenOpen:
							parenlevels++;
							break;

						case ParenClose:
							parenlevels--;
							break;

						case BlockOpen:
							bracelevels++;
							break;

						case BlockClose:
							bracelevels--;
							break;

						case ArrayOpen:
							bracketlevels++;
							break;

						case ArrayClose:
							bracketlevels--;
							break;
					}
				}
			}

			return parenlevels != 0 || bracelevels != 0 || bracketlevels != 0;
		}
		private string ParseContinuations(TextReader source, ref bool inquote, ref bool verbatim, ref int parenlevels, ref int bracelevels, ref int bracketlevels)
		{
			string code;
			var sb = new StringBuilder(256);

			while ((code = source.ReadLine()) != null)
			{
				code = Parser.StripComment(code).Trim(Spaces);

				if (code.Length > 0)
				{
					if (code[0] == ParenOpen && bracketlevels == 0)//There can be multiline strings within continuation sections. Ignore for array creation, because a lambda like () => 1 might be an element, in which case it'd match the check for ParenOpen.
					{
						_ = LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
						var buf = new StringBuilder(256);
						_ = buf.Append(code);
						_ = buf.Append(newlineToUse);
						var wasinquote = false;

						while ((code = source.ReadLine()) != null)
						{
							var codeTrim = code.TrimStart(Spaces);

							if (codeTrim.Length > 0 && codeTrim[0] == ParenClose)
							{
								wasinquote = inquote;
								code = string.Concat(codeTrim.SkipWhile(x => x == ParenClose));
								_ = buf.Append(ParenClose);
								_ = LineLevels(codeTrim, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
								break;
							}
							else
							{
								_ = buf.Append(code);
								_ = buf.Append(newlineToUse);
								_ = LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
							}
						}

						var str = buf.ToString();
						var result = (inquote || wasinquote) ? Parser.MultilineString(str) : str;
						_ = sb.Append(result);

						if (code != null)//Nested continuation statements can sometimes return null.
							_ = sb.Append(code);
						else
							break;
					}
					else
					{
						_ = sb.Append(code);
					}

					var cont = Parser.IsContinuationLine(code, true);

					if (cont)//Without inserting a space, the parser will get confused if operators are attached to other symbols.
						_ = sb.Append(' ');

					if (!LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels) && !cont)
						break;
				}
			}

			if (parenlevels != 0)
				throw new ParseException($"{ExUnbalancedParens} around line {sb}");

			if (bracelevels != 0)
				throw new ParseException($"{ExUnbalancedBraces} around line {sb}");

			if (bracketlevels != 0)
				throw new ParseException($"{ExUnbalancedBrackets} around line {sb}");

			return sb.ToString();
		}
	}
}