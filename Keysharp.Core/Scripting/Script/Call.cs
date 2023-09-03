using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.COM;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		[ThreadStatic]
		private static object lastItem = null;

		[ThreadStatic]
		private static string lastKey = null;

		[ThreadStatic]
		private static int lastParamCount = 0;

		[ThreadStatic]
		private static (object, MethodPropertyHolder) lastMph = (null, null);

		public static (object, MethodPropertyHolder) GetMethodOrProperty(object item, string key, int paramCount)//This has to be public because the script will emit it in Main().
		{
			if (ReferenceEquals(item, lastItem) && ReferenceEquals(key, lastKey) && paramCount == lastParamCount)
				return lastMph;

			Type typetouse = null;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else if (item != null)
				typetouse = item.GetType();

			if (item == null)
			{
				if (Reflections.FindMethod(key, paramCount) is MethodPropertyHolder mph0)
				{
					lastItem = item;
					lastKey = key;
					lastParamCount = paramCount;
					return lastMph = (item, mph0);
				}
			}
			else if (Reflections.FindAndCacheMethod(typetouse, key, paramCount) is MethodPropertyHolder mph1)
			{
				lastItem = item;
				lastKey = key;
				lastParamCount = paramCount;
				return lastMph = (item, mph1);
			}
			else if (Reflections.FindAndCacheProperty(typetouse, key, paramCount) is MethodPropertyHolder mph2)
			{
				lastItem = item;
				lastKey = key;
				lastParamCount = paramCount;
				return lastMph = (item, mph2);
			}
			else if (item is ComObject co)
			{
				//return (co.Ptr, new ComMethodPropertyHolder(key));//Unwrap.
				return GetMethodOrProperty(co.Ptr, key, paramCount);
			}
			else if (Marshal.IsComObject(item))
			{
				return (item, new ComMethodPropertyHolder(key));
			}

			throw new MemberError($"Attempting to get method or property {item} with key {key} failed.");
		}

		public static object GetPropertyValue(object item, object name)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		{
			Type typetouse = null;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else if (item != null)
				typetouse = item.GetType();

			var namestr = name.ToString();

			if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph)
			{
				try
				{
					return mph.callFunc(item, null);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
			//This is for the case where a Map accesses a key within the Map as if it were a property, so we try to get the indexer property, then pass the name of the passed in property as the key/index.
			//We check for a param length of 1 so we don't accidentally grab properties named Item which have no parameters, such as is the case with ComObject.
			else if (Reflections.FindAndCacheMethod(typetouse, "get_Item", 1) is MethodPropertyHolder mph1 && mph1.ParamLength == 1)
			{
				return mph1.callFunc(item, new object[] { namestr });
			}
			else if (item is ComObject co)
			{
				//return co.Ptr.GetType().InvokeMember(namestr, BindingFlags.GetProperty, null, item, null);//Unwrap.
				return GetPropertyValue(co.Ptr, namestr);
			}
			else if (Marshal.IsComObject(item))
			{
				return item.GetType().InvokeMember(namestr, BindingFlags.GetProperty, null, item, null);
			}

			throw new PropertyError($"Attempting to get property {name} on object {item} failed.");
		}

		public static object GetStaticMemberValueT<T>(object name)
		{
			if (Reflections.FindAndCacheProperty(typeof(T), name.ToString(), 0) is MethodPropertyHolder mph && mph.IsStaticProp)
			{
				try
				{
					return mph.callFunc(null, null);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}

			throw new PropertyError($"Attempting to get property {name} failed.");
		}

		public static object Invoke((object, MethodPropertyHolder) mitup, params object[] parameters)
		{
			try
			{
				var ret = mitup.Item2.callFunc(mitup.Item1, parameters);

				//The following check is done when accessing a class property that is a function object. The user intended to call it.
				//Catching this during compilation is very hard when calling it from outside of the class definition.
				//So catch it here instead.
				if (ret is IFuncObj fo && mitup.Item2.pi != null)
					return fo.Call(parameters);

				return ret;
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}
		}

		public static object SetPropertyValue(object item, object name, object value)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		{
			Type typetouse = null;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else if (item != null)
				typetouse = item.GetType();

			var namestr = name.ToString();

			if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph)
			{
				try
				{
					mph.setPropFunc(item, value);
					return value;
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
			else if (Reflections.FindAndCacheMethod(typetouse, "set_Item", 2) is MethodPropertyHolder mph1 && mph1.ParamLength == 2)
			{
				return mph1.callFunc(item, new object[] { namestr, value });
			}
			else if (item is ComObject co)
			{
				//_ = co.Ptr.GetType().InvokeMember(namestr, System.Reflection.BindingFlags.SetProperty, null, item, new object[] { value });//Unwrap.
				return SetPropertyValue(co.Ptr, namestr, value);
			}
			else if (Marshal.IsComObject(item))
			{
				_ = item.GetType().InvokeMember(namestr, System.Reflection.BindingFlags.SetProperty, null, item, new object[] { value });
				return value;
			}

			throw new PropertyError($"Attempting to set property {namestr} on object {item} to value {value} failed.");
		}

		public static void SetStaticMemberValueT<T>(object name, object value)
		{
			var namestr = name.ToString();

			if (Reflections.FindAndCacheProperty(typeof(T), namestr, 0) is MethodPropertyHolder mph && mph.IsStaticProp)
			{
				try
				{
					mph.setPropFunc(null, value);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
			else
				throw new PropertyError($"Attempting to set property {namestr} to value {value} failed.");
		}
	}
}