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
		private readonly StringComparer stringComparer;

		/// <summary>
		/// Constructor that takes a case comparison mode.
		/// </summary>
		/// <param name="caseSense">The case comparison mode to use.</param>
		public CaseEqualityComp(eCaseSense caseSense)
		{
			//Choose an appropriate built-in StringComparer.

			stringComparer = caseSense switch
		{
				eCaseSense.On => StringComparer.Ordinal,
				eCaseSense.Off => StringComparer.OrdinalIgnoreCase,
				_ => StringComparer.CurrentCultureIgnoreCase,
		};
	}

	/// <summary>
	/// The implementation for <see cref="IEqualityComparer.Equals"/> which compares two objects.
	/// If both objects are strings, then the case sensitivity mode specified in the constructor is used.
	/// </summary>
	/// <param name="x">The first object to compare.</param>
	/// <param name="y">The second object to compare.</param>
	/// <returns>True if the two objects are equal, else false.</returns>
	public new bool Equals(object x, object y)
		{
			//If both are strings, use the built-in comparer.
			if (x is string s1 && y is string s2)
				return stringComparer.Equals(s1, s2);

			//Otherwise, use default equality.
			return object.Equals(x, y);
		}

		/// <summary>
		/// The implementation for <see cref="IEqualityComparer.GetHashCode(object)"/>.
		/// If the object is a string, a hash code for its lowercase version is returned.
		/// Otherwise obj.GetHashCode() is returned.
		/// </summary>
		/// <param name="obj">The object to get the hash code for.</param>
		/// <returns>The hash code for the object.</returns>
		public int GetHashCode(object obj)
		{
			if (obj is string s)
				return stringComparer.GetHashCode(s);

			return obj?.GetHashCode() ?? 0;
		}
	}

	/// <summary>
	/// Map class that wraps a <see cref="Dictionary{object, object}"/>.
	/// </summary>
	public class Map : KeysharpObject, I__Enum, IEnumerable<(object, object)>, ICollection
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
		public object Capacity
		{
			get => map != null ? map.Capacity : 0L;
			set => map?.EnsureCapacity(value.Ai());
		}

		/// <summary>
		/// Gets or sets the case sensitivity comparison mode for string keys.
		/// </summary>
		public object CaseSense
		{
			get => caseSense.ToString();

			set
			{
				Error err;
				var oldVal = caseSense;
				var str = value.ToString().ToLower();
				var val = Options.OnOff(str);

				if (val != null)
					caseSense = val.IsTrue() ? eCaseSense.On : eCaseSense.Off;
				else if (str == "locale")
					caseSense = eCaseSense.Locale;

				if (map == null)
					return;

				if (Count > 0)
				{
					_ = Errors.ErrorOccurred(err = new PropertyError("Attempted to change case sensitivity of a map which was not empty.")) ? throw err : "";
					return;
				}

				if (caseSense != oldVal)
					map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));
			}
		}

		/// <summary>
		/// Gets the number of elements in the map.
		/// </summary>
		public int Count => map != null ? map.Count : 0;

		/// <summary>
		/// Gets or sets the default value to use when retrieving a value for a key that doesn't exist.
		/// </summary>
		public object Default { get; set; }

		/// <summary>
		/// The implementation for <see cref="KeysharpObject.super"/> for this class to return this type.
		/// </summary>
		public new (Type, object) super => (typeof(KeysharpObject), this);

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
		public Map(params object[] args)
		{
			Init__Item();
			_ = __New(args);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class without creating the __Item dynamic property.
		/// This is needed so Map doesn't enter an infinite recursion loop because dynamic properties themselves have Maps.
		/// See <see cref="__New(object[])"/>.
		/// </summary>
		/// <param name="make__Item">True to create __Item, else false. Always specify false.</param>
		internal Map(bool make__Item, params object[] args) => _ = __New(args);

		/// <summary>
		/// Gets the enumerator object which returns a key,value tuple for each element
		/// </summary>
		/// <param name="count">The number of items each element should contain:<br/>
		///     1: Return the key in the first element, with the second being null.<br/>
		///     2: Return the key in the first element, and the value in the second.
		/// </param>
		/// <returns><see cref="KeysharpEnumerator"/></returns>
		public KeysharpEnumerator __Enum(object count) => new MapKeyValueIterator(map, count.Ai());

		/// <summary>
		/// Initializes a new instance of the <see cref="Map"/> class.
		/// </summary>
		/// <param name="args">An array of values to initialize the map with.<br/>
		/// This can be one of several values:<br/>
		///     null: creates an empty map.<br/>
		///     object[] or <see cref="Array"/>: adds every two elements as a key,value pair to the underlying map.<br/>
		///     <see cref="Map"/>: assigns the map directly to the underlying dictionary as well as the case sense mode.<br/>
		///     <see cref="Dictionary{object, object}"/>: assigns the dictionary directly to the underlying dictionary.
		/// </param>
		/// <returns>Empty string, unused.</returns>
		public new object __New(params object[] args)
		{
			Set(args);
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
		/// The implementation for <see cref="ICollection.CopyTo"/> which copies the keys and values<br/>
		/// of the the map to the passed in <see cref="System.Array"/> as interleaved key,value pairs,<br/>
		/// starting at the passed in index.
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
			Error err;

			if (map.Remove(key, out var val))
				return val;

			return Errors.ErrorOccurred(err = new KeyError($"Key {key} was not present in the map.")) ? throw err : null;
		}

		/// <summary>
		/// Returns the value associated with a key in the following manner:
		///     Return the value associated with key, if found.
		///     Return the value of the default parameter, if specified.
		///     Return the value of this.Default, if defined.
		///     Throw an <see cref="UnsetItemError"/>.
		/// </summary>
		/// <param name="key">They key whose value will be returned.</param>
		/// <param name="default">The value to return if key is not found.</param>
		/// <returns></returns>
		/// <exception cref="UnsetItemError">Throws an <see cref="UnsetItemError"/> if key is not found and no defaults are supplied.</exception>
		public object Get(object key, object @default = null)
		{
			Error err;
			var k = key;
			var def = @default;

			if (TryGetValue(k, out var val))
				return val;

			if (def != null)
				return def;

			if (Default != null)
				return Default;

			return Errors.ErrorOccurred(err = new UnsetItemError($"Key {k} was not present in the map.")) ? throw err : null;
		}

		/// <summary>
		/// The implementation for <see cref="IEnumerable{(object, object)}.GetEnumerator()"/> which returns an <see cref="MapKeyValueIterator"/>.
		/// </summary>
		/// <returns>An <see cref="IEnumerator{(object, object)}"/> which is a <see cref="MapKeyValueIterator"/>.</returns>
		public IEnumerator<(object, object)> GetEnumerator() => new MapKeyValueIterator(map, 2);

		/// <summary>
		/// Returns true if the specified key has an associated value within a map, otherwise false.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <returns>True if key is found, else false.</returns>
		public bool Has(object key) => TryGetValue(key, out _);

		/// <summary>
		/// Returns the greatest integer key in the map.
		/// </summary>
		/// <returns>The greatest integer key if found, else empty string.</returns>
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

		/// <summary>
		/// Returns the least integer key in the map.
		/// </summary>
		/// <returns>The least integer key if found, else empty string.</returns>
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

		/// <summary>
		/// Print every element in the map to the passed in <see cref="StringBuffer"/>.
		/// </summary>
		/// <param name="name">The name to use for this object.</param>
		/// <param name="sbuf">The <see cref="StringBuffer"/> to print to.</param>
		/// <param name="tabLevel">The tab level to use when printing.</param>
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
						key = tempsb.ToString().TrimEnd(CrLf);
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
						val = tempsb.ToString().TrimEnd(CrLf);
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
			var opi = (OwnPropsIterator)OwnProps(true, false);
			tabLevel++;
			indent = new string('\t', tabLevel);

			while (opi.MoveNext())
			{
				var (propName, val) = opi.Current;
				var fieldType = val != null ? val.GetType().Name : "";

				if (val != this && val is KeysharpObject kso2)
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

		/// <summary>
		/// Sets zero or more items.
		/// The items can be either an <see cref="Array"/>, a <see cref="DictionaryBase{object,object}"/>, or an object[].
		/// </summary>
		/// <param name="args">The values to set, arranged as key,value,key2,value2,etc...</param>
		/// <exception cref="ValueError">A <see cref="ValueError"/> exception is thrown if values was not of a supported type.</exception>
		public void Set(params object[] args)
		{
			if (args == null || args.Length == 0)
			{
				if (map == null)
					map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));
				else
					map.Clear();
			}
			else
			{
				Error err;

				if (args.Length == 1)
				{
					if (args[0] is Map m)
					{
						map = m.map;
						caseSense = m.caseSense;
						return;
					}
					else if (args[0] is Dictionary<object, object> dkt)
					{
						map = dkt;
					}
					else if (args[0] is Array temp)
					{
						var count = (temp.Count / 2) * 2;//Do not flatten here because the caller may want a map of maps, or a map of arrays.

						if (map == null)
							map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));

						_ = map.EnsureCapacity(count);

						for (var i = 0; i < count - 1; i += 2)
							Insert(temp.array[i], temp.array[i + 1]);//Access the underlying ArrayList directly for performance.
					}
					else if (args[0] is Dictionary<string, object> tempm)
					{
						if (map == null)
							map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));

						_ = map.EnsureCapacity(tempm.Count);

						foreach (var kv in tempm)
							Insert(kv.Key, kv.Value);
					}
					else
					{
						_ = Errors.ErrorOccurred(err = new ValueError($"Improper object type of {args[0].GetType()} passed to Map constructor.")) ? throw err : "";
						return;
					}
				}
				else
				{
					var count = (args.Length / 2) * 2;

					if (map == null)
						map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));

					for (var i = 0; i < count; i += 2)
						Insert(args[i], args[i + 1]);
				}
			}
		}
		/// <summary>
		/// Returns the string representation of all elements in the map.
		/// </summary>
		/// <returns>The string representation.</returns>
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
		/// <summary>
		/// The implementation for <see cref="IEnumerable.GetEnumerator"/> which just calls <see cref="__Enum"/>.
		/// </summary>
		/// <returns><see cref="MapKeyValueIterator"/></returns>
		IEnumerator IEnumerable.GetEnumerator() => new MapKeyValueIterator(map, 2);
		/// <summary>
		/// Internal helper to insert a key,value pair into the map.
		/// </summary>
		/// <param name="key">The key to insert.</param>
		/// <param name="value">The value to insert.</param>
		private void Insert(object key, object value)
		{
			//if (caseSense != eCaseSense.On && key is string s)
			//  map[s.ToLower()] = value;
			//else
			map[key] = value;
		}
		/// <summary>
		/// Internal helper to wrap <see cref="Dictionary{object,object}.TryGetValue(object, out object)"/>.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">The value found.</param>
		/// <returns>True if key was found else false.</returns>
		private bool TryGetValue(object key, out object value) => map.TryGetValue(key, out value);
		/// <summary>
		/// Indexer which retrieves or sets the value of an array element.
		/// </summary>
		/// <param name="index">The index to get or set.</param>
		/// <returns>The value at the index.</returns>
		/// <exception cref="IndexError">An <see cref="IndexError"/> exception is thrown if index is zero or out of range.</exception>
		/// <summary>
		/// Indexer which retrieves or sets the value of an key.
		/// </summary>
		/// <param name="key">They key to search for.</param>
		/// <returns>The value if found, else <see cref="Default"/> if specified.</returns>
		/// <exception cref="UnsetItemError">An <see cref="UnsetItemError"/> exception is thrown if key is not found and <see cref="Default"/> is not specified.</exception>
		public object this[object key]
		{
			get
			{
				Error err;

				if (TryGetValue(key, out var val))
					return val;

				return Default ?? (Errors.ErrorOccurred(err = new UnsetItemError($"Key {key} was not present in the map.")) ? throw err : null);
			}

			set => Insert(key, value);
		}
	}

	/// <summary>
	/// A two component iterator for <see cref="Map"/> which returns the key and the value as a tuple.
	/// </summary>
	internal class MapKeyValueIterator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		/// <summary>
		/// Cache for iterators with either 1 or 2 parameters.
		/// This prevents reflection from having to always be done to find the Call method.
		/// </summary>
		private static FuncObj p1, p2;

		/// <summary>
		/// The internal map to be iterated over.
		/// </summary>
		private readonly Dictionary<object, object> map;

		/// <summary>
		/// The iterator for the map.
		/// </summary>
		protected IEnumerator<KeyValuePair<object, object>> iter;

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Current"/> which gets the key,value tuple at the current iterator position.
		/// </summary>
		public (object, object) Current
		{
			get
			{
				try
				{
					var kv = iter.Current;

					if (Count == 1)
						return (kv.Key, null);
					else
						return (kv.Key, kv.Value);
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();//Should never happen when using regular loops.
				}
			}
		}

		/// <summary>
		/// The <see cref="IEnumerator.Current"/> implementation that just returns <see cref="Current"/>.
		/// </summary>
		object IEnumerator.Current => Current;

		/// <summary>
		/// Initializes a new instance of the <see cref="MapKeyValueIterator"/> class.
		/// </summary>
		/// <param name="m">The <see cref="Dictionary{object,object}"/> to iterate over.</param>
		/// <param name="c">The number of items to return for each iteration.</param>
		public MapKeyValueIterator(Dictionary<object, object> m, int c)
			: base(null, c)
		{
			map = m;
			iter = map.GetEnumerator();
			var p = c <= 1 ? p1 : p2;
			var fo = (FuncObj)p.Clone();
			fo.Inst = this;
			CallFunc = fo;
		}

		/// <summary>
		/// Static constructor to initialize function objects.
		/// </summary>
		static MapKeyValueIterator()
		{
			Error err;
			var mi1 = Reflections.FindAndCacheMethod(typeof(MapKeyValueIterator), "Call", 1);
			p1 = new FuncObj(mi1, null);

			if (!p1.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";

			var mi2 = Reflections.FindAndCacheMethod(typeof(MapKeyValueIterator), "Call", 2);
			p2 = new FuncObj(mi2, null);

			if (!p2.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";
		}

		/// <summary>
		/// Calls <see cref="Current"/> and places the key value in the passed in object reference.
		/// </summary>
		/// <param name="key">A reference to the key value.</param>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public override object Call(ref object key)
		{
			if (MoveNext())
			{
				(key, _) = Current;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Calls <see cref="Current"/> and places the key and value in the passed in object references.
		/// </summary>
		/// <param name="key">A reference to the key value.</param>
		/// <param name="value">A reference to the object value.</param>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public override object Call(ref object key, ref object value)
		{
			if (MoveNext())
			{
				(key, value) = Current;
				return true;
			}

			return false;
		}

		/// <summary>
		/// The implementation for <see cref="IComparer.Dispose"/> which internally resets the iterator.
		/// </summary>
		public void Dispose() => Reset();

		/// <summary>
		/// The implementation for <see cref="IEnumerator.MoveNext"/> which moves the iterator to the next position.
		/// </summary>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public bool MoveNext() => iter.MoveNext();

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Reset"/> which resets the iterator.
		/// </summary>
		public void Reset() => iter = map.GetEnumerator();
	}

	/// <summary>
	/// The different case comparison modes used in <see cref="Map"/>.
	/// </summary>
	public enum eCaseSense
	{
		On,
		Off,
		Locale
	}
}