using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace System//Extension methods should be in the same namespace of the objec they extend to make their use easy.
{
	public static class CharExtensions
	{
		public static bool IsHex(this char c)
		{
			return (c >= '0' && c <= '9') ||
				   (c >= 'a' && c <= 'f') ||
				   (c >= 'A' && c <= 'F');
		}
	}

	public static class StringExtensions
	{
		public static List<string> SplitWithDelimiter(this string aMatchList, char[] delims, bool append)
		{
			var match = new List<string>();

			if (!string.IsNullOrEmpty(aMatchList))
			{
				var sb = new StringBuilder(16);

				for (var i = 0; i < aMatchList.Length; i++)
				{
					if (!delims.Contains(aMatchList[i])) // Not a delim, so just copy it over.
					{
						sb.Append(aMatchList[i]);
						continue;
					}

					//Otherwise: it's a delim, which becomes the terminator of the previous key phrase unless
					//it's a double delim, in which case it's considered to be part of the previous phrase
					//rather than the next.
					if (i < aMatchList.Length - 1 && aMatchList[i + 1] == aMatchList[i]) // double delim
					{
						if (append)
						{
							sb.Append(aMatchList[i]);
						}
						else
						{
							match.Add(sb.ToString());
							match.Add(aMatchList[i].ToString());
							sb.Clear();
						}

						i++;  // Omit the second delim of the pair, i.e. each pair becomes a single literal delim.
						continue;
					}

					//Otherwise, this is a delimiting delim.
					if (sb.Length > 0)
					{
						match.Add(sb.ToString());
						sb.Clear();
					}
				}

				if (sb.Length > 0)
					match.Add(sb.ToString());
			}

			return match;
		}

		public static string OmitTrailingWhitespace(this string input, int marker)
		{
			for (; marker >= 0 && marker < input.Length && (input[marker] == ' ' || input[marker] == '\t'); --marker)
			{
			}

			return input.Substring(0, marker);
		}

		public static string RemoveAfter(this string input, string token)
		{
			var index = input.IndexOf(token);
			return index > 0 ? input.Substring(0, index) : input;
		}

		public static string RemoveAll(this string str, string chars)
		{
			var buffer = new char[str.Length];
			var idx = 0;

			foreach (var c in str)
				if (!chars.Contains(c))
					buffer[idx++] = c;

			return new string(buffer, 0, idx);
		}

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

		public static ReadOnlySpan<char> TrimEndAlpha(this string s)
		{
			var len = s.Length;

			while (len > 0 && char.IsLetter(s[len - 1]))
				len--;

			return s.AsSpan(0, len);
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

		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/141045/how-do-i-replace-the-first-instance-of-a-string-in-net
		/// </summary>
		/// <param name="text"></param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <returns></returns>
		internal static string ReplaceFirst(this string text, string search, string replace)
		{
			var pos = text.IndexOf(search);
			return pos < 0 ? text : text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}
	}
}