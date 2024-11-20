namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private static bool IsBalanced(List<object> source, string ch1, string ch2)
		{
			int ct1 = 0, ct2 = 0;

			foreach (var ob in source)
			{
				var os = ob.ToString();

				if (os == ch1)
					ct1++;
				else if (os == ch2)
					ct2++;
			}

			return ct1 == ct2;
		}

		private bool IsArrayExtension(object item) => item is CodeMethodInvokeExpression cmie&& cmie.Method.MethodName == InternalMethods.ExtendArray.MethodName;

		private bool IsJsonObject(object item) => item is CodeMethodInvokeExpression cmie&& cmie.Method.MethodName == InternalMethods.Index.MethodName;

		private int ParseBalanced(Span<object> parts, string delim)
		{
			var parenLevel = 0;
			var braceLevel = 0;
			var bracketLevel = 0;
			var ternaryLevel = 0;
			var i = 0;

			for (; i < parts.Length; i++)
			{
				if (parts[i] is string s)
				{
					if (s.EndsWith('('))//Either it's a ( or a function call which will end with a (.
					{
						parenLevel++;
					}
					else if (s == ")")
					{
						parenLevel--;
					}
					else if (s == "{")
					{
						braceLevel++;
					}
					else if (s == "}")
					{
						braceLevel--;
					}
					else if (s == "[")
					{
						bracketLevel++;
					}
					else if (s == "]")
					{
						bracketLevel--;
					}
					else if (s == "?")
					{
						ternaryLevel++;
					}
					else if (parenLevel == 0 && braceLevel == 0 && bracketLevel == 0 && ternaryLevel == 0 && (delim.Length == 0 || s == delim))
					{
						return i;
					}
					else if (ternaryLevel > 0 && s == ":")//Put this below the delimiter check because delim might be ":".
					{
						ternaryLevel--;//Only subtract if we're in a ternary, otherwise we're parsing an object.
					}
				}
			}

			return i;
		}

		private int ParseBalancedArray(Span<object> parts)
		{
			var bracketLevel = 0;
			var i = 0;

			for (; i < parts.Length; i++)
			{
				if (parts[i] is string s)
				{
					if (s == "[")
					{
						bracketLevel++;
					}
					else if (s == "]")
					{
						bracketLevel--;
					}

					if (bracketLevel == 0)
					{
						break;
					}
				}
			}

			return i - 1;
		}

		private void ParseObject(CodeLine codeLine, string code, List<object> parts, out CodeExpression[] kvs, bool create)
		{
			var names = new List<CodeExpression>();
			var entries = new List<CodeExpression>();
			var firstBrace = code.IndexOf('{') + 1;
			var lastBrace = code.LastIndexOf('}');
			var splits = SplitStringBalanced(code.Substring(firstBrace, lastBrace - firstBrace), ',');

			for (var i = 0; i < parts.Count; i++)
			{
				var hadResolve = false;
				CodeExpression value = null;

				if (!(parts[i] is string name))//Each token shouldn't be anything else other than a string.
					throw new ParseException($"{ExUnexpected}", codeLine);

				if (name.Length > 2)
				{
					if (name[0] == StringBound && name[name.Length - 1] == StringBound)
						name = name.Substring(1, name.Length - 2).ToLower();//If enclosed in quotes, just use as is, which will be a string in quotes.
					else if (name[0] == DefaultResolve)
						hadResolve = true;//If enclosed in percent signs, remove them and parse the expression.
				}

				if (hadResolve)//Remove percent signs and reparse.
				{
					var startPos = i;
					name = "";

					while (i < parts.Count)
					{
						var str = parts[i].ToString();

						if (str.Length > 0 && str[0] == AssignPre)
							break;

						if (str.StartsWith(DefaultResolve) || str.EndsWith(DefaultResolve))
							parts[i] = str = str.Trim(DefaultResolve);

						if (parts[i].ToString() == "")
						{
							parts.RemoveAt(i);
							i--;
						}

						if (str == "[*")
							name += '.';
						else if (str != "*]")
							name += str;

						i++;
					}

					//Need to reparse so that something like map.one can be properly interpreted.
					//This is because it would have been incorrectly parsed earlier because it was enclosed with %%.
					var tokens = SplitTokens(codeLine, name);
					_ = ExtractRange(parts, startPos, i);
					i = startPos - 1; //i++ below will reset back to zero.
					var expr = ParseExpression(codeLine, code, tokens, false);
					names.Add(expr);
				}
				else
					names.Add(new CodePrimitiveExpression(EscapedString(name.ToLower(), false)));//Add as a quoted lowercase string.

				i++;//Ensure the next token is a : char.

				if (i == parts.Count)
					goto collect;

				if (!(parts[i] is string assign))
					throw new ParseException(ExUnexpected, codeLine);

				if (assign.Length == 1 && assign[0] == Multicast)
					goto collect;

				if (!(assign.Length == 1 && (assign[0] == Equal || assign[0] == HotkeyBound)))//Should be an = or : char.
					throw new ParseException(ExUnexpected, codeLine);

				i++;

				if (i == parts.Count)
					goto collect;

				//Now get the value portion, which comes after the : char.
				var subs = new List<List<object>>();
				var span = CollectionsMarshal.AsSpan(parts);
				var tempparts = ParseObjectValue(span.Slice(i));

				if (splits.Count > 0)
				{
					var exprs = ParseMultiExpression(codeLine, splits[entries.Count], tempparts, create, subs);

					if (exprs.Length > 0)
					{
						value = exprs[0];
						i += subs[0].Count;
					}
				}

				if (i == parts.Count)
					goto collect;

				if (!(parts[i] is string delim))
					throw new ParseException(ExUnexpected, codeLine);

				if (!(delim.Length == 1 && delim[0] == Multicast))
					throw new ParseException(ExUnexpected, codeLine);

				collect:
				entries.Add(value ?? nullPrimitive);
			}

			//Interleave the two arrays.
			kvs = names.Zip(entries, (f, s) => new[] { f, s })
				  .SelectMany(f => f).ToArray();
		}

		private object[] ParseObjectValue(Span<object> parts)
		{
			var i = ParseBalanced(parts, ":");

			if (i < parts.Length)//If we hit the end, just return it.
			{
				i--;

				while (i > 0 && ((parts[i] as string) != ","))
					i--;
			}

			return parts.Slice(0, i).ToArray();
		}
	}
}