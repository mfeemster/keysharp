using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private bool blockOpen;
		private uint casecount;
		private uint excount;
		private uint switchcount;
		private uint trycount;

		private CodeStatement[] ParseFlow(List<CodeLine> lines, int index)
		{
			var line = lines[index];
			var code = line.Code;
			string[] parts = { string.Empty, string.Empty };
			var offset = 2;
			var delimiters = new char[Spaces.Length + offset];
			delimiters[0] = Multicast;
			delimiters[1] = HotkeyBound;
			Spaces.CopyTo(delimiters, offset);
			int[] d = { code.IndexOfAny(delimiters), code.IndexOfAny(new[] { BlockOpen, ParenOpen }) };

			if (d[0] == -1 && d[1] == -1)
				parts[0] = code;
			else if (d[1] != -1 && (d[1] < d[0] || d[0] == -1))
			{
				parts[0] = code.Substring(0, d[1]);
				parts[1] = code.Substring(d[1], code.Length - d[1]).Trim(Spaces);
			}
			else
			{
				parts[0] = code.Substring(0, d[0]);
				parts[1] = code.Substring(d[0] + 1, code.Length - d[0] - 1).Trim(Spaces);
			}

			if (parts.Length > 1 && IsEmptyStatement(parts[1]))
				parts = new[] { parts[0].Trim() };

			var lower = parts[0].ToLowerInvariant();

			//This is needed becauses elses is popped when an else is encountered, but if and if statement
			//has no else, then the elses will remain and can sometimes affect later code which thinks it's
			//still in an if/else block, even though the block has long since terminated.
			if (parentBlock == null || parentBlock.Kind != CodeBlock.BlockKind.IfElse)
				if (lower != FlowElse && elses.Count > 0)
					elses.Clear();

			switch (lower)
			{
				case FlowIf:
				{
					if (parts.Length < 1)
						throw new ParseException("If requires a parameter", line);

					var condition = ParseFlowParameter(line, parts[1], true, out var blockOpen, false);
					var ifelse = new CodeConditionStatement { Condition = condition };
					var block = new CodeBlock(line, Scope, ifelse.TrueStatements, CodeBlock.BlockKind.IfElse, blocks.PeekOrNull());
					block.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					elses.Push(ifelse.FalseStatements);
					return new CodeStatement[] { ifelse };
				}

				case FlowElse:
				{
					if (parent.Count > 0 && parent[parent.Count - 1] is CodeTryCatchFinallyStatement tcf)
					{
						var left = VarId($"try{trycount}success", true);
						//
						var cas = new CodeAssignStatement(left, new CodeSnippetExpression("false"));
						tcf.TryStatements.Insert(0, cas);
						//
						cas = new CodeAssignStatement(left, new CodeSnippetExpression("true"));
						_ = tcf.TryStatements.Add(cas);
						//
						var condition = ParseFlowParameter(line, $"try{trycount}success", true, out var blockOpen, true);
						var ccs = new CodeConditionStatement { Condition = condition };
						tcf.FinallyStatements.Insert(0, ccs);
						var type = parts.Length > 1 && parts[1][parts[1].Length - 1] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(line, Scope, ccs.TrueStatements, CodeBlock.BlockKind.TryElse, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
					else if (elses.Count > 0)//This must come last otherwise it will take precedence over the special situations above.
					{
						var next = line.Code.TrimStart(Spaces).Substring(FlowElse.Length).TrimStart(Spaces);

						if (!IsEmptyStatement(next))
							lines.Insert(index + 1, new CodeLine(line.FileName, line.LineNumber, next.TrimEnd(BlockOpenAndSpace)));

						var type = parts.Length > 1 && parts[1][parts[1].Length - 1] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(line, Scope, elses.Pop(), CodeBlock.BlockKind.IfElse, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
					else if (parent.Count > 0 && parent[parent.Count - 1] is CodeExpressionStatement ces && ces.Expression is CodeMethodInvokeExpression cmie && cmie.Method.MethodName == "Pop")
					{
						var blockOpen = parts.Length > 1 && parts[1].Trim().EndsWith(BlockOpen);
						parent.RemoveAt(parent.Count - 1);
						var ifelse = new CodeConditionStatement { Condition = new CodeSnippetExpression("Pop().index == 0L") };
						var block = new CodeBlock(line, Scope, ifelse.TrueStatements, CodeBlock.BlockKind.IfElse, blocks.PeekOrNull());
						block.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						_ = CloseTopSingleBlock();
						blocks.Push(block);
						return new CodeStatement[] { ifelse };
					}
					else
						throw new ParseException("Else with no preceeding if, try, for, loop or while block", line);
				}
				break;

				case FlowSwitch:
				{
					var snip = new CodeSnippetStatement();
					var switchargs = new string[1];

					if (parts.Length > 1)
					{
						switchargs = StripCommentSingle(parts[1]).Trim(Parens).Split(Spaces.Append(',').ToArray(), StringSplitOptions.RemoveEmptyEntries);
					}

					var switchvar = switchargs[0];
					var cse = new CodeSwitchStatement(switchvar, switchargs.Length > 1 ? switchargs[1] : null, switchcount++);
					var type = parts.Length > 1 && parts[1][0] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
					var block = new CodeBlock(line, Scope, cse.AllStatements, CodeBlock.BlockKind.Switch, blocks.PeekOrNull()) { Type = type };
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					switches.Push(cse);
				}
				break;

				case FlowCase:
				{
					if (switches.PeekOrNull() is CodeSwitchStatement css)
					{
						if (parentBlock != null && (parentBlock.Kind == CodeBlock.BlockKind.CaseWithBrace || parentBlock.Kind == CodeBlock.BlockKind.CaseWithoutBrace))
						{
							CloseBlock();
						}

						if (parts.Length > 1)//You must keep track here of whether this case starts with a { or not.//TODO
						{
							var colonindex = parts[1].IndexOf(':');
							var casearg = colonindex != -1 ? parts[1].Substring(0, colonindex) : throw new ParseException("Case not terminated with a colon", line);
							var casearr = casearg.Split(Spaces, StringSplitOptions.RemoveEmptyEntries);
							var casename = $"ks_caselabel{casecount++}";
							colonindex++;

							if (colonindex < parts[1].Length)
							{
								var next = parts[1].Substring(colonindex).TrimStart(Spaces);

								if (!IsEmptyStatement(next))
									lines.Insert(index + 1, new CodeLine(line.FileName, line.LineNumber, next));
							}

							if (casearr.Length == 1)
							{
								if (css.CaseSense != null)
								{
									var comparg = "System.StringComparison." + css.CaseSense.ToString();
									var cmie = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Compare"));
									_ = cmie.Parameters.Add(new CodeSnippetExpression(css.SwitchVarTempName));
									_ = cmie.Parameters.Add(new CodeSnippetExpression($"{casearg}.ToString()"));
									_ = cmie.Parameters.Add(new CodeSnippetExpression(comparg));
									var coe = new CodeBinaryOperatorExpression(cmie, CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("0"));
									css.CaseExpressions.Add(casename, coe);
								}
								else if (!string.IsNullOrEmpty(css.SwitchVar))
									css.CaseExpressions.Add(casename, $"{css.SwitchVar} == {casearr[0]}");
								else
									css.CaseExpressions.Add(casename, $"{casearr[0]}");
							}
							else
							{
								casearr = casearg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();

								if (!string.IsNullOrEmpty(css.SwitchVar))
									for (var i = 0; i < casearr.Length; i++)
										if (casearr[i].IndexOf(' ') == -1)
											casearr[i] = $"{css.SwitchVar} == {casearr[i]}";

								css.CaseExpressions.Add(casename, string.Join("||", casearr));
							}

							var hasbrace = lines.Count > index + 1 && lines[index + 1].Code.TrimStart(Spaces).StartsWith(BlockOpen);
							var type = CodeBlock.BlockType.Within;//Might want this to always be within to avoid the funny stuff Statements does for normal blocks.
							var block = new CodeBlock(line, Scope, css.CaseBodyStatements.GetOrAdd(casename), hasbrace ? CodeBlock.BlockKind.CaseWithBrace : CodeBlock.BlockKind.CaseWithoutBrace, blocks.PeekOrNull()) { Type = type };
							_ = CloseTopSingleBlock();
							blocks.Push(block);
						}
						else
							throw new ParseException("Case with no argument", line);
					}
					else
						throw new ParseException("Case with no preceeding switch block", line);
				}
				break;

				case FlowDefault:
				{
					if (switches.PeekOrNull() is CodeSwitchStatement css)
					{
						if (parentBlock != null && (parentBlock.Kind == CodeBlock.BlockKind.CaseWithBrace || parentBlock.Kind == CodeBlock.BlockKind.CaseWithoutBrace))
						{
							CloseBlock();
						}

						var hasbrace = lines.Count > index + 1 && lines[index + 1].Code.TrimStart(Spaces).StartsWith(BlockOpen);
						var type = CodeBlock.BlockType.Within;//Might want this to always be within to avoid the funny stuff Statements does for normal blocks.
						var block = new CodeBlock(line, Scope, css.DefaultStatements, hasbrace ? CodeBlock.BlockKind.CaseWithBrace : CodeBlock.BlockKind.CaseWithoutBrace, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
				}
				break;

				case FlowGosub://Might be good to see if this call can be made directly rather than through reflection with LabelCall(). Might still need it when the call is with a variable.//MATT
				{
					throw new ParseException("Gosub is no longer supported, use a label with goto instead.", line);
				}

				case FlowGoto:
				{
					return parts.Length < 1 ? throw new ParseException("No label specified", line)
					: (new CodeStatement[] { new CodeGotoStatement(parts[1]) });
				}

				case FlowLoop:
				{
					var trimmed = StripComment(parts.Last());
					var blockOpen = trimmed.EndsWith(BlockOpen);

					if (blockOpen)
						parts[parts.Length - 1] = trimmed.Trim(new char[] { BlockOpen, ' ' });
					var skip = true;
					//var checkBrace = true;
					CodeMethodInvokeExpression iterator;

					if (parts.Length > 1 && parts.Last() != string.Empty)//If the last char was a {, then it was trimmed and replaced with a "".
					{
						var sub = parts[1].Split(Spaces, 2);
						var regex = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");//Gotten from https://stackoverflow.com/questions/3147836/c-sharp-regex-split-commas-outside-quotes
						var sub2 = sub.Length > 1 ? regex.Split(sub[1])/*.Select(x => x.Trim(SpacesQuotes)).ToArray()*/ : new string[] { };
						//sub2[0] = sub2[0].Trim(new char[] { '"' });
						//sub = new[] { sub[0].Trim(), sub.Length > 1 ? sub[1].Trim() : string.Empty };
						sub = new string[] { sub[0] }.Concat(sub2);

						switch (sub[0].ToUpperInvariant())
						{
							case "READ":
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopRead;
								break;

							case "PARSE":
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopParse;
								break;

							case "REG":
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopRegistry;
								break;

							case "EACH":
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopEach;
								break;

							case "FILES":
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopFile;
								break;

							default:
							{
								//var file = false;
								//if (parts[1].IndexOf(Multicast) != -1)
								//  file = true;
								//// TODO: check file/iteration loop types
								skip = false;
								sub = sub.Select(x => x.Trim(SpacesQuotes)).ToArray();
								//iterator = (CodeMethodInvokeExpression)(file ? InternalMethods.LoopFile : InternalMethods.Loop);
								iterator = (CodeMethodInvokeExpression)InternalMethods.Loop;
							}
							break;
						}

						//if (skip)
						//parts[1] = sub[1];

						//if (checkBrace)
						//{
						//  // TODO: check expression parameters before stripping comments
						//  //var x = parts.Length == 1 ? 0 : 1;
						//  //var part = StripComment(parts[x]).TrimEnd(Spaces);
						//  //var l = part.Length - 1;
						//  //if (part.Length > 0 && part[l] == BlockOpen)
						//  //{
						//  //  blockOpen = true;
						//  //  parts[x] = part.Substring(0, l);
						//  //}
						//  var nocomm = StripComment(sub.Last());
						//
						//  if (nocomm.EndsWith(BlockOpen))
						//  {
						//      //blockOpen = true;
						//      sub[sub.Length - 1] = nocomm.Trim(new char[] { BlockOpen, ' ' });
						//  }
						//}

						if (skip && parts[1].Length == 0)
							throw new ParseException("Loop type must have an argument", line);

						sub = skip ? sub.Skip(1).ToArray() : sub;

						foreach (var s in sub)
						{
							var arg = s;
							//foreach (var arg in SplitCommandParameters(s))//MATT
							_ = iterator.Parameters.Add(ParseCommandParameter(arg.Trim(), false, true));
						}
					}
					else
					{
						iterator = (CodeMethodInvokeExpression)InternalMethods.Loop;
						_ = iterator.Parameters.Add(new CodePrimitiveExpression(-1));
					}

					var id = InternalID;
					var init = new CodeVariableDeclarationStatement();
					init.Name = id;
					init.Type = new CodeTypeReference(typeof(IEnumerator));
					init.InitExpression = new CodeMethodInvokeExpression(iterator, "GetEnumerator", System.Array.Empty<CodeExpression>());
					var condition = new CodeMethodInvokeExpression();
					condition.Method.TargetObject = new CodeVariableReferenceExpression(id);
					condition.Method.MethodName = "MoveNext";
					var loop = new CodeIterationStatement
					{
						InitStatement = init,
						IncrementStatement = new CodeSnippetStatement(string.Empty),
						TestExpression = condition
					};
					var block = new CodeBlock(line, Scope, loop.Statements, CodeBlock.BlockKind.Loop, blocks.PeekOrNull(), InternalID, InternalID);
					block.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);
					return new CodeStatement[] { loop, new CodeLabeledStatement(block.ExitLabel), pop };
				}

				case FlowFor:
				{
					//The documentation shows this working with function objects, where the constructor returns an enumeration object, and the
					//Call() method is called each time the enumerator is incremented (like MoveNext() would be calling it under the hood).
					//The class Enumerator has one method, which is Call (you can think of it inheriting from Func, which also has that method).
					//However, we've not implemented that here yet.//TODO
					var trimmed = StripCommentSingle(parts[1]).Trim(Parens);
					var temp = trimmed.Split(" in ");

					if (temp.Length == 2)
					{
						var coldeclid = InternalID;
						var testid = InternalID;
						var id = InternalID;
						var blockOpen = trimmed.EndsWith(BlockOpen);
						var expr = ParseSingleExpression(temp.Last(), false);
						var col = Ch.CodeToString(expr);

						if (blockOpen)
							parts[parts.Length - 1] = trimmed.Trim(new char[] { BlockOpen, ' ' });
						var varsplits = temp[0].Split(',', StringSplitOptions.TrimEntries).ToList();

						if (varsplits.Count == 1)//If only one present, use second because it's the value, instead of the index/key.
							varsplits.Insert(0, "");

						var varlist = new List<string>(varsplits.Count);
						var enumlist = new List<string>(varsplits.Count);

						//If only one is present, it actually means discard the first one, and use the second.
						//So in the case of a map, it would mean get the values, not the key.
						//In the case of an array, it would mean get the values, not the index.
						foreach (var split in varsplits)
						{
							enumlist.Add("object");

							if (split != "")
							{
								var loopvarexpr = ParseSingleExpression(split, true);//We do want to create the iteration loop vars as local function vars.
								var loopvarstr = Ch.CodeToString(loopvarexpr);
								varlist.Add(split?.Length == 0 ? "_" : loopvarstr);
							}
							else
								varlist.Add(split?.Length == 0 ? "_" : split);
						}

						var vars = string.Join(',', varlist);
						var enums = string.Join(',', enumlist);
						var coldecl = new CodeExpressionStatement(new CodeSnippetExpression($"var {coldeclid} = {col}"));
						var iterdecl = new CodeExpressionStatement(new CodeSnippetExpression($"var {id} = {coldeclid} is IEnumerator<({enums})> {testid} ? {testid} : ((IEnumerable<({enums})>){coldeclid}).GetEnumerator()"));
						_ = parent.Add(new CodeSnippetExpression("{"));
						_ = parent.Add(coldecl);
						_ = parent.Add(iterdecl);
						var condition = new CodeMethodInvokeExpression();
						condition.Method.TargetObject = new CodeVariableReferenceExpression(id);
						condition.Method.MethodName = "MoveNext";
						var loop = new CodeIterationStatement
						{
							TestExpression = condition,
							InitStatement = new CodeSnippetStatement(string.Empty),
							IncrementStatement = new CodeSnippetStatement(string.Empty)
						};
						loop.Statements.Insert(0, new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Inc));
						_ = loop.Statements.Add(new CodeSnippetExpression($"/*preventtrim*/({vars}) = {id}.Current"));
						var block = new CodeBlock(line, Scope, loop.Statements, CodeBlock.BlockKind.Loop, blocks.PeekOrNull(), InternalID, InternalID)
						{
							Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect
						};
						_ = CloseTopSingleBlock();
						blocks.Push(block);//Must add block first, before doing local variable names so that the scoping is correct and they don't conflict with function level vars.
						var push = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Push);
						var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);
						//return new CodeStatement[] { push, loop, new CodeLabeledStatement(block.ExitLabel), pop, new CodeSnippetStatement("}") };
						return new CodeStatement[] { push, loop, new CodeLabeledStatement(block.ExitLabel), pop, new CodeExpressionStatement(new CodeSnippetExpression("}")) };
					}
				}
				break;

				case FlowWhile:
				{
					var blockOpen = false;
					var condition = parts.Length > 1 ? ParseFlowParameter(line, parts[1], true, out blockOpen, true) : new CodePrimitiveExpression(true);
					var loop = new CodeIterationStatement
					{
						TestExpression = condition,
						InitStatement = new CodeSnippetStatement(string.Empty),
						IncrementStatement = new CodeSnippetStatement(string.Empty)
					};
					loop.Statements.Insert(0, new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Inc));
					var block = new CodeBlock(line, Scope, loop.Statements, CodeBlock.BlockKind.Loop, blocks.PeekOrNull(), InternalID, InternalID)
					{
						Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect
					};
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					var push = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Push);
					var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);
					return new CodeStatement[] { push, loop, new CodeLabeledStatement(block.ExitLabel), pop };
				}

				case FlowUntil:
				{
					if (parent.Cast<CodeStatement>().Last(cs => cs is CodeIterationStatement) is CodeIterationStatement cis)
					{
						var ccs = new CodeConditionStatement();
						var token = StripCommentSingle(parts[1]);
						var expr = ParseSingleExpression(token, false);
						var iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
						_ = iftest.Parameters.Add(expr);
						ccs.Condition = iftest;
						_ = ccs.TrueStatements.Add(new CodeSnippetExpression("break"));
						_ = cis.Statements.Add(ccs);
					}
				}
				break;

				case FlowBreak:
					var b = 1;

					if (parts.Length > 1)
					{
						parts[1] = StripCommentSingle(parts[1]);

						if (!int.TryParse(parts[1], out b) || b < 1)
							throw new ParseException("Break parameter must be a static integer greater than zero", line);
					}

					var exit = PeekLoopLabel(true, b);

					if (exit == null)
						throw new ParseException("Cannot break outside a loop", line);//This needs to work with switch/case.//MATT

					return new CodeStatement[] { new CodeExpressionStatement(new CodeSnippetExpression("break")) };

				case FlowContinue:
					var c = 1;

					if (parts.Length > 1)
					{
						parts[1] = StripCommentSingle(parts[1]);

						if (!int.TryParse(parts[1], out c) || c < 1)
							throw new ParseException("Continue parameter must be a static integer greater than zero", line);
					}

					var cont = PeekLoopLabel(false, c);

					if (cont == null)
						throw new ParseException("Cannot continue outside a loop", line);

					//Need to use a goto instead of a continue statement because Until statements still need to be executed after continue.
					return new CodeStatement[] { new CodeExpressionStatement(new CodeSnippetExpression($"goto {cont}")) };

				case FlowReturn:
					if (Scope?.Length == 0)
					{
						_ = CloseTopLabelBlock();//MATT

						if (parts.Length > 1)
							throw new ParseException("Cannot have return parameter for entry point method", line);

						return new CodeStatement[] { new CodeMethodReturnStatement() };
					}
					else
					{
						var result = parts.Length > 1 ? ParseSingleExpression(parts[1], false) : new CodePrimitiveExpression("");
						return new CodeStatement[] { new CodeMethodReturnStatement(result) };
					}

				case FunctionLocal:
					if (localFuncVars.PeekOrNull() is List<string> lflist)
					{
						var parencount = 0;
						var temptoks = SplitTokens(parts[1]);//Find the variable names because they needed to be added to the local vars before ParseMultiExpression() is called so they get properly added.
						var funclocalvarinitstatements = new List<CodeStatement>(temptoks.Count);

						for (var ti = 0; ti < temptoks.Count; ti++)
						{
							if (temptoks[ti] is string tok)
							{
								if (tok[0] == '(' || tok.EndsWith('('))//Make sure the comma is not because it's using a function with multiple arguments to init. ) will be at the end of function call tokens.
									parencount++;
								else if (tok[0] == ')')
									parencount--;

								if (parencount == 0 && (ti == 0 || (temptoks[ti - 1] as string)[0] == ','))
									lflist.Add(tok);
							}
						}

						//Now that we know the variable names, reparse, but pass true to create them.
						//Unlike global, we do want to create any variable we encounter here as local ones.
						foreach (var expr in ParseMultiExpression(parts[1], true))
							if (expr is CodeExpressionStatement ces &&
									ces.Expression is CodeBinaryOperatorExpression cboe &&
									cboe.Left is CodeVariableReferenceExpression cvre)
								funclocalvarinitstatements.Add(new CodeExpressionStatement(cboe));

						if (funclocalvarinitstatements.Count > 0)
							return funclocalvarinitstatements.ToArray();
					}

					break;

				case FunctionGlobal:
					if (parts.Length > 1)
					{
						var mutltiexprs = ParseMultiExpression(parts[1], false);//Do not create any variables based on what is parsed from the global variable initialization statements.

						if (globalFuncVars.PeekOrNull() is List<string> gflist)
						{
							var funcglobalvarinitstatements = new List<CodeStatement>(mutltiexprs.Length);

							foreach (var expr in mutltiexprs)
							{
								if (expr is CodeExpressionStatement ces)
								{
									if (ces.Expression is CodeBinaryOperatorExpression cboe)
									{
										if (cboe.Left is CodeVariableReferenceExpression cvre)
										{
											cvre.VariableName = cvre.VariableName.Trim('%').ToLower();//ParseMultiExpression() knows we are in a function, so it will attach the scope. But this is a global variable reference, so remove it plus the _.
											gflist.Add(cvre.VariableName);
											funcglobalvarinitstatements.Add(new CodeExpressionStatement(cboe));
										}
									}
									else if (ces.Expression is CodeVariableReferenceExpression cvre)
										gflist.Add(cvre.VariableName.Trim('%').ToLower());
								}
							}

							if (funcglobalvarinitstatements.Count > 0)
								return funcglobalvarinitstatements.ToArray();
						}
					}
					else if (parts.Length == 1)
					{
						_ = allGlobalVars.TryPop(out _);
						allGlobalVars.Push(true);
					}

					break;

				case FunctionStatic:
					if (parts.Length > 1)
					{
						if (staticFuncVars.PeekOrNull() is Dictionary<string, CodeExpression> dkt)
						{
							var parencount = 0;
							var temptoks = SplitTokens(parts[1]);//Find the variable names because they needed to be added to the static vars before ParseMultiExpression() is called so they get properly added.

							for (var ti = 0; ti < temptoks.Count; ti++)
							{
								if (temptoks[ti] is string tok)
								{
									if (tok[0] == '(' || tok.EndsWith('('))//Make sure the comma is not because it's using a function with multiple arguments to init. ) will be at the end of function call tokens.
										parencount++;
									else if (tok[0] == ')')
										parencount--;

									if (parencount == 0 && (ti == 0 || (temptoks[ti - 1] as string)[0] == ','))
										dkt[Scope + ScopeVar + tok] = null;//Must add as scoped for it to work.
								}
							}

							var mutltiexprs = ParseMultiExpression(parts[1], true);//Do not create any variables based on what is parsed from the static variable initialization statements.

							foreach (var expr in mutltiexprs)
							{
								if (expr is CodeExpressionStatement ces)
								{
									if (ces.Expression is CodeBinaryOperatorExpression cboe)
									{
										if (cboe.Left is CodeVariableReferenceExpression cvre)
											dkt[cvre.VariableName.Trim('%').ToLower()] = cboe.Right;
									}
									else if (ces.Expression is CodeVariableReferenceExpression cvre)
										dkt[cvre.VariableName.Trim('%').ToLower()] = null;
								}
							}
						}
					}
					else if (parts.Length == 1)
					{
						_ = allStaticVars.TryPop(out _);
						allStaticVars.Push(true);
					}

					break;

				case Throw:
				{
					var ctes = new CodeThrowExceptionStatement();

					if (parts.Length > 1)
					{
						var token = StripCommentSingle(parts[1]);

						if (IsPrimitiveObject(token, out var obj))
							ctes.ToThrow = new CodeSnippetExpression($"Error({obj})");
						else
							ctes.ToThrow = ParseSingleExpression(token, false);
					}

					return new CodeStatement[] { ctes };
				}

				case FlowTry:
				{
					trycount++;
					var tcf = new CodeTryCatchFinallyStatement();
					var ctch = new CodeCatchClause($"ex{excount++}", new CodeTypeReference("Keysharp.Core.Error"));
					_ = tcf.CatchClauses.Add(ctch);
					var block = new CodeBlock(line, Scope, tcf.TryStatements, CodeBlock.BlockKind.Try, blocks.PeekOrNull())
					{
						Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect
					};
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					return new CodeStatement[] { tcf };
				}

				case FlowCatch:
				{
					//It is impossible to use any type of special state between the try block above and here to determine the level, instead, we must look at whatever the parent block is
					//to get the correct try that this catch was attached to. This is mainly needed because one try can have multiple catch statements.
					if (parent.Count > 0 && parent[parent.Count - 1] is CodeTryCatchFinallyStatement tcf)
					{
						var excname = "";
						var exctypename = "Keysharp.Core.Error";

						if (parts.Length > 1)
						{
							var rest = StripCommentSingle(parts[1]).Trim(Parens).Split(Spaces);

							if (rest.Length > 0 && rest[0] != "as")
								exctypename = "Keysharp.Core." + rest[0];

							if (rest.Length >= 2 && rest[rest.Length - 2] == "as")
								excname = rest[rest.Length - 1];
						}

						CodeCatchClause ctch;

						if (tcf.CatchClauses.Count == 1 &&
								tcf.CatchClauses[0].CatchExceptionType.BaseType == "Keysharp.Core.Error" &&
								tcf.CatchClauses[0].Statements.Count == 0)//Check for the implicit catch.
						{
							ctch = tcf.CatchClauses[0];

							if (excname?.Length == 0)
								excname = ctch.LocalName;
						}
						else//User defined explicit catch.
						{
							if (excname?.Length == 0)
								excname = $"ex{excount++}";

							ctch = new CodeCatchClause(excname, new CodeTypeReference(exctypename));
							tcf.CatchClauses.Insert(0, ctch);
						}

						var catchVars = new HashSet<string>();
						_ = catchVars.Add(excname);
						excCatchVars.Push(catchVars);
						var type = parts.Length > 1 && parts[1][0] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(line, Scope, ctch.Statements, CodeBlock.BlockKind.Catch, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
						var left = VarId(excname, false);
						var result = new CodeSnippetExpression(excname);
						//var cas = new CodeAssignStatement(left, result);
						ctch.LocalName = excname;
						ctch.CatchExceptionType = new CodeTypeReference(exctypename);
						//ctch.Statements.Insert(0, cas);
						var catches = tcf.CatchClauses.Cast<CodeCatchClause>().ToArray();
						//Types must be sorted from most derived to least derived.
						System.Array.Sort(catches,
										  (c1, c2) =>
						{
							var t1 = Type.GetType(c1.CatchExceptionType.BaseType).BaseType;
							var t2 = Type.GetType(c2.CatchExceptionType.BaseType).BaseType;

							if (t1 == t2)
								return 0;
							else if (t1.IsSubclassOf(t2))
								return -1;
							else
								return 1;
						});
						tcf.CatchClauses.Clear();
						tcf.CatchClauses.AddRange(catches);
					}
					else
						throw new ParseException("Catch with no preceeding try block", line);
				}
				break;

				case FlowFinally:
				{
					if (parent.Count > 0 && parent[parent.Count - 1] is CodeTryCatchFinallyStatement tcf)
					{
						var type = parts.Length > 1 && parts[1][0] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(line, Scope, tcf.FinallyStatements, CodeBlock.BlockKind.Finally, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
					else
						throw new ParseException("Finally with no preceeding try block", line);
				}
				break;

				default:
					throw new ParseException(ExUnexpected, line);
			}

			return null;
		}

		private CodeExpression ParseFlowParameter(CodeLine line, string code, bool inequality, out bool blockOpen, bool expr)
		{
			blockOpen = false;
			code = code.Trim(Spaces);

			if (code.Length == 0)
				return new CodePrimitiveExpression(false);

			if (LaxExpressions && IsLegacyIf(code))
				return ParseLegacyIf(code);
			else if (expr || IsExpressionIf(code))
			{
				code = StripComment(code).TrimEnd(Spaces);
				var l = code.Length - 1;

				if (code.Length > 0 && code[l] == BlockOpen)
				{
					blockOpen = true;
					code = code.Substring(0, l).Trim();
				}

				this.blockOpen = false;
				var result = ParseSingleExpression(code, false);
				blockOpen = blockOpen || this.blockOpen;
				var trimparens = code.Trim(BothParens);

				if (trimparens.Equals("true", System.StringComparison.OrdinalIgnoreCase) || trimparens.Equals("1"))//Skip an if test call if we know it's a bool.
					return new CodePrimitiveExpression(true);
				else if (trimparens.Equals("false", System.StringComparison.OrdinalIgnoreCase) || trimparens.Equals("0"))
					return new CodePrimitiveExpression(false);
				else
				{
					var iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
					_ = iftest.Parameters.Add(result);
					return iftest;
				}
			}
			else if (LegacyIf)
			{
				code = StripCommentSingle(code);
				var l = code.Length - 1;

				if (code.Length > 0 && code[l] == BlockOpen)
				{
					blockOpen = true;
					code = code.Substring(0, l).Trim();
				}

				if (inequality)
					return ParseInequality(line, code);

				if (IsPrimitiveObject(code, out var result))
					return new CodePrimitiveExpression(result);
				else
					throw new ParseException(ExUnexpected, line);
			}
			else
				throw new ParseException("Invalid arguments for if statement", line);
		}

		private CodeExpression ParseInequality(CodeLine line, string code)
		{
			var buf = new StringBuilder(code.Length);
			var i = 0;

			while (i < code.Length && IsSpace(code[i])) i++;

			while (i < code.Length && (IsIdentifier(code[i]) || code[i] == Resolve))
				_ = buf.Append(code[i++]);

			while (i < code.Length && IsSpace(code[i])) i++;

			if (i != code.Length) // if test argument is not a lone identifier then it is an expression
			{
				var op = new[] { Equal, Not, Greater, Less };

				if (System.Array.IndexOf(op, code[i]) == -1)
					throw new ParseException(ExUnexpected, line);

				_ = buf.Append(code[i++]);

				if (i < code.Length && System.Array.IndexOf(op, code[i]) != -1)
					_ = buf.Append(code[i++]);

				_ = buf.Append(StringBound);

				while (i < code.Length && IsSpace(code[i])) i++;

				if (i < code.Length)
				{
					var str = code.Substring(i);
					str = str.Replace(StringBound.ToString(), new string(StringBound, 2));
					_ = buf.Append(str);
				}

				while (i < code.Length && IsSpace(code[i])) i++;

				_ = buf.Append(StringBound);
			}

			var iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
			var expr = ParseSingleExpression(buf.ToString(), false);
			_ = iftest.Parameters.Add(expr);
			return iftest;
		}

		private CodeExpression ParseLegacyIf(string code)
		{
			var parts = code.TrimStart(Spaces).Split(Spaces, 3);

			if (parts.Length != 3)
				throw new ArgumentOutOfRangeException();

			if (!IsIdentifier(parts[0]))
				throw new ArgumentException();

			var not = false;

			if (parts[1].Equals(NotTxt, System.StringComparison.OrdinalIgnoreCase))
			{
				//not = false;
				not = true;//Original set not to false here, which makes no sense.//MATT
				var sub = parts[2].Split(Spaces, 2);
				parts[1] = sub[0];
				parts[2] = sub[1];
			}

			var invoke = (CodeMethodInvokeExpression)InternalMethods.IfLegacy;
			_ = invoke.Parameters.Add(VarId(parts[0], false));
			parts[1] = parts[1].ToLowerInvariant();

			switch (parts[1])
			{
				case BetweenTxt:
				case InTxt:
				case ContainsTxt:
				case IsTxt:
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(parts[1]));
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			_ = invoke.Parameters.Add(ParseCommandParameter(parts[2]));

			if (not)//Easiest way to do not since CodeDOM does not support unary ! operators.//MATT
			{
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(true));
			}

			/*

			                //Just pass the not string directly to IfLegacy() and handle the logical inversion internally
			                if (not)
			                {
			                var forcebool = (CodeMethodInvokeExpression)InternalMethods.ForceBool;
			                var flip = (CodeMethodInvokeExpression)InternalMethods.OperateUnary;
			                //_ = flip.Parameters.Add(OperatorAsFieldReference(Script.Operator.BitwiseNot));
			                _ = flip.Parameters.Add(OperatorAsFieldReference(Script.Operator.LogicalNot));//Original did bitwise not, but we probably want logical not since LegacyIf returns bool.//MATT
			                _ = flip.Parameters.Add(invoke);
			                forcebool.Parameters.Add(flip);
			                invoke = forcebool;
			                //invoke = flip;
			                //var castExpression = new CodeCastExpression(
			                //  "System.Int32",
			                //  invoke);
			                //return castExpression;
			                }*/
			return invoke;
		}
	}
}