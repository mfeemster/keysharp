#if WINDOWS
using System.Runtime.InteropServices.Marshalling;

namespace Keysharp.Core.COM
{
	/// <summary>
	/// A two component iterator for a COM object which returns the value and the type of the element as a tuple.
	/// </summary>
	internal class ComEnumerator : KeysharpEnumerator, IEnumerator<(object, object)>
	{
		/// <summary>
		/// The COM object to create an enumerator for.
		/// </summary>
		private object com;

		/// <summary>
		/// The COM enumerator object for the COM object.
		/// </summary>
		private object newEnum;

		/// <summary>
		/// newEnum cast to an <see cref="IEnumerator"/>.
		/// </summary>
		private IEnumerator enumerator;

		/// <summary>
		/// Cache for iterators with either 1 or 2 parameters.
		/// This prevents reflection from having to always be done to find the Call method.
		/// </summary>
		private static FuncObj p1, p2;

		/// <summary>
		/// The implementation for <see cref="IEnumerator.Current"/> which gets the type,value tuple at the current iterator position.
		/// This differs from other enumerators in that the two element version returns the type of the COM element instead of the index.
		/// </summary>
		public (object, object) Current
		{
			get
			{
				try
				{
					var val = enumerator.Current;

					if (Count == 1)
						return (val, null);
					else
						return (val, Com.ComObjType(val));
				}
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();//Should never happen when using regular loops.
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ComEnumerator"/> class.
		/// </summary>
		/// <param name="o">The COM object to create an enumerator for.</param>
		internal ComEnumerator(object o, int c)
			: base(null, c)
		{
			com = o;
			var p = c <= 1 ? p1 : p2;
			var fo = (FuncObj)p.Clone();
			fo.Inst = this;
			CallFunc = fo;

			try
			{
				newEnum = Keysharp.Scripting.Script.Invoke(Keysharp.Scripting.Script.GetMethodOrProperty(com, "_NewEnum", 0));
				enumerator = (IEnumerator)newEnum;
			}
			catch (Keysharp.Core.Error ex)
			{
				Error err;
				_ = Errors.ErrorOccurred(err = new Error($"Could not retrieve the _NewEnum() method on a COM object while trying to create an enumerator: {ex}")) ? throw err : "";
			}
		}

		/// <summary>
		/// Static constructor to initialize function objects.
		/// </summary>
		static ComEnumerator()
		{
			Error err;
			var mi1 = Reflections.FindAndCacheMethod(typeof(ComEnumerator), "Call", 1);
			p1 = new FuncObj(mi1, null);

			if (!p1.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";

			var mi2 = Reflections.FindAndCacheMethod(typeof(ComEnumerator), "Call", 2);
			p2 = new FuncObj(mi2, null);

			if (!p2.IsValid)
				_ = Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : "";
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
		/// Calls <see cref="Current"/> and places the COM type in type and the value in val.
		/// </summary>
		/// <param name="type">A reference to the COM type value.</param>
		/// <param name="val">A reference to the object value.</param>
		/// <returns>True if the iterator position has not moved past the last element, else false.</returns>
		public override object Call(ref object type, ref object val)
		{
			if (MoveNext())
			{
				(type, val) = Current;
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