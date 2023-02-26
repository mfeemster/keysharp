using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		private void AddPropStatements(CodeBlock.BlockKind blockKind)
		{
			var scope = Scope.TrimEndOf(blockKind == CodeBlock.BlockKind.PropGet ? "_get" : "_set");

			if (properties[typeStack.Peek()].TryGetValue(scope, out var cmp))
			{
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
							statements.Add(dec);
						}
					}
				}

				statements.AddRange(parentBlock.Statements);
			}

			EndFunction();
		}

		private void Statements(List<CodeLine> lines)
		{
			for (var i = 0; i < lines.Count; i++)
			{
				var codeline = lines[i];
				var code = codeline.Code;

				if (string.IsNullOrEmpty(code))
					continue;

				line = codeline.LineNumber;
				fileName = codeline.FileName;
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
							block = new CodeBlock(codeline, Scope, null, CodeBlock.BlockKind.Dummy, blocks.PeekOrNull());
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
							throw new ParseException(ExUnexpected, codeline);

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
												var dec = new CodeVariableDeclarationStatement(typeof(object), v.Key, new CodeSnippetExpression("null"));//Ensure everything is initialized to null so the compiler won't complain about uninitialized variables.
												meth.Statements.Insert(vari++, dec);
											}
										}
									}
								}

								EndFunction();
							}
							else if (parentBlock.Kind == CodeBlock.BlockKind.Class)
							{
								_ = typeStack.PopOrNull();
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

					codeline.Code = code;
				}

				var token = GetToken(codeline);

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
							//assign.LinePragma = codeline;
							_ = parent.Add(assign);
							break;

						case Token.Command:
							var command = new CodeExpressionStatement(OptimizeExpression(ParseCommand(code)));

							if (command.Expression == null)
								continue;

							//command.LinePragma = codeline;
							_ = parent.Add(command);
							break;

						case Token.Label:
							//Labels in AHK scrip behave exactly as labels in C# do.
							_ = parent.Add(new CodeLabeledStatement(code.Trim(HotkeyBound), new CodeSnippetStatement(";")));//End labels seem to need a semicolon.
							break;

						case Token.Hotkey:
							var hotkey = ParseHotkey(lines, i);
							//hotkey.LinePragma = codeline;
							_ = parent.Add(hotkey);
							break;

						case Token.Flow:
						{
							var result = ParseFlow(lines, i);

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
							if (typeStack.Peek().Name != mainClassName && Scope.Length == 0)
							{
								var copy = code;
								var isstatic = false;

								if (copy.StartsWith("static "))
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
									Attributes = MemberAttributes.Public,
									Type = new CodeTypeReference(typeof(object)),
									Name = copy//No need to check here for whether it's a valid identifier, because that was done by IsProperty() in GetToken().
								};

								if (copy.Length > 0)
								{
									if (openBracket != -1)
									{
										openBracket = code.IndexOf('[');
										var closeBracket = code.IndexOf(']');
										var indexParams = code.Substring(openBracket + 1, closeBracket - openBracket - 1).Split(Comma, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
										prop.Attributes |= MemberAttributes.Final;

										foreach (var p in indexParams)
											_ = prop.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), p));
									}
								}

								if (isstatic)
									prop.Attributes |= MemberAttributes.Static;

								var lower = prop.Name.ToLower();
								properties[typeStack.Peek()][lower] = prop;
								var blockType = CodeBlock.BlockType.Expect;
								var propblock = new CodeBlock(codeline, lower, null, CodeBlock.BlockKind.Prop, blocks.PeekOrNull());
								propblock.Type = blockType;
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
							var prop = properties[typeStack.Peek()][propName];

							foreach (CodeParameterDeclarationExpression p in prop.Parameters)
								funcParams.Add(p.Name);

							var blockType = CodeBlock.BlockType.Expect;
							var propblock = new CodeBlock(codeline, token == Token.PropGet ? "get" : "set", null, token == Token.PropGet ? CodeBlock.BlockKind.PropGet : CodeBlock.BlockKind.PropSet, blocks.PeekOrNull());
							propblock.Type = blockType;
							_ = CloseTopSingleBlock();
							blocks.Push(propblock);
							break;
						}

						case Token.Expression:
						{
							var n = i + 1;

							if (IsFunction(code, n < lines.Count ? lines[n].Code : string.Empty))
								_ = ParseFunction(codeline);
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

									//This is checking for the declaration and initialization of class member variables. Only record here after the parsing and optimization above have been done.
									if (typeStack.Peek().Name != mainClassName && Scope.Length == 0 && expr is CodeBinaryOperatorExpression cboe && cboe.Operator == CodeBinaryOperatorType.Assign
											&& cboe.Left is CodeVariableReferenceExpression cvre)//We are in a type that is not the main class, and also not inside of a function. Static or instance properties can be initialized with a string.
									{
										allVars[typeStack.Peek()].GetOrAdd(Scope)[cvre.VariableName] = cboe.Right;

										if (memberVarsStatic)
											cboe.Right.UserData["isstatic"] = true;
									}

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
							throw new ParseException(ExUnexpected, codeline);
					}
				}
				catch (ParseException e)
				{
					throw new ParseException(e.Message, codeline);
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