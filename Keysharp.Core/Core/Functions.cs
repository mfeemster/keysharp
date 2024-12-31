namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for function object and function reflection-related functions.
	/// </summary>
	public static class Functions
	{
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
		public static IFuncObj Func(object funcName, object obj = null, object paramCount = null)
		{
			var name = funcName.As();
			var fo = new FuncObj(name, obj, paramCount);
			return fo.Name != "" ? fo
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {(obj != null ? obj.GetType() : "")} with parameter count {paramCount.Ai(-1)}.");
		}

		/// <summary>
		/// Same as Func(), but does not throw an exception if the method is not found.
		/// </summary>
		/// <param name="funcName">The name of the method.</param>
		/// <param name="obj">The object to call the method on. Default: null for static functions.</param>
		/// <param name="paramCount">The number of parameters the method has. Default: use the first method found.</param>
		/// <returns>An <see cref="IFuncObj"/> which can later be called like a method.</returns>
		public static IFuncObj FuncObj(object funcName, object obj = null, object paramCount = null) => new FuncObj(funcName.As(), obj, paramCount);

        public static IFuncObj FuncObj(object funcName, Type t, object paramCount = null) => new FuncObj(funcName.As(), t, paramCount);

        public static IFuncObj FuncObj(Delegate del, object obj = null) => new FuncObj(del, obj ?? del.Target);

        /// <summary>
        /// Gets a method of an object.
        /// </summary>
        /// <param name="value">The object to find the method on. Can't be a ComObject.</param>
        /// <param name="name">If omitted, validation is performed on value itself and value is returned if successful.<br/>
        /// Otherwise, specify the name of the method to retrieve.
        /// </param>
        /// <param name="paramCount">The number of parameters the method has. Default: use the first method found.</param>
        /// <exception cref="MethodError">A <see cref="MethodError"/> exception is thrown if the method cannot be found.</exception>
        /// <returns>An <see cref="IFuncObj"/> which can later be called like a method.</returns>
        public static IFuncObj GetMethod(object value, object name = null, object paramCount = null)
		{
			var v = value;
			var n = name.As();
			var count = paramCount.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(v.GetType(), n.Length > 0 ? n : "Call", count);
			return mph != null && mph.mi != null ? new FuncObj(mph.mi, v)
				   : throw new MethodError($"Unable to retrieve method {n} from object of type {v.GetType()} with parameter count {count}.");
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

				if (value is Map map)
					if (map.map.ContainsKey(n))
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
		internal static IFuncObj GetFuncObj(object h, object eventObj, bool throwIfBad = false)
		{
			IFuncObj del = null;

			if (h is string s)
			{
				if (s.Length > 0)
				{
					var tempdel = new FuncObj(s, eventObj);

					if (tempdel.IsValid)
						del = tempdel;
					else if (throwIfBad)
						throw new MethodError($"Unable to retrieve method {s} when creating a function object.");
				}//Empty string will just return null, which is a valid value in some cases.
			}
			else if (h is IFuncObj fo)
				del = fo;
            else if (h is Delegate d)
                del = new FuncObj(d.Method);
            else if (h is MethodInfo mi)
                del = new FuncObj(mi);
            else if (throwIfBad)
				throw new TypeError($"Improper value of {h} was supplied for a function object.");

			return del;
		}
	}
}