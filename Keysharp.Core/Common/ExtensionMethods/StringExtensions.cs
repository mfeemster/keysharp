namespace System//Extension methods should be in the same namespace of the object they extend to make their use easy.
{
	public static class CharExtensions
	{
		internal static bool IsHex(this char c)
		{
			return (c >= '0' && c <= '9') ||
				   (c >= 'a' && c <= 'f') ||
				   (c >= 'A' && c <= 'F');
		}

		internal static bool IsIdentifierChar(this char c) => (uint)c > 0x7F || char.IsAsciiLetterOrDigit(c) || c == '_';

		internal static bool IsLeadingIdentifierChar(this char c) => (uint)c > 0x7F || char.IsLetter(c) || c == '_';
	}

	public static class StringExtensions
	{
		/// <summary>
		/// Provide an easy way to iterate through the lines of a string without using as much memory as string.Split().
		/// Taken from https://stackoverflow.com/questions/1547476/easiest-way-to-split-a-string-on-newlines-in-net from user Steve Cooper
		/// </summary>
		/// <param name="input">The string whose lines will be traversed</param>
		/// <returns>Each line one at a time as an element in an enumerable</returns>
		public static IEnumerable<string> SplitLines(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				yield break;
			}

			using (var reader = new StringReader(input))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}

		public static string TrimNofAny(this string str, string any, int n)
		{
			var i = 0;
			var chars = any.ToCharArray();

			for (; i < n && i < str.Length; i++)
			{
				var pos = str.IndexOfAny(chars, i);

				if (pos != i)
					break;
			}

			return str.Substring(i);
		}

		internal static bool AllHex(this string source)
		{
			foreach (var ch in source)
				if (!ch.IsHex())
					return false;

			return true;
		}

#if LINUX
		/// <summary>
		/// Gotten from: https://stackoverflow.com/questions/54325155/how-to-get-hard-disk-serial-number-compatible-with-linux-windows
		/// </summary>
		/// <param name="cmd">The Bash command to run</param>
		/// <param name="wait">Whether to wait for the program to finish before returning. Default: true.</param>
		/// <returns>The output of the Bash command if wait is true, else empty string.</returns>
		internal static string Bash(this string cmd, bool wait = true)
		{
			var escapedArgs = cmd.Replace("\"", "\\\"");
			var process = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/bin/bash",
					Arguments = $"-c \"{escapedArgs}\"",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};
			_ = process.Start();

			if (wait)
			{
				string result = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				return result.Trim(Keywords.CrLf);
			}
			else
				return "";
		}
#endif

		internal static int EndsWithAnyOf(this ReadOnlySpan<char> str, IEnumerable<string> strings)
		{
			foreach (var end in strings)
				if (str.EndsWith(end, StringComparison.OrdinalIgnoreCase))
					return end.Length;

			return -1;
		}

		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/4588695/algorithm-to-locate-unbalanced-parentheses-in-a-string
		/// </summary>
		/// <param name="source"></param>
		/// <param name="ch1"></param>
		/// <param name="ch2"></param>
		/// <returns></returns>
		internal static int FindFirstImbalanced(this string source, char ch1, char ch2)
		{
			int temp;
			var stack = new Stack<int>();
			var escape = false;
			var inquote = false;

			for (var i = 0; i < source.Length; i++)
			{
				var ch = source[i];

				if (ch == '\'')
				{
					if (!inquote)
					{
						if (i == 0 || source[i - 1] != '`')
							inquote = true;
					}
				}
				else if (ch == '\"')
				{
					if (!inquote)
					{
						if (i == 0 || source[i - 1] != '`')
							inquote = true;
					}
					else
					{
						if (i == 0 || source[i - 1] != '`' || !escape)//Checking escape accounts for ``.
							inquote = false;
					}
				}

				if (ch == Keywords.Escape)
					escape = !escape;
				else
					escape = false;

				if (!inquote)
				{
					if (ch == ch1)
					{
						stack.Push(i);
					}
					else if (ch == ch2)
					{
						if (!stack.TryPop(out _))
						{
							return i;
						}
					}
				}
			}

			var ret = -1;

			while (stack.TryPop(out temp))
			{
				ret = temp;
			}

			return ret;
		}

		internal static int FindFirstNotInQuotes(this string source, string s)
		{
			var escape = false;
			var inquote = false;

			for (var i = 0; i < source.Length; i++)
			{
				var ch = source[i];

				if (ch == '\'')
				{
					if (!inquote)
					{
						if (i == 0 || source[i - 1] != '`')
							inquote = true;
					}
				}
				else if (ch == '\"')
				{
					if (!inquote)
					{
						if (i == 0 || source[i - 1] != '`')
							inquote = true;
					}
					else
					{
						if (i == 0 || source[i - 1] != '`' || !escape)//Checking escape accounts for ``.
							inquote = false;
					}
				}

				if (ch == Keywords.Escape)
					escape = !escape;
				else
					escape = false;

				if (!inquote)
				{
					if (source.Substring(i).StartsWith(s, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
			}

			return -1;
		}

		internal static int FindFirstNotOf(this string source, string chars, int offset = 0)
		{
			if (source.Length == 0) return -1;

			if (chars.Length == 0) return -1;

			for (var i = offset; i < source.Length; i++)
				if (chars.IndexOf(source[i]) == -1)
					return i;

			return -1;
		}

		internal static int FindFirstNotOf(this string source, char[] chars, int offset = 0)
		{
			if (source.Length == 0) return 0;// -1;

			if (chars.Length == 0) return 0;// -1;

			for (var i = offset; i < source.Length; i++)
				if (!chars.Contains(source[i]))
					return i;

			return source.Length;// -1;
		}

		/// <summary>
		/// Returns the remainder of the string, starting at the character which is not valid in an identifier (var, func, or obj.key name).
		/// </summary>
		/// <param name="buf"></param>
		/// <returns></returns>
		internal static int FindIdentifierEnd(this string buf)
		{
			var i = 0;

			for (; i < buf.Length; i++)
				if (!buf[i].IsIdentifierChar())
					return i;

			return i;
		}

		internal static int FirstIndexOf(this string source, Func<char, bool> func, int offset = 0) =>
		source.AsSpan().FirstIndexOf(func, offset);

		internal static int FirstIndexOf(this ReadOnlySpan<char> source, Func<char, bool> func, int offset = 0)
		{
			for (var i = offset; i < source.Length; i++)
				if (func(source[i]))
					return i;

			return -1;
		}

		internal static bool IsBalanced(this string source, char ch1, char ch2)
		{
			int ct1 = 0, ct2 = 0;

			foreach (var ch in source)
				if (ch == ch1)
					ct1++;
				else if (ch == ch2)
					ct2++;

			return ct1 == ct2;
		}

		/// <summary>
		/// Reverse vesion of NthIndexOf().
		/// </summary>
		/// <param name="str"></param>
		/// <param name="substr"></param>
		/// <param name="pos"></param>
		/// <param name="n"></param>
		/// <param name="comp"></param>
		/// <returns></returns>
		internal static int LastNthIndexOf(this string str, string substr, int pos, int n, StringComparison comp)
		{
			pos = str.Length + pos + 1;

			do
			{
				pos = str.LastIndexOf(substr, pos - 1, comp);
			} while (--n > 0 && pos != -1);

			return pos;
		}

		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/186653/get-the-index-of-the-nth-occurrence-of-a-string
		/// </summary>
		/// <param name="str"></param>
		/// <param name="substr"></param>
		/// <param name="n"></param>
		/// <param name="comp"></param>
		/// <returns></returns>
		internal static int NthIndexOf(this string str, string substr, int pos, int n, StringComparison comp)
		{
			pos--;

			do
			{
				pos = str.IndexOf(substr, pos + 1, comp);
			} while (--n > 0 && pos != -1);

			return pos;
		}

		internal static int NthIndexOfAny(this string str, char[] substr, int pos, int n)
		{
			pos--;

			do
			{
				pos = str.IndexOfAny(substr, pos + 1);
			} while (--n > 0 && pos != -1);

			return pos;
		}

		internal static bool OcurredInBalance(this string source, string s1, char ch1, char ch2)
		{
			var b = 0;
			var index = source.IndexOf(s1);

			if (index == -1)
				return false;

			for (int i = 0; i < source.Length; i++)
			{
				char ch = source[i];

				if (ch == ch1)
					b++;
				else if (ch == ch2)
					b--;

				if (i == index)
					return b == 0;
			}

			return false;
		}

		internal static string OmitTrailingWhitespace(this string input, int marker) => input.AsSpan(0, marker).TrimEnd(Keywords.SpaceTab).ToString();

		internal static string RemoveAfter(this string input, string token)
		{
			var index = input.IndexOf(token);
			return index > 0 ? input.Substring(0, index) : input;
		}

		internal static string RemoveAll(this string str, string chars)
		{
			var buffer = new char[str.Length];
			var idx = 0;

			foreach (var c in str)
				if (!chars.Contains(c))
					buffer[idx++] = c;

			return new string(buffer, 0, idx);
		}

		internal static string RemoveAll(this string str, char[] chars)
		{
			var buffer = new char[str.Length];
			var idx = 0;

			foreach (var c in str)
				if (!chars.Contains(c))
					buffer[idx++] = c;

			return new string(buffer, 0, idx);
		}

		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/141045/how-do-i-replace-the-first-instance-of-a-string-in-net
		/// </summary>
		/// <param name="text"></param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <returns></returns>
		internal static string ReplaceFirst(this string text, string search, string replace, StringComparison comparison = StringComparison.Ordinal)
		{
			var pos = text.IndexOf(search, comparison);
			return pos < 0 ? text : text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		internal static List<string> SplitWithDelimiter(this string aMatchList, char[] delims, bool append)
		{
			var match = new List<string>();

			if (!string.IsNullOrEmpty(aMatchList))
			{
				var sb = new StringBuilder(16);

				for (var i = 0; i < aMatchList.Length; i++)
				{
					if (!delims.Contains(aMatchList[i])) // Not a delim, so just copy it over.
					{
						_ = sb.Append(aMatchList[i]);
						continue;
					}

					//Otherwise: it's a delim, which becomes the terminator of the previous key phrase unless
					//it's a double delim, in which case it's considered to be part of the previous phrase
					//rather than the next.
					if (i < aMatchList.Length - 1 && aMatchList[i + 1] == aMatchList[i]) // double delim
					{
						if (append)
						{
							_ = sb.Append(aMatchList[i]);
						}
						else
						{
							match.Add(sb.ToString());
							match.Add(aMatchList[i].ToString());
							_ = sb.Clear();
						}

						i++;  // Omit the second delim of the pair, i.e. each pair becomes a single literal delim.
						continue;
					}

					//Otherwise, this is a delimiting delim.
					if (sb.Length > 0)
					{
						match.Add(sb.ToString());
						_ = sb.Clear();
					}
				}

				if (sb.Length > 0)
					match.Add(sb.ToString());
			}

			return match;
		}

		internal static int StartsWithAnyOf(this ReadOnlySpan<char> str, IEnumerable<string> strings)
		{
			foreach (var start in strings)
				if (str.StartsWith(start, StringComparison.OrdinalIgnoreCase))
					return start.Length;

			return -1;
		}

		internal static ReadOnlySpan<char> TrimEndAlpha(this string s)
		{
			var len = s.Length;

			while (len > 0 && char.IsLetter(s[len - 1]))
				len--;

			return s.AsSpan(0, len);
		}

		internal static string TrimEndOf(this string str, string trim, bool ignoreCase = true) =>
		str.EndsWith(trim, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
		? str.Substring(0, str.LastIndexOf(trim))
		: str;

		internal static string TrimStartOf(this string str, string trim, bool ignoreCase = true) =>
		str.StartsWith(trim, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)
		? str.Substring(trim.Length)
		: str;
	}
}