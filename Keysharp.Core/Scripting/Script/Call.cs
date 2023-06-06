using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Keysharp.Core;

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
			else if (Reflections.FindAndCacheMethod(typetouse, "get_Item", 1) is MethodPropertyHolder mph1)
			{
				return mph1.callFunc(item, new object[] { name });
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
				return mitup.Item2.callFunc(mitup.Item1, parameters);
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
			else if (Reflections.FindAndCacheMethod(typetouse, "set_Item", 2) is MethodPropertyHolder mph1)
			{
				return mph1.callFunc(item, new object[] { name, value });
			}
			else if (Marshal.IsComObject(item))
			{
				_ = item.GetType().InvokeMember(namestr, System.Reflection.BindingFlags.SetProperty, null, item, new object[] { value });
				return value;
			}
			else
				throw new PropertyError($"Attempting to set property {name} on object {item} to value {value} failed.");
		}

		public static void SetStaticMemberValueT<T>(object name, object value)
		{
			if (Reflections.FindAndCacheProperty(typeof(T), name.ToString(), 0) is MethodPropertyHolder mph && mph.IsStaticProp)
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
				throw new PropertyError($"Attempting to set property {name} to value {value} failed.");
		}
	}
}