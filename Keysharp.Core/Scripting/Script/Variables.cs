using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using Keysharp.Core;
using Keysharp.Core.Windows;

namespace Keysharp.Scripting
{
	public partial class Script
	{
		public class Variables
		{
			private Stack<string> collect = new Stack<string>();
			private Dictionary<string, object> table = new Dictionary<string, object>();
			private static Dictionary<string, FieldInfo> globalVars = new Dictionary<string, FieldInfo>();
			internal static DateTime startTime = DateTime.Now;

			public bool AutoMark { get; set; }

			public static void InitGlobalVars()
			{
#if WINDOWS
				Keysharp.Core.Processes.MainThreadID = Keysharp.Core.Windows.WindowsAPI.GetCurrentThreadId();
#else
				Keysharp.Core.Processes.MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;//Figure out how to do this on linux.//TODO
#endif
				var stack = new StackTrace(false).GetFrames();

				for (var i = stack.Length - 1; i >= 0; i--)
				{
					var type = stack[i].GetMethod().DeclaringType;

					//if (type == typeof(Core))
					//continue;

					// UNDONE: better way to check correct type for reflecting local methods
					//if (type.FullName != "Program")
					//continue;

					if (type.FullName.StartsWith("Keysharp.Main", StringComparison.OrdinalIgnoreCase))
					{
						var fields = type.GetFields(BindingFlags.Static |
													BindingFlags.NonPublic |
													BindingFlags.Public);
						_ = globalVars.EnsureCapacity(fields.Length);

						foreach (var field in fields)
							globalVars[field.Name] = field;

						//globalVars.Add(field.Name, field);
						//if (field_info == null)
						//{
						//}
						//else if (field_info.FieldType.IsArray)
						//{
						//  // Join the array values into a string.
						//  //string[] values = (string[])field_info.GetValue(this);
						//  //lblValue.Text = string.Join(",", values);
						//}
						//else
						//{
						//  // Just convert it into a string.
						//  Console.WriteLine(field_info.GetValue(null).ToString());
						//}
						//var list = type.GetEnumValues;
						//
						//for (var z = 0; z < list.Length; z++)
						//  if (list[z].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
						//      return list[z];
						break;
					}
				}

				foreach (var dll in Keysharp.Scripting.Parser.preloadedDlls)//Need to figure out a cross platform way to do DLL work.//TODO
				{
					if (dll.Item1.Length == 0)
					{
						if (!WindowsAPI.SetDllDirectory(null))// an empty #DllLoad restores the default search order.
							throw new Error("An internal function call failed.");
					}
					else if (Directory.Exists(dll.Item1))
					{
						if (!WindowsAPI.SetDllDirectory(dll.Item1))
							throw new Error("An internal function call failed.");
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
							throw new Error("Failed to load DLL.", dllname);
					}
				}

				Reflections.Initialize();//For some reason, the program will crash if these are delay initialized, so do them now.
				Accessors.SetInitialFloatFormat();//This must be done intially and not just when A_FormatFloat is referenced for the first time.
			}

			public object GetVariable(string key)
			{
				lock (table)
				{
					//if (table.TryGetValue(key, out var val))//First try local scope.
					//  return val;
					//
					//var z = key.LastIndexOf('.');//Then try global scope.
					//
					//if (z != -1)
					//{
					//  var temp = key.Substring(z);
					//
					//  if (table.TryGetValue(temp, out var globalval))
					//      return globalval;
					//}
					//if (globalVars.TryGetValue(key.StartsWith(Parser.ScopeVar) ? key : Parser.ScopeVar + key, out var field))
					if (globalVars.TryGetValue(key, out var field))
					{
						return field.GetValue(null);
						//if (val != null)
						//{
						//  var field2name = val.ToString();
						//
						//  //if (!field2name.StartsWith(Parser.ScopeVar))//Users will rever to the vars by their name without the underscore
						//  //field2name = Parser.ScopeVar + field2name;
						//
						//  if (globalVars.TryGetValue(field2name.StartsWith(Parser.ScopeVar) ? field2name : Parser.ScopeVar + field2name, out var field2))
						//      return field2.GetValue(null);
						//
						//  return GetReservedVariable(field2name);//Last, try reserved variable.
						//}
						//return val.GetValue(null);//Passing null, so all global vars must be marked static.
					}

					return GetReservedVariable(key);//Last, try reserved variable.
					//if (AutoMark && !collect.Contains(key))
					//  collect.Push(key);
					//return table[key];
				}

				//return null;
			}

			//public void Mark(params string[] keys)
			//{
			//  foreach (var key in keys)
			//      if (!collect.Contains(key))
			//          collect.Push(key);
			//}

			public object SetVariable(string key, object value)
			{
				//lock (table)
				//{
				//  if (value == null)
				//      _ = table.Remove(key);
				//  else
				//      table[key] = value;

				//  return value;
				//}
				//if (globalVars.TryGetValue(key.StartsWith(Parser.ScopeVar) ? key : Parser.ScopeVar + key, out var field))
				if (globalVars.TryGetValue(key, out var field))
				{
					field.SetValue(null, value);
					//return value;
					//var val = field.GetValue(null);
					//if (val != null)
					{
						//var field2name = val.ToString();
						//if (!field2name.StartsWith(Parser.ScopeVar))//Users will rever to the vars by their name without the underscore
						//field2name = Parser.ScopeVar + field2name;
						//if (globalVars.TryGetValue(field2name.StartsWith(Parser.ScopeVar) ? field2name : Parser.ScopeVar + field2name, out var field2))
						//field2.SetValue(null, value);
						//return field2.GetValue(null);
					}
					//return value;
					//return val.GetValue(null);//Passing null, so all global vars must be marked static.
				}
				else
					SetReservedVariable(key, value);

				return value;
			}

			//public void Sweep()
			//{
			//  while (collect.Count != 0)
			//      this[collect.Pop()] = null;
			//}

			private static PropertyInfo FindReservedVariable(string name)
			{
				//const string A_ = "A_";
				//var z = name.LastIndexOf('.');
				//
				//if (z != -1)
				//{
				//  z++;
				//
				//  if (z + A_.Length > name.Length)
				//      return null;
				//
				//  name = name.Substring(z);
				//}
				// UNDONE: This check fails on ErrorLevel
				//if(!name.Substring(0, A_.Length).Equals(A_, StringComparison.OrdinalIgnoreCase))
				//    return null;
				//PropertyInfo prop = null;
				//
				//foreach (var item in typeof(Script).BaseType.GetProperties())
				//  if (item.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
				//      prop = item;
				_ = Parser.libProperties.TryGetValue(name.ToLowerInvariant(), out var prop);
				return prop;
			}

			private static object GetReservedVariable(string name)
			{
				var prop = FindReservedVariable(name);
				return prop == null || !prop.CanRead ? null : prop.GetValue(null, null);
			}

			private static bool SetReservedVariable(string name, object value)
			{
				var prop = FindReservedVariable(name);
				var set = prop != null && prop.CanWrite;

				if (set)
				{
					value = ForceType(prop.PropertyType, value);
					prop.SetValue(null, value, null);
				}

				return set;
			}

			public object this[object key]
			{
				get { return key is string s ? GetVariable(s) : ""; }
				set { _ = SetVariable(key is string s ? s : "", value); }
			}
		}
	}
}