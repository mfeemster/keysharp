namespace Keysharp.Core
{
	public interface IClearable
	{
		void Clear();

		void Init();
	}

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

	public class RefHolder
	{
		internal int index;
		internal Action<object> reassign;
		internal object val;

		public RefHolder(int i, object o, Action<object> r)
		{
			index = i;
			val = o ?? "";
			reassign = r;
		}
	}

	/// <summary>
	/// This class is meant to be a highly optimized and stripped down version of the built in Stack collection type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SlimStack<T> where T : class
	{
		private readonly List<T> list;
		private readonly int size;
		private int index;
		public int Index => index;

		public SlimStack(int s)
		{
			index = 0;
			size = s;
			list = new List<T>(size);

			for (var i = 0; i < size; i++)
				list.Add(null);//Empty placeholder slots.
		}

		public bool Push(T obj)
		{
			var next = Interlocked.Increment(ref index);

			if (next > 0 && next <= size)
			{
				list[next - 1] = obj;
				return true;
			}

			_ = Interlocked.Decrement(ref index);//Went too far up, so bump back down.
			return false;//No room, so just don't return the object and let the GC handle it.
		}

		public T TryPeek()
		{
			var i = index;
			return i > 0 && i <= list.Count ? list[i - 1] : default;
		}

		public bool TryPop(out T obj)
		{
			var next = Interlocked.Decrement(ref index);

			if (next >= 0 && next < size)
			{
				obj = list[next];
				list[next] = null;
				return true;
			}
			else
				_ = Interlocked.Increment(ref index);//Went too far down, so bump back up.

			obj = default;
			return false;
		}
	}
}