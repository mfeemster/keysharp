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

			while (i < code.Length && IsSpace(code[i])) i++;

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

					return code[0] switch
				{
						//>>=
						Greater or Less or Divide or TernaryA => true,
						_ => false,
				};
			}
			else
				return false;
		}
		else if (code.Length == 4)
			{
				if (code[0] == code[1] && code[1] == code[2])
				{

					return code[0] switch
				{
						Greater => true,
						_ => false,
				};
			}
			else
				return false;
		}
		else
		{

			return code[0] switch
			{
					Greater or Less or Not or BitNOT => false,
					_ => true,
			};
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

			return code[i] switch
		{
				Equal or Not or Greater or Less => false,
				_ => true,
		};
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
			var z = code.FindFirstNotInQuotes(HotkeySignal);

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
					case ';':
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

		internal static bool IsHotstringLabel(string code) => code.Length > 0 && code[0] == HotkeyBound&& code.FindFirstNotInQuotes(HotkeySignal) != -1 && code.Count(ch => ch == HotkeyBound) >= 4;

		internal static bool IsIdentifier(char symbol) => char.IsLetterOrDigit(symbol) || VarExt.Contains(symbol);

		internal static bool IsKeyword(string code) => keywords.Contains(code);

		internal static bool IsKeyword(char symbol)
		{

			return symbol switch
		{
				TernaryA => true,
				_ => false,
		};
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
				case Unset:
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

		internal static bool IsSpace(char sym) => SpacesSv.Contains(sym);

		internal static bool IsSpace(string code)
		{
			foreach (var sym in code)
				if (!IsSpace(sym))
					return false;

			return true;
		}

		internal static bool IsUnaryOperator(Script.Operator op)
		{

			return op switch
		{
				//Minus doesn't seem to be needed here.
				Script.Operator.Subtract or Script.Operator.LogicalNot or Script.Operator.LogicalNotEx or Script.Operator.BitwiseNot or Script.Operator.BitwiseAnd or Script.Operator.Dereference => true,
				//TODO
				//This messes up the postfix operator when used in an assignment like y := x++
				//case Script.Operator.Add:
				//return true;
				_ => false,
		};
	}

	internal bool IsFlowOperator(string code)
		{
			foreach (Range r in code.AsSpan().SplitAny(FlowDelimiters2))
			{
				var word = code.AsSpan(r);

				if (Scope.Length > 0)
				{
					if (word.Equals(FunctionStatic, StringComparison.OrdinalIgnoreCase))
						return true;
				}

				return flowOperatorsAlt.Contains(word);
			}

			return false;
		}

		internal bool IsIdentifier(string token) => IsIdentifier(token, false);

		internal bool IsIdentifier(string token, bool dynamic)
		{
			var isId = false;

			if (string.IsNullOrEmpty(token))
				return false;

			if (token == "?" || token == "??" || token == "??=")
				return false;

			foreach (var sym in token)
			{
				if (!IsIdentifier(sym))
				{
					if (dynamic && sym == Resolve)
					{
						isId = !isId;
						//if (!isId)//If we've gone from true back to false it means we've completed a %% pair, so assume everything is part of it.
						//  break;
						continue;
					}
					else if (isId)//Count anything within %% to be valid, because it will be evaluated as an expression.
						continue;

					return false;
				}
			}

			if (double.TryParse(token, out var _))//Need to ensure it's not a number, because identifiers can't be numbers.
				return false;

			if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
					int.TryParse(token.AsSpan(2), NumberStyles.HexNumber, culture, out var _))
				return false;

			return (dynamic && ((token.Count(ch => ch == Resolve) & 2) == 2)) || string.Compare(token, "this", StringComparison.OrdinalIgnoreCase) == 0 || Ch.IsValidIdentifier(token) || Ch.IsValidIdentifier($"@token");
		}

		internal bool IsLegacyIf(string code)
		{
			var part = code.TrimStart(Spaces).Split(Spaces, 3);

			if (part.Length < 2 || !IsIdentifier(part[0]))
				return false;

			return part[1].ToLowerInvariant() switch
		{
				NotTxt or BetweenTxt or InTxt or ContainsTxt or IsTxt => true,
				_ => false,
		};
	}

	internal static bool IsNumericString(ReadOnlySpan<char> str) => long.TryParse(str, out _) || double.TryParse(str, out _);

		internal bool IsVariable(string code) => IsIdentifier(code, true)&& !IsKeyword(code);

		internal List<object> SplitTokens(CodeLine codeLine, string code)
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
					var prevWasTernaryA = list.Count > 0 && list[list.Count - 1].ToString() == "?";
					var nextIsEqual = i < code.Length - 1 && code[i + 1] == '=';

					if (prevWasTernaryA)
					{
						if (nextIsEqual)
						{
							list[list.Count - 1] = "??=";
							i++;
						}
						else
							list[list.Count - 1] = "??";
					}
					else
						list.Add("?");
				}
				else if (IsCommentAt(code, i))
					MoveToEOL(code, ref i);
				else if (IsIdentifier(sym) || sym == Resolve || (sym == Concatenate && i + 1 < code.Length && IsIdentifier(code[i + 1])))
				{
					var id = new StringBuilder(code.Length);

					if (sym == Resolve)
					{
						var nextResolve = code.IndexOf(Resolve, i + 1);

						if (nextResolve > i)//Same check as below, but need to do here because the first character might be a %.
						{
							var tokenLength = nextResolve - i + 1;
							_ = id.Append(code.Substring(i, tokenLength));
							i = nextResolve + 1;
						}
						else
						{
							_ = id.Append(sym);
							i++;
						}
					}
					else
					{
						_ = id.Append(sym);
						i++;
					}

					for (; i < code.Length; i++)
					{
						sym = code[i];

						if ((sym == 'e' || sym == 'E') && IsNumericString(id.ToString().AsSpan()) && id.ToString().IndexOf("0x") != 0 && i + 1 < code.Length)//IsPrimitiveObject(id.ToString()) && id.Length == code.Length && id.ToString().IndexOf("0x") != 0 && i + 1 < code.Length)
						{
							_ = id.Append(sym);
							sym = code[++i];

							if (!(sym == '+' || sym == '-' || char.IsDigit(sym)))
								throw new ParseException(ExInvalidExponent, codeLine);

							_ = id.Append(sym);
						}
						else if (sym == Resolve && i < code.Length - 1)
						{
							var nextResolve = code.IndexOf(Resolve, i + 1);

							if (nextResolve > i)
							{
								var tokenLength = nextResolve - i + 1;
								_ = id.Append(code.Substring(i, tokenLength));
								i = nextResolve;
							}
						}
						else if (IsIdentifier(sym) || (sym == Concatenate && (i + 1 < code.Length ? code[i + 1] != Equal : true)))
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

					if (IsPrimitiveObject(seq))
					{
						if (seq.Length != 0)
							list.Add(seq);
					}
					else
					{
						_ = id.Clear();
						char lastDelim = (char)0;
						var listCount = list.Count;
						var idstr = "";

						for (var seqi = 0; seqi < seq.Length; seqi++)
						{
							var ch = seq[seqi];

							if (ch == Concatenate)
							{
								idstr = id.ToString();
								lastDelim = ch;

								if (list.Count == listCount)
								{
									if (idstr.Length != 0)
										list.Add(idstr);
								}
								else
								{
									list.Add("[*");//Special signifier [**] that this is a property lookup and not a map[var] lookup. Without distinguishing the two, a map could never have a key that had the same name as a property, such as "Default".

									if (idstr.Contains('%'))//If it was a dynamic variable, don't enclose in quotes.
										list.Add($"{idstr}");
									else
										list.Add(string.Concat("\"", idstr, "\""));//Can't use interpolated string here because the AStyle formatter misinterprets it.

									list.Add("*]");
								}

								_ = id.Clear();
							}
							else if (ch == Resolve)
							{
								var nextResolve = seq.IndexOf(Resolve, seqi + 1);

								if (nextResolve > seqi)
								{
									var tokenLength = nextResolve - seqi + 1;
									_ = id.Append(seq.Substring(seqi, tokenLength));
									seqi = nextResolve;
								}
							}
							else
								_ = id.Append(ch);
						}

						idstr = id.ToString();

						//If the last added token was before a . then add the next token as a property.
						if (idstr.Length > 0)
						{
							if (lastDelim == Concatenate)
							{
								list.Add("[*");//Special signifier [**] that this is a property lookup and not a map[var] lookup. Without distinguishing the two, a map could never have a key that had the same name as a property, such as "Default".

								if (idstr.Contains('%'))//If it was a dynamic variable, don't enclose in quotes.
									list.Add($"{idstr}");
								else
									list.Add(string.Concat("\"", idstr, "\""));//Can't use interpolated string here because the AStyle formatter misinterprets it.

								list.Add("*]");
							}
							else
								list.Add(idstr);
						}

						_ = id.Clear();
					}
				}
				else if (sym == StringBound || sym == StringBoundVerbatim)
				{
					list.Add(ParseString(codeLine, code, ref i, true));
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
											throw new ParseException(ExUnexpected, codeLine);
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
									throw new ParseException(ExUnexpected, codeLine);
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

		private static string ParseString(CodeLine codeLine, string code, ref int i, bool prependVerbatimEscape)
		{
			var escape = false;
			var sym = code[i];
			var verbatim = sym == StringBoundVerbatim;
			var str = new StringBuilder(code.Length);
			_ = str.Append(StringBound);
			i++;

			if (i == code.Length)
				throw new ParseException(ExUntermStr, codeLine);

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

					if (prependVerbatimEscape && sym == StringBound)
						_ = str.Append(Escape);

					_ = str.Append(sym);

					if ((!isbound || escape) && i == code.Length - 1)//If we've reached the end and it's not a quote. or it is a quote but we are in escape, then it's an unterminated string.
						throw new ParseException(ExUntermStr, codeLine);
				}
				else
				{
					var isbound = sym == StringBound;
					_ = str.Append(sym);

					if (!escape && isbound)
						break;

					if ((!isbound || escape) && i == code.Length - 1)//If we've reached the end and it's not a quote. or it is a quote but we are in escape, then it's an unterminated string.
						throw new ParseException(ExUntermStr, codeLine);
				}

				escape = sym == Escape ? !escape : false;
			}

			return str.ToString();
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

		private Token GetToken(CodeLine codeLine)
		{
			var code = codeLine.Code;
			code = code.TrimStart(Spaces);

			if (code.Length == 0)
				return Token.Unknown;

			var codeSpan = code.AsSpan();

			if (IsGetOrSet(codeSpan, "get"))
				return Token.PropGet;
			else if (IsGetOrSet(codeSpan, "set"))
				return Token.PropSet;
			else if (IsProperty(codeLine))
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
		{
			if (code.StartsWith(name, StringComparison.OrdinalIgnoreCase))
			{
				var parensIndex = code.IndexOfAny(ParensSv);

				if (parensIndex >= 0)//If there are parens, ensure they are preceded by a =>
				{
					var fatArrowIndex = code.IndexOf("=>");

					if (fatArrowIndex == -1 || fatArrowIndex > parensIndex)
						return false;
				}

				return InClassDefinition() && Scope.Length > 0;
			}

			return false;
		}

		private bool IsProperty(CodeLine codeLine)
		{
			var code = codeLine.Code;

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
								throw new ParseException("Indexed properties are not supported except in the special case of the __Item property.", codeLine);
						}
						else
							throw new ParseException("Missing close bracket on property indexer.", codeLine);
					}
					else if (isitem)
						throw new ParseException("The __Item property must have brackets and take at least one parameter.", codeLine);

					if (isstatic && isitem)
						throw new ParseException("The __Item property cannot be static.", codeLine);

					if (IsIdentifier(copy))
						return true;
				}
			}

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