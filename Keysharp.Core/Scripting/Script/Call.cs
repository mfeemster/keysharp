using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		//public static object FunctionCall(object name, params object[] parameters)
		//{
		//  var namestr = name.ToString();
		//  var stack = new StackTrace(false).GetFrames();
		//  MethodInfo method = null;

		//  for (var i = 0; i < 3; i++)
		//  {
		//      var type = stack[i].GetMethod().DeclaringType;
		//      method = FindMethod(namestr, type.GetMethods(), parameters);

		//      if (method != null)
		//          break;
		//  }

		//  return method == null || !method.IsStatic ? null : method.Invoke(null, parameters.Length == 0 ? new object[] { parameters } : parameters);//Don't pass parameters to methods which don't have them.//MATT
		//}

		public static object Invoke(object del, params object[] parameters)
		{
			try
			{
				if (del is ITuple mitup && mitup.Length > 1 && mitup[0] is object ob && mitup[1] is MethodInfo mi)
				{
					if (mi.IsDefined(typeof(ExtensionAttribute), false))
						//return mi.Invoke(null, new object[] { ob }.Concat(parameters));
						return mi.Invoke(null, new object[] { ob, parameters });
					//else if (mi.GetParameters().Length == 0)
					//{
					//  return mi.Invoke(ob, null);
					//  //return mi.Invoke(ob, new object[1] { new object[1] { "" } });
					//}
					else
						//return mi.Invoke(ob, mi.GetParameters().Length == 0 ? null : parameters.Length == 1 ? new object[] { parameters } : parameters);
						//return mi.Invoke(ob, mi.GetParameters().Length == 0 ? null : parameters);// new object[] { parameters });
						return mi.Invoke(ob, mi.GetParameters().Length == 0 ? null : new object[] { parameters });//Even though parameters itself is an object[], we still must put it in an object[] array.
				}

				if (del is Delegate d)
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