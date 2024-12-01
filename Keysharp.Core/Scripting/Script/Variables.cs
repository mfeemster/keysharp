namespace Keysharp.Scripting
{
	public partial class Script
	{
		public class Variables
		{
			internal static List<(string, bool)> preloadedDlls = [];
			internal static DateTime startTime = DateTime.Now;
			private static readonly Dictionary<string, MemberInfo> globalVars = new (StringComparer.OrdinalIgnoreCase);
#if LINUX
			internal static string ldLibraryPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? "";
			private static Encoding enc1252 = Encoding.Default;
#endif
			public bool AutoMark { get; set; }

			/// <summary>
			/// Will be a generated call within Main which calls into this class to add DLLs.
			/// </summary>
			/// <param name="p"></param>
			/// <param name="s"></param>
			public static void AddPreLoadedDll(string p, bool s) => preloadedDlls.Add((p, s));

			public static void InitGlobalVars()
			{
				Window.SetProcessDPIAware();
				Flow.Init();
				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
				CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
#if LINUX
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);//For some reason, linux needs this for rich text to work.
				enc1252 = Encoding.GetEncoding(1252);
#endif
				Processes.MainThreadID = mgr.CurrentThreadId();
				Processes.ManagedMainThreadID = Thread.CurrentThread.ManagedThreadId;//Figure out how to do this on linux.//TODO
				_ = Threads.PushThreadVariables(0, true, false, true);//Ensure there is always one thread in existence for reference purposes, but do not increment the actual thread counter.
				var stack = new StackTrace(false).GetFrames();
				//If we're running via passing in a script and are not in a unit test, then set the working directory to that of the script file.
				var path = Path.GetFileName(Application.ExecutablePath).ToLowerInvariant();

				if (path != "testhost.exe" && path != "testhost.dll" && !Accessors.A_IsCompiled)
					Dir.SetWorkingDir(Accessors.A_ScriptDir);

				for (var i = stack.Length - 1; i >= 0; i--)
				{
					var type = stack[i].GetMethod().DeclaringType;

					if (type != null && type.FullName.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase))
					{
						var fields = type.GetFields(BindingFlags.Static |
													BindingFlags.NonPublic |
													BindingFlags.Public);
						var props = type.GetProperties(BindingFlags.Static |
													   BindingFlags.NonPublic |
													   BindingFlags.Public);
						_ = globalVars.EnsureCapacity(fields.Length + props.Length);

						foreach (var field in fields)
							globalVars[field.Name] = field;

						foreach (var prop in props)
							globalVars[prop.Name] = prop;

						break;
					}
				}

				foreach (var dll in preloadedDlls)
				{
					if (dll.Item1.Length == 0)
					{
						if (!mgr.SetDllDirectory(null))//An empty #DllLoad restores the default search order.
							if (!dll.Item2)
								throw new Error("PlatformProvider.Manager.SetDllDirectory(null) failed.");
					}
					else if (Directory.Exists(dll.Item1))
					{
						if (!mgr.SetDllDirectory(dll.Item1))
							if (!dll.Item2)
								throw new Error($"PlatformProvider.Manager.SetDllDirectory({dll.Item1}) failed.");
					}
					else
					{
						var dllname = dll.Item1;
#if WINDOWS

						if (!dllname.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
							dllname += ".dll";

#endif
						var hmodule = mgr.LoadLibrary(dllname);

						if (hmodule != IntPtr.Zero)
						{
#if WINDOWS
							// "Pin" the dll so that the script cannot unload it with FreeLibrary.
							// This is done to avoid undefined behavior when DllCall optimizations
							// resolves a proc address in a dll loaded by this directive.
							_ = WindowsAPI.GetModuleHandleEx(WindowsAPI.GET_MODULE_HANDLE_EX_FLAG_PIN, dllname, out hmodule);  // MSDN regarding hmodule: "If the function fails, this parameter is NULL."
#endif
						}
						else if (!dll.Item2)
							throw new Error($"Failed to load DLL {dllname}.");
					}
				}

				Reflections.Initialize();//For some reason, the program will crash if these are delay initialized, so do them now.
				SetInitialFloatFormat();//This must be done intially and not just when A_FormatFloat is referenced for the first time.
				Application.AddMessageFilter(new MessageFilter());
			}

			public object GetVariable(string key)
			{
				if (globalVars.TryGetValue(key, out var field))
				{
					if (field is PropertyInfo pi)
						return pi.GetValue(null);
					else if (field is FieldInfo fi)
						return fi.GetValue(null);
				}

				return GetReservedVariable(key);//Last, try reserved variable.
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

			private static PropertyInfo FindReservedVariable(string name)
			{
				_ = Reflections.flatPublicStaticProperties.TryGetValue(name, out var prop);
				return prop;
			}

			private static object GetReservedVariable(string name)
			{
				var prop = FindReservedVariable(name);
				return prop == null || !prop.CanRead ? null : prop.GetValue(null);
			}

			private static bool SetReservedVariable(string name, object value)
			{
				var prop = FindReservedVariable(name);
				var set = prop != null && prop.CanWrite;

				if (set)
				{
					value = ForceType(prop.PropertyType, value);
					prop.SetValue(null, value);
				}

				return set;
			}

			public object this[object key]
			{
				get => GetVariable(key.ToString()) ?? "";
				set => _ = SetVariable(key.ToString(), value);
			}
		}
	}
}