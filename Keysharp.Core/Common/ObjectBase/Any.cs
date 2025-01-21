using System.Collections.Generic;

namespace Keysharp.Core.Common.ObjectBase
{
	public class Any
	{
		public static object __Static
		{
			get => Script.Variables.Statics.GetValueOrDefault(GetCallingType());
			set => Script.Variables.Statics[GetCallingType()] = (KeysharpObject)value;
        }

        private static Type GetCallingType()
        {
            var frame = new System.Diagnostics.StackTrace().GetFrame(3); // Get the caller two levels up
            return frame?.GetMethod()?.DeclaringType;
        }

        public static string BaseExc = "Changing a class base property at runtime cannot be implemented in C#.";

		public object Base => GetType().BaseType;//Documentation says this can be set, but C# doesn't support changing a base at runtime.

		public (Type, object) super => (typeof(object), this);

		public virtual IFuncObj GetMethod(object obj0 = null, object obj1 = null) => Functions.GetMethod(this, obj0, obj1);

		//public bool DefineProp(object obj0, object obj1)
		//{
		//  var name obj0.As();

		//}

		public long HasBase(object obj) => obj.GetType().IsAssignableFrom(GetType()) ? 1L : 0L;

		public long HasMethod(object obj0 = null, object obj1 = null) => Functions.HasMethod(this, obj0, obj1);

		public long HasProp(object obj) => Functions.HasProp(this, obj);

		//public virtual string tostring() => ToString();
	}
}