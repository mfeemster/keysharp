namespace Keysharp.Core.Common
{
	public class Any
	{
		public static string BaseExc = "Changing a class base property at runtime cannot be implemented in C#.";

		public object Base => GetType().BaseType;//Documentation says this can be set, but C# doesn't support changing a base at runtime.

		public (Type, object) super
		{
			get
			{
				return (GetType().BaseType, this);
			}
		}

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