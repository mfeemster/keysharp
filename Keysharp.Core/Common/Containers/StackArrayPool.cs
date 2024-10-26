namespace Keysharp.Core.Common.Containers
{
	/// <summary>
	/// A wrapper class for an array pool which uses a SlimStack of object arrays whose elements are of type T.
	/// The intended usage is for the caller to rent an array from the pool, then return it when done.
	/// This differs from ArrayPool in that each array is the same size of that passed to the constructor.
	/// Since the underlying SlimStack object is thread-safe, so is this class, hence the prefix "Concurrent" in the name.
	/// The arrays aren't actually created until the moment they are first rented.
	/// </summary>
	/// <typeparam name="T"/>
	public class ConcurrentStackArrayPool<T>
	{
		/// <summary>
		/// The SlimStack which holds the arrays of T.
		/// </summary>
		private readonly SlimStack<T[]> collection;

		/// <summary>
		/// The size of each array.
		/// </summary>
		private readonly int arraySize;

		/// <summary>
		/// Gets the current index of the stack.
		/// </summary>
		public int Index => collection.Index;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentStackArrayPool"/> class.
		/// </summary>
		/// <param name="_arraySize">The size of each array.</param>
		/// <param name="stackSize">The fixed size of each stack.</param>
		public ConcurrentStackArrayPool(int _arraySize, int stackSize = 16)
		{
			arraySize = _arraySize;
			collection = new (stackSize);
		}

		/// <summary>
		/// Rents an array from the stack.
		/// If there are no free elements in the stack, a new array is returned which
		/// is just allocated regularly on the heap.
		/// </summary>
		/// <returns>An array of type T whose size is that specified in the constructor.</returns>
		public T[] Rent() => collection.TryPop(out var obj) ? obj : (new T[arraySize]);

		/// <summary>
		/// Return an array to the stack.
		/// Note that the array size must match the size specified in the constructor, otherwise
		/// it will not be pushed back onto the stack.
		/// It still may be cleared in this case.
		/// </summary>
		/// <param name="array">The array to return.</param>
		/// <param name="clearArray">True to set all elements of the array to null before returning, else false.
		/// This helps avoid holding on to references unintentionally.
		/// </param>
		/// <returns>True if returned, else false.</returns>
		public bool Return(T[] array, bool clearArray = false)
		{
			if (array != null)
			{
				if (clearArray)
					System.Array.Clear(array);

				if (array.Length == arraySize)
					return collection.Push(array);
			}

			return false;
		}
	}
}