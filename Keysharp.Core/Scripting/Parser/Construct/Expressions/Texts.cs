using System.Collections.Generic;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private void RemoveExcessParentheses(List<object> parts)
		{
			while (parts.Count > 1)
			{
				var level = 0;
				var last = parts.Count - 1;

				if (!(--last > 1 &&
						parts[0] is string s0 && s0.Length == 1 && s0[0] == ParenOpen &&
						parts[last] is string sl && sl.Length == 1 && sl[0] == ParenClose))
					return;

				for (var i = 0; i < last; i++)
				{
					var check = parts[i] as string;

					if (string.IsNullOrEmpty(check))
						continue;

					switch (check[check.Length - 1])
					{
						case ParenOpen:
							level++;
							break;

						case ParenClose:
							if (check.Length != 1)
								break;
							else if (--level < 0)
								throw new ParseException(ExUnbalancedParens);

							break;
					}
				}

				if (level != 0)
					return;

				parts.RemoveAt(last);
				parts.RemoveAt(0);
			}
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
					var escape = false;
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

					list.Add(str.ToString());
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
	}
}