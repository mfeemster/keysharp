using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Keysharp.Scripting
{
	/// <summary>
	/// This was needed to implement this note in the documentation for && and ||:
	///     In an expression where all operands resolve to True, the last operand that resolved to True is returned. Otherwise, the first operand that resolves to False is returned.
	///     In an expression where at least one operand resolves to True, the first operand that resolved to True is returned. Otherwise, the last operand that resolves to False is returned.
	/// </summary>
	public class BoolResult// : Keysharp.Core.KeysharpObject
	{
		internal bool b;
		internal object o;

		public BoolResult(bool _b, object _o)
		{
			b = _b;
			o = GetNestedObj(_o);
		}

		public static implicit operator bool(BoolResult r) => r.b;

		public static BoolResult operator &(BoolResult obj1, BoolResult obj2) => !obj1.b ? obj1 : obj2;

		public static bool operator false(BoolResult obj) => !obj.b;

		public static bool operator true(BoolResult obj) => obj.b;

		public override string ToString() => o.ToString();

		public static BoolResult operator |(BoolResult obj1, BoolResult obj2) => obj1.b ? obj1 : obj2;

		private object GetNestedObj(object obj) => obj is BoolResult br ? GetNestedObj(br.o) : obj;//Could potentially be very slow.
	}

	public partial class Script
	{
		internal static bool ForceBool(object input)
		{
			if (input == null)
				return false;
			else if (input is bool b)//MATT
				return b;
			else if (input is long || input is double || input is decimal || input is float || input is int)
				return ForceDouble(input) != 0;
			else if (input is string s)
				return !string.IsNullOrEmpty(s);
			else if (input.GetType().GetMethods(BindingFlags.Static) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(bool))
						return (bool)mi.Invoke(input, new object[] { input });
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
			else if (input.GetType().GetMethods(BindingFlags.Static) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(decimal))
						return (decimal)mi.Invoke(input, new object[] { input });
			}

			if (input is IConvertible)
				return Convert.ToDecimal(input);

			return 0m;
		}

		internal static double ForceDouble(object input)
		{
			if (input is double d)
				return d;
			else if (input is long l)
				return (double)l;
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
			else if (input is bool b)//MATT
				return b ? 1.0 : 0.0;
			else if (input is null)
				return 0.0;
			else if (input is int i)//These should never happen.
				return (double)i;
			else if (input is float f)
				return (double)f;
			else if (input is uint ui)
				return (double)ui;
			else if (input is ulong ul)
				return (double)ul;
			else if (input.GetType().GetMethods(BindingFlags.Static) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(double))
						return (double)mi.Invoke(input, new object[] { input });
			}

			if (input is IConvertible)
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
			else if (input is null)
				return 0;
			else if (input.GetType().GetMethods(BindingFlags.Static) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(int))
						return (int)mi.Invoke(input, new object[] { input });
			}

			if (input is IConvertible)
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
			else if (input is null)
				return 0;
			else if (input.GetType().GetMethods(BindingFlags.Static) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(long))
						return (long)mi.Invoke(input, new object[] { input });
			}

			if (input is IConvertible)
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
				var simple = t == typeof(int) || t == typeof(uint) || t == typeof(long);
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
			else if (input.GetType().GetMethods(BindingFlags.Static) is MethodInfo[] mis)
			{
				foreach (var mi in mis)
					if (mi.Name == "op_Implicit" && mi.ReturnType == typeof(string))
						return (string)mi.Invoke(input, new object[] { input });
			}

			var type = input.GetType();
			var buffer = new StringBuilder();

			if (input is Keysharp.Core.Map map)
			{
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
			else if (input is Array array)
			{
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
	}
}