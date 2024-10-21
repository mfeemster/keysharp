namespace Keysharp.Core
{
	public static class Collections
	{
		public static Array Array(params object[] obj)
		{
			if (obj == null || obj.Length == 0)
			{
				var arr = new Array
				{
					Capacity = 64
				};
				return arr;
			}
			else
				return new Array(obj);
		}

		public static Buffer Buffer(object obj0, object obj1 = null) => new (obj0, obj1);

		public static Map Dictionary(object[] keys, object[] values)
		{
			var table = new Map();

			for (var i = 0; i < keys.Length; i++)
			{
				var name = keys[i];
				var entry = i < values.Length ? values[i] : null;

				if (entry == null)
				{
					if (table.Has(name))
						_ = table.Delete(name);
				}
				else
					table[name] = entry;
			}

			return table;
		}

		public static Map Map(params object[] obj) => Objects.Object(obj);
	}
}