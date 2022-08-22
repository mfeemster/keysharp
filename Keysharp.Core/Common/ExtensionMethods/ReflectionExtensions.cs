using System.Collections.Generic;
using System.Windows.Forms;
using Keysharp.Core;

namespace System.Reflection
{
	public static class ReflectionExtensions
	{
		public static object ArgumentAdjustedInvoke(this MethodInfo mi, object inst, params object[] obj)
		{
			var parameters = mi.GetParameters();
			var isVariadic = parameters.Length == 1 && parameters[0].ParameterType == typeof(object[]);

			if (parameters.Length == 0)
				return mi.ControlCheckedInvoke(inst, null);

			if (isVariadic)
				return mi.ControlCheckedInvoke(inst, new object[] { obj });

			if (parameters.Length == obj.Length)
				return mi.ControlCheckedInvoke(inst, obj);

			var i = 0;//The slowest case: a function is trying to be called with a different number of parameters than it actually has, so manually create an array of parameters that matches the required size.
			var newobj = new object[parameters.Length];

			for (; i < obj.Length && i < newobj.Length; i++)
			{
				if (i == newobj.Length - 1 && parameters[i].ParameterType == typeof(object[]))//Check if the last param is variadic, and if so, put all remaining params there.
				{
					var newi = i;
					var arr = new object[obj.Length - i];

					for (; i < obj.Length; i++)
						arr[i - newi] = obj[i];

					newobj[newi] = arr;
					break;
				}
				else
					newobj[i] = obj[i];
			}

			for (; i < newobj.Length; i++)
				newobj[i] = null;

			return mi.ControlCheckedInvoke(inst, newobj);
		}

		public static object ControlCheckedInvoke(this MethodInfo mi, object inst, params object[] parameters)
		{
			object ret = null;

			if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
				ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, parameters));
			else
				ret = mi.Invoke(inst, parameters);

			return ret;
		}
	}
}