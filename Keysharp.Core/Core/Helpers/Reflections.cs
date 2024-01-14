using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Keysharp.Scripting;
using sttd = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<int, Keysharp.Core.MethodPropertyHolder>>>;
using ttsd = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<int, Keysharp.Core.MethodPropertyHolder>>>;

namespace Keysharp.Core
{
	public static class Reflections
	{
		internal static Dictionary<string, MethodInfo> flatPublicStaticMethods = new Dictionary<string, MethodInfo>(500, StringComparer.OrdinalIgnoreCase);
		internal static Dictionary<string, PropertyInfo> flatPublicStaticProperties = new Dictionary<string, PropertyInfo>(200, StringComparer.OrdinalIgnoreCase);
		internal static Dictionary<string, Assembly> loadedAssemblies;
		internal static Dictionary<Type, Dictionary<string, FieldInfo>> staticFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
		internal static sttd stringToTypeBuiltInMethods = new sttd(sttcap, StringComparer.OrdinalIgnoreCase);
		internal static sttd stringToTypeLocalMethods = new sttd(sttcap / 10, StringComparer.OrdinalIgnoreCase);
		internal static sttd stringToTypeMethods = new sttd(sttcap, StringComparer.OrdinalIgnoreCase);
		internal static sttd stringToTypeProperties = new sttd(sttcap, StringComparer.OrdinalIgnoreCase);
		internal static int sttcap = 1000;
		internal static ttsd typeToStringMethods = new ttsd(sttcap / 5);
		internal static ttsd typeToStringProperties = new ttsd(sttcap / 5);
		internal static Dictionary<string, Type> stringToTypes = new Dictionary<string, Type>(sttcap / 4, StringComparer.OrdinalIgnoreCase);

		static Reflections() => Initialize();

		/// <summary>
		/// This should only ever be called from the unit tests.
		/// </summary>
		[PublicForTestOnly]
		public static void Clear()
		{
			staticFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
			stringToTypeBuiltInMethods = new sttd(sttcap, StringComparer.OrdinalIgnoreCase);
			stringToTypeLocalMethods = new sttd(sttcap / 10, StringComparer.OrdinalIgnoreCase);
			stringToTypeMethods = new sttd(sttcap, StringComparer.OrdinalIgnoreCase);
			stringToTypeProperties = new sttd(sttcap, StringComparer.OrdinalIgnoreCase);
			typeToStringMethods = new ttsd(sttcap / 5);
			typeToStringProperties = new ttsd(sttcap / 5);
			loadedAssemblies = new Dictionary<string, Assembly>();
			flatPublicStaticMethods = new Dictionary<string, MethodInfo>(500, StringComparer.OrdinalIgnoreCase);
			flatPublicStaticProperties = new Dictionary<string, PropertyInfo>(200, StringComparer.OrdinalIgnoreCase);
			stringToTypes = new Dictionary<string, Type>(sttcap / 4, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// This must be manually called before any program is run.
		/// Normally we'd put this kind of init in the static constructor, however it must be able to be manually called
		/// when running unit tests. Once upon init, then again within the unit test's auto generated program so it can find
		/// any locally declared methods inside.
		/// Also note that when running a script from Keysharp.exe, this will get called once when the parser starts in Keysharp, then again
		/// when the script actually runs. On the second time, there will be an extra assembly loaded, which is the compiled script itself. More system assemblies will also be loaded.
		/// </summary>
		[PublicForTestOnly]
		public static void Initialize()
		{
			loadedAssemblies = GetLoadedAssemblies();
			CacheAllMethods();
			CacheAllPropertiesAndFields();
			var types = loadedAssemblies.Values.Where(asm => asm.FullName.StartsWith("Keysharp.Core,"))
						.SelectMany(t => t.GetTypes())
						.Where(t => t.Namespace != null && t.Namespace.StartsWith("Keysharp.Core")
							   && t.Namespace != "Keysharp.Core.Properties"
							   && t.IsClass && t.IsPublic);
			var tl = types;

			foreach (var t in tl)
				stringToTypes[t.Name] = t;

			types = types.Where(t => t.IsSealed && t.IsAbstract);

			foreach (var property in types
					 .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Static))
					 .Where(p => p.GetCustomAttribute<PublicForTestOnly>() == null))
				flatPublicStaticProperties.TryAdd(property.Name, property);

			foreach (var method in types
					 .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
					 .Where(m => !m.IsSpecialName && m.GetCustomAttribute<PublicForTestOnly>() == null))
				flatPublicStaticMethods.TryAdd(method.Name, method);

#if DEBUG
			var mlist = flatPublicStaticMethods.Keys.ToList();
			mlist.Sort();
			var plist = flatPublicStaticProperties.Keys.ToList();
			plist.Sort();
			System.IO.File.WriteAllText("methpropskeysharp.txt", string.Join("\n", mlist.Select(m => $"{flatPublicStaticMethods[m].DeclaringType}.{m}()").OrderBy(s => s))
										+ "\n"
										+ string.Join("\n", plist.Select(p => $"{flatPublicStaticProperties[p].DeclaringType}.{p}").OrderBy(s => s)));
#endif
		}

		internal static FieldInfo FindAndCacheField(Type t, string name, BindingFlags propType =
					BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
		{
			try
			{
				do
				{
					if (staticFields.TryGetValue(t, out var dkt))
					{
					}
					else//Field on this type has not been used yet, so get all properties and cache.
					{
						var fields = t.GetFields(propType);

						if (fields.Length > 0)
						{
							foreach (var field in fields)
								staticFields.GetOrAdd(field.ReflectedType,
													  () => new Dictionary<string, FieldInfo>(fields.Length, StringComparer.OrdinalIgnoreCase))
								[field.Name] = field;
						}
						else//Make a dummy entry because this type has no fields. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
						{
							staticFields[t] = dkt = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
							t = t.BaseType;
							continue;
						}
					}

					if (dkt == null && !staticFields.TryGetValue(t, out dkt))
					{
						t = t.BaseType;
						continue;
					}

					if (dkt.TryGetValue(name, out var fi))//Since the Dictionary was created above with StringComparer.OrdinalIgnoreCase, this will be a case insensitive match.
						return fi;

					t = t.BaseType;
				} while (t.Assembly == typeof(Any).Assembly || t.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase));
			}
			catch (Exception)
			{
				throw;
			}

			return null;
		}

		internal static MethodPropertyHolder FindAndCacheMethod(Type t, string name, int paramCount, BindingFlags propType =
					BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly, bool isSystem = false)
		{
			do
			{
				if (typeToStringMethods.TryGetValue(t, out var dkt))
				{
				}
				else
				{
					var meths = t.GetMethods(propType);

					if (meths.Length > 0)
					{
						foreach (var meth in meths)
							typeToStringMethods.GetOrAdd(meth.ReflectedType,
														 () => new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(meths.Length, StringComparer.OrdinalIgnoreCase))
							.GetOrAdd(meth.Name)[meth.GetParameters().Length] = new MethodPropertyHolder(meth, null);
					}
					else//Make a dummy entry because this type has no methods. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
					{
						typeToStringMethods[t] = dkt = new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase);
						t = t.BaseType;
						continue;
					}
				}

				if (dkt == null && !typeToStringMethods.TryGetValue(t, out dkt))
				{
					t = t.BaseType;
					continue;
				}

				if (dkt.TryGetValue(name, out var methDkt))//Since the Dictionary was created above with StringComparer.OrdinalIgnoreCase, this will be a case insensitive match.
				{
					if (paramCount < 0 || methDkt.Count == 1)
						return methDkt.First().Value;
					else if (methDkt.TryGetValue(paramCount, out var mph))
						return mph;
				}

				t = t.BaseType;
			} while (t.Assembly == typeof(Any).Assembly

					 || t.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase)
					 || isSystem);//Traverse down to the base, but only do it for types that are part of this library. Once a base crosses the library boundary, the loop stops.

			return null;
		}

		internal static MethodPropertyHolder FindAndCacheProperty(Type t, string name, int paramCount, BindingFlags propType =
					BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly, bool isSystem = false)
		{
			try
			{
				do
				{
					if (typeToStringProperties.TryGetValue(t, out var dkt))
					{
					}
					else//Property on this type has not been used yet, so get all properties and cache.
					{
						var props = t.GetProperties(propType);

						if (props.Length > 0)
						{
							foreach (var prop in props)
								typeToStringProperties.GetOrAdd(prop.ReflectedType,
																() => new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(props.Length, StringComparer.OrdinalIgnoreCase))
								.GetOrAdd(prop.Name)[prop.GetIndexParameters().Length] = new MethodPropertyHolder(null, prop);
						}
						else//Make a dummy entry because this type has no properties. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
						{
							typeToStringProperties[t] = dkt = new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase);
							t = t.BaseType;
							continue;
						}
					}

					if (dkt == null && !typeToStringProperties.TryGetValue(t, out dkt))
					{
						t = t.BaseType;
						continue;
					}

					if (dkt.TryGetValue(name, out var propDkt))//Since the Dictionary was created above with StringComparer.OrdinalIgnoreCase, this will be a case insensitive match.
					{
						if (paramCount < 0 || propDkt.Count == 1)
							return propDkt.First().Value;
						else if (propDkt.TryGetValue(paramCount, out var mph))
							return mph;
					}

					t = t.BaseType;
				} while (t.Assembly == typeof(Any).Assembly

						 || t.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase)
						 || isSystem);
			}
			catch (Exception)
			{
				throw;
			}

			return null;
		}

		internal static MethodPropertyHolder FindBuiltInMethod(string name, int paramCount) =>
		FindMethod(stringToTypeBuiltInMethods, name, paramCount);

		internal static MethodPropertyHolder FindLocalMethod(string name, int paramCount) =>
		FindMethod(stringToTypeLocalMethods, name, paramCount);

		internal static MethodPropertyHolder FindMethod(string name, int paramCount) => FindLocalMethod(name, paramCount) is MethodPropertyHolder mph ? mph : FindBuiltInMethod(name, paramCount);

		internal static bool FindOwnProp(Type t, string name, bool userOnly = true)
		{
			name = name.ToLower();

			try
			{
				while (t != typeof(KeysharpObject))
				{
					if (userOnly && t.Assembly == typeof(Any).Assembly)
						break;

					if (typeToStringProperties.TryGetValue(t, out var dkt))
					{
						if (dkt.TryGetValue(name, out var prop))
							return true;
					}

					t = t.BaseType;
				}
			}
			catch (Exception)
			{
				throw;
			}

			return false;
		}

		internal static List<MethodPropertyHolder> GetOwnProps(Type t, bool userOnly = true)
		{
			var props = new List<MethodPropertyHolder>();

			try
			{
				while (t != typeof(KeysharpObject))
				{
					if (userOnly && t.Assembly == typeof(Any).Assembly)
						break;

					if (typeToStringProperties.TryGetValue(t, out var dkt))
					{
						foreach (var kv in dkt)
							if (kv.Key != "__Class" && kv.Value.Count > 0)
							{
								var mph = kv.Value.First().Value;

								if (mph.ParamLength == 0)//Do not add Index[] properties.
									props.Add(mph);
							}
					}

					t = t.BaseType;
				}
			}
			catch (Exception)
			{
				throw;
			}

			return props;
		}

		internal static long OwnPropCount(Type t, bool userOnly = true)
		{
			var ct = 0L;

			try
			{
				while (t != typeof(KeysharpObject))
				{
					if (userOnly && t.Assembly == typeof(Any).Assembly)
						break;

					if (typeToStringProperties.TryGetValue(t, out var dkt))
						ct += dkt.Count - 1;//Subtract 1 because of the auto generated __Class property.

					t = t.BaseType;
				}
			}
			catch (Exception)
			{
				throw;
			}

			return ct;
		}

		internal static bool SafeHasProperty(object item, string name) => item.GetType().GetProperties().Where(prop => prop.Name == name).Count() > 0;

		internal static T SafeGetProperty<T>(object item, string name) => (T)item.GetType().GetProperty(name, typeof(T))?.GetValue(item);

		internal static void SafeSetProperty(object item, string name, object value) => item.GetType().GetProperty(name, value.GetType())?.SetValue(item, value, null);

		private static void CacheAllMethods()
		{
			List<Assembly> assemblies;
			var loadedAssembliesList = loadedAssemblies.Values;
			stringToTypeLocalMethods.Clear();
			stringToTypeBuiltInMethods.Clear();

			if (AppDomain.CurrentDomain.FriendlyName == "testhost")//When running unit tests, the assembly names are changed for the auto generated program.
				assemblies = loadedAssembliesList.ToList();
			else if (Assembly.GetEntryAssembly().FullName.StartsWith("Keysharp,", StringComparison.OrdinalIgnoreCase))//Running from Keysharp.exe which compiled this script and launched it as a dynamically loaded assembly.
				assemblies = loadedAssembliesList.Where(assy => assy.Location.Length == 0 || assy.FullName.StartsWith("Keysharp.", StringComparison.OrdinalIgnoreCase)).ToList();//The . is important, it means only inspect Keysharp.Core because Keysharp, is the main Keysharp program, which we don't want to inspect. An assembly with an empty location is the compiled exe.
			else//Running as a standalone executable.
				assemblies = loadedAssembliesList.Where(assy => assy.FullName.StartsWith("Keysharp.", StringComparison.OrdinalIgnoreCase) ||
														(assy.EntryPoint != null &&
																assy.EntryPoint.DeclaringType != null &&
																assy.EntryPoint.DeclaringType.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase)
														)).ToList();

			//_ = MessageBox.Show(string.Join('\n', assemblies.Select(assy => assy.FullName)));

			foreach (var asm in assemblies)
				foreach (var type in asm.GetTypes())
					if (type.IsClass && type.IsPublic && type.Namespace != null &&
							(type.Namespace.StartsWith("Keysharp.Core", StringComparison.OrdinalIgnoreCase) ||
							 type.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase) ||
							 type.Namespace.StartsWith("Keysharp.Tests", StringComparison.OrdinalIgnoreCase)))//Allow tests so we can use function objects inside of unit tests.
						_ = FindAndCacheMethod(type, "", -1);

			var propType = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
			_ = FindAndCacheMethod(typeof(object[]), "", -1, propType);//Needs to be done manually because many of the properties are decalred in a base class.

			foreach (var typekv in typeToStringMethods)
			{
				foreach (var methkv in typekv.Value)
				{
					_ = stringToTypeMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);

					if (typekv.Key.FullName.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase) || typekv.Key.FullName.StartsWith("Keysharp.Tests", StringComparison.OrdinalIgnoreCase))//Need to include Tests so that unit tests will work.
						_ = stringToTypeLocalMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);
					else
						_ = stringToTypeBuiltInMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);
				}
			}
		}

		private static void CacheAllPropertiesAndFields()
		{
			typeToStringProperties.Clear();
			stringToTypeProperties.Clear();

			foreach (var item in loadedAssemblies.Values.Where(assy => assy.FullName.StartsWith("Keysharp")))
				foreach (var type in item.GetTypes())
					if (type.IsClass && type.IsPublic && type.Namespace != null &&
							(type.Namespace.StartsWith("Keysharp.Core", StringComparison.OrdinalIgnoreCase) ||
							 type.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase)))
					{
						_ = FindAndCacheProperty(type, "", 0);
						_ = FindAndCacheField(type, "");
					}

			var propType = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
			_ = FindAndCacheProperty(typeof(object[]), "", 0, propType); //Needs to be done manually because many of the properties are decalred in a base class.
			_ = FindAndCacheProperty(typeof(Exception), "", 0, propType); //Same.

			foreach (var typekv in typeToStringProperties)
				foreach (var propkv in typekv.Value)
					_ = stringToTypeProperties.GetOrAdd(propkv.Key).GetOrAdd(typekv.Key, propkv.Value);
		}

		private static MethodPropertyHolder FindMethod(sttd dkt, string name, int paramCount)
		{
			if (dkt.TryGetValue(name, out var meths))
				if (meths.Count > 0)
				{
					var first = meths.First().Value;

					if (paramCount < 0 || first.Count == 1)
						return first.First().Value;
					else if (first.TryGetValue(paramCount, out var mph))
						return mph;
				}

			return null;
		}

		private static Dictionary<string, Assembly> GetLoadedAssemblies()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var dkt = new Dictionary<string, Assembly>(assemblies.Length);

			foreach (var assembly in assemblies)
			{
				try
				{
					if (!assembly.IsDynamic)
						dkt[assembly.Location] = assembly;
				}
				catch (Exception ex)
				{
					Keysharp.Scripting.Script.OutputDebug(ex.Message);
				}
			}

			return dkt;
		}
	}

	internal class UnloadableAssemblyLoadContext : AssemblyLoadContext
	{
		private readonly AssemblyDependencyResolver resolver;

		public UnloadableAssemblyLoadContext(string mainAssemblyToLoadPath) : base(isCollectible: true) => resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);

		protected override Assembly Load(AssemblyName name)
		{
			var assemblyPath = resolver.ResolveAssemblyToPath(name);
			return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
		}
	}
}