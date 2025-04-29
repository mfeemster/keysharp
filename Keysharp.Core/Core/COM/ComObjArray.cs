#if WINDOWS
namespace Keysharp.Core.COM
{
	public class ComArrayIndexValueEnumerator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		private static FuncObj p1, p2;

		/// <summary>
		/// The internal array to be iterated over.
		/// </summary>
		private readonly System.Array arr;

		/// <summary>
		/// The number of items to return for each iteration. Allowed values are 1 and 2:
		/// 1: return just the value in the first position
		/// 2: return the index in the first position and the value in the second.
		/// </summary>
		private readonly int count;

		/// <summary>
		/// The current 0-based position the iterator is at.
		/// </summary>
		private int position = -1;

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

		static ComArrayIndexValueEnumerator()
		{
			Error err;
			var mi1 = Reflections.FindAndCacheMethod(typeof(ComArrayIndexValueEnumerator), "Call", 1);
			p1 = new FuncObj(mi1, null);

			if (!p1.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";

			var mi2 = Reflections.FindAndCacheMethod(typeof(ComArrayIndexValueEnumerator), "Call", 2);
			p2 = new FuncObj(mi2, null);

			if (!p2.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";
		}

		public ComArrayIndexValueEnumerator(System.Array a, int c)
			: base(null, c)
		{
			arr = a;
			count = c;
			var p = c <= 1 ? p1 : p2;
			fo = (FuncObj)p.Clone();
			fo.Inst = this;
		}

		public override object Call(object pos)
		{
			if (MoveNext())
			{
				Script.SetPropertyValue(pos, "__Value", Current.Item1);
				return true;
			}

			return false;
		}

		public override object Call(object pos, object val)
		{
			if (MoveNext())
			{
				Script.SetPropertyValue(pos, "__Value", Current.Item1);
				Script.SetPropertyValue(val, "__Value", Current.Item2);
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
	}

	public class ComObjArray : ComObject, I__Enum, IEnumerable<(object, object)>, ICollection, IList
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

		public IFuncObj __Enum(object count) => new ComArrayIndexValueEnumerator(array, count.Ai()).fo;

		public int Add(object value) => ((IList)array).Add(value);

		public new void Clear() => ((IList)array).Clear();

		public object Clone(object obj0) => array.Clone();

		public bool Contains(object value) => ((IList)array).Contains(value);

		public void CopyTo(System.Array array, int index) => ((ICollection)this.array).CopyTo(array, index);

		public IEnumerator<(object, object)> GetEnumerator() => new ComArrayIndexValueEnumerator(array, 2);

		public int IndexOf(object value) => ((IList)array).IndexOf(value);

		public void Insert(int index, object value) => ((IList)array).Insert(index, value);

		public object MaxIndex(object obj0) => array.GetUpperBound(obj0.Ai(1) - 1);

		public object MinIndex(object obj0) => array.GetLowerBound(obj0.Ai(1) - 1);

		public void Remove(object value) => ((IList)array).Remove(value);

		public void RemoveAt(int index) => ((IList)array).RemoveAt(index);

		IEnumerator IEnumerable.GetEnumerator() => new ComArrayIndexValueEnumerator(array, 2);

		public object this[int index] { get => ((IList)array)[index]; set => ((IList)array)[index] = value; }
	}
}
#endif