namespace System
{
	/// <summary>
	/// Extension methods for the System.String classes.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Provides an easy way to iterate through the lines of a string without using as much memory as string.Split().
		/// Taken from https://stackoverflow.com/questions/1547476/easiest-way-to-split-a-string-on-newlines-in-net from user Steve Cooper
		/// </summary>
		/// <param name="str">The string whose lines will be traversed</param>
		/// <returns>Each line one at a time as an element in an enumerable</returns>
		public static IEnumerable<string> SplitLines(this string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				yield break;
			}

			using (var reader = new StringReader(str))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					yield return line;
				}
			}
		}

		/// <summary>
		/// Trims a given number of characters from a list from the beginning of a string.
		/// </summary>
		/// <param name="str">The string to trim.</param>
		/// <param name="any">The list of characters to trim.</param>
		/// <param name="n">The number of characters to trim.</param>
		/// <returns>The trimmed string.</returns>
		public static string TrimNofAnyFromStart(this string str, string any, int n)
		{
			var i = 0;
			var chars = any.ToCharArray();

			for (; i < n && i < str.Length; i++)
			{
				var pos = str.IndexOfAny(chars, i);

				if (pos != i)
					break;
			}

			return i == 0 ? str : str.Substring(i);
		}

		/// <summary>
		/// Determines if all characters within a string are hexadecimal.
		/// </summary>
		/// <param name="str">The string to examine.</param>
		/// <returns>True if all characters were hexadecimal, else false.</returns>
		internal static bool AllHex(this string str)
		{
			foreach (var ch in str)
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
		/// Finds the first index in a string where a non-quoted imbalance of the two specified characters occurs.
		/// Gotten from https://stackoverflow.com/questions/4588695/algorithm-to-locate-unbalanced-parentheses-in-a-string
		/// </summary>
		/// <param name="str">The string to examine.</param>
		/// <param name="ch1">The left character to balance.</param>
		/// <param name="ch2">The right character to balance.</param>
		/// <returns>The index of the first imbalanced character, else -1 if no imbalance occurred.</returns>
		internal static int FindFirstImbalanced(this string str, char ch1, char ch2)
		{
			var escape = false;
			var inquote = false;
			var stack = new Stack<int>();

			for (var i = 0; i < str.Length; i++)
			{
				var ch = str[i];

				if (ch == '\'')
				{
					if (!inquote)
					{
						if (i == 0 || str[i - 1] != '`')
							inquote = true;
					}
				}
				else if (ch == '\"')
				{
					if (!inquote)
					{
						if (i == 0 || str[i - 1] != '`')
							inquote = true;
					}
					else
					{
						if (i == 0 || str[i - 1] != '`' || !escape)//Checking escape accounts for ``.
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

			while (stack.TryPop(out int temp))
			{
				ret = temp;
			}

			return ret;
		}

		/// <summary>
		/// Finds the index of the first occurrence of a string in another string
		/// which is not inside quotes.
		/// </summary>
		/// <param name="str">The string to examine.</param>
		/// <param name="s">The string to search for inside of str.</param>
		/// <returns>The index of the first occurrence of s inside of str outside of quotes, else -1 if not found.</returns>
		internal static int FindFirstNotInQuotes(this string str, string s)
		{
			var escape = false;
			var inquote = false;

			for (var i = 0; i < str.Length; i++)
			{
				var ch = str[i];

				if (ch == '\'')
				{
					if (!inquote)
					{
						if (i == 0 || str[i - 1] != '`')
							inquote = true;
					}
				}
				else if (ch == '\"')
				{
					if (!inquote)
					{
						if (i == 0 || str[i - 1] != '`')
							inquote = true;
					}
					else
					{
						if (i == 0 || str[i - 1] != '`' || !escape)//Checking escape accounts for ``.
							inquote = false;
					}
				}

				if (ch == Keywords.Escape)
					escape = !escape;
				else
					escape = false;

				if (!inquote)
				{
					if (str.Substring(i).StartsWith(s, StringComparison.OrdinalIgnoreCase))
					{
						return i;
					}
				}
			}

			return -1;
		}

		//internal static int FindFirstNotOf(this string source, string chars, int offset = 0)
		//{
		//  if (source.Length == 0) return -1;

		//  if (chars.Length == 0) return -1;

		//  for (var i = offset; i < source.Length; i++)
		//      if (chars.IndexOf(source[i]) == -1)
		//          return i;

		//  return -1;
		//}

		/// <summary>
		///
		/// </summary>
		/// <param name="str"></param>
		/// <param name="chars"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		internal static int FindFirstNotOf(this string str, char[] chars, int offset = 0) =>
		FindFirstNotOf(str.AsSpan(), chars, offset);

		/// <summary>
		///
		/// </summary>
		/// <param name="str"></param>
		/// <param name="chars"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		internal static int FindFirstNotOf(this ReadOnlySpan<char> str, char[] chars, int offset = 0)
		{
			if (str.Length == 0) return 0;// -1;

			if (chars.Length == 0) return 0;// -1;

			for (var i = offset; i < str.Length; i++)
				if (!chars.Contains(str[i]))
					return i;

			return str.Length;// -1;
		}

		/// <summary>
		/// Returns the remainder of the string, starting at the character which is not valid in an identifier (var, func, or obj.key name).
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		internal static int FindIdentifierEnd(this string str)
		{
			var i = 0;

			for (; i < str.Length; i++)
				if (!str[i].IsIdentifierChar())
					return i;

			return i;
		}

		//internal static int FirstIndexOf(this string source, Func<char, bool> func, int offset = 0) =>
		//source.AsSpan().FirstIndexOf(func, offset);

		internal static int FirstIndexOf(this ReadOnlySpan<char> str, Func<char, bool> func, int offset = 0)
		{
			for (var i = offset; i < str.Length; i++)
				if (func(str[i]))
					return i;

			return -1;
		}

		internal static int IndexOf(this ReadOnlySpan<char> str, char search, int offset)
		{
			var val = str.Slice(offset).IndexOf(search);

			if (val == -1)
				return val;

			return val + offset;
		}

		/// <summary>
		/// .NET IndexOfAny() with an offset for string but not span. So implement it here.
		/// </summary>
		/// <param name="str">The string to search</param>
		/// <param name="search">The characters to search for in the string</param>
		/// <param name="offset">The offset to begin searching at</param>
		/// <returns>The 0-based index of the first occurrence of any of the characters, else -1 if not found.</returns>
		internal static int IndexOfAny(this ReadOnlySpan<char> str, SearchValues<char> search, int offset)
		{
			var val = str.Slice(offset).IndexOfAny(search);

			if (val == -1)
				return val;

			return val + offset;
		}

		internal static bool IsBalanced(this string str, char ch1, char ch2)
		{
			int ct1 = 0, ct2 = 0;

			foreach (var ch in str)
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

		internal static bool OcurredInBalance(this string str, string s1, char ch1, char ch2)
		{
			var b = 0;
			var index = str.IndexOf(s1);

			if (index == -1)
				return false;

			for (int i = 0; i < str.Length; i++)
			{
				char ch = str[i];

				if (ch == ch1)
					b++;
				else if (ch == ch2)
					b--;

				if (i == index)
					return b == 0;
			}

			return false;
		}

		internal static string OmitTrailingWhitespace(this string str, int marker) => str.AsSpan(0, marker).TrimEnd(Keywords.SpaceTab).ToString();

		internal static string RemoveAfter(this string str, string token)
		{
			var index = str.IndexOf(token);
			return index > 0 ? str.Substring(0, index) : str;
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
		/// <param name="str"></param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <returns></returns>
		internal static string ReplaceFirst(this string str, string search, string replace, StringComparison comparison = StringComparison.Ordinal)
		{
			var pos = str.IndexOf(search, comparison);
			return pos < 0 ? str : str.Substring(0, pos) + replace + str.Substring(pos + search.Length);
		}

		internal static int StartsWithAnyOf(this ReadOnlySpan<char> str, IEnumerable<string> strings)
		{
			foreach (var start in strings)
				if (str.StartsWith(start, StringComparison.OrdinalIgnoreCase))
					return start.Length;

			return -1;
		}

		internal static ReadOnlySpan<char> TrimEndAlpha(this string str)
		{
			var len = str.Length;

			while (len > 0 && char.IsLetter(str[len - 1]))
				len--;

			return str.AsSpan(0, len);
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