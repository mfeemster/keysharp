using System;
using System.Reflection;
using System.Runtime.CompilerServices;
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
		private static (object, MethodPropertyHolder) lastMph = (null, null);

		public static (object, MethodPropertyHolder) GetMethodOrProperty(object item, string key)//This has to be public because the script will emit it in Main().
		{
			if (ReferenceEquals(item, lastItem) && ReferenceEquals(key, lastKey))
				return lastMph;

			if (item == null)
			{
				if (Reflections.FindMethod(key) is MethodPropertyHolder mph0)
				{
					lastItem = item;
					lastKey = key;
					return lastMph = (item, mph0);
				}
			}
			else if (Reflections.FindAndCacheMethod(item.GetType(), key) is MethodPropertyHolder mph1)
			{
				lastItem = item;
				lastKey = key;
				return lastMph = (item, mph1);
			}
			else if (Reflections.FindAndCacheProperty(item.GetType(), key) is MethodPropertyHolder mph2)
			{
				lastItem = item;
				lastKey = key;
				return lastMph = (item, mph2);
			}

			throw new MemberError($"Attempting to get method or property {item} with key {key} failed.");
		}

		public static object GetPropertyValue(object item, object name)
		{
			Type typetouse;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else
				typetouse = item.GetType();

			if (Reflections.FindAndCacheProperty(typetouse, name.ToString()) is MethodPropertyHolder mph)
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
			else if (item is Map map)
			{
				if (map.map.TryGetValue(name, out var val))
					return val;
			}

			throw new PropertyError($"Attempting to get property {name} on object {item} failed.");
		}

		public static object GetStaticMemberValueT<T>(object name)
		{
			if (Reflections.FindAndCacheProperty(typeof(T), name.ToString()) is MethodPropertyHolder mph && mph.IsStaticProp)
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

		public static void SetPropertyValue(object item, object name, object value)
		{
			Type typetouse;

			if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
			{
				typetouse = t;
				item = o;
			}
			else
				typetouse = item.GetType();

			if (Reflections.FindAndCacheProperty(typetouse, name.ToString()) is MethodPropertyHolder mph)
			{
				try
				{
					mph.setPropFunc(item, value);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
			else if (item is Map map)//If it wasn't a property, try it as a key to a map.
				map[name] = value;
			else
				throw new PropertyError($"Attempting to set property {name} on object {item} to value {value} failed.");
		}

		public static void SetStaticMemberValueT<T>(object name, object value)
		{
			if (Reflections.FindAndCacheProperty(typeof(T), name.ToString()) is MethodPropertyHolder mph && mph.IsStaticProp)
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