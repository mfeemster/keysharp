using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Keysharp.Core
{
	public static class Strings
	{
		internal static RegexEntry regdkt = new ();

		/// <summary>
		/// Decodes a base 64 character string to binary data.
		/// </summary>
		/// <param name="s">The base 64 string to decode.</param>
		/// <returns>A binary byte array of the given sequence.</returns>
		public static byte[] Base64Decode(string s) => Convert.FromBase64String(s);

		/// <summary>
		/// Encodes binary data to a base 64 character string.
		/// </summary>
		/// <param name="value">The data to encode.</param>
		/// <returns>A base 64 string representation of the given binary data.</returns>
		public static string Base64Encode(object value) => Convert.ToBase64String(Crypt.ToByteArray(value));

		/// <summary>
		/// Returns the single character corresponding to a Unicode value.
		/// </summary>
		/// <param name="n">A Unicode value.</param>
		/// <returns>A Unicode character whose value is <paramref name="n"/>.</returns>
		public static string Chr(object obj) => ((char)obj.Al()).ToString();

		/// <summary>
		/// Formats a string using the same syntax used by string.Format(), except it uses 1-based indexing.
		/// Traditional AHK formatting syntax is not supported.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Format(object obj0, params object[] obj) => string.Format(obj0.As(), new object[] { null }.Concat(obj));

		/// <summary>
		/// Transforms a YYYYMMDDHH24MISS timestamp into the specified date/time format.
		/// </summary>
		/// <param name="stamp">Leave this parameter blank to use the current local date and time.
		/// Otherwise, specify all or the leading part of a timestamp in the YYYYMMDDHH24MISS format.</param>
		/// <param name="format">
		/// <para>If omitted, it defaults to the time followed by the long date,
		/// both of which will be formatted according to the current user's locale.</para>
		/// <para>Otherwise, specify one or more of the date-time formats,
		/// along with any literal spaces and punctuation in between.</para>
		/// </param>
		/// <returns>The formatted date/time string</returns>
		public static string FormatTime(object obj0 = null, object obj1 = null)
		{
			var stamp = obj0.As();
			var format = obj1.As();
			DateTime time;
			var output = string.Empty;
			var splits = stamp.Split(' ');
			var ci = System.Globalization.CultureInfo.CurrentCulture;

			if (stamp?.Length == 0)
			{
				time = DateTime.Now;
			}
			else
			{
				stamp = splits[0];
				var haslsys = splits.Contains("LSys");

				if (haslsys)
					ci = new CultureInfo(ci.LCID, false);

				for (var i = 1; i < splits.Length; i++)
				{
					if (!haslsys && splits[i].StartsWith("L"))
					{
						var sub = splits[i].Substring(1);

						if (!int.TryParse(sub, out var li))
							li = Convert.ToInt32(sub, 16);

						ci = new System.Globalization.CultureInfo(li, false);
					}
					else if (splits[i].StartsWith("D"))
					{
						var sub = splits[i].Substring(1);

						if (!ulong.TryParse(sub, out var di))
							di = Convert.ToUInt64(sub, 16);

						if (di == 0x80000000)
							if (!haslsys)
								ci = new CultureInfo(ci.LCID, false);//No user overrides, if we haven't already done this above.
					}
					else if (splits[i].StartsWith("T"))
					{
						var sub = splits[i].Substring(1);

						if (!ulong.TryParse(sub, out var ti))
							ti = Convert.ToUInt64(sub, 16);

						if (ti == 0x80000000)
							if (!haslsys)
								ci = new CultureInfo(ci.LCID, false);//No user overrides, if we haven't already done this above.
					}
				}

				try
				{
					time = Conversions.ToDateTime(stamp, ci.Calendar);
				}
				catch
				{
					output = null;
					return output;
				}
			}

			if (format != string.Empty)
			{
				var fl = format.ToLowerInvariant();

				switch (fl)
				{
					case Core.Keyword_Time:
						format = "h:mm tt";
						break;

					case Core.Keyword_ShortDate:
						format = "d";
						break;

					case Core.Keyword_LongDate:
						format = "D";
						break;

					case Core.Keyword_YearMonth:
						format = "Y";
						break;

					case Core.Keyword_YDay:
						output = ci.Calendar.GetDayOfYear(time).ToString();
						return output;

					case Core.Keyword_YDay0:
						output = ci.Calendar.GetDayOfYear(time).ToString().PadLeft(3, '0');
						return output;

					case Core.Keyword_WDay:
						output = ((int)ci.Calendar.GetDayOfWeek(time) + 1).ToString();
						return output;

					case Core.Keyword_YWeek:
					{
						var week = ci.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, ci.DateTimeFormat.FirstDayOfWeek);
						output = ci.Calendar.GetYear(time).ToString() + week;
						return output;
					}

					default:
						if (format.Contains('\''))
						{
							format = format.Replace("'", "\"");
							format = format.Replace("\"\"\"\"", "\\'");
						}

						break;
				}

				if (fl.Length == 1)
					format = "%" + format;
			}
			else
			{
				if (splits.Contains("R"))
					format = "f";
				else
					format = "h:mm tt dddd, MMMM d, yyyy";
			}

			try
			{
				output = time.ToString(format, ci);
			}
			catch
			{
				output = null;
			}

			return output;
		}

		//Just pass the whole array again as the format, because the first position is ignored because we use 1-based indexing. //Need to add an element in the front because the format string is 1 indexed.
		/// <summary>
		/// Decodes a hexadecimal string to binary data.
		/// </summary>
		/// <param name="hex">The hexadecimal string to decode.</param>
		/// <returns>A binary byte array of the given sequence.</returns>
		public static byte[] HexDecode(string hex)
		{
			var binary = new byte[hex.Length / 2];

			for (var i = 0; i < hex.Length; i += 2)
			{
				var n = new string(new[] { hex[i], hex[i + 1] });
				binary[i / 2] = byte.Parse(n, NumberStyles.AllowHexSpecifier);
			}

			return binary;
		}

		/// <summary>
		/// Encodes binary data to a hexadecimal string.
		/// </summary>
		/// <param name="value">The data to encode.</param>
		/// <returns>A hexadecimal string representation of the given binary data.</returns>
		public static string HexEncode(object value) => BytesToHexString(Crypt.ToByteArray(value));

		/// <summary>
		/// Returns the position of the first or last occurrence of the specified substring within a string.
		/// </summary>
		/// <param name="input">The string to check.</param>
		/// <param name="needle">The substring to search for.</param>
		/// <param name="caseSensitive"><c>true</c> to use a case sensitive comparison, <c>false</c> otherwise.</param>
		/// <param name="index">The one-based starting character position, default 1, negative for reverse order.</param>
		/// <param name="occurrence">The one-based nth occurrence to find.</param>
		/// <returns>The one-based index of the position of <paramref name="needle"/> in <paramref name="input"/>.
		/// A value of zero indicates no match.</returns>
		public static long InStr(object obj0, object obj1, object obj2 = null, object obj3 = null, object obj4 = null)
		{
			var input = obj0.As();
			var needle = obj1.As();
			var comp = obj2.As();
			var index = (int)obj3.Al(1);
			var occurrence = (int)obj4.Al(1);

			if (input != "")
			{
				if (string.IsNullOrEmpty(needle))
					throw new Exception("Search string was empty");

				var cs = comp != "" ? Keysharp.Core.Conversions.ParseComparisonOption(comp) : StringComparison.OrdinalIgnoreCase;
				const int offset = 1;//Everything is 1-based indexing.
				return index < 0
					   ? offset + input.LastNthIndexOf(needle, index, occurrence, cs)
					   : index == 0 || index > input.Length ? 0 :
					   offset + input.NthIndexOf(needle, index - 1, occurrence, cs);
			}

			return 0L;
		}

		public static string Join(params object[] obj)
		{
			if (obj.Length > 1)
			{
				var s1 = obj[0].Str();
				var vals = obj.Skip(1);
				return string.Join(s1, vals);
			}

			return "";
		}

		public static string LTrim(object obj0, object obj1 = null) => obj0.As().TrimStart(obj1.As(" \t").ToCharArray());

		public static string NormalizeEol(object obj0, object obj1 = null)
		{
			const string CR = "\r", LF = "\n", CRLF = "\r\n";
			var text = obj0.As();
			var eol = obj1.As(Environment.NewLine);

			switch (eol)
			{
				case CR:
					return text.Replace(CRLF, CR).Replace(LF, CR);

				case LF:
					return text.Replace(CRLF, LF).Replace(CR, LF);

				case CRLF:
					return text.Replace(CR, string.Empty).Replace(LF, CRLF);
			}

			return text;
		}

		/// <summary>
		/// Returns the ordinal value (numeric character code) of the first character in the specified string.
		/// </summary>
		/// <param name="str">A string.</param>
		/// <returns>The Unicode value.
		/// If <paramref name="str"/> is empty, <c>0</c> is returned.</returns>
		public static long Ord(object obj)
		{
			var str = obj.As();
			return string.IsNullOrEmpty(str) ? 0L : (long)str[0];
		}

		/// <summary>
		/// Determines whether a string contains a pattern (regular expression).
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <param name="needle">The pattern to search for, which is a regular expression.</param>
		/// <param name="index">The one-based starting character position.
		/// If this is less than one it is considered an offset from the end of the string.</param>
		/// <returns>The RegExResults object which contains the matches, if any.</returns>
		public static RegExResults RegExMatch(object obj0, object obj1, object obj2 = null)
		{
			var input = obj0.As();
			var needle = obj1.As();
			var index = (int)obj2.Al(1);
			var reverse = index < 1;
			var str = needle + reverse;
			RegexWithTag exp = null;

			lock (regdkt)//KeyedCollection is not threadsafe, the way ConcurrentDictionary is, so we must lock. We use KC because we need to preserve order to remove the first entry.
			{
				if (!regdkt.TryGetValue(str, out exp))
				{
					try
					{
						exp = Conversions.ParseRegEx(needle, reverse);//This will not throw PCRE style errors like the documentation says.//MATT
					}
					catch (Exception ex)
					{
						throw new Error("Regular expression compile error", "", ex.Message);
					}

					exp.tag = str;
					regdkt.Add(exp);

					while (regdkt.Count > 100)
						regdkt.RemoveAt(0);
				}
			}

			if (index < 0)
			{
				index = input.Length + index + 1;

				if (index < 0)
					index = input.Length;
			}
			else
				index = Math.Min(Math.Max(0, index - 1), input.Length);

			try
			{
				return new RegExResults(exp.Match(input, index));
			}
			catch (Exception ex)
			{
				throw new Error("Regular expression execution error", "", ex.Message);
			}
		}

		/// <summary>
		/// Replaces occurrences of a pattern (regular expression) inside a string.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <param name="needle">The pattern to search for, which is a regular expression.</param>
		/// <param name="replace">The string to replace <paramref name="needle"/>.</param>
		/// <param name="count">The variable to store the number of replacements that occurred.</param>
		/// <param name="limit">The maximum number of replacements to perform.
		/// If this is below one all matches will be replaced.</param>
		/// <param name="index">The one-based starting character position.
		/// If this is less than one it is considered an offset from the end of the string.</param>
		/// <returns>The result object which contains a string and the number of replacements</returns>
		public static ReplaceResults RegExReplace(object obj0, object obj1, object obj2 = null, object obj3 = null, object obj4 = null)
		{
			var input = obj0.As();
			var needle = obj1.As();
			var replace = obj2.As();
			var limit = (int)obj3.Al(-1);
			var index = (int)obj4.Al(1);
			var n = 0;
			var reverse = index < 1;
			var str = needle + reverse;
			RegexWithTag exp = null;

			lock (regdkt)//KeyedCollection is not threadsafe, the way ConcurrentDictionary is, so we must lock. We use KeyedCollection because we need to preserve order to remove the first entry.
			{
				if (!regdkt.TryGetValue(str, out exp))
				{
					try
					{
						exp = Conversions.ParseRegEx(needle, reverse);
					}
					catch (ArgumentException ex)
					{
						throw new Error("Regular expression compile error", "", ex.Message);
					}

					exp.tag = str;
					regdkt.Add(exp);

					while (regdkt.Count > 100)
						regdkt.RemoveAt(0);
				}
			}

			if (limit < 1)
				limit = int.MaxValue;

			if (index < 0)
			{
				index = input.Length + index + 1;

				if (index < 0)
					index = input.Length;
			}
			else
				index = Math.Min(Math.Max(0, index - 1), input.Length);

			string match(Match hit)
			{
				n++;
				return hit.Result(replace);
			}

			try
			{
				var result = exp.Replace(input, match, limit, index);
				return new ReplaceResults(result, n);
			}
			catch (Exception ex)
			{
				throw new Error("Regular expression execution error", "", ex.Message);
			}
		}

		public static string RTrim(object obj0, object obj1 = null) => obj0.As().TrimEnd(obj1.As(" \t").ToCharArray());

		/// <summary>
		/// Arranges a variable's contents in alphabetical, numerical, or random order optionally removing duplicates.
		/// </summary>
		/// <param name="input">The variable whose contents to use as the input.</param>
		/// <param name="options">See the remarks.</param>
		/// <remarks>
		/// <list type="table">
		/// <listheader>
		/// <term>Name</term>
		/// <description>Description</description>
		/// </listheader>
		/// <item>
		/// <term>C</term>
		/// <description>Case sensitive.</description>
		/// </item>
		/// <item>
		/// <term>CL</term>
		/// <description>Case sensitive based on current user's locale.</description>
		/// </item>
		/// <item>
		/// <term>D<c>x</c></term>
		/// <description>Specifies <c>x</c> as the delimiter character which is <c>`n</c> by default.</description>
		/// </item>
		/// <item>
		/// <term>F <c>name</c></term>
		/// <description>Use the return value of the specified function for comparing two items.</description>
		/// </item>
		/// <item>
		/// <term>N</term>
		/// <description>Numeric sorting.</description>
		/// </item>
		/// <item>
		/// <term>P<c>n</c></term>
		/// <description>Sorts items based on character position <c>n</c>.</description>
		/// </item>
		/// <item>
		/// <term>R</term>
		/// <description>Sort in reverse order.</description>
		/// </item>
		/// <item>
		/// <term>Random</term>
		/// <description>Sort in random order.</description>
		/// </item>
		/// <item>
		/// <term>U</term>
		/// <description>Remove any duplicate items.</description>
		/// </item>
		/// <item>
		/// <term>Z</term>
		/// <description>Considers a trailing delimiter as a boundary which otherwise would be ignored.</description>
		/// </item>
		/// <item>
		/// <term>\</term>
		/// <description>File path sorting.</description>
		/// </item>
		/// </list>
		/// </remarks>
		public static string Sort(object obj0, object obj1 = null, object obj2 = null)
		{
			var input = obj0.As();
			var options = obj1.As();
			var splits = options.Split(" ");
			var opts = Options.KeyValues(string.Join(",", splits), true, new[] { 'f' });
			IFuncObj function = null;
			var split = '\n';
			var dopt = splits.FirstOrDefault(s => s.StartsWith("d", StringComparison.OrdinalIgnoreCase)) ?? "";

			if (obj2 is string strfunc)
			{
				function = new FuncObj(strfunc);

				if (function.Name == null)
					return "";
			}
			else if (obj2 is IFuncObj fo)
				function = fo;

			if (!string.IsNullOrEmpty(dopt))
			{
				var ddelim = dopt.Substring(1);

				if (!string.IsNullOrEmpty(ddelim))
					split = ddelim[0];
			}

			var zopt = !string.IsNullOrEmpty(splits.FirstOrDefault(s => s.Equals("z", StringComparison.OrdinalIgnoreCase)) ?? "");
			var list = input.Split(new[] { split }, zopt ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);

			if (split == '\n')
			{
				for (var i = 0; i < list.Length; i++)
				{
					var x = list[i].Length - 1;

					if (x >= 0 && list[i][x] == '\r')
						list[i] = list[i].Substring(0, x);
				}
			}

			var withcase = false;
			var withlocale = false;
			var copt = splits.FirstOrDefault(s => s.StartsWith("c", StringComparison.OrdinalIgnoreCase)) ?? "";

			if (!string.IsNullOrEmpty(copt))
			{
				var cdelim = copt.Substring(1);

				if (!string.IsNullOrEmpty(cdelim) && cdelim.Equals("l", StringComparison.OrdinalIgnoreCase))
					withlocale = true;
				else
					withcase = true;
			}

			var numeric = !string.IsNullOrEmpty(splits.FirstOrDefault(s => s.StartsWith("n", StringComparison.OrdinalIgnoreCase)) ?? "");
			var sortAt = 1;
			var popt = splits.FirstOrDefault(s => s.StartsWith("p", StringComparison.OrdinalIgnoreCase)) ?? "";

			if (!string.IsNullOrEmpty(popt))
			{
				var pdelim = popt.Substring(1);

				if (!string.IsNullOrEmpty(pdelim) && int.TryParse(pdelim, out var pp))
					sortAt = pp;
			}

			sortAt = Math.Max(0, sortAt - 1);
			var reverse = !string.IsNullOrEmpty(splits.FirstOrDefault(s => s.Equals("r", StringComparison.OrdinalIgnoreCase)) ?? "");
			var random = !string.IsNullOrEmpty(splits.FirstOrDefault(s => s.Equals(Core.Keyword_Random, StringComparison.OrdinalIgnoreCase)) ?? "");
			var unique = !string.IsNullOrEmpty(splits.FirstOrDefault(s => s.Equals("u", StringComparison.OrdinalIgnoreCase)) ?? "");
			var slashopt = splits.FirstOrDefault(s => s.Equals("\\", StringComparison.OrdinalIgnoreCase) || s.Equals("/", StringComparison.OrdinalIgnoreCase)) ?? "";
			var slash = false;
			var slashtype = '\\';

			if (!string.IsNullOrEmpty(slashopt))
			{
				slash = true;
				slashtype = slashopt[0];
			}

			var compl = withlocale ? StringComparison.CurrentCultureIgnoreCase : StringComparison.OrdinalIgnoreCase;
			var comp = withcase ? StringComparison.Ordinal : compl;

			if (random)
			{
				var rand = new Random();
				list = list.OrderBy(x => rand.Next()).ToArray();
			}
			else if (function != null)
			{
				var index = 0L;
				var indexedlist = new ValueTuple<string, long>[list.Length];//Temporary needed to include the indices for the third argument.

				foreach (var val in list)
					indexedlist[index] = (val, index++);

				var args = new object[3];//Cache to avoid allocations inside of the sort function.
				System.Array.Sort(indexedlist, delegate (ValueTuple<string, long> x, ValueTuple<string, long> y)
				{
					object value = null;
					args[0] = x.Item1;
					args[1] = y.Item1;
					args[2] = y.Item2 - x.Item2;

					try { value = function.Call(args); }
					catch (Exception) { }

					if (value is int i)
						return i;
					else if (value is long l)
						return (int)l;
					else
						return 0;
				});
				index = 0;

				foreach (var val in indexedlist)
					list[index++] = val.Item1;
			}
			else
			{
				System.Array.Sort(list, (x, y) =>
				{
					ReadOnlySpan<char> xs, ys;

					if (withlocale)
					{
						xs = sortAt == 0 || slash ? x.RemoveAll("'-").AsSpan() : sortAt < x.Length ? x.RemoveAll("'-").AsSpan(sortAt) : string.Empty.AsSpan();
						ys = sortAt == 0 || slash ? y.RemoveAll("'-").AsSpan() : sortAt < y.Length ? y.RemoveAll("'-").AsSpan(sortAt) : string.Empty.AsSpan();
					}
					else
					{
						xs = sortAt == 0 || slash ? x.AsSpan() : sortAt < x.Length ? x.AsSpan(sortAt) : string.Empty.AsSpan();
						ys = sortAt == 0 || slash ? y.AsSpan() : sortAt < y.Length ? y.AsSpan(sortAt) : string.Empty.AsSpan();
					}

					if (xs == ys)
					{
						return 0;
					}
					else if (numeric && !slash)
					{
						return double.TryParse(xs, out var a) && double.TryParse(ys, out var b) ?
							   a.CompareTo(b) : xs.CompareTo(ys, compl);
					}
					else
					{
						if (slash)
						{
							var z = xs.LastIndexOf(slashtype);

							if (z != -1)
								xs = xs.Slice(z + 1);

							z = ys.LastIndexOf(slashtype);

							if (z != -1)
								ys = ys.Slice(z + 1);

							if (xs == ys)
								return 0;
						}

						return xs.CompareTo(ys, comp);
					}
				});
			}

			if (unique)
			{
				var ulist = new List<string>(list.Length);

				for (var i = 0; i < list.Length; i++)
				{
					var item = list[i];

					if (numeric)
					{
						if (double.TryParse(item, out var dd))
							if (!list.Any(val => double.TryParse(val, out var d) && d == dd))
								ulist.Add(item);
					}
					else if (random || function != null)
					{
						if (i == 0 || !item.Equals(list[i - 1], comp))
							ulist.Add(item);
					}
					else if (!ulist.Any(val => item.Equals(val, comp)))
						ulist.Add(item);
				}

				list = ulist.ToArray();
			}

			if (reverse && function == null)
				System.Array.Reverse(list);

			return string.Join(split.ToString(), list);
		}

		public static long StrCompare(object obj0, object obj1, object obj2 = null)
		{
			var s1 = obj0.As();
			var s2 = obj1.As();
			var s3 = obj2.As();

			if (s1 != "" || s2 != "")
			{
				var cs = StringComparison.OrdinalIgnoreCase;

				if (s3 != "")
				{
					if (string.Compare(s3, "logical", true) == 0)
						return NaturalComparer.NaturalCompare(s1, s2);

					cs = Keysharp.Core.Conversions.ParseComparisonOption(s3);
				}

				return string.Compare(s1, s2, cs);
			}

			return 0L;
		}

		public static string StrGet(object obj0, object obj1 = null, object obj2 = null)
		{
			var len = obj1.Al(long.MinValue);// buf != null ? Math.Min((long)buf.Size, Convert.ToInt32(o[1])) : Convert.ToInt32(o[1]);
			var encoding = obj2 is string s ? File.GetEncoding(s) : Encoding.Unicode;
			var ptr = IntPtr.Zero;
			var buf = obj0 as Buffer;

			if (buf != null)
				ptr = buf.Ptr;
			else if (obj0 is long l)
				ptr = new IntPtr(l);

			if (ptr == IntPtr.Zero)
				throw new ValueError($"No valid address or buffer was supplied.");
			else if (ptr.ToInt64() < 65536)//65536 is the first valid address.
				throw new ValueError($"Address of {ptr.ToInt64()} is less than the minimum allowable address of 65,536.");

			unsafe
			{
				var finalLen = 0;
				var raw = (byte*)ptr.ToPointer();
				byte[] bytes = null;

				if (len == long.MinValue)//No length specified, only copy up to the first 0.
				{
					while (raw[finalLen] != 0)
						finalLen++;
				}
				else if (len < 0)//Length is negative, copy exactly the absolute value of len, regardless of 0s. Clamp to buf size if buf.
				{
					var abs = Math.Abs(len);

					if (encoding != Encoding.ASCII)//Sort of crude, UTF-8 can require up to 4 bytes per char.
						abs *= 2;

					finalLen = (int)(buf != null ? Math.Min((long)buf.Size, abs) : abs);
				}
				else//Positive length was passed, copy as long as length is not reached and value is not 0.
				{
					if (encoding != Encoding.ASCII)
						len *= 2;

					if (buf != null)
						len = (int)Math.Min((long)buf.Size, len);

					while (raw[finalLen] != 0 && finalLen < len)
						finalLen++;
				}

				bytes = new byte[finalLen];

				for (var i = 0; i < finalLen; i++)
					bytes[i] = raw[i];

				return encoding.GetString(bytes);
			}
			//return len > 0 ? Marshal.PtrToStringAuto(ptr, (int)len) : Marshal.PtrToStringAuto(ptr);
		}

		public static string String(object obj) => obj.As();

		/// <summary>
		/// Returns the length of a string.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The total length of the string, including any invisbile characters such as null.</returns>
		public static long StrLen(object obj) => obj.As().Length;

		/// <summary>
		/// Converts a string to lowercase.
		/// </summary>
		/// <param name="input">The string to convert to lower</param>
		/// <param name="title"><c>true</c> to use title casing, <c>false</c> otherwise.</param>
		/// <returns>The converted string.</returns>
		public static string StrLower(object obj) => obj.As().ToLowerInvariant();

		public static long StrPtr(object obj) => throw new Error("Cannot take the address of a string in C#, so just use the string as is.");

		public static long StrPut(params object[] obj)//Leave this as variadic because the parameter scheme is complex.
		{
			var o = obj.L();

			if (o.Count > 0 && o[0] != null)
			{
				var s = o.As(0);
				var len = long.MinValue;
				var encoding = Encoding.Unicode;
				var ptr = IntPtr.Zero;
				Buffer buf = null;

				if (o.Count > 1)
				{
					buf = o[1] as Buffer;

					if (buf != null)
						ptr = buf.Ptr;
					else if (o[1] is long l)
						ptr = new IntPtr(l);
				}

				if (ptr != IntPtr.Zero && ptr.ToInt64() < 65536)//65536 is the first valid address.
					throw new ValueError($"Address of {ptr.ToInt64()} is less than the minimum allowable address of 65,536.");

				if (o.Count > 2 && !o[2].IsNullOrEmpty())
					len = Math.Abs(o.Al(2));

				if (o.Count > 3)
					encoding = File.GetEncoding(o[3]);

				var bytes = encoding.GetBytes(s);

				if (buf != null)
				{
					var written = (int)Math.Min((long)buf.Size, bytes.Length);
					Marshal.Copy(bytes, 0, ptr, written);
					return written;
				}
				else
				{
					if (len != long.MinValue)
					{
						if (len < s.Length || len < bytes.Length)
							throw new ValueError($"Length of {len} is less than the either the length of the string {s.Length} or the length of the converted buffer {bytes.Length}.");
					}
					else if (ptr == IntPtr.Zero)
						return bytes.Length;
					else if (len == long.MinValue)
						throw new ValueError($"Length was not specified, but the target was not a Buffer object. Either pass a Buffer, or specify a Length.");

					Marshal.Copy(bytes, 0, ptr, Math.Min((int)len, bytes.Length));
				}
			}

			return 0L;
		}

		/// <summary>
		/// Replaces the specified substring with a new string.
		/// </summary>
		/// <param name="input">The variable whose contents to use as the input.</param>
		/// <param name="search">The substring to search for.</param>
		/// <param name="replace">The string to replace <paramref name="search"/>.</param>
		/// <param name="outvarcount">The variable name to store the number of replacements in, if not empty.</param>
		/// <param name="limit">The maximum number of replacements to make.</param>
		/// <returns>The modified string.</returns>
		public static ReplaceResults StrReplace(object obj0, object obj1, object obj2 = null, object obj3 = null, object obj4 = null)
		{
			var input = obj0.As();
			var search = obj1.As();
			var replace = obj2.As();
			var comp = obj3.As("Off");
			var limit = obj4.Al(-1);

			if (Options.IsAnyBlank(input, search))
				return new ReplaceResults("", 0L);

			var compare = Keysharp.Core.Conversions.ParseComparisonOption(comp);
			var ct = 0L;
			var buf = new StringBuilder(input.Length);
			int z = 0, n = 0, l = search.Length;

			while (z < input.Length &&
					(z = input.IndexOf(search, z, compare)) != -1 &&
					(limit < 0 || ct < limit))
			{
				if (n < z)
					_ = buf.Append(input, n, z - n);

				_ = buf.Append(replace);
				z += l;
				n = z;
				ct++;
			}

			if (n < input.Length)
				_ = buf.Append(input, n, input.Length - n);

			return new ReplaceResults(buf.ToString(), ct);
		}

		/// <summary>
		/// Separates a string into an array of substrings using the specified delimiters.
		/// </summary>
		/// <param name="output">The variable to store the result.</param>
		/// <param name="input">The variable whose contents to use as the input.</param>
		/// <param name="delimiters">One or more characters (case sensitive), each of which is used to determine
		/// where the boundaries between substrings occur in <paramref name="input"/>.
		/// If this is blank each character of <paramref name="input"/> will be treated as a substring.</param>
		/// <param name="trim">An optional list of characters (case sensitive) to exclude from the beginning and end of each array element.</param>
		public static Array StrSplit(object obj0, object obj1 = null, object obj2 = null, object obj3 = null)
		{
			string delimiters = string.Empty, trim = string.Empty;

			if (obj0 is string input)
			{
				var count = (int)obj3.Al(-1L);

				if (obj1 is string d)
					delimiters = d;
				else if (obj1 is IList il)
					foreach (var id in il.Flatten())
						delimiters += id.ToString();

				if (obj2 is string t)
					trim = t;
				else if (obj2 is IList il)
					foreach (var id in il.Flatten())
						trim += id.ToString();

				if (delimiters.Length == 0)
				{
					var list = new List<string>(input.Length);

					if (count > 0)
					{
						int i = 0, ct = 0;

						for (; ct < count - 1 && i < input.Length; i++)
						{
							var ch = input[i];

							if (trim.IndexOf(ch) == -1)
							{
								list.Add(ch.ToString());
								ct++;
							}
						}

						if (ct < input.Length && i < input.Length)
						{
							list.Add(input.Substring(i));
						}
					}
					else
					{
						foreach (var letter in input)
							if (trim.IndexOf(letter) == -1)
								list.Add(letter.ToString());
					}

					return new Array(list.Cast<object>().ToArray());
				}

				var cha = delimiters.ToCharArray();
				var output = count > 0 ? input.Split(cha, count, StringSplitOptions.None) : input.Split(cha, StringSplitOptions.None);

				if (trim.Length != 0)
				{
					var omit = trim.ToCharArray();

					for (var i = 0; i < output.Length; i++)
						output[i] = output[i].Trim(omit);
				}

				return new Array(output.Cast<object>().ToArray());
			}

			return new Array();
		}

		public static string StrTitle(object obj) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(obj.As());

		/// <summary>
		/// Converts a string to uppercase.
		/// </summary>
		/// <param name="input">The string to convert to upper</param>
		/// <param name="title"><c>true</c> to use title casing, <c>false</c> otherwise.</param>
		/// <returns>The converted string.</returns>
		public static string StrUpper(object obj) => obj.As().ToUpperInvariant();

		/// <summary>
		/// Retrieves one or more characters from the specified position in a string.
		/// </summary>
		/// <param name="input">The string to use.</param>
		/// <param name="index">The one-based starting character position.
		/// If this is less than one it is considered an offset from the end of the string.</param>
		/// <param name="length">The maximum number of characters to retrieve.
		/// Leave this parameter blank to return the entire leading part of the string.
		/// Specify a negative value to omit that many characters from the end of the string.</param>
		/// <returns>The new substring.</returns>
		public static string SubStr(object obj0, object obj1 = null, object obj2 = null)
		{
			var input = obj0.As();
			var index = (int)obj1.Al(1L);
			var length = obj2.Al(long.MaxValue);

			if (string.IsNullOrEmpty(input) || length == 0 || index == 0 || index > input.Length)
				return string.Empty;

			if (index < 1)
			{
				index += input.Length;

				if (index < 0)
					index = 0;
			}
			else
				index--;

			var d = input.Length - index;

			if (index < 0 || index >= input.Length)
				return string.Empty;

			if (length < 0)
				length += d;

			length = Math.Max(0, Math.Min(length, d));
			return input.Substring((int)index, (int)length);
		}

		/// <summary>
		/// Performs miscellaneous math functions, bitwise operations, and tasks such as ASCII to Unicode conversion.
		/// This function is obsolete, please use the related newer syntax.
		/// <seealso cref="Asc"/>
		/// <seealso cref="Chr"/>
		/// <seealso cref="Mod"/>
		/// <seealso cref="Exp"/>
		/// <seealso cref="Sqrt"/>
		/// <seealso cref="Log"/>
		/// <seealso cref="Ln"/>
		/// <seealso cref="Round"/>
		/// <seealso cref="Ceil"/>
		/// <seealso cref="Floor"/>
		/// <seealso cref="Abs"/>
		/// <seealso cref="Sin"/>
		/// <seealso cref="Cos"/>
		/// <seealso cref="Tan"/>
		/// <seealso cref="ASin"/>
		/// <seealso cref="ACos"/>
		/// <seealso cref="ATan"/>
		/// <seealso cref="Floor"/>
		/// <seealso cref="Floor"/>
		/// </summary>
		[Obsolete]
		public static void Transform(ref string OutputVar, string Cmd, string Value1, string Value2)
		{
			OutputVar = string.Empty;

			switch (Cmd.Trim().ToLowerInvariant())
			{
				case Core.Keyword_Unicode:
					OutputVar = Value1 ?? Clipboard.GetText();
					break;

				case Core.Keyword_Asc:
					OutputVar = char.GetNumericValue(Value1, 0).ToString();
					break;

				case Core.Keyword_Chr:
					OutputVar = char.ConvertFromUtf32(int.Parse(Value1));
					break;

				case Core.Keyword_Deref:
					// TODO: dereference transform
					break;

				case "html":
					OutputVar = Value1
								.Replace("\"", "&quot;")
								.Replace("&", "&amp;")
								.Replace("<", "&lt;")
								.Replace(">", "&gt;")
								.Replace("\n", "<br/>\n");
					break;

				case Core.Keyword_Mod:
					OutputVar = (double.Parse(Value1) % double.Parse(Value2)).ToString();
					break;

				case Core.Keyword_Pow:
					OutputVar = Math.Pow(double.Parse(Value1), double.Parse(Value2)).ToString();
					break;

				case Core.Keyword_Exp:
					OutputVar = Math.Pow(double.Parse(Value1), Math.E).ToString();
					break;

				case Core.Keyword_Sqrt:
					OutputVar = Math.Sqrt(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Log:
					OutputVar = Math.Log10(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Ln:
					OutputVar = Math.Log(double.Parse(Value1), Math.E).ToString();
					break;

				case Core.Keyword_Round:
					var p = int.Parse(Value2);
					OutputVar = Math.Round(double.Parse(Value1), p == 0 ? 1 : p).ToString();
					break;

				case Core.Keyword_Ceil:
					OutputVar = Math.Ceiling(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Floor:
					OutputVar = Math.Floor(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Abs:
					var d = double.Parse(Value1);
					OutputVar = (d < 0 ? d * -1 : d).ToString();
					break;

				case Core.Keyword_Sin:
					OutputVar = Math.Sin(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Cos:
					OutputVar = Math.Cos(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Tan:
					OutputVar = Math.Tan(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Asin:
					OutputVar = Math.Asin(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Acos:
					OutputVar = Math.Acos(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_Atan:
					OutputVar = Math.Atan(double.Parse(Value1)).ToString();
					break;

				case Core.Keyword_BitNot:
					OutputVar = (~int.Parse(Value1)).ToString();
					break;

				case Core.Keyword_BitAnd:
					OutputVar = (int.Parse(Value1) & int.Parse(Value2)).ToString();
					break;

				case Core.Keyword_BitOr:
					OutputVar = (int.Parse(Value1) | int.Parse(Value2)).ToString();
					break;

				case Core.Keyword_BitXor:
					OutputVar = (int.Parse(Value1) ^ int.Parse(Value2)).ToString();
					break;

				case Core.Keyword_BitShiftLeft:
					OutputVar = (int.Parse(Value1) << int.Parse(Value2)).ToString();
					break;

				case Core.Keyword_BitShiftRight:
					OutputVar = (int.Parse(Value1) >> int.Parse(Value2)).ToString();
					break;
			}
		}

		public static string Trim(object obj0, object obj1 = null) => obj0.As().Trim(obj1.As(" \t").ToCharArray());

		public static long VarSetStrCapacity(params object[] obj) => throw new Keysharp.Core.Error("VarSetStrCapacity() not supported or necessary.");

		public static long VerCompare(object obj0, object obj1)
		{
			var v1 = obj0.As();
			var v2 = obj1.As();
			var semver1 = Semver.SemVersion.Parse(v1, Semver.SemVersionStyles.Any);
			var semver2 = Semver.SemVersion.Parse(v2, Semver.SemVersionStyles.Any);
			return semver1.CompareSortOrderTo(semver2);
		}

		public static long EndsWith(object obj0, object obj1, object obj2 = null) => obj0.As().EndsWith(obj1.As(), obj2.Ab() ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) ? 1L : 0L;

		public static long StartsWith(object obj0, object obj1, object obj2 = null) => obj0.As().StartsWith(obj1.As(), obj2.Ab() ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) ? 1L : 0L;

		/// <summary>
		/// This appears to be the fastest known way to do this.
		/// Gotten from: https://www.meziantou.net/comparing-implementations-with-benchmarkdotnet.htm
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		internal static string BytesToHexString(byte[] bytes)
		{
			int b;
			var c = new char[bytes.Length * 2];

			for (var i = 0; i < bytes.Length; i++)
			{
				b = bytes[i] >> 4;
				c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
				b = bytes[i] & 0xF;
				c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
			}

			return new string(c);
		}

		internal static bool Cisalnum(char c) => (c & 0x80) == 0 && char.IsLetterOrDigit(c);

		internal static bool Cisalpha(char c) => (c & 0x80) == 0 && char.IsLetter(c);

		internal static bool Cisdigit(char c) => (c & 0x80) == 0 && char.IsDigit(c);

		internal static bool Cislower(char c) => (c & 0x80) == 0 && char.IsLower(c);

		internal static bool Cisprint(char c) => (c & 0x80) == 0 && !char.IsControl(c) || char.IsWhiteSpace(c);

		internal static bool Cisspace(char c) => (c & 0x80) == 0 && char.IsWhiteSpace(c);

		internal static bool Cisupper(char c) => (c & 0x80) == 0 && char.IsUpper(c);

		internal static bool Cisxdigit(char c) => (c & 0x80) == 0 && c.IsHex();

		internal static bool IsSpaceOrTab(char c) => c == ' ' || c == '\t';

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
		/// An internal optimized version of StrCompare().
		/// </summary>
		internal static int StrCmp(string left, string right, bool caseSensitive) => string.Compare(left, right, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

		internal class RegexEntry : KeyedCollection<string, RegexWithTag>
		{
			protected override string GetKeyForItem(RegexWithTag item) => item.tag;
		}
	}

	public class RegExResults : KeysharpObject, IEnumerable
	{
		private Match match;

		public long OutputVarCount => match.Groups.Count - 1;
		public string Mark => match.Groups.Count > 0 ? match.Groups[ ^ 1].Name : "";
		public bool Success => match.Success;

		public RegExResults(Match m) => match = m;

		public static implicit operator long(RegExResults r) => r.Pos();

		public override string ToString() => Pos().ToString();

		public IEnumerator GetEnumerator() => match.Groups.GetEnumerator();

		public long Len(object obj)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Length : 0;
		}

		public string Name(object obj)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Name : "";
		}

		public long Pos(object obj = null)
		{
			var g = GetGroup(obj);
			return g != null && g.Success ? g.Index + 1 : 0;
		}

		private Group GetGroup(object obj)
		{
			var o = obj;

			if (o == null)
				return match;
			else if (o is string s)
				return match.Groups[s];
			else
			{
				var index = Convert.ToInt32(o);

				if (index == 0)
					return match;
				else if (index > 0 && index <= match.Groups.Count)
					return match.Groups[index];
			}

			return null;
		}

		public string this[object obj]
		{
			get
			{
				var g = GetGroup(obj);
				return g != null && g.Success ? g.Value : "";
			}
		}
	}

	public class ReplaceResults : KeysharpObject
	{
		private string str;

		public long OutputVarCount { get; }

		public ReplaceResults(string s, long c)
		{
			str = s;
			OutputVarCount = c;
		}

		public static implicit operator string(ReplaceResults r) => r.str;

		public override string ToString() => str;
	}

	/// <summary>
	/// Human readable sorting from https://www.codeproject.com/Articles/22175/Sorting-Strings-for-Humans-with-IComparer
	/// and slightly modified.
	/// </summary>
	internal class NaturalComparer : IComparer, IComparer<string>
	{
		private static readonly Regex regex;

		static NaturalComparer() => regex = new Regex(@"[\W\.]*([\w-[\d]]+|[\d]+)", RegexOptions.Compiled);

		public int Compare(string left, string right) => NaturalCompare(left, right);

		public int Compare(object left, object right)
		{
			if (!(left is string s1))
				throw new System.ArgumentException("Parameter type is not string", "left");

			if (!(right is string s2))
				throw new System.ArgumentException("Parameter type is not string", "right");

			return Compare(s1, s2);
		}

		internal static int NaturalCompare(string left, string right)
		{
			// optimization: if left and right are the same object, then they compare as the same
			if (left == right)
			{
				return 0;
			}

			var leftmatches = regex.Matches(left);
			var rightmatches = regex.Matches(right);
			var enrm = rightmatches.GetEnumerator();

			foreach (Match lm in leftmatches)
			{
				if (!enrm.MoveNext())
				{
					// the right-hand string ran out first, so is considered "less-than" the left
					return 1;
				}

				var rm = enrm.Current as Match;
				var tokenresult = CompareTokens(lm.Captures[0].Value, rm.Captures[0].Value);

				if (tokenresult != 0)
				{
					return tokenresult;
				}
			}

			// the lefthand matches are exhausted;
			// if there is more, then left was shorter, ie, lessthan
			// if there's no more left in the righthand, then they were all equal
			return enrm.MoveNext() ? -1 : 0;
		}

		private static int CompareTokens(string left, string right)
		{
			var leftisnum = double.TryParse(left, out var leftval);
			var rightisnum = double.TryParse(right, out var rightval);

			if (leftisnum)// numbers always sort in front of text
			{
				if (!rightisnum)
					return -1;

				if (leftval < rightval)// they're both numeric
					return -1;

				if (rightval < leftval)
					return 1;

				// if values are same, this might be due to leading 0s.
				// Assuming this, the longest string would indicate more leading 0s
				// which should be considered to have lower value
				return Math.Sign(right.Length - left.Length);
			}

			// if the right's numeric but left isn't, then the right one must sort first
			if (rightisnum)
				return 1;

			// otherwise do a straight text comparison
			return string.Compare(left, right, StringComparison.CurrentCulture);//Spec says to use "locale" with "logical" sorting.
		}
	}

	internal class RegexWithTag : Regex
	{
		internal string tag;

		internal RegexWithTag(string s)
			: base(s)
		{
		}

		internal RegexWithTag(string s, RegexOptions options)
			: base(s, options)
		{
		}
	}

	//public class RegExMatchInfo
	//{
	//  public long Pos { get; set; }
	//
	//  public long Pos(long n)
	//  {
	//      return 1;
	//  }
	//  //public long Pos { get; set; }
	//}
}