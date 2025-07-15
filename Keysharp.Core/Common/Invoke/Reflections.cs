//#define CONCURRENT
#if CONCURRENT

using sttd = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<System.Type, System.Collections.Concurrent.ConcurrentDictionary<int, Keysharp.Core.Common.Invoke.MethodPropertyHolder>>>;
using ttsd = System.Collections.Concurrent.ConcurrentDictionary<System.Type, System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<int, Keysharp.Core.Common.Invoke.MethodPropertyHolder>>>;

#else

using sttd = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<int, Keysharp.Core.Common.Invoke.MethodPropertyHolder>>>;
using ttsd = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<int, Keysharp.Core.Common.Invoke.MethodPropertyHolder>>>;

#endif

namespace Keysharp.Core.Common.Invoke
{
	internal class ReflectionsData
	{
		internal Dictionary<string, MethodInfo> flatPublicStaticMethods = new (500, StringComparer.OrdinalIgnoreCase);
		internal Dictionary<string, PropertyInfo> flatPublicStaticProperties = new (200, StringComparer.OrdinalIgnoreCase);
		internal Dictionary<string, Assembly> loadedAssemblies;
		internal Dictionary<Type, Dictionary<string, FieldInfo>> staticFields = [];

#if CONCURRENT
		internal const int sttcap = 1000;
		internal sttd stringToTypeBuiltInMethods = new (StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeLocalMethods = new (StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeMethods = new (StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeStaticMethods = new (StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeProperties = new (StringComparer.OrdinalIgnoreCase);
		internal Dictionary<string, Type> stringToTypes = new (StringComparer.OrdinalIgnoreCase);
		internal ttsd typeToStringMethods = new ();
		internal ttsd typeToStringStaticMethods = new ();
		internal ttsd typeToStringProperties = new ();
#else
		internal const int sttcap = 1000;
		internal sttd stringToTypeBuiltInMethods = new (sttcap, StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeLocalMethods = new (sttcap / 10, StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeMethods = new (sttcap, StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeStaticMethods = new (sttcap, StringComparer.OrdinalIgnoreCase);
		internal sttd stringToTypeProperties = new (sttcap, StringComparer.OrdinalIgnoreCase);
		internal Dictionary<string, Type> stringToTypes = new (sttcap / 4, StringComparer.OrdinalIgnoreCase);
		internal ttsd typeToStringMethods = new (sttcap / 5);
		internal ttsd typeToStringStaticMethods = new (sttcap / 5);
		internal ttsd typeToStringProperties = new (sttcap / 5);
#endif
		internal readonly Lock locker = new ();
	}

	public class Reflections
	{
		public Reflections()
		{
			Initialize();
		}

		/// <summary>
		/// This must be manually called before any program is run.
		/// Normally we'd put this kind of init in the constructor, however it must be able to be manually called
		/// when running unit tests. Once upon init, then again within the unit test's auto generated program so it can find
		/// any locally declared methods inside.
		/// Also note that when running a script from Keysharp.exe, this will get called once when the parser starts in Keysharp, then again
		/// when the script actually runs. On the second time, there will be an extra assembly loaded, which is the compiled script itself. More system assemblies will also be loaded.
		/// </summary>
		[PublicForTestOnly]
		public static void Initialize(bool ignoreMainAssembly = false)
		{
			var rd = Script.TheScript.ReflectionsData;
			rd.loadedAssemblies = GetLoadedAssemblies();
			CacheAllMethods(ignoreMainAssembly);
			CacheAllPropertiesAndFields();
			var types = rd.loadedAssemblies.Values.Where(asm => asm.FullName.StartsWith("Keysharp.Core,"))
						.SelectMany(t => t.GetExportedTypes())
						.Where(t => t.GetCustomAttribute<PublicForTestOnly>() == null && t.Namespace != null && t.Namespace.StartsWith("Keysharp.Core")
							   && t.Namespace != "Keysharp.Core.Properties"
							   && t.IsClass && (t.IsPublic || t.IsNestedPublic));
			var tl = types;

			foreach (var t in tl)
				rd.stringToTypes[t.Name] = t;

			types = types.Where(t => t.IsSealed && t.IsAbstract);

			foreach (var property in types
					 .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Static))
					 .Where(p => p.GetCustomAttribute<PublicForTestOnly>() == null))
				rd.flatPublicStaticProperties.TryAdd(property.Name, property);

			foreach (var method in types
					 .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
					 .Where(m => !m.IsSpecialName && m.GetCustomAttribute<PublicForTestOnly>() == null))
				rd.flatPublicStaticMethods.TryAdd(method.Name, method);

#if DEBUG
			//var typelist = tl.ToList();
			//var mlist = rd.flatPublicStaticMethods.Keys.ToList();
			//mlist.Sort();
			//var plist = rd.flatPublicStaticProperties.Keys.ToList();
			//plist.Sort();
			//System.IO.File.WriteAllText("methpropskeysharp.txt", string.Join("\n", typelist.Select(t => t.FullName))
			//                          + "\n"
			//                          + string.Join("\n", mlist.Select(m => $"{rd.flatPublicStaticMethods[m].DeclaringType}.{m}()").OrderBy(s => s))
			//                          + "\n"
			//                          + string.Join("\n", plist.Select(p => $"{rd.flatPublicStaticProperties[p].DeclaringType}.{p}").OrderBy(s => s)));
#endif
		}

		internal static FieldInfo FindAndCacheField(Type t, string name, BindingFlags propType =
					BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
		{
			try
			{
				do
				{
					var rd = Script.TheScript.ReflectionsData;

					if (rd.staticFields.TryGetValue(t, out var dkt))
					{
					}
					else//Field on this type has not been used yet, so get all properties and cache.
					{
						lock (rd.locker)
						{
							var fields = t.GetFields(propType);

							if (fields.Length > 0)
							{
								foreach (var field in fields)
									rd.staticFields.GetOrAdd(field.ReflectedType,
															 () => new Dictionary<string, FieldInfo>(fields.Length, StringComparer.OrdinalIgnoreCase))
									[field.Name] = field;
							}
							else//Make a dummy entry because this type has no fields. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
							{
								rd.staticFields[t] = dkt = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
								t = t.BaseType;
								continue;
							}
						}
					}

					if (dkt == null && !rd.staticFields.TryGetValue(t, out dkt))
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

		internal static MethodPropertyHolder FindAndCacheInstanceMethod(Type t, string name, int paramCount, BindingFlags propType =//probably dont even want to allow this to be passed.
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, bool isSystem = false) =>
		FindAndCacheMethod(Script.TheScript.ReflectionsData.typeToStringMethods, t, name, paramCount, propType, isSystem);

		internal static MethodPropertyHolder FindAndCacheStaticMethod(Type t, string name, int paramCount, BindingFlags propType =
					BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, bool isSystem = false) =>
		FindAndCacheMethod(Script.TheScript.ReflectionsData.typeToStringStaticMethods, t, name, paramCount, propType, isSystem);

		internal static MethodPropertyHolder FindAndCacheMethod(Type t, string name, int paramCount)
		{
			var mph = FindAndCacheInstanceMethod(t, name, paramCount);

			if (mph != null)
				return mph;

			return FindAndCacheStaticMethod(t, name, paramCount);
		}

		private static MethodPropertyHolder FindAndCacheMethod(ttsd typeToMethods, Type t, string name, int paramCount, BindingFlags propType, bool isSystem = false)
		{
			do
			{
				if (typeToMethods.TryGetValue(t, out var dkt))
				{
				}
				else
				{
					lock (Script.TheScript.ReflectionsData.locker)
					{
						var meths = t.GetMethods(propType);
#if CONCURRENT

						if (meths.Length > 0)
						{
							foreach (var meth in meths)
								typeToMethods.GetOrAdd(meth.ReflectedType,
													   (tp) => new ConcurrentDictionary<string, ConcurrentDictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase))
								.GetOrAdd(meth.Name)[meth.GetParameters().Length] = MethodPropertyHolder.GetOrAdd(meth);
						}
						else//Make a dummy entry because this type has no methods. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
						{
							typeToMethods[t] = dkt = new ConcurrentDictionary<string, ConcurrentDictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase);
							t = t.BaseType;
							continue;
						}

#else

						if (meths.Length > 0)
						{
							foreach (var meth in meths)
							{
								var mph = MethodPropertyHolder.GetOrAdd(meth);
								typeToMethods.GetOrAdd(meth.ReflectedType,
													   () => new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(meths.Length, StringComparer.OrdinalIgnoreCase))
								.GetOrAdd(meth.Name)[mph.ParamLength] = mph;
							}
						}
						else//Make a dummy entry because this type has no methods. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
						{
							typeToMethods[t] = dkt = new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase);
							t = t.BaseType;
							continue;
						}

#endif
					}
				}

				if (dkt == null && !typeToMethods.TryGetValue(t, out dkt))
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
					var rd = Script.TheScript.ReflectionsData;

					if (rd.typeToStringProperties.TryGetValue(t, out var dkt))
					{
					}
					else//Property on this type has not been used yet, so get all properties and cache.
					{
						lock (rd.locker)
						{
							var props = t.GetProperties(propType);
#if CONCURRENT

							if (props.Length > 0)
							{
								foreach (var prop in props)
									typeToStringProperties.GetOrAdd(prop.ReflectedType,
																	(tp) => new ConcurrentDictionary<string, ConcurrentDictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase))
									.GetOrAdd(prop.Name)[prop.GetIndexParameters().Length] = MethodPropertyHolder.GetOrAdd(prop);
							}
							else//Make a dummy entry because this type has no properties. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
							{
								typeToStringProperties[t] = dkt = new ConcurrentDictionary<string, ConcurrentDictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase);
								t = t.BaseType;
								continue;
							}

#else

							if (props.Length > 0)
							{
								foreach (var prop in props)
								{
									var mph = MethodPropertyHolder.GetOrAdd(prop);
									rd.typeToStringProperties.GetOrAdd(prop.ReflectedType,
																	() => new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(props.Length, StringComparer.OrdinalIgnoreCase))
									.GetOrAdd(prop.Name)[mph.ParamLength] = mph;
								}
							}
							else//Make a dummy entry because this type has no properties. This saves us additional searching later on when we encounter a type derived from this one. It will make the first Dictionary lookup above return true.
							{
								rd.typeToStringProperties[t] = dkt = new Dictionary<string, Dictionary<int, MethodPropertyHolder>>(StringComparer.OrdinalIgnoreCase);
								t = t.BaseType;
								continue;
							}

#endif
						}
					}

					if (dkt == null && !rd.typeToStringProperties.TryGetValue(t, out dkt))
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
		FindMethod(Script.TheScript.ReflectionsData.stringToTypeBuiltInMethods, name, paramCount);

		internal static MethodPropertyHolder FindLocalMethod(string name, int paramCount) =>
		FindMethod(Script.TheScript.ReflectionsData.stringToTypeLocalMethods, name, paramCount);

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

					if (Script.TheScript.ReflectionsData.typeToStringProperties.TryGetValue(t, out var dkt))
					{
						if (name != "__Class" && name != "__Static")
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

					if (Script.TheScript.ReflectionsData.typeToStringProperties.TryGetValue(t, out var dkt))
					{
						foreach (var kv in dkt)
							if (kv.Value.Count > 0 && kv.Key != "__Class" && kv.Key != "__Static")
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

					if (Script.TheScript.ReflectionsData.typeToStringProperties.TryGetValue(t, out var dkt))
					{
						ct += dkt.Count;

                        if (dkt.ContainsKey("__Static"))
							--ct;

						if (dkt.ContainsKey("__Class"))
							--ct;
					}

					t = t.BaseType;
				}
			}
			catch (Exception)
			{
				throw;
			}

			return ct;
		}

		internal static long GetPtrProperty(object item, bool throwIfZero = false)
		{
			long addr = 0L;

			if (item is long l)
				addr = l;
			else if (item is IPointable buf)//Put Buffer, StringBuffer etc check first because it's faster and more likely.
				addr = buf.Ptr;
			else if (item is KeysharpObject kso && Script.TryGetPropertyValue(kso, "ptr", out object p))
				addr = p.Al();
			else
				addr = item.Al();

			if (throwIfZero && addr == 0L)
				return (long)Errors.TypeErrorOccurred(item, typeof(long), DefaultErrorLong);

			return addr;
		}

		//internal static T SafeGetProperty<T>(object item, string name) => (T)item.GetType().GetProperty(name, typeof(T))?.GetValue(item);

		//internal static bool SafeHasProperty(object item, string name) => item.GetType().GetProperties().Where(prop => prop.Name == name).Any();

		internal static void SafeSetProperty(object item, string name, object value) => item.GetType().GetProperty(name, value.GetType())?.SetValue(item, value, null);

		private static void CacheAllMethods(bool ignoreMainAssembly = false)
		{
			List<Assembly> assemblies;
			var rd = Script.TheScript.ReflectionsData;
			var loadedAssembliesList = rd.loadedAssemblies.Values;
			rd.stringToTypeLocalMethods.Clear();
			rd.stringToTypeBuiltInMethods.Clear();

			if (AppDomain.CurrentDomain.FriendlyName == "testhost")//When running unit tests, the assembly names are changed for the auto generated program.
				assemblies = loadedAssembliesList.ToList();
			else if (Assembly.GetEntryAssembly().FullName.StartsWith("Keysharp,", StringComparison.OrdinalIgnoreCase))//Running from Keysharp.exe which compiled this script and launched it as a dynamically loaded assembly.
				assemblies = loadedAssembliesList.Where(assy => assy.GetCustomAttribute<PublicForTestOnly>() == null && (assy.Location.Length == 0 || assy.FullName.StartsWith("Keysharp.", StringComparison.OrdinalIgnoreCase))).ToList();//The . is important, it means only inspect Keysharp.Core because Keysharp, is the main Keysharp program, which we don't want to inspect. An assembly with an empty location is the compiled exe.
			else//Running as a standalone executable.
				assemblies = loadedAssembliesList.Where(assy => assy.GetCustomAttribute<PublicForTestOnly>() == null && (assy.FullName.StartsWith("Keysharp.", StringComparison.OrdinalIgnoreCase) ||
														(assy.EntryPoint != null &&
																assy.EntryPoint.DeclaringType != null &&
																assy.EntryPoint.DeclaringType.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase)
														))).ToList();

			//_ = MessageBox.Show(string.Join('\n', assemblies.Select(assy => assy.FullName)));
			foreach (var asm in assemblies)
				foreach (var type in asm.GetExportedTypes())
					if (type.GetCustomAttribute<PublicForTestOnly>() == null &&
						type.IsClass && type.IsPublic && type.Namespace != null && (!ignoreMainAssembly || type.Name != Keywords.MainClassName) &&
							(type.Namespace.StartsWith("Keysharp.Core", StringComparison.OrdinalIgnoreCase) ||
							 type.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase) ||
							 type.Namespace.StartsWith("Keysharp.Tests", StringComparison.OrdinalIgnoreCase)))//Allow tests so we can use function objects inside of unit tests.
					{
						_ = FindAndCacheInstanceMethod(type, "", -1);
						_ = FindAndCacheStaticMethod(type, "", -1);
					}

			_ = FindAndCacheInstanceMethod(typeof(object[]), "", -1, BindingFlags.Public | BindingFlags.Instance);//Needs to be done manually because many of the properties are declared in a base class.
			_ = FindAndCacheStaticMethod(typeof(object[]), "", -1, BindingFlags.Public | BindingFlags.Static);

			foreach (var typekv in rd.typeToStringMethods)
			{
				foreach (var methkv in typekv.Value)
					_ = rd.stringToTypeMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);
			}

			foreach (var typekv in rd.typeToStringStaticMethods)
			{
				foreach (var methkv in typekv.Value)
				{
					_ = rd.stringToTypeStaticMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);

					if (typekv.Key.FullName.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase) || typekv.Key.FullName.StartsWith("Keysharp.Tests", StringComparison.OrdinalIgnoreCase))//Need to include Tests so that unit tests will work.
						_ = rd.stringToTypeLocalMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);
					else
						_ = rd.stringToTypeBuiltInMethods.GetOrAdd(methkv.Key).GetOrAdd(typekv.Key, methkv.Value);
				}
			}
		}

		private static void CacheAllPropertiesAndFields()
		{
			var rd = Script.TheScript.ReflectionsData;
			rd.typeToStringProperties.Clear();
			rd.stringToTypeProperties.Clear();
			var exeAssembly = Accessors.GetAssembly();

			//The compiled and running output of a script will have the name of the script file without the extension.
			//So we can't just use "Keysharp" to identify it.
			foreach (var asm in rd.loadedAssemblies.Values.Where(assy => assy.FullName.StartsWith("Keysharp", StringComparison.OrdinalIgnoreCase) || exeAssembly == assy))
				foreach (var type in asm.GetExportedTypes())
					if (type.IsClass && type.IsPublic && type.Namespace != null && type.GetCustomAttribute<PublicForTestOnly>() == null &&
							(type.Namespace.StartsWith("Keysharp.Core", StringComparison.OrdinalIgnoreCase) ||
							 type.Namespace.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase)))
					{
						_ = FindAndCacheProperty(type, "", 0);
						_ = FindAndCacheField(type, "");
					}

			var propType = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
			_ = FindAndCacheProperty(typeof(object[]), "", 0, propType); //Needs to be done manually because many of the properties are declared in a base class.
			_ = FindAndCacheProperty(typeof(Exception), "", 0, propType); //Same.

			foreach (var typekv in rd.typeToStringProperties)
				foreach (var propkv in typekv.Value)
					_ = rd.stringToTypeProperties.GetOrAdd(propkv.Key).GetOrAdd(typekv.Key, propkv.Value);
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
					_ = KeysharpEnhancements.OutputDebugLine(ex.Message);
				}
			}

			return dkt;
		}

		private static IEnumerable<Type> GetNestedTypes(Type[] types)
		{
			foreach (var t in types)
			{
				yield return t;
				_ = GetNestedTypes(t.GetNestedTypes());
			}
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