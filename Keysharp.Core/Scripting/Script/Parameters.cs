namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static object[] CombineParams(params object[] obj)
		{
			var l = new List<object>(obj.Length * 3);

			for (var i = 0; i < obj.Length; i += 2)
			{
				if (obj[i] is bool b && b)
				{
					if (obj[i + 1] is IEnumerable en)
						l.AddRange(en.Flatten().Cast<object>());
					else
						l.Add(obj[i + 1]);
				}
				else
					l.Add(obj[i + 1]);
			}

			return l.ToArray();
		}

		public static object[] FlattenParam(object obj)
		{
			if (obj is IEnumerable en)
			{
				var l = new List<object>();
				l.AddRange(en.Flatten().Cast<object>());
				return l.ToArray();
			}
			else
				return new object[] { obj };
		}

		public static object Parameter(object[] values, object def, int index) => index < values.Length ? values[index] : def;

		public static void Parameters(string[] names, object[] values, object[] defaults)
		{
			for (var i = 0; i < names.Length; i++)
			{
				var init = i < values.Length ? values[i] : i < defaults.Length ? defaults[i] : null;
				Vars[names[i]] = init;
			}
		}
	}
}