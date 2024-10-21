namespace System
{
	public static class ObjectExtensions
	{
		public static bool Ab(this object obj, bool def = default) => obj != null ? obj.ParseBool() ?? def : def;

		public static double Ad(this object obj, double def = default) => obj != null ? obj.ParseDouble().Value : def;

		public static float Af(this object obj, float def = default) => obj != null ? obj.ParseFloat().Value : def;

		public static int Ai(this object obj, int def = default) => obj != null ? obj.ParseInt().Value : def;

		public static long Al(this object obj, long def = default) => obj != null ? obj.ParseLong().Value : def;

		public static string As(this object obj, string def = "") => obj != null ? obj.ToString() : def;

		public static uint Aui(this object obj, uint def = default) => obj != null ? obj.ParseUInt().Value : def;

		public static T CastTo<T>(this object o) => (T)o;

		public static object CastToReflected(this object o, Type type)
		{
			var methodInfo = typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
			var genericArguments = new[] { type };
			var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
			return genericMethodInfo?.Invoke(null, [o]);
		}

		public static T[] Concat<T>(this T[] x, T[] y)
		{
			if (x == null) throw new ArgumentNullException("x");

			if (y == null) throw new ArgumentNullException("y");

			var oldLen = x.Length;
			Array.Resize(ref x, x.Length + y.Length);
			Array.Copy(y, 0, x, oldLen, y.Length);
			return x;
		}

		public static Control GetControl(this object obj)
		{
			if (obj is Gui gui)
				return gui.form;
			else if (obj is GuiControl ctrl)
				return ctrl.Control;
			else if (obj is Menu menu)
				return menu.GetMenu();
			else if (obj is Control control)//Final check in the event it's some kind of native control or form.
				return control;

			return null;
		}

		public static bool IsAlmostEqual(this double d, double comparison) => d.IsAlmostEqual(comparison, 0.00001);

		public static bool IsAlmostEqual(this double d, double comparison, double tolerance) => (d - comparison).IsAlmostZero(tolerance);

		public static bool IsAlmostZero(this double d, double tolerance) => d > -tolerance&& d < tolerance;

		public static bool IsCallbackResultNonEmpty(this object result) => result != null&& ((result.ParseLong(false) is long l&& l != 0) || result.ParseBool().IsTrue() || (result is string s&& s != ""));

		public static bool IsFalse(this bool? b) => b.HasValue&& !b.Value;

		public static bool IsKeysharpGui(this object obj) => obj is Gui || obj is GuiControl || obj is Menu;

		public static bool IsNotNullOrEmpty(this object obj) => obj != null&& !(obj is string s&& s?.Length == 0);

		public static bool IsNullOrEmpty(this object obj) => obj == null ? true : obj is string s ? s?.Length == 0 : false;

		public static bool IsTrue(this bool? b) => b.HasValue&& b.Value;

		public static IList L(this object[] obj) => obj.Flatten().Cast<object>().ToList();

		public static IList L(this IEnumerable obj) => obj.Flatten().Cast<object>().ToList();

		public static bool? ParseBool(this object obj)
		{
			if (obj is bool b)
				return b;

			if (obj is BoolResult br)
				return br.o.ParseBool();

			return Options.OnOff(obj);
		}

		public static decimal? ParseDecimal(this object obj, bool doconvert = true, bool requiredot = false)
		{
			if (obj is decimal m)
				return m;

			if (obj is long l)
				return l;

			if (obj is BoolResult br)
				return br.o.ParseDecimal(doconvert, requiredot);

			if (obj is int i)//int is seldom used in Keysharp, so check last.
				return i;

			var s = obj.ToString().AsSpan();

			if (s.Length == 0)
				return new decimal? ();

			if (requiredot && !s.Contains('.'))
				return new decimal? ();

			if (decimal.TryParse(s, out m))
				return m;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a double like "123.0D".
				if (decimal.TryParse(s.Slice(0, s.Length - 1), out m))
					return m;

			if (doconvert)
				return Convert.ToDecimal(obj);

			return new decimal? ();
		}

		public static double? ParseDouble(this object obj, bool doconvert = true, bool requiredot = false)
		{
			if (obj is double d)
				return d;

			if (obj is long l)
				return l;

			if (obj is BoolResult br)
				return br.o.ParseDouble(doconvert, requiredot);

			if (obj is int i)//int is seldom used in Keysharp, so check last.
				return i;

			var s = obj.ToString().AsSpan();

			if (s.Length == 0)
				return new double? ();

			if (requiredot && !s.Contains('.'))
				return new double? ();

			if (double.TryParse(s, out d))
				return d;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a double like "123.0D".
				if (double.TryParse(s.Slice(0, s.Length - 1), out d))
					return d;

			if (doconvert)
				return Convert.ToDouble(obj);

			return new double? ();
		}

		public static float? ParseFloat(this object obj, bool doconvert = true, bool requiredot = false)
		{
			if (obj is float d)
				return d;

			if (obj is long l)
				return l;

			if (obj is BoolResult br)
				return br.o.ParseFloat(doconvert, requiredot);

			if (obj is int i)//int is seldom used in Keysharp, so check last.
				return i;

			var s = obj.ToString().AsSpan();

			if (s.Length == 0)
				return new float? ();

			if (requiredot && !s.Contains('.'))
				return new float? ();

			if (float.TryParse(s, out d))
				return d;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a double like "123.0D".
				if (float.TryParse(s.Slice(0, s.Length - 1), out d))
					return d;

			if (doconvert)
				return (float)Convert.ToDouble(obj);

			return new float? ();
		}

		public static int? ParseInt(this object obj, bool doconvert = true, bool donoprefixhex = true)
		{
			if (obj is int i)
				return i;

			if (obj is long l)
				return (int)l;

			if (obj is BoolResult br)
				return br.o.ParseInt(doconvert);

			var s = obj.ToString().AsSpan();

			if (s.Length == 0)
				return new int? ();

			if (int.TryParse(s, out i))
				return i;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a int like "123I".
				if (int.TryParse(s.Slice(0, s.Length - 1), out i))
					return i;

			var neg = false;

			if (s[0] == Keywords.Minus)
			{
				neg = true;
				s = s.Slice(1);
			}

			if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
					int.TryParse(s.Slice(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var ii))
				return neg ? -ii : ii;

			if (donoprefixhex)
				if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ii))
					return neg ? -ii : ii;

			return doconvert ? Convert.ToInt32(obj) : new int? ();
		}

		public static long? ParseLong(this object obj, bool doconvert = true, bool donoprefixhex = true)
		{
			if (obj is long l)
				return l;

			if (obj is BoolResult br)
				return br.o.ParseLong(doconvert);

			var s = obj.ToString().AsSpan();

			if (s.Length == 0)
				return new long? ();

			if (long.TryParse(s, out l))
				return l;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a long like "123L".
				if (long.TryParse(s.Slice(0, s.Length - 1), out l))
					return l;

			var neg = false;

			if (s[0] == Keywords.Minus)
			{
				neg = true;
				s = s.Slice(1);
			}

			if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
					long.TryParse(s.Slice(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var ii))
				return neg ? -ii : ii;

			if (donoprefixhex)
				if (long.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ii))
					return neg ? -ii : ii;

			return doconvert ? Convert.ToInt64(obj) : new long? ();
		}

		public static object ParseObject(this object obj) => obj is BoolResult br ? br.o : obj;

		public static uint? ParseUInt(this object obj, bool doconvert = true, bool donoprefixhex = true)
		{
			if (obj is uint i)
				return i;

			if (obj is BoolResult br)
				return br.o.ParseUInt(doconvert);

			var s = obj.ToString().AsSpan();

			if (s.Length == 0)
				return new uint? ();

			if (uint.TryParse(s, out i))
				return i;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a uint like "123U".
				if (uint.TryParse(s.Slice(0, s.Length - 1), out i))
					return i;

			if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
					uint.TryParse(s.Slice(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var ii))
				return ii;

			if (donoprefixhex)
				if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out ii))
					return ii;

			return doconvert ? Convert.ToUInt32(obj) : new uint? ();
		}

		public static IList Pl(this object[] obj) => obj.Select(x => x).ToList();

		public static string Str(this object obj) => obj != null ? obj.ToString() : "";
	}
}