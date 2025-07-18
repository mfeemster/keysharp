﻿namespace System.Reflection
{
	/// <summary>
	/// Extension methods for various System.Reflection classes.
	/// </summary>
	public static class ReflectionExtensions
	{
		/// <summary>
		/// Invokes a <see cref="MethodInfo"/> with special checks using the Control.CheckedInvoke() extension method.<br/>
		/// If inst is not of type <see cref="Control"/>, then mi.Invoke() is called.
		/// </summary>
		/// <param name="mi">The <see cref="MethodInfo"/> to invoke.</param>
		/// <param name="inst">The object instance to invoke mi on.</param>
		/// <param name="parameters">The parameters to pass to mi.</param>
		/// <returns>The return value of calling mi.</returns>
		public static object ControlCheckedInvoke(this MethodInfo mi, object inst, params object[] parameters)
		{
			object ret = null;

			if (inst.GetControl() is Control ctrl)//If it's a gui control, then invoke on the gui thread.
				_ = ctrl.CheckedInvoke(() => ret = mi.Invoke(inst, parameters), false);
			else
				ret = mi.Invoke(inst, parameters);

			return ret;
		}

		internal static bool IsVariadic(this ParameterInfo pi) => pi != null && pi.CustomAttributes.Any(cad => cad.AttributeType == typeof(ParamArrayAttribute));
		//public static async Task<object> InvokeAsync(this MethodInfo mi, object inst, params object[] parameters)
		//{
		//  var tsk = mi.Invoke(inst, parameters);

		//  if (tsk is Task<object> to)
		//      return await to;

		//  return tsk;
		//}
	}
}