namespace Keysharp.Core.Common.Containers
{
	public interface IClearable
	{
		void Clear();

		void Init();
	}

	public class ConcurrentStackPool<T>
		where T : class, IClearable, new ()
	{
		private readonly SlimStack<T> collection;
		public int Index => collection.Index;
		public ConcurrentStackPool(int size)
		{
			collection = new (size);

			for (var i = 0; i < size; i++)
				_ = collection.Push(new T());
		}
		public T Rent()
		{
			if (collection.TryPop(out var obj))
			{
				obj.Init();
				return obj;
			}

			return new T();
		}
		public void Return(T val, bool clear = true)
		{
			if (clear)
				val.Clear();

			_ = collection.Push(val);
		}
	}
}