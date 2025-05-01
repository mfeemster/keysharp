using System;

namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for function object and function reflection-related functions.
	/// </summary>
	public static class Functions
	{
		/// <summary>
		/// Avoid creating new FuncObj objects for the same string/delegate.
		/// </summary>
		internal static ConcurrentDictionary<object, IFuncObj> cachedFuncObj = new (new CaseEqualityComp(eCaseSense.Off));

		/// <summary>
		/// Creates a function object by searching for a method within the script.
		/// The search is done by matching the object type, name and parameter count.
		/// Static functions use null for the object.
		/// </summary>
		/// <param name="funcName">The name of the method.</param>
		/// <param name="obj">The object to call the method on. Default: null for static functions.</param>
		/// <param name="paramCount">The number of parameters the method has. Default: use the first method found.</param>
		/// <returns>An <see cref="IFuncObj"/> which can later be called like a function.</returns>
		/// <exception cref="MethodError">A <see cref="MethodError"/> exception is thrown if the method cannot be found.</exception>
		//public static IFuncObj Func(object funcName, object obj = null, object paramCount = null)
		//{
		//  Error err;
		//  var name = funcName.As();
		//  var fo = new FuncObj(name, obj, paramCount);

		//  if (fo.Name != "")
		//      return fo;

		//  return Errors.ErrorOccurred(err = new MethodError($"Unable to retrieve method {name} from object of type {(obj != null ? obj.GetType() : "")} with parameter count {paramCount.Ai(-1)}.")) ? throw err : null;
		//}

		/// <summary>
		/// Creates a function object by searching for a method within the script if funcName was a string.
		/// The search is done by matching the object type, name and parameter count.
		/// If funcName was a delegate, then a new <see cref="FuncObj"/> is returned using delegate.Method.
		/// If funcName was already an <see cref="IFuncObj"/>, then it's just casted and returned.
		/// Static functions use null for the object.
		/// </summary>
		/// <param name="funcName">The name, delegate or IFuncObj of the method.</param>
		/// <param name="obj">The object to call the method on. Default: null for static functions.</param>
		/// <param name="paramCount">The number of parameters the method has. Default: use the first method found.</param>
		/// <returns>An <see cref="IFuncObj"/> which can later be called like a function.</returns>
		public static IFuncObj Func(object funcName, object obj = null, object paramCount = null) => GetFuncObj(funcName, obj, paramCount);

		/// <summary>
		/// Gets a method of an object.
		/// </summary>
		/// <param name="value">The object to find the method on. Can't be a ComObject.</param>
		/// <param name="name">If omitted, validation is performed on value itself and value is returned if successful.<br/>
		/// Otherwise, specify the name of the method to retrieve.
		/// </param>
		/// <param name="paramCount">The number of parameters the method has. Default: use the first method found.</param>
		/// <returns>An <see cref="IFuncObj"/> which can later be called like a method.</returns>
		/// <exception cref="MethodError">A <see cref="MethodError"/> exception is thrown if the method cannot be found.</exception>
		public static IFuncObj GetMethod(object value, object name = null, object paramCount = null)
		{
			Error err;
			var v = value;
			var n = name.As();
			var count = paramCount.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(v.GetType(), n.Length > 0 ? n : "Call", count);

			if (mph != null && mph.mi != null)
				return new FuncObj(mph.mi, v);

			return Errors.ErrorOccurred(err = new MethodError($"Unable to retrieve method {n} from object of type {v.GetType()} with parameter count {count}.")) ? throw err : null;
		}

		/// <summary>
		/// Returns whether the specified value has a method by the specified name.
		/// </summary>
		/// <param name="value">The object to find the method on. Can't be a ComObject.</param>
		/// <param name="name">If omitted, Value itself is checked whether it is callable.<br/>
		/// Otherwise, specify the method name to check for.
		/// </param>
		/// <param name="paramCount">The number of parameters the method has. Default: use the first function found.</param>
		/// <returns>1 if the method was found on the object, else 0.</returns>
		public static long HasMethod(object value, object name = null, object paramCount = null)
		{
			var val = value;
			var n = name.As();
			var count = paramCount.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(val.GetType(), n.Length > 0 ? n : "Call", count);
			return mph != null && mph.mi != null ? 1L : 0L;
		}

		/// <summary>
		/// Returns whether the specified value has a property by the specified name.
		/// </summary>
		/// <param name="value">The object to find the property on. Can't be a ComObject.</param>
		/// <param name="name">The property name to check for.</param>
		/// <param name="paramCount">The number of parameters the property takes.<br/>
		/// This is used for indexers which can take 1 or more parameters. Default: 0.
		/// </param>
		/// <returns>1 if the property was found on the object, else 0.</returns>
		public static long HasProp(object value, object name, object paramCount = null)
		{
			var val = value;
			var n = name.As();
			var count = paramCount.Ai(-1);

			if (value is KeysharpObject kso)
			{
				if (kso.op != null && kso.op.ContainsKey(n))
					return 1L;
			}

			var mph = Reflections.FindAndCacheProperty(val.GetType(), n, count);
			return mph != null && mph.pi != null ? 1L : 0L;
		}

		/// <summary>
		/// Creates a <see cref="BoundFunc"/> object which calls a method of a given object.
		/// </summary>
		/// <param name="obj">The object to find the method on.</param>
		/// <param name="name">A method name. If omitted, the bound function calls obj itself.</param>
		/// <param name="paramCount">The number of parameters the property takes. Default: use the first function found.</param>
		/// <param name="args">The arguments to bind to the function.</param>
		/// <returns>An new <see cref="BoundFunc"/> object with the specified arguments bound to it.</returns>
		public static IFuncObj ObjBindMethod(object obj, object name = null, object paramCount = null, params object[] args)
		{
			var o = obj;
			var n = name.As();
			var count = paramCount.Ai(-1);
			object[] a = args;

			if (Reflections.FindAndCacheMethod(o.GetType(), n, count) is MethodPropertyHolder mph && mph.mi != null)
				return new BoundFunc(mph.mi, a, o);

			return null;
		}

		/// <summary>
		/// Internal helper to get a function object which supports different ways of identifying such.
		/// </summary>
		/// <param name="h">The object to examine. This can be a string or an existing function object.</param>
		/// <param name="eventObj">The object to find the method on.</param>
		/// <param name="throwIfBad">Whether throw an exception if the method could not be found. Default: false.</param>
		/// <returns>An <see cref="IFuncObj"/> which may be a newly recreated one, or h if it was already one.</returns>
		/// <exception cref="MethodError">A <see cref="MethodError"/> exception is thrown if a function object couldn't be created</exception>
		/// <exception cref="TypeError">A <see cref="TypeError"/> exception is thrown if h was not a string or existing function object.</exception>
		public static IFuncObj GetFuncObj(object h, object eventObj, object paramCount = null, bool throwIfBad = false)
		{
			Error err;
			IFuncObj del = null;

			if (h is string s)
			{
				if (s.Length > 0)
				{
					var tempdel = cachedFuncObj.GetOrAdd(s, (key) => new FuncObj(s, eventObj, paramCount));

					if (tempdel.IsValid)
						del = tempdel;
					else if (throwIfBad)
						return Errors.ErrorOccurred(err = new MethodError($"Unable to retrieve method {s} when creating a function object.")) ? throw err : null;
				}//Empty string will just return null, which is a valid value in some cases.
			}
			else if (h is IFuncObj fo)
			{
				var tempdel = fo;

				if (tempdel.IsValid)
					del = tempdel;
				else if (throwIfBad)
					return Errors.ErrorOccurred(err = new MethodError($"Existing function object was invalid.")) ? throw err : null;
			}
			else if (h is Delegate d)
			{
				var tempdel = cachedFuncObj.GetOrAdd(d, (key) => new FuncObj(d, eventObj));

				if (tempdel.IsValid)
					del = tempdel;
				else if (throwIfBad)
					return Errors.ErrorOccurred(err = new MethodError($"Unable to retrieve method info for {d.Method.Name} when creating a function object from delegate.")) ? throw err : null;
			}
			else if (h is KeysharpEnumerator ke)
			{
				del = ke.CallFunc;
			}
			else if (throwIfBad)
				return Errors.ErrorOccurred(err = new TypeError($"Improper value of {h} was supplied for a function object.")) ? throw err : null;

			return del;
		}
	}
}