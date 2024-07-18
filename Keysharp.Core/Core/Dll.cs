#if WINDOWS
namespace Keysharp.Core
{
	public static class Dll
	{
		private static ConcurrentDictionary<string, DllCache> dllCache = new ConcurrentDictionary<string, DllCache>();

		public static DelegateHolder CallbackCreate(object obj0, object obj1 = null, object obj2 = null)
		{
			var options = obj1.As();
			//obj2/paramcount is unused.
			return new DelegateHolder(obj0, options.Contains("f", StringComparison.OrdinalIgnoreCase), options.Contains("&"));
		}

		public static void CallbackFree(object obj0)
		{
			if (obj0 is DelegateHolder dh)
				dh.Clear();
		}

		/// <summary>
		/// Calls an unmanaged function in a DLL.
		/// </summary>
		/// <param name="function">
		/// <para>The path to the function, e.g. <c>C:\path\to\my.dll</c>. The ".dll" file extension can be omitted.</para>
		/// <para>If an absolute path is not specified on Windows the function will search the following system libraries (in order):
		/// User32.dll, Kernel32.dll, ComCtl32.dll, or Gdi32.dll.</para>
		/// </param>
		/// <param name="parameters">The type and argument list.</param>
		/// <returns>The value returned by the function.</returns>
		/// <remarks>
		/// <para><see cref="ErrorLevel"/> will be set to one of the following:</para>
		/// <list type="bullet">
		/// <item><term>0</term>: <description>success</description></item>
		/// <item><term>-3</term>: <description>file could not be accessed</description></item>
		/// <item><term>-4</term>: <description>function could not be found</description></item>
		/// </list>
		/// <para>The following types can be used:</para>
		/// <list type="bullet">
		/// <item><term><c>str</c></term>: <description>a string</description></item>
		/// <item><term><c>int64</c></term>: <description>a 64-bit integer</description></item>
		/// <item><term><c>int</c></term>: <description>a 32-bit integer</description></item>
		/// <item><term><c>short</c></term>: <description>a 16-bit integer</description></item>
		/// <item><term><c>char</c></term>: <description>an 8-bit integer</description></item>
		/// <item><term><c>float</c></term>: <description>a 32-bit floating point number</description></item>
		/// <item><term><c>double</c></term>: <description>a 64-bit floating point number</description></item>
		/// <item><term><c>*</c> or <c>P</c> suffix</term>: <description>pass the specified type by address</description></item>
		/// <item><term><c>U</c> prefix</term>: <description>use unsigned values for numeric types</description></item>
		/// </list>
		/// </remarks>
		public static object DllCall(object function, params object[] parameters)
		{
			//You should some day add the ability to use this with .NET dlls, exposing some type of reflection to the script.//TODO
			if (function is string path)
			{
				string name;
				var helper = new DllArgumentHelper(parameters);
				var z = path.LastIndexOf(Path.DirectorySeparatorChar);

				if (z == -1)
				{
					name = path;
					path = string.Empty;

					if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					{
						foreach (var lib in new[] { "user32", "kernel32", "comctl32", "gdi32" })
						{
							var handle = WindowsAPI.GetModuleHandle(lib);

							if (handle == IntPtr.Zero)
								continue;

							var address = WindowsAPI.GetProcAddress(handle, name);

							if (address == IntPtr.Zero)
								address = WindowsAPI.GetProcAddress(handle, name + "W");

							if (address != IntPtr.Zero)
							{
								path = lib + ".dll";
								break;
							}
						}
					}

					if (path.Length == 0)
					{
						throw new Error($"Unable to locate dll with path {name}.");
					}
				}
				else
				{
					z++;

					if (z >= path.Length)
						throw new Error($"Improperly formatted path of {path}.");

					name = path.Substring(z);
					path = path.Substring(0, z - 1);
				}

				if (Environment.OSVersion.Platform == PlatformID.Win32NT && path.Length != 0 && !Path.HasExtension(path))
					path += ".dll";

				var id = GetDllCallId(name, path, helper);
				MethodInfo method;

				if (dllCache.TryGetValue(id, out var cached))
				{
					method = cached.method;
				}
				else
				{
					try
					{
						//Caching this would be ideal, but it doesn't seem possible because you can't modify the type after it's created.
						//Creating the assembly, module, type and method take about 4-7ms, so it's not too big of a deal.
						var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("pinvokes"), AssemblyBuilderAccess.RunAndCollect);
						var module = assembly.DefineDynamicModule("module");
						var container = module.DefineType("container", TypeAttributes.Public | TypeAttributes.UnicodeClass);
						var invoke = container.DefinePInvokeMethod(
										 name,
										 path,
										 MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl,
										 CallingConventions.Standard,
										 helper.ReturnType,
										 helper.types,
										 helper.CDecl ? CallingConvention.Cdecl : CallingConvention.Winapi,
										 CharSet.Auto);
						invoke.SetImplementationFlags(invoke.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);

						for (var i = 0; i < helper.args.Length; i++)
						{
							if (helper.args[i] is string s)
							{
								if (helper.names[i] == "astr")
								{
									var pb = invoke.DefineParameter(i + 1, ParameterAttributes.HasFieldMarshal, $"dynparam_{i}");
									pb.SetCustomAttribute(new CustomAttributeBuilder(
															  typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) }),
															  new object[] { System.Runtime.InteropServices.UnmanagedType.LPStr }));
								}
								else if (helper.names[i] == "bstr")
								{
									var pb = invoke.DefineParameter(i + 1, ParameterAttributes.HasFieldMarshal, $"dynparam_{i}");
									pb.SetCustomAttribute(new CustomAttributeBuilder(
															  typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) }),
															  new object[] { System.Runtime.InteropServices.UnmanagedType.BStr }));
								}
							}
							else if (helper.args[i] is System.Array array)
							{
								//var p = invoke.GetParameters();
								var pb = invoke.DefineParameter(i + 1, ParameterAttributes.HasFieldMarshal, $"dynparam_{i}");
								pb.SetCustomAttribute(new CustomAttributeBuilder(
														  typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) }),
														  new object[] { System.Runtime.InteropServices.UnmanagedType.SafeArray },
														  new FieldInfo[] { typeof(MarshalAsAttribute).GetField("SafeArraySubType") },
														  new object[] { System.Runtime.InteropServices.VarEnum.VT_VARIANT }
													  ));
							}
						}

						var created = container.CreateType();
						method = created.GetMethod(name);

						if (method == null)
							throw new Error($"Method {name} could not be found.");

						dllCache[id] = new DllCache()
						{
							assembly = assembly,
							module = module,
							container = container,
							invoke = invoke,
							created = created,
							method = method
						};
					}
					catch (Exception e)
					{
						var inner = e.InnerException != null ? " " + e.InnerException.Message : "";

						if (e.InnerException is Keysharp.Core.Error err)
							inner += " " + err.Message;

						var error = new Error($"An error occurred when calling {name}() in {path}: {e.Message}{inner}");
						error.Extra = "0x" + Accessors.A_LastError.ToString("X");
						throw error;
					}
				}

				try
				{
					var value = method.Invoke(null, helper.args);

					if (helper.ReturnName == "HRESULT" && value is int retval && retval < 0)
					{
						var ose = new OSError($"DllCall with return type of HRESULT returned {retval}.");
						ose.Extra = "0x" + ose.Number.ToString("X");
						throw ose;
					}

					if (helper.ReturnType == typeof(IntPtr))
					{
						if (helper.ReturnName == "astr")
							value = Marshal.PtrToStringAnsi((IntPtr)value);
						else if (helper.ReturnName == "str" || helper.ReturnName == "wstr")
							value = Marshal.PtrToStringUni((IntPtr)value);
					}

					//If they passed in a ComObject with Ptr as an address, make that address into a __ComObject.
					for (var pi = 0; pi < parameters.Length; pi++)
					{
						var p = parameters[pi];

						if (p is ComObject co)
						{
							object obj = co.Ptr;
							co.Ptr = obj;//Reassign to ensure pointers are properly cast to __ComObject.
						}
					}

					if (value is int i)
						return (long)i;
					else if (value is uint ui)
						return (long)ui;

					return value;
				}
				catch (Exception e)
				{
					var inner = e.InnerException != null ? " " + e.InnerException.Message : "";

					if (e.InnerException is Keysharp.Core.Error err)
						inner += " " + err.Message;

					var error = new Error($"An error occurred when calling {name}() in {path}: {e.Message}{inner}");
					error.Extra = "0x" + Accessors.A_LastError.ToString("X");
					throw error;
				}
			}
			else if (function is DelegateHolder dh)
			{
				//var ptrs = new IntPtr[31];
				//var helper = new DllArgumentHelper(parameters);
				var helper = new ComArgumentHelper(parameters);
				//unsafe
				//{
				//  //fixed (object* pin = args)
				//  {
				//      for (var i = 0; i < helper.args.Length && i < ptrs.Length; i++)
				//      {
				//          if (helper.types[i] == typeof(float))
				//          {
				//              var f = (float)helper.args[i];
				//              int* iref = (int*)&f;
				//              ptrs[i] = new IntPtr(*iref);
				//          }
				//          else if (helper.types[i] == typeof(double))
				//          {
				//              var d = (double)helper.args[i];
				//              long* lref = (long*)&d;
				//              ptrs[i] = new IntPtr(*lref);
				//          }
				//          else if (helper.types[i] == typeof(long))
				//          {
				//              var l = (long)helper.args[i];
				//              ptrs[i] = new IntPtr(l);
				//          }
				//          else if (helper.types[i] == typeof(IntPtr))
				//          {
				//              ptrs[i] = (IntPtr)helper.args[i];
				//          }
				//          else if (helper.types[i] == typeof(string))
				//          {
				//              var str = helper.args[i] as string;
				//              fixed (char* p = str)//If the string moves after this is assigned, the program will likely crash. Unsure what else to do.//TODO
				//              {
				//                  ptrs[i] = new IntPtr(p);
				//              }
				//          }
				//          else if (helper.types[i] == typeof(ulong))
				//          {
				//              var ul = (ulong)helper.args[i];
				//              ptrs[i] = new IntPtr((long)ul);
				//          }
				//          else if (helper.types[i] == typeof(int))
				//          {
				//              var ii = (int)helper.args[i];
				//              ptrs[i] = new IntPtr((long)ii);
				//          }
				//          else if (helper.types[i] == typeof(uint))
				//          {
				//              var ui = (uint)helper.args[i];
				//              ptrs[i] = new IntPtr(ui);
				//          }
				//          else if (helper.types[i] == typeof(short))
				//          {
				//              var s = (short)helper.args[i];
				//              ptrs[i] = new IntPtr(s);
				//          }
				//          else if (helper.types[i] == typeof(ushort))
				//          {
				//              var us = (ushort)helper.args[i];
				//              ptrs[i] = new IntPtr(us);
				//          }
				//          else if (helper.types[i] == typeof(char))
				//          {
				//              var c = (char)helper.args[i];
				//              ptrs[i] = new IntPtr(c);
				//          }
				//          else if (helper.types[i] == typeof(sbyte))
				//          {
				//              var sb = (sbyte)helper.args[i];
				//              ptrs[i] = new IntPtr(sb);
				//          }
				//          else if (helper.types[i] == typeof(byte))
				//          {
				//              var b = (byte)helper.args[i];
				//              ptrs[i] = new IntPtr(b);
				//          }
				//      }
				//  }
				//}
				return dh.DelegatePlaceholderArr(helper.args);
				//return dh.DelegatePlaceholderArr(ptrs);
			}
			else if (function is Delegate del)
			{
				var helper = new DllArgumentHelper(parameters);
				return del.DynamicInvoke(helper.args);
			}
			else
			{
				var address = IntPtr.Zero;

				if (function is IntPtr ip)
				{
					address = ip;
				}
				else if (function is int || function is long)
				{
					address = new IntPtr(function.Al());
				}
				else
				{
					var val = Keysharp.Core.Reflections.SafeGetProperty<IntPtr>(function, "Ptr");
					address = val == IntPtr.Zero
							  ? throw new TypeError($"Function argument was of type {function.GetType()}. It must be string, StringBuffer, int, long or an object with a Ptr member.")
							  : val;
				}

				//if (address > 0)//Nothing in this block works and the code below is the remnants of various attempts.
				{
					try
					{
						//var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("KeysharpDynamicMethods"), AssemblyBuilderAccess.RunAndCollect);
						//var module = assembly.DefineDynamicModule("KeysharpDynamicModule");
						//var container = module.DefineType("KeysharpDynamicContainer", TypeAttributes.Public | TypeAttributes.UnicodeClass);
						//var typeBuilder = module.DefineType("KeysharpDynamicType", TypeAttributes.Public);
						//var methodBuilder = typeBuilder.DefineMethod(
						//                      "mymethodname",
						//                      MethodAttributes.Public | MethodAttributes.Static,
						//                      helper.returnType,
						//                      helper.types);
						//var tp = typeBuilder.CreateType();
						//var definition = methodBuilder.GetGenericMethodDefinition();
						//var methodInfo = tp.GetMethod("mymethodname");
						//MsgBox("Your Dynamic Method: {0};", methodInfo.ToString());
						//var value = Marshal.GetDelegateForFunctionPointer(new IntPtr(address), typeof(Delegate)).Method.Invoke(null, helper.args);
						//var ptrdel = Marshal.GetDelegateForFunctionPointer(new IntPtr(address), typeof(Delegate));
						//var delType = Expression.GetFuncType(helper.types.Concat(new[] { helper.returnType}));
						var comHelper = new ComArgumentHelper(parameters);
						var val = CallDel(address, comHelper.args);
						//var delType = Expression.GetDelegateType(helper.types.Concat(new[] { helper.ReturnType }));
						//var ptrdel = GetDelegateForFunctionPointerFix(new IntPtr(address), delType);
						//System.Runtime.CompilerServices.
						//System.Linq.Expressions.Compiler.DelegateHelpers.MakeNewCustomDelegate
						//var ptrdel = Marshal.GetDelegateForFunctionPointer(new IntPtr(address), typeof(Action));
						//var value = ptrdel.DynamicInvoke(helper.args.Length == 0 ? null : helper.args);
						//var value = ptrdel.Method.Invoke(null, args);
						return val;
					}
					catch (Exception e)
					{
						var error = new Error($"An error occurred when calling {function}(): {e.Message}");
						error.Extra = "0x" + Accessors.A_LastError.ToString("X");
						throw error;
					}
				}
				//else if (address <= 0)
				//{
				//  throw new ValueError($"Function argument of type {function.GetType()} was treated as an address and had a value of {address}. It must greater than 0.");
				//}
				//else// if (function is float || function is double || function is decimal)
				//{
				//  throw new TypeError($"Function argument was of type {function.GetType()}. It must be string, StringBuffer, int, long or an object with a Ptr member.");
				//}
			}
		}

		internal static string GetDllCallId(string name, string path, DllArgumentHelper helper)
		{
			return $"{name}{path}{helper.ReturnType}{string.Join(',', helper.names)}-{string.Join(',', helper.types.Select(t => t.Name))}";
		}

		private static IntPtr CallDel(IntPtr vtbl, IntPtr[] args)
		{
			switch (args.Length)
			{
				case 0:
					var del0 = (DelNone)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(DelNone));
					return del0();

				case 1:
					var del1 = (Del0)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del0));
					return del1(args[0]);

				case 2:
					var del2 = (Del1)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del1));
					return del2(args[0], args[1]);

				case 3:
					var del3 = (Del2)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del2));
					return del3(args[0], args[1], args[2]);

				case 4:
					var del4 = (Del3)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del3));
					return del4(args[0], args[1], args[2], args[3]);

				case 5:
					var del5 = (Del4)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del4));
					return del5(args[0], args[1], args[2], args[3], args[4]);

				case 6:
					var del6 = (Del5)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del5));
					return del6(args[0], args[1], args[2], args[3], args[4], args[5]);

				case 7:
					var del7 = (Del6)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del6));
					return del7(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

				case 8:
					var del8 = (Del7)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del7));
					return del8(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

				case 9:
					var del9 = (Del8)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del8));
					return del9(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);

				case 10:
					var del10 = (Del9)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del9));
					return del10(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);

				case 11:
					var del11 = (Del10)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del10));
					return del11(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);

				case 12:
					var del12 = (Del11)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del11));
					return del12(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);

				case 13:
					var del13 = (Del12)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del12));
					return del13(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);

				case 14:
					var del14 = (Del13)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del13));
					return del14(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);

				case 15:
					var del15 = (Del14)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del14));
					return del15(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);

				case 16:
					var del16 = (Del15)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del15));
					return del16(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);
			}

			return IntPtr.Zero;
		}
	}

	internal class DllCache
	{
		internal AssemblyBuilder assembly;
		internal TypeBuilder container;
		internal Type created;
		internal MethodBuilder invoke;
		internal MethodInfo method;
		internal ModuleBuilder module;
	}
}
#endif