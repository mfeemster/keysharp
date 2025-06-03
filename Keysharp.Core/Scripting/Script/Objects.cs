using System.Reflection;
using Keysharp.Core;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static void InitStaticInstance(Type t, Type alias = null)
		{
			var script = Script.TheScript;

			var isBuiltin = !t.Namespace.Equals("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase);

            var proto = script.Vars.Prototypes[t] = (KeysharpObject)RuntimeHelpers.GetUninitializedObject(alias ?? t);
			KeysharpObject staticInst = script.Vars.Statics[t] = (KeysharpObject)RuntimeHelpers.GetUninitializedObject(alias ?? t);

			proto.op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);
			staticInst.op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

			// Get all instance methods
			MethodInfo[] methods;

            if (isBuiltin && script.ReflectionsData.typeToStringMethods.ContainsKey(t))
                methods = script.ReflectionsData.typeToStringMethods[t]
                    .Values // Get Dictionary<string, Dictionary<int, MethodPropertyHolder>>
                    .SelectMany(m => m.Values) // Flatten to IEnumerable<Dictionary<int, MethodPropertyHolder>>
                    .Select(mph => mph.mi) // Flatten to IEnumerable<MethodPropertyHolder>
                    .ToArray();
            else
                methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
				var methodName = method.Name;

                bool isStatic = isBuiltin && method.IsStatic;
                if (methodName.StartsWith(Keywords.ClassStaticPrefix))
                {
                    isStatic = true;
                    methodName = methodName.Substring(Keywords.ClassStaticPrefix.Length);
                }

                if (methodName.StartsWith("get_") || methodName.StartsWith("set_"))
				{
					var propName = methodName.Substring(4);

                    if (propName == "Item")
						propName = "__Item";

					OwnPropsDesc propertyMap = new OwnPropsDesc();
					OwnPropsDesc propDesc;

					if (isStatic)
					{
						if (staticInst.op.TryGetValue(propName, out propDesc))
							propertyMap = propDesc;

					} 
					else
					{
                        if (proto.op.TryGetValue(propName, out propDesc))
                            propertyMap = propDesc;
                    }

                    if (methodName.StartsWith("get_"))
                        propertyMap.Get = new FuncObj(method);
                    else
                        propertyMap.Set = new FuncObj(method);

					if (isStatic)
						staticInst.op[propName] = propertyMap;
					else
						proto.op[propName] = propertyMap;

					continue;
                }

                if (isStatic)
                {
					DefineProp(staticInst, methodName, new OwnPropsDesc(staticInst, null, null, null, new FuncObj(method)));
					continue;
                }

				// Wrap method in FuncObj
				DefineProp(proto, methodName, new OwnPropsDesc(proto, null, null, null, new FuncObj(method)));
            }

			// Get all static methods
            if (isBuiltin && script.ReflectionsData.typeToStringStaticMethods.ContainsKey(t))
                methods = script.ReflectionsData.typeToStringStaticMethods[t]
                    .Values // Get Dictionary<string, Dictionary<int, MethodPropertyHolder>>
					.SelectMany(m => m.Values) // Flatten to IEnumerable<Dictionary<int, MethodPropertyHolder>>
					.Select(mph => mph.mi) // Flatten to IEnumerable<MethodPropertyHolder>
					.ToArray();
            else
                methods = [];

            foreach (var method in methods)
			{
				DefineProp(staticInst, method.Name, new OwnPropsDesc(staticInst, new FuncObj(method)));
            }

            // Get all instance and static properties

            PropertyInfo[] properties;

            if (isBuiltin && script.ReflectionsData.typeToStringProperties.ContainsKey(t))
                properties = script.ReflectionsData.typeToStringProperties[t]
                    .Values
                    .SelectMany(m => m.Values) 
                    .Select(mph => mph.pi)
                    .ToArray();
            else
                properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var prop in properties)
            {
                var propertyName = prop.Name;
                OwnPropsDesc propertyMap = null;
                if ((prop.GetMethod?.IsStatic ?? false) || (prop.SetMethod?.IsStatic ?? false) || (propertyName.StartsWith(Keywords.ClassStaticPrefix)))
				{
					if (propertyName.StartsWith(Keywords.ClassStaticPrefix))
						propertyName = propertyName.Substring(Keywords.ClassStaticPrefix.Length);
                    propertyMap = staticInst.op != null && staticInst.op.TryGetValue(propertyName, out OwnPropsDesc staticPropDesc) ? staticPropDesc : new OwnPropsDesc();

                    if (prop.GetMethod != null)
                    {
                        propertyMap.Get = new FuncObj(prop.GetMethod);
                    }

                    if (prop.SetMethod != null)
                    {
                        propertyMap.Set = new FuncObj(prop.SetMethod);
                    }

                    if (!propertyMap.IsEmpty)
                        staticInst.op[propertyName] = propertyMap;

					continue;
                }

                propertyMap = proto.op.TryGetValue(propertyName, out OwnPropsDesc propDesc) ? propDesc : new OwnPropsDesc();

                if (prop.GetMethod != null)
                {
                    propertyMap.Get = new FuncObj(prop.GetMethod);
                }

                if (prop.SetMethod != null)
                {
                    propertyMap.Set = new FuncObj(prop.SetMethod);
                }

                if (!propertyMap.IsEmpty)
                    proto.op[propertyName] = propertyMap;
            }

			if (!(t == typeof(Any) || t == typeof(FuncObj) || t == typeof(Class)))
				proto.op["base"] = new OwnPropsDesc(proto, script.Vars.Prototypes[t.BaseType]);

			if (isBuiltin)
			{
				string name = t.Name;
				if (Keywords.TypeNameAliases.ContainsKey(name))
					name = Keywords.TypeNameAliases[name];
				proto.op["__Class"] = new OwnPropsDesc(proto, name);
			}

			staticInst.op["prototype"] = new OwnPropsDesc(staticInst, script.Vars.Prototypes[t]);

			if (t != typeof(FuncObj) && t != typeof(Any) && t != typeof(Class))
				staticInst.op["base"] = new OwnPropsDesc(staticInst, t.BaseType == typeof(KeysharpObject) ? script.Vars.Prototypes[typeof(Class)] : script.Vars.Statics[t.BaseType]);

			if (!isBuiltin)
			{
				if (staticInst.op.TryGetValue("__Static", out var __static) && __static.Set != null)
				{
					if (__static.Set is IFuncObj ifo)
						ifo.Call((object)staticInst);
				}
				var nestedTypes = t.GetNestedTypes(BindingFlags.Public);
                foreach (var nestedType in nestedTypes)
                {
                    RuntimeHelpers.RunClassConstructor(nestedType.TypeHandle);
					DefineProp(script.Vars.Statics[t], nestedType.Name, 
						new OwnPropsDesc(script.Vars.Statics[t], null, 
							new FuncObj((params object[] args) => script.Vars.Statics[nestedType]),
							null,
							new FuncObj((object @this, params object[] args) => Script.Invoke(script.Vars.Statics[nestedType], "Call", args))
						)
					);
                }

                Script.InvokeMeta(script.Vars.Statics[t], "__Init");
                Script.InvokeMeta(script.Vars.Statics[t], "__New");
            }
        }

		internal static void DefineProp(KeysharpObject kso, string name, OwnPropsDesc desc)
		{
			if (kso.op == null)
				kso.op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);

			if (kso.op.TryGetValue(name, out var existing))
				existing.Merge(desc);
			else
				kso.op[name] = desc;
		}

		public static object Index(object item, params object[] index) => item == null ? null : IndexAt(item, index);

		public static object SetObject(object value, object item, params object[] index)
		{
			object key = null;
			Error err;
			Type typetouse = null;

			try
			{
				if (item is ITuple otup && otup.Length > 1)
				{
					if (otup[0] is Type t && otup[1] is object o)
					{
						typetouse = t;
						item = o;
					} else if (otup[0] is KeysharpObject kso && otup[1] is object ob)
					{
                        item = ob; typetouse = kso.GetType();
                    }
				}
				else
					typetouse = item.GetType();

				if (index.Length == 1)
				{
					key = index[0];

					//This excludes types derived from Array so that super can be used.
					if (typetouse == typeof(Keysharp.Core.Array))
					{
						((Keysharp.Core.Array)item)[key] = value;
						return value;
					}
					else if (typetouse == typeof(Keysharp.Core.Map))
					{
						((Keysharp.Core.Map)item)[key] = value;
						return value;
					}

					var position = (int)ForceLong(key);

					if (item is object[] objarr)
					{
						var actualindex = position < 0 ? objarr.Length + position : position - 1;
						objarr[actualindex] = value;
						return value;
					}
					else if (item is System.Array array)
					{
						var actualindex = position < 0 ? array.Length + position : position - 1;
						array.SetValue(value, actualindex);
						return value;
					}

#if WINDOWS
					else if (item is ComObjArray coa)
					{
						var actualindex = position < 0 ? coa.array.Length + position : position;
						coa.array.SetValue(value, actualindex);
						return value;
					}

#endif
					else if (item == null)
					{
						return null;
					}
				}

				if (item is KeysharpObject kso2)
				{
					if (TryGetOwnPropsMap(kso2, "__Item", out var opm)) {
						if (opm.Set != null && opm.Set is IFuncObj ifo)
							return ifo.Call([kso2, .. index, value]);
                        else if (opm.Call != null && opm.Call is IFuncObj ifo2)
                            return ifo2.Call([kso2, .. index, value]);
                    }
					else if (TryGetOwnPropsMap(kso2, "__Set", out var opm2) && opm2.Call != null && opm2.Call is IFuncObj ifo2)
						return ifo2.Call(kso2, new Keysharp.Core.Array(index), value);
                }

#if WINDOWS

				if (item is ComObject co)
				{
					if (index.Length == 0 && (co.vt & VarEnum.VT_BYREF) != 0)
					{
						ComObject.WriteVariant(co.Ptr.Al(), co.vt, value);
						return value;
					} else
						return co.Ptr.GetType().InvokeMember("Item", BindingFlags.SetProperty, null, co.Ptr, index.Concat([value]));
				}
				else if (Marshal.IsComObject(item))
					return item.GetType().InvokeMember("Item", BindingFlags.SetProperty, null, item, index.Concat([value]));

#endif
				var il1 = index.Length + 1;

				if (Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", il1) is MethodPropertyHolder mph2)
				{
					if (il1 == mph2.ParamLength || mph2.IsVariadic)
					{
						_ = mph2.callFunc(item, index.Concat([value]));
						return value;
					}
					else
						return Errors.ErrorOccurred(err = new ValueError($"{il1} arguments were passed to a set indexer which only accepts {mph2.ParamLength}.")) ? throw err : null;
				}
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to set index {key} of object {item} to value {value} failed.")) ? throw err : null;
		}

		private static object IndexAt(object item, params object[] index)
		{
			Error err;
			int len;
			object key = null;

			try
			{
				if (index != null && index.Length > 0)
				{
					len = index.Length;
					key = index[0];
				}
				else
					len = 1;

				KeysharpObject type = item as KeysharpObject;

                if (item is ITuple otup && otup.Length > 1 && otup[0] is KeysharpObject t)
				{
					type = t; item = otup[1];
				}

				if (type != null)
				{
					if (TryGetOwnPropsMap(type, "__Item", out var opm, true, OwnPropsMapType.Get) && opm.Get is IFuncObj ifo)
						return ifo.Call([item, .. index]);
					else if (TryGetOwnPropsMap(type, "__Get", out var opm2, true, OwnPropsMapType.Call) && opm2.Call is IFuncObj ifo2)
						return ifo2.Call(item, new Keysharp.Core.Array(index));
				}

				if (len == 1)
				{
					var position = (int)ForceLong(key);

					//The most common is going to be a string, array, map or buffer.
					if (item is string s)
					{
						var actualindex = position < 0 ? s.Length + position : position - 1;
						return s[actualindex];
					}
					else if (item is object[] objarr)//Used for indexing into variadic function params.
					{
						var actualindex = position < 0 ? objarr.Length + position : position - 1;
						return objarr[actualindex];
					}
					else if (item is System.Array array)
					{
						var actualindex = position < 0 ? array.Length + position : position - 1;
						return array.GetValue(actualindex);
					}

#if WINDOWS
                    else if (item is ComObjArray coa)
					{
						var actualindex = position < 0 ? coa.array.Length + position : position;
						return coa.array.GetValue(actualindex);
					}
#endif
				}

#if WINDOWS

				if (item is ComObject co)
				{
					//Could be an indexer, but MethodPropertyHolder currently doesn't support those
					if (index.Length == 0 && (co.vt & VarEnum.VT_BYREF) != 0)
						return ComObject.ReadVariant(co.Ptr.Al(), co.vt);
					return Invoke((co.Ptr, new ComMethodPropertyHolder("Item")), index);
				}
				else if (Marshal.IsComObject(item))
					return Invoke((item, new ComMethodPropertyHolder("Item")), index);

#endif
			}
			catch (Exception e)
			{
				if (e.InnerException is KeysharpException ke)
					throw ke;
				else
					throw;
			}

			return Errors.ErrorOccurred(err = new Error($"Attempting to get index of {key} on item {item} failed.")) ? throw err : null;
		}
	}
}