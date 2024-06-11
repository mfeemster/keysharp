using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using static Keysharp.Scripting.Keywords;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		internal static bool ForceBool(object input)
		{
			if (input is bool b)
				return b;
			else if (input == null)
				return false;
			else if (input is long || input is double || input is decimal || input is float || input is int || input is byte || input is char)
				return ForceDouble(input) != 0;
			else if (input is string s)
				return !string.IsNullOrEmpty(s);
			else if (input is IntPtr ip)
				return ip != IntPtr.Zero;
			else if (input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(bool))
						return (bool)mi.Invoke(input, new object[] { input });
					else if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(long))
						return ((long)mi.Invoke(input, new object[] { input }) != 0);
			}

			return true;//Any non-null, non-empty string is considered true.
		}

		internal static decimal ForceDecimal(object input)
		{
			if (input is decimal m)
				return m;
			else if (input is string s)
			{
				if (s != "")
				{
					if (char.IsLetter(s[ ^ 1]))
					{
						var strimmed = s.TrimEndAlpha();

						if (decimal.TryParse(strimmed, out var result))
							return result;
					}
					else
					{
						if (decimal.TryParse(s, out var result))
							return result;
					}
				}
			}
			else if (input is bool b)
				return b ? 1m : 0m;
			else if (input is byte by)
				return by;
			else if (input is char c)
				return c;
			else if (input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(decimal))
						return (decimal)mi.Invoke(input, new object[] { input });
			}
			else if (input is IConvertible)
				return Convert.ToDecimal(input);

			return 0m;
		}

		internal static double ForceDouble(object input)
		{
			if (input is double d)
				return d;
			else if (input is long l)
				return l;
			else if (input is string s)
			{
				if (s != "")
				{
					if (char.IsLetter(s[ ^ 1]))
					{
						var strimmed = s.TrimEndAlpha();

						if (double.TryParse(strimmed, out var result))
							return result;
					}
					else
					{
						if (double.TryParse(s, out var result))
							return result;
					}
				}
			}
			//else if (input is BoolResult br)
			//return ForceDouble(br.o);
			else if (input is bool b)
				return b ? 1.0 : 0.0;
			else if (input is byte by)
				return by;
			else if (input is char c)
				return c;
			else if (input is null)
				return 0.0;
			else if (input is int i)//These should never happen.
				return i;
			else if (input is float f)
				return (double)f;
			else if (input is uint ui)
				return ui;
			else if (input is ulong ul)
				return ul;
			else if (input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(double))
						return (double)mi.Invoke(input, new object[] { input });
			}
			else if (input is IConvertible)
				return Convert.ToDouble(input);

			return 0.0;
		}

		internal static int ForceInt(object input)
		{
			if (input is long l)//The most likely case is for it to be a long.
				return (int)l;
			else if (input is int i)
				return i;
			else if (input is uint ui)
				return (int)ui;
			else if (input is string s)
			{
				if (s != "")
				{
					if (s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
							int.TryParse(s.AsSpan(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out var ll))
					{
						return ll;
					}
					else if (char.IsLetter(s[ ^ 1]))
					{
						var strimmed = s.TrimEndAlpha();

						if (int.TryParse(strimmed, out var result))
							return result;
					}
					else
					{
						if (int.TryParse(s, out var result))
							return result;
					}
				}
			}
			else if (input is bool b)
				return b ? 1 : 0;
			else if (input is byte by)
				return by;
			else if (input is char c)
				return c;
			else if (input is null)
				return 0;
			else if (input is IntPtr ptr)
				return ptr.ToInt32();
			else if (input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(int))
						return (int)mi.Invoke(input, new object[] { input });
			}
			else if (input is IConvertible)
				return Convert.ToInt32(input);

			return 0;
		}

		internal static long ForceLong(object input)
		{
			if (input is long l)
				return l;
			else if (input is int i)
				return i;
			else if (input is uint ui)
				return ui;
			else if (input is string s)
			{
				if (s != "")
				{
					if (s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
							long.TryParse(s.AsSpan(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out var ll))
					{
						return ll;
					}
					else if (char.IsLetter(s[ ^ 1]))
					{
						var strimmed = s.TrimEndAlpha();

						if (long.TryParse(strimmed, out var result))
							return result;
					}
					else
					{
						if (long.TryParse(s, out var result))
							return result;
					}
				}
			}
			else if (input is bool b)
				return b ? 1 : 0;
			else if (input is byte by)
				return by;
			else if (input is char c)
				return c;
			else if (input is null)
				return 0;
			else if (input is IntPtr ptr)
				return ptr.ToInt64();
			else if (input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(long))
						return (long)mi.Invoke(input, new object[] { input });
			}
			else if (input is IConvertible)
				return Convert.ToInt64(input);

			return 0;
		}

		internal static string ForceString(object input)
		{
			if (input == null)
				return string.Empty;
			else if (input is string s)
				return s;
			else if (input is char c)
				return c.ToString();
			else if (input is bool b)
				return b ? "1" : "0";
			else if (input is byte[] arr)
				return Encoding.Unicode.GetString(arr);
			else if (input is decimal m)
				return m.ToString();
			else if (input is Core.FuncObj fo)
				return fo.Name;
			else if (IsNumeric(input))
			{
				var t = input.GetType();
				var simple = t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(byte) || t == typeof(char);
				var integer = simple || (t == typeof(double) && Math.IEEERemainder((double)input, 1) == 0);
				var format = Keysharp.Core.Accessors.A_FormatNumeric as string;
				var hex = format.IndexOf('x') != -1;
				const string hexpre = "0x";

				if (integer)
				{
					if (!hex)
						format = "d";

					var result = simple ? ForceLong(input).ToString(format) : ((int)(double)input).ToString("d");

					if (hex)
						result = hexpre + result;

					return result;
				}

				var d = (double)input;

				if (hex)
				{
					Keysharp.Core.Accessors.A_FormatNumeric = "X";
					var result = d.ToString(Keysharp.Core.Accessors.A_FormatNumeric as string);
					Keysharp.Core.Accessors.A_FormatNumeric = format;
					return hexpre + result;
				}

				return d.ToString(format).TrimEnd(zerochars);//Remove trailing zeroes for string compare.
			}
			else if (input.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(string))
						return (string)mi.Invoke(input, new object[] { input });
			}

			if (input is Keysharp.Core.Map map)
			{
				var buffer = new StringBuilder();
				_ = buffer.Append(BlockOpen);
				var first = true;

				foreach (var (k, v) in map)
				{
					if (first)
						first = false;
					else
						_ = buffer.Append(DefaultMulticast);

					_ = buffer.Append(StringBound);
					_ = buffer.Append(ForceString(k));
					_ = buffer.Append(StringBound);
					_ = buffer.Append(AssignPre);

					if (v == null)
					{
						_ = buffer.Append(NullTxt);
						continue;
					}

					var obj = v is Array || v is Core.Map || v is Core.FuncObj;// Delegate;

					if (!obj)
						_ = buffer.Append(StringBound);

					_ = buffer.Append(ForceString(v));

					if (!obj)
						_ = buffer.Append(StringBound);
				}

				_ = buffer.Append(BlockClose);
				return buffer.ToString();
			}
			else if (input is Keysharp.Core.Array array)
			{
				var buffer = new StringBuilder();
				_ = buffer.Append(ArrayOpen);
				var first = true;

				foreach (var item in array)
				{
					if (first)
						first = false;
					else
						_ = buffer.Append(DefaultMulticast);

					_ = buffer.Append(ForceString(item));
				}

				_ = buffer.Append(ArrayClose);
				return buffer.ToString();
			}
			else
				return input.ToString();
		}

		private static object ForceType(Type requested, object value)
		{
			if (requested == typeof(object) || requested.IsAssignableFrom(value.GetType()))
				return value;

			if (requested == typeof(decimal))
				return ForceDecimal(value);

			if (requested == typeof(double))
				return ForceDouble(value);

			//if (requested == typeof(int))
			//  return ForceInt(value);

			if (requested == typeof(long))
				return ForceLong(value);

			return requested == typeof(string) ? ForceString(value) : value;
		}
	}
}