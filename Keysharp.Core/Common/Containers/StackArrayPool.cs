namespace Keysharp.Core.Common.Containers
{
	public class ConcurrentStackArrayPool<T>
	{
		private readonly SlimStack<T[]> collection = new (16); //Unlikely there would ever be more than 16 threads calling a given function at the same time before any of the others returned.
		private readonly int exactSize;

		public ConcurrentStackArrayPool(int size)
		{
			exactSize = size;
		}

		public T[] Rent()
		{
			return collection.TryPop(out var obj) ? obj : (new T[exactSize]);
		}

		public void Return(T[] array, bool clearArray = false)
		{
			if (clearArray)
				System.Array.Clear(array);

			_ = collection.Push(array);
		}
	}
}