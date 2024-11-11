namespace Keysharp.Core
{
	/// <summary>
	/// A comparer which allows the caller to specify the case comparison mode for comparing strings.
	/// This is used in <see cref="Map"/>.
	/// </summary>
	public class CaseEqualityComp : IEqualityComparer<object>
	{
		/// <summary>
		/// The comparison type.
		/// </summary>
		private readonly StringComparison compType;

		/// <summary>
		/// Constructor that takes a case comparison mode.
		/// </summary>
		/// <param name="caseSense">The case comparison mode to use.</param>
		public CaseEqualityComp(eCaseSense caseSense)
		{
			if (caseSense == eCaseSense.On)
				compType = StringComparison.Ordinal;
			else if (caseSense == eCaseSense.Off)
				compType = StringComparison.OrdinalIgnoreCase;
			else
				compType = StringComparison.CurrentCulture;
		}

		/// <summary>
		/// The implementation for <see cref="IEqualityComparer.Equals"/> which compares two objects.
		/// If both objects are strings, then the case sensitivity mode specified in the constructor is used.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>True if the two objects are equal, else false.</returns>
		public new bool Equals(object x, object y)// => x is not null&& y is not null&& string.Compare(x.ToString(), y.ToString(), compType) == 0;
		{
			if (x is string ls && y is string rs)
				return string.Compare(ls, rs, compType) == 0;

			if (ReferenceEquals(x, y))
				return true;

			return x.Equals(y);
		}

		/// <summary>
		/// The implementation for <see cref="IEqualityComparer.GetHashCode(object)"/>.
		/// If the object is a string, a hash code for its lowercase version is returned.
		/// Otherwise obj.GetHashCode() is returned.
		/// </summary>
		/// <param name="obj">The object to get the hash code for.</param>
		/// <returns>The hash code for the object.</returns>
		public int GetHashCode(object obj) => obj is string s ? s.ToLower().GetHashCode() : obj.GetHashCode();
	}

	/// <summary>
	/// Map class that wraps a <see cref="Dictionary{object, object}"/>.
	/// </summary>
	public class Map : KeysharpObject, IEnumerable<(object, object)>, ICollection
	{
		/// <summary>
		/// The underlying <see cref="Dictionary"/> that holds the values.
		/// </summary>
		internal Dictionary<object, object> map;

		/// <summary>
		/// The case comparison to use for string keys.
		/// </summary>
		private eCaseSense caseSense = eCaseSense.On;

		/// <summary>
		/// Gets or sets the capacity of the map.
		/// </summary>
		public long Capacity
		{
			get => map.Capacity;
			set => map.EnsureCapacity((int)value);
		}

		/// <summary>
		/// Gets or sets the case sensitivity comparison mode for string keys.
		/// </summary>
		public string CaseSense
		{
			get => caseSense.ToString();

			set
			{
				if (Count > 0)
					throw new PropertyError("Attempted to change case sensitivity of a map which was not empty.");

				var oldval = caseSense;
				var str = value.ToLower();
				var val = Options.OnOff(str);

				if (val != null)
					caseSense = val.IsTrue() ? eCaseSense.On : eCaseSense.Off;
				else if (str == "locale")
					caseSense = eCaseSense.Locale;

				if (caseSense != oldval)
					map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));
			}
		}

		/// <summary>
		/// Gets the number of elements in the map.
		/// </summary>
		public int Count => map.Count;

		/// <summary>
		/// Gets or sets the default value to use when retrieving a value for a key that doesn't exist.
		/// </summary>
		public object Default { get; set; }

		/// <summary>
		/// Gets a value indicating whether synchronized.
		/// </summary>
		bool ICollection.IsSynchronized => ((ICollection)map).IsSynchronized;

		/// <summary>
		/// The implementation for <see cref="ICollection.SyncRoot"/> which just calls map.SyncRoot.
		/// </summary>
		object ICollection.SyncRoot => ((ICollection)map).SyncRoot;

		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class.
		/// See <see cref="__New(object[])"/>.
		/// </summary>
		public Map(params object[] obj) => _ = __New(obj);

		/// <summary>
		/// Gets the enumerator object which returns a key,value tuple for each element
		/// </summary>
		/// <returns><see cref="IEnumerator{(object, object)}"/></returns>
		public IEnumerator<(object, object)> __Enum() => ((IEnumerable<(object, object)>)this).GetEnumerator();

		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class.
		/// </summary>
		/// <param name="values">An array of values to initialize the map with.<br/>
		/// This can be one of several values:<br/>
		///     null: creates an empty map.<br/>
		///     object[] or <see cref="Array"/>: adds every two elements as a key,value pair to the underlying map.<br/>
		///     <see cref="Map"/>: assigns the map directly to the underlying dictionary as well as the case sense mode.<br/>
		///     <see cref="Dictionary{object, object}"/>: assigns the dictionary directly to the underlying dictionary.
		/// </param>
		/// <returns>Empty string, unused.</returns>
		public override object __New(params object[] values)
		{
			if (values == null || values.Length == 0)
			{
				map = [];
			}
			else
			{
				if (values.Length == 1)
				{
					if (values[0] is Map m)
					{
						map = m.map;
						caseSense = m.caseSense;
						return "";
					}
					else if (values[0] is Dictionary<object, object> dkt)
					{
						map = dkt;
						return "";
					}
				}

				Set(values);
			}

			return "";
		}

		/// <summary>
		/// Clears all elements from the map.
		/// </summary>
		public void Clear() => map.Clear();

		/// <summary>
		/// Returns whether the map contains the specified key.
		/// </summary>
		/// <param name="item">They key to search for.</param>
		/// <returns>True if the map contains the key, else false.</returns>
		public bool Contains(object item) => map.ContainsKey(item);

		/// <summary>
		/// The implementation for <see cref="ICollection.CopyTo"/> which copies the elements<br/>
		/// of the this array to the passed in <see cref="System.Array"/>, starting at the passed in index.
		/// </summary>
		/// <param name="array">The <see cref="System.Array"/> to copy elements to.</param>
		/// <param name="index">The index in the array to start copying to.</param>
		public void CopyTo(System.Array array, int index)
		{
			var kvs = new List<object>(map.Count * 2);

			foreach (var kv in map)
			{
				kvs.Add(kv.Key);
				kvs.Add(kv.Value);
			}

			for (int i = index, j = 0; i < array.Length && j < kvs.Count; i++, j++)
				array.SetValue(kvs[j], i);
		}

		/// <summary>
		/// Removes a key-value pair from the map.
		/// </summary>
		/// <param name="key">They key to remove</param>
		/// <returns>The removed value if found.</returns>
		/// <exception cref="KeyError">An <see cref="KeyError"/> exception is thrown if they key was not found.</exception>
		public object Delete(object key)
		{
			var k = key is string s && caseSense != eCaseSense.On ? s.ToLower() : key;
			return map.Remove(k, out var val)
				   ? val
				   : throw new KeyError($"Key {k} was not present in the map.");
		}

		/// <summary>
		/// Returns the value associated with a key, or a default value.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="default"></param>
		/// <returns></returns>
		/// <exception cref="KeyError"></exception>
		public object Get(object key, object @default = null)
		{
			var k = key;
			var def = @default;

			if (TryGetValue(k, out var val))
				return val;

			if (def != null)
				return def;

			if (Default != null)
				return Default;

			throw new KeyError($"Key {k} was not present in the map.");
		}

		public IEnumerator<(object, object)> GetEnumerator() => new MapKeyValueIterator(map);

		public bool Has(object obj) => TryGetValue(obj, out _);

		public object MaxIndex()
		{
			var val = long.MinValue;

			foreach (var el in map)
			{
				var temp = el.Key.Al();

				if (temp > val)
					val = temp;
			}

			return val != long.MinValue ? val : string.Empty;
		}

		public object MinIndex()
		{
			var val = long.MaxValue;

			foreach (var el in map)
			{
				var temp = el.Key.Al();

				if (temp < val)
					val = temp;
			}

			return val != long.MaxValue ? val : string.Empty;
		}

		public override void PrintProps(string name, StringBuffer sbuf, ref int tabLevel)
		{
			var sb = sbuf.sb;
			var indent = new string('\t', tabLevel);

			if (map.Count > 0)
			{
				var i = 0;

				if (name.Length == 0)
					_ = sb.Append($"{indent}\t{{");
				else
					_ = sb.Append(indent + name + ": " + "\t {");//Need to put this in multiple steps because the AStyle formatter misinterprets it.

				foreach (var kv in map)
				{
					string key;

					if (kv.Key is string ks)
						key = "\"" + ks + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
					else if (kv.Key is KeysharpObject kso)
					{
						var tempsb = new StringBuffer();
						tabLevel++;
						_ = sb.AppendLine();
						kso.PrintProps("", tempsb, ref tabLevel);
						key = tempsb.ToString().TrimEnd(Keywords.CrLf);
						tabLevel--;
					}
					else
						key = kv.Key.ToString();

					string val;

					if (kv.Value is string vs)
						val = "\"" + vs + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
					else if (kv.Value is KeysharpObject kso)
					{
						var tempsb = new StringBuffer();
						tabLevel++;
						indent = new string('\t', tabLevel);
						key = indent + key;//Indent the line if it's an object.
						_ = sb.AppendLine();
						kso.PrintProps("", tempsb, ref tabLevel);
						val = tempsb.ToString().TrimEnd(Keywords.CrLf);
						tabLevel--;
					}
					else if (kv.Value is null)
						val = "null";
					else
						val = kv.Value.ToString();

					if (i < map.Count - 1)
						_ = sb.Append($"{key}: {val}, ");
					else
						_ = sb.Append($"{key}: {val}");

					i++;
				}
				_ = sb.AppendLine($"}} ({GetType().Name})");
			}
			else
			{
				if (name.Length == 0)
					_ = sb.Append($"{indent} {{}} ({GetType().Name})");
				else
					_ = sb.AppendLine($"{indent}{name}: {{}} ({GetType().Name})");
			}
			var opi = (OwnPropsIterator)OwnProps(true, false, true);
			tabLevel++;
			indent = new string('\t', tabLevel);

			while (opi.MoveNext())
			{
				var (propName, val) = opi.Current;
				var fieldType = val != null ? val.GetType().Name : "";

				if (val is KeysharpObject kso2)
				{
					kso2.PrintProps(propName.ToString(), sbuf, ref tabLevel);
				}
				else if (val != null)
				{
					if (val is string vs)
					{
						var str = "\"" + vs + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
						_ = sb.AppendLine($"{indent}{propName}: {str} ({fieldType})");
					}
					else
						_ = sb.AppendLine($"{indent}{propName}: {val} ({fieldType})");
				}
				else
					_ = sb.AppendLine($"{indent}{propName}: null");
			}
			tabLevel--;
		}

		public void Set(params object[] values)
		{
			if (values.Length == 1)
			{
				if (values[0] is Array temp)
				{
					var count = (temp.Count / 2) * 2;//Do not flatten here because the caller may want a map of maps, or a map of arrays.
					_ = map.EnsureCapacity(count);

					for (var i = 0; i < count - 1; i += 2)
						Insert(temp.array[i], temp.array[i + 1]);//Access the underlying ArrayList directly for performance.
				}
				else if (values[0] is Dictionary<string, object> tempm)
				{
					_ = map.EnsureCapacity(tempm.Count);

					foreach (var kv in tempm)
						Insert(kv.Key, kv.Value);
				}
				else
					throw new ValueError($"Improper object type of {values[0].GetType()} passed to Map constructor.");
			}
			else
			{
				var count = (values.Length / 2) * 2;

				for (var i = 0; i < count; i += 2)
					Insert(values[i], values[i + 1]);
			}
		}
		public override string ToString()
		{
			if (map.Count > 0)
			{
				var sb = new StringBuilder(map.Count * 10);
				_ = sb.Append('{');
				var i = 0;

				foreach (var kv in map)
				{
					string key;

					if (kv.Key is string ks)
						key = "\"" + ks + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
					else
						key = kv.Key.ToString();

					string val;

					if (kv.Value is string vs)
						val = "\"" + vs + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
					else
						val = kv.Value.ToString();

					if (i < map.Count - 1)
						_ = sb.Append($"{key}: {val}, ");
					else
						_ = sb.Append($"{key}: {val}");

					i++;
				}

				_ = sb.Append('}');
				return sb.ToString();
			}
			else
				return "{}";
		}
		IEnumerator IEnumerable.GetEnumerator() => __Enum();
		private void Insert(object key, object value)
		{
			if (caseSense != eCaseSense.On && key is string s)
				map[s.ToLower()] = value;
			else
				map[key] = value;
		}
		private bool TryGetValue(object key, out object value)
		{
			if (caseSense != eCaseSense.On && key is string s)
			{
				if (caseSense == eCaseSense.Off)
				{
					if (map.TryGetValue(s.ToLower(), out var val))
					{
						value = val;
						return true;
					}
				}
				else if (caseSense == eCaseSense.Locale)//By far the slowest.
				{
					foreach (var kv in map)
					{
						if (string.Compare(s, kv.Key.ToString(), StringComparison.CurrentCultureIgnoreCase) == 0)
						{
							value = kv.Value;
							return true;
						}
					}
				}
			}
			else
				return map.TryGetValue(key, out value);

			value = null;
			return false;
		}
		public object this[object key]
		{
			get
			{
				if (TryGetValue(key, out var val))
					return val;

				return Default ?? throw new UnsetItemError($"Key {key} was not present in the map.");
			}

			set => Insert(key, value);
		}
	}

	/// <summary>
	/// The map key value iterator.
	/// </summary>
	internal class MapKeyValueIterator : IEnumerator<(object, object)>
	{
		/// <summary>
		/// The map.
		/// </summary>
		private readonly Dictionary<object, object> map;
		/// <summary>
		/// The iter.
		/// </summary>
		private IEnumerator<KeyValuePair<object, object>> iter;

		/// <summary>
		/// Gets the current.
		/// </summary>
		public (object, object) Current
		{
			get
			{
				try
				{
					var kv = iter.Current;
					return (kv.Key, kv.Value);
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		/// <summary>
		/// Gets the current.
		/// </summary>
		object IEnumerator.Current => Current;

		/// <summary>
		/// Initializes a new instance of the <see cref="MapKeyValueIterator"/> class.
		/// </summary>
		/// <param name="m">The M.</param>
		public MapKeyValueIterator(Dictionary<object, object> m)
		{
			map = m;
			iter = map.GetEnumerator();
		}

		public void Call(ref object obj0) => (obj0, _) = Current;

		public void Call(ref object obj0, ref object obj1) => (obj0, obj1) = Current;

		public void Dispose() => Reset();

		public bool MoveNext() => iter.MoveNext();

		public void Reset() => iter = map.GetEnumerator();

		private IEnumerator<(object, object)> GetEnumerator() => this;
	}

	/// <summary>
	/// The es case senses.
	/// </summary>
	public enum eCaseSense
	{
		On,
		Off,
		Locale
	}
}