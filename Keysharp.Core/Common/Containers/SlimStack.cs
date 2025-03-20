namespace Keysharp.Core.Common.Containers
{
	/// <summary>
	/// This class is meant to be a highly optimized and stripped down version of the built in Stack collection type.<br/>
	/// It does not resize and is meant to be used in a scenario where the stack is known to never exceed a given size.<br/>
	/// Usage is thread-safe because it internally uses <see cref="Interlocked.Increment"/> and <see cref="Interlocked.Decrement"/> to keep track
	/// of the stack index.
	/// </summary>
	/// <typeparam name="T">The type of object to hold in each element of the stack.</typeparam>
	[PublicForTestOnly]
	public class SlimStack<T> where T : class
	{
		/// <summary>
		/// The internal list used to contain the stack elements.
		/// </summary>
		private readonly T[] array;

		/// <summary>
		/// The size of the list which is set once during construction and does not change.
		/// </summary>
		private readonly int size;

		/// <summary>
		/// The current size/index of the stack which may be less than or equal to the size of the list.
		/// </summary>
		private int index;

		/// <summary>
		/// Gets the current size/index of the stack.
		/// </summary>
		public int Index => index;

		/// <summary>
		/// Initializes a new instance of the <see cref="SlimStack"/> class.<br/>
		/// All stack elements start off as null.
		/// </summary>
		/// <param name="s">The maximum size of the stack.</param>
		public SlimStack(int s)
		{
			index = 0;
			size = s;
            array = new T[size];
        }

        /// <summary>
        /// Pushes a new object onto the stack.
        /// </summary>
        /// <param name="obj">The object to push onto the stack.</param>
        /// <returns>True if the object was pushed, else false if there was no available space.</returns>
        public bool Push(T obj)
		{
			var next = Interlocked.Increment(ref index);

			if (next > 0 && next <= size)
			{
				array[next - 1] = obj;
				return true;
			}

			_ = Interlocked.Decrement(ref index);//Went too far up, so bump back down.
			return false;//No room, so just don't return the object and let the GC handle it.
		}

		/// <summary>
		/// Returns the most recent element in the stack if it exists.<br/>
		/// In the unlikely case this is called on another thread between the <see cref="Interlocked.Increment"/> and <see cref="Interlocked.Decrement"/>
		/// calls in <see cref="Push"/>,<br/>and all available space in the list has been used,
		/// it will return null.
		/// </summary>
		/// <returns>The most recent element, else null if empty.</returns>
		public T TryPeek()
		{
			var i = index;
			return i > 0 && i <= size ? array[i - 1] : default;
		}

		/// <summary>
		/// Removes and return the most recent element in the stack if it exists.
		/// </summary>
		/// <param name="obj">A reference to the object which will hold the popped element.
		/// This will be null if no element was popped.
		/// </param>
		/// <returns>True if an element was successfully removed and assigned to obj, else false.</returns>
		public bool TryPop(out T obj)
		{
			var next = Interlocked.Decrement(ref index);

			if (next >= 0 && next < size)
			{
				obj = array[next];
				array[next] = null;
				return true;
			}
			else
				_ = Interlocked.Increment(ref index);//Went too far down, so bump back up.

			obj = default;
			return false;
		}
	}
}