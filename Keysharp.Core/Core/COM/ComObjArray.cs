#if WINDOWS

namespace Keysharp.Core.COM
{
	public class ComArrayIndexValueIterator : IEnumerator<(object, object)>
	{
		/// <summary>
		/// The internal array to be iterated over.
		/// </summary>
		private readonly System.Array arr;

		/// <summary>
		/// The current 0-based position the iterator is at.
		/// </summary>
		private int position = -1;

		/// <summary>
		/// The number of items to return for each iteration. Allowed values are 1 and 2:
		/// 1: return just the value in the first position
		/// 2: return the index in the first position and the value in the second.
		/// </summary>
		private readonly int count;

		public (object, object) Current
		{
			get
			{
				try
				{
					if (count == 1)
						return (arr.GetValue(position), null);
					else
						return ((long)position + 1, arr.GetValue(position));
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();//Should never happen when using regular loops.
				}
			}
		}

		object IEnumerator.Current => Current;

		public ComArrayIndexValueIterator(System.Array a, int c)
		{
			arr = a;
			count = c;
		}

		public void Call(ref object obj0) => (obj0, _) = Current;

		public void Call(ref object obj0, ref object obj1) => (obj0, obj1) = Current;

        public object Call(params object[] args)
        {
            if (MoveNext())
            {
                // If only one variable is passed, return just the value.
                if (args.Length == 1)
                {
                    Script.SetPropertyValue(args[0], "__Value", Current.Item1);
                }
                // Otherwise return the index (1-based) and the value.
                else if (args.Length >= 2)
                {
                    Script.SetPropertyValue(args[0], "__Value", Current.Item1);
                    Script.SetPropertyValue(args[1], "__Value", Current.Item2);
                }
                return true;
            }
            return false;
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

		public int Count => ((ICollection)array).Count;

		public bool IsFixedSize => ((IList)array).IsFixedSize;

		public bool IsReadOnly => ((IList)array).IsReadOnly;

		public bool IsSynchronized => ((ICollection)array).IsSynchronized;

		public object SyncRoot => ((ICollection)array).SyncRoot;

		public ComObjArray(System.Array arr) : base(Com.vt_array, arr, 1)
		{
			array = arr;
		}

		public IEnumerator<(object, object)> __Enum(object count) => new ComArrayIndexValueIterator(array, count.Ai());

		public int Add(object value) => ((IList)array).Add(value);

		public new void Clear() => ((IList)array).Clear();

		public object Clone(object obj0) => array.Clone();

		public bool Contains(object value) => ((IList)array).Contains(value);

		public void CopyTo(System.Array array, int index) => ((ICollection)this.array).CopyTo(array, index);

		public IEnumerator<(object, object)> GetEnumerator() => new ComArrayIndexValueIterator(array, 2);

		public int IndexOf(object value) => ((IList)array).IndexOf(value);

		public void Insert(int index, object value) => ((IList)array).Insert(index, value);

		public object MaxIndex(object obj0) => array.GetUpperBound(obj0.Ai(1) - 1);

		public object MinIndex(object obj0) => array.GetLowerBound(obj0.Ai(1) - 1);

		public void Remove(object value) => ((IList)array).Remove(value);

		public void RemoveAt(int index) => ((IList)array).RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => __Enum(2);

		public object this[int index] { get => ((IList)array)[index]; set => ((IList)array)[index] = value; }
	}
}

#endif