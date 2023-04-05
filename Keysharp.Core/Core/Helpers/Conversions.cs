using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Keysharp.Core
{
	public static class Conversions
	{
		private const string bin = "REG_BINARY";

		private const string dword = "REG_DWORD";

		private const string multisz = "REG_MULTI_SZ";

		private const string none = "NONE";

		private const string qword = "REG_QWORD";

		private const string regexpsz = "REG_EXPAND_SZ";

		private const string regsz = "REG_SZ";

		private const string unk = "UNKNOWN";

		/// <summary>
		/// Need to manually provide a wrapper because calendar is not a constant, which is required for default parameters.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public static DateTime ToDateTime(string time)
		{
			switch (time.Length)
			{
				case 14:
					if (DateTime.TryParseExact(time, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var dt))
						return dt;

					break;

				case 12:
					if (DateTime.TryParseExact(time, "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
						return dt;

					break;

				case 10:
					if (DateTime.TryParseExact(time, "yyyyMMddHH", System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
						return dt;

					break;

				case 8:
					if (DateTime.TryParseExact(time, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
						return dt;

					break;

				case 6:
					if (DateTime.TryParseExact(time, "yyyyMM", System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
						return dt;

					break;

				case 4:
					if (DateTime.TryParseExact(time, "yyyy", System.Globalization.CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
						return dt;

					break;
			}

			return DateTime.MinValue;
		}

		internal static string FromFileAttribs(FileAttributes attribs)
		{
			var str = new StringBuilder(9);

			if ((attribs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				_ = str.Append('R');

			if ((attribs & FileAttributes.Archive) == FileAttributes.Archive)
				_ = str.Append('A');

			if ((attribs & FileAttributes.System) == FileAttributes.System)
				_ = str.Append('S');

			if ((attribs & FileAttributes.Hidden) == FileAttributes.Hidden)
				_ = str.Append('H');

			if ((attribs & FileAttributes.Normal) == FileAttributes.Normal)
				_ = str.Append('N');

			if ((attribs & FileAttributes.Directory) == FileAttributes.Directory)
				_ = str.Append('D');

			if ((attribs & FileAttributes.Offline) == FileAttributes.Offline)
				_ = str.Append('O');

			if ((attribs & FileAttributes.Compressed) == FileAttributes.Compressed)
				_ = str.Append('C');

			if ((attribs & FileAttributes.Temporary) == FileAttributes.Temporary)
				_ = str.Append('T');

			if (str.Length == 0)
				_ = str.Append('X');

			return str.ToString();
		}

		internal static long FromTime(DateTime time)
		{
			const int len = 2;
			const char pad = '0';
			var str = new StringBuilder(4 + 2 * 5);
			_ = str.Append(time.Year.ToString().PadLeft(len * 2, pad));
			_ = str.Append(time.Month.ToString().PadLeft(len, pad));
			_ = str.Append(time.Day.ToString().PadLeft(len, pad));
			_ = str.Append(time.Hour.ToString().PadLeft(len, pad));
			_ = str.Append(time.Minute.ToString().PadLeft(len, pad));
			_ = str.Append(time.Second.ToString().PadLeft(len, pad));
			return long.Parse(str.ToString());
		}

		internal static RegistryValueKind GetRegistryType(string val)
		{
			switch (val)
			{
				case none:
					return RegistryValueKind.None;

				case unk:
					return RegistryValueKind.Unknown;

				case regsz:
					return RegistryValueKind.String;

				case regexpsz:
					return RegistryValueKind.ExpandString;

				case bin:
					return RegistryValueKind.Binary;

				case dword:
					return RegistryValueKind.DWord;

				case multisz:
					return RegistryValueKind.MultiString;

				case qword:
					return RegistryValueKind.QWord;

				default:
					return RegistryValueKind.Unknown;
			}
		}

		internal static string GetRegistryTypeName(RegistryValueKind val)
		{
			switch (val)
			{
				case RegistryValueKind.None:
					return none;

				case RegistryValueKind.Unknown:
					return unk;

				case RegistryValueKind.String:
					return regsz;

				case RegistryValueKind.ExpandString:
					return regexpsz;

				case RegistryValueKind.Binary:
					return bin;

				case RegistryValueKind.DWord:
					return dword;

				case RegistryValueKind.MultiString:
					return multisz;

				case RegistryValueKind.QWord:
					return qword;

				default:
					return unk;
			}
		}

		internal static byte HighByte(int i) => (byte)((((ulong)i) >> 8) & 0xff);

		internal static short HighWord(int i) => (short)((((ulong)i) >> 16) & 0xffff);

		internal static byte LowByte(int i) => (byte)(((ulong)i) & 0xff);

		internal static short LowWord(int i) => (short)(((ulong)i) & 0xffff);

		internal static int MakeInt(int lowPart, int highPart) => (lowPart & 0x0000FFFF) | (highPart << 16);

		internal static StringComparison ParseComparisonOption(object option)
		{
			switch (option.ToString().ToLowerInvariant())
			{
				case "1":
				case Core.Keyword_On: return System.StringComparison.Ordinal;

				case Core.Keyword_Locale: return System.StringComparison.CurrentCulture;

				case "0":
				case Core.Keyword_Off:
				default: return System.StringComparison.OrdinalIgnoreCase;
			}
		}

		internal static Font ParseFont(Font standard, string styles, string family = null)
		{
			family = string.IsNullOrEmpty(family) ? standard.FontFamily.Name : family;
			var size = standard.Size;
			var display = standard.Style;
			var weight = 400;
			var quality = 0;

			foreach (var opt in Options.ParseOptions(styles))
			{
				var mode = opt.ToLowerInvariant();

				if (Options.TryParse(mode, "s", ref size)) { }
				else if (Options.TryParse(mode, "q", ref quality)) { }
				else if (Options.TryParse(mode, "w", ref weight))
				{
					if (weight <= 400)
						display &= ~FontStyle.Bold;
					else if (weight >= 700)
						display |= FontStyle.Bold;
				}

				switch (mode)
				{
					case Core.Keyword_Bold: display |= FontStyle.Bold; break;

					case Core.Keyword_Italic: display |= FontStyle.Italic; break;

					case Core.Keyword_Strike: display |= FontStyle.Strikeout; break;

					case Core.Keyword_Underline: display |= FontStyle.Underline; break;

					case Core.Keyword_Norm: display = FontStyle.Regular; break;
				}
			}

			return new Font(family, size, display);
		}

		internal static List<int> ParseRange(string[] splits)
		{
			var vals = new List<int>();

			for (var i = 0; i < splits.Length; i++)
			{
				if (splits[i]?.Length == 0)
					continue;

				if (int.TryParse(splits[i], out var val))
				{
					if (i > 0)
						if (splits[i - 1]?.Length == 0)
							val = -val;

					vals.Add(val);
				}
			}

			return vals;
		}

		internal static void ParseRange(string range, out DateTime dtlow, out DateTime dthigh)
		{
			var splits = range.Split('-', StringSplitOptions.None);
			dtlow = DateTime.MinValue;
			dthigh = DateTime.MaxValue;

			if (splits.Length == 2)
			{
				if (string.IsNullOrEmpty(splits[0]) && !string.IsNullOrEmpty(splits[1]))
				{
					dtlow = DateTime.MinValue;
					_ = DateTime.TryParseExact(splits[1], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dthigh);
				}
				else if (!string.IsNullOrEmpty(splits[0]) && string.IsNullOrEmpty(splits[1]))
				{
					_ = DateTime.TryParseExact(splits[0], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dtlow);
					dthigh = DateTime.MaxValue;
				}
				else if (!string.IsNullOrEmpty(splits[0]) && !string.IsNullOrEmpty(splits[1]))
				{
					_ = DateTime.TryParseExact(splits[0], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dtlow);
					_ = DateTime.TryParseExact(splits[1], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dthigh);
				}
			}
			else if (splits.Length == 1 && !string.IsNullOrEmpty(splits[0]))
			{
				_ = DateTime.TryParseExact(splits[0], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dtlow);
				dthigh = DateTime.MaxValue;
			}
		}

		internal static RegexWithTag ParseRegEx(string exp, bool reverse = false)
		{
			var opts = reverse ? RegexOptions.RightToLeft : RegexOptions.None;
			opts |= RegexOptions.Compiled;
			var parenIndex = exp.IndexOf(')');

			if (parenIndex != -1)
			{
				var leftParenIndex = exp.IndexOf('(');

				if (leftParenIndex == -1 || (leftParenIndex > parenIndex))//Make sure it was just a ) for options and not a ().
				{
					var span = exp.AsSpan(0, parenIndex);
					var substr = exp.Substring(parenIndex + 1);
					opts |= ToRegexOptions(span);

					if (span.Contains('A'))
					{
						substr = "\\A" + substr;
					}

					return new RegexWithTag(substr, opts);
				}
			}

			return new RegexWithTag(exp, opts);
		}

		/// <summary>
		/// Gotten from https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa
		/// </summary>
		/// <param name="hex">The hexadecimal formatted input string to convert</param>
		/// <returns>The hexadecimal formatted input string converted to an array of bytes</returns>
		internal static byte[] StringToByteArray(string hex)
		{
			var len = hex.Length;
			var bytes = new byte[len / 2];

			for (var i = 0; i < len; i += 2)
				bytes[i / 2] = byte.Parse(hex.AsSpan(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

			return bytes;
		}

		internal static DateTime ToDateTime(string time, Calendar cal)
		{
			int[] t = { DateTime.Now.Year / 100, DateTime.Now.Year % 100, 1, 1, 0, 0, 0, 0 };
			int i, k;

			for (i = 0, k = 0; i < t.Length; i++, k += 2)
				if (k + 1 >= time.Length || !int.TryParse(time.AsSpan(k, 2), out t[i]))
					break;

			return i == 0 ? DateTime.MinValue : new DateTime((t[0] * 100) + t[1], t[2], t[3], t[4], t[5], t[6], cal);
		}

		internal static FileAttributes ToFileAttribs(string set, FileAttributes attribs)
		{
			var state = '+';

			foreach (var flag in set)
			{
				var applied = FileAttributes.Normal;

				switch (flag)
				{
					case '+':
					case '-':
					case '^':
						state = flag;
						continue;

					case 'r':
					case 'R':
						applied = FileAttributes.ReadOnly;
						break;

					case 'a':
					case 'A':
						applied = FileAttributes.Archive;
						break;

					case 's':
					case 'S':
						applied = FileAttributes.System;
						break;

					case 'h':
					case 'H':
						applied = FileAttributes.Hidden;
						break;

					case 'n':
					case 'N':
						applied = FileAttributes.Normal;
						break;

					//case 'd':
					//case 'D':
					//  applied = FileAttributes.Directory;
					//  break;

					case 'o':
					case 'O':
						applied = FileAttributes.Offline;
						break;

					//case 'c':
					//case 'C':
					//  applied = FileAttributes.Compressed;
					//  break;

					case 't':
					case 'T':
						applied = FileAttributes.Temporary;
						break;
				}

				switch (state)
				{
					case '-':
						attribs &= ~applied;
						break;

					case '^':
						if ((attribs & applied) == applied)
							attribs &= ~applied;
						else
							attribs |= applied;

						break;

					default:
						attribs |= applied;
						break;
				}

				state = '+';
			}

			return attribs;
		}

		internal static string[] ToFiles(string path, bool files, bool dirs, bool recurse)
		{
			var filelist = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileName(path),
											  recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

			if (dirs)
			{
				var dirlist = new HashSet<string>();

				foreach (var file in filelist)
					dirlist.Add(Path.GetDirectoryName(file));

				if (files)
				{
					int i;
					var merge = new string[dirlist.Count + filelist.Length];

					for (i = 0; i < filelist.Length; i++)
						merge[i] = filelist[i];

					foreach (var dir in dirlist)
						merge[i++] = dir;

					return merge;
				}
				else return dirlist.ToArray();
			}
			else if (files)
				return filelist;

			return System.Array.Empty<string>();
		}

		internal static string ToOSType(PlatformID id)
		{
			switch (id)
			{
				case PlatformID.MacOSX: return "MACOSX";

				case PlatformID.Unix: return "UNIX";

				case PlatformID.Win32NT: return "WIN32_NT";

				case PlatformID.Win32S: return "WIN32_S";

				case PlatformID.Win32Windows: return "WIN32_WINDOWS";

				case PlatformID.WinCE: return "WINCE";

				case PlatformID.Xbox: return "XBOX";

				default: return "UNKNOWN";
			}
		}

		internal static RegexOptions ToRegexOptions(ReadOnlySpan<char> sequence)
		{
			var options = RegexOptions.None;

			foreach (var modifier in sequence)
			{
				switch (modifier)
				{
					case 'i':
					case 'I':
						options |= RegexOptions.IgnoreCase;
						break;

					case 'm':
					case 'M':
						options |= RegexOptions.Multiline;
						break;

					case 's':
					case 'S':
						options |= RegexOptions.Singleline;
						break;

					case 'x':
					case 'X':
						options |= RegexOptions.IgnorePatternWhitespace;
						break;
				}
			}

			return options;
		}

		internal static (RegistryKey, string, string) ToRegKey(string root, bool writable = false)
		{
			var (reg, comp, key) = ToRegRootKey(root);
			//var keys = SubKey.Split(@"/\".ToCharArray());
			//
			//for (var i = 0; i < keys.Length - (Parent ? 1 : 0); i++)
			//  reg = reg.OpenSubKey(keys[i].Trim());
			//
			//SubKey = Parent ? keys[keys.Length - 1] : string.Empty;
			//if (reg.)
			var regkey = reg.OpenSubKey(key, writable);

			if (regkey == null)
				regkey = reg.CreateSubKey(key, writable);

			return (regkey, comp, key);
		}

		internal static (RegistryKey, string, string) ToRegRootKey(string name)
		{
			var computername = "";

			if (name.StartsWith("\\\\"))
			{
				var index = name.IndexOf("\\", 2);
				computername = name.Substring(2, index - 2);
				name = name.Substring(index + 1);
			}

			var index2 = name.IndexOf("\\");
			var root = name.Substring(0, index2);
			var key = name.Substring(index2 + 1);

			switch (root.ToLowerInvariant())
			{
				case Core.Keyword_HKey_Local_Machine:
				case Core.Keyword_HKLM:
					return (RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Registrys.GetRegView()), computername, key);

				case Core.Keyword_HKey_Users:
				case Core.Keyword_HKU:
					return (RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.Users, Registrys.GetRegView()), computername, key);

				case Core.Keyword_HKey_Current_User:
				case Core.Keyword_HKCU:
					return (RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Registrys.GetRegView()), computername, key);

				case Core.Keyword_HKey_Classes_Root:
				case Core.Keyword_HKCR:
					return (RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.ClassesRoot, Registrys.GetRegView()), computername, key);

				case Core.Keyword_HKey_Current_Config:
				case Core.Keyword_HKCC:
					return (RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentConfig, Registrys.GetRegView()), computername, key);

				case Core.Keyword_HKey_Performance_Data:
				case Core.Keyword_HKPD:
					return (RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.PerformanceData, Registrys.GetRegView()), computername, key);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal static string ToStringCaseSense(StringComparison type)
		{
			switch (type)
			{
				case StringComparison.CurrentCultureIgnoreCase: return Core.Keyword_Locale;

				case StringComparison.Ordinal: return Core.Keyword_On;

				case StringComparison.OrdinalIgnoreCase: return Core.Keyword_Off;

				default: return Core.Keyword_Off;
			}
		}

		internal static string ToYYYYMMDDHH24MISS(DateTime time) => time.ToString("yyyyMMddHHmmss");

		internal static bool TryParseColor(string name, out Color c)
		{
			var isnum = false;
			name = name.Trim();

			if (name.Length > 0 && name[0] == '#')
			{
				isnum = true;
				name = name.Substring(1);
			}
			else if (name.Length > 1 && name[0] == '0' && (name[1] == 'x' || name[1] == 'X'))
			{
				isnum = true;
				name = name.Substring(2);
			}
			else if (name.AllHex())
				isnum = true;

			if (isnum)
			{
				if (uint.TryParse(name, NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out var val))
				{
					if (name.Length <= 6)
						val |= 0xFF000000;

					c = Color.FromArgb((int)val);
					return true;
				}
			}

			if (string.Compare(name, "default", true) == 0)
			{
				c = Form.DefaultForeColor;
				return true;
			}

			c = Color.FromName(name);
			return c.IsKnownColor;
		}
	}
}