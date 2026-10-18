namespace Keysharp.Internals.Containers;

/// <summary>
/// A simple single-threaded reusable object pool with LIFO (stack) behavior.
/// An optional size can be passed to limit the number of objects kept in the pool.
/// If the pool is full, returned objects are discarded and will be garbage collected.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
[PublicHiddenFromUser]
public class ObjectStackPool<T> where T : class
{
	private readonly int maxSize;
	private readonly Func<T> objectGenerator;
	private readonly Action<T> resetAction;
	private readonly Stack<T> stack = new();

	/// <summary>
	/// Current number of objects in the pool.
	/// </summary>
	public int Count => stack.Count;

	/// <summary>
	/// Creates a new object stack pool.
	/// </summary>
	/// <param name="objectGenerator">Function to create a new object when pool is empty.</param>
	/// <param name="resetAction">Optional action to reset object state before reuse.</param>
	/// <param name="maxSize">Maximum number of objects to keep in the pool. Default: no limit (int.MaxValue).</param>
	public ObjectStackPool(Func<T> objectGenerator, Action<T> resetAction = null, int maxSize = int.MaxValue)
	{
		this.objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
		this.resetAction = resetAction;
		this.maxSize = maxSize > 0 ? maxSize : throw new ArgumentOutOfRangeException(nameof(maxSize));
	}

	/// <summary>
	/// Gets an object from the pool or creates a new one if empty.
	/// </summary>
	public T Get() => stack.TryPop(out var item) ? item : objectGenerator();

	/// <summary>
	/// Returns an object to the pool for reuse.
	/// </summary>
	public void Return(T item)
	{
		if (item == null)
			return;

		resetAction?.Invoke(item);

		// Only store if pool is not full
		if (stack.Count < maxSize)
			stack.Push(item);
		// else discard and let GC handle it
	}
}
