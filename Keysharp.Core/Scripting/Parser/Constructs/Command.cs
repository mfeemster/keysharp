using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		internal static string[] SplitCommandParameters(string code, bool[] exp = null)
		{
			var parts = new List<string>();
			bool start = true, expr = false, str = false;
			var last = 0;
			int[] levels = [0, 0, 0]; // parentheses, objects, arrays

			for (var i = 0; i < code.Length; i++)
			{
				var sym = code[i];

				if (str)
				{
					if (sym == StringBound)
						str = !str;
					else if (sym == Multicast)
						goto delim;

					continue;
				}
				else if (IsCommentAt(code, i))
					break;

				if (start)
				{
					if (IsSpace(sym))
						continue;
					else
					{
						start = false;
						var n = i + 1;
						expr = sym == Resolve && (n < code.Length ? IsSpace(code[n]) : true);
						n = parts.Count;

						if (exp != null && exp.Length > n && exp[n])
							expr = true;
					}
				}

				if (expr)
				{
					switch (sym)
					{
						case StringBound: str = !str; break;

						case ParenOpen: levels[0]++; break;

						case ParenClose: levels[0]--; break;

						case BlockOpen: levels[1]++; break;

						case BlockClose: levels[1]--; break;

						case ArrayOpen: levels[2]++; break;

						case ArrayClose: levels[2]--; break;
					}
				}

				delim:

				if (sym == Multicast && (i == 0 || code[i - 1] != Escape) && !str && levels[0] == 0 && levels[1] == 0 && levels[2] == 0)
				{
					parts.Add(code.Substring(last, i - last));
					last = i + 1;
					start = true;
					expr = false;
				}
			}

			var d = code.Length - last;

			if (d != 0)
				parts.Add(code.Substring(last, d));

			return parts.ToArray();
		}

		internal static string[] SplitCommandStatement(string code)
		{
			var i = 0;
			var d = false;
			code = code.TrimStart(Spaces);

			for (; i < code.Length; i++)
			{
				if (code[i] == Multicast)
					break;
				else if (IsSpace(code[i]))
					d = true;
				else if (d)
				{
					i--;
					break;
				}
			}

			var n = i + 1;
			var parts = new[] { code.Substring(0, i).TrimEnd(Spaces), n >= code.Length ? string.Empty : code.Substring(n).TrimStart(Spaces) };
			return parts;
		}

		private string ConvertCommandToExpression(string code)
		{
			var span = code.AsSpan();
			var firstSpace = code.IndexOf(' ');

			if (firstSpace != -1)
			{
				return string.Concat(span.Slice(0, firstSpace).TrimEnd(','), "(", span.Slice(firstSpace + 1), ")");
			}
			else if (span.IndexOfAny(ParensSv) == -1)
			{
				return $"{code}()";
			}

			return "";
		}

		private bool ParseCommand(CodeLine codeLine, string code)
		{
			var newCode = ConvertCommandToExpression(code);

			if (newCode.Length > 0)
			{
				var lineNumber = codeLines.IndexOf(codeLine);
				codeLines[lineNumber] = new CodeLine(codeLine.FileName, codeLine.LineNumber, newCode);
				return true;
			}
			else
				return false;
		}

		private CodeExpression ParseCommandParameter(CodeLine codeLine, string code, bool byref = false, bool expr = false)
		{
			code = code.Trim(Spaces);

			if (code.Length == 0)
				return nullPrimitive;

			if (expr && code.Length > 2 && code[0] == Resolve && code[code.Length - 1] == Resolve)
				code = code.Substring(1, code.Length - 2);

			var explicitExpr = false;

			if (IsExpressionParameter(code))
			{
				code = code.Substring(2);
				expr = true;
				explicitExpr = true;
			}

			if (expr)
			{
				try
				{
					return ParseSingleExpression(codeLine, code, false);
				}
				catch (ParseException)
				{
					// soft failure for implicit expression mode only
					if (explicitExpr)
						throw;

					return nullPrimitive;
				}
			}

			code = StripComment(code);
			return byref ? VarId(codeLine, code, false) : VarIdExpand(codeLine, code);
		}
	}
}