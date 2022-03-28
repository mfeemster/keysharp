using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private readonly List<string> includes = new List<string>();
		private string includePath = "./";
		private char[] libBrackets = new char[] { '<', '>' };
		private readonly string multiLineComments = new string( new[] { MultiComB, MultiComA });

		private List<CodeLine> Read(TextReader source, string name)
		{
			string code;
			var line = 0;
			var list = new List<CodeLine>();
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
				{ "%A_ScriptDir%", Path.GetDirectoryName(Name) },//Note that Name, with a capital N, is the initial script file, not any of the included files.
				{ "%A_ScriptFullPath%", Name },
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
						list.AddRange(Read(new StreamReader(cmdinc), cmdinc));
						_ = includes.AddUnique(cmdinc);
					}
				}
				else
					throw new ParseException($"Command line include file {cmdinc} specified with -/include not found", line);
			}

			while ((code = source.ReadLine()) != null)
			{
				line++;

				if (line == 1 && code.Length > 2 && code[0] == '#' && code[1] == '!')
					continue;

				code = StripComment(code).Trim(Spaces);

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
						throw new ParseException(ExUnknownDirv, line);

					var delim = new char[Spaces.Length + 1];
					delim[0] = Multicast;
					Spaces.CopyTo(delim, 1);
					var sub = code.Split(delim, 2);
					var parts = new[] { sub[0], sub.Length > 1 ? sub[1] : string.Empty };
					var p1 = StripComment(parts[1]).Trim(Spaces);
					_ = int.TryParse(p1, out var value);
					var next = true;
					var includeOnce = false;

					switch (parts[0].Substring(1).ToUpperInvariant())
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

							preloadedDlls.Add((p1, silent));
						}
						break;

						case "INCLUDEAGAIN":
						{
							var silent = false;
							var isLib = false;
							p1 = p1.Trim('"');//Quotes throw off the system file/path functions, so remove them.

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
								var dirs = new string[]
								{
									$"{Path.GetDirectoryName(Name)}\\Lib\\{p1}",//Local library
									$"{Accessors.A_MyDocuments}\\AutoHotkey\\Lib\\{p1}",//User library
									$"{Path.GetDirectoryName(Accessors.A_KeysharpPath)}\\Lib\\{p1}"//Standard library
								};
								var found = false;

								foreach (var dir in dirs)
								{
									if (System.IO.File.Exists(dir))
									{
										found = true;

										if (includeOnce && includes.Contains(dir))
											break;

										list.AddRange(Read(new StreamReader(dir), dir));
										_ = includes.AddUnique(dir);
										break;
									}
								}

								if (!found && !silent)
									throw new ParseException($"Include file {p1} not found", line);
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

									list.AddRange(Read(new StreamReader(path), path));
									_ = includes.AddUnique(path);
								}
								else
								{
									if (!silent)
										throw new ParseException($"Include file {p1} not found", line);

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
									var reqvers = Options.ParseVersionToInts(ver);

									if (!reqvers.Any(x => x != 0))
										throw new ParseException($"This script requires {p1}", line, name);

									Options.VerifyVersion(ver, plus, line, name);
									//In addition to being checked here, it must be added to the code for when it runs as a compiled exe.
									var cmie = (CodeMethodInvokeExpression)new MethodReference(typeof(Keysharp.Core.Options), "VerifyVersion");
									_ = cmie.Parameters.Add(new CodeSnippetExpression($"\"{ver}\""));
									_ = cmie.Parameters.Add(new CodePrimitiveExpression(plus));
									_ = cmie.Parameters.Add(new CodePrimitiveExpression(0));
									_ = cmie.Parameters.Add(new CodePrimitiveExpression(""));
									initial.Insert(0, new CodeExpressionStatement(cmie));
									//Sub release designators such as "-alpha", "-beta" are not supported in C#. Only the assembly version is supported.
								}
							}
						}
						break;

						case "NODYNAMICVARS":
							DynamicVars = false;
							break;

						//case "NOENV":
						//  NoEnv = true;
						//  break;

						case "NOTRAYICON":
							NoTrayIcon = true;
							break;

						case "PERSISTENT":
							Persistent = true;
							break;

						case "SINGLEINSTANCE":
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

							break;

						case "WINACTIVATEFORCE":
							WinActivateForce = true;
							break;

						case "HOTSTRING":
							switch (p1.ToUpperInvariant())
							{
								case "NOMOUSE":
									HotstringNoMouse = true;
									break;

								case "ENDCHARS":
									HotstringEndChars = p1;
									break;

								default:
									next = false;
									break;
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
						case "MAXTHREADS":
						case "MAXTHREADSBUFFER":
						case "MAXTHREADSPERHOTKEY":
						case "USEHOOK":
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
						throw new ParseException(ExUnexpected, line);

					var buf = new StringBuilder(256);
					//_ = buf.Append(options[0]);// code);
					_ = buf.Append(code);
					_ = buf.Append(Environment.NewLine);

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
							_ = buf.Append(Environment.NewLine);
						}
					}

					var str = buf.ToString();
					var result = MultilineString(str);
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

				if (code.Length == 0 || IsCommentLine(code))
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
					var cont = IsContinuationLine(code, true);
					var peek = source.Peek();

					if (code.EndsWith('=') && peek != '(' && peek != '[')//Very special case for ending with = If the next line is not a paren or bracket, it's an empty assignment, otherwise it's the start of a continuation statement.
						cont = false;

					//Don't count hotstrings/keys because they can have brackets and braces as their trigger, which may not be balanced.
					var ll = code.Contains("::", StringComparison.OrdinalIgnoreCase) ? false : LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);

					if (cont || (ll && (!code.EndsWith('{') ||//Don't treat non-flow statements that end in {, such as constructing a map, as the start of a multiline statement.
										(code.Length > 1 &&
										 !code.Contains('(') && !code.Contains(')') &&
										 !code.StartsWith("if", StringComparison.OrdinalIgnoreCase) &&//Don't treat flow statements as the start of a multiline statement.
										 !code.StartsWith("while", StringComparison.OrdinalIgnoreCase) &&
										 !code.StartsWith("else", StringComparison.OrdinalIgnoreCase) &&
										 !code.StartsWith("for", StringComparison.OrdinalIgnoreCase) &&
										 !code.StartsWith("try", StringComparison.OrdinalIgnoreCase) &&
										 !code.StartsWith("catch", StringComparison.OrdinalIgnoreCase) &&
										 !code.StartsWith("switch", StringComparison.OrdinalIgnoreCase) &&
										 !code.StartsWith("loop", StringComparison.OrdinalIgnoreCase)
										)
									   )))
					{
						if (cont)
							code += SingleSpace;

						code += ParseContinuations(source, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
					}
				}

				if (IsContinuationLine(code, false))
				{
					if (list.Count == 0)
						throw new ParseException(ExUnexpected, line);

					var i = list.Count - 1;
					var buf = new StringBuilder(list[i].Code, list[i].Code.Length + /*Environment.NewLine.Length*/1 + code.Length);//Was originally using newline, but probably want space instead.
					_ = buf.Append(/*Environment.NewLine*/SingleSpace);
					_ = buf.Append(code);
					list[i].Code = buf.ToString();
				}
				else
				{
					Translate(ref code);

					if (code.Length != 0)
						list.Add(new CodeLine(name, line, code));
				}
			}

			return list;
		}

		private string ParseContinuations(TextReader source, ref bool inquote, ref bool verbatim, ref int parenlevels, ref int bracelevels, ref int bracketlevels)
		{
			string code;
			var sb = new StringBuilder(256);

			while ((code = source.ReadLine()) != null)
			{
				code = StripComment(code).Trim(Spaces);

				if (code.Length > 0)
				{
					if (code[0] == ParenOpen)//There can be multiline strings within continuation sections.
					{
						_ = LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
						var buf = new StringBuilder(256);
						_ = buf.Append(code);
						_ = buf.Append(Environment.NewLine);
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
								_ = buf.Append(Environment.NewLine);
								_ = LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels);
							}
						}

						var str = buf.ToString();
						var result = (inquote || wasinquote) ? MultilineString(str) : str;
						_ = sb.Append(result);

						if (code != null)//Nested continuation statements can sometimes return null.
							_ = sb.Append(code);
						else
							break;
					}
					else
						_ = sb.Append(code);

					var cont = IsContinuationLine(code, true);

					if (cont)//Without inserting a space, the parser will get confused if operators are attached to other symbols.
						_ = sb.Append(' ');

					if (!LineLevels(code, ref inquote, ref verbatim, ref parenlevels, ref bracelevels, ref bracketlevels) && !cont)
						break;
				}
			}

			if (parenlevels != 0)
				throw new ParseException(ExUnbalancedParens);

			if (bracelevels != 0)
				throw new ParseException(ExUnbalancedBraces);

			if (bracketlevels != 0)
				throw new ParseException(ExUnbalancedBrackets);

			return sb.ToString();
		}

		private bool LineLevels(string code, ref bool inquote, ref bool verbatim, ref int parenlevels, ref int bracelevels, ref int bracketlevels)
		{
			var escape = false;

			for (var i = 0; i < code.Length; i++)
			{
				var ch = code[i];

				if (IsSpace(ch))
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

					//continue;
				}
				else if (ch == '\"' && !verbatim)
				{
					if (!inquote)
					{
						if (i == 0 || code[i - 1] != '`')
							inquote = true;
					}
					else
					{
						if (i == 0 || code[i - 1] != '`' || !escape)//Checking escape accounts for ``.
							inquote = false;
					}

					//continue;
				}

				if (ch == Escape)
					escape = !escape;
				else
					escape = false;

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
	}
}