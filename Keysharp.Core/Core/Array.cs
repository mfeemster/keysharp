using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Keysharp.Core
{
	public class Array : KeysharpObject, IEnumerable<(object, object)>, ICollection, IList
	{
		internal List<object> array;

		public object Capacity
		{
			get => (long)array.Capacity;

			set
			{
				var val = value.ParseInt().Value;
				array.Capacity = val < array.Capacity ? array.Count : val;
			}
		}

		public int Count => array.Count;

		public bool IsFixedSize => ((IList)array).IsFixedSize;

		public bool IsReadOnly => ((IList)array).IsReadOnly;

		public object Length
		{
			get => (long)array.Count;

			set
			{
				var i = value.ParseInt(true).Value;

				if (i > array.Count)
				{
					if (array.Capacity < i)
						array.Capacity = i;

					for (var ii = 0; ii < i; ii++)
						if (ii >= array.Count)
							array.Add(null);
				}
			}
		}

		bool ICollection.IsSynchronized => ((ICollection)array).IsSynchronized;

		object ICollection.SyncRoot => ((ICollection)array).SyncRoot;

		public Array(object obj = null)
		{
			if (obj == null)
				array = new List<object>();
			else if (obj is Array arr)
				array = arr.array;
			else if (obj is ICollection c)
				array = new List<object>(c.Cast<object>().ToList());
			else
			{
				var l = obj.Al(-1);

				if (l > 0)
					array = new List<object>((int)l);
				else
					throw new ValueError(obj: $"Invalid value of {obj} passed to Array constructor.");
			}
		}

		public IEnumerator<(object, object)> __Enum() => ((IEnumerable<(object, object)>)this).GetEnumerator();

		public void __New(params object[] values) => Push(values);

		public int Add(object value)
		{
			array.Add(value);
			return array.Count;
		}

		public void AddRange(ICollection c) => array.AddRange(c.Cast<object>());

		public void Clear() => ((IList)array).Clear();

		public override object Clone(params object[] values) => new Array(array.ToArray());

		public bool Contains(object value) => ((IList)array).Contains(value);

		//public long Count(params object[] values) => array.Count;
		public void CopyTo(System.Array array, int index) => ((ICollection)this.array).CopyTo(array, index);

		public object Delete(object obj)
		{
			var index = (int)obj.Al() - 1;

			if (index < array.Count)
			{
				var ob = array[index];
				array[index] = null;
				return ob;
			}

			return null;
		}

		public object Get(long index) => this[Scripting.Script.ForceInt(index)];

		public IEnumerator<(object, object)> GetEnumerator() => new ArrayIndexValueIterator(array);

		public bool Has(object obj)
		{
			var index = (int)obj.Al() - 1;
			return index < array.Count ? array[index] != null : false;
		}

		public int IndexOf(object value) => ((IList)array).IndexOf(value);

		public void Insert(int index, object value) => ((IList)array).Insert(index, value);

		public void InsertAt(params object[] values)
		{
			var o = values;

			if (o.Length > 1)
			{
				var index = o.I1() - 1;

				for (var i = 1; i < values.Length; i++)//Need to use values here and not o because the enumerator will make the elements into Tuples because of the special enumerator.
				{
					//if (values[i] is ICollection ie)
					//{
					//  array.InsertRange(index, ie);
					//  index += ie.Count;
					//}
					//else
					{
						array.Insert(index++, values[i]);
					}
				}
			}
		}

		public object MaxIndex()
		{
			var val = long.MinValue;

			foreach (var el in array)
			{
				var temp = el.Al();

				if (temp > val)
					val = temp;
			}

			return val != long.MinValue ? val : string.Empty;
		}

		public object MinIndex()
		{
			var val = long.MaxValue;

			foreach (var el in array)
			{
				var temp = el.Al();

				if (temp < val)
					val = temp;
			}

			return val != long.MaxValue ? val : string.Empty;
		}

		public object Pop()
		{
			if (array.Count < 1)
				throw new Error($"Array was empty in {new StackFrame(0).GetMethod().Name}");

			var index = array.Count - 1;
			var val = array[index];
			array.RemoveAt(index);
			return val;
		}

		public void Push(params object[] values) => array.AddRange(values);

		public void Remove(object value) => ((IList)array).Remove(value);

		public object RemoveAt(params object[] values)//This must be variadic to properly resolve ahead of the interface method RemoveAt().
		{
			var o = values;

			if (array.Count > 0 && o.Length > 0)
			{
				var index = (int)o.L1() - 1;

				if (o.Length > 1)
				{
					var len = (int)o.Al(1);

					if (index + len <= array.Count)
						array.RemoveRange(index, len);

					return null;
				}
				else if (index < array.Count)
				{
					var ob = array[index];
					array.RemoveAt(index);
					return ob;
				}
			}

			return null;
		}

		public override string ToString()
		{
			if (array.Count > 0)
			{
				var sb = new StringBuilder(array.Count * 10);
				_ = sb.Append('[');

				for (var i = 0; i < array.Count; i++)
				{
					string str;
					var val = array[i];

					if (val is string vs)
						str = "\"" + vs + "\"";//Can't use interpolated string here because the AStyle formatter misinterprets it.
					else
						str = val.ToString();

					if (i < array.Count - 1)
						_ = sb.Append($"{str}, ");
					else
						_ = sb.Append($"{str}");
				}

				_ = sb.Append(']');
				return sb.ToString();
			}
			else
				return "[]";
		}

		IEnumerator IEnumerable.GetEnumerator() => __Enum();

		void IList.RemoveAt(int index) => RemoveAt(new object[] { index });//The explicit IList qualifier is necessary or else this will show up as a duplicate function.

		public virtual object this[object idx]
		{
			get
			{
				var index = idx.Ai();

				if (index > 0)
					return array[index - 1];
				else if (index < 0)
					return array[array.Count + index];
				else
					throw new IndexError($"Invalid index of {index} in {new StackFrame(0).GetMethod().Name}");
			}
			set
			{
				var index = idx.Ai();

				if (index > 0)
					array[index - 1] = value;
				else if (index < 0)
					array[array.Count + index] = value;
				else
					throw new IndexError($"Invalid index of {index} in {new StackFrame(0).GetMethod().Name}");
			}
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				this[index] = value;
			}
		}
	}

	public class ArrayIndexValueIterator : IEnumerator<(object, object)>
	{
		private List<object> arr;
		private int position = -1;

		public (object, object) Current
		{
			get
			{
				try
				{
					return ((long)position + 1, arr[position]);
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		object IEnumerator.Current => Current;

		public ArrayIndexValueIterator(List<object> a)
		{
			arr = a;
		}

		public void Dispose() => Reset();

		public bool MoveNext()
		{
			position++;
			return position < arr.Count;
		}

		public void Reset() => position = -1;

		private IEnumerator<(object, object)> GetEnumerator() => this;
	}
}