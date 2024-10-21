namespace Keysharp.Core
{
	public class Types
	{
		public static long HasBase(object obj0, object obj1) => obj1.GetType().IsAssignableFrom(obj0.GetType()) ? 1L : 0L;

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsAlnum(object obj)
		{
			var s = obj.As();
			return s?.Length == 0 || s.All(ch => char.IsLetter(ch) || char.IsNumber(ch)) ? 1 : 0;
		}

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsAlpha(object obj)
		{
			var s = obj.As();
			return s?.Length == 0 || s.All(char.IsLetter) ? 1 : 0;
		}

		public static long IsDate(object obj) => IsTime(obj);

		public static long IsDigit(object obj)
		{
			var s = obj.As();
			return s?.Length == 0 || s.All(char.IsDigit) ? 1 : 0;
		}

		public static long IsFloat(object obj)
		{
			var o = obj;

			if (o is double || o is float || o is decimal)
				return 1;

			var val = o.ParseDouble(false, true);
			return val.HasValue ? 1 : 0;
		}

		public static long IsFunc(object obj0, object obj1 = null) => Reflections.FindMethod(obj0.ToString(), obj1.Ai(-1)) is MethodPropertyHolder mph && mph.mi != null ? 1L : 0L;

		public static long IsInteger(object obj)
		{
			var o = obj;

			if (o is long || o is int || o is uint || o is ulong)
				return 1L;

			if (o is double || o is float || o is decimal)
				return 0L;

			var val = o.ParseLong(false);
			return val.HasValue ? 1L : 0L;
		}

		public static long IsLabel(object name) => throw new Error("C# does not allow querying labels at runtime.");

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsLower(object obj)
		{
			var s = obj.As();
			return s?.Length == 0 || s.All(ch => char.IsLetter(ch) && char.IsLower(ch)) ? 1 : 0;
		}

		public static long IsNumber(object obj) => IsInteger(obj) | IsFloat(obj);

		public static long IsObject(object obj) => obj is KeysharpObject ? 1 : 0;

		public static long IsSet(object obj) => obj != UnsetArg.Default&& obj != null ? 1 : 0;

		public static long IsSpace(object obj) => obj.ToString().AsSpan().IndexOfAnyExcept(Keywords.SpacesSv) != -1 ? 0L : 1L;

		public static long IsTime(object obj)
		{
			var s = obj.As();
			DateTime dt;

			try
			{
				dt = Conversions.ToDateTime(s);

				if (dt == DateTime.MinValue)
					return 0L;
			}
			catch
			{
				return 0L;
			}

			int[] t = [DateTime.Now.Year / 100, DateTime.Now.Year % 100, 1, 1, 0, 0, 0, 0];
			var tempdt = new DateTime(t[1], t[2], t[3], t[4], t[5], t[6], CultureInfo.CurrentCulture.Calendar);//Will be wrong this if parsing totally failed.
			return dt != tempdt ? 1L : 0L;
		}

		/// <summary>
		/// Differs in that locale is always considered.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static long IsUpper(object obj)
		{
			var s = obj.As();
			return s?.Length == 0 || s.All(ch => char.IsLetter(ch) && char.IsUpper(ch)) ? 1 : 0;
		}

		public static long IsXDigit(object obj)
		{
			var s = obj.As();
			var sp = s.AsSpan();

			if (sp.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				sp = sp.Slice(2);

			foreach (var ch in sp)
				if (!((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f')))
					return 0L;

			return 1L;
		}

		public static string Type(object t) => t.GetType().Name;
	}
}