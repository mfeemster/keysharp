using System.CodeDom;
using System.Collections.Generic;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		//private void AddMainVar(string tp, string name)
		//{
		//  var cmf = new CodeMemberField(tp, name);

		//  if (!targetClass.Members.Contains(cmf))
		//      targetClass.Members.Add(cmf);
		//}

		private CodeStatementCollection parent;
		private CodeBlock parentBlock;

		private void Statements(List<CodeLine> lines)
		{
			for (var i = 0; i < lines.Count; i++)
			{
				var code = lines[i].Code;

				if (string.IsNullOrEmpty(code))
					continue;

				line = lines[i].LineNumber;
				fileName = lines[i].FileName;
				parentBlock = blocks.Count > 0 ? blocks.Peek() : null;
				parent = parentBlock != null ? parentBlock.Statements : main.Statements;
				var blocksCount = -1;
				CodeBlock block;
				var sym = code[0];
				var skip = false;

				switch (sym)
				{
					case BlockOpen:
						if (blocks.Count == 0)
						{
							block = new CodeBlock(lines[i], Scope, new CodeStatementCollection(), CodeBlock.BlockKind.Dummy, blocks.PeekOrNull());
							_ = CloseTopSingleBlock();
							blocks.Push(block);
						}

						block = blocks.Peek();

						if (block.Type == CodeBlock.BlockType.Expect)
							block.Type = CodeBlock.BlockType.Within;

						skip = true;
						break;

					case BlockClose:
						if (blocks.Count == 0)
							throw new ParseException(ExUnexpected, lines[i]);

						//Case statements don't need to be enclosed in braces.
						//But different action needs to be taken based on whether it was opened with a brace.
						//Case opened with a brace: encountering this brace means close the case statement.
						//Case opened without a brance: encountering this brace means close the case statement AND the switch statement.
						if (parentBlock != null)
						{
							if (parentBlock.Kind == CodeBlock.BlockKind.CaseWithoutBrace)
								CloseBlock();
							//If the case statement just contained empty braces, it means go do nothing and go to the end, instead of falling through which would be done when no braces or statements are present.
							else if (parentBlock.Kind == CodeBlock.BlockKind.CaseWithBrace && parentBlock.Statements.Count == 0 && switches.PeekOrNull() is CodeSwitchStatement css)
								_ = parentBlock.Statements.Add(new CodeGotoStatement(css.FinalLabelStatement.Label));
							else if (parentBlock.Kind == CodeBlock.BlockKind.Catch)
							{
								_ = excCatchVars.PopOrNull();
							}
							else if (parentBlock.Kind == CodeBlock.BlockKind.Function)
							{
								if (methods.TryGetValue(parentBlock.Method, out var meth))
								{
									var vari = 0;

									if (meth.UserData.Contains(initParams) && meth.UserData[initParams] is CodeStatementCollection csc)
										foreach (CodeStatement cs in csc)
											meth.Statements.Insert(vari++, cs);

									if (staticFuncVars.PeekOrNull() is Dictionary<string, CodeExpression> dkt)
									{
										foreach (var kv in dkt)
										{
											var scopedvar = kv.Key;
											var cmf = new CodeMemberField(typeof(object), scopedvar)
											{
												Attributes = MemberAttributes.Public | MemberAttributes.Static
											};

											if (kv.Value is CodeExpression ce)
												cmf.InitExpression = ce;

											_ = targetClass.Members.Add(cmf);
										}
									}

									var scope = Scope.ToLower();

									if (allVars.TryGetValue(scope, out var av))
									{
										var gfv = globalFuncVars.PeekOrNull();

										foreach (var v in av)
										{
											if (gfv == null || !gfv.Contains(v))
											{
												var dec = new CodeVariableDeclarationStatement(typeof(object), v, new CodeSnippetExpression("null"));//Ensure everything is initialized to null so the compiler won't complain about uninitialized variables.
												meth.Statements.Insert(vari++, dec);
											}
										}
									}
								}

								_ = currentFuncParams.PopOrNull();
								_ = allGlobalVars.TryPop(out _);
								_ = allStaticVars.TryPop(out _);
								_ = globalFuncVars.PopOrNull();
								_ = localFuncVars.PopOrNull();
								_ = staticFuncVars.PopOrNull();
							}
						}

						CloseBlock();
						skip = true;
						break;

					default:
						if (blocks.Count > 0 && blocks.Peek().Type == CodeBlock.BlockType.Expect)// && blocks.Peek().Kind != CodeBlock.BlockKind.Case)
						{
							blocksCount = blocks.Count;
							block = blocks.Peek();
							block.Type = CodeBlock.BlockType.Within;
							block.Level = blocksCount;
						}

						break;
				}

				if (skip)
				{
					code = code.Substring(1).TrimStart(Spaces);

					if (code.Length == 0)
						continue;

					lines[i].Code = code;
				}

				var token = GetToken(code);

				try
				{
					switch (token)
					{
						case Token.Assign:
							var assign = ParseAssign(code);
							//if (parent == main.Statements)//Test to see if we're processing statements within main.//MATT
							//{
							//  AddMainVar(assign.Right.GetType().ToString(), assign.Left.ToString());
							//}
							//assign.LinePragma = lines[i];
							_ = parent.Add(assign);
							break;

						case Token.Command:
							var command = new CodeExpressionStatement(OptimizeExpression(ParseCommand(code)));

							if (command.Expression == null)
								continue;

							//command.LinePragma = lines[i];
							_ = parent.Add(command);
							break;

						case Token.Label:
							_ = parent.Add(new CodeLabeledStatement(code.Trim(HotkeyBound)));//Labels in AHK scrip behave exactly as labels in C# do.
							break;

						case Token.Hotkey:
							var hotkey = ParseHotkey(lines, i);
							//hotkey.LinePragma = lines[i];
							_ = parent.Add(hotkey);
							break;

						case Token.Flow:
						{
							var result = ParseFlow(lines, i);

							if (result != null)
							{
								//for (var n = 0; n < result.Length; n++)
								//  result[n].LinePragma = lines[i];
								parent.AddRange(result);
							}
						}
						break;

						//case Token.Throw:
						//{
						//  var result = ParseFlow(lines, i);

						//  if (result != null)
						//      parent.AddRange(result);
						//}
						//break;

						case Token.Expression:
						{
							var n = i + 1;

							if (IsFunction(code, n < lines.Count ? lines[n].Code : string.Empty))
								_ = ParseFunction(lines[i]);
							else
							{
								var statements = ParseMultiExpression(code, true);

								for (n = 0; n < statements.Length; n++)
								{
									var expr = OptimizeLoneExpression(statements[n].Expression);

									if (expr == null)
										continue;
									else
										statements[n] = new CodeExpressionStatement(expr);

									//statements[n].LinePragma = lines[n];
									//if (parentBlock != null && parentBlock.Kind == CodeBlock.BlockKind.Try && parentBlock.Statements.Count > 0)
									//  parentBlock.Statements.Insert(parentBlock.Statements.Count - 1, statements[n]);
									//else
									_ = parent.Add(statements[n]);//This will erroneously enclose the expression in parens, which must be stripped out at the code level.//MATT
								}
							}
						}
						break;

						case Token.Directive:
							ParseDirective(code);
							break;

						case Token.Unknown:
						default:
							throw new ParseException(ExUnexpected, lines[i]);
					}
				}
				catch (ParseException e)
				{
					throw new ParseException(e.Message, lines[i]);
				}
				finally { }

				if (blocks.Count == blocksCount && blocks.Peek().IsSingle)
					CloseBlock(blocksCount, blocks.Count > blocksCount && blocksCount != -1);
			}

			CloseTopSingleBlocks();
			_ = CloseTopLabelBlock();
			CloseTopSingleBlocks();
			CloseSingleLoopBlocks();

			if (blocks.Count > 0)
				throw new ParseException(ExUnclosedBlock, blocks.Peek().Line);
		}
	}
}