namespace Keysharp.Scripting
{
	public partial class Parser
	{
		public static int TypeDistance(Type t1, Type t2)
		{
			var distance = 0;

			while (t1 != null && t1 != t2)
			{
				distance++;
				t1 = t1.BaseType;
			}

			return distance;
		}

		private CodeStatement[] ParseFlow(List<CodeLine> lines, int index)
		{
			var codeLine = lines[index];
			var code = codeLine.Code;
			var cspan = code.AsSpan();
			string[] parts = [string.Empty, string.Empty];
			int[] d = [cspan.IndexOfAny(FlowDelimitersSv), cspan.IndexOfAny(BlockOpenParenOpenSv)];

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
				parts = [parts[0].Trim()];

			var lower = parts[0].ToLowerInvariant();

			//This is needed becauses elses is popped when an else is encountered, but if an if statement
			//has no else, then the elses will remain and can sometimes affect later code which thinks it's
			//still in an if/else block, even though the block has long since terminated.
			//if (parentBlock == null || parentBlock.Kind != CodeBlock.BlockKind.IfElse)
			//  if (lower != FlowElse && elses.Count > 0)
			//      elses.Clear();
			switch (lower)
			{
				case FlowIf:
				{
					if (parts.Length < 1)
						throw new ParseException("If requires a parameter", codeLine);

					var condition = ParseFlowParameter(codeLine, parts[1], true, out var blockOpen, false);
					var ifelse = new CodeConditionStatement { Condition = condition };
					var block = new CodeBlock(codeLine, Scope, ifelse.TrueStatements, CodeBlock.BlockKind.IfElse, blocks.PeekOrNull());
					block.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
					_ = CloseTopSingleBlock();
					CloseElseBlocks();//Also called in Statements. Both places are needed to handle various situations.
					blocks.Push(block);
					elses.Push(ifelse.FalseStatements);
					return [ifelse];
				}

				case FlowElse:
				{
					if (parent.Count > 0 && parent[parent.Count - 1] is CodeTryCatchFinallyStatement tcf)
					{
						var left = VarId(codeLine, $"try{tryCount}success", true);
						//
						var cas = new CodeAssignStatement(left, new CodeSnippetExpression("false"));
						tcf.TryStatements.Insert(0, cas);
						//
						cas = new CodeAssignStatement(left, new CodeSnippetExpression("true"));
						_ = tcf.TryStatements.Add(cas);
						//
						var condition = ParseFlowParameter(codeLine, $"try{tryCount}success", true, out var blockOpen, true);
						var ccs = new CodeConditionStatement { Condition = condition };
						tcf.FinallyStatements.Insert(0, ccs);
						var type = parts.Length > 1 && parts[1][parts[1].Length - 1] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(codeLine, Scope, ccs.TrueStatements, CodeBlock.BlockKind.TryElse, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
					else if (elses.Count > 0)//This must come last otherwise it will take precedence over the special situations above.
					{
						var next = codeLine.Code.AsSpan().TrimStart(Spaces).Slice(FlowElse.Length).TrimStart(Spaces).ToString();

						if (!IsEmptyStatement(next))
							lines.Insert(index + 1, new CodeLine(codeLine.FileName, codeLine.LineNumber, next.TrimEnd(BlockOpenAndSpace)));

						var type = parts.Length > 1 && parts[1][parts[1].Length - 1] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(codeLine, Scope, elses.Pop(), CodeBlock.BlockKind.IfElse, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
					else if (parent.Count > 1 && parent[parent.Count - 2] is CodeTryCatchFinallyStatement tcfs && tcfs.FinallyStatements.Count == 1)//This is brittle, but finally statements should only ever be populated by the parser. There is no way to make one in script code.
					{
						var blockOpen = parts.Length > 1 && parts[1].AsSpan().Trim().EndsWith("{");
						tcfs.FinallyStatements.Clear();
						var ifelse = new CodeConditionStatement { Condition = new CodeSnippetExpression("Pop().index == 0L") };
						var block = new CodeBlock(codeLine, Scope, ifelse.TrueStatements, CodeBlock.BlockKind.IfElse, blocks.PeekOrNull());
						block.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						_ = CloseTopSingleBlock();
						blocks.Push(block);
						tcfs.FinallyStatements.Add(ifelse);
						return null;
					}
					else
						throw new ParseException("Else with no preceding if, try, for, loop or while block.", codeLine);
				}
				break;

				case FlowSwitch:
				{
					var snip = new CodeSnippetStatement();
					var switchargs = new List<string>() { "" };
					var lastPart = parts.Last();
					var otb = lastPart.EndsWith('{');

					if (otb)
						parts[parts.Length - 1] = lastPart.TrimEnd(SpaceTabOpenBrace);

					if (parts.Length > 1)
						switchargs = SplitStringBalanced(parts[1], ',');

					var switchvar = switchargs.Count > 0 ? switchargs[0] : "";
					var cse = new CodeSwitchStatement(switchvar, switchargs.Count > 1 ? switchargs[1].Trim() : null, switchCount++);
					var type = otb ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
					var block = new CodeBlock(codeLine, Scope, cse.AllStatements, CodeBlock.BlockKind.Switch, blocks.PeekOrNull()) { Type = type };
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
							CloseBlock(codeLine);
						}

						if (parts.Length > 1)//Note that case does not support OTB style braces.
						{
							var colonindex = parts[1].IndexOf(':');
							var casearg = colonindex != -1 ? parts[1].Substring(0, colonindex) : throw new ParseException("Case not terminated with a colon.", codeLine);
							var casearr = SplitStringBalanced(casearg, ',').Select(a => a.Trim()).ToArray();
							var casename = $"_ks_caselabel{caseCount++}";
							colonindex++;

							if (colonindex < parts[1].Length)
							{
								var next = parts[1].AsSpan(colonindex).TrimStart(Spaces).ToString();

								if (!IsEmptyStatement(next))
									lines.Insert(index + 1, new CodeLine(codeLine.FileName, codeLine.LineNumber, next));
							}

							if (casearr.Length == 1)
							{
								if (css.CaseSense != null)
								{
									var comparg = "System.StringComparison." + css.CaseSense.ToString();
									var cmie = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("System.String"), "Compare"));
									_ = cmie.Parameters.Add(new CodeSnippetExpression($"{css.SwitchVarTempName}str"));
									_ = cmie.Parameters.Add(new CodeSnippetExpression($"{casearg}.ToString()"));
									_ = cmie.Parameters.Add(new CodeSnippetExpression(comparg));
									var coe = new CodeBinaryOperatorExpression(cmie, CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("0"));
									css.CaseExpressions.Add(casename, coe);
								}
								else if (!string.IsNullOrEmpty(css.SwitchVar) && !string.IsNullOrEmpty(css.SwitchVarTempName))
									css.CaseExpressions.Add(casename, $"{css.SwitchVarTempName} == {casearr[0]}");
								else
									css.CaseExpressions.Add(casename, $"{casearr[0]}");
							}
							else
							{
								casearr = SplitStringBalanced(casearg, ',').Select(a => a.Trim()).ToArray();

								if (!string.IsNullOrEmpty(css.SwitchVar) && !string.IsNullOrEmpty(css.SwitchVarTempName))
									for (var i = 0; i < casearr.Length; i++)
										//if (casearr[i].IndexOf(' ') == -1)
										casearr[i] = $"{css.SwitchVarTempName} == {casearr[i]}";

								css.CaseExpressions.Add(casename, string.Join("||", casearr));
							}

							var hasbrace = blockOpen || (lines.Count > index + 1 && lines[index + 1].Code.TrimStart(Spaces).StartsWith(BlockOpen));
							var type = CodeBlock.BlockType.Within;
							var block = new CodeBlock(codeLine, Scope, css.CaseBodyStatements.GetOrAdd(casename), hasbrace ? CodeBlock.BlockKind.CaseWithBrace : CodeBlock.BlockKind.CaseWithoutBrace, blocks.PeekOrNull()) { Type = type };
							_ = CloseTopSingleBlock();
							blocks.Push(block);
						}
						else
							throw new ParseException("Case with no argument.", codeLine);
					}
					else
						throw new ParseException("Case with no preceding switch block.", codeLine);
				}
				break;

				case FlowDefault:
				{
					if (switches.PeekOrNull() is CodeSwitchStatement css)
					{
						if (parentBlock != null && (parentBlock.Kind == CodeBlock.BlockKind.CaseWithBrace || parentBlock.Kind == CodeBlock.BlockKind.CaseWithoutBrace))
						{
							CloseBlock(codeLine);
						}

						var hasbrace = lines.Count > index + 1 && lines[index + 1].Code.TrimStart(Spaces).StartsWith(BlockOpen);
						var type = CodeBlock.BlockType.Within;//Might want this to always be within to avoid the funny stuff Statements does for normal blocks.
						var block = new CodeBlock(codeLine, Scope, css.DefaultStatements, hasbrace ? CodeBlock.BlockKind.CaseWithBrace : CodeBlock.BlockKind.CaseWithoutBrace, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
				}
				break;

				case FlowGosub:
				{
					throw new ParseException("Gosub is no longer supported, use goto with a label instead.", codeLine);
				}

				case FlowGoto:
				{
					if (parts.Length < 1)
						throw new ParseException("No label specified for goto statement.", codeLine);

					var cgs = new CodeGotoStatement(parts[1]);

					if (parentBlock != null)
						gotos[cgs] = parentBlock;

					return [cgs];
				}

				case FlowLoop:
				{
					var skip = true;
					var trimmed = StripComment(parts.Last());
					var blockOpen = trimmed.EndsWith(BlockOpen);

					if (blockOpen)
						parts[parts.Length - 1] = trimmed.Trim([BlockOpen, ' ']);

					CodeMethodInvokeExpression iterator;
					var lt = LoopType.Normal;

					if (parts.Length > 1 && parts.Last() != string.Empty)//If the last char was a {, then it was trimmed and replaced with a "".
					{
						var sub = parts[1].Split(SpaceTabComma, 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
						var regex = CommaQuotesRegex();
						var sub2 = sub.Length > 1 ? regex.Split(sub[1]) : [];
						//sub2[0] = sub2[0].Trim(new char[] { '"' });
						//sub = new[] { sub[0].Trim(), sub.Length > 1 ? sub[1].Trim() : string.Empty };
						sub = new string[] { sub[0] }.Concat(sub2);

						switch (sub[0].ToUpperInvariant())
						{
							case "READ":
								lt = LoopType.File;
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopRead;
								break;

							case "PARSE":
								lt = LoopType.Parse;
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopParse;
								break;
#if WINDOWS

							case "REG":
								lt = LoopType.Registry;
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopRegistry;
								break;
#endif

							case "FILES":
								lt = LoopType.Directory;
								iterator = (CodeMethodInvokeExpression)InternalMethods.LoopFile;
								break;

							default:
								skip = false;
								lt = LoopType.Normal;
								iterator = (CodeMethodInvokeExpression)InternalMethods.Loop;
								break;
						}

						if (skip && parts[1].Length == 0)
							throw new ParseException("Loop type must have an argument.", codeLine);

						if (skip)
						{
							sub = sub.Skip(1).ToArray();

							foreach (var s in sub)
								_ = iterator.Parameters.Add(ParseCommandParameter(codeLine, s.Trim(), false, true));
						}
						else
							_ = iterator.Parameters.Add(ParseCommandParameter(codeLine, parts[1], false, true));
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
						TestExpression = new CodeMethodInvokeExpression(null, "IsTrueAndRunning", condition),
						IncrementStatement = new CodeSnippetStatement(string.Empty)
					};
					var block = new CodeBlock(codeLine, Scope, loop.Statements, CodeBlock.BlockKind.Loop, blocks.PeekOrNull(), InternalID, InternalID);
					block.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;

					if (parent.Count > 0 && parent[parent.Count - 1] is CodeLabeledStatement cls)
						block.Name = cls.Label;

					_ = CloseTopSingleBlock();
					blocks.Push(block);
					var tcf = new CodeTryCatchFinallyStatement();
					var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);
					var push = (CodeMethodInvokeExpression)InternalMethods.Push;
					push.Parameters.Add(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(LoopType)), lt.ToString()));
					tcf.TryStatements.Add(new CodeExpressionStatement(push));
					tcf.TryStatements.Add(loop);
					tcf.FinallyStatements.Add(pop);
					return [tcf, new CodeLabeledStatement(block.ExitLabel, new CodeSnippetStatement(";"))];//End labels seem to need a semicolon.
				}

				case FlowFor:
				{
					//The documentation shows this working with function objects, where the constructor returns an enumeration object, and the
					//Call() method is called each time the enumerator is incremented (like MoveNext() would be calling it under the hood).
					//The class Enumerator has one method, which is Call (you can think of it inheriting from Func, which also has that method).
					//However, we've not implemented that here yet, and probably won't.//TODO
					var blockOpen = parts[1].EndsWith(BlockOpen);
					parts[1] = StripCommentSingle(parts[1]).TrimEnd(BlockOpen).Trim(Spaces);
					var trimmed = StripEnclosing(parts[1], '(', ')');
					var temp = trimmed.Split("in ", 2, StringSplitOptions.TrimEntries);

					if (temp.Length == 2)
					{
						var coldeclid = InternalID;
						var testid = InternalID;
						var id = InternalID;
						var expr = ParseSingleExpression(codeLine, temp.Last(), false);
						var varsplits = temp[0] == "" ? [] : temp[0].ToLower().Split(',', StringSplitOptions.TrimEntries).ToList();

						for (var vari = 0; vari < varsplits.Count; ++vari)
							if (varsplits[vari] == "")
								varsplits[vari] = $"_ks_forvar{vari}";

						//Extreme hack to tell the enumerator whether to return 1 or 2 values which changes
						//the behavior when dealing with OwnProps().
						if (expr is CodeMethodInvokeExpression cmieInvoke
								&& cmieInvoke.Method.MethodName == "Invoke"
								&& cmieInvoke.Parameters.Count == 1
								&& cmieInvoke.Parameters[0] is CodeMethodInvokeExpression cmieGetMeth
								&& cmieGetMeth.Method.MethodName == "GetMethodOrProperty"
								&& cmieGetMeth.Parameters.Count > 2
								&& cmieGetMeth.Parameters[1] is CodePrimitiveExpression cpe
								&& cpe.Value.ToString() == "OwnProps")
						{
							if (varsplits.Count > 1)
								_ = cmieInvoke.Parameters.Add(new CodePrimitiveExpression(true));
						}

						//If only one is present, it actually means discard the first one, and use the second.
						//So in the case of a map, it would mean get the key, not the value.
						//In the case of an array, it would mean get the value, not the index.
						foreach (var split in varsplits)
						{
							if (split != "")
							{
								//Don't create iteration loop variables as global/local function variables, they have to be specially created manually below.
								var loopvarexpr = ParseSingleExpression(codeLine, split, false);
								var loopvarstr = Ch.CodeToString(loopvarexpr);
								//Create entries in the allVars map so these don't get recreated if they are used, such
								//as being passed to a function. However, set the Value to null to prevent them from actually
								//being created because we create them manually with a snippet below.
								//See: case BlockClose and AddPropStatements() in Statements.cs.
								var dkt = allVars[typeStack.Peek()].GetOrAdd(Scope);

								if (!dkt.ContainsKey(loopvarstr))
									dkt[loopvarstr] = null;
							}
						}

						var col = Ch.CodeToString(expr);
						var coldecl = new CodeExpressionStatement(new CodeSnippetExpression($"var {coldeclid} = {col}"));
						var iterdecl = new CodeExpressionStatement(new CodeSnippetExpression($"var {id} = MakeEnumerator({coldeclid}, {varsplits.Count})"));
						_ = parent.Add(coldecl);
						_ = parent.Add(iterdecl);
						var condition = new CodeMethodInvokeExpression();
						condition.Method.TargetObject = new CodeVariableReferenceExpression(id);
						condition.Method.MethodName = "Call";
						var initSb = new StringBuilder(64);

						for (var vi = 0; vi < varsplits.Count; ++vi)
						{
							var v = varsplits[vi];

							if (vi > 0)
								initSb.Append(", ");
							else
								initSb.Append("object ");

							initSb.Append($"{v} = null");
							condition.Parameters.Add(new CodeSnippetExpression($"ref {v}"));
						}

						var loop = new CodeIterationStatement
						{
							InitStatement = new CodeSnippetStatement(initSb.ToString()),
							TestExpression = new CodeMethodInvokeExpression(null, "IsTrueAndRunning", condition),
							IncrementStatement = new CodeSnippetStatement(string.Empty)
						};
						loop.Statements.Insert(0, new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Inc));
						var block = new CodeBlock(codeLine, Scope, loop.Statements, CodeBlock.BlockKind.Loop, blocks.PeekOrNull(), InternalID, InternalID)
						{
							Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect
						};
						_ = CloseTopSingleBlock();
						blocks.Push(block);//Must add block first, before doing local variable names so that the scoping is correct and they don't conflict with function level vars.
						var push = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Push);
						var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);
						var tcf = new CodeTryCatchFinallyStatement();
						tcf.TryStatements.Add(loop);
						tcf.FinallyStatements.Add(pop);
						return [push, tcf, new CodeLabeledStatement(block.ExitLabel, new CodeSnippetStatement(";"))];//End labels seem to need a semicolon.
					}
				}
				break;

				case FlowWhile:
				{
					var blockOpen = false;
					var condition = parts.Length > 1 ? ParseFlowParameter(codeLine, parts[1], true, out blockOpen, true) : new CodePrimitiveExpression(true);
					var loop = new CodeIterationStatement
					{
						InitStatement = new CodeSnippetStatement(string.Empty),
						TestExpression = new CodeMethodInvokeExpression(null, "IsTrueAndRunning", condition),
						IncrementStatement = new CodeSnippetStatement(string.Empty)
					};
					loop.Statements.Insert(0, new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Inc));
					var block = new CodeBlock(codeLine, Scope, loop.Statements, CodeBlock.BlockKind.Loop, blocks.PeekOrNull(), InternalID, InternalID)
					{
						Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect
					};
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					var push = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Push);
					var pop = new CodeExpressionStatement((CodeMethodInvokeExpression)InternalMethods.Pop);
					var tcf = new CodeTryCatchFinallyStatement();
					tcf.TryStatements.Add(push);
					tcf.TryStatements.Add(loop);
					tcf.FinallyStatements.Add(pop);
					return [tcf, new CodeLabeledStatement(block.ExitLabel, new CodeSnippetStatement(";"))];//End labels seem to need a semicolon.
				}

				case FlowUntil:
				{
					if (parent.Cast<CodeStatement>().Last(cs => cs is CodeTryCatchFinallyStatement) is CodeTryCatchFinallyStatement tcf)
					{
						var ccs = new CodeConditionStatement();
						var token = StripCommentSingle(parts[1]);
						var expr = ParseSingleExpression(codeLine, token, false);
						var iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
						_ = iftest.Parameters.Add(expr);
						ccs.Condition = iftest;
						_ = ccs.TrueStatements.Add(new CodeSnippetExpression("break"));

						if (tcf.TryStatements.Cast<CodeStatement>().Last(ts => ts is CodeIterationStatement) is CodeIterationStatement cis)
							cis.Statements.Add(ccs);
					}
				}
				break;

				case FlowBreak:
				{
					var b = 1;
					string exit = null;

					if (parts.Length > 1)
					{
						parts[1] = StripCommentSingle(parts[1]);

						if (int.TryParse(parts[1], out b))
						{
							if (b < 1)
								throw new ParseException("Break parameter must be a static integer greater than zero.", codeLine);

							exit = PeekLoopLabel(true, b);

							if (exit == null)
								throw new ParseException($"Could not find a loop {b} levels above.", codeLine);
						}
						else//Assume it was a label if parsing an int failed.
						{
							(exit, b) = PeekLoopLabel(true, parts[1]);

							if (exit == null)
								throw new ParseException($"Could not find a loop above with the name {parts[1]}.", codeLine);
						}
					}
					else
						(exit, b) = PeekLoopLabel(true, "");

					if (exit == null)
						throw new ParseException("Cannot break outside of a loop.", codeLine);

					return [new CodeGotoStatement(exit)];
				}

				case FlowContinue:
				{
					var c = 1;

					if (parts.Length > 1)
					{
						parts[1] = StripCommentSingle(parts[1]);

						if (!int.TryParse(parts[1], out c) || c < 1)
							throw new ParseException("Continue parameter must be a static integer greater than zero.", codeLine);
					}

					var cont = PeekLoopLabel(false, c);

					if (cont == null)
						throw new ParseException("Cannot continue outside a loop.", codeLine);

					//Need to use a goto instead of a continue statement because Until statements still need to be executed after continue.
					return [new CodeExpressionStatement(new CodeSnippetExpression($"goto {cont}"))];
				}

				case FlowReturn:
				{
					if (Scope?.Length == 0)
					{
						_ = CloseTopLabelBlock();

						if (parts.Length > 1)
							throw new ParseException("Cannot have return parameter for entry point method.", codeLine);

						return [new CodeMethodReturnStatement(new CodePrimitiveExpression(1))];
					}
					else
					{
						var result = parts.Length > 1 ? ParseSingleExpression(codeLine, parts[1], false) : emptyStringPrimitive;
						return [new CodeMethodReturnStatement(result)];
					}
				}

				case FlowClass:
				{
					string basetype;
					var classparts = parts[1].Split(SpaceTab, StringSplitOptions.RemoveEmptyEntries);
					var classtype = AddType(classparts[0].ToLower());
					var blockOpen = parts.Length > 1 && parts[1].AsSpan().Trim().EndsWith("{");
					var block = new CodeBlock(codeLine, Scope, null, CodeBlock.BlockKind.Class, blocks.PeekOrNull(), InternalID, InternalID)
					{
						Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect
					};
					_ = CloseTopSingleBlock();
					blocks.Push(block);
					var constructor = new CodeConstructor
					{
						Attributes = MemberAttributes.Public | MemberAttributes.Final
					};
					var staticConstructor = new CodeTypeConstructor
					{
						Attributes = MemberAttributes.Public | MemberAttributes.Final | MemberAttributes.Static
					};

					if (classparts.Length > 2 && string.Compare(classparts[1], "extends", true) == 0)
					{
						basetype = classparts[2];
						classtype.BaseTypes.Add(basetype);

						if (string.Compare(basetype, "gui", true) == 0)
							Persistent = true;
					}
					else
					{
						basetype = "KeysharpObject";
						classtype.BaseTypes.Add(basetype);
					}

					_ = classtype.Members.Add(constructor);
					_ = classtype.Members.Add(staticConstructor);
					//var callmeth = new CodeMemberMethod
					//{
					//  Name = "Call",
					//  ReturnType = new CodeTypeReference(classtype.Name),
					//  Attributes = MemberAttributes.Public | MemberAttributes.Static
					//};
					////Body of Call() will be added later.
					//_ = classtype.Members.Add(callmeth);
					//methods[typeStack.Peek()][callmeth.Name] = callmeth;
					var initmeth = new CodeMemberMethod
					{
						Name = "__Init",
						Attributes = MemberAttributes.Family | MemberAttributes.Override
					};
					initmeth.Statements.Insert(0, new CodeExpressionStatement(new CodeSnippetExpression("base.__Init()")));
					_ = classtype.Members.Add(initmeth);
					methods[typeStack.Peek()][initmeth.Name] = initmeth;
					var nameprop = new CodeMemberProperty
					{
						Name = "__Class",
						Type = new CodeTypeReference(typeof(string)),
						Attributes = MemberAttributes.Public | MemberAttributes.Static
					};
					_ = nameprop.GetStatements.Add(new CodeSnippetExpression("return \"" + classtype.Name + "\"")); //Can't use interpolated string here because the AStyle formatter misinterprets it.
					_ = classtype.Members.Add(nameprop);
					return null;
				}

				case FunctionLocal:
				{
					if (localFuncVars.PeekOrNull() is List<string> lflist)
					{
						if (parts.Length > 1)
						{
							var parencount = 0;
							var temptoks = SplitTokens(codeLine, parts[1]);//Find the variable names because they needed to be added to the local vars before ParseMultiExpression() is called so they get properly added.
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

							foreach (var expr in ParseMultiExpression(codeLine, parts[1], true))
								if (expr is CodeExpressionStatement ces &&
										ces.Expression.WasCboe() is CodeBinaryOperatorExpression cboe &&
										cboe.Left is CodeVariableReferenceExpression cvre)
									funclocalvarinitstatements.Add(new CodeExpressionStatement(BinOpToSnippet(cboe)));

							if (funclocalvarinitstatements.Count > 0)
								return funclocalvarinitstatements.ToArray();
						}
						else
							throw new ParseException("local keyword must be followed be one or more variable names.", codeLine);
					}
				}
				break;

				case FunctionGlobal:
				{
					if (parts.Length > 1)
					{
						var multiexprs = ParseMultiExpression(codeLine, parts[1], false);//Do not create any variables based on what is parsed from the global variable initialization statements.

						if (globalFuncVars.PeekOrNull() is List<string> gflist)
						{
							var funcglobalvarinitstatements = new List<CodeStatement>(multiexprs.Length);

							foreach (var expr in multiexprs)
							{
								if (expr is CodeExpressionStatement ces)
								{
									if (ces.Expression.WasCboe() is CodeBinaryOperatorExpression cboe)
									{
										if (cboe.Left is CodeVariableReferenceExpression cvre)
										{
											cvre.VariableName = cvre.VariableName.Trim('%').ToLower();//ParseMultiExpression() knows we are in a function, so it will attach the scope. But this is a global variable reference, so remove it plus the _.
											gflist.Add(cvre.VariableName);
											funcglobalvarinitstatements.Add(new CodeExpressionStatement(BinOpToSnippet(cboe)));
										}
									}
									else if (ces.Expression is CodeVariableReferenceExpression cvre)
										gflist.Add(cvre.VariableName.Trim('%').ToLower());
									else
										funcglobalvarinitstatements.Add(expr);
								}
							}

							if (funcglobalvarinitstatements.Count > 0)
								return funcglobalvarinitstatements.ToArray();
						}
						else//Strange syntax, but global can be declared outside of a function and it's just ignored.
						{
							var codeStatements = new List<CodeStatement>(multiexprs.Length);

							foreach (var expr in multiexprs)
							{
								if (expr.Expression.WasCboe() is CodeBinaryOperatorExpression cboe && cboe.Left is CodeVariableReferenceExpression cvre2)
								{
									allVars[targetClass].GetOrAdd(Scope)[cvre2.VariableName] = cboe.Right;
									codeStatements.Add(new CodeExpressionStatement(BinOpToSnippet(cboe)));
								}
								else if (expr.Expression is CodeVariableReferenceExpression cvre)
									allVars[targetClass].GetOrAdd(Scope)[cvre.VariableName] = nullPrimitive;
							}

							if (codeStatements.Count > 0)
								return codeStatements.ToArray();

							//lines.Insert(index + 1, new CodeLine(codeLine.FileName, codeLine.LineNumber, code.Remove(0, 6).TrimStart(Spaces)));
							//return null;
						}
					}
					else if (parts.Length == 1)
					{
						_ = allGlobalVars.TryPop(out _);
						allGlobalVars.Push(true);
					}
				}
				break;

				case FunctionStatic:
				{
					if (parts.Length > 1)
					{
						if (staticFuncVars[typeStack.Peek()].PeekOrNull() is Dictionary<string, CodeExpression> dkt)
						{
							var parencount = 0;
							var bracketcount = 0;
							var temptoks = SplitTokens(codeLine, parts[1]);//Find the variable names because they needed to be added to the static vars before ParseMultiExpression() is called so they get properly added.

							for (var ti = 0; ti < temptoks.Count; ti++)
							{
								if (temptoks[ti] is string tok)
								{
									if (tok[0] == '(' || tok.EndsWith('('))//Make sure the comma is not because it's using a function with multiple arguments to init. ) will be at the end of function call tokens.
										parencount++;
									else if (tok[0] == ')')
										parencount--;
									else if (tok[0] == '[')
										bracketcount++;
									else if (tok[0] == ']')
										bracketcount--;

									if ((parencount == 0 && bracketcount == 0) && (ti == 0 || (temptoks[ti - 1] as string)[0] == ','))
										dkt[Scope + scopeChar + tok.ToLower()] = null;//Must add as scoped for it to work.
								}
							}

							var mutltiexprs = ParseMultiExpression(codeLine, parts[1], true);//Do not create any variables based on what is parsed from the static variable initialization statements.

							foreach (var expr in mutltiexprs)
							{
								if (expr is CodeExpressionStatement ces)
								{
									if (ces.Expression.WasCboe() is CodeBinaryOperatorExpression cboe)
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
				}
				break;

				case FlowThrow:
				{
					var ctes = new CodeThrowExceptionStatement();

					if (parts.Length > 1)
					{
						var token = StripCommentSingle(parts[1]);

						if (IsPrimitiveObject(token, out var obj))
							ctes.ToThrow = new CodeSnippetExpression($"Error({obj})");
						else if (token.StartsWith('"') && token.EndsWith('"'))
							ctes.ToThrow = new CodeSnippetExpression($"Error({token})");
						else
							ctes.ToThrow = ParseSingleExpression(codeLine, token, false);
					}

					return [ctes];
				}

				case FlowTry:
				{
					tryCount++;
					var tcf = new CodeTryCatchFinallyStatement();
					var ctch = new CodeCatchClause($"ex{exCount++}", new CodeTypeReference(typeof(Error)));
					_ = tcf.CatchClauses.Add(ctch);
					var type = parts.Length > 1 && parts[1][0] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;

					if (parts.Length > 1 && parts[1] != "{")
					{
						var lineIndex = codeLines.IndexOf(codeLine) + 1;
						codeLines.Insert(lineIndex, new CodeLine(codeLine.FileName, lineIndex, parts[1]));
						SetLineIndexes();
					}

					var block = new CodeBlock(codeLine, Scope, tcf.TryStatements, CodeBlock.BlockKind.Try, blocks.PeekOrNull())
					{
						Type = type
					};
					blocks.Push(block);
					return [tcf];
				}

				case FlowCatch:
				{
					//It is impossible to use any type of special state between the try block above and here to determine the level, instead, we must look at whatever the parent block is
					//to get the correct try that this catch was attached to. This is mainly needed because one try can have multiple catch statements.
					if (parent.Count > 0 && parent[parent.Count - 1] is CodeTryCatchFinallyStatement tcf)
					{
						var excname = "";
						var exctypename = "Keysharp.Core.Error";
						var extraExceptions = new List<string>(8);

						if (parts.Length > 1)
						{
							var rest = StripCommentSingle(parts[1]).RemoveAll(Parens).Split(SpaceTabComma);

							if (rest.Length > 0 && rest[0] != "as" && rest[0] != "{")
							{
								for (var exi = 0; exi < rest.Length; ++exi)
								{
									var exctype = rest[exi];

									if (exi == 0)
									{
										if (Reflections.stringToTypes.TryGetValue(exctype, out var t))
										{
											if (t == typeof(Any))//Catch Any means all exceptions. So catch not only Error, but basic Exceptions too.
												exctypename = typeof(Exception).FullName;
											else if (typeof(KeysharpException).IsAssignableFrom(t))
											{
												if (!extraExceptions.Contains(t.FullName))
													exctypename = t.FullName;
											}
											else
												throw new ParseException($"Exception type of '{t.FullName}' was not derived from Error.", codeLine);
										}
										else//This should never happen, but keep in case.
											exctypename = "Keysharp.Core." + exctype;
									}
									else
									{
										if (Reflections.stringToTypes.TryGetValue(exctype, out var t))
										{
											if (typeof(KeysharpException).IsAssignableFrom(t))
											{
												if (!extraExceptions.Contains(t.FullName))
													extraExceptions.Add(t.FullName);
											}
											else
												throw new ParseException($"Exception type of '{t.FullName}' was not derived from Error.", codeLine);
										}
									}
								}
							}

							var excCount = extraExceptions.Count + 1;
							var tokenCount = rest[rest.Length - 1] == "{" ? excCount + 3 : excCount + 2;

							//Possible scenarios:
							//Error as errorname {
							//Error as errorname
							//Error errorname {
							//Error errorname
							//Error {
							//Error

							if (rest.Length == tokenCount)
							{
								if (string.Compare(rest[excCount], "as", true) != 0)
									throw new ParseException($"Unrecognized token '{rest[1]}' between exception type and name.", codeLine);

								excname = rest[excCount + 1];
							}
							else if (rest.Length == tokenCount - 1)
								excname = rest[excCount];
							else if (rest.Length > excCount + 1)//No variable name declared. So either Error or Error {
								throw new ParseException($"Invalid number of tokens ({rest.Length}) in catch statement", codeLine);
						}

						CodeCatchClause ctch;

						if (tcf.CatchClauses.Count == 1 &&
								tcf.CatchClauses[0].CatchExceptionType.BaseType == typeof(Error).FullName &&
								tcf.CatchClauses[0].Statements.Count == 0)//Check for the implicit catch.
						{
							ctch = tcf.CatchClauses[0];

							if (excname?.Length == 0)
								excname = ctch.LocalName;
						}
						else//User defined explicit catch.
						{
							if (excname?.Length == 0)
								excname = $"ex{exCount++}";

							ctch = new CodeCatchClause(excname, new CodeTypeReference(exctypename));
							tcf.CatchClauses.Insert(0, ctch);
						}

						var catchVars = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
						_ = catchVars.Add(excname);
						excCatchVars.Push(catchVars);
						var type = parts.Length > 1 && parts[1].EndsWith(BlockOpen) ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(codeLine, Scope, ctch.Statements, CodeBlock.BlockKind.Catch, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
						var left = VarId(codeLine, excname, false);
						ctch.LocalName = excname;
						ctch.CatchExceptionType = new CodeTypeReference(exctypename);

						foreach (var extra in extraExceptions)
							tcf.CatchClauses.Add(new CodeCatchClause("", new CodeTypeReference(extra), new CodeExpressionStatement(new CodeSnippetExpression("throw"))));

						var catches = tcf.CatchClauses.Cast<CodeCatchClause>().ToArray();
						//Types must be sorted from most derived to least derived.
						System.Array.Sort(catches,
										  (c1, c2) =>
						{
							var t1 = Type.GetType(c1.CatchExceptionType.BaseType);
							var t2 = Type.GetType(c2.CatchExceptionType.BaseType);

							if (t1 == t2)
								return 0;

							var d1 = TypeDistance(t1, typeof(KeysharpException));
							var d2 = TypeDistance(t2, typeof(KeysharpException));

							if (d1 == d2)
								return 0;
							else if (d1 < d2)
								return 1;
							else
								return -1;
						});
						tcf.CatchClauses.Clear();
						tcf.CatchClauses.AddRange(catches);
					}
					else
						throw new ParseException("Catch with no preceding try block.", codeLine);
				}
				break;

				case FlowFinally:
				{
					if (parent.Count > 0 && parent[parent.Count - 1] is CodeTryCatchFinallyStatement tcf)
					{
						var type = parts.Length > 1 && parts[1][0] == BlockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
						var block = new CodeBlock(codeLine, Scope, tcf.FinallyStatements, CodeBlock.BlockKind.Finally, blocks.PeekOrNull()) { Type = type };
						_ = CloseTopSingleBlock();
						blocks.Push(block);
					}
					else
						throw new ParseException("Finally with no preceding try block.", codeLine);
				}
				break;

				default:
					return null;
			}

			return null;
		}

		private CodeExpression ParseFlowParameter(CodeLine codeLine, string code, bool inequality, out bool blockOpen, bool expr)
		{
			blockOpen = false;
			code = code.Trim(Spaces);

			if (code.Length == 0)
				return new CodePrimitiveExpression(false);

			if (LaxExpressions && IsLegacyIf(code))
				return ParseLegacyIf(codeLine, code);
			else if (expr || IsExpressionIf(code))
			{
				code = StripComment(code).TrimEnd(Spaces);
				var l = code.Length - 1;

				if (code.Length > 0 && code[l] == BlockOpen)
				{
					blockOpen = true;
					code = code.AsSpan(0, l).Trim().ToString();
				}

				this.blockOpen = false;
				var result = ParseSingleExpression(codeLine, code, false);
				blockOpen = blockOpen || this.blockOpen;
				var trimparens = code.Trim(BothParens);

				if (trimparens.Equals("true", StringComparison.OrdinalIgnoreCase) || trimparens.Equals("1"))//Skip an if test call if we know it's a bool.
					return new CodePrimitiveExpression(true);
				else if (trimparens.Equals("false", StringComparison.OrdinalIgnoreCase) || trimparens.Equals("0"))
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
					code = code.AsSpan(0, l).Trim().ToString();
				}

				if (inequality)
					return ParseInequality(codeLine, code);

				if (IsPrimitiveObject(code, out var result))
					return new CodePrimitiveExpression(result);
				else
					throw new ParseException(ExUnexpected, codeLine);
			}
			else
				throw new ParseException("Invalid arguments for if statement.", codeLine);
		}

		private CodeExpression ParseInequality(CodeLine codeLine, string code)
		{
			var i = 0;
			var buf = new StringBuilder(code.Length);

			while (i < code.Length && IsSpace(code[i])) i++;

			while (i < code.Length && (IsIdentifier(code[i]) || code[i] == Resolve))
				_ = buf.Append(code[i++]);

			while (i < code.Length && IsSpace(code[i])) i++;

			if (i != code.Length) // if test argument is not a lone identifier then it is an expression
			{
				if (System.Array.IndexOf(ops, code[i]) == -1)
					throw new ParseException(ExUnexpected, codeLine);

				_ = buf.Append(code[i++]);

				if (i < code.Length && System.Array.IndexOf(ops, code[i]) != -1)
					_ = buf.Append(code[i++]);

				while (i < code.Length && IsSpace(code[i])) i++;

				if (i < code.Length)
					_ = buf.Append(code.Substring(i));

				while (i < code.Length && IsSpace(code[i])) i++;
			}

			var iftest = (CodeMethodInvokeExpression)InternalMethods.IfElse;
			var expr = ParseSingleExpression(codeLine, buf.ToString(), false);
			_ = iftest.Parameters.Add(expr);
			return iftest;
		}

		private CodeExpression ParseLegacyIf(CodeLine codeLine, string code)
		{
			var parts = code.TrimStart(Spaces).Split(Spaces, 3);

			if (parts.Length != 3)
				throw new ParseException($"Legacy if requires 3 parts, but {parts.Length} were supplied.", codeLine);

			if (!IsIdentifier(parts[0]))
				throw new ParseException($"First legacy if token of {parts[0]} was not an identifier.", codeLine);

			var not = false;

			if (parts[1].Equals(NotTxt, StringComparison.OrdinalIgnoreCase))
			{
				not = true;
				var sub = parts[2].Split(Spaces, 2);
				parts[1] = sub[0];
				parts[2] = sub[1];
			}

			var invoke = (CodeMethodInvokeExpression)InternalMethods.IfLegacy;
			_ = invoke.Parameters.Add(VarId(codeLine, parts[0], false));
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
					throw new ParseException($"Second legacy if token of {code} was not any of: between, in, contains, is.", codeLine);
			}

			_ = invoke.Parameters.Add(ParseCommandParameter(codeLine, parts[2]));

			if (not)//Easiest way to do not since CodeDOM does not support unary ! operators.
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(true));

			return invoke;
		}

		[GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))")]
		private static partial Regex CommaQuotesRegex();//Gotten from https://stackoverflow.com/questions/3147836/c-sharp-regex-split-commas-outside-quotes
	}
}