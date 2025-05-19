using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace Keysharp.Scripting
{
	public class Variables
	{
        public Dictionary<Type, KeysharpObject> Prototypes = new();
		public Dictionary<Type, KeysharpObject> Statics = new();
        internal List<(string, bool)> preloadedDlls = [];
		internal DateTime startTime = DateTime.UtcNow;
		internal PlatformManagerBase mgr;
		private readonly Dictionary<string, MemberInfo> globalVars = new (StringComparer.OrdinalIgnoreCase);
#if LINUX
		internal string ldLibraryPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? "";
		private Encoding enc1252 = Encoding.Default;
#endif

		/// <summary>
		/// Will be a generated call within Main which calls into this class to add DLLs.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="s"></param>
		public void AddPreLoadedDll(string p, bool s) => preloadedDlls.Add((p, s));

		public Variables(Type program = null)
		{
			if (program != null)
			{
				var fields = program.GetFields(BindingFlags.Static |
											BindingFlags.NonPublic |
											BindingFlags.Public);
				var props = program.GetProperties(BindingFlags.Static |
												BindingFlags.NonPublic |
												BindingFlags.Public);
				_ = globalVars.EnsureCapacity(fields.Length + props.Length);

				foreach (var field in fields)
					globalVars[field.Name] = field;

				foreach (var prop in props)
					globalVars[prop.Name] = prop;
			}
		}

		public void InitPrototypes()
		{
			// Initialize prototypes 
			Dictionary<Type, KeysharpObject> protos = new();

			var anyType = typeof(Any);
			var types = script.ReflectionsData.stringToTypes.Values
				.Concat(script.ReflectionsData.stringToTypes.Values.SelectMany(t => t.GetNestedTypes(BindingFlags.Public)))
				.Where(type => type.IsClass && !type.IsAbstract && anyType.IsAssignableFrom(type));

			/*
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && anyType.IsAssignableFrom(type));
			*/

			// Initiate necessary base types in specific order
			InitStaticInstance(typeof(FuncObj));
			// Need to do this so that FuncObj methods contain themselves in the prototype,
			// meaning a circular reference. This shouldn't prevent garbage collection, but
			// I haven't verified that.
			var fop = Prototypes[typeof(FuncObj)];
			foreach (var op in fop.op)
			{
				var opm = op.Value;
				if (opm.Value is FuncObj fov && fov != null)
					fov.op["base"] = new OwnPropsDesc(fov, fop);
				if (opm.Get is FuncObj fog && fog != null)
					fog.op["base"] = new OwnPropsDesc(fog, fop);
				if (opm.Set is FuncObj fos && fos != null)
					fos.op["base"] = new OwnPropsDesc(fos, fop);
				if (opm.Call is FuncObj foc && foc != null)
					foc.op["base"] = new OwnPropsDesc(foc, fop);
			}
			InitStaticInstance(typeof(Any), typeof(KeysharpObject));

			InitStaticInstance(typeof(Class));

			Statics[typeof(Class)].DefineProp("base", Collections.Map("value", Statics[typeof(Any)]));
			InitStaticInstance(typeof(KeysharpObject));
			Prototypes[typeof(Class)].DefineProp("base", Collections.Map("value", Prototypes[typeof(KeysharpObject)]));
			Statics[typeof(KeysharpObject)] = (KeysharpObject)Prototypes[typeof(KeysharpObject)].Clone();
			Statics[typeof(KeysharpObject)].DefineProp("prototype", Collections.Map("value", Prototypes[typeof(KeysharpObject)]));
			Prototypes[typeof(FuncObj)].DefineProp("base", Collections.Map("value", Prototypes[typeof(KeysharpObject)]));
			Statics[typeof(FuncObj)].DefineProp("base", Collections.Map("value", Statics[typeof(KeysharpObject)]));


			var typesToRemoveSet = new HashSet<Type>(new[] { typeof(Any), typeof(FuncObj), typeof(KeysharpObject), typeof(Class) });
			var orderedTypes = types.Where(type => !typesToRemoveSet.Contains(type)).OrderBy(GetInheritanceDepth);
			foreach (var t in orderedTypes)
			{
				Script.InitStaticInstance(t);
			}

			// Now that the static objects are created, loop the types again and call __Init and __New for all built-in classes
			foreach (var t in orderedTypes)
			{
				var nestedTypes = t.GetNestedTypes(BindingFlags.Public);
				foreach (var nestedType in nestedTypes)
				{
					Statics[t].DefineProp(nestedType.Name, Collections.Map("value", Statics[nestedType]));
				}
				if (t.Namespace.Equals("Keysharp.CompiledMain", StringComparison.InvariantCultureIgnoreCase))
				{
					Script.Invoke(Statics[t], "__Init");
					Script.Invoke(Statics[t], "__New");
				}
			}
		}

        private static int GetInheritanceDepth(Type type)
        {
            int depth = 0;
            while (type.BaseType != null)
            {
                depth++;
                type = type.BaseType;
            }
            return depth;
        }

		public bool HasVariable(string key) =>
			globalVars.ContainsKey(key)
			|| script.ReflectionsData.flatPublicStaticProperties.ContainsKey(key)
			|| script.ReflectionsData.flatPublicStaticMethods.ContainsKey(key);

        public object GetVariable(string key)
		{
			if (globalVars.TryGetValue(key, out var field))
			{
				if (field is PropertyInfo pi)
					return pi.GetValue(null);
				else if (field is FieldInfo fi)
					return fi.GetValue(null);
			}

			var rv = GetReservedVariable(key); // Try reserved variable first, to take precedence over IFuncObj
			if (rv != null)
				return rv;

			return Functions.GetFuncObj(key, null);

        }

		public object SetVariable(string key, object value)
		{
			if (globalVars.TryGetValue(key, out var field))
			{
				if (field is PropertyInfo pi)
					pi.SetValue(null, value);
				else if (field is FieldInfo fi)
					fi.SetValue(null, value);
			}
			else
				_ = SetReservedVariable(key, value);

			return value;
		}

		private PropertyInfo FindReservedVariable(string name)
		{
			_ = script.ReflectionsData.flatPublicStaticProperties.TryGetValue(name, out var prop);
			return prop;
		}

		private object GetReservedVariable(string name)
		{
			var prop = FindReservedVariable(name);
			return prop == null || !prop.CanRead ? null : prop.GetValue(null);
		}

		private bool SetReservedVariable(string name, object value)
		{
			var prop = FindReservedVariable(name);
			var set = prop != null && prop.CanWrite;

			if (set)
			{
				value = Script.ForceType(prop.PropertyType, value);
				prop.SetValue(null, value);
			}

			return set;
		}



		public object this[object key]
        {
			get => GetPropertyValue(key, "__Value", false) ?? GetVariable(key.ToString()) ?? "";
			set => _ = (key is KeysharpObject kso && Functions.HasProp(kso, "__Value") == 1) ? Script.SetPropertyValue(kso, "__Value", value) : SetVariable(key.ToString(), value);
		}

		public class Dereference
		{
            private readonly Dictionary<string, object> vars = new(StringComparer.OrdinalIgnoreCase);
			private eScope scope = eScope.Local;
			private HashSet<string> globals;
            public Dereference(eScope funcScope, HashSet<string> funcGlobals, params object[] args)
			{
				scope = funcScope;
				globals = funcGlobals;

				for (int i = 0; i < args.Length; i += 2)
				{
					if (args[i] is string varName)
					{
						vars[varName] = args[i + 1];
					}
				}
			}

            public object this[object key]
			{
				get
				{
					if (key is KeysharpObject)
						return GetPropertyValue(key, "__Value");
					if (vars.TryGetValue(key.ToString(), out var val))
						return GetPropertyValue(val, "__Value");
					return script.Vars[key];
				}
				set
				{
					if (key is KeysharpObject)
					{
						SetPropertyValue(key, "__Value", value);
						return;
					}

					var s = key.ToString();
					if (vars.TryGetValue(s, out var val))
					{
						SetPropertyValue(val, "__Value", value);
						return;
					}
					if ((scope == eScope.Global || (globals?.Contains(s) ?? false)) && script.Vars.HasVariable(s))
					{
						script.Vars[s] = value;
						return;
					}

					vars[s] = new VarRef(null);
                }
			}
        }
		}
}