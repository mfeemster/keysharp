using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Keysharp.Core;
#if WINDOWS
	using Keysharp.Core.COM;
#endif
namespace Keysharp.Scripting
{
	public partial class Script
	{
		//[ThreadStatic]
		//private static object lastItem = null;

		//[ThreadStatic]
		//private static string lastKey = null;

		//[ThreadStatic]
		//private static (object, object) lastMph = (null, null);

		//[ThreadStatic]
		//private static int lastParamCount = 0;
		public static object FindObjectForMethod(object obj, string name, int paramCount)
		{
			if (Reflections.FindAndCacheMethod(obj.GetType(), name, paramCount) is MethodPropertyHolder mph)
				return obj;

			if (Reflections.FindMethod(name, paramCount) is MethodPropertyHolder mph2)
				return null;

			throw new Error($"Could not find a class, global or built-in method for {name} with param count of {paramCount}.");
		}

		public static (object, object) GetMethodOrProperty(object item, string key, int paramCount)//This has to be public because the script will emit it in Main().
		{
			//if (ReferenceEquals(item, lastItem) && ReferenceEquals(key, lastKey) && paramCount == lastParamCount)
			//  return lastMph;
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
					//lastItem = item;
					//lastKey = key;
					//lastParamCount = paramCount;
					return /*lastMph = */ (item, mph0);
				}
			}
			else if (item is KeysharpObject kso && kso.op != null)
			{
				if (kso.op.TryGetValue(key, out var val) && val is OwnpropsMap map)
				{
					//Pass the ownprops map so that Invoke() knows to pass the parent object (item) as the first argument.
					if (map.map.TryGetValue("call", out var callval) && callval is IFuncObj ifo1)//Call must come first.
					{
						//lastItem = item;
						//lastKey = key;
						//lastParamCount = paramCount;
						return /*lastMph = */ (map, ifo1);
					}
					else if (map.map.TryGetValue("get", out var getval) && getval is IFuncObj ifo3)
					{
						//lastItem = item;
						//lastKey = key;
						//lastParamCount = paramCount;
						var getret = ifo3.Call();//No params passed in, just call as is.
						return /*lastMph =*/ (map, getret);
					}
					else if (map.map.TryGetValue("value", out var valval) && valval is IFuncObj ifo2)
					{
						//lastItem = item;
						//lastKey = key;
						//lastParamCount = paramCount;
						return /*lastMph = */ (map, ifo2);
					}

					throw new MemberError($"Attempting to get method or property {key} on object {map} failed.");
				}
			}

			if (Reflections.FindAndCacheMethod(typetouse, key, paramCount) is MethodPropertyHolder mph1)
			{
				//lastItem = item;
				//lastKey = key;
				//lastParamCount = paramCount;
				return /*lastMph = */ (item, mph1);
			}
			else if (Reflections.FindAndCacheProperty(typetouse, key, paramCount) is MethodPropertyHolder mph2)
			{
				//lastItem = item;
				//lastKey = key;
				//lastParamCount = paramCount;
				return /*lastMph = */ (item, mph2);
			}
			else if (Reflections.FindAndCacheMethod(typetouse, "get_Item", 1) is MethodPropertyHolder mph)//Last ditch attempt, see if it was a map entry, but was treated as a class property.
			{
				var val = mph.callFunc(item, new object[] { key });
				return (item, val);
			}

#if WINDOWS
			else if (item is ComObject co)
			{
				//return (co.Ptr, new ComMethodPropertyHolder(key));//Unwrap.
				return GetMethodOrProperty(co.Ptr, key, paramCount);
			}
			else if (Marshal.IsComObject(item))
			{
				return (item, new ComMethodPropertyHolder(key));
			}

#endif
			throw new MemberError($"Attempting to get method or property {key} on object {item} failed.");
		}

		public static object GetPropertyValue(object item, object name, bool throwOnError = true)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
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

			if (item is KeysharpObject kso && kso.op != null)
			{
				if (kso.op.TryGetValue(namestr, out var val) && val is OwnpropsMap map)
				{
					if (map.map.TryGetValue("value", out var valval))
						return valval;

					if (map.map.TryGetValue("get", out var getval) && getval is IFuncObj ifo)
						return ifo.Call(kso);
				}
			}

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

#if WINDOWS
			else if (item is ComObject co)
			{
				//return co.Ptr.GetType().InvokeMember(namestr, BindingFlags.GetProperty, null, item, null);//Unwrap.
				return GetPropertyValue(co.Ptr, namestr);
			}
			else if (Marshal.IsComObject(item))
			{
				return item.GetType().InvokeMember(namestr, BindingFlags.GetProperty, null, item, null);
			}

#endif

			if (throwOnError)
				throw new PropertyError($"Attempting to get property {name} on object {item} failed.");
			else
				return null;
		}

		public static object GetStaticMemberValueT<T>(object name)
		{
			var namestr = name.ToString();

			if (Reflections.FindAndCacheField(typeof(T), namestr) is FieldInfo fi && fi.IsStatic)
			{
				try
				{
					return fi.GetValue(null);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
			else if (Reflections.FindAndCacheProperty(typeof(T), namestr, 0) is MethodPropertyHolder mph && mph.IsStaticProp)
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
			else
				throw new PropertyError($"Attempting to get static property or field {name} failed.");
		}

		public static object Invoke((object, object) mitup, params object[] parameters)
		{
			try
			{
				object ret = null;

				if (mitup.Item2 is MethodPropertyHolder mph)
				{
					ret = mph.callFunc(mitup.Item1, parameters);

					//The following check is done when accessing a class property that is a function object. The user intended to call it.
					//Catching this during compilation is very hard when calling it from outside of the class definition.
					//So catch it here instead.
					if (ret is IFuncObj ifo1 && mph != null && mph.pi != null)
						return ifo1.Call(parameters);
				}
				else if (mitup.Item2 is IFuncObj ifo2)
				{
					//if (mitup.Item1 is OwnpropsMap opm)
					//{
					//  var arr = new object[parameters.Length + 1];
					//  arr[0] = opm.Parent;//Special logic here: this was called on an OwnProps map, so uses its parent as the object.
					//  System.Array.Copy(parameters, 0, arr, 1, parameters.Length);
					//  ret = ifo2.Call(arr);
					//  System.Array.Copy(arr, 1, parameters, 0, parameters.Length);//In case any params were references.
					//}
					if (mitup.Item1 is Keysharp.Core.Map)//Either Map or OwnpropsMap.
					{
						var lenIsZero = parameters.Length == 0;

						if (lenIsZero)
						{
							var arr = new object[2];
							arr[0] = mitup.Item1 is OwnpropsMap opm ? opm.Parent : mitup.Item1;
							ret = ifo2.Call(arr);
						}
						else
						{
							var arr = new object[parameters.Length + 1];
							arr[0] = mitup.Item1 is OwnpropsMap opm ? opm.Parent : mitup.Item1;
							System.Array.Copy(parameters, 0, arr, 1, parameters.Length);
							ret = ifo2.Call(arr);
							System.Array.Copy(arr, 1, parameters, 0, parameters.Length);//In case any params were references.
						}
					}
					else
						return ifo2.Call(parameters);
				}

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

		public static object InvokeWithRefs((object, object) mitup, params object[] parameters)
		{
			try
			{
				var refs = new List<RefHolder>(parameters.Length);

				for (var i = 0; i < parameters.Length; i++)
				{
					if (parameters[i] is RefHolder rh)
					{
						refs.Add(rh);
						parameters[i] = rh.val;
					}
				}

				object ret = null;

				if (mitup.Item2 is MethodPropertyHolder mph)
				{
					ret = mph.callFunc(mitup.Item1, parameters);

					//The following check is done when accessing a class property that is a function object. The user intended to call it.
					//Catching this during compilation is impossible when calling it from outside of the class definition.
					//So catch it here instead.
					if (ret is IFuncObj ifo1 && mph.pi != null)
						ret = ifo1.Call(parameters);
				}
				else if (mitup.Item2 is IFuncObj ifo2)
				{
					if (mitup.Item1 is Keysharp.Core.Map)//Either Map or OwnpropsMap.
					{
						var lenIsZero = parameters.Length == 0;

						if (lenIsZero)
						{
							var arr = new object[2];
							arr[0] = mitup.Item1 is OwnpropsMap opm ? opm.Parent : mitup.Item1;
							ret = ifo2.Call(arr);
						}
						else
						{
							for (var i = 0; i < refs.Count; i++)
								refs[i].index++;//Need to move the indices forward by one because of the additional parameter we'll add to the front below.

							var arr = new object[parameters.Length + 1];
							arr[0] = mitup.Item1 is OwnpropsMap opm ? opm.Parent : mitup.Item1;
							System.Array.Copy(parameters, 0, arr, 1, parameters.Length);
							ret = ifo2.Call(arr);
							parameters = arr;//For the reassign loop below, so the indices line up.
						}
					}
					else
						ret = ifo2.Call(parameters);
				}

				for (var i = 0; i < refs.Count; i++)
				{
					var rh = refs[i];
					rh.reassign(parameters[rh.index]);
				}

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

		public static (object, object) MakeObjectTuple(object obj0, object obj1) => (obj0, obj1);

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
			KeysharpObject kso = null;

			if ((kso = item as KeysharpObject) != null && kso.op != null)
			{
				if (kso.op.TryGetValue(namestr, out var val) && val is OwnpropsMap opm)
				{
					if (opm.map.ContainsKey("value"))
					{
						opm.map["value"] = value;
						return value;
					}

					if (opm.map.TryGetValue("set", out var setval) && setval is IFuncObj ifo)
					{
						var arr = new object[2];
						arr[0] = item;//Special logic here: this was called on an OwnProps map, so it uses its parent as the object.
						arr[1] = value;
						return ifo.Call(arr);
					}
				}
			}

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

#if WINDOWS
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

#endif
			else if (kso != null)//No property was present, so create one and assign the value to it.
			{
				_ = kso.DefineProp(namestr, Misc.Map("value", value));
				return value;
			}

			throw new PropertyError($"Attempting to set property {namestr} on object {item} to value {value} failed.");
		}

		public static void SetStaticMemberValueT<T>(object name, object value)
		{
			var namestr = name.ToString();

			if (Reflections.FindAndCacheField(typeof(T), namestr) is FieldInfo fi && fi.IsStatic)
			{
				try
				{
					fi.SetValue(null, value);
				}
				catch (Exception e)
				{
					if (e.InnerException is KeysharpException ke)
						throw ke;
					else
						throw;
				}
			}
			else if (Reflections.FindAndCacheProperty(typeof(T), namestr, 0) is MethodPropertyHolder mph && mph.IsStaticProp)
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
				throw new PropertyError($"Attempting to set static property or field {namestr} to value {value} failed.");
		}
	}
}