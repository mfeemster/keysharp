namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static object[] FlattenParam(object obj)
		{
			if (obj is IEnumerable en)
			{
				var l = new List<object>();
				l.AddRange(en.Flatten(false).Cast<object>());
				return l.ToArray();
			}
			else if (obj is IEnumerator<(object, object)> ieoo)
			{
				var l = new List<object>();

				while (ieoo.MoveNext())
					l.Add(ieoo.Current.Item1);

				return l.ToArray();
			}
			else if (Loops.MakeEnumerator(obj, 1L) is KeysharpEnumerator ke)
			{
				var l = new List<object>();
				object v1 = null;

				while (ke.Call(ref v1).IsCallbackResultNonEmpty())
					l.Add(v1);

				return l.ToArray();
			}
			else
				return [obj];
		}

		public static object Parameter(object[] values, object def, int index) => index < values.Length ? values[index] : def;

		public static void Parameters(string[] names, object[] values, object[] defaults)
		{
			for (var i = 0; i < names.Length; i++)
			{
				var init = i < values.Length ? values[i] : i < defaults.Length ? defaults[i] : null;
				Script.TheScript.Vars[names[i]] = init;
			}
		}
	}
}