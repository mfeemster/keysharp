namespace System.Reflection
{
	public static class ReflectionExtensions
	{
		public static object ArgumentAdjustedInvoke(this MethodInfo mi, object inst, params object[] obj)
		{
			var parameters = mi.GetParameters();
			var isVariadic = parameters.Length == 1 && parameters[0].ParameterType == typeof(object[]);

			if (parameters.Length == 0)
				return mi.Invoke(inst, null);

			if (isVariadic)
				return mi.Invoke(inst, new object[] { obj });

			if (parameters.Length == obj.Length)
				return mi.Invoke(inst, obj);

			var i = 0;//The slowest case: a function is trying to be called with a different number of parameters than it actually has, so manually create an array of parameters that matches the required size.
			var newobj = new object[parameters.Length];

			for (; i < obj.Length && i < newobj.Length; i++)
				newobj[i] = obj[i];

			for (; i < newobj.Length; i++)
				newobj[i] = null;

			return mi.Invoke(inst, newobj);
		}
	}
}