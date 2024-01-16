using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private void CloseElseBlocks()
		{
			var ifCount = blocks.Where(b => b.Kind == CodeBlock.BlockKind.IfElse).Count();

			//Pop previous undeclared else blocks, such as:
			//if ()
			//{
			//  if ()
			//  {
			//  }
			//}//When the parser gets here, it needs to pop the previous else which was never declared.
			//
			//Also handles this:
			//if ()
			//  x := 123
			//
			//if (b)//Elses need to be popped here.
			//{
			//}
			//else
			//{
			//}
			while (elses.Count > ifCount)
				_ = elses.Pop();
		}

		private void CloseBlock(Stack<CodeBlock> stack)
		{
			var top = stack.Pop();

			if (top.Kind == CodeBlock.BlockKind.Switch)
			{
				var localparent = stack.Count > 0 ? stack.Peek().Statements : main.Statements;
				var origlocalparent = localparent;

				if (localparent != null)
				{
					if (switches.PopOrNull() is CodeSwitchStatement css)
					{
						if (css.CaseSense != null)
						{
							var right = new CodeMethodInvokeExpression(VarId(css.SwitchVar, false), "ToString");
							_ = origlocalparent.Add(BinOpToSnippet(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(css.SwitchVarTempName), CodeBinaryOperatorType.Assign, right)));
							_ = origlocalparent.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("System.String"), $"{css.SwitchVarTempName}str", right));
							allVars[typeStack.Peek()].GetOrAdd(Scope)[css.SwitchVarTempName] = nullPrimitive;
						}
						else if (!string.IsNullOrEmpty(css.SwitchVar))
						{
							var tokens = SplitTokens(css.SwitchVar);
							var right = ParseExpression(top.Line, css.SwitchVar, tokens, true);
							_ = origlocalparent.Add(BinOpToSnippet(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(css.SwitchVarTempName), CodeBinaryOperatorType.Assign, right)));
							allVars[typeStack.Peek()].GetOrAdd(Scope)[css.SwitchVarTempName] = nullPrimitive;
						}

						foreach (var cond in css.CaseExpressions)
						{
							CodeExpression expr = null;

							if (cond.Value is CodeExpression ce)
								expr = ce;
							else if (cond.Value is string s)
								expr = ParseFlowParameter(top.Line, s, true, out var blockOpen, true);

							if (expr != null)
							{
								var ifelse = new CodeConditionStatement { Condition = expr };
								_ = ifelse.TrueStatements.Add(new CodeSnippetExpression($"goto {cond.Key}"));
								_ = localparent.Add(ifelse);
								localparent = ifelse.FalseStatements;
							}
						}

						foreach (var cond in css.CaseExpressions)
						{
							_ = origlocalparent.Add(new CodeLabeledStatement(cond.Key));

							if (css.CaseBodyStatements.TryGetValue(cond.Key, out var csc))
							{
								origlocalparent.AddRange(csc);
								var anyNonGotos = false;

								foreach (CodeStatement tempcsc in csc)
								{
									if (tempcsc is CodeGotoStatement cgs)
									{
										if (gotos.TryGetValue(cgs, out var tempBlock) && stack.Count > 0)
											gotos[cgs] = stack.Peek();
									}
									else
										anyNonGotos = true;
								}

								if (anyNonGotos)//This allows multiple case statements to run the same code (equivalent of no breaks in C#).
									_ = origlocalparent.Add(new CodeGotoStatement(css.FinalLabelStatement.Label));
							}
						}

						if (css.DefaultStatements.Count > 0)
						{
							_ = localparent.Add(new CodeSnippetExpression($"goto {css.DefaultLabelStatement.Label}"));
							_ = origlocalparent.Add(css.DefaultLabelStatement);

							foreach (CodeStatement def in css.DefaultStatements)
								_ = origlocalparent.Add(def);
						}
						else
							_ = localparent.Add(new CodeGotoStatement(css.FinalLabelStatement.Label));

						_ = origlocalparent.Add(css.FinalLabelStatement);
						_ = origlocalparent.Add(new CodeSnippetStatement(";"));//End labels seem to need a semicolon.

						if (css.CaseSense != null || !string.IsNullOrEmpty(css.SwitchVar))
							_ = origlocalparent.Add(new CodeAssignStatement(new CodeSnippetExpression(css.SwitchVarTempName), new CodeSnippetExpression("null")));
					}
				}
			}
			else if (top.EndLabel != null)
				_ = top.Statements.Add(new CodeLabeledStatement(top.EndLabel, new CodeSnippetStatement(";")));//End labels seem to need a semicolon.
		}

		private void CloseBlock()
		{
			CloseTopSingleBlocks();
			CloseBlock(-1, false);
		}

		private void CloseBlock(int level, bool skip)
		{
			if (blocks.Count < (skip ? 2 : 1))
				return;

			var peek = skip ? blocks.Pop() : null;
			var top = blocks.Peek();

			if (top.IsSingle && blocks.Count > top.Level)
				goto end;

			CloseBlock(blocks);
			end:

			if (skip)
				blocks.Push(peek);
		}

		private void CloseSingleLoopBlocks()
		{
			while (singleLoops.Count != 0)
				CloseBlock(singleLoops);
		}

		private CodeBlock CloseTopLabelBlock() => blocks.Count != 0 && blocks.Peek().Kind == CodeBlock.BlockKind.Label ? blocks.Pop() : null;

		private bool CloseTopSingleBlock()
		{
			if (blocks.Count == 0)
				return false;

			if (blocks.Peek().IsSingle)
			{
				var top = blocks.Pop();

				if (top.Kind == CodeBlock.BlockKind.Loop)
					singleLoops.Push(top);

				return true;
			}

			return false;
		}

		private void CloseTopSingleBlocks()
		{
			while (CloseTopSingleBlock()) ;
		}

		private int GotoLoopDepth(CodeBlock block, string label)
		{
			var depth = 0;
			var parent = block;

			while (parent != null)
			{
				if (parent.Kind == CodeBlock.BlockKind.Loop)
				{
					foreach (var cs in parent.Statements)
					{
						if (cs is CodeLabeledStatement cls)
						{
							if (cls.Label == label)
								goto found;
						}
					}

					depth++;
				}

				parent = parent.Parent;
			}

			found:
			return depth;
		}

		private int LoopDepth()
		{
			if (blocks.Count == 0)
				return 0;

			var depth = 0;
			var parent = blocks.Peek();

			while (parent != null)
			{
				if (parent.Kind == CodeBlock.BlockKind.Loop)
					depth++;

				parent = parent.Parent;
			}

			return depth;
		}

		private string PeekLoopLabel(bool exit, int n)
		{
			if (blocks.Count == 0)
				return null;

			var parent = blocks.Peek();

			while (parent != null)
			{
				if (parent.Kind == CodeBlock.BlockKind.Loop)
					n--;

				if (n < 1)
					return exit ? parent.ExitLabel : parent.EndLabel;

				parent = parent.Parent;
			}

			return null;
		}

		private (string, int) PeekLoopLabel(bool exit, string label)
		{
			if (blocks.Count == 0)
				return (null, 0);

			var depth = 0;
			var parent = blocks.Peek();

			while (parent != null)
			{
				if (parent.Kind == CodeBlock.BlockKind.Loop)
				{
					depth++;

					if (label.Length == 0 || parent.Name == label)
						return exit ? (parent.ExitLabel, depth) : (parent.EndLabel, depth);
				}

				parent = parent.Parent;
			}

			return (null, 0);
		}
	}
}