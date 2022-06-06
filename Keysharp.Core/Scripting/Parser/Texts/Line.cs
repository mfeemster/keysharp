using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private bool IsContinuationLine(string code, bool next)
		{
			if (code.Length == 0)
				return false;

			//This is needed so that ordinary lines that end in an operator, like FileAppend, pass, *, are not considered continuations.
			//Checking for a comma appears to be enough, but it might need further checks if some corner case throws it off in the future.
			if (next && code.Contains(','))
				return false;

			var index = next ? code.Length - 1 : 0;
			var offset = next ? -1 : 1;
			var nextindex = index + offset;

			switch (code[index])
			{
				case Divide:
				case BitOR:
				case Concatenate:
				case Equal:
				case TernaryA:
				case Not:
				case BitNOT:
				case BitXOR:
				case Address:
				case Less:
				case Greater:
					return !(IsHotstringLabel(code) || IsHotkeyLabel(code));

				//case ParenOpen ://Unsure if these would ever be needed.
				//case BlockOpen :
				//case ArrayOpen :
				//  return true;

				case Multiply:
					if (1 < code.Length && code[nextindex] == MultiComA)
						return false;

				goto case Not;

				case Add:
				case Subtract:
					if (1 < code.Length && code[nextindex] == code[index])
						return false;

				goto case Not;

				case TernaryB:
					if (!next && code.Length > 1 && code[1] == Equal)
						return true;

					return !(code.Length > 1 && !IsSpace(code[nextindex]));

				default:
					if (code[index] == Multicast)
					goto case Divide;
					break;
			}

			//This is really weird, and likely wrong, and means a variable name can never start with and or or, such as original.//MATT
			//if (code.Length >= AndTxt.Length && code.Substring(0, AndTxt.Length).Equals(AndTxt, StringComparison.OrdinalIgnoreCase))
			//  return true;
			//else if (code.Length >= OrTxt.Length && code.Substring(0, OrTxt.Length).Equals(OrTxt, StringComparison.OrdinalIgnoreCase))
			//  return true;
			//What they probably meant was this.//MATT
			var splits = code.Split(new char [] { ' ', '\t' });
			index = next ? splits.Length - 1 : 0;

			if (splits[index].Equals(AndTxt, System.StringComparison.OrdinalIgnoreCase) ||
					splits[index].Equals(OrTxt, System.StringComparison.OrdinalIgnoreCase) ||
					splits[index].Equals(NotTxt, System.StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}

		private void MoveToEOL(string code, ref int i)
		{
			while (i < code.Length)
			{
				switch (code[i])
				{
					case CR:
						var n = i + 1;

						if (n < code.Length && code[n] == LF)
							i = n;

					goto case LF;

					case LF:
						return;

					default:
						i++;
						break;
				}
			}
		}
	}
}