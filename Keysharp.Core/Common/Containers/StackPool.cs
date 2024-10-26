namespace Keysharp.Core.Common.Containers
{
	/// <summary>
	/// Interface for an object which can be initialized and cleared.
	/// </summary>
	public interface IClearable
	{
		/// <summary>
		/// Clear the object.
		/// </summary>
		void Clear();

		/// <summary>
		/// Initialize the object.
		/// </summary>
		void Init();
	}

	/// <summary>
	/// A wrapper class for a for an object pool which uses a SlimStack of objects of type T.
	/// The intended usage is for the caller to rent an object from the pool, then return it when done.
	/// Since the underlying SlimStack object is thread-safe, so is this class, hence the prefix "Concurrent" in the name.
	/// The objects are all created in the constructor.
	/// </summary>
	/// <typeparam name="T"/>
	public class ConcurrentStackPool<T>
		where T : class, IClearable, new ()
	{
		/// <summary>
		/// The SlimStack which holds the objects of type T.
		/// </summary>
		private readonly SlimStack<T> collection;
		/// <summary>
		/// Gets the current index of the stack.
		/// </summary>
		public int Index => collection.Index;
		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentStackPool"/> class.
		/// </summary>
		/// <param name="stackSize">The fixed size of the stack.</param>
		public ConcurrentStackPool(int stackSize)
		{
			collection = new (stackSize);

			for (var i = 0; i < stackSize; i++)
				_ = collection.Push(new T());
		}
		/// <summary>
		/// Rents an object from the stack.
		/// If an object was successfully rented, its Init() interface method will be called before returning.
		/// If there are no free elements in the stack, a new object is returned which
		/// is just allocated regularly on the heap.
		/// </summary>
		/// <returns>An object of type T.</returns>
		public T Rent()
		{
			if (collection.TryPop(out var obj))
			{
				obj.Init();
				return obj;
			}

			return new T();
		}
		/// <summary>
		/// Return an object to the stack and optionally clear it before returning.
		/// </summary>
		/// <param name="val">The object to return.</param>
		/// <param name="clear">True to call the Clear() interface method before returning, else false.</param>
		/// <returns>True if returned, else false.</returns>
		public bool Return(T val, bool clear = true)
		{
			if (clear)
				val.Clear();

			return collection.Push(val);
		}
	}
}