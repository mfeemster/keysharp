#if WINDOWS
namespace Keysharp.Core.COM
{
	internal class ComIterator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		internal object com;
		internal object newEnum;
		internal int position;
		IEnumerator enumerator;

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Current"/> which gets the index,value tuple at the current iterator position.
		/// For COM objects, it will always just be value,null.
		/// </summary>
		public (object, object) Current
		{
			get
			{
				try
				{
					var val = enumerator.Current;
					return (val, null);
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();//Should never happen when using regular loops.
				}
			}
		}

		internal ComIterator(object o)
			: base(null, 1)
		{
			com = o;

			try
			{
				newEnum = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetMethodOrProperty(com, "_NewEnum", 0));
				enumerator = (IEnumerator)newEnum;
			}
			catch (Keysharp.Core.Error ex0)
			{
				Error err;
				_ = Errors.ErrorOccurred(err = new Error($"Could not retrieve the _NewEnum() method on a COM object while trying to create an enumerator.")) ? throw err : "";
			}
		}

		/// <summary>
		/// The <see cref="IEnumerator.Current"/> implementation that just returns <see cref="Current"/>.
		/// </summary>
		object IEnumerator.Current => Current;

		/// <summary>
		/// Calls <see cref="Current"/> and places the value in the passed in object reference.
		/// </summary>
		/// <param name="value">A reference to the value.</param>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public override object Call(ref object value)
		{
			if (MoveNext())
			{
				(value, _) = Current;
				return true;
			}

			return false;
		}

		/// <summary>
		/// The implementation for <see cref="IComparer.Dispose"/> which internally resets the iterator.
		/// </summary>
		public void Dispose() => Reset();

		/// <summary>
		/// The implementation for <see cref="IEnumerator.MoveNext"/> which moves the iterator to the next position.
		/// </summary>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public bool MoveNext() => enumerator.MoveNext();

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Reset"/> which resets the iterator.
		/// </summary>
		public void Reset() => enumerator.Reset();
	}
}
#endif