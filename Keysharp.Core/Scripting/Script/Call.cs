namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static object FindObjectForMethod(object obj, string name, int paramCount)
		{
			Error err;

			if (Reflections.FindAndCacheInstanceMethod(obj.GetType(), name, paramCount) is MethodPropertyHolder mph)
				return obj;

			if (Reflections.FindAndCacheStaticMethod(obj.GetType(), name, paramCount) is MethodPropertyHolder mph2)
				return null;

			if (Reflections.FindMethod(name, paramCount) is MethodPropertyHolder mph3)
				return null;

			return Errors.ErrorOccurred(err = new Error($"Could not find a class, global or built-in method for {name} with param count of {paramCount}.")) ? throw err : null;
		}
		public static (object, object) GetMethodOrProperty(object item, string key, int paramCount)//This has to be public because the script will emit it in Main().
		{
			Error err;
			Type typetouse = null;

			try
			{
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
						return (item, mph0);
				}
				else if (item is KeysharpObject kso && kso.op != null)
				{
					if (kso.op.TryGetValue(key, out var val) && val is OwnPropsMap map)
					{
						//Pass the ownprops map so that Invoke() knows to pass the parent object (item) as the first argument.
						if (map.map.TryGetValue("call", out var callval) && callval is IFuncObj ifocall)//Call must come first.
							return (map, ifocall);
						else if (map.map.TryGetValue("get", out var getval) && getval is IFuncObj ifoget)
							return (map, ifoget.Call());//No params passed in, just call as is.
						else if (map.map.TryGetValue("value", out var valval) && valval is IFuncObj ifoval)
							return (map, ifoval);
						else if (map.map.TryGetValue("set", out var setval) && setval is IFuncObj ifoset)
							return (map, ifoset);

						return Errors.ErrorOccurred(err = new Error($"Attempting to get method or property {key} on object {map} failed.")) ? throw err : (null, null);
					}
				}

				if (Reflections.FindAndCacheInstanceMethod(typetouse, key, paramCount) is MethodPropertyHolder mph1)
				{
					return (item, mph1);
				}
				else if (Reflections.FindAndCacheProperty(typetouse, key, paramCount) is MethodPropertyHolder mph2)
				{
					return (item, mph2);
				}

#if WINDOWS
				//COM checks must come before Item checks because they can get confused sometimes and COM should take
				//precedence in such cases.
				else if (item is ComObject co)
				{
					return GetMethodOrProperty(co.Ptr, key, paramCount);
				}
				else if (Marshal.IsComObject(item))
				{
					return (item, new ComMethodPropertyHolder(key));
				}

#endif
				else if (Reflections.FindAndCacheInstanceMethod(typetouse, "get_Item", 1) is MethodPropertyHolder mph)//Last ditch attempt, see if it was a map entry, but was treated as a class property.
				{
					var val = mph.callFunc(item, [key]);
					return (item, val);
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to get method or property {key} on object {item} failed.")) ? throw err : (null, null);
		}

		public static object GetPropertyValue(object item, object name, bool throwOnError = true)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		{
			Error err;
			Type typetouse = null;
			var namestr = name.ToString();

			try
			{
				if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
				{
					typetouse = t;
					item = o;
				}
				else if (item != null)
					typetouse = item.GetType();

				if (item is KeysharpObject kso && kso.op != null)
				{
					if (kso.op.TryGetValue(namestr, out var val) && val is OwnPropsMap map)
					{
						if (map.map.TryGetValue("value", out var valval))
							return valval;

						if (map.map.TryGetValue("get", out var getval) && getval is IFuncObj ifo)
							return ifo.Call(kso);
					}
				}

				if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph)
				{
					return mph.callFunc(item, null);
				}

				//This is for the case where a Map accesses a key within the Map as if it were a property, so we try to get the indexer property, then pass the name of the passed in property as the key/index.
				//We check for a param length of 1 so we don't accidentally grab properties named Item which have no parameters, such as is the case with ComObject.
				//else if (Reflections.FindAndCacheInstanceMethod(typetouse, "get_Item", 1) is MethodPropertyHolder mph1 && mph1.ParamLength == 1)
				//{
				//  return mph1.callFunc(item, [namestr]);
				//}
#if WINDOWS
				else if (item is ComObject co)
				{
					return GetPropertyValue(co.Ptr, namestr);
				}
				else if (Marshal.IsComObject(item))
				{
					//Many COM properties are internally stored as methods with 0 parameters.
					//So try invoking the member as either a property or a method.
					return item.GetType().InvokeMember(namestr, BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, item, null);
				}

#endif
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			if (throwOnError)
				return Errors.ErrorOccurred(err = new Error($"Attempting to get property {namestr} on object {item} failed.")) ? throw err : null;
			else
				return null;
		}

		public static object GetStaticMemberValueT<T>(object name)
		{
			Error err;
			var namestr = name.ToString();

			try
			{
				if (Reflections.FindAndCacheField(typeof(T), namestr) is FieldInfo fi && fi.IsStatic)
				{
					return fi.GetValue(null);
				}
				else if (Reflections.FindAndCacheProperty(typeof(T), namestr, 0) is MethodPropertyHolder mph && mph.IsStaticProp)
				{
					return mph.callFunc(null, null);
				}
				else if (name is Delegate d)
				{
					return Functions.Func(d);
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to get static property or field {namestr} failed.")) ? throw err : null;
		}

		public static (object, MethodPropertyHolder) GetStaticMethodT<T>(object name, int paramCount)
		{
			Error err;

			if (Reflections.FindAndCacheStaticMethod(typeof(T), name.ToString(), paramCount) is MethodPropertyHolder mph && mph.mi != null && mph.IsStaticFunc)
				return (null, mph);

			return Errors.ErrorOccurred(err = new Error($"Attempting to get static method {name} failed.")) ? throw err : (null, null);
		}

		public static object Invoke((object, object) mitup, params object[] parameters)
		{
			Error err;

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

					return ret;
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
					if (mitup.Item1 is Map)//Either Map or OwnpropsMap.
					{
						var lenIsZero = parameters.Length == 0;

						if (lenIsZero)
						{
							var arr = new object[2];
							arr[0] = mitup.Item1 is OwnPropsMap opm ? opm.Parent : mitup.Item1;
							return ifo2.Call(arr);
						}
						else
						{
							var arr = new object[parameters.Length + 1];
							arr[0] = mitup.Item1 is OwnPropsMap opm ? opm.Parent : mitup.Item1;
							System.Array.Copy(parameters, 0, arr, 1, parameters.Length);
							ret = ifo2.Call(arr);
							System.Array.Copy(arr, 1, parameters, 0, parameters.Length);//In case any params were references.
							return ret;
						}
					}
					else
						return ifo2.Call(parameters);
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to invoke method or property {mitup.Item1},{mitup.Item2} failed.")) ? throw err : null;
		}

		public static object InvokeWithRefs((object, object) mitup, params object[] parameters)
		{
			Error err;

			try
			{
				var mph = mitup.Item2 as MethodPropertyHolder;
				var isFuncBind = mph != null && mph.IsBind;
				List<RefHolder> refs = null;

				//This is an extreme hack and I don't know how to get around it.
				//Bind is a very special function which needs the Mrh objects themselves to be passed.
				//Rather than the value held by the Mrh.
				if (!isFuncBind)
				{
					refs = new (parameters.Length);

					for (var i = 0; i < parameters.Length; i++)
					{
						if (parameters[i] is RefHolder rh)
						{
							refs.Add(rh);
							parameters[i] = rh.val;
						}
					}
				}

				object ret = null;
				var called = false;

				if (mph != null)
				{
					called = true;
					ret = mph.callFunc(mitup.Item1, parameters);//parameters won't have been changes in the case of IFuncObj.Bind().

					//The following check is done when accessing a class property that is a function object. The user intended to call it.
					//Catching this during compilation is impossible when calling it from outside of the class definition.
					//So catch it here instead.
					if (ret is IFuncObj ifo1 && mph.pi != null)
						ret = ifo1.Call(parameters);
				}
				else if (mitup.Item2 is IFuncObj ifo2)
				{
					called = true;

					if (mitup.Item1 is Map)//Either Map or OwnpropsMap.
					{
						var lenIsZero = parameters.Length == 0;

						if (lenIsZero)
						{
							var arr = new object[2];
							arr[0] = mitup.Item1 is OwnPropsMap opm ? opm.Parent : mitup.Item1;
							ret = ifo2.Call(arr);
						}
						else
						{
							if (refs != null)//Should always be not null here.
								for (var i = 0; i < refs.Count; i++)
									refs[i].index++;//Need to move the indices forward by one because of the additional parameter we'll add to the front below.

							var arr = new object[parameters.Length + 1];
							arr[0] = mitup.Item1 is OwnPropsMap opm ? opm.Parent : mitup.Item1;
							System.Array.Copy(parameters, 0, arr, 1, parameters.Length);
							ret = ifo2.Call(arr);
							parameters = arr;//For the reassign loop below, so the indices line up.
						}
					}
					else
						ret = ifo2.Call(parameters);
				}

				if (called)
				{
					if (!isFuncBind)
					{
						for (var i = 0; i < refs.Count; i++)
						{
							var rh = refs[i];
							rh.reassign(parameters[rh.index]);
						}
					}

					return ret;
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to invoke method or property {mitup.Item1},{mitup.Item2} with references failed.")) ? throw err : null;
		}
		public static (object, object) MakeObjectTuple(object obj0, object obj1) => (obj0, obj1);

		public static object SetPropertyValue(object item, object name, object value)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		{
			Error err;
			Type typetouse = null;
			var namestr = name.ToString();

			try
			{
				if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t && otup[1] is object o)
				{
					typetouse = t;
					item = o;
				}
				else if (item != null)
					typetouse = item.GetType();

				KeysharpObject kso = null;

				if ((kso = item as KeysharpObject) != null && kso.op != null)
				{
					if (kso.op.TryGetValue(namestr, out var val) && val is OwnPropsMap opm)
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
					mph.setPropFunc(item, value);
					return value;
				}

#if WINDOWS
				//COM checks must come before Item checks because they can get confused sometimes and COM should take
				//precedence in such cases.
				else if (item is ComObject co)
				{
					//_ = co.Ptr.GetType().InvokeMember(namestr, System.Reflection.BindingFlags.SetProperty, null, item, new object[] { value });//Unwrap.
					return SetPropertyValue(co.Ptr, namestr, value);
				}
				else if (Marshal.IsComObject(item))
				{
					_ = item.GetType().InvokeMember(namestr, BindingFlags.SetProperty, null, item, [value]);
					return value;
				}

#endif
				//else if (kso is Map m && Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", 2) is MethodPropertyHolder mph1 && mph1.ParamLength == 2)
				//{
				//  return mph1.callFunc(item, [namestr, value]);
				//}
				else if (kso != null)//No property was present, so create one and assign the value to it.
				{
					_ = kso.DefineProp(namestr, Collections.Map("value", value));
					return value;
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to set property {namestr} on object {item} to value {value} failed.")) ? throw err : null;
		}

		public static void SetStaticMemberValueT<T>(object name, object value)
		{
			Error err;
			var namestr = name.ToString();

			try
			{
				if (Reflections.FindAndCacheField(typeof(T), namestr) is FieldInfo fi && fi.IsStatic)
				{
					fi.SetValue(null, value);
					return;
				}
				else if (Reflections.FindAndCacheProperty(typeof(T), namestr, 0) is MethodPropertyHolder mph && mph.IsStaticProp)
				{
					mph.setPropFunc(null, value);
					return;
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			_ = Errors.ErrorOccurred(err = new Error($"Attempting to set static property or field {namestr} to value {value} failed.")) ? throw err : "";
		}
	}
}