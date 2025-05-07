namespace Keysharp.Core.Common.Containers
{
	/// <summary>
	/// This class is meant to be a highly optimized and stripped down version of the built in Stack collection type.<br/>
	/// It does not resize and is meant to be used in a scenario where the stack is known to never exceed a given size.<br/>
	/// All elements start off as null and will be created as needed when <see cref="TryPush" /> is called.<br/>
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
		private readonly T[] list;
		/// <summary>
		/// The size of the list which is set once during construction and does not change.
		/// </summary>
		private readonly int size;
		/// <summary>
		/// The current size/index of the stack which may be less than or equal to the size of the list.
		/// </summary>
		private int index;
		/// <summary>
		/// An object creation function. This is done so this class can be used with T and T[].
		/// </summary>
		private Func<T> create;
		/// <summary>
		/// Gets the current size/index of the stack.
		/// </summary>
		public int Index => index;
		/// <summary>
		/// Initializes a new instance of the <see cref="SlimStack"/> class.<br/>
		/// All stack elements start off as null and will be created as needed
		/// by calling create().
		/// </summary>
		/// <param name="s">The maximum size of the stack.</param>
		/// <param name="c">The creation function used to create each element.</param>
		public SlimStack(int s, Func<T> c)
		{
			index = 0;
			size = s;
			list = new T[size];//All will be null to begin with.
			create = c;
		}
		/// <summary>
		/// Attempts to push a new object onto the stack.
		/// Any initialization of the object's internal state must be done by the caller after this function exits.
		/// </summary>
		/// <param name="obj">The object to push onto the stack.</param>
		/// <returns>True if the object was pushed, else false if there was no available space.</returns>
		public bool TryPush(out T obj)
		{
			var next = Interlocked.Increment(ref index);

			if (next > 0 && next <= size)
			{
				obj = list[next - 1];

				if (obj == null)
					list[next - 1] = obj = create();

				return true;
			}

			obj = null;
			_ = Interlocked.Decrement(ref index);//Went too far up, so bump back down.
			return false;//No room, so just don't return the object and let the GC handle it.
		}
		/// <summary>
		/// Returns the most recent element in the stack if it exists.<br/>
		/// In the unlikely case this is called on another thread between the <see cref="Interlocked.Increment"/> and <see cref="Interlocked.Decrement"/>
		/// calls in <see cref="TryPush"/>,<br/>and all available space in the list has been used,
		/// it will return null.
		/// </summary>
		/// <returns>The most recent element, else null if empty.</returns>
		public T TryPeek()
		{
			var i = index;
			return i > 0 && i <= list.Length ? list[i - 1] : null;
		}
		/// <summary>
		/// Removes and returns the most recent element in the stack if it exists.
		/// Any clearing/destruction of the object's internal state must be done by the caller after this function exits.
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
				obj = list[next];
				return true;
			}
			else
				_ = Interlocked.Increment(ref index);//Went too far down, so bump back up.

			obj = null;
			return false;
		}
	}
}