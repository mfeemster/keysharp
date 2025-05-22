using static Keysharp.Scripting.Parser;
using Antlr4.Runtime;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Keysharp.Scripting
{
	internal class PreReader
	{
		private static readonly char[] libBrackets = ['<', '>'];
		private static int hotifcount;
		private readonly Stack<(bool, bool)> currentDefines = new ();

		private readonly HashSet<string> defines =
			[
				"KEYSHARP",
#if WINDOWS
				"WINDOWS",
#elif LINUX
				"LINUX",
#endif
			];

		private readonly List<string> includes = [];
		private readonly Parser parser;
		private string includePath = "./";
		private CompilerHelper tempCompiler = null;
		internal int NextHotIfCount => ++hotifcount;
		internal List<(string, bool)> PreloadedDlls { get; } = [];
		
		internal eScriptInstance SingleInstance { get; private set; } = eScriptInstance.Prompt;

		internal PreReader(Parser p) => parser = p;

        internal string[,] accessorReplaceTemplate = new[,]//These will need to be done differently on linux.//LINUXTODO
{
				// The order of the first three must not be changed without altering the order in ReadTokens
                { "%A_LineFile%", "" },
                { "%A_ScriptDir%", "" },
				{ "%A_ScriptFullPath%", "" },
                { "%A_AhkPath%", Accessors.A_AhkPath },
                { "%A_AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) },
                { "%A_AppDataCommon%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) },
                { "%A_ComputerName%", Accessors.A_ComputerName },
#if WINDOWS
				{ "%A_ComSpec%", Accessors.A_ComSpec },
#endif
				{ "%A_Desktop%", Accessors.A_Desktop },
                { "%A_DesktopCommon%", Accessors.A_DesktopCommon },
                { "%A_IsCompiled%", Accessors.A_IsCompiled.ToString() },
                { "%A_KeysharpPath%", Accessors.A_KeysharpPath },
                { "%A_MyDocuments%", Accessors.A_MyDocuments },
                { "%A_ProgramFiles%", Accessors.A_ProgramFiles },
                { "%A_Programs%", Accessors.A_Programs },
                { "%A_ProgramsCommon%", Accessors.A_ProgramsCommon },

                { "%A_ScriptName%", Accessors.A_ScriptName },
                { "%A_Space%", Accessors.A_Space },
                { "%A_StartMenu%", Accessors.A_StartMenu },
                { "%A_StartMenuCommon%", Accessors.A_StartMenuCommon },
                { "%A_Startup%", Accessors.A_Startup },
                { "%A_StartupCommon%", Accessors.A_StartupCommon },
                { "%A_Tab%", Accessors.A_Tab },
                { "%A_Temp%", Accessors.A_Temp },
                { "%A_UserName%", Accessors.A_UserName },
#if WINDOWS
				{ "%A_WinDir%", Accessors.A_WinDir },
#endif
			};

        internal List<IToken> ReadScriptTokens(TextReader source, string name)
		{
            if (Env.FindCommandLineArgVal("include") is string cmdinc)
            {
                if (File.Exists(cmdinc))
                {
                    if (!includes.Contains(cmdinc))
                    {
                        _ = includes.AddUnique(cmdinc);
                        using var reader = new StreamReader(cmdinc);
                        parser.codeTokens.AddRange(ReadTokens(reader, cmdinc));
                    }
                }
                else
                    throw new ParseException($"Command line include file {cmdinc} specified with -/include not found.", 0, "");
            }

            parser.codeTokens.AddRange(ReadTokens(source, name));
			return parser.codeTokens;
		}

		internal List<IToken> ReadTokens(TextReader source, string name)
		{
			var replace = (string[,])accessorReplaceTemplate.Clone();
			replace[0, 1] = name; //Note that Name, with a capital N, is the initial script file, not any of the included files.
			replace[1, 1] = Path.GetDirectoryName(parser.name);
			replace[2, 1] = parser.name;

            includePath = name = File.Exists(name) ? Path.GetFullPath(name) : "./";

			String codeText = source.ReadToEnd() + "\n";
            var codeLines = Regex.Split(codeText, "\r\n|\r|\n");

            AntlrInputStream inputStream = new AntlrInputStream(codeText);
			inputStream.name = name;
            MainLexer preprocessorLexer = new MainLexer(inputStream);

            List<IToken> codeTokens = new List<IToken>();
            List<IToken> commentTokens = new List<IToken>();

            var tokens = preprocessorLexer.GetAllTokens();
            var directiveTokens = new List<IToken>();
            var directiveTokenSource = new ListTokenSource(directiveTokens);
            var directiveTokenStream = new CommonTokenStream(directiveTokenSource, MainLexer.DIRECTIVE);
            PreprocessorParser preprocessorParser = new PreprocessorParser(directiveTokenStream);

            int index = 0;
            bool compiliedTokens = true;
            while (index < tokens.Count && tokens[index].Type == MainLexer.WS)
                index++;
            while (index < tokens.Count)
            {
                var token = tokens[index];
                if (token.Type == MainLexer.Hashtag)
                {
                    directiveTokens.Clear();
                    int directiveTokenIndex = index + 1;
                    // Collect all preprocessor directive tokens.
                    while (directiveTokenIndex < tokens.Count &&
                        tokens[directiveTokenIndex].Type != MainLexer.Eof &&
                        tokens[directiveTokenIndex].Type != MainLexer.DirectiveNewline &&
                        tokens[directiveTokenIndex].Type != MainLexer.Hashtag)
                    {
                        if (tokens[directiveTokenIndex].Channel != Lexer.Hidden)
                        {
                            directiveTokens.Add(tokens[directiveTokenIndex]);
                        }
                        directiveTokenIndex++;
                    }

                    directiveTokenSource = new ListTokenSource(directiveTokens);
                    directiveTokenStream = new CommonTokenStream(directiveTokenSource, MainLexer.DIRECTIVE);
                    preprocessorParser.TokenStream = directiveTokenStream;

                    //preprocessorParser.SetInputStream(directiveTokenStream);
                    preprocessorParser.Reset();

                    // Parse condition in preprocessor directive (based on CSharpPreprocessorParser.g4 grammar).
                    PreprocessorParser.Preprocessor_directiveContext directive = preprocessorParser.preprocessor_directive();
                    // if true than next code is valid and not ignored.
                    compiliedTokens = directive.value;

                    String directiveStr = tokens[index + 1].Text.Trim().ToUpper();
                    String conditionalSymbol = null;
                    var lineNumber = token.Line;
                    var code = codeLines[lineNumber - 1];
                    var includeOnce = false;

                    switch (directiveStr)
					{
                        case "IF":
                        case "ELIF":
                        case "ELSE":
                        case "REGION":
                        case "NULLABLE":
                            // Leave compiliedTokens untouched such that it affects whether the following
							// non-directive tokens will be interpreted or not.
                            break;
                        case "DEFINE":
                            // add to the conditional symbols 
                            conditionalSymbol = tokens[index + 2].Text;
                            preprocessorParser.ConditionalSymbols.Add(conditionalSymbol);
                            compiliedTokens = true;
                            break;
                        case "UNDEF":
                            conditionalSymbol = tokens[index + 2].Text;
                            preprocessorParser.ConditionalSymbols.Remove(conditionalSymbol);
                            compiliedTokens = true;
                            break;
                        default:
							compiliedTokens = true;
							break;
                    }

					// Process some AHK directives which are easier to handle here rather than PreprocessorParserBase.cs
                    switch (directiveStr)
					{
                        case "DLLLOAD":
                            {
                                var p1 = tokens[index + 2].Text;
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
                        case "INCLUDE":
                            includeOnce = true;

                            goto case "INCLUDEAGAIN";

                        case "INCLUDEAGAIN":
                            {
								var p1 = tokens[index + 2].Text;
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
                                        paths.Add($"{includePath}\\{p1}");//Folder relative to the script file, or as overridden.
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
                                        if (File.Exists(dir))
                                        {
                                            found = true;

                                            if (includeOnce && includes.Contains(dir))
                                                break;

                                            _ = includes.AddUnique(dir);
                                            using var dirReader = new StreamReader(dir);
                                            parser.codeTokens.AddRange(ReadTokens(dirReader, dir));
                                            break;
                                        }
                                    }

                                    if (!found && !silent)
                                        throw new ParseException($"Include file {p1} not found at any of the locations: {string.Join(Environment.NewLine, paths)}", token.Line, tokens[index].Text + tokens[index + 1].Text + tokens[index + 2].Text);
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
                                    else if (File.Exists(path))
                                    {
                                        if (includeOnce && includes.Contains(path))
                                            break;

                                        _ = includes.AddUnique(path);
                                        using var pathReader = new StreamReader(path);
                                        codeTokens.AddRange(ReadTokens(pathReader, path));
                                    }
                                    else
                                    {
                                        if (!silent)
                                            throw new ParseException($"Include file {p1} not found at location {path}", token.Line, tokens[index].Text + tokens[index + 1].Text + tokens[index + 2].Text);

                                        break;
                                    }
                                }
                            }
                            break;
                        case "REQUIRES":
                        {
                            var p1 = tokens[index + 2].Text;
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
                                    if (reqAhk && (ver.StartsWith("2.") || ver == "2"))
                                        break;

                                    if (!reqvers.Any(x => x != 0))
                                        throw new ParseException($"This script requires {p1}", lineNumber, name);

                                    Script.TheScript.VerifyVersion(ver, plus, lineNumber, code);

									//In addition to being checked here, it must be added to the code for when it runs as a compiled exe.
									parser.mainFuncInitial.Add($"Keysharp.Scripting.Script.VerifyVersion({ver}, {plus}, 0, name);");
                                    //Sub release designators such as "-alpha", "-beta" are not supported in C#. Only the assembly version is supported.
                                }
                            }
							break;
                        }
						case "SINGLEINSTANCE":
                            switch (tokens[index + 2].Text.ToUpperInvariant())
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
                                    SingleInstance = eScriptInstance.Force;
                                    break;
                            }
							break;
                        case "NODYNAMICVARS":
                            parser.DynamicVars = false;
                            break;

                        case "PERSISTENT":
							var nextTokenText = tokens[index + 2].Text.ToLowerInvariant().Trim();
                            parser.persistent = !(nextTokenText == "false" || nextTokenText == "0");
                            break;

                        case "ERRORSTDOUT":
							parser.errorStdOut = true;
							break;
                        case "CLIPBOARDTIMEOUT":
                        case "HOTIFTIMEOUT":
                        case "MAXTHREADS":
                        case "MAXTHREADSBUFFER":
                        case "MAXTHREADSPERHOTKEY":
                        case "ASSEMBLYTITLE":
                        case "ASSEMBLYDESCRIPTION":
                        case "ASSEMBLYCONFIGURATION":
                        case "ASSEMBLYCOMPANY":
                        case "ASSEMBLYPRODUCT":
                        case "ASSEMBLYCOPYRIGHT":
                        case "ASSEMBLYTRADEMARK":
						case "ASSEMBLYVERSION":
							parser.generalDirectives[directiveStr] = tokens[index + 2].Text.Trim();
                            break;
                        case "WINACTIVATEFORCE":
                        case "NOTRAYICON":
							parser.generalDirectives[directiveStr] = "1";
							break;
                        case "WARN":
                            Console.WriteLine("Not implemented");
                            break;
                    }
                    index = directiveTokenIndex - 1;
                }
                else if (token.Channel == Lexer.DefaultTokenChannel && compiliedTokens)
                {
                    int i;
                    switch (token.Type)
                    {
                        case MainLexer.OpenBrace:
                        case MainLexer.OpenBracket:
                        case MainLexer.DerefStart:
                            break;
                        case MainLexer.CloseBracket:
                        case MainLexer.DerefEnd:
                            break;
                        case MainLexer.OpenParen:
                            SkipWhitespaces(index);
                            break;
                        case MainLexer.Comma:
                            SkipWhitespaces(index);
                            i = codeTokens.Count;
                            while (--i > 0 && codeTokens.Count > 0)
                            {
                                if (codeTokens[i].Channel != Lexer.DefaultTokenChannel)
                                    continue;
                                if (codeTokens[i].Type == MainLexer.WS)
                                    continue;
                                else if (codeTokens[i].Type == MainLexer.EOL)
                                    codeTokens.RemoveAt(i);
                                else
                                    break;
                            }
                            break;
                        case MainLexer.CloseParen:
                            PopWhitespaces(codeTokens.Count);
                            break;
                        case MainLexer.EOL:
						case MainLexer.Dot:
                        case MainLexer.Assign:
                        case MainLexer.Divide:
                        case MainLexer.IntegerDivide:
                        case MainLexer.Power:
                        case MainLexer.NullCoalesce:
                        case MainLexer.RightShiftArithmetic:
                        case MainLexer.LeftShiftArithmetic:
                        case MainLexer.RightShiftLogical:
                        case MainLexer.LessThan:
                        case MainLexer.MoreThan:
                        case MainLexer.LessThanEquals:
                        case MainLexer.GreaterThanEquals:
                        case MainLexer.Equals_:
                        case MainLexer.NotEquals:
                        case MainLexer.IdentityEquals:
                        case MainLexer.IdentityNotEquals:
                        case MainLexer.RegExMatch:
                        case MainLexer.BitXOr:
                        case MainLexer.BitOr:
                        case MainLexer.And:
                        case MainLexer.Or:
                        case MainLexer.VerbalAnd:
                        case MainLexer.VerbalOr:
                        case MainLexer.MultiplyAssign:
                        case MainLexer.DivideAssign:
                        case MainLexer.ModulusAssign:
                        case MainLexer.PlusAssign:
                        case MainLexer.MinusAssign:
                        case MainLexer.LeftShiftArithmeticAssign:
                        case MainLexer.RightShiftArithmeticAssign:
                        case MainLexer.RightShiftLogicalAssign:
                        case MainLexer.IntegerDivideAssign:
                        case MainLexer.ConcatenateAssign:
                        case MainLexer.BitAndAssign:
                        case MainLexer.BitXorAssign:
                        case MainLexer.BitOrAssign:
                        case MainLexer.PowerAssign:
                        case MainLexer.NullishCoalescingAssign:
                        case MainLexer.QuestionMarkDot:
                        case MainLexer.Arrow:
                            PopWhitespaces(codeTokens.Count);
                            SkipWhitespaces(index);
                            break;
                        case MainLexer.Try:
                        case MainLexer.If:
                        case MainLexer.Catch:
                        case MainLexer.Finally:
                        case MainLexer.As:
                        case MainLexer.Loop:
                        case MainLexer.LoopFiles:
                        case MainLexer.LoopParse:
                        case MainLexer.LoopRead:
                        case MainLexer.LoopReg:
                        case MainLexer.For:
                        case MainLexer.Switch:
                        case MainLexer.Case:
                        case MainLexer.Default:
                        case MainLexer.While:
                        case MainLexer.Until:
                        case MainLexer.Else:
                        case MainLexer.Goto:
                        case MainLexer.Throw:
                        case MainLexer.Async:
                        case MainLexer.Static:
						case MainLexer.Not:
                        case MainLexer.VerbalNot:
                            i = index;
                            codeTokens.Add(token);
                            while (++i < tokens.Count)
                            {
                                if (tokens[i].Channel == MainLexer.DIRECTIVE)
                                    break;
								if ((tokens[i].Channel != Lexer.DefaultTokenChannel) || (tokens[i].Type == MainLexer.EOL && (token.Type == MainLexer.Not || token.Type == MainLexer.VerbalNot)))
									index++;
								else if (tokens[i].Type == MainLexer.WS)
								{
									codeTokens.Add(tokens[i]);
                                    index++;
								}
								else
									break;
                            }
                            goto SkipAdd;
                        case MainLexer.CloseBrace:
                            i = PopWhitespaces(codeTokens.Count, false);
                            var eolToken = new CommonToken(MainLexer.EOL)
                            {
                                Line = token.Line,
                                Column = token.Column
                            };
                            if (i >= 0 && codeTokens[i].Type != MainLexer.EOL)
                            {
                                codeTokens.Add(eolToken);
                            }
                            codeTokens.Add(token);
                            i = index;
                            bool eolPresent = false;
                            while (++i < tokens.Count)
                            {
                                if (tokens[i].Channel == MainLexer.DIRECTIVE)
                                    break;
                                if (tokens[i].Channel != Lexer.DefaultTokenChannel)
                                    continue;
                                if (tokens[i].Type == MainLexer.WS)
                                    index++;
                                else if (tokens[i].Type == MainLexer.EOL)
                                {
                                    eolPresent = true;
                                    break;
                                }
                                else
                                    break;
                            }
                            if (!eolPresent)
                                codeTokens.Add(eolToken);
                            goto SkipAdd;
						case MainLexer.HotIf:
						case MainLexer.InputLevel:
						case MainLexer.UseHook:
						case MainLexer.SuspendExempt:
							i = index;
                            while (++i < tokens.Count)
                            {
                                if (tokens[i].Channel == MainLexer.DIRECTIVE)
                                    break;
                                if (tokens[i].Channel != Lexer.DefaultTokenChannel)
                                    index++;
                                else if (tokens[i].Type == MainLexer.WS)
                                    index++;
                                else
                                    break;
                            }
							break;
                        case MainLexer.WS:
                            break;
                        default:
                            break;
                    }
                AddToken:
                    codeTokens.Add(token); // Collect code tokens.
                }
            SkipAdd:
                index++;
            }

            for (int i = 0; i < codeTokens.Count; i++)
            {
                if (codeTokens[i].Type == MainLexer.WS || codeTokens[i].Type == MainLexer.EOL)
                {
                    codeTokens.RemoveAt(i);
                    i--;
                }
                else
                    break;
            }

            int PopWhitespaces(int i, bool linebreaks = true)
            {
                while (--i > 0 && codeTokens.Count > 0)
                {
                    if (codeTokens[i].Channel != Lexer.DefaultTokenChannel)
                        continue;
                    if (codeTokens[i].Type == MainLexer.WS || (linebreaks && codeTokens[i].Type == MainLexer.EOL))
                        codeTokens.RemoveAt(codeTokens.Count - 1);
                    else
                        break;
                }
                return i;
            }

            int SkipWhitespaces(int i, bool linebreaks = true)
            {
                while (++i < tokens.Count)
                {
                    if (tokens[i].Channel == MainLexer.DIRECTIVE)
                        break;
                    if ((tokens[i].Channel != Lexer.DefaultTokenChannel) || tokens[i].Type == MainLexer.WS || (linebreaks && tokens[i].Type == MainLexer.EOL))
                        index++;
                    else
                        break;
                }
                return i;
            }

			/*
            foreach (var token in codeTokens)
            {
                System.Diagnostics.Debug.WriteLine($"Token: {preprocessorLexer.Vocabulary.GetSymbolicName(token.Type)}, Text: '{token.Text}'" + (token.Channel == MainLexer.Hidden ? " (hidden)" : ""));
            }
			*/

            return codeTokens;

        }
    }
}