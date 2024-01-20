using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Keysharp.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal static bool IsAssignment(string code, bool checkexprassign = false)
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

		internal static bool IsAssignOp(string code)
		{
			if (!(code.Length >= 2 && code.Length <= 4))
				return false;

			if (code[0] == Equal || code[code.Length - 1] != Equal)
				return false;

			if (code.Length == 3)
			{
				if (code[0] == code[1])
				{
					switch (code[0])
					{
						case Greater:
						case Less:
						case Divide:
							return true;

						default:
							return false;
					}
				}
				else
					return false;
			}
			else if (code.Length == 4)
			{
				if (code[0] == code[1] && code[1] == code[2])
				{
					switch (code[0])
					{
						case Greater:
							return true;

						default:
							return false;
					}
				}
				else
					return false;
			}
			else
			{
				switch (code[0])
				{
					case Greater:
					case Less:
					case Not:
					case BitNOT:
						return false;

					default:
						return true;
				}
			}
		}

		internal static bool IsCommand(string code)
		{
			var i = 0;

			while (i < code.Length && (IsIdentifier(code[i]) || code[i] == '.')) i++;

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

		internal static bool IsDynamicReference(string code)
		{
			var d = false;

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (sym == Resolve)
				{
					if (d)
						if (code[i - 1] == Resolve)
							return false;

					d = !d;
				}
				else if (!IsIdentifier(sym))
					return false;
			}

			return code.Length != 0;
		}

		internal static bool IsExpressionIf(string code)
		{
			code = code.TrimStart(Spaces);
			var i = 0;

			if (code.Length == 0)
				return true;

			if (code[0] == ParenOpen)
				return true;

			while (i < code.Length && IsIdentifier(code[i])) i++;

			if (i == 0 || IsKeyword(code.Substring(0, i)))
				return true;

			while (i < code.Length && IsSpace(code[i])) i++;

			if (i == 0 || i == code.Length)
				return false;

			switch (code[i])
			{
				case Equal:
				case Not:
				case Greater:
				case Less:
					return false;
			}

			return true;
		}

		internal static bool IsExpressionParameter(string code)
		{
			code = code.TrimStart(Spaces);
			var z = code.IndexOf(Resolve);
			return z == 0 && (code.Length == 1 || IsSpace(code[1]));
		}

		internal static bool IsFunction(string code, string next)
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
						{
							str = !str;
						}
						else if (!str)
						{
							if (sym == ParenClose)
								stage++;
							else if (sym == ParenOpen)
								stage--;
						}

						break;

					case 2:
						if (sym == BlockOpen)
							return true;
						else if (i < code.Length - 2 && sym == '=' && code[i + 1] == '>')
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

		internal static bool IsHotkeyLabel(string code)
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

		//=> string.Compare(code, name, true) == 0 && typeStack.Peek().Name != mainClassName&& Scope.Length > 0;
		internal static bool IsHotstringLabel(string code) => code.Length > 0 && code[0] == HotkeyBound&& code.Contains(HotkeySignal)&& code.Count(ch => ch == HotkeyBound) >= 4;

		internal static bool IsIdentifier(char symbol) => char.IsLetterOrDigit(symbol) || VarExt.IndexOf(symbol) != -1;

		internal static bool IsIdentifier(string token) => IsIdentifier(token, false);

		internal static bool IsIdentifier(string token, bool dynamic)
		{
			if (string.IsNullOrEmpty(token))
				return false;

			if (token[0] == TernaryA && (token.Length == 1 || token.Length == 2 && token[1] == TernaryA))
				return false;

			foreach (var sym in token)
			{
				if (!IsIdentifier(sym))
				{
					if (dynamic && sym == Resolve)
						continue;

					return false;
				}
			}

			if (double.TryParse(token, out var _))//Need to ensure it's not a number, because identifiers can't be numbers.
				return false;

			if (token.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
					int.TryParse(token.AsSpan(2), NumberStyles.HexNumber, culture, out var _))
				return false;

			return true;
		}

		internal static bool IsKeyword(string code) => keywords.Contains(code);

		internal static bool IsKeyword(char symbol)
		{
			switch (symbol)
			{
				case TernaryA:
					return true;

				default:
					return false;
			}
		}

		internal static bool IsLabel(string code)
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

		internal static bool IsLegacyIf(string code)
		{
			var part = code.TrimStart(Spaces).Split(Spaces, 3);

			if (part.Length < 2 || !IsIdentifier(part[0]))
				return false;

			switch (part[1].ToLowerInvariant())
			{
				case NotTxt:
				case BetweenTxt:
				case InTxt:
				case ContainsTxt:
				case IsTxt:
					return true;
			}

			return false;
		}

		internal static bool IsOperator(string code)
		{
			if (!IsAssignOp(code))
				return OperatorFromString(code).Item1;

			return true;
		}

		internal static bool IsPrimitiveObject(string code, out object result)
		{
			result = null;

			if (code == null)
				return true;

			if (code.Length == 0)
			{
				result = code;
				return true;
			}

			switch (code.ToLowerInvariant())
			{
				case TrueTxt:
					//result = 1L;
					result = true;//Althought the AHK documentation says true/false are really just 1/0 under the hood, that causes problems here.
					return true;//Particularly with the Force[type]() functions used in Operate(). Having them be a bool type makes it much easier to determine the caller's intent when comparing values.

				case FalseTxt:
					//result = 0L;
					result = false;
					return true;

				case NullTxt:
					result = null;
					return true;
			}

			//Mono incorrectly determines "." as a numeric value.
			if (code.Length == 1 && code[0] == Concatenate)
				return false;

			var codeTrim = code.Trim(Spaces);
			var longresult = codeTrim.ParseLong(false, false);//Also supports hex, but do not consider raw hex, because then a variable name like a would be returned as 10.

			if (longresult.HasValue)
			{
				result = longresult.Value;
				goto exp;
			}

			if (double.TryParse(codeTrim, NumberStyles.Any, culture, out var d))//This will make any number be a double internally. Not sure if this is what AHK does.
			{
				result = d;
				goto exp;
			}

			result = null;
			return false;
			exp:
			return true;
		}

		internal static bool IsPrimitiveObject(string code) => IsPrimitiveObject(code, out var result);

		internal static bool IsRemap(string code)//This is totally wrong and has nothing to do with how remaps are done in v2.//TODO
		{
			code = code.Trim(Spaces);

			if (code.Length == 0)
				return false;

			if (IsSpace(code[0]))
				return false;

			for (var i = 1; i < code.Length; i++)
			{
				if (IsCommentAt(code, i))
					return true;
				else if (!IsSpace(code[i]))
					return false;
			}

			return true;
		}

		internal static bool IsSpace(char sym) => System.Array.IndexOf(Spaces, sym) != -1;

		internal static bool IsSpace(string code)
		{
			foreach (var sym in code)
				if (!IsSpace(sym))
					return false;

			return true;
		}

		internal static bool IsUnaryOperator(Script.Operator op)
		{
			switch (op)
			{
				case Script.Operator.Subtract://Minus doesn't seem to be needed here.
				case Script.Operator.LogicalNot:
				case Script.Operator.LogicalNotEx:
				case Script.Operator.BitwiseNot:
				case Script.Operator.BitwiseAnd:
				case Script.Operator.Dereference:
					return true;

				//TODO
				//This messes up the postfix operator when used in an assignment like y := x++
				//case Script.Operator.Add:
				//return true;

				default:
					return false;
			}
		}

		internal static bool IsVariable(string code) => IsIdentifier(code, true)&& !IsKeyword(code);

		internal bool IsFlowOperator(string code)
		{
			var word = code.Split(FlowDelimiters2, 2)[0].ToLowerInvariant();

			if (Scope.Length > 0)
			{
				switch (word)
				{
					case FunctionStatic:
						return true;
				}
			}

			return flowOperators.Contains(word);
		}

		private List<object> ExtractRange(List<object> parts, int start, int end)
		{
			var extracted = new List<object>(end - start);

			for (var i = start; i < end; i++)
			{
				extracted.Add(parts[start]);
				parts.RemoveAt(start);
			}

			return extracted;
		}

		private Token GetToken(CodeLine line)
		{
			var code = line.Code;
			code = code.TrimStart(Spaces);

			if (code.Length == 0)
				return Token.Unknown;

			var codeSpan = code.AsSpan();

			if (IsGetOrSet(codeSpan, "get"))
				return Token.PropGet;
			else if (IsGetOrSet(codeSpan, "set"))
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

		private bool IsDirective(string code) => code.Length > 2 && code[0] == Directive;

		private bool IsGetOrSet(ReadOnlySpan<char> code, string name)
		=> code.StartsWith(name, StringComparison.OrdinalIgnoreCase) && code.IndexOfAny(ParensSv) == -1 && InClassDefinition() && Scope.Length > 0;

		private bool IsProperty(CodeLine line)
		{
			var code = line.Code;

			if (InClassDefinition() && Scope.Length == 0)
			{
				if (code.Contains(":="))
					return false;

				if (code.EndsWith('{'))
					code = code.TrimEnd(SpaceTabOpenBrace);

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

		private List<object> SplitTokens(string code)
		{
			var json = false;
			var list = new List<object>();

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (IsSpace(sym))
					continue;
				else if (sym == TernaryA)
				{
					if (list.Count > 0 && list[list.Count - 1].ToString() == "?")
						list[list.Count - 1] = "??";
					else
						list.Add("?");
				}
				else if (IsCommentAt(code, i))
					MoveToEOL(code, ref i);
				else if (IsIdentifier(sym) || sym == Resolve || (sym == Concatenate && i + 1 < code.Length && IsIdentifier(code[i + 1])))
				{
					var id = new StringBuilder(code.Length);
					_ = id.Append(sym);
					i++;

					for (; i < code.Length; i++)
					{
						sym = code[i];

						if ((sym == 'e' || sym == 'E') && IsPrimitiveObject(id.ToString()) && id.ToString().IndexOf("0x") != 0 && i + 1 < code.Length)
						{
							_ = id.Append(sym);
							sym = code[++i];

							if (!(sym == '+' || sym == '-' || char.IsDigit(sym)))
								throw new ParseException(ExInvalidExponent);

							_ = id.Append(sym);
						}
						else if (IsIdentifier(sym) || sym == Resolve || (sym == Concatenate && (i + 1 < code.Length ? code[i + 1] != Equal : true)))
							_ = id.Append(sym);
						else
						{
							if (sym == ParenOpen && !IsKeyword(id.ToString()) && !id.ToString().Contains(Concatenate.ToString()))
								_ = id.Append(ParenOpen);
							else
								i--;

							break;
						}
					}

					var seq = id.ToString();
					var parts = IsPrimitiveObject(seq) ? new[] { seq } : seq.Split(Concatenate);

					if (parts[0].Length != 0)
						list.Add(parts[0]);

					for (var n = 1; n < parts.Length; n++)
					{
						list.Add("[*");//Special signifier [**] that this is a property lookup and not a map[var] lookup. Without distinguishing the two, a map could never have a key that had the same name as a property, such as "Default".

						if (parts[n].Contains("%"))//If it was a dynamic variable, don't enclose in quotes.
							list.Add($"{parts[n]}");
						else
							list.Add("\"" + parts[n] + "\"");//Can't use interpolated string here because the AStyle formatter misinterprets it.

						list.Add("*]");
						//list.Add(ArrayOpen.ToString());
						//var str = StringBound.ToString();
						//list.Add(string.Concat(str, parts[n], str));
						//list.Add(ArrayClose.ToString());
					}
				}
				else if (sym == StringBound || sym == StringBoundVerbatim)
				{
					list.Add(ParseString(code, ref i));
				}
				else
				{
					var op = new StringBuilder(3);
					var n = i + 1;
					var symNext = n < code.Length ? code[n] : Reserved;
					var tri = false;

					if (sym == symNext)
					{
						var peekAssign = false;

						switch (sym)
						{
							case Divide:
							case Greater:
							case Less:
								peekAssign = true;

							goto case Add;

							case Add:
							case Minus:
							case Multiply:
							case BitOR:
							case BitAND:
								_ = op.Append(sym);
								_ = op.Append(symNext);
								i++;
								tri = true;

								if (peekAssign)
								{
									n = i + 1;

									if (n < code.Length)
									{
										if (n + 2 < code.Length && sym == Greater && symNext == Greater && code[n] == Greater && code[n + 1] == Equal)
										{
											_ = op.Append(code[n]);
											_ = op.Append(code[n + 1]);
											i = n + 1;
										}
										else if ((sym == Greater && code[n] == Greater) || (code[n] == Equal))
										{
											_ = op.Append(code[n]);
											i = n;
										}
									}
								}

								break;
						}
					}

					if (!tri)
					{
						if (symNext == Equal)
						{
							switch (sym)
							{
								case AssignPre:
								case Add:
								case Minus:
								case Multiply:
								case Divide:
								case Concatenate:
								case BitAND:
								case BitXOR:
								case BitOR:
								case BitNOT:
								case Not:
								case Equal:
								case Greater:
								case Less:
								{
									_ = op.Append(sym);
									_ = op.Append(symNext);
									i++;
									n++;
									symNext = n < code.Length ? code[n] : Reserved;

									if (symNext == Equal)
									{
										_ = op.Append(symNext);
										i++;
									}
								}
								break;
							}
						}
						else if (sym == Equal && symNext == Greater)
						{
							_ = op.Append(sym);
							_ = op.Append(symNext);
							i++;
						}
						else if ((sym == Less && symNext == Greater) || (sym == TernaryA && symNext == TernaryA))
						{
							_ = op.Append(sym);
							_ = op.Append(symNext);
							i++;
						}
						else
						{
							switch (sym)
							{
								case Add:
								case Minus:
								case Multiply:
								case Not:
								case BitNOT:
								case BitAND:
								case Greater:
								case Less:
								case BitXOR:
								case BitOR:
								case ParenOpen:
								case ParenClose:
								case Equal:
								case Concatenate:
								case TernaryB:
								case Divide:
								case ArrayOpen:
								case ArrayClose:
									_ = op.Append(sym);
									break;

								case BlockOpen:
									if (json)
									{
										_ = op.Append(sym);
										break;
									}

									blockOpen = true;
									var j = i + 2;

									if (j < code.Length && !IsCommentAt(code, j))
									{
										blockOpen = false;
										json = true;

									goto case BlockOpen;
									}

									j--;

									if (j < code.Length)
									{
										if (code[j] == BlockClose)
										{
											json = true;

										goto case BlockClose;
										}
										else if (!IsSpace(code[j]))
											throw new ParseException(ExUnexpected);
									}

									return list;

								case BlockClose:
									if (!json)
										goto default;

									_ = op.Append(sym);
									break;

								default:
									if (sym == Resolve || sym == Multicast)
									goto case Add;
									throw new ParseException(ExUnexpected);
							}
						}
					}

					if (op.Length == 0)
						_ = op.Append(sym);

					list.Add(op.ToString());
				}
			}

			return list;
		}

		private static string ParseString(string code, ref int i)
		{
			var escape = false;
			var sym = code[i];
			var verbatim = sym == StringBoundVerbatim;
			var str = new StringBuilder(code.Length);
			_ = str.Append(StringBound);
			i++;

			if (i == code.Length)
				throw new ParseException(ExUntermStr);

			for (; i < code.Length; i++)
			{
				sym = code[i];

				if (verbatim)
				{
					var isbound = sym == StringBoundVerbatim;

					if (!escape && isbound)
					{
						_ = str.Append(StringBound);
						break;
					}

					_ = str.Append(sym);

					if ((!isbound || escape) && i == code.Length - 1)//If we've reached the end and it's not a quote. or it is a quote but we are in escape, then it's an unterminated string.
						throw new ParseException(ExUntermStr);
				}
				else
				{
					var isbound = sym == StringBound;
					_ = str.Append(sym);

					if (!escape && isbound)
						break;

					if ((!isbound || escape) && i == code.Length - 1)//If we've reached the end and it's not a quote. or it is a quote but we are in escape, then it's an unterminated string.
						throw new ParseException(ExUntermStr);
				}

				escape = sym == Escape ? !escape : false;
			}

			return str.ToString();
		}

		private string TokensToCode(List<object> tokens)
		{
			var last = "";
			var sb = new StringBuilder();

			foreach (var token in tokens)
			{
				var s = token.ToString();

				if (s == "[*")
				{
					last = s;
					s = ".";
				}
				else if (last == "[*")
				{
					last = s;

					if (s.StartsWith('\"') && s.EndsWith('\"'))
						s = s.Substring(1, s.Length - 2);
				}
				else if (s == "*]")
				{
					last = s;
					continue;//Do nothing.
				}
				else if (s == "=>" || IsOperator(s))
				{
					s = $" {s} ";
				}

				_ = sb.Append(s);
			}

			return sb.ToString();
		}

		private enum Token
		{ Unknown, Assign, Command, Label, Hotkey, Flow, Throw, Expression, Directive, Prop, PropGet, PropSet }
	}
}