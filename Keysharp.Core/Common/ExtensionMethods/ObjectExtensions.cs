namespace System
{
	/// <summary>
	/// Extension methods for the System.Object class.
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Converts an object to a bool.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null or the conversion fails.</param>
		/// <returns>The object as a bool if conversion succeeded, else def.</returns>
		public static bool Ab(this object obj, bool def = default) => obj != null ? obj.ParseBool() ?? def : def;

		/// <summary>
		/// Converts an object to a double.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null.</param>
		/// <returns>The object as a double if it was not null, else def.</returns>
		public static double Ad(this object obj, double def = default) => obj != null ? obj.ParseDouble().Value : def;

		/// <summary>
		/// Converts an object to a float.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null.</param>
		/// <returns>The object as a float if it was not null, else def.</returns>
		public static float Af(this object obj, float def = default) => obj != null ? obj.ParseFloat().Value : def;

		/// <summary>
		/// Converts an object to an int.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null.</param>
		/// <returns>The object as an int if it was not null, else def.</returns>
		public static int Ai(this object obj, int def = default) => obj != null ? obj.ParseInt().Value : def;

		/// <summary>
		/// Converts an object to a long.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null.</param>
		/// <returns>The object as a long if it was not null, else def.</returns>
		public static long Al(this object obj, long def = default) => obj != null ? obj.ParseLong().Value : def;

		/// <summary>
		/// Converts an object to a string.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null.</param>
		/// <returns>The object as a string if it was not null, else def.</returns>
		public static string As(this object obj, string def = "") => obj != null ? obj.ToString() : def;

		/// <summary>
		/// Converts an object to an unsigned int.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="def">A default value to use if obj is null.</param>
		/// <returns>The object as an unsigned int if it was not null, else def.</returns>
		public static uint Aui(this object obj, uint def = default) => obj != null ? obj.ParseUInt().Value : def;

		/// <summary>
		/// Wrapper around casting an object to a type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type to cast the object to.</typeparam>
		/// <param name="obj">The object to cast.</param>
		/// <returns>The object casted to <typeparamref name="T"/>.</returns>
		public static T CastTo<T>(this object obj) => (T)obj;

		/// <summary>
		/// Casts an object to a type using reflection.
		/// </summary>
		/// <param name="obj">The object to cast.</param>
		/// <param name="type">The type to cast obj to.</param>
		/// <returns>obj casted to type.</returns>
		public static object CastToReflected(this object obj, Type type)
		{
			var methodInfo = typeof(ObjectExtensions).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
			var genericArguments = new[] { type };
			var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
			return genericMethodInfo?.Invoke(null, [obj]);
		}

		/// <summary>
		/// Attempts to convert an object to a <see cref="Control"/>.
		/// </summary>
		/// <param name="obj">The object to examine.</param>
		/// <returns>A <see cref="Control"/> if the conversion succeeded, else null.</returns>
		public static Control GetControl(this object obj)
		{
			if (obj is Gui gui)
				return gui.form;
			else if (obj is GuiControl ctrl)
				return ctrl.Control;
			else if (obj is Keysharp.Core.Menu menu)
				return menu.GetMenu();
			else if (obj is Control control)//Final check in the event it's some kind of native control or form.
				return control;

			return null;
		}

		/// <summary>
		/// Returns whether a callback result non-empty.
		/// </summary>
		/// <param name="result">The callback result to examine.</param>
		/// <returns>True if non-empty, else false.</returns>
		public static bool IsCallbackResultNonEmpty(this object result) => result != null&& ((result.ParseLong(false) is long l&& l != 0) || result.ParseBool().IsTrue() || (result is string s&& s != ""));

		/// <summary>
		/// Returns whether an object is a <see cref="Gui"/>, <see cref="GuiControl"/> or <see cref="Menu"/>.
		/// </summary>
		/// <param name="obj">The object to examine.</param>
		/// <returns>True if obj was a <see cref="Gui"/>, <see cref="GuiControl"/> or <see cref="Menu"/>, else false.</returns>
		public static bool IsKeysharpGui(this object obj) => obj is Gui || obj is GuiControl || obj is Keysharp.Core.Menu;

		/// <summary>
		/// Returns whether an object is a string that is not empty.
		/// </summary>
		/// <param name="obj">The obj to examine.</param>
		/// <returns>True if obj was a string that was not empty, else false.</returns>
		public static bool IsNotNullOrEmpty(this object obj) => obj != null&& !(obj is string s&& s?.Length == 0);

		/// <summary>
		/// Returns whether an object is null or an empty string.
		/// </summary>
		/// <param name="obj">The obj to examine.</param>
		/// <returns>True if obj was null or an empty string, else false.</returns>
		public static bool IsNullOrEmpty(this object obj) => obj == null ? true : obj is string s ? s?.Length == 0 : false;

		/// <summary>
		/// Attempt to convert an object to a bool.
		/// This treats 0, "", false, and off as false.
		/// and 1, true and on as true.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <returns>The nullable bool resulting from the conversion.</returns>
		public static bool? ParseBool(this object obj)
		{
			if (obj is bool b)
				return b;

			if (obj is BoolResult br)
				return br.o.ParseBool();

			return Options.OnOff(obj);
		}

		/// <summary>
		/// Attempts various methods for converting an object to a decimal value.<br/>
		/// This will first attempt direct casting because it's the most efficient and the most likely scenario.<br/>
		/// String parsing will be attempted after that, then using the <see cref="Convert"/> class as a final attempt.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="doconvert">Whether to attempt using the <see cref="Convert"/> class if all other attempts fail.</param>
		/// <param name="requiredot">Whether to require a . character in the string when parsing after other attempts have failed.</param>
		/// <returns>The converted value as a nullable decimal.</returns>
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

			var s = obj.ToString().AsSpan().Trim();

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

		/// <summary>
		/// Attempts various methods for converting an object to a double value.<br/>
		/// This will first attempt direct casting because it's the most efficient and the most likely scenario.<br/>
		/// String parsing will be attempted after that, then using the <see cref="Convert"/> class as a final attempt.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="doconvert">Whether to attempt using the <see cref="Convert"/> class if all other attempts fail.</param>
		/// <param name="requiredot">Whether to require a . character in the string when parsing after other attempts have failed.</param>
		/// <returns>The converted value as a nullable double.</returns>
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

			var s = obj.ToString().AsSpan().Trim();

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

		/// <summary>
		/// Attempts various methods for converting an object to a float value.<br/>
		/// This will first attempt direct casting because it's the most efficient and the most likely scenario.<br/>
		/// String parsing will be attempted after that, then using the <see cref="Convert"/> class as a final attempt.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="doconvert">Whether to attempt using the <see cref="Convert"/> class if all other attempts fail.</param>
		/// <param name="requiredot">Whether to require a . character in the string when parsing after other attempts have failed.</param>
		/// <returns>The converted value as a nullable float.</returns>
		public static float? ParseFloat(this object obj, bool doconvert = true, bool requiredot = false)
		{
			if (obj is float d)
				return d;

			if (obj is double dd)//Check for double here, but not the reverse in ParseDouble() because most decimal numbers will be double.
				return (float)dd;

			if (obj is long l)
				return l;

			if (obj is BoolResult br)
				return br.o.ParseFloat(doconvert, requiredot);

			if (obj is int i)//int is seldom used in Keysharp, so check last.
				return i;

			var s = obj.ToString().AsSpan().Trim();

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

		/// <summary>
		/// Attempts various methods for converting an object to a int value.<br/>
		/// This will first attempt direct casting because it's the most efficient and the most likely scenario.<br/>
		/// String parsing will be attempted after that, then using the <see cref="Convert"/> class as a final attempt.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="doconvert">Whether to attempt using the <see cref="Convert"/> class if all other attempts fail.</param>
		/// <param name="donoprefixhex">Whether to treat a hexadecimal string without an 0x prefix as valid.</param>
		/// <returns>The converted value as a nullable int.</returns>
		public static int? ParseInt(this object obj, bool doconvert = true, bool donoprefixhex = true)
		{
			if (obj is int i)
				return i;

			if (obj is long l)
				return (int)l;

			if (obj is BoolResult br)
				return br.o.ParseInt(doconvert);

			var s = obj.ToString().AsSpan().Trim();

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
					int.TryParse(s.Slice(2), NumberStyles.HexNumber, Parser.inv, out var ii))
				return neg ? -ii : ii;

			if (donoprefixhex)
				if (int.TryParse(s, NumberStyles.HexNumber, Parser.inv, out ii))
					return neg ? -ii : ii;

			return doconvert ? Convert.ToInt32(obj) : new int? ();
		}

		/// <summary>
		/// Attempts various methods for converting an object to a long value.<br/>
		/// This will first attempt direct casting because it's the most efficient and the most likely scenario.<br/>
		/// String parsing will be attempted after that, then using the <see cref="Convert"/> class as a final attempt.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="doconvert">Whether to attempt using the <see cref="Convert"/> class if all other attempts fail.</param>
		/// <param name="donoprefixhex">Whether to treat a hexadecimal string without an 0x prefix as valid.</param>
		/// <returns>The converted value as a nullable long.</returns>
		public static long? ParseLong(this object obj, bool doconvert = true, bool donoprefixhex = true)
		{
			if (obj is long l)
				return l;

			if (obj is BoolResult br)
				return br.o.ParseLong(doconvert);

			var s = obj.ToString().AsSpan().Trim();

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
					long.TryParse(s.Slice(2), NumberStyles.HexNumber, Parser.inv, out var ii))
				return neg ? -ii : ii;

			if (donoprefixhex)
				if (long.TryParse(s, NumberStyles.HexNumber, Parser.inv, out ii))
					return neg ? -ii : ii;

			return doconvert ? Convert.ToInt64(obj) : new long? ();
		}

		/// <summary>
		/// Attempts to extract and return the object from a BoolResult if obj is a BoolResult.
		/// </summary>
		/// <param name="obj">The object to examine.</param>
		/// <returns>The .o field of the object if it was a BoolResult, else the object itself.</returns>
		public static object ParseObject(this object obj) => obj is BoolResult br ? br.o : obj;

		/// <summary>
		/// Attempts various methods for converting an object to a uint value.<br/>
		/// This will first attempt direct casting because it's the most efficient and the most likely scenario.<br/>
		/// String parsing will be attempted after that, then using the <see cref="Convert"/> class as a final attempt.
		/// </summary>
		/// <param name="obj">The object to convert.</param>
		/// <param name="doconvert">Whether to attempt using the <see cref="Convert"/> class if all other attempts fail.</param>
		/// <param name="donoprefixhex">Whether to treat a hexadecimal string without an 0x prefix as valid.</param>
		/// <returns>The converted value as a nullable uint.</returns>
		public static uint? ParseUInt(this object obj, bool doconvert = true, bool donoprefixhex = true)
		{
			if (obj is uint i)
				return i;

			if (obj is BoolResult br)
				return br.o.ParseUInt(doconvert);

			var s = obj.ToString().AsSpan().Trim();

			if (s.Length == 0)
				return new uint? ();

			if (uint.TryParse(s, out i))
				return i;

			if (!char.IsNumber(s[s.Length - 1]))//Handle a string specifying a uint like "123U".
				if (uint.TryParse(s.Slice(0, s.Length - 1), out i))
					return i;

			if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
					uint.TryParse(s.Slice(2), NumberStyles.HexNumber, Parser.inv, out var ii))
				return ii;

			if (donoprefixhex)
				if (uint.TryParse(s, NumberStyles.HexNumber, Parser.inv, out ii))
					return ii;

			return doconvert ? Convert.ToUInt32(obj) : new uint? ();
		}

		/// <summary>
		/// Returns the string representation of an object.
		/// </summary>
		/// <param name="obj">The object to examine.</param>
		/// <returns>If obj is not null, the result of calling obj.ToString(), else empty string.</returns>
		public static string Str(this object obj) => obj != null ? obj.ToString() : "";
	}
}