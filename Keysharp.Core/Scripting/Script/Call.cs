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
			if (Reflections.FindAndCacheInstanceMethod(obj.GetType(), name, paramCount) is MethodPropertyHolder mph)
				return obj;

			if (Reflections.FindAndCacheStaticMethod(obj.GetType(), name, paramCount) is MethodPropertyHolder mph2)
				return null;//This seems like a bug. Wouldn't we want to return the object?

			if (Reflections.FindMethod(name, paramCount) is MethodPropertyHolder mph3)
				return null;

			_ = Errors.ErrorOccurred($"Could not find a class, global or built-in method for {name} with param count of {paramCount}.");
			return null;
		}

		[Flags] public enum OwnPropsMapType
		{
			None = 0,
			Call = 1,
			Get = 2,
			Set = 4,
			Value = 8
		}
        public static bool TryGetOwnPropsMap(Any baseObj, string key, out OwnPropsDesc opm, bool searchBase = true, OwnPropsMapType type = 0)
        {
            opm = null;

            var ownProps = baseObj.op;
            if (ownProps != null && ownProps.TryGetValue(key, out opm))
                return true;
			if (key.Equals("base", StringComparison.OrdinalIgnoreCase))
			{
				opm = new OwnPropsDesc(baseObj, baseObj.Base);
				return true;
			}
			if (!searchBase)
                return false;
			while (true)
			{
				if ((baseObj = baseObj.Base) == null)
					return false;
				ownProps = baseObj.op;
				if (ownProps != null && ownProps.TryGetValue(key, out opm))
				{
					if (type == OwnPropsMapType.None)
						return true;
					if (opm.Call != null && (type & OwnPropsMapType.Call) != 0) return true;
					if (opm.Get != null && (type & OwnPropsMapType.Get) != 0) return true;
					if (opm.Set != null && (type & OwnPropsMapType.Set) != 0) return true;
					if (opm.Value != null && (type & OwnPropsMapType.Value) != 0) return true;
				}
			}
        }

		public static bool TryGetProps(Any baseObj, out Dictionary<string, OwnPropsDesc> props, bool searchBase = true, OwnPropsMapType type = 0)
		{
			OwnPropsDesc opm = null;
			props = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

			var ownProps = baseObj.op;

			while (ownProps != null && ownProps.Count != 0)
			{
				foreach (var prop in ownProps)
				{
					if (props.ContainsKey(prop.Key))
						continue;

					opm = prop.Value;

					if (type == OwnPropsMapType.None)
					{
						props[prop.Key] = prop.Value;
						continue;
					}
					
					if ((opm.Call != null && (type & OwnPropsMapType.Call) != 0) 
						|| (opm.Get != null && (type & OwnPropsMapType.Get) != 0)
						|| (opm.Set != null && (type & OwnPropsMapType.Set) != 0)
						|| (opm.Value != null && (type & OwnPropsMapType.Value) != 0))
						props[prop.Key] = prop.Value;
				}

				if (!searchBase)
					break;

				baseObj = baseObj.Base;
				if (baseObj == null)
					break;
				ownProps = baseObj.op;
			}

			return props.Count != 0;
		}

		public static (object, object) GetMethodOrProperty(object item, string key, int paramCount, bool checkBase = true, bool throwIfMissing = true, bool invokeMeta = true)//This has to be public because the script will emit it in Main().
		{
			Error err;
			Any kso = null;

			try
			{
                if (item is Any)
                    kso = (Any)item;
                else if (item is ITuple otup && otup.Length > 1 && otup[0] is Any t)
				{
					kso = t; item = otup[1];
				}

                if (kso != null && kso.op != null)
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
                    } else if (invokeMeta && TryGetOwnPropsMap(kso, "__Call", out var protoCall) && protoCall.Call != null && protoCall.Call is IFuncObj ifoprotocall)
                        return (null, ifoprotocall.Bind(item, key));
                }

				if (item == null)
				{
					if (Reflections.FindMethod(key, paramCount) is MethodPropertyHolder mph0)
						return (item, mph0);
				}
#if WINDOWS
				//COM checks must come before Item checks because they can get confused sometimes and COM should take
				//precedence in such cases.
				//This assumes the caller is never trying to retrieve properties or methods on the underlying
				//COM object that have the same name in ComObject, such as Ptr, __Delete() or Dispose().
				else if (item is ComObject co)
				{
					var ptr = co.Ptr;
					if (ptr != null && Marshal.IsComObject(ptr))
						return (ptr, new ComMethodPropertyHolder(key));
				}
#endif
				else if (item is not Any)
				{
					Type typetouse = item.GetType();

					if (Reflections.FindAndCacheInstanceMethod(typetouse, key, paramCount) is MethodPropertyHolder mph1)
					{
						return (item, mph1);
					}
					else if (Reflections.FindAndCacheProperty(typetouse, key, paramCount) is MethodPropertyHolder mph2)
					{
						return (item, mph2);
					}
					else if (Reflections.FindAndCacheInstanceMethod(typetouse, "get_Item", 1) is MethodPropertyHolder mph)//Last ditch attempt, see if it was a map entry, but was treated as a class property.
					{
						var val = mph.CallFunc(item, [key]);
						return (item, val);
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

			if (throwIfMissing)
				_ = Errors.ErrorOccurred($"Attempting to get method or property {key} on object {item} failed.");
			return (null, null);
		}

		public static object GetPropertyValue(object item, object name)
			=> TryGetPropertyValue(item, name, out object value)
				? value
				: Errors.ErrorOccurred($"Attempting to get property {name.ToString()} on object {item} failed.");

		public static object GetPropertyValue(object item, object name, object defaultValue) => 
			TryGetPropertyValue(item, name, out var value)
				 ? value 
				 : defaultValue;

		public static bool TryGetPropertyValue(object item, object name, out object value)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		{
			Type typetouse = null;
			var namestr = name.ToString();
			Any kso = null;

			try
			{
				if (item is VarRef vr && namestr.Equals("__Value", StringComparison.OrdinalIgnoreCase))
				{
					value = vr.__Value;
					return true;
				}

				if (item is ITuple otup && otup.Length > 1 && otup[0] is Any t)
				{
					kso = t;
					item = otup[1];
				}

				if ((kso != null || (kso = item as Any) != null))
				{
					if (TryGetOwnPropsMap(kso, namestr, out var opm))
					{
						if (opm.Value != null)
							value = opm.Value;
						else if (opm.Get != null && opm.Get is IFuncObj ifo && ifo != null)
							value = ifo.Call(item);
						else if (opm.Call != null && opm.Call is IFuncObj ifo2 && ifo2 != null)
							value = ifo2;
						else
						{
							value = null;
							return false;
						}
						return true;
					}
					else if (TryGetOwnPropsMap(kso, "__Get", out var protoGet) && (protoGet.Call != null ? protoGet.Call : protoGet.Value) is IFuncObj ifoprotoget && ifoprotoget != null)
					{
						value = ifoprotoget.Call(item, namestr, new Keysharp.Core.Array());
						return true;
					}
				}

#if WINDOWS
				if (item is ComObject co)
				{
					return TryGetPropertyValue(co.Ptr, namestr, out value);
				}
				else if (Marshal.IsComObject(item))
				{
					//Many COM properties are internally stored as methods with 0 parameters.
					//So try invoking the member as either a property or a method.
					value = item.GetType().InvokeMember(namestr, BindingFlags.InvokeMethod | BindingFlags.GetProperty, null, item, null);
					return true;
				}
#endif
				else if (item != null)
				{
					if (typetouse == null)
						typetouse = item.GetType();

					if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph2)
					{
						value = mph2.CallFunc(item, null);
						return true;
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

			value = null;
			return false;
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
			var namestr = name.ToString();

			try
			{
				if (Reflections.FindAndCacheField(typeof(T), namestr) is FieldInfo fi && fi.IsStatic)
				{
					return fi.GetValue(null);
				}
				else if (Reflections.FindAndCacheProperty(typeof(T), namestr, 0) is MethodPropertyHolder mph && mph.IsStaticProp)
				{
					return mph.CallFunc(null, null);
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

			return Errors.ErrorOccurred($"Attempting to get static property or field {namestr} failed.");
		}

		public static (object, MethodPropertyHolder) GetStaticMethodT<T>(object name, int paramCount)
		{
			if (Reflections.FindAndCacheStaticMethod(typeof(T), name.ToString(), paramCount) is MethodPropertyHolder mph && mph.mi != null && mph.IsStaticFunc)
				return (null, mph);

			_ = Errors.ErrorOccurred($"Attempting to get static method {name} failed.");
			return (DefaultErrorObject, null);
		}

		public static object InvokeMeta(object obj, object meth, params object[] parameters)
		{
			if (obj == null)
				throw new UnsetError("Cannot invoke property on an unset variable");
			try
			{
				(object, object) mitup = (null, null);
				var methName = (string)meth;

				mitup = GetMethodOrProperty(obj, methName, -1, checkBase: true, throwIfMissing: false, invokeMeta: false);

				if (mitup.Item2 == null)
					return null;

				if (obj is ITuple otup && otup.Length > 1)
				{
					if (otup[1] is not ComObject)
						mitup.Item1 = otup[1];
				}
				
				if (mitup.Item2 is IFuncObj ifo2)
				{
					if (mitup.Item1 == null) // This means __Call was found and should be invoked
						return ifo2.Call(new Keysharp.Core.Array(parameters));

					if (parameters == null)
						return ifo2.Call(mitup.Item1);

					return ifo2.CallInst(mitup.Item1, parameters);
				}
				else if (mitup.Item2 is KeysharpObject kso)
				{
					if (parameters.Length == 0)
						return Invoke(kso, "Call", obj);
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

				if (mitup.Item2 is MethodPropertyHolder mph)
				{
					//Mostly used by COM
					return mph.CallFunc(mitup.Item1, parameters);
				}
				else if (mitup.Item2 is IFuncObj ifo2)
				{
					if (mitup.Item1 == null) // This means __Call was found and should be invoked
						return ifo2.Call(new Keysharp.Core.Array(parameters));

					if (parameters == null)
						return ifo2.Call(mitup.Item1);

					return ifo2.CallInst(mitup.Item1, parameters);
				}
				else if (mitup.Item2 is KeysharpObject kso)
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

        public static object Invoke((object, object) mitup, params object[] parameters)
		{
			try
			{
				object ret = null;

				if (mitup.Item2 is MethodPropertyHolder mph)
				{
					//Mostly used by COM
					return mph.CallFunc(mitup.Item1, parameters);
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
					if (mitup.Item1 is Map || mitup.Item1 is OwnPropsDesc)
					{
						var lenIsZero = parameters == null || parameters.Length == 0;

						if (lenIsZero)
						{
							return ifo2.Call(mitup.Item1 is OwnPropsDesc opm ? opm.Parent : mitup.Item1);
						}
						else
						{
							var arr = new object[parameters.Length + 1];
							arr[0] = mitup.Item1 is OwnPropsDesc opm ? opm.Parent : mitup.Item1;
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

			return Errors.ErrorOccurred($"Attempting to invoke method or property {mitup.Item1},{mitup.Item2} failed.");
		}

		public static (object, object) MakeObjectTuple(object obj0, object obj1) => (obj0, obj1);

        public static object SetPropertyValue(object item, object name, object value, bool setAny = false)//Always assume these are not index properties, which we instead handle via method call with get_Item and set_Item.
		{
			Type typetouse = null;
			var namestr = name.ToString();
			Any any = null;

            try
			{
				if (item is VarRef vr && namestr.Equals("__Value", StringComparison.OrdinalIgnoreCase))
				{
					vr.__Value = value;
					return value;
				}
				else if (item is ITuple otup && otup.Length > 1 && otup[0] is Any t)
				{
					any = t; item = otup[1];
                }

				if ((any != null || (any = item as Any) != null))
				{
					if (any.op != null && any.op.TryGetValue(namestr, out var own)) {
						if (own.Set != null && own.Set is IFuncObj ifo)
						{
							var arr = new object[2];
							arr[0] = item;//Special logic here: this was called on an OwnProps map, so it uses its parent as the object.
							arr[1] = value;
							return ifo.Call(item, value) ?? value;
						}
						else if (own.Call == null && own.Get == null)
							return own.Value = value;
						else
							return Errors.PropertyErrorOccurred($"Property {namestr} on object {item} is read-only.");
					} 
					else if (namestr.Equals("base", StringComparison.OrdinalIgnoreCase))
					{
						any.Base = (KeysharpObject)value;
						return value;
					}
					if (TryGetOwnPropsMap(any, namestr, out var opm))
					{
						if (opm.Set != null && opm.Set is IFuncObj ifo)
						{
							var arr = new object[2];
							arr[0] = item;//Special logic here: this was called on an OwnProps map, so it uses its parent as the object.
							arr[1] = value;
							return ifo.Call(item, value) ?? value;
						}

					}
					else if (TryGetOwnPropsMap(any, "__Set", out var protoSet) && protoSet.Call != null && protoSet.Call is IFuncObj ifoprotoset)
						return ifoprotoset.Call(item, namestr, new Keysharp.Core.Array(), value);
                }

                if (typetouse == null && item != null)
                    typetouse = item.GetType();

                if (Reflections.FindAndCacheProperty(typetouse, namestr, 0) is MethodPropertyHolder mph && namestr.ToLower() != "base")
				{
					mph.SetProp(item, value);
					return value;
				}

#if WINDOWS
				//COM checks must come before Item checks because they can get confused sometimes and COM should take
				//precedence in such cases.
				else if (item is ComObject co && co.Ptr != null)
				{
					//_ = co.Ptr.GetType().InvokeMember(namestr, System.Reflection.BindingFlags.SetProperty, null, item, new object[] { value });//Unwrap.
					return SetPropertyValue(co.Ptr, namestr, value) ?? value;
				}
				else if (Marshal.IsComObject(item))
				{
					_ = item.GetType().InvokeMember(namestr, BindingFlags.SetProperty, null, item, [value]);
					return value;
				}

#endif
				else if (item is KeysharpObject kso)//No property was present, so create one and assign the value to it.
				{
					_ = kso.op[namestr] = new OwnPropsDesc(kso, value);
					return value;
				}
				else if (setAny && any != null)
				{
					any.op[namestr] = new OwnPropsDesc(any, value);
					return value;
				}
				else if (Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", 2) is MethodPropertyHolder mph1 && mph1.ParamLength == 2)
				{
					return mph1.CallFunc(item, [namestr, value]) ?? value;
				}
            }
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred($"Attempting to set property {namestr} on object {item} to value {value} failed.");
		}

		public static void SetStaticMemberValueT<T>(object name, object value)
		{
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
					mph.SetProp(null, value);
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

			_ = Errors.ErrorOccurred($"Attempting to set static property or field {namestr} to value {value} failed.");
		}
    }
}