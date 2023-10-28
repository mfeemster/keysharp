using Keysharp.Core;
using Keysharp.Core.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public class Variables
		{
			internal static List<(string, bool)> preloadedDlls = new List<(string, bool)>();
			internal static DateTime startTime = DateTime.Now;
			private static Dictionary<string, MemberInfo> globalVars = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);

			public bool AutoMark { get; set; }

			/// <summary>
			/// Will be a generated call within Main which calls into this class to add DLLs.
			/// </summary>
			/// <param name="p"></param>
			/// <param name="s"></param>
			public static void AddPreLoadedDll(string p, bool s) => preloadedDlls.Add((p, s));

			public static void InitGlobalVars()
			{
				Keysharp.Core.Window.SetProcessDPIAware();
				Keysharp.Core.Flow.Init();
#if WINDOWS
				Keysharp.Core.Processes.MainThreadID = Keysharp.Core.Windows.WindowsAPI.GetCurrentThreadId();
#else
				Keysharp.Core.Processes.MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;//Figure out how to do this on linux.//TODO
#endif
				Keysharp.Core.Processes.ManagedMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;//Figure out how to do this on linux.//TODO
				var stack = new StackTrace(false).GetFrames();

				for (var i = stack.Length - 1; i >= 0; i--)
				{
					var type = stack[i].GetMethod().DeclaringType;

					if (type.FullName.StartsWith("Keysharp.CompiledMain", StringComparison.OrdinalIgnoreCase))
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

				foreach (var dll in preloadedDlls)//Need to figure out a cross platform way to do DLL work.//TODO
				{
					if (dll.Item1.Length == 0)
					{
						if (!WindowsAPI.SetDllDirectory(null))//An empty #DllLoad restores the default search order.
							if (!dll.Item2)
								throw new Error("WindowsAPI.SetDllDirectory(null) failed.");
					}
					else if (Directory.Exists(dll.Item1))
					{
						if (!WindowsAPI.SetDllDirectory(dll.Item1))
							if (!dll.Item2)
								throw new Error($"WindowsAPI.SetDllDirectory({dll.Item1}) failed.");
					}
					else
					{
						var dllname = dll.Item1;

						if (!dllname.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
							dllname += ".dll";

						var hmodule = WindowsAPI.LoadLibrary(dllname);

						if (hmodule != IntPtr.Zero)
						{
							// "Pin" the dll so that the script cannot unload it with FreeLibrary.
							// This is done to avoid undefined behavior when DllCall optimizations
							// resolves a proc address in a dll loaded by this directive.
							_ = WindowsAPI.GetModuleHandleEx(WindowsAPI.GET_MODULE_HANDLE_EX_FLAG_PIN, dllname, out hmodule);  // MSDN regarding hmodule: "If the function fails, this parameter is NULL."
						}
						else if (!dll.Item2)
							throw new Error($"Failed to load DLL {dllname}.");
					}
				}

				Reflections.Initialize();//For some reason, the program will crash if these are delay initialized, so do them now.
				SetInitialFloatFormat();//This must be done intially and not just when A_FormatFloat is referenced for the first time.
				Application.AddMessageFilter(new Keysharp.Core.MessageFilter());
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