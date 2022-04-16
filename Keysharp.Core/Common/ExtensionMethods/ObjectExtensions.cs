using System.Collections;
using System.Globalization;
using System.Linq;

namespace System
{
	public static class ObjectExtensions
	{
		/// <summary>
		/// V2 version name of Enum().
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static IEnumerator __Enum(this IEnumerable obj, params object[] values) => obj.GetEnumerator();

		public static T[] Concat<T>(this T[] x, T[] y)
		{
			if (x == null) throw new ArgumentNullException("x");

			if (y == null) throw new ArgumentNullException("y");

			var oldLen = x.Length;
			Array.Resize<T>(ref x, x.Length + y.Length);
			Array.Copy(y, 0, x, oldLen, y.Length);
			return x;
		}

		public static bool IsFalse(this bool? b) => b.HasValue&& !b.Value;

		public static bool IsTrue(this bool? b) => b.HasValue&& b.Value;

		//public static IList L(this object[] obj) => obj.Length > 0 && obj[0] is IList oo ? oo : obj;
		public static IList L(this object[] obj) => obj.Flatten().Cast<object>().ToList();

		public static bool? ParseBool(this object obj)
		{
			if (obj is bool b)
				return b;

			if (obj is Keysharp.Scripting.BoolResult br)
				return br.o.ParseBool();

			return Keysharp.Core.Options.OnOff(obj);
		}

		public static decimal? ParseDecimal(this object obj, bool doconvert = true, bool requiredot = false)
		{
			if (obj is decimal m)
				return m;

			if (obj is long l)
				return l;

			if (obj is Keysharp.Scripting.BoolResult br)
				return br.o.ParseDecimal(doconvert, requiredot);

			if (obj is int i)//int is seldom used in Keysharp, so check last.
				return i;

			var s = obj.ToString();
			return requiredot && !s.Contains('.')
				   ? new decimal? ()
				   : decimal.TryParse(s, out m) ? m : doconvert ? Convert.ToDecimal(obj) : new decimal? ();
		}

		public static double? ParseDouble(this object obj, bool doconvert = true, bool requiredot = false)
		{
			if (obj is double d)
				return d;

			if (obj is long l)
				return l;

			if (obj is Keysharp.Scripting.BoolResult br)
				return br.o.ParseDouble(doconvert, requiredot);

			if (obj is int i)//int is seldom used in Keysharp, so check last.
				return i;

			var s = obj.ToString();
			return requiredot && !s.Contains('.')
				   ? new double? ()
				   : double.TryParse(s, out d) ? d : doconvert ? Convert.ToDouble(obj) : new double? ();
		}

		public static int? ParseInt(this object obj, bool doconvert = true)//Need to make everywhere use this.//MATT
		{
			if (obj is int i)
				return i;

			if (obj is Keysharp.Scripting.BoolResult br)
				return br.o.ParseInt(doconvert);

			var s = obj.ToString().AsSpan();

			if (int.TryParse(s, out i))
				return i;

			if (s.Length == 0)
				return new int? ();

			var neg = false;

			if (s[0] == Keysharp.Core.Core.Minus)
			{
				neg = true;
				s = s.Slice(1);
			}

			if (s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
					int.TryParse(s.Slice(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out var ii))
				return neg ? -ii : ii;

			return doconvert ? Convert.ToInt32(obj) : new int? ();
		}

		public static long? ParseLong(this object obj, bool doconvert = true)
		{
			if (obj is long l)
				return l;

			if (obj is Keysharp.Scripting.BoolResult br)
				return br.o.ParseLong(doconvert);

			var s = obj.ToString().AsSpan();

			if (long.TryParse(s, out l))
				return l;

			if (s.Length == 0)
				return new long? ();

			var neg = false;

			if (s[0] == Keysharp.Core.Core.Minus)
			{
				neg = true;
				s = s.Slice(1);
			}

			if (s.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase) &&
					long.TryParse(s.Slice(2), NumberStyles.HexNumber, System.Globalization.CultureInfo.CurrentCulture, out var ii))
				return neg ? -ii : ii;

			return doconvert ? Convert.ToInt64(obj) : new long? ();
		}

		public static object ParseObject(this object obj) => obj is Keysharp.Scripting.BoolResult br ? br.o : obj;

		public static IList Pl(this object[] obj) => obj.Select(x => x).ToList();

		public static string Str(this object obj) => obj != null ? obj.ToString() : "";

		internal static bool IsNotNullOrEmpty(this object ob) => ob != null&& !(ob is string s&& s?.Length == 0);
	}
}