namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private void AddPropStatements(CodeBlock.BlockKind blockKind)
		{
			var scope = Scope.TrimEndOf(blockKind == CodeBlock.BlockKind.PropGet ? "_get" : "_set");

			if (properties[typeStack.Peek()].TryGetValue(scope, out var propList))
			{
				var cmp = propList.Last();

				if (staticFuncVars[typeStack.Peek()].PeekOrNull() is Dictionary<string, CodeExpression> dkt)
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

						_ = typeStack.Peek().Members.Add(cmf);
					}
				}

				scope = Scope.ToLower();
				var statements = blockKind == CodeBlock.BlockKind.PropGet ? cmp.GetStatements : cmp.SetStatements;

				if (allVars[typeStack.Peek()].TryGetValue(scope, out var av))
				{
					var gfv = globalFuncVars.PeekOrNull();

					foreach (var v in av)
					{
						if (gfv == null || !gfv.Contains(v.Key))
						{
							var dec = new CodeVariableDeclarationStatement(typeof(object), v.Key, new CodeSnippetExpression("null"));
							_ = statements.Add(dec);
						}
					}
				}

				statements.AddRange(parentBlock.Statements);
			}

			EndFunction();
		}

		private void Statements()
		{
			for (var i = 0; i < codeLines.Count; i++)
			{
				var codeLine = codeLines[i];
				var code = codeLine.Code;

				if (string.IsNullOrEmpty(code))
					continue;

				fileName = codeLine.FileName;
				parentBlock = blocks.Count > 0 ? blocks.Peek() : null;
				parent = parentBlock != null ? parentBlock.Statements : userMainMethod.Statements;
				var blocksCount = -1;
				CodeBlock block = null;
				var sym = code[0];
				var skip = false;
				var isHotkeyString = IsHotkeyLabel(code) || IsHotstringLabel(code);

				if (!isHotkeyString)//Do not take special action on the first character if it was a hotkey or hotstring.
				{
					switch (sym)
					{
						case BlockOpen:
							if (blocks.Count == 0)
							{
								block = new CodeBlock(codeLine, Scope, null, CodeBlock.BlockKind.Dummy, blocks.PeekOrNull());
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
								throw new ParseException(ExUnexpected, codeLine);

							CloseElseBlocks();

							//Case statements don't need to be enclosed in braces.
							//But different action needs to be taken based on whether it was opened with a brace.
							//Case opened with a brace: encountering this brace means close the case statement.
							//Case opened without a brance: encountering this brace means close the case statement AND the switch statement.
							if (parentBlock != null)
							{
								if (parentBlock.Kind == CodeBlock.BlockKind.CaseWithoutBrace)
									CloseBlock(codeLine);
								//If the case statement just contained empty braces, it means go do nothing and go to the end, instead of falling through which would be done when no braces or statements are present.
								else if (parentBlock.Kind == CodeBlock.BlockKind.CaseWithBrace && parentBlock.Statements.Count == 0 && switches.PeekOrNull() is CodeSwitchStatement css)
									_ = parentBlock.Statements.Add(new CodeGotoStatement(css.FinalLabelStatement.Label));
								else if (parentBlock.Kind == CodeBlock.BlockKind.Catch)
								{
									_ = excCatchVars.PopOrNull();
								}
								else if (parentBlock.Kind == CodeBlock.BlockKind.Function)
								{
									var typeMethods = methods[typeStack.Peek()];

									if (typeMethods.TryGetValue(parentBlock.Method, out var meth))
									{
										var vari = 0;

										if (meth.UserData.Contains(initParams) && meth.UserData[initParams] is CodeStatementCollection csc)
											foreach (CodeStatement cs in csc)
												meth.Statements.Insert(vari++, cs);

										if (staticFuncVars[typeStack.Peek()].PeekOrNull() is Dictionary<string, CodeExpression> dkt)
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

												_ = typeStack.Peek().Members.Add(cmf);
											}
										}

										var scope = Scope.ToLower();

										if (allVars[typeStack.Peek()].TryGetValue(scope, out var av))
										{
											var gfv = globalFuncVars.PeekOrNull();

											foreach (var v in av)
											{
												if (gfv == null || !gfv.Contains(v.Key))
												{
													if (!InClassDefinition() || v.Key != "this")//Never create a "this" variable inside of a class definition.
													{
														var dec = new CodeVariableDeclarationStatement(typeof(object), v.Key, new CodeSnippetExpression("null"));//Ensure everything is initialized to null so the compiler won't complain about uninitialized variables.
														meth.Statements.Insert(vari++, dec);
													}
												}
											}
										}
									}

									EndFunction();
								}
								else if (parentBlock.Kind == CodeBlock.BlockKind.Class)
								{
									if (typeStack.PopOrNull() is CodeTypeDeclaration ctd)
									{
									}
								}
								else if (parentBlock.Kind == CodeBlock.BlockKind.Prop)
								{
									EndFunction();
								}
								else if (parentBlock.Kind == CodeBlock.BlockKind.PropGet)
								{
									AddPropStatements(CodeBlock.BlockKind.PropGet);
								}
								else if (parentBlock.Kind == CodeBlock.BlockKind.PropSet)
								{
									AddPropStatements(CodeBlock.BlockKind.PropSet);
								}
							}

							CloseBlock(codeLine);
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
				}

				if (skip)
				{
					code = code.Substring(1).TrimStart(Spaces);

					if (code.Length == 0)
						continue;

					codeLine.Code = code;
				}

				var token = GetToken(codeLine);

				try
				{
					switch (token)
					{
						case Token.Assign:
							var assign = ParseAssign(codeLine, code);
							//assign.LinePragma = codeline;
							_ = parent.Add(assign);
							break;

						case Token.Command:
							if (ParseCommand(codeLine, code))
							{
								if (block != null)//Reset the block state and reparse this line as a function call.
									block.Type = CodeBlock.BlockType.Expect;

								i--;
								continue;
							}

							break;

						case Token.Label:
							//Labels in an AHK script behave exactly as labels in C# do.
							_ = parent.Add(new CodeLabeledStatement(code.Trim(HotkeyBound), new CodeSnippetStatement(";")));//End labels seem to need a semicolon.
							break;

						case Token.Hotkey:
							var hotkey = ParseHotkey(codeLines, i);
							break;

						case Token.Flow:
						{
							var result = ParseFlow(codeLines, i);

							if (result != null)
							{
								//for (var n = 0; n < result.Length; n++)
								//  result[n].LinePragma = codeline;
								parent.AddRange(result);
							}

							break;
						}

						case Token.Prop:
						{
							if (InClassDefinition() && Scope.Length == 0)
							{
								var copy = code.ToLower();
								var isstatic = false;

								if (copy.EndsWith('{'))
									copy = copy.TrimEnd(SpaceTabOpenBrace);

								if (copy.StartsWith("static", StringComparison.OrdinalIgnoreCase) && (copy.Length > 6 && copy[6] == ' ' || copy[6] == '\t'))
								{
									copy = copy.Substring(7, copy.Length - 7);
									isstatic = true;
								}

								var openBracket = copy.IndexOf('[');

								if (openBracket != -1)
								{
									copy = copy.AsSpan(0, openBracket).Trim().ToString();
								}

								var prop = new CodeMemberProperty
								{
									Attributes = MemberAttributes.Public | MemberAttributes.Final,
									Type = objTypeRef,
									Name = copy//No need to check here for whether it's a valid identifier, because that was done by IsProperty() in GetToken().
								};

								if (copy.Length > 0)
								{
									if (openBracket != -1)
									{
										openBracket = code.IndexOf('[');
										var closeBracket = code.IndexOf(']');
										var sub = code.AsSpan(openBracket + 1, closeBracket - openBracket - 1);

										foreach (Range r in sub.Split(','))
										{
											var p = sub[r].Trim();

											if (p.Length > 0)
											{
												var pstr = p;
												var variadic = false;

												if (p.SequenceEqual("*"))
												{
													variadic = true;
													pstr = "args";//Parameter is variadic, but the name is unspecified, so use "args" as a default (because "params" is a reserved word).
												}
												else if (p.EndsWith('*'))
												{
													variadic = true;
													pstr = p.TrimEnd('*');
												}

												var pdecl = new CodeParameterDeclarationExpression(objTypeRef, pstr.ToString());

												if (variadic)
												{
													pdecl.Type = new CodeTypeReference(typeof(object[]));
													_ = pdecl.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(ParamArrayAttribute))));
												}

												_ = prop.Parameters.Add(pdecl);
											}
										}
									}
								}

								if (isstatic)
									prop.Attributes |= MemberAttributes.Static;

								properties[typeStack.Peek()].GetOrAdd(prop.Name).Add(prop);
								var blockOpen = codeLine.Code.AsSpan().Trim().EndsWith("{");
								var blockType = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
								var propblock = new CodeBlock(codeLine, prop.Name, null, CodeBlock.BlockKind.Prop, blocks.PeekOrNull())
								{
									Type = blockType
								};
								_ = CloseTopSingleBlock();
								blocks.Push(propblock);
							}
						}
						break;

						case Token.PropGet:
						case Token.PropSet:
						{
							StartNewFunction();
							var propName = blocks.Peek().Method;
							var funcParams = currentFuncParams.Peek();
							var prop = properties[typeStack.Peek()][propName].Last();

							foreach (CodeParameterDeclarationExpression p in prop.Parameters)
								funcParams.Add(p.Name);

							var span1 = codeLine.Code.ToLower().AsSpan();
							var span2 = span1.TrimStart("get").TrimStart("set").TrimStart(Spaces);
							var isFatArrow = span2.StartsWith("=>");
							var propblock = new CodeBlock(codeLine, token == Token.PropGet ? "get" : "set", null, token == Token.PropGet ? CodeBlock.BlockKind.PropGet : CodeBlock.BlockKind.PropSet, blocks.PeekOrNull());

							if (isFatArrow)
							{
								var theRest = span2.TrimStart("=>").Trim().ToString();
								codeLines.Insert(i + 1, new CodeLine(codeLine.FileName, codeLine.LineNumber, "{"));
								codeLines.Insert(i + 2, new CodeLine(codeLine.FileName, codeLine.LineNumber, token == Token.PropGet ? $"return {theRest}" : theRest));
								codeLines.Insert(i + 3, new CodeLine(codeLine.FileName, codeLine.LineNumber, "}"));
								SetLineIndexes();
								propblock.Type = CodeBlock.BlockType.Expect;
							}
							else
							{
								var blockOpen = span1.Trim().EndsWith("{");
								propblock.Type = blockOpen ? CodeBlock.BlockType.Within : CodeBlock.BlockType.Expect;
							}

							_ = CloseTopSingleBlock();
							blocks.Push(propblock);
							break;
						}

						case Token.Expression:
						{
							var n = i + 1;

							if (IsFunction(code, n < codeLines.Count ? codeLines[n].Code : string.Empty))
							{
								_ = ParseFunction(codeLine, n);
							}
							else
							{
								var statements = ParseMultiExpression(codeLine, code, true);

								for (n = 0; n < statements.Length; n++)
								{
									if (statements[n] is CodeExpressionStatement ces)
									{
										var expr = ces.Expression;

										//This is checking for the declaration and initialization of class member variables. Only record here after the parsing and optimization above have been done.
										if (InClassDefinition() && Scope.Length == 0 && expr.WasCboeAssign() is CodeBinaryOperatorExpression cboe
												&& cboe.Left is CodeVariableReferenceExpression cvre)//We are in a type that is not the main class, and also not inside of a function. Static or instance properties can be initialized with a string.
										{
											allVars[typeStack.Peek()].GetOrAdd(Scope)[cvre.VariableName] = cboe.Right;

											if (memberVarsStatic)
												cboe.Right.UserData["isstatic"] = true;

											if (statements.Length > 1)//If it was a multi statement, then consider it to be part of the initialization of a class member.
											{
												for (n++; n < statements.Length; n++)
													if (statements[n] is CodeExpressionStatement ces2)
														cboe.Right.UserData[ces2] = ces2;

												break;
											}
										}

										_ = parent.Add(statements[n]);
									}
								}
							}
						}
						break;

						case Token.Directive:
							ParseDirective(codeLine, code);
							break;

						case Token.Unknown:
						default:
							throw new ParseException(ExUnexpected, codeLine);
					}
				}
				catch (ParseException e)
				{
					throw new ParseException(e.RawMessage, codeLine);
				}
				finally { }

				if (blocks.Count == blocksCount && blocks.Peek().IsSingle)
					CloseBlock(codeLine, blocksCount, blocks.Count > blocksCount && blocksCount != -1);
			}

			CloseTopSingleBlocks();
			_ = CloseTopLabelBlock();
			CloseTopSingleBlocks();
			CloseSingleLoopBlocks(new CodeLine(name, 0, ""));

			if (blocks.Count > 0)
				throw new ParseException(ExUnclosedBlock, blocks.Peek().Line);
		}
	}
}