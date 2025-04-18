using System;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Windows;

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
        public static bool TryGetOwnPropsMap(KeysharpObject baseObj, string key, out OwnPropsDesc opm, bool searchBase = true)
        {
            opm = null;

            var ownProps = baseObj.op;
            if (ownProps != null && ownProps.TryGetValue(key, out opm))
                return true;
            if (!searchBase)
                return false;
			while (true)
			{
				if (!ownProps.TryGetValue("base", out var baseEntry) || baseEntry.Value == null)
					return false;
				ownProps = ((KeysharpObject)baseEntry.Value).op;
				if (ownProps != null && ownProps.TryGetValue(key, out opm))
					return true;
			}
        }
        public static (object, object) GetMethodOrProperty(object item, string key, int paramCount, bool checkBase = true)//This has to be public because the script will emit it in Main().
		{
			Error err;
			Type typetouse = null;
			KeysharpObject kso = null;

			try
			{
                if (item is KeysharpObject)
                    kso = (KeysharpObject)item;
                else if (item is ITuple otup && otup.Length > 1 && otup[0] is Type t)
				{
					typetouse = t; item = otup[1];
				}

                if (item == null)
				{
					if (Reflections.FindMethod(key, paramCount) is MethodPropertyHolder mph0)
						return (item, mph0);
				}
				else if (((typetouse != null && Variables.Prototypes.TryGetValue(typetouse, out kso)) || kso != null) && kso.op != null)
				{
					if (TryGetOwnPropsMap(kso, key, out var val))
					{
                        //Pass the ownprops map so that Invoke() knows to pass the parent object (item) as the first argument.
                        if (val.Call != null && val.Call is IFuncObj ifocall)//Call must come first.
                            return (item, ifocall);
                        else if (val.Get != null && val.Get is IFuncObj ifoget)
                            return (item, ifoget.Call(item));//No params passed in, just call as is.
                        else if (val.Value != null)
                            return (item, val.Value);
                        else if (val.Set != null && val.Set is IFuncObj ifoset)
                            return (item, ifoset);

                        return Errors.ErrorOccurred(err = new Error($"Attempting to get method or property {key} on object {val} failed.")) ? throw err : (null, null);
                    } else if (TryGetOwnPropsMap(kso, "__Call", out var protoCall) && protoCall.Call != null && protoCall.Call is IFuncObj ifoprotocall)
                        return (null, ifoprotocall.Bind(item, key));
                }

                if (typetouse == null)
                    typetouse = item.GetType();

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

		public static object GetPropertyValue(object item, object name, bool throwOnError = true, bool checkBase = true)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		
		{
			Error err;
			Type typetouse = null;
			var namestr = name.ToString();
			KeysharpObject kso = null;

			try
			{
                if (item is VarRef vr && namestr.Equals("__Value", StringComparison.OrdinalIgnoreCase))
					return vr.__Value;

                if (item is ITuple otup && otup.Length > 1)
				{
					if (otup[0] is Type t && otup[1] is object o)
					{
						typetouse = t;
						item = o;
						Variables.Prototypes.TryGetValue(t, out kso);
					}
				}
                
				if (typetouse == null && item != null)
                    typetouse = item.GetType();


                if ((kso != null || (kso = item as KeysharpObject) != null) && kso.op != null)
                {
                    if (TryGetOwnPropsMap(kso, namestr, out var opm))
					{
                        if (opm.Value != null)
                            return opm.Value;
                        else if (opm.Get != null && opm.Get is IFuncObj ifo && ifo != null)
                            return ifo.Call(item);
                        else if (opm.Call != null && opm.Call is IFuncObj ifo2 && ifo2 != null)
                            return ifo2;
						return null;
                    }
                    else if (TryGetOwnPropsMap(kso, "__Get", out var protoGet) && protoGet.Call != null && protoGet.Call is IFuncObj ifoprotoget)
                        return ifoprotoget.Call(item, new Keysharp.Core.Array(), namestr);

                    if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph2)
                    {
                        return mph2.callFunc(item, null);
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
					return GetPropertyValue(co.Ptr, namestr, throwOnError);
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

        public static bool GetObjectPropertyValue(object inst, object obj, string namestr, out object returnValue)
		{
			returnValue = null;
			if (!(obj is KeysharpObject kso) || kso.op == null)
				return false;
			if (kso.op.TryGetValue(namestr, out var val))
			{
				if (val.Value != null)
					returnValue = val.Value;
				else if (val.Get != null && val.Get is IFuncObj ifo && ifo != null)
					returnValue = ifo.Call(inst);
				else if (val.Call != null && val.Call is IFuncObj ifo2 && ifo2 != null)
					returnValue = ifo2;
				else
					return false;
				return true;
			}
			return false;
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

		internal static object InvokeMeta(object obj, string meth)
		{
			try { 
				var mitup = GetMethodOrProperty(obj, meth, -1);

				if (mitup.Item2 is MethodPropertyHolder mph)
					return mph.callFunc(mitup.Item1, null);
				else if (mitup.Item2 is IFuncObj ifo2)
				{
					if (mitup.Item1 == null) // __Call was found
						return null;
					return ifo2.Call(mitup.Item1);
				}
				else if (mitup.Item2 is KeysharpObject kso)
				{
					return Invoke(kso, "Call", obj);
				}
			}
            catch (Exception e)
            {
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
            }

            throw new MemberError($"Attempting to invoke method or property {meth} failed.");
}

        public static object Invoke(object obj, object meth, params object[] parameters)
        {
			if (obj == null)
				throw new UnsetError("Cannot invoke property on an unset variable");
            try
            {
                (object, object) mitup = (null, null);
                var methName = (string)meth;

				// Used with for example with Class.Call
                if (obj is IFuncObj fo2 && methName.Equals("Call", StringComparison.OrdinalIgnoreCase))
                {
                    return fo2.Call(parameters);
                } 
				else if (obj is ITuple otup && otup.Length > 1)
                {
                    mitup = GetMethodOrProperty(otup, methName, -1);
                    if (otup[1] is not ComObject)
                        mitup.Item1 = otup[1];
                }
                else
                {
                    mitup = GetMethodOrProperty(obj, methName, -1);
                }

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
					if (mitup.Item1 == null) // This means __Call was found and should be invoked
						return ifo2.Call(new Keysharp.Core.Array(parameters));

					if (parameters == null)
						return ifo2.Call(mitup.Item1);

					return ifo2.CallInst(mitup.Item1, parameters);
				}
				else if (mitup.Item2 is KeysharpObject kso && !methName.Equals("Call", StringComparison.OrdinalIgnoreCase))
				{
                    int count = parameters.Length;
                    object[] args = new object[count + 1];
                    args[0] = obj;
                    System.Array.Copy(parameters, 0, args, 1, count);
                    return Invoke(kso, "Call", args);
				}
            }
            catch (Exception e)
            {
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
            }

            throw new MemberError($"Attempting to invoke method or property {meth} failed.");
        }

        /*
        public static object Invoke(object obj, object meth, params object[] parameters)
		{
            try
            {
				(object, object) mitup = (null, null);
                IFuncObj f;
				var methName = (string)meth;
				if (meth is IFuncObj fo1)
					mitup = (obj, fo1);
				else if (obj is ITuple otup && otup.Length > 1)
				{
					mitup = GetMethodOrProperty(otup, methName, -1);
					if (otup[0] is object o && !(o is ComObject))
						mitup.Item1 = o;
				}
				else if (obj is IFuncObj fo2 && methName.Equals("Call", StringComparison.OrdinalIgnoreCase))
				{
					return fo2.Call(fo2.Inst == null ? parameters : new object[] { fo2.Inst }.Concat(parameters));
				}
				else
				{
					mitup = GetMethodOrProperty(obj, methName, -1);
					if (!(obj is ComObject))
						mitup.Item1 = obj;
				}

                return Invoke(mitup, parameters);
            }
            catch (Exception e)
            {
                if (e.InnerException is KeysharpException ke)
                    throw ke;
                else
                    throw;
            }

            throw new MemberError($"Attempting to invoke method or property {meth} failed.");
        }

		*/

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
						var lenIsZero = parameters == null || parameters.Length == 0;

						if (lenIsZero)
						{
							return ifo2.Call(mitup.Item1 is OwnPropsMap opm ? opm.Parent : mitup.Item1);
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
					{
						if (parameters == null)
                            return ifo2.Call(mitup.Item1);

                        int count = parameters.Length;
                        object[] args = new object[count + 1];
                        args[0] = mitup.Item1;
						System.Array.Copy(parameters, 0, args, 1, count);
                        return ifo2.Call(args);
                    }
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
            KeysharpObject kso = null;

            try
			{
				if (item is VarRef vr && namestr.Equals("__Value", StringComparison.InvariantCultureIgnoreCase))
				{
					vr.__Value = value;
					return value;
				}
				else if (item is ITuple otup && otup.Length > 1)
				{
					if (otup[0] is Type t && otup[1] is object o)
					{
						typetouse = t;
						item = o;
						Variables.Prototypes.TryGetValue(typetouse, out kso);
					}
                }

				if ((kso != null || (kso = item as KeysharpObject) != null) && kso.op != null)
				{
					if (kso.op.TryGetValue(namestr, out var own)) {
						if (own.Set != null && own.Set is IFuncObj ifo)
						{
							var arr = new object[2];
							arr[0] = item;//Special logic here: this was called on an OwnProps map, so it uses its parent as the object.
							arr[1] = value;
							return ifo.Call(item, value);
						}
						else
							return own.Value = value;
                    }
					if (TryGetOwnPropsMap(kso, namestr, out var opm))
					{
                        if (opm.Set != null && opm.Set is IFuncObj ifo)
                        {
                            var arr = new object[2];
                            arr[0] = item;//Special logic here: this was called on an OwnProps map, so it uses its parent as the object.
                            arr[1] = value;
                            return ifo.Call(item, value);
                        }

                    }
                    else if (TryGetOwnPropsMap(kso, "__Set", out var protoSet) && protoSet.Call != null && protoSet.Call is IFuncObj ifoprotoset)
                        return ifoprotoset.Call(item, namestr, new Keysharp.Core.Array(), value);
                }

                if (typetouse == null && item != null)
                    typetouse = item.GetType();

                if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph && namestr.ToLower() != "base")
				{
					mph.setPropFunc(item, value);
                    return value;
				}

#if WINDOWS
				//COM checks must come before Item checks because they can get confused sometimes and COM should take
				//precedence in such cases.
				else if (item is ComObject co && co.Ptr != null)
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
				else if (kso != null)//No property was present, so create one and assign the value to it.
				{
					_ = kso.DefineProp(namestr, Collections.MapWithoutBase("value", value));
					return value;
				}
				else if (Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", 2) is MethodPropertyHolder mph1 && mph1.ParamLength == 2)
				{
					return mph1.callFunc(item, [namestr, value]);
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