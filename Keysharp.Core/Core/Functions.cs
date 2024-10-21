namespace Keysharp.Core
{
	public static class Functions
	{
		public static IFuncObj Func(object obj, object obj1 = null, object obj2 = null)
		{
			var name = obj.As();
			var fo = new FuncObj(name, obj1, obj2);
			return fo.Name != "" ? fo
				   : throw new MethodError($"Unable to retrieve method {name}.");
		}

		public static IFuncObj FuncObj(object obj0, object obj1 = null, object obj2 = null) => new FuncObj(obj0.As(), obj1, obj2);

		public static IFuncObj GetMethod(object obj0, object obj1 = null, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			var paramCount = obj2.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(val.GetType(), name.Length > 0 ? name : "Call", paramCount);
			return mph != null && mph.mi != null ? new FuncObj(mph.mi, val)
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {val.GetType()} with parameter count {paramCount}.");
		}

		public static long HasMethod(object obj0, object obj1 = null, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			var paramCount = obj2.Ai(-1);
			var mph = Reflections.FindAndCacheMethod(val.GetType(), name.Length > 0 ? name : "Call", paramCount);
			return mph != null && mph.mi != null ? 1L : 0L;
		}

		public static long HasProp(object obj0, object obj1, object obj2 = null)
		{
			var val = obj0;
			var name = obj1.As();
			var paramCount = obj2.Ai(-1);

			if (obj0 is KeysharpObject kso)
			{
				if (kso.op != null && kso.op.ContainsKey(name))
					return 1L;

				if (obj0 is Map map)
					if (map.map.ContainsKey(name))
						return 1L;
			}

			var mph = Reflections.FindAndCacheProperty(val.GetType(), name, paramCount);
			return mph != null && mph.pi != null ? 1L : 0L;
		}

		public static IFuncObj ObjBindMethod(object obj0, object obj1 = null, object obj2 = null, params object[] obj)
		{
			var o = obj0;
			var paramCount = obj1.Ai(-1);
			var name = obj2.As();
			object[] args = obj;

			if (Reflections.FindAndCacheMethod(o.GetType(), name, paramCount) is MethodPropertyHolder mph && mph.mi != null)
				return new BoundFunc(mph.mi, args, o);

			return null;
		}

		public static string ObjGetBase(object obj) => obj.GetType().BaseType.Name;

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
			else if (throwIfBad)
				throw new TypeError($"Improper value of {h} was supplied for a function object.");

			return del;
		}
	}
}