using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Keysharp.Core
{
	public class Any
	{
		public static string BaseExc = "Changing a class base property at runtime cannot be implemented in C#.";
		internal Dictionary<object, object> props = new Dictionary<object, object>();

		public object Base => GetType().BaseType;//Documentation says this can be set, but C# doesn't support changing a base at runtime.

		public (Type, object) super
		{
			get
			{
				return (GetType().BaseType, this);
			}
		}

		public virtual FuncObj GetMethod(object obj0 = null, object obj1 = null) => Function.GetMethod(this, obj0, obj1);
		//public bool DefineProp(object obj0, object obj1)
		//{
		//  var name obj0.As();

		//}

		public long HasBase(object obj) => obj.GetType().IsAssignableFrom(GetType()) ? 1L : 0L;

		public long HasMethod(object obj0 = null, object obj1 = null) => Function.HasMethod(this, obj0, obj1);

		public long HasProp(object obj) => Function.HasProp(this, obj);

		//public virtual string tostring() => ToString();
	}

	public class KeysharpObject : Any
	{
		//internal object indexOverride;
		public static KeysharpObject Object() => new KeysharpObject();

		public virtual object Clone()
		{
			return MemberwiseClone();
			//If ownprops are implemented, might need to add extra code for those.
		}

		public KeysharpObject DefineProp(object obj0, object obj1)
		{
			var name = obj0.As();

			if (obj1 is Keysharp.Core.Map map)
			{
				if (op == null)
					op = new Dictionary<string, Keysharp.Core.Map>(new CaseEqualityComp(eCaseSense.Off));

				op[name] = map;
			}

			return this;
		}

		public object DeleteProp(object obj) => op != null && op.Remove(obj.As(), out var map) ? map : "";

		public long GetCapacity() => throw new Keysharp.Core.Error("GetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");

		public object GetOwnPropDesc(params object[] obj) => null;

		public long HasOwnProp(object obj) => op != null && op.ContainsKey(obj.As()) ? 1L : 0L;

		public long OwnPropCount(params object[] obj) => obj != null ? op.Count : 0L;

		public object OwnProps(params object[] obj)//Need yield enum here.
		=> true;

		public void SetBase(params object[] obj) => throw new Exception(Any.BaseExc);

		public long SetCapacity(object obj) => throw new Keysharp.Core.Error("SetCapacity() is not supported or needed in Keysharp. The C# runtime handles all memory.");

		protected internal Dictionary<string, Keysharp.Core.Map> op;
	}
}