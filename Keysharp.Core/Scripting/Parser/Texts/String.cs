namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal static string EscapedString(string code, bool resolve) => EscapedString(code.AsSpan(), resolve);

		internal static string EscapedString(ReadOnlySpan<char> code, bool resolve)
		{
			if (code.Length == 0)
				return DefaultObject;

			var buffer = new StringBuilder(code.Length + 32);
			var escaped = false;

			foreach (var sym in code)
			{
				if (escaped)
				{

					_ = sym switch
				{
						'n' => buffer.Append('\n'),
							'r' => buffer.Append('\r'),
							'b' => buffer.Append('\b'),
							't' => buffer.Append('\t'),
							'v' => buffer.Append('\v'),
							'a' => buffer.Append('\a'),
							'f' => buffer.Append('\f'),
							's' => buffer.Append(' '),
							'0' => buffer.Append('\0'),
							//case '"': _ = buffer.Append('"'); break;
							//
							//case '\'': _ = buffer.Append('\''); break;
							//
							//case ';': _ = buffer.Append(';'); break;
							//
							//case ':': _ = buffer.Append(':'); break;
							//
							//case '{': _ = buffer.Append('{'); break;
							_ => buffer.Append(sym),//if (sym == Resolve)//This was likely here to parse legacy style syntax, but makes it impossible to send "'%", so we omit it.
							//_ = buffer.Append(Escape);
					};

					escaped = false;
				}
				else if (sym == Escape)
					escaped = true;
				else
					_ = buffer.Append(sym);
			}

			return buffer.ToString();
		}

		internal static string MultilineString(string code, int lineNumber, string name)
		{
			var reader = new StringReader(code);
			var line = reader.ReadLine().Trim(Spaces);

			if (line.Length < 1 || line[0] != ParenOpen)
				throw new ParseException($"Multiline string length of {line.Length} is not < 1 or the first character of {line[0]} is not '('.", lineNumber, code, name);

			var join = DefaultNewLine;
			bool? ltrim = null;
			bool rtrim = true, stripComments = false, percentResolve = true, literalEscape = false;

			if (line.Length > 2)
			{
				if (line.Contains('%'))
				{
					percentResolve = false;
					line = line.Replace("%", string.Empty);
				}

				var span = line.AsSpan(1).Trim();

				foreach (Range r in span.SplitAny(SpacesSv))
				{
					var option = span[r];
					if (option.IsEmpty)
						continue;

					if (option.StartsWith(Keyword_Join, StringComparison.OrdinalIgnoreCase))
					{
						join = EscapedString(option.Slice(4), false);
					}
					else
					{
						switch (option)
						{
							case var b when option.Equals("ltrim", StringComparison.OrdinalIgnoreCase):
								ltrim = true;
								break;

							case var b when option.Equals("ltrim0", StringComparison.OrdinalIgnoreCase):
								ltrim = false;
								break;

							case var b when option.Equals("rtrim", StringComparison.OrdinalIgnoreCase):
								rtrim = true;
								break;

							case var b when option.Equals("rtrim0", StringComparison.OrdinalIgnoreCase):
								rtrim = false;
								break;

							case var b when option.Equals("comments", StringComparison.OrdinalIgnoreCase):
							case var b2 when option.Equals("comment", StringComparison.OrdinalIgnoreCase):
							case var b3 when option.Equals("com", StringComparison.OrdinalIgnoreCase):
							case var b4 when option.Equals("c", StringComparison.OrdinalIgnoreCase):
								stripComments = true;
								break;

							case var b4 when option.Equals("`", StringComparison.OrdinalIgnoreCase):
								literalEscape = true;
								break;

							default:
								const string joinOpt = "join";

								if (option.Length > joinOpt.Length && option.Slice(0, joinOpt.Length).Equals(joinOpt, StringComparison.OrdinalIgnoreCase))
									join = option.Slice(joinOpt.Length).ToString().Replace("`s", " ");
								else
									throw new ParseException(ExMultiStr, lineNumber, code, name);

								break;
						}
					}
				}
			}

			var str = new StringBuilder(code.Length);
			var resolve = Resolve.ToString();
			var escape = Escape.ToString();
			var cast = Multicast.ToString();
			var resolveEscaped = string.Concat(escape, resolve);
			var escapeEscaped = new string(Escape, 2);
			var castEscaped = string.Concat(escape, cast);

			// Track default indent from first content line
			string indentSample = null;
			bool firstLine = true;

			while ((line = reader.ReadLine()) != null)
			{
				var check = line.Trim();

				if (check.Length > 0 && check[0] == ParenClose)
					break;

				// On first content line, capture indent sample if trimming
				if (firstLine)
				{
					firstLine = false;
					if (!ltrim.HasValue)
					{
						// Capture only the first run of identical indent characters (space or tab)
						if (line.Length > 0 && (line[0] == ' ' || line[0] == '\t'))
						{
							char indentChar = line[0];
							int count = 1;
							while (count < line.Length && line[count] == indentChar)
								count++;
							indentSample = new string(indentChar, count);
						}
					}
				}

				if (!ltrim.HasValue && !string.IsNullOrEmpty(indentSample) && line.StartsWith(indentSample))
				{
					line = line.Substring(indentSample.Length);
				}

				if (ltrim.HasValue && ltrim.Value)
				{
					if (rtrim)
						line = line.Trim(Spaces);
					else
						line = line.TrimStart(Spaces);
				}
				else if (rtrim)
					line = line.TrimEnd(Spaces);

				if (stripComments)
				{
					line = StripComment(line, out bool strippedAny);
					if (strippedAny && line == "")
						continue;
				}

				if (!percentResolve)
					line = line.Replace(resolve, resolveEscaped);

				if (literalEscape)
					line = line.Replace(escape, escapeEscaped);

				line = line.Replace("\"", Escape + "\"");//Can't use interpolated string here because the AStyle formatter misinterprets it.
				line = line.Replace(cast, castEscaped);
				_ = str.Append(line);
				_ = str.Append(join);
			}

			if (str.Length == 0)
				return DefaultObject;

			_ = str.Remove(str.Length - join.Length, join.Length);
			return str.ToString();
		}

		private int FindNextBalanced(string s, char ch1, char ch2)
		{
			var level = 0;
			var escape = false;
			var inquote = false;

			for (int i = 0; i < s.Length; i++)
			{
				char ch = s[i];

				if (ch == '\"')
				{
					if (!inquote)
					{
						if (i == 0 || s[i - 1] != Escape)
							inquote = true;
					}
					else
					{
						if (i == 0 || s[i - 1] != Escape || !escape)//Checking escape accounts for ``.
							inquote = false;
					}
				}

				escape = ch == Escape ? !escape : false;

				if (!inquote)
				{
					if (ch == ch1)
					{
						level++;
					}
					else if (ch == ch2)
					{
						level--;

						if (level == 0)
							return i;
					}
				}
			}

			return -1;
		}

		private void RemoveExcessParentheses(CodeLine codeLine, List<object> parts)
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
								throw new ParseException(ExUnbalancedParens, codeLine);

							break;
					}
				}

				if (level != 0)
					return;

				parts.RemoveAt(last);
				parts.RemoveAt(0);
			}
		}

		private List<string> SplitStringBalanced(string s, char delim, bool addEmpty = false)
		{
			var escape = false;
			var inquote = false;
			var parenLevel = 0;
			var braceLevel = 0;
			var bracketLevel = 0;
			var parts = new List<string>();
			var sb = new StringBuilder();

			for (int i = 0; i < s.Length; i++)
			{
				char ch = s[i];

				if (ch == '\"')
				{
					if (!inquote)
					{
						if (i == 0 || s[i - 1] != Escape)
							inquote = true;
					}
					else
					{
						if (i == 0 || s[i - 1] != Escape || !escape)//Checking escape accounts for ``.
							inquote = false;
					}
				}

				escape = ch == Escape ? !escape : false;

				if (ch == '(' && !inquote)//Either it's a ( or a function call which will end with a (.
				{
					parenLevel++;
					_ = sb.Append(ch);
				}
				else if (ch == ')' && !inquote)
				{
					if (parenLevel > 0)
						_ = sb.Append(ch);

					parenLevel--;
				}
				else if (ch == '{' && !inquote)
				{
					braceLevel++;
					_ = sb.Append(ch);
				}
				else if (ch == '}' && !inquote)
				{
					if (braceLevel > 0)
						_ = sb.Append(ch);

					braceLevel--;
				}
				else if (ch == '[' && !inquote)
				{
					_ = sb.Append(ch);
					bracketLevel++;
				}
				else if (ch == ']' && !inquote)
				{
					if (bracketLevel > 0)
						_ = sb.Append(ch);

					bracketLevel--;
				}
				else if (parenLevel == 0 && braceLevel == 0 && bracketLevel == 0 && ch == delim && !inquote)//Assuming delim is != to any of the above characters.
				{
					//parts.Add(EscapedString(sb.ToString(), false));
					parts.Add(sb.ToString());
					_ = sb.Clear();
				}
				else
					_ = sb.Append(ch);
			}

			if (sb.Length > 0 || (addEmpty && s.Length > 0 && s[s.Length - 1] == delim))
			{
				//parts.Add(EscapedString(sb.ToString(), false));
				parts.Add(sb.ToString());
			}

			return parts;
		}

		private CodeExpression StringConcat(params CodeExpression[] parts)
		{
			var list = new List<CodeExpression>(parts.Length);

			foreach (var part in parts)
			{
				if (part is CodePrimitiveExpression cpe && cpe.Value is string s)
				{
					if (string.IsNullOrEmpty(s))
						continue;
				}

				list.Add(part);
			}

			if (list.Count == 1)
				return list[0];

			var str = typeof(object);// typeof(string);
			var method = (CodeMethodReferenceExpression)InternalMethods.StringConcat;
			var all = new CodeArrayCreateExpression(str, list.ToArray());
			return new CodeMethodInvokeExpression(method, all);
		}

		//private string Replace(string input, string search, string replace)
		//{
		//  var buf = new StringBuilder(input.Length);
		//  int z = 0, n = 0, l = search.Length;
		//
		//  while (z < input.Length && (z = input.IndexOf(search, z, System.StringComparison.OrdinalIgnoreCase)) != -1)
		//  {
		//      if (n < z)
		//          _ = buf.Append(input, n, z - n);
		//
		//      _ = buf.Append(replace);
		//      z += l;
		//      n = z;
		//  }
		//
		//  if (n < input.Length)
		//      _ = buf.Append(input, n, input.Length - n);
		//
		//  return buf.ToString();
		//}
	}
}