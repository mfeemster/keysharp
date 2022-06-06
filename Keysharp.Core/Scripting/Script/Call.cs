using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Keysharp.Core;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static object Invoke(object del, params object[] parameters)
		{
			try
			{
				if (del is ITuple mitup && mitup.Length > 1 && mitup[1] is MethodInfo mi)
				{
					var ob = mitup[0];
					var funcParams = mi.GetParameters();
					var isVariadic = funcParams.Length == 1 && funcParams[0].ParameterType == typeof(object[]);

					if (ob is IFuncObj fo && mi.Name == "Call")
						return fo.Call(parameters);
					//else if (mi.IsDefined(typeof(ExtensionAttribute), false))//Not supporting user callable extension methods anymore.
					//  return mi.Invoke(null, new object[] { ob }.Concat(new object[] { isVariadic ? parameters : parameters[0] }));
					else
						return mi.ArgumentAdjustedInvoke(ob, parameters);

					//return mi.Invoke(ob, mi.GetParameters().Length == 0 ? null : new object[] { isVariadic ? parameters : parameters[0] });//Even though parameters itself is an object[], we still must put it in an object[] array for variadic functions.
				}

				if (del is Delegate d)//Unlikely ever used, we don't use raw delegates anymore and instead use FuncObj.
				{
					return d.DynamicInvoke(parameters);// new object[] { parameters });
				}
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;

				throw;
			}

			return null;
		}
	}
}