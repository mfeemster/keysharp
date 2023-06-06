using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private Token GetToken(CodeLine line)
		{
			var code = line.Code;
			code = code.TrimStart(Spaces);

			if (code.Length == 0)
				return Token.Unknown;

			if (IsGetOrSet(code, "get"))
				return Token.PropGet;
			else if (IsGetOrSet(code, "set"))
				return Token.PropSet;
			else if (IsProperty(line))
				return Token.Prop;
			else if (IsFlowOperator(code))
				return Token.Flow;
			else if (IsLabel(code))
				return Token.Label;
			else if (IsHotkeyLabel(code) || IsHotstringLabel(code))
				return Token.Hotkey;
			else if (IsAssignment(code))
				return Token.Assign;
			else if (IsDirective(code))
				return Token.Directive;
			else return IsCommand(code) ? Token.Command : Token.Expression;
		}

		private bool IsAssignment(string code, bool checkexprassign = false)
		{
			var i = 0;

			while (i < code.Length && (IsIdentifier(code[i]) || code[i] == Resolve)) i++;

			if (i == 0 || i == code.Length)
				return false;

			while (IsSpace(code[i])) i++;

			if (i < code.Length && code[i] == Equal)
				return true;

			//The statement above only checks for simple =, not :=. For some reason, checking for := completely throws off other parts of the parsing logic.
			//My hunch is that := is intended to be treated as an expression, not an assignment.
			//So it's better to leave this out for now, and figure out what would be ideal in the future if needed.
			//if (checkexprassign)
			//  if (i < code.Length - 1 && (code[i] == AssignPre && code[i + 1] == Equal))
			//      return true;
			return false;
		}

		private bool IsCommand(string code)
		{
			var i = 0;

			while (i < code.Length && IsIdentifier(code[i])) i++;

			if (i == 0)
				return false;
			else if (i == code.Length)
				return true;
			else if (code[i] == Multicast)
				return true;
			else if (IsKeyword(code.Substring(0, i)))
				return false;
			else if (IsSpace(code[i]))
			{
				i++;

				while (i < code.Length && IsSpace(code[i])) i++;

				if (i < code.Length && code[i] == Equal)
					return false;
				else if (IsCommentAt(code, i))
					return true;

				if (IsIdentifier(code[i]))
					return !IsKeyword(code[i]);

				int y = i + 1, z = i + 2;

				if (y < code.Length)
				{
					if (code[y] == Equal)
						return false;
					else if (z + 1 < code.Length && code[i] == code[y] && code[y] == code[z] && code[z + 1] == Equal)
						return false;
					else if (z < code.Length && code[i] == code[y] && code[z] == Equal)
						return false;
					else if (LaxExpressions)
					{
						if (IsOperator(code.Substring(i, 1)) && code.Contains(" ? "))
							return false;
					}
				}

				var pre = code.AsSpan(0, i).TrimEnd(Spaces).ToString();
				return !IsPrimitiveObject(pre);
			}
			else
				return false;
		}

		private bool IsDirective(string code) => code.Length > 2 && code[0] == Directive;

		private bool IsFlowOperator(string code)
		{
			const int offset = 4;
			var delimiters = new char[Spaces.Length + offset];
			delimiters[0] = Multicast;
			delimiters[1] = BlockOpen;
			delimiters[2] = ParenOpen;
			delimiters[3] = HotkeyBound;//Need ':' colon for default: statements. Unsure if this breaks anything else.
			Spaces.CopyTo(delimiters, offset);
			var word = code.Split(delimiters, 2)[0].ToLowerInvariant();

			if (Scope.Length > 0)
			{
				switch (word)
				{
					case FunctionStatic:
						return true;
				}
			}

			switch (word)
			{
				case FlowBreak:
				case FlowContinue:
				case FlowCase:
				case FlowClass:
				case FlowDefault:
				case FlowFor:
				case FlowElse:
				case FlowGosub:
				case FlowGoto:
				case FlowIf:
				case FlowLoop:
				case FlowReturn:
				case FlowWhile:
				case FunctionLocal:
				case FunctionGlobal:
				case FlowTry:
				case FlowCatch:
				case FlowFinally:
				case FlowUntil:
				case FlowSwitch:
				case Throw:
					return true;
			}

			return false;
		}

		private bool IsFunction(string code, string next)
		{
			if (code.Length == 0 || code[0] == ParenOpen)
				return false;

			var stage = 0;
			var str = false;

			if (code.StartsWith("static", StringComparison.OrdinalIgnoreCase))
				code = code.AsSpan(6).Trim(SpaceTab).ToString();

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				switch (stage)
				{
					case 0:
						if (sym == ParenOpen)
							stage++;
						else if (!IsIdentifier(sym))
							return false;

						break;

					case 1:
						if (sym == StringBound)
							str = !str;
						else if (!str && sym == ParenClose)
							stage++;

						break;

					case 2:
						if (sym == BlockOpen)
							return true;
						else if (IsCommentAt(code, i))
							goto donext;
						else if (!IsSpace(sym))
							return false;

						break;
				}
			}

			donext:

			if (next.Length == 0)
				return false;

			var reader = new StringReader(next);

			while (reader.Peek() != -1)
			{
				var sym = (char)reader.Read();

				if (sym == BlockOpen)
					return true;
				else if (!IsSpace(sym))
					return false;
			}

			return false;
		}

		private bool IsGetOrSet(string code, string name)
		=> code.StartsWith(name, StringComparison.OrdinalIgnoreCase) && typeStack.Peek().Name != mainClassName&& Scope.Length > 0;
		//=> string.Compare(code, name, true) == 0 && typeStack.Peek().Name != mainClassName&& Scope.Length > 0;

		private bool IsHotkeyLabel(string code)
		{
			var z = code.IndexOf(HotkeySignal);

			if (z == -1)
				return false;

			var p = false;

			for (var i = 0; i < z; i++)
			{
				var sym = code[i];

				switch (sym)
				{
					case '#':
					case '!':
					case '^':
					case '+':
					case '<':
					case '>':
					case '*':
					case '~':
					case '$':
						break;

					case '&':
						p = false;
						break;

					default:
						if (!IsSpace(sym) && !char.IsLetterOrDigit(sym))
						{
							if (p)
								return false;
							else
								p = true;
						}

						break;
				}
			}

			return true;
		}

		private bool IsHotstringLabel(string code) => code.Length > 0 && code[0] == HotkeyBound&& code.Contains(HotkeySignal)&& code.Count(ch => ch == HotkeyBound) >= 4;

		private bool IsLabel(string code)
		{
			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (IsIdentifier(sym))
					continue;

				switch (sym)
				{
					case HotkeyBound:
						if (i == 0)
							return false;
						else if (i == code.Length - 1)
							return true;
						else
						{
							var sub = StripCommentSingle(code.Substring(i));
							return sub.Length == 0 || IsSpace(sub);
						}

					case ParenOpen:
					case ParenClose:
						break;

					default:
						return false;
				}
			}

			return false;
		}

		private bool IsProperty(CodeLine line)
		{
			var code = line.Code;

			if (typeStack.Peek().Name != mainClassName && Scope.Length == 0)
			{
				if (code.Contains(":="))
					return false;

				if (code.EndsWith('{'))
					code = code.TrimEnd(SpaceOpenBrace);

				var copy = code;
				var isstatic = false;

				if (copy.StartsWith("static "))
				{
					copy = copy.Substring(7, code.Length - 7);
					isstatic = true;
				}

				var openBracket = copy.IndexOf('[');

				if (openBracket != -1)
					copy = copy.AsSpan(0, openBracket).Trim().ToString();

				if (copy.Length > 0)
				{
					var isitem = string.Compare(copy, "__Item", true) == 0;

					if (openBracket != -1)
					{
						var closeBracket = code.IndexOf(']');

						if (closeBracket == code.Length - 1)
						{
							if (!isitem)
								throw new ParseException("Indexed properties are not supported except in the special case of the __Item property.", line);
						}
						else
							throw new ParseException("Missing close bracket on property indexer.", line);
					}
					else if (isitem)
						throw new ParseException("The __Item property must have brackets and take at least one parameter.", line);

					if (isstatic && isitem)
						throw new ParseException("The __Item property cannot be static.", line);

					if (IsIdentifier(copy))
						return true;
				}
			}

			return false;
		}

		private bool IsSpace(char sym) => System.Array.IndexOf(Spaces, sym) != -1;

		private bool IsSpace(string code)
		{
			foreach (var sym in code)
				if (!IsSpace(sym))
					return false;

			return true;
		}

		private enum Token
		{ Unknown, Assign, Command, Label, Hotkey, Flow, Throw, Expression, Directive, Prop, PropGet, PropSet }
	}
}