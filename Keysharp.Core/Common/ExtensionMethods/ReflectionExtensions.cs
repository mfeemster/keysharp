using System.Threading.Tasks;
using System.Windows.Forms;

namespace System.Reflection
{
	public static class ReflectionExtensions
	{
		public static object ControlCheckedInvoke(this MethodInfo mi, object inst, params object[] parameters)
		{
			object ret = null;

			if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
				_ = ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, parameters), false);
			else
				ret = mi.Invoke(inst, parameters);

			return ret;
		}

		public static async Task<object> InvokeAsync(this MethodInfo mi, object inst, params object[] parameters)
		{
			var tsk = mi.Invoke(inst, parameters);

			if (tsk is Task<object> to)
				return await to;

			return tsk;
		}
	}
}