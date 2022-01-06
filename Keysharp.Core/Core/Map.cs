using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Keysharp.Core
{
	public class CaseEqualityComp : IEqualityComparer<object>
	{
		private StringComparison compType;

		public CaseEqualityComp(eCaseSense caseSense)
		{
			if (caseSense == eCaseSense.On)
				compType = StringComparison.Ordinal;
			else if (caseSense == eCaseSense.Off)
				compType = StringComparison.OrdinalIgnoreCase;
			else
				compType = StringComparison.CurrentCulture;
		}

		public new bool Equals(object x, object y) => x is not null&& y is not null&& string.Compare(x.ToString(), y.ToString(), compType) == 0;

		public int GetHashCode(object obj) => obj.GetHashCode();
	}

	//public class Map : KeysharpObject, ICollection<KeyValuePair<object, object>>
	public class Map : KeysharpObject, IEnumerable<(object, object)>, ICollection
	{
		internal Dictionary<object, object> map = new Dictionary<object, object>();

		private eCaseSense caseSense = eCaseSense.On;

		public long Capacity
		{
			get => map.EnsureCapacity(0);
			set => map.EnsureCapacity((int)value);
		}

		public string CaseSense
		{
			get => caseSense.ToString();

			set
			{
				if (map.Count > 0)
					throw new Exception("Attempted to change case sensitivity of a map which was not empty.");

				var str = value.ToLower();
				var val = Options.OnOff(str);

				if (val != null)
					caseSense = val.IsTrue() ? eCaseSense.On : eCaseSense.Off;
				else if (str == "locale")
					caseSense = eCaseSense.Locale;

				map = new Dictionary<object, object>(new CaseEqualityComp(caseSense));
			}
		}

		public object Default { get; set; }

		public int Count => map.Count;

		bool ICollection.IsSynchronized => ((ICollection)map).IsSynchronized;

		object ICollection.SyncRoot => ((ICollection)map).SyncRoot;

		public Map()
		{
		}

		private Map(Dictionary<object, object> dkt) => map = dkt;

		public Map(params object[] values) => Set(values);

		public IEnumerator<(object, object)> __Enum() => ((IEnumerable<(object, object)>)this).GetEnumerator();

		public void __New(params object[] values) => Set(values);

		public void Clear() => map.Clear();

		public override object Clone(params object[] values) => new Map(map.ToDictionary(entry => entry.Key, entry => entry.Value))
		{
			Default = Default,
			Capacity = Capacity
		};


		public bool Contains(object item) => map.ContainsKey(item.ParseObject());

		public void CopyTo(System.Array array, int index)
		{
			//foreach (var kv in map)
			//array[index] = kv;
		}

		public object Delete(params object[] values)
		{
			var k = values[0].ParseObject();//.L()[0];
			var rem = false;
			object ret = null;

			if (k is string s && caseSense != eCaseSense.On)
			{
				s = s.ToLower();
				ret = map[s];
				rem = map.Remove(s);
			}
			else
			{
				ret = map[k];
				rem = map.Remove(k);
			}

			if (!rem)
				throw new Exception($"KeyError: Key {k} was not present in the dictionary. {new StackFrame(0).GetMethod().Name}");

			return ret;
		}

		public object Get(params object[] values)
		{
			var (key, def) = values/*.L()*/.O2();

			if (TryGetValue(key, out var val))
				return val;

			if (def != null)
				return def;

			if (Default != null)
				return Default;

			throw new KeyError($"{key} was not found in the map.");
		}

		public bool Has(params object[] values) => map.ContainsKey(values/*.L()*/[0]);

		public object MaxIndex(params object[] values)
		{
			var val = long.MinValue;

			foreach (var el in map)
			{
				var temp = Convert.ToInt64(el.Key);

				if (temp > val)
					val = temp;
			}

			return val != long.MinValue ? val : (object)string.Empty;
		}

		public object MinIndex(params object[] values)
		{
			var val = long.MaxValue;

			foreach (var el in map)
			{
				var temp = Convert.ToInt64(el.Key);

				if (temp < val)
					val = temp;
			}

			return val != long.MaxValue ? val : (object)string.Empty;
		}

		public void Set(params object[] values)
		{
			IList ic;
			Array arr = null;

			if (values.Length == 1 && values[0].ParseObject() is Array temp)
				ic = arr = temp;
			else
				ic = values;

			var count = (ic.Count / 2) * 2;//Do not flatten here because the caller may want a map of maps, or a map of arrays.
			_ = map.EnsureCapacity(count);

			if (arr != null)
			{
				for (var i = 0; i < count; i += 2)
					Insert(ic[i + 1].ParseObject(), ic[i + 2].ParseObject());
			}
			else
			{
				for (var i = 0; i < count; i += 2)
					Insert(ic[i].ParseObject(), ic[i + 1].ParseObject());
			}
		}

		public IEnumerator<(object, object)> GetEnumerator() => new MapKeyValueIterator(map);

		IEnumerator IEnumerable.GetEnumerator() => __Enum();

		private void Insert(object key, object value)
		{
			if (key.ParseObject() is string s && caseSense != eCaseSense.On)
				map[s.ToLower()] = value.ParseObject();
			else
				map[key.ParseObject()] = value.ParseObject();
		}

		private bool TryGetValue(object key, out object value)
		{
			key = key.ParseObject();

			if (key is string s && caseSense != eCaseSense.On)
			{
				if (caseSense == eCaseSense.Off)
				{
					foreach (var kv in map)
					{
						if (string.Compare(s, kv.Key.ToString(), true) == 0)
						{
							value = kv.Value;
							return true;
						}
					}
				}
				else if (caseSense == eCaseSense.Locale)
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
				key = key.ParseObject();

				if (TryGetValue(key, out var val))
					return val;

				return null;
			}

			set => Insert(key, value);
		}
	}

	public class MapKeyValueIterator : IEnumerator<(object, object)>
	{
		private IEnumerator<KeyValuePair<object, object>> iter;
		private Dictionary<object, object> map;

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

		object IEnumerator.Current => Current;

		public MapKeyValueIterator(Dictionary<object, object> m)
		{
			map = m;
			iter = map.GetEnumerator();
		}

		public void Dispose() => Reset();

		public bool MoveNext() => iter.MoveNext();

		public void Reset() => iter = map.GetEnumerator();

		private IEnumerator<(object, object)> GetEnumerator() => this;
	}

	public enum eCaseSense
	{
		On,
		Off,
		Locale
	}
}