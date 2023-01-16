using System;
using System.Collections.Generic;
using System.Reflection;

namespace Keysharp.Core
{
	public class Any
	{
		public static string BaseExc = "Base property cannot be implemented in C#.";
		internal Dictionary<object, object> props = new Dictionary<object, object>();

		public object Base => GetType().BaseType;//Documentation says this can be set, but C# doesn't support changing a base at runtime.

		public (Type, object) super
		{
			get
			{
				return (GetType().BaseType, this);
			}
		}

		public virtual FuncObj GetMethod(object obj0, object obj1 = null)
		{
			var name = obj0.As();
			//var paramcount = obj1.Al();
			return Reflections.FindAndCacheMethod(GetType(), name) is MethodInfo mi
				   ? new FuncObj(mi, this)
				   : throw new MethodError($"Unable to retrieve method {name} from object of type {GetType()}.");
		}

		//public bool DefineProp(object obj0, object obj1)
		//{
		//  var name obj0.As();

		//}

		public bool HasBase(object obj) => obj.GetType().IsAssignableFrom(GetType());

		public long HasMethod(object obj) => Reflections.FindAndCacheMethod(GetType(), obj.As()) is MethodInfo ? 1L : 0L;

		public long HasProp(object obj)
		{
			var name = obj.As();

			if (Reflections.FindAndCacheProperty(GetType(), name) is PropertyInfo pi)
				return 1L;

			//Figure out ownprops.//TODO
			return 0L;
		}
	}

	public class KeysharpObject : Any
	{
		public static KeysharpObject Object() => new KeysharpObject();

		public virtual object Clone(params object[] obj) => null;

		public void DefineProp(params object[] obj)
		{
		}

		public void DeleteProp(params object[] obj)
		{
		}

		public long GetCapacity(params object[] obj) => throw new Keysharp.Core.Error("GetCapacity() is not supported for dictionaries in C#");

		public object GetOwnPropDesc(params object[] obj) => null;

		public object HasOwnProp(params object[] obj) => 1L;

		public long OwnPropCount(params object[] obj) => 1L;

		public object OwnProps(params object[] obj)//Need yield enum here.
		=> true;

		public void SetBase(params object[] obj) => throw new Exception(Any.BaseExc);

		public long SetCapacity(params object[] obj) => 1L;
	}
}