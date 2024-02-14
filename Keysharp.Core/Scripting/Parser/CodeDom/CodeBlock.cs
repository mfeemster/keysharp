using System.CodeDom;

namespace Keysharp.Scripting
{
	internal class CodeBlock
	{
		internal string EndLabel { get; }

		internal string ExitLabel { get; }

		internal bool IsSingle => Level != int.MaxValue;

		internal BlockKind Kind { get; }

		internal int Level { get; set; }

		internal CodeLine Line { get; }

		internal string Method { get; }

		internal string Name { get; set; }

		internal CodeBlock Parent { get; }

		internal CodeStatementCollection Statements { get; }

		internal BlockType Type { get; set; }

		internal CodeBlock(CodeLine codeLine, string method, CodeStatementCollection statements, BlockKind kind, CodeBlock parent)
			: this(codeLine, method, statements, kind, parent, null, null) { }

		internal CodeBlock(CodeLine codeLine, string method, CodeStatementCollection statements, BlockKind kind, CodeBlock parent, string endLabel, string exitLabel)
		{
			Line = codeLine;
			Method = method;
			Statements = statements ?? new CodeStatementCollection();
			Type = BlockType.Expect;
			Kind = kind;
			Parent = parent;
			Level = int.MaxValue;
			EndLabel = endLabel;
			ExitLabel = exitLabel;
		}

		internal enum BlockKind
		{ Dummy, IfElse, Function, Label, Loop, Try, Catch, Finally, TryElse, Switch, CaseWithBrace, CaseWithoutBrace, CaseDefault, Class, Prop, PropGet, PropSet };

		internal enum BlockType
		{ None, Expect, Within };
	}
}