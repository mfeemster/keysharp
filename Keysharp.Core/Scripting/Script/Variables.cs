using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Keysharp.Core.Common.Cryptography;

namespace Keysharp.Scripting
{
	public class Variables
	{
        public LazyDictionary<Type, Any> Prototypes = new();
		public LazyDictionary<Type, Any> Statics = new();
        internal List<(string, bool)> preloadedDlls = [];
		internal DateTime startTime = DateTime.UtcNow;
		private readonly Dictionary<string, MemberInfo> globalVars = new (StringComparer.OrdinalIgnoreCase);
		public Type MainProgram = null;

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
				MainProgram = program;
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

		public void InitClasses()
		{
			// Initialize prototypes 
			Dictionary<Type, KeysharpObject> protos = new();

			var anyType = typeof(Any);
			var types = Script.TheScript.ReflectionsData.stringToTypes.Values
				.Where(type => type.IsClass && !type.IsAbstract && anyType.IsAssignableFrom(type));

			if (MainProgram != null)
			{
				var nested = Reflections.GetNestedTypes(MainProgram.GetNestedTypes());
				types = types.Concat(nested);
			}

			/*
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract && anyType.IsAssignableFrom(type));
			*/

			// Initiate necessary base types in specific order
			InitClass(typeof(FuncObj));
			// Need to do this so that FuncObj methods contain themselves in the prototype,
			// meaning a circular reference. This shouldn't prevent garbage collection, but
			// I haven't verified that.
			var fop = Prototypes[typeof(FuncObj)];
			foreach (var op in fop.op)
			{
				var opm = op.Value;
				if (opm.Value is FuncObj fov && fov != null)
					fov._base = fop;
				if (opm.Get is FuncObj fog && fog != null)
					fog._base = fop;
				if (opm.Set is FuncObj fos && fos != null)
					fos._base = fop;
				if (opm.Call is FuncObj foc && foc != null)
					foc._base = fop;
			}
			InitClass(typeof(Any));
			InitClass(typeof(KeysharpObject));
			InitClass(typeof(Class));

			// The static instance of Object is copied from Object prototype
			Statics[typeof(KeysharpObject)] = (Any)Prototypes[typeof(KeysharpObject)].Clone();
			// Class.Base == Object
			Statics[typeof(Class)]._base = Statics[typeof(KeysharpObject)];
			// Any.Base == Class.Prototype
			Statics[typeof(Any)]._base = Prototypes[typeof(Class)];

			// Manually define Object static instance prototype property to be the Object prototype
			var ksoStatic = Statics[typeof(KeysharpObject)];
			if (ksoStatic.op == null)
				ksoStatic.op = new Dictionary<string, OwnPropsDesc>(StringComparer.OrdinalIgnoreCase);
			ksoStatic.op["prototype"] = new OwnPropsDesc(ksoStatic, Prototypes[typeof(KeysharpObject)]);
			// Object.Base == Any
			ksoStatic._base = Statics[typeof(Any)];

			//FuncObj was initialized when Object wasn't, so define the bases
			Prototypes[typeof(FuncObj)]._base = Prototypes[typeof(KeysharpObject)];
			Statics[typeof(FuncObj)]._base = Prototypes[typeof(Class)];

			// Do not initialize the core types again
			var typesToRemoveSet = new HashSet<Type>(new[] { typeof(Any), typeof(FuncObj), typeof(KeysharpObject), typeof(Class) });
			var orderedTypes = types.Where(type => !typesToRemoveSet.Contains(type)).OrderBy(GetInheritanceDepth);

			// Lazy-initialize all other classes
			foreach (var t in orderedTypes)
			{
				Script.InitClass(t);
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
			|| Script.TheScript.ReflectionsData.flatPublicStaticProperties.ContainsKey(key)
			|| Script.TheScript.ReflectionsData.flatPublicStaticMethods.ContainsKey(key);

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
			_ = Script.TheScript.ReflectionsData.flatPublicStaticProperties.TryGetValue(name, out var prop);
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
			get => TryGetPropertyValue(key, "__Value", out object val) ? val : GetVariable(key.ToString()) ?? "";
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
					return Script.TheScript.Vars[key];
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
					if ((scope == eScope.Global || (globals?.Contains(s) ?? false)) && Script.TheScript.Vars.HasVariable(s))
					{
						Script.TheScript.Vars[s] = value;
						return;
					}

					vars[s] = new VarRef(null);
                }
			}
        }
		}
}