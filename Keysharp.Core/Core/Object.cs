using System;
using System.Collections.Generic;
using System.Reflection;

namespace Keysharp.Core
{
	public class Any
	{
		public static string BaseExc = "Base property cannot be implemented in C#.";

		public object Base => GetType().BaseType;//Documentation says this can be set, but C# doesn't support changing a base at runtime.

		public virtual FuncObj GetMethod(params object[] obj)
		{
			var name = obj.L().S1();
			return Reflections.FindAndCacheMethod(GetType(), name) is MethodInfo mi
				   ? new FuncObj(mi, this)
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {GetType()}.");
		}

		public virtual bool HasBase(params object[] obj)
		{
			var o = obj.O1();//Do not flatten, always use the argument directly.
			return o.GetType().IsAssignableFrom(GetType());
		}

		public virtual long HasMethod(params object[] obj)
		{
			var name = obj.L().S1();
			return Reflections.FindAndCacheMethod(GetType(), name) is MethodInfo ? 1L : 0L;
		}

		public virtual long HasProp(params object[] obj)
		{
			var name = obj.L().S1();

			if (Reflections.FindAndCacheProperty(GetType(), name) is PropertyInfo pi)
				return 1L;

			//Figure out ownprops.//TODO
			return 0L;
		}
	}

	public class KeysharpObject : Any
	{
		public static KeysharpObject Object() => new KeysharpObject();

		public virtual object Clone(params object[] obj)
		{
			return null;
		}

		public void DefineProp(params object[] obj)
		{
		}

		public void DeleteProp(params object[] obj)
		{
		}

		public long GetCapacity(params object[] obj)
		{
			return 42;
		}

		public object GetOwnPropDesc(params object[] obj)
		{
			return null;
		}

		public object HasOwnProp(params object[] obj)
		{
			return 1L;
		}

		public long OwnPropCount(params object[] obj)
		{
			return 1L;
		}

		public object OwnProps(params object[] obj)//Need yield enum here.
		{
			return true;
		}

		public void SetBase(params object[] obj)
		{
			throw new Exception(Any.BaseExc);
		}

		public long SetCapacity(params object[] obj)
		{
			return 1L;
		}
	}
}