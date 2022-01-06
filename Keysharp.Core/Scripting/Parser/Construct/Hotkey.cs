using System;
using System.CodeDom;
using System.Collections.Generic;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private Dictionary<string, string> conditionIds;

		private string HotkeyConditionId()
		{
			const string sep = ".";

			if (conditionIds == null)
			{
				conditionIds = new Dictionary<string, string>();
				conditionIds.Add(sep + sep + sep + sep + sep + sep + sep, string.Empty);
			}

			var criteria = string.Concat(
							   IfWinActive_WinTitle, sep, IfWinActive_WinText, sep,
							   IfWinExist_WinTitle, sep, IfWinExist_WinText, sep,
							   IfWinNotActive_WinTitle, sep, IfWinNotActive_WinText, sep,
							   IfWinNotExist_WinTitle, sep, IfWinNotExist_WinText);

			if (conditionIds.ContainsKey(criteria))
				return conditionIds[criteria];

			var id = InternalID;
			conditionIds.Add(criteria, id);
			return id;
		}

		private CodeMethodInvokeExpression ParseHotkey(List<CodeLine> lines, int index)
		{
			var code = lines[index].Code;
			var hotstring = code[0] == HotkeyBound;
			var mode = string.Empty;

			if (hotstring)
			{
				var z = code.IndexOf(HotkeyBound, 1) + 1;
				mode = code.Substring(0, z);
				code = code.Substring(z);
			}

			var parts = code.Split(new[] { HotkeySignal }, 2, StringSplitOptions.None);

			if (parts.Length == 0 || parts[0].Length == 0)
				throw new ParseException("Blank hotkey definition");

			if (hotstring)
				parts[0] = string.Concat(mode, parts[0]);

			var name = Script.LabelMethodName(parts[0]);
			var cond = HotkeyConditionId();

			if (cond.Length != 0)
				name += "_" + cond;

			//PushLabel(lines[index], name, parts[0], false);//Don't need for now, do later.//MATT
			//var tempblock = CloseTopLabelBlock();
			//if (tempblock != null)
			//  parent = tempblock.Statements;
			parts[1] = parts[1].RemoveAfter(" ;").TrimEnd();//Need to account for a comment on this line.//MATT

			if (parts.Length > 0)// && !IsEmptyStatement(parts[1]))//Original guarded against empty statements, which ruined the stack position. Empty hostrings make sense, so try to keep them.//MATT
			{
				var remap = IsRemap(parts[1]);

				if (hotstring)
				{
					if (parts[1]?.Length == 0)
					{
						_ = PushLabel(lines[index], name, parts[0], false);//Not entirely clear when or where this is exactly needed, but know that it's needed somewhere, somehow.//MATT
						//var pop = blocks.Pop();//You need to determine where to work this in.//MATT
						//if (parts[1] != string.Empty)//No need to call Send() if the string is empty.//MATT
						//{
						//  var send = (CodeMethodInvokeExpression)InternalMethods.Send;
						//  _ = send.Parameters.Add(new CodePrimitiveExpression(remap ? parts[1].TrimStart(Spaces).Substring(0, 1) : parts[1]));
						//  pop.Statements.Add(send);
						//}
						//var cdve = new CodeDefaultValueExpression(new CodeTypeReference(typeof(object)));//MATT
						//_ = pop.Statements.Add(new CodeMethodReturnStatement(cdve));
						//_ = pop.Statements.Add(cdve);
					}
				}
				else
				{
					lines.Insert(index + 1, new CodeLine(lines[index].FileName, lines[index].LineNumber, parts[1]));
					lines[index].Code = string.Concat(parts[0], HotkeySignal);
					blocks.Peek().Type = CodeBlock.BlockType.Expect;
				}
			}

			parent = main.Statements;//Appears to work, but skeptical it's right. Seems to make sense though, because we'd never want to call the hotstring label creation method outisde of main.
			CodeMethodInvokeExpression invoke;

			if (hotstring)
			{
				if (parts[1] != string.Empty)
				{
					invoke = (CodeMethodInvokeExpression)InternalMethods.Hotstring;
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(parts[0]));//.Substring(mode.Length));
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(parts[1]));
					//_ = invoke.Parameters.Add(new CodePrimitiveExpression(name));
					//var options = mode.Substring(1, mode.Length - 2);
					//if (!string.IsNullOrEmpty(HotstringNewOptions))
					//options = string.Concat(HotstringNewOptions, SingleSpace.ToString(), options);
					//_ = invoke.Parameters.Add(new CodePrimitiveExpression(options));
				}
				else
				{
					invoke = (CodeMethodInvokeExpression)InternalMethods.HotstringLabel;
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(parts[0].Substring(mode.Length)));
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(name));
					var options = mode.Substring(1, mode.Length - 2);

					if (!string.IsNullOrEmpty(HotstringNewOptions))
						options = string.Concat(HotstringNewOptions, SingleSpace.ToString(), options);

					_ = invoke.Parameters.Add(new CodePrimitiveExpression(options));
				}
			}
			else
			{
				invoke = (CodeMethodInvokeExpression)InternalMethods.Hotkey;
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(parts[0]));
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(name));
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(string.Empty));
			}

			//_ = prepend.Add(invoke);
			Parser.Persistent = true;
			//return new CodeMethodReturnStatement();
			return invoke;//MATT
		}
	}
}