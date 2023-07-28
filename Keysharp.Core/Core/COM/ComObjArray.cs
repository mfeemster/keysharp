using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core.COM
{
	public class ComArrayIndexValueIterator : IEnumerator<(object, object)>
	{
		private System.Array arr;
		private int position = -1;

		public (object, object) Current
		{
			get
			{
				try
				{
					return ((long)position, arr.GetValue(position));
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		object IEnumerator.Current => Current;

		public ComArrayIndexValueIterator(System.Array a)
		{
			arr = a;
		}

		public void Dispose() => Reset();

		public bool MoveNext()
		{
			position++;
			return position < arr.GetLength(0);
		}

		public void Reset() => position = -1;

		private IEnumerator<(object, object)> GetEnumerator() => this;

	}

	public class ComObjArray : ComObject, IEnumerable<(object, object)>, ICollection, IList
	{
		public System.Array array;

		public ComObjArray(System.Array arr) : base(Com.vt_array, arr, 1)
		{
			array = arr;
		}

		public int Count => ((ICollection)array).Count;

		public bool IsSynchronized => ((ICollection)array).IsSynchronized;

		public object SyncRoot => ((ICollection)array).SyncRoot;

		public bool IsFixedSize => ((IList)array).IsFixedSize;

		public bool IsReadOnly => ((IList)array).IsReadOnly;

		public object this[int index] { get => ((IList)array)[index]; set => ((IList)array)[index] = value; }

		public object MaxIndex(object obj0) => array.GetUpperBound(obj0.Ai(1) - 1);

		public object MinIndex(object obj0) => array.GetLowerBound(obj0.Ai(1) - 1);

		public object Clone(object obj0) => array.Clone();

		public IEnumerator<(object, object)> __Enum() => ((IEnumerable<(object, object)>)this).GetEnumerator();

		public IEnumerator<(object, object)> GetEnumerator() => new ComArrayIndexValueIterator(array);

		IEnumerator IEnumerable.GetEnumerator() => __Enum();

		public void CopyTo(System.Array array, int index) => ((ICollection)this.array).CopyTo(array, index);

		public int Add(object value) => ((IList)array).Add(value);

		public void Clear() => ((IList)array).Clear();

		public bool Contains(object value) => ((IList)array).Contains(value);

		public int IndexOf(object value) => ((IList)array).IndexOf(value);

		public void Insert(int index, object value) => ((IList)array).Insert(index, value);

		public void Remove(object value) => ((IList)array).Remove(value);

		public void RemoveAt(int index) => ((IList)array).RemoveAt(index);
	}
}
