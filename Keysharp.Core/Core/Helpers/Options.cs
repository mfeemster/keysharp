using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Core
{
	public static class Options
	{
		internal static ToggleValueType ConvertOnOff(object mode, ToggleValueType def = ToggleValueType.Invalid)
		{
			if (mode == null)
				return ToggleValueType.Neutral;

			var str = mode.ParseObject().ToString();

			if (str?.Length == 0)
				return ToggleValueType.Neutral;

			if (string.Compare(str, "on", true) == 0 || str == "1") return ToggleValueType.On;

			if (string.Compare(str, "off", true) == 0 || str == "0") return ToggleValueType.Off;

			return def;
		}

		internal static ToggleValueType ConvertOnOffToggle(object mode, ToggleValueType def = ToggleValueType.Default)
		{
			var toggle = ConvertOnOff(mode);

			if (toggle != ToggleValueType.Invalid)
				return toggle;

			var str = mode.ParseObject().ToString();
			return string.Compare(str, "Toggle", true) == 0 || str == "-1" ? ToggleValueType.Toggle : def;
		}

		internal static bool IsAnyBlank(params string[] args)
		{
			foreach (var str in args)
				if (string.IsNullOrEmpty(str))
					return true;

			return false;
		}

		internal static bool IsOption(string options, string search)
		{
			return string.IsNullOrEmpty(options) || string.IsNullOrEmpty(search)
				   ? false
				   : options.Trim().Equals(search, StringComparison.OrdinalIgnoreCase);
		}

		internal static Dictionary<char, string> KeyValues(string Options, bool Lowercase, char[] Exceptions)
		{
			var table = new Dictionary<char, string>();
			var buf = new StringBuilder();
			var i = 0;
			var exp = false;
			char key = default;
			string val;

			if (Lowercase)
				for (i = 0; i < Exceptions.Length; i++)
					Exceptions[i] = char.ToLowerInvariant(Exceptions[i]);

			i = 0;

			while (i < Options.Length)
			{
				var sym = Options[i];
				i++;

				if (char.IsWhiteSpace(sym) || i == Options.Length)
				{
					if (buf.Length == 0)
						continue;

					if (exp)
					{
						exp = false;
						val = buf.ToString();
						buf.Length = 0;

						if (table.ContainsKey(key))
							table[key] = val;
						else
							table.Add(key, val);

						continue;
					}

					key = buf[0];

					if (Lowercase)
						key = char.ToLowerInvariant(key);

					foreach (var ex in Exceptions)
						if (key == ex)
						{
							exp = true;
							buf.Length = 0;
							continue;
						}

					val = buf.Length > 1 ? buf.Remove(0, 1).ToString() : string.Empty;

					if (table.ContainsKey(key))
						table[key] = val;
					else
						table.Add(key, val);

					buf.Length = 0;
				}
				else
					_ = buf.Append(sym);
			}

			if (exp && key != default(char))
			{
				if (table.ContainsKey(key))
					table[key] = null;
				else
					table.Add(key, null);
			}

			return table;
		}

		/// <summary>
		/// Merges two Dictionarys in generic way
		/// </summary>
		/// <typeparam name="T">any</typeparam>
		/// <typeparam name="T2">any</typeparam>
		/// <param name="dict1">Dictionary 1</param>
		/// <param name="dict2">Dictionary 2</param>
		/// <returns>Merged Dictionary</returns>
		internal static Dictionary<T, T2> MergeDictionarys<T, T2>(Dictionary<T, T2> dict1, Dictionary<T, T2> dict2)
		{
			var merged = new Dictionary<T, T2>();

			foreach (var key in dict1.Keys)
			{
				merged.Add(key, dict1[key]);
			}

			foreach (var key in dict2.Keys)
			{
				if (!merged.ContainsKey(key))
					merged.Add(key, dict2[key]);
			}

			return merged;
		}

		internal static bool? OnOff(object mode)
		{
			switch (mode.ParseObject().ToString().ToLowerInvariant())
			{
				case Core.Keyword_On:
				case "1":
				case "true":
					return true;

				case Core.Keyword_Off:
				case "0":
				case "false":
					return false;

				default:
					return null;
			}
		}

		internal static bool OptionContains(string options, params string[] keys)
		{
			foreach (var key in keys)
				if (!OptionContains(options, key))
					return false;

			return true;
		}

		internal static bool OptionContains(string options, string key, bool casesensitive = false)
		{
			// TODO: test OptionContains method
			var comp = casesensitive ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase;
			var i = 0;

			while (i < options.Length)
			{
				var z = options.IndexOf(key, i, comp);
				var p = z == 0 || !char.IsLetter(options, z - 1);
				z += key.Length;

				if (!p)
					continue;

				p = z == options.Length || !char.IsLetter(options, z + 1);

				if (!p)
					continue;

				return true;
			}

			return false;
		}

		internal static List<string> ParseFlags(ref string arg)
		{
			var list = new List<string>();
			const char flag = '*';
			var i = -1;

			foreach (var sym in arg)
			{
				i++;

				if (System.Array.IndexOf(Core.Keyword_Spaces, sym) != -1)
					continue;

				if (sym != flag)
					break;

				var z = i;

				for (; i < arg.Length; i++)
					if (char.IsWhiteSpace(arg, i) || arg[i] == flag)
						break;

				if (z == i)
					continue;

				var item = arg.Substring(z, i - z);
				list.Add(item);
			}

			arg = arg.Substring(i);
			return list;
		}

		internal static string[] ParseOptions(string options) => options.Split(Core.Keyword_Spaces, StringSplitOptions.RemoveEmptyEntries);

		internal static Dictionary<string, string> ParseOptionsRegex(ref string options, Dictionary<string, Regex> items, bool remove = true)
		{
			var results = new Dictionary<string, string>();

			foreach (var item in items)
			{
				if (item.Value.IsMatch(options))
				{
					var match = item.Value.Match(options).Groups[1].Captures[0];
					results.Add(item.Key, match.Value);

					if (remove)
						options = options.Substring(0, match.Index) + options.Substring(match.Index + match.Length);
				}
				else
				{
					results.Add(item.Key, "");
				}
			}

			return results;
		}

		internal static int[] ParseVersionToInts(string ver)
		{
			var vers = new int[] { 0, 0, 0, 0 };
			var versplits = ver.Split('.', StringSplitOptions.RemoveEmptyEntries);

			if (versplits.Length > 0)
			{
				for (var i = 0; i < 4; i++)
				{
					if (versplits.Length > i)
						if (versplits[i].ParseInt(false) is int v)
							vers[i] = v;
				}
			}

			return vers;
		}

		internal static bool TryParse(string opt, string prefix, ref int result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowempty = false, int def = default) =>
		TryParseWrapper(opt, prefix, (ReadOnlySpan<char> s, out int x) =>
		{
			if (int.TryParse(s, out x))
				return true;

			return s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
				   int.TryParse(s.Slice(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out x);
		}, ref result, comp, allowempty, def);

		internal static bool TryParse(string opt, string prefix, ref long result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowempty = false, long def = default) =>
		TryParseWrapper(opt, prefix, (ReadOnlySpan<char> s, out long x) =>
		{
			if (long.TryParse(s, out x))
				return true;

			return s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
				   long.TryParse(s.Slice(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out x);
		}, ref result, comp, allowempty, def);

		internal static bool TryParse(string opt, string prefix, ref float result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowempty = false, float def = default) =>
		TryParseWrapper(opt, prefix, float.TryParse, ref result, comp, allowempty, def);

		internal static bool TryParse(string opt, string prefix, ref double result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowempty = false, double def = default) =>
		TryParseWrapper(opt, prefix, double.TryParse, ref result, comp, allowempty, def);

		internal static bool TryParse(string opt, string prefix, ref Color result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowempty = false) =>
		TryParseWrapper(opt, prefix, (ReadOnlySpan<char> v, out Color r) => { return Conversions.TryParseColor(v.ToString(), out r); }, ref result, comp, allowempty);

		internal static bool TryParse(string opt, string prefix, ref bool result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowempty = false, bool def = default)
		{
			if (opt[0] == '-' && opt.AsSpan(1).CompareTo(prefix, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = false;
				return true;
			}
			else if (opt[0] == '+' && opt.AsSpan(1).CompareTo(prefix, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = true;
				return true;
			}

			return TryParseWrapper(opt, prefix, (ReadOnlySpan<char> v, out bool r) =>
			{
				var b = OnOff(v.ToString());

				if (b != null)
				{
					r = b.Value;
					return true;
				}
				else if (allowempty)
				{
					r = false;
					return true;
				}

				r = false;
				return false;
			}, ref result, comp, allowempty, def);
		}

		/// <summary>
		/// Parse a string and get Coordinates
		/// </summary>
		/// <param name="input">String in Format X123 Y123</param>
		/// <param name="p">out Point Struct if possible</param>
		/// <returns>true if parsing succesful</returns>
		internal static bool TryParseCoordinate(string input, out Point p)
		{
			throw new NotImplementedException();
		}

		internal static bool TryParseDateTime(string opt, string prefix, string format, ref DateTime result, StringComparison comp = StringComparison.OrdinalIgnoreCase) =>
		TryParseWrapper(opt, prefix, (ReadOnlySpan<char> v, out DateTime r) =>
		{
			if (!DateTime.TryParseExact(v, format.AsSpan(), CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out r))
				r = Conversions.ToDateTime(v.ToString());

			return true;
		}, ref result, comp);

		internal static bool TryParseString(string opt, string prefix, ref string result, StringComparison comp = StringComparison.OrdinalIgnoreCase, bool allowEmpty = false) =>
		TryParseWrapper(opt, prefix, (ReadOnlySpan<char> v, out string r) => { r = v.ToString(); return true; }, ref result, comp, allowEmpty);

		internal static void VerifyVersion(string ver, bool plus, int line, string source)
		{
			var ahkver = Accessors.A_AhkVersion;
			var reqvers = ParseVersionToInts(ver);
			var thisvers = ParseVersionToInts(ahkver);

			for (var i = 0; i < 4; i++)
			{
				if (plus)
				{
					if (reqvers[i] > thisvers[i])
						throw new ParseException($"This script requires Keysharp >= v{ver}, but you have v{ahkver}", line, source);
				}
				else if (reqvers[i] != thisvers[i])
					throw new ParseException($"This script requires Keysharp == v{ver}, but you have v{ahkver}", line, source);

				if (thisvers[i] > reqvers[i])
					break;
			}
		}

		private static bool TryParseWrapper<T>(string opt, string prefix, TryParseHandler<T> handler, ref T result, StringComparison comp = StringComparison.OrdinalIgnoreCase,
											   bool allowempty = false, T def = default)// where T : struct
		{
			if (opt.StartsWith(prefix, comp))
			{
				var suffix = opt.AsSpan(prefix.Length);

				if (allowempty && suffix.CompareTo(ReadOnlySpan<char>.Empty, StringComparison.OrdinalIgnoreCase) == 0)//Need CompareTo() because == doesn't work with spans and "".
				{
					result = def;
					return true;
				}
				else if (handler(suffix, out var res))
				{
					result = res;
					return true;
				}
			}

			return false;
		}

		internal delegate bool TryParseHandler<T>(ReadOnlySpan<char> value, out T result);
	}
}