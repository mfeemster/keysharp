using static Keysharp.Scripting.Keywords;
using static Keysharp.Scripting.Parser;

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
			FlowFor,
			FlowGosub,
			FlowIf,
			FlowLoop,
			//FlowReturn,//A brace following return is not OTB, instead it's the beginning of the creation of a Map to be returned.
			FlowSwitch,
			FlowTry,
			FlowUntil,//Could until { one : 1 } == x ever be done?
			FlowWhile//Same: while { one : 1 } == x
		} .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
		private Parser parser;
		internal static int NextHotIfCount => ++hotifcount;
		internal List<(string, bool)> PreloadedDlls { get; } = new List<(string, bool)>();
		internal eScriptInstance SingleInstance { get; private set; } = eScriptInstance.Force;

		internal PreReader(Parser p) => parser = p;

		internal List<CodeLine> Read(TextReader source, string name)
		{
			var lineNumber = 0;
			var sb = new StringBuilder(256);
			var list = new List<CodeLine>();
			var extralines = new List<CodeLine>();
			var escape = false;
			var verbatim = false;
			string code;
			ReadOnlySpan<char> prev = "";
			ReadOnlySpan<char> openBraceSpan = "{".ToCharArray();
			ReadOnlySpan<char> closeBraceSpan = "}".ToCharArray();
			ReadOnlySpan<char> spaceTabOpenBraceSpan = " \t{".ToCharArray();
			ReadOnlySpan<char> hotkeySignalSpan = "::".ToCharArray();
			ReadOnlySpan<char> hotkeySignalOtbSpan = "::{".ToCharArray();
			char last = (char)0;
			bool inString = false, inMlComment = false;
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
					throw new ParseException($"Command line include file {cmdinc} specified with -/include not found.", lineNumber, "");
			}

			void StartNewLine(string str)
			{
				list.Add(new CodeLine(name, lineNumber, str));
				_ = sb.Clear();
			}
			void AddToPreviousLine(CodeLine prevLine, string str)
			{
				prevLine.Code += str;
				_ = sb.Clear();
			}
			list.Add(new CodeLine(name, lineNumber, ""));//Always have one line to avoid always having to check for Count > 0.

			while ((code = source.ReadLine()) != null)
			{
				lineNumber++;

				if (code.Length == 0)
					continue;

				if (code[0] == ';')
					continue;

				var commentIgnore = false;
				var noCommentCode = Parser.StripComment(code);
				var span = noCommentCode.AsSpan().Trim(Spaces);

				if (span.Length == 0)
					continue;

				var prevSpan = prev.TrimEnd(Spaces);

				for (var i = 0; i < span.Length; i++)//Should really just reference span everywhere.
				{
					var ch = span[i];
					var wasCont = false;

					if (inMlComment)
					{
						if (ch == '/' && last == '*')
						{
							inMlComment = false;

							if (i < span.Length - 1)
							{
								last = ch;
								continue;
							}
							else
								commentIgnore = true;
						}
						else
						{
							last = ch;
							continue;
						}
					}
					else if (!inString)
					{
						if (ch == '*' && last == '/')
						{
							inMlComment = true;
							last = ch;
							sb.Remove(sb.Length - 1, 1);

							if (i < span.Length - 1)
								continue;
							else
								commentIgnore = true;
						}
						else if (ch == ';' && (last == ' ' || last == '\t'))
						{
							span = span.Slice(0, i).TrimEnd(Spaces);
							i = span.Length - 1;
							commentIgnore = true;
						}
					}

					if (!commentIgnore)
					{
						if (i == 0)
						{
							if (span[0] == '#')
							{
								if (span.Length < 2)
									throw new ParseException(ExUnknownDirv, lineNumber, code);

								var sub = noCommentCode.Split(SpaceMultiDelim, 2, StringSplitOptions.TrimEntries);
								var parts = new[] { sub[0], sub.Length > 1 ? sub[1] : string.Empty };
								var p1 = Parser.StripComment(parts[1]).Trim(Spaces);
								var numeric = int.TryParse(p1, out var value);
								var includeOnce = false;
								var upper = parts[0].Substring(1).ToUpperInvariant();
								var next = true;

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

										for (var ii = 0; ii < replace.Length / 2; ii++)
											p1 = p1.Replace(replace[ii, 0], replace[ii, 1]);

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
												throw new ParseException($"Include file {p1} not found at any of the locations: {string.Join(Environment.NewLine, paths)}", lineNumber, code);
										}
										else
										{
											for (var ii = 0; ii < replace.Length / 2; ii++)
												p1 = p1.Replace(replace[ii, 0], replace[ii, 1]);

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
													throw new ParseException($"Include file {p1} not found at location {path}", lineNumber, code);

												break;
											}
										}
									}
									break;

									case "REQUIRES":
									{
										var reqAhk = p1.StartsWith("AutoHotkey");

										if (reqAhk || p1.StartsWith("Keysharp"))
										{
											var splits = p1.Split(' ', StringSplitOptions.RemoveEmptyEntries);

											if (splits.Length > 1)
											{
												var ver = splits[1].Trim(new char[] { 'v', '+' });
												var plus = splits[1].EndsWith('+');
												var reqvers = Script.ParseVersionToInts(ver);

												//If it's AHK v2.x, then we support it, so don't check.
												if (reqAhk && ver.StartsWith("2."))
													break;

												if (!reqvers.Any(x => x != 0))
													throw new ParseException($"This script requires {p1}", lineNumber, name);

												Script.VerifyVersion(ver, plus, lineNumber, code);
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
											extralines.Add(new CodeLine(name, lineNumber, $"{hotiffuncname}(thehotkey)"));
											extralines.Add(new CodeLine(name, lineNumber, "{"));
											extralines.Add(new CodeLine(name, lineNumber, $"return {parts[1]}"));
											extralines.Add(new CodeLine(name, lineNumber, "}"));
											var tempcl = new CodeLine(name, lineNumber, "HotIf(FuncObj(\"" + hotiffuncname + "\"))");//Can't use interpolated string here because the AStyle formatter misinterprets it.

											if (lineNumber < list.Count)
											{
												list.Insert(lineNumber, tempcl);
											}
											else
											{
												list.Add(tempcl);
											}
										}
										else
											list.Add(new CodeLine(name, lineNumber, "HotIf(\"\")"));

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
														list.Add(new CodeLine(name, lineNumber, span.ToString()));
														break;

													case "ENDCHARS":
														list.Add(new CodeLine(name, lineNumber, span.ToString()));
														break;

													default:
														list.Add(new CodeLine(name, lineNumber, "Hotstring(\"" + sub[1] + "\")"));//Can't use interpolated string here because the AStyle formatter misinterprets it.
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
										list.Add(new CodeLine(name, lineNumber, span.ToString()));
										break;

									default:
										next = false;
										break;
								}

								if (!next)
									list.Add(new CodeLine(name, lineNumber, span.ToString()));

								break;
							}
							else if (span[0] == '(')//Continuation statements have to be parsed in line because they logic doesn't carry over to normal parsing.
							{
								//Comments within the quote preceeding a continuation ( are not part of the string.
								if (last == '"' || prevSpan.EndsWith(Quote) || prevSpan.EndsWith(":="))
								{
									if (list.Count == 0)
										throw new ParseException(ExUnexpected, lineNumber, code);

									var buf = new StringBuilder(256);
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
									var result = Parser.MultilineString(str, lineNumber, name);
									var lastlistitem = list[list.Count - 1];

									if (lastlistitem.Code.EndsWith('\''))
										lastlistitem.Code = lastlistitem.Code.Substring(0, lastlistitem.Code.Length - 1) + '\"';

									if (code.EndsWith('\''))
										code = code.Substring(0, code.Length - 1) + '\"';

									_ = sb.Append(result);//This should get added here and break//TODO
									i = 0;
									span = code.AsSpan();

									if (span.Length == 0)
									{
										ch = result.Length > 0 ? result[result.Length - 1] : (char)0;
										commentIgnore = true;
									}
									else
										ch = span[0];

									wasCont = true;
								}
							}
							else if (span.StartsWith("/*"))//Need this here so it doesn't get confused with a line starting with an operator below.
							{
								last = '*';
								inMlComment = true;
								i++;
								continue;
							}
							else if (span.StartsWith("*/"))//Need this here so it doesn't get confused with a line starting with an operator below.
							{
								last = '/';
								inMlComment = false;
								i++;
								continue;
							}
						}

						if (!commentIgnore)
						{
							if (ch == '\'')
							{
								if (!inString)
								{
									if (i == 0 || span[i - 1] != '`')
										inString = verbatim = true;
								}
								else if (verbatim)
								{
									if (i == 0 || span[i - 1] != '`')
										inString = verbatim = false;
								}
							}
							else if (ch == '\"' && !verbatim)
							{
								if (!inString)
								{
									if (i == 0 || span[i - 1] != Escape)
										inString = true;
								}
								else
								{
									if (i == 0 || span[i - 1] != Escape || !escape)//Checking escape accounts for ``.
										inString = false;
								}
							}

							escape = ch == Escape ? !escape : false;
							_ = sb.Append(ch);
						}
					}

					if (i == span.Length - 1 || span.Length == 0)//Span length will be 0 after a non quoted continuation section.
					{
						var prevLine = list[list.Count - 1];
						var newLineStr = wasCont ? sb.ToString() : sb.ToString().Trim(Spaces);

						if (IsHotstringLabel(prevLine.Code))
						{
							StartNewLine(newLineStr);
							goto EndLine;
						}

						var newLineSpan = newLineStr.AsSpan();
						var prevLineSpan = prevLine.Code.AsSpan().Trim(Spaces);
						bool newInQuote = false, prevInQuote = false;
						bool newVerbatim = false, prevVerbatim = false;
						int newParenlevels = 0, newBracelevels = 0, newBracketlevels = 0;
						int prevParenlevels = 0, prevBracelevels = 0, prevBracketlevels = 0;
						var lastNested = LineLevels(prevLine.Code, ref prevInQuote, ref prevVerbatim, ref prevParenlevels, ref prevBracelevels, ref prevBracketlevels);
						var anyNested = LineLevels(newLineStr, ref newInQuote, ref newVerbatim, ref newParenlevels, ref newBracelevels, ref newBracketlevels);
						var lastQuoteIndex = prevLineSpan.LastIndexOf('"');
						var tempPrev = lastQuoteIndex != -1 ? prevLineSpan.Slice(0, lastQuoteIndex).Trim(Spaces) : prevLineSpan;
						//Getting the first or last token is necessary because of the and, is, not, or operators because those words
						//might be contained within a variable name.
						var newStartOpIndex = newLineSpan.IndexOfAny(SpaceTabOpenParenSv);
						var newStartSubSpan = newStartOpIndex == -1 ? newLineSpan : newLineSpan.Slice(0, newStartOpIndex).Trim(Spaces);
						var newStartSubSpanStr = newStartSubSpan.ToString();
						//
						var prevStartOpIndex = prevLineSpan.IndexOfAny(SpaceTabOpenParenSv);
						var prevStartSubSpan = prevStartOpIndex == -1 ? prevLineSpan : prevLineSpan.Slice(0, prevStartOpIndex).Trim(Spaces);
						var prevStartSubSpanStr = prevStartSubSpan.ToString();
						//
						var prevEndOpIndex = prevLineSpan.LastIndexOfAny(SpaceTabOpenParenSv);
						var prevEndSubSpan = prevEndOpIndex == -1 ? prevLineSpan : prevLineSpan.Slice(prevEndOpIndex + 1).Trim(Spaces);
						var prevEndSubSpanStr = prevEndSubSpan.ToString();

						//Previous line was an empty if test without parens, don't join.
						//if x =
						//...
						if (!lastNested
								&& Parser.flowOperators.Contains(prevStartSubSpanStr)
								&& prevLine.Code.EndsWith('='))
						{
							StartNewLine(newLineStr);
						}
						//Check if the previous line ends with a := or a := "
						//Note that := "" is not a continuation line.
						//This could be a standalone empty assign, or an assign that is to be joined with this line.
						else if (tempPrev.EndsWith(":="))
						{
							//Empty assign before closing brace, don't join.
							//if (...)
							//{
							//  x :=
							//}
							if (newLineStr == "}")
							{
								StartNewLine(newLineStr);
							}
							else
							{
								//Flow, don't join.
								//x :=
								//if (...)
								if (Parser.flowOperators.Contains(newStartSubSpanStr)
										|| Parser.propKeywords.Contains(newStartSubSpanStr)
										|| string.Compare(newStartSubSpanStr, "static", true) == 0//Don't join assignments of static class or function variables with any others.
										|| string.Compare(prevStartSubSpanStr, "static", true) == 0)
								{
									StartNewLine(newLineStr);
								}
								else//All other cases, join.
								{
									AddToPreviousLine(prevLine, newLineStr);
								}
							}
						}
						else//Other cases.
						{
							var newIsHotkey = newLineStr.FindFirstNotInQuotes("::") != -1;
							var prevIsHotkey = prevLine.Code.FindFirstNotInQuotes("::") != -1;
							var prevIsDirective = prevLine.Code.StartsWith('#');

							//New line started with an operator.
							//x := 1//Previous line.
							//+ 2//New line.
							if (newStartSubSpan.StartsWithAnyOf(Parser.nonContExprOperatorsList) == -1
									&& !prevIsHotkey//Ensure previous line wasn't a hotkey because they start with characters that would be mistaken for operators, such as ! and ^.
									&& !newIsHotkey//Ensure same for new.
									&& !prevIsDirective//Ensure previous line wasn't a directive.
									&& (Parser.exprVerbalOperators.Contains(newStartSubSpanStr)
										|| newStartSubSpan.StartsWithAnyOf(Parser.contExprOperatorsList) != -1))//Verbal operators must match the token, others can just be the start of the string.
							{
								AddToPreviousLine(prevLine, newLineStr);
							}
							else
							{
								//Previous line ended with an operator.
								//x := 1 +//Previous line.
								//2//New line.
								if (!prevIsHotkey//Ensure previous line wasn't a hotkey because they start with characters that would be mistaken for operators, such as ! and ^.
										&& !newIsHotkey//Ensure same for new.
										&& !prevIsDirective//Ensure previous line wasn't a directive.
										&& prevEndSubSpan.EndsWithAnyOf(Parser.nonContExprOperatorsList) == -1//Ensure we don't count ++ and -- as continuation operators.
										&& (Parser.contExprOperators.Contains(prevEndSubSpanStr) || prevEndSubSpan.EndsWithAnyOf(Parser.contExprOperatorsList) != -1)
								   )
								{
									if (prevEndSubSpanStr.EndsWith(':') && !lastNested && !prevLineSpan.Contains('?'))//Special check to differentiate labels, and also make sure it wasn't part of a ternary operator.
									{
										StartNewLine(newLineStr);
									}
									else
									{
										AddToPreviousLine(prevLine, newLineStr);
									}
								}
								else
								{
									var isPropLine = Parser.propKeywords.Contains(newStartSubSpanStr);
									var wasPropLine = Parser.propKeywords.Contains(prevStartSubSpanStr);
									var wasFuncDef = prevLine.Code.FindFirstNotInQuotes("(") != -1 && prevLine.Code.FindFirstNotInQuotes(")") != -1;

									if (prevParenlevels == 0 && prevBracelevels == 1 && prevBracketlevels == 0)
									{
										//Previous line was in an imbalanced state with only 1 extra open brace { and
										//the beginning of the line is a flow statement. This is OTB style and is not
										//a continuation line, so add a new line.
										//if (...) {
										if (prevLineSpan.EndsWith(openBraceSpan)
												&& (prevIsHotkey//::d {
													|| otbFlowKeywords.Contains(prevStartSubSpanStr)//if (...) {
													|| wasPropLine//get {
													|| wasFuncDef)//myfunc() {
										   )
										{
											//if (...) {
											//} else...
											//...or
											//d:: {
											//...
											if (newLineSpan.StartsWith(closeBraceSpan) && newLineSpan.Length > 1)
											{
												var newLineRest = newLineSpan.Slice(1).Trim(Spaces).ToString();
												var newCloseBrace = "}";
												StartNewLine(newCloseBrace);
												StartNewLine(newLineRest);
											}
											else
												StartNewLine(newLineStr);

											goto EndLine;
										}
									}

									//Previous line was non OTB flow or function definition and this line starts with { as expected,
									//but there is unexpected code after.
									//Place the unexpected code on its own line.
									//if (...)//Previous line.
									//{ x := 123//New line.
									//...
									if ((otbFlowKeywords.Contains(prevStartSubSpanStr) || wasFuncDef)
											&& !prevLineSpan.EndsWith(openBraceSpan)
											&& newLineSpan.StartsWith(openBraceSpan)
											&& newLineSpan.Length > 1)
									{
										var newLineRest = newLineSpan.Slice(1).Trim(Spaces).ToString();
										var newOpenBrace = "{";
										StartNewLine(newOpenBrace);
										StartNewLine(newLineRest);
									}
									else if (newLineSpan.StartsWith(closeBraceSpan) && newLineSpan.Length > 1)
									{
										var newLineRest = newLineSpan.Slice(1).Trim(Spaces);
										var newLineRestFirstTokenIndex = newLineRest.IndexOfAny(SpaceTabOpenParenSv);
										//Check for closing of a function call or fat arrow function like:
										//})
										var newLineRestFirstTokenSpan = newLineRestFirstTokenIndex == -1 ? newLineRest : newLineRest.Slice(0, newLineRestFirstTokenIndex);

										if (!otbFlowKeywords.Contains(newLineRestFirstTokenSpan.ToString()))
										{
											prevLine.Code += newLineStr;
											_ = sb.Clear();
											goto EndLine;
										}

										//Check for the start of a new flow statement like:
										//} else {
										StartNewLine("}");

										if (newLineRestFirstTokenIndex > 0 && newLineRest.Length > newLineRestFirstTokenIndex)
										{
											var flowBraceIndex = newLineRest.IndexOf('{');

											if (flowBraceIndex != -1)
											{
												StartNewLine(newLineRest.Slice(0, flowBraceIndex).ToString());
												StartNewLine("{");
												StartNewLine(newLineRest.Slice(flowBraceIndex + 1).TrimStart(Spaces).ToString());
											}
											else//There wasn't a {, but there could have been more flow statements afteward, such as else if.
												StartNewLine(newLineRest.ToString());
										}
										else//Length matched exactly, so it was just a } else.
											StartNewLine(newLineRest.ToString());
									}
									//Previous line was a standalone open or close brace, so don't join.
									//This will have only happened if the line before it was not a multiline statement.
									//if (...)
									//{//Previous line.
									//  x := 123//New line.
									//...
									else if (prevLine.Code == "{")
									{
										StartNewLine(newLineStr);
									}
									else if (prevLine.Code == "}")
									{
										StartNewLine(newLineStr);
									}
									//Previous line was balanced and the new line is imbalanced in a non-OTB manner.
									//x := 1//Previous line.
									//y := [//New line.
									//...
									else if (!lastNested && anyNested)
									{
										StartNewLine(newLineStr);
									}
									//Previous line was imbalanced in a non-OTB manner, but the new line balanced it.
									//x := [//Previous line.
									//1, 2]//New line.
									else if (lastNested && !anyNested && !isPropLine && !wasPropLine)
									{
										AddToPreviousLine(prevLine, newLineStr);
									}
									//Previous and/or new line is imbalanced in a non-OTB manner.
									//x := [//Previous line.
									//1//New line.
									//...
									else if (anyNested && !isPropLine && !wasPropLine)
									{
										AddToPreviousLine(prevLine, newLineStr);
									}
									else//No other special cases detected, so just add it as a new line.
									{
										StartNewLine(newLineStr);
									}
								}
							}
						}
					}

					EndLine:

					if (!commentIgnore)
						last = ch;
				}

				prev = span;
			}

			if (sb.Length > 0)//If anything is left over, add it.
			{
				var newLineStr = sb.ToString().Trim(Spaces);
				StartNewLine(newLineStr);
			}

			foreach (var extraline in extralines)
				list.Add(extraline);

			if (list.Count > 0)//Entire file could have just been comments, or empty.
			{
				lineNumber = list[0].LineNumber;

				foreach (var codeLine in list)
				{
					var s = codeLine.Code;
					Parser.Translate(codeLine, ref s);

					if (s == null)
						throw new ParseException($"Could not translate line {s}", codeLine);

					codeLine.Code = s;
					codeLine.LineNumber = lineNumber++;
				}
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
	}
}