namespace Keysharp.Core
{
	public static class Objects
	{
		public static Map Object(params object[] obj)
		{
			if (obj.Length == 0)
				return new Map();

			var dkt = new Map();
			dkt.Set(obj);
			return dkt;
		}

		public static object ObjGetCapacity(object obj) => obj is KeysharpObject kso ? kso.GetCapacity() : throw new Error($"Object of type {obj.GetType()} was not of type KeysharpObject.");

		//Make RefHolder.
		public static long ObjHasOwnProp(object obj0, object obj1) => obj0 is KeysharpObject kso ? kso.HasOwnProp(obj1) : 0L;

		public static long ObjOwnPropCount(object obj) => obj is KeysharpObject kso ? kso.OwnPropCount() : throw new Error($"Object of type {obj.GetType()} was not of type KeysharpObject.");

		public static object ObjOwnProps(object obj0, object obj1 = null) => obj0 is KeysharpObject kso ? kso.OwnProps(obj1) : throw new Error($"Object of type {obj0.GetType()} was not of type KeysharpObject.");

		public static void ObjSetBase(params object[] obj) => throw new Exception(Any.BaseExc);

		public static object ObjSetCapacity(object obj0, object obj1) => obj0 is KeysharpObject kso ? kso.SetCapacity(obj1) : throw new Error($"Object of type {obj0.GetType()} was not of type KeysharpObject.");
	}
}