using System.CodeDom;
using System.Collections.Generic;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private CodeMethodInvokeExpression ParseLabel(CodeLine line)
		//private CodeMemberMethod ParseLabel(CodeLine line)//It makes no sense to call a label function just because it was declared, so just return void.//MATT
		{
			var code = line.Code;
			var z = code.Length - 1;
			var name = z > 0 ? code.Substring(0, z) : string.Empty;

			if (code.Length < 2 || code[z] != HotkeyBound)
				throw new ParseException("Invalid label name");

			_ = PushLabel(line, name, name, true);
			//return LocalLabelInvoke(name);
			return LocalMethodInvoke(name);//Labels are executed when they are found, but also treated like a function that can be called later. No need to do label invoke if we know the name, just call it as a regular function.//MATT
		}

		private CodeMemberMethod PushLabel(CodeLine line, string name, string realname, bool fallthrough)//Return the method.//MATT
		{
			var last = CloseTopLabelBlock();

			if (fallthrough && last != null)
				_ = last.Statements.Add(LocalLabelInvoke(name));

			//var method = LocalMethod(name);
			var method = LocalLabelMethod(name);//Label methods should take no parameters and return void.//MATT
			var block = new CodeBlock(line, method.Name, method.Statements, CodeBlock.BlockKind.Label, blocks.PeekOrNull())
			{
				Type = CodeBlock.BlockType.Within,
				Name = realname
			};
			_ = CloseTopSingleBlock();
			blocks.Push(block);
			methods[typeStack.Peek()][method.Name] = method;
			return method;
		}
	}
}