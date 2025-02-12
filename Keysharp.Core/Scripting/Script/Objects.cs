using System.Reflection;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static void InitStaticInstance(Type t, Type alias = null)
		{
            var isBuiltin = !t.Namespace.Equals("Keysharp.CompiledMain", StringComparison.InvariantCultureIgnoreCase);

            Variables.Prototypes[t] = (KeysharpObject)RuntimeHelpers.GetUninitializedObject(alias ?? t);
            object inst = Script.Variables.Statics[t] = (KeysharpObject)RuntimeHelpers.GetUninitializedObject(alias ?? t);
			KeysharpObject staticInst = (KeysharpObject)inst;

            var proto = Variables.Prototypes[t];

            // Get all instance methods
            MethodInfo[] methods;

            if (isBuiltin && Reflections.typeToStringMethods.ContainsKey(t))
                methods = Reflections.typeToStringMethods[t]
                    .Values // Get Dictionary<string, Dictionary<int, MethodPropertyHolder>>
                    .SelectMany(m => m.Values) // Flatten to IEnumerable<Dictionary<int, MethodPropertyHolder>>
                    .Select(mph => mph.mi) // Flatten to IEnumerable<MethodPropertyHolder>
                    .ToArray();
            else
                methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
				var methodName = method.Name;
                if (!isBuiltin && methodName.StartsWith(Keywords.ClassStaticPrefix))
                {
					methodName = methodName.Substring(Keywords.ClassStaticPrefix.Length);
                    staticInst.DefineProp(methodName, Collections.MapWithoutBase("call", new FuncObj(method)));
					continue;
                }

                // Wrap method in FuncObj
                proto.DefineProp(methodName, Collections.MapWithoutBase("call", new FuncObj(method)));
            }

			// Get all static methods
            if (isBuiltin && Reflections.typeToStringStaticMethods.ContainsKey(t))
                methods = Reflections.typeToStringStaticMethods[t]
                    .Values // Get Dictionary<string, Dictionary<int, MethodPropertyHolder>>
					.SelectMany(m => m.Values) // Flatten to IEnumerable<Dictionary<int, MethodPropertyHolder>>
					.Select(mph => mph.mi) // Flatten to IEnumerable<MethodPropertyHolder>
					.ToArray();
            else
                methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

            methods = t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
			{
                SetPropertyValue(staticInst, method.Name, new FuncObj(method));
            }

            // Get all instance and static properties

            PropertyInfo[] properties;

            if (isBuiltin && Reflections.typeToStringProperties.ContainsKey(t))
                properties = Reflections.typeToStringProperties[t]
                    .Values
                    .SelectMany(m => m.Values) 
                    .Select(mph => mph.pi)
                    .ToArray();
            else
                properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var prop in properties)
            {
                var propertyName = prop.Name;
				OwnPropsMap propertyMap = null;
                if (prop.GetMethod.IsStatic || (!isBuiltin && propertyName.StartsWith(Keywords.ClassStaticPrefix)))
				{
					if (propertyName.StartsWith(Keywords.ClassStaticPrefix))
						propertyName = propertyName.Substring(Keywords.ClassStaticPrefix.Length);
                    propertyMap = staticInst.op.TryGetValue(propertyName, out OwnPropsMap staticPropDesc) ? staticPropDesc : new OwnPropsMap(null, Collections.Map());

                    if (prop.GetMethod != null)
                    {
                        propertyMap["get"] = new FuncObj(prop.GetMethod);
                    }

                    if (prop.SetMethod != null)
                    {
                        propertyMap["set"] = new FuncObj(prop.SetMethod);
                    }

                    if (propertyMap.Count > 0)
                        staticInst.op[propertyName] = propertyMap;

					continue;
                }

                propertyMap = proto.op.TryGetValue(propertyName, out OwnPropsMap propDesc) ? propDesc : new OwnPropsMap(null, Collections.Map());

                if (prop.GetMethod != null)
                {
                    propertyMap["get"] = new FuncObj(prop.GetMethod);
                }

                if (prop.SetMethod != null)
                {
                    propertyMap["set"] = new FuncObj(prop.SetMethod);
                }

                if (propertyMap.Count > 0)
                    proto.op[propertyName] = propertyMap;
            }

            if (!(t == typeof(Any) || t == typeof(FuncObj)))
                proto.DefineProp("base", Collections.MapWithoutBase("value", Variables.Prototypes[t.BaseType]));

            staticInst.DefineProp("prototype", Collections.Map("value", Variables.Prototypes[t]));

			if (t != typeof(FuncObj) && t != typeof(Any))
				staticInst.DefineProp("base", Collections.MapWithoutBase("value", Variables.Statics[t.BaseType]));

			/*
			if (!isBuiltin)
			{
                Script.Invoke(Script.Variables.Statics[t], "__Init");
                Script.Invoke(Script.Variables.Statics[t], "__New");
            }
			*/
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
                        item = kso; typetouse = ob.GetType();
                    }
				}
				else
					typetouse = item.GetType();

				if (index.Length == 1)
				{
					key = index[0];
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
				else if (index.Length == 0)//Access brackets with no index like item.prop[] := 123.
				{
					if (Reflections.FindAndCacheInstanceMethod(typetouse, "set_Item", 0) is MethodPropertyHolder mph1)
					{
						_ = mph1.callFunc(item, index.Concat([value]));
						return value;
					}
				}

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

				Type typetouse = null;

				if (item is ITuple otup && otup.Length > 1)
				{
					if (otup[0] is Type t && otup[1] is object o)
					{
						typetouse = t;
						item = o;
					} else if (otup[0] is KeysharpObject kso && otup[1] is object ob)
					{
						item = kso; typetouse = ob.GetType();
					}
				}
				else
					typetouse = item.GetType();

				if (len == 1)
				{
					//The most common is going to be a string, array, map or buffer.
					if (item is string s)
					{
						var position = (int)ForceLong(key);
						var actualindex = position < 0 ? s.Length + position : position - 1;
						return s[actualindex];
					}
					else if (item is object[] objarr)//Used for indexing into variadic function params.
					{
						var position = (int)ForceLong(key);
						var actualindex = position < 0 ? objarr.Length + position : position - 1;
						return objarr[actualindex];
					}
					else if (item is Core.Buffer buf)
					{
						var position = (int)ForceLong(key);
						return buf[position];
					}
					else if (item is System.Array array)
					{
						var position = (int)ForceLong(key);
						var actualindex = position < 0 ? array.Length + position : position - 1;
						return array.GetValue(actualindex);
					}

#if WINDOWS
					else if (item is ComObjArray coa)
					{
						var position = (int)ForceLong(key);
						var actualindex = position < 0 ? coa.array.Length + position : position;
						return coa.array.GetValue(actualindex);
					}

#endif
					//These are probably never used.
					/*  else if (typeof(IEnumerable).IsAssignableFrom(item.GetType()))
					    {
					    var ienum = (IEnumerable)item;
					    var enumerator = ienum.GetEnumerator();
					    var i = 0;
					    var len = 0;
					    var tempenum = ienum.GetEnumerator();

					    while (tempenum.MoveNext())
					        len++;

					    var actualindex = position < 0 ? len + position : position - 1;

					    while (enumerator.MoveNext())
					    {
					        if (i == actualindex)
					            return enumerator.Current;

					        i++;
					    }

					    return null;
					    }*/
				}

				if (Reflections.FindAndCacheInstanceMethod(typetouse, "get_Item", len) is MethodPropertyHolder mph)
				{
					if (len == mph.ParamLength || mph.IsVariadic)
						return mph.callFunc(item, index);
					else
						return Errors.ErrorOccurred(err = new ValueError($"{len} arguments were passed to a get indexer which only accepts {mph.ParamLength}.")) ? throw err : null;
				}
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