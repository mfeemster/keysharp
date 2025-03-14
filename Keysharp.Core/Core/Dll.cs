#if WINDOWS

namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for DLL-related functions.
	/// </summary>
	public static class Dll
	{
		/// <summary>
		/// Calling <see cref="DllCall"/> requires creating a dynamic assembly, module, type, and method.<br/>
		/// Then an instance the type is finally created.<br/>
		/// Doing all of these take significant time.<br/>
		/// Sadly, an existing assembly/module/type cannot have new methods created on it once the initial creation is done.<br/>
		/// An optimization is to keep a cache of these objects, keyed by the exact function name and argument types.<br/>
		/// Doing this saves significant time when doing repeated calls to the same DLL function with the same argument types.
		/// </summary>
		private static readonly ConcurrentDictionary<string, DllCache> dllCache = new ();

		/// <summary>
		/// Creates a <see cref="DelegateHolder"/> object that wraps a <see cref="FuncObj"/>.
		/// Passing string pointers to <see cref="DllCall"/> when passing a created callback is strongly recommended against.<br/>
		/// This is because the string pointer cannot remain pinned, and is likely to crash the program if the pointer gets moved by the GC.
		/// </summary>
		/// <param name="function">
		/// A function object to call automatically whenever the <see cref="DelegateHolder"/> is called, optionally passing arguments.<br/>
		/// A closure or bound function can be used to differentiate between multiple callbacks which all call the same script function.<br/>
		/// The callback retains a reference to the function object, and releases it when the script calls <see cref="CallbackFree"/>.
		/// </param>
		/// <param name="options">
		/// If blank or omitted, a new thread will be started each time function is called, the standard calling convention will be used, and the parameters will be passed individually to function.<br/>
		/// Otherwise, specify one or more of the following options. Separate each option from the next with a space (e.g. "C Fast").<br/>
		///     Fast or F: Avoids starting a new thread each time function is called.Although this performs better, it must be avoided whenever the thread from which Address is called varies (e.g.when the callback is triggered by an incoming message).<br/>
		///     This is because function will be able to change global settings such as <see cref="A_LastError"/> and the last-found window for whichever thread happens to be running at the time it is called.<br/>
		///     <![CDATA[&]]>: Causes the address of the parameter list (a single integer) to be passed to function instead of the individual parameters. Parameter values can be retrieved by using <see cref="External.NumGet"/>.<br/>
		/// </param>
		/// <param name="paramCount">
		/// If omitted, it defaults to 0, which is usually the number of mandatory parameters in the definition of function.<br/>
		/// Otherwise, specify the number of parameters that Address's caller will pass to it.<br/>
		/// In either case, ensure that the caller passes exactly this number of parameters.
		/// </param>
		/// <returns>A <see cref="DelegateHolder"/> object which internally holds a function pointer.<br/>
		/// This is typically passed to an external function via <see cref="DllCall"/> or placed in a struct using <see cref="NumPut"/>, but can also be called directly by <see cref="DllCall"/>.
		/// </returns>
		public static DelegateHolder CallbackCreate(object function, object options = null, object paramCount = null)
		{
			var o = options.As();
			return new DelegateHolder(Functions.GetFuncObj(function, null, true), o.Contains('f', StringComparison.OrdinalIgnoreCase), o.Contains('&'));//paramCount is unused.
		}

		/// <summary>
		/// Frees the specified callback by internally setting it to null.
		/// </summary>
		/// <param name="address">The <see cref="DelegateHolder"/> to be freed.</param>
		public static object CallbackFree(object address)
		{
			(address as DelegateHolder)?.Clear();
			return null;
		}

		/// <summary>
		/// Calls a function inside a DLL, such as a standard Windows API function.
		/// </summary>
		/// <param name="function">
		/// The DLL or EXE file name followed by a backslash and the name of the function.<br/>
		/// For example: "MyDLL\MyFunction" (the file extension ".dll" is the default when omitted).<br/>
		/// If an absolute path isn't specified, DllFile is assumed to be in the system's PATH or <see cref="A_WorkingDir"/>.
		/// DllFile may be omitted when calling a function that resides in User32.dll, Kernel32.dll, ComCtl32.dll, or Gdi32.dll.<br/>
		/// For example, "User32\IsWindowVisible" produces the same result as "IsWindowVisible".<br/>
		/// If no function can be found by the given name, a "W" (Unicode) suffix is automatically appended.<br/>
		/// For example, "MessageBox" is the same as "MessageBoxW".<br/>
		/// This parameter may also consist solely of an integer, which is interpreted as the address of the function to call. Sources of such addresses include COM and <see cref="CallbackCreate"/>.<br/>
		/// If this parameter is an object, the value of the object's Ptr property is used. If no such property exists, a <see cref="PropertyError"/> is thrown.
		/// As an alternative to passing a <see cref="Buffer"/> object with type Ptr to a function which will allocate and place string data into the buffer, pass <see cref="StringBuffer"/> object to hold the new string.
		///     This relieves the caller of having to call <see cref="StrGet"/> on the new string data.
		/// Also use Ptr and <see cref="StringBuffer"/> for double pointer parameters such as LPTSTR*.
		/// When using type Str for string data the function will modify, but not reallocate, the passed in string argument must be<br/>
		/// passed by <![CDATA[&]]> reference.<br/>
		///     This is also supported for strings passed as AStr.
		/// <see cref="StrGet"/> must be called to retrieve any memory allocated and returned inside of function.
		/// </param>
		/// <param name="parameters">Type1, Arg1<br/>
		/// Each of these pairs represents a single parameter to be passed to the function. The number of pairs is unlimited for normal DLL calls and is limited to 16 for COM calls.<br/>
		/// The argument types can be: Str, WStr, AStr, Int64, Int, Short, Char, Float, Double, Ptr or HRESULT (a 32-bit integer).<br/>
		/// Append an asterisk (with optional preceding space) to any of the above types to cause the address of the argument to be passed rather than the value itself.<br/>
		/// Prepend the letter U to any of the integer types above to interpret it as an unsigned integer (UInt64, UInt, UShort, and UChar).<br/>
		/// Strictly speaking, this is necessary only for return values and asterisk variables because it does not matter whether an argument passed by value is unsigned or signed (except for Int64).<br/>
		/// </param>
		/// <returns>The actual value returned by function.<br/>
		/// If function is of a type that does not return a value, the result is an undefined value of the specified return type (integer by default).</returns>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown if there is any problem creating the dynamic assembly/function or calling it.</exception>
		/// <exception cref="OSError">A <see cref="OSError"/> exception is thrown if the return type was HRESULT and the return value was negative.</exception>
		/// <exception cref="TypeError">A <see cref="TypeError"/> exception is thrown if any of the arguments was required to have a .Ptr member, but none was found.</exception>
		public static unsafe object DllCall(object function, params object[] parameters)
		{
			//You should some day add the ability to use this with .NET dlls, exposing some type of reflection to the script.//TODO
			Error err;

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
						return Errors.ErrorOccurred(err = new Error($"Unable to locate dll with path {name}.")) ? throw err : null;
				}
				else
				{
					z++;

					if (z >= path.Length)
						return Errors.ErrorOccurred(err = new Error($"Improperly formatted path of {path}.")) ? throw err : null;

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
						//The best we can do is the caching above in dllCache.
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
															  typeof(MarshalAsAttribute).GetConstructor([typeof(UnmanagedType)]),
															  [UnmanagedType.LPStr]));
								}
								else if (helper.names[i] == "bstr")
								{
									var pb = invoke.DefineParameter(i + 1, ParameterAttributes.HasFieldMarshal, $"dynparam_{i}");
									pb.SetCustomAttribute(new CustomAttributeBuilder(
															  typeof(MarshalAsAttribute).GetConstructor([typeof(UnmanagedType)]),
															  [UnmanagedType.BStr]));
								}
							}
							else if (helper.args[i] is System.Array array)
							{
								//var p = invoke.GetParameters();
								var pb = invoke.DefineParameter(i + 1, ParameterAttributes.HasFieldMarshal, $"dynparam_{i}");
								pb.SetCustomAttribute(new CustomAttributeBuilder(
														  typeof(MarshalAsAttribute).GetConstructor([typeof(UnmanagedType)]),
														  [UnmanagedType.SafeArray],
														  new FieldInfo[] { typeof(MarshalAsAttribute).GetField("SafeArraySubType") },
														  [VarEnum.VT_VARIANT]
													  ));
							}
						}

						var created = container.CreateType();
						method = created.GetMethod(name);

						if (method == null)
							return Errors.ErrorOccurred(err = new Error($"Method {name} could not be found.")) ? throw err : null;

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

						if (e.InnerException is Error ie)
							inner += " " + ie.Message;

						return Errors.ErrorOccurred(err = new Error($"An error occurred when calling {name}() in {path}: {e.Message}{inner}")
						{
							Extra = "0x" + Accessors.A_LastError.ToString("X")
						}) ? throw err : null;
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

					FixParamTypesAndCopyBack(parameters, helper.args);

                    foreach (var refIndex in helper.refs)
                        Script.SetPropertyValue(refIndex.Value, "__Value", parameters[refIndex.Key]);

                    if (value is int i)
						return (long)i;
					else if (value is uint ui)
						return (long)ui;

					return value;
				}
				catch (Exception e)
				{
					var inner = e.InnerException != null ? " " + e.InnerException.Message : "";

					if (e.InnerException is Error ie)
						inner += " " + ie.Message;

					return Errors.ErrorOccurred(err = new Error($"An error occurred when calling {name}() in {path}: {e.Message}{inner}")
					{
						Extra = "0x" + Accessors.A_LastError.ToString("X")
					}) ? throw err : null;
				}
			}
			else if (function is DelegateHolder dh)
			{
				return dh.DirectCall(parameters);
			}
			else if (function is Delegate del)
			{
				var helper = new DllArgumentHelper(parameters);
				var value = del.DynamicInvoke(helper.args);
				FixParamTypesAndCopyBack(parameters, helper.args);
                foreach (var refIndex in helper.refs)
                    Script.SetPropertyValue(refIndex.Value, "__Value", parameters[refIndex.Key]);
                return value;
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
					var val = Reflections.SafeGetProperty<IntPtr>(function, "Ptr");

					if (val == IntPtr.Zero)
						return Errors.ErrorOccurred(err = new TypeError($"Function argument was of type {function.GetType()}. It must be string, StringBuffer, int, long or an object with a Ptr member.")) ? throw err : null;

					address = val;
				}

				try
				{
					var comHelper = new ComArgumentHelper(parameters);
					var value = CallDel(address, comHelper.args);
					FixParamTypesAndCopyBack(parameters, [.. comHelper.args.Cast<nint>().Select(x => (object)x)]);
                    foreach (var refIndex in comHelper.refs)
                        Script.SetPropertyValue(refIndex.Value, "__Value", parameters[refIndex.Key]);
                    return value;
				}
				catch (Exception ex)
				{
					return Errors.ErrorOccurred(err = new Error($"An error occurred when calling {function}(): {ex.Message}")
					{
						Extra = "0x" + Accessors.A_LastError.ToString("X")
					}) ? throw err : null;
				}
			}
		}

		/// <summary>
		/// A private helper to wrap invoking a COM method with a specific number of arguments.<br/>
		/// This is done because there is no way to dynamically create and COM call in C# at runtime without knowing the COM ID ahead of time.<br/>
		/// Since it can only be done at compile time, we have to provide specific function signatures from 0 to 16 parameters,<br/>
		/// then call the appropriate one based on how many arguments are specificed when called.<br/>
		/// All arguments are considered <see cref="IntPtr"/> internally.
		/// </summary>
		/// <param name="vtbl">The vtbl of the COM object.</param>
		/// <param name="args">The argument list.</param>
		/// <returns>An <see cref="IntPtr"/> which contains the return value of the COM call.</returns>
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
					var del2 = (Del01)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del01));
					return del2(args[0], args[1]);

				case 3:
					var del3 = (Del02)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del02));
					return del3(args[0], args[1], args[2]);

				case 4:
					var del4 = (Del03)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del03));
					return del4(args[0], args[1], args[2], args[3]);

				case 5:
					var del5 = (Del04)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del04));
					return del5(args[0], args[1], args[2], args[3], args[4]);

				case 6:
					var del6 = (Del05)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del05));
					return del6(args[0], args[1], args[2], args[3], args[4], args[5]);

				case 7:
					var del7 = (Del06)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del06));
					return del7(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

				case 8:
					var del8 = (Del07)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del07));
					return del8(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

				case 9:
					var del9 = (Del08)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del08));
					return del9(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);

				case 10:
					var del10 = (Del09)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del09));
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

		internal static unsafe void FixParamTypeAndCopyBack(ref object p, string ps, IntPtr aip)
		{
			if (ps.EndsWith("uint*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("uintp", StringComparison.OrdinalIgnoreCase))
			{
				var tempui = *(uint*)aip.ToPointer();
				var templ = (long)tempui;
				p = templ;
			}
			else if (ps.EndsWith("int*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("intp", StringComparison.OrdinalIgnoreCase))
			{
				var tempi = *(int*)aip.ToPointer();
				var templ = (long)tempi;
				p = templ;
			}
			else if (ps.EndsWith("int64*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("int64p", StringComparison.OrdinalIgnoreCase))
			{
				var templ = *(long*)aip.ToPointer();
				p = templ;
			}
			else if (ps.EndsWith("double*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("doublep", StringComparison.OrdinalIgnoreCase))
			{
				var tempd = *(double*)aip.ToPointer();
				p = tempd;
			}
			else if (ps.EndsWith("float*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("floatp", StringComparison.OrdinalIgnoreCase))
			{
				var tempf = *(float*)aip.ToPointer();
				var tempd = (double)tempf;
				p = tempd;
			}
			else if (ps.EndsWith("ushort*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("ushortp", StringComparison.OrdinalIgnoreCase))
			{
				var tempus = *(ushort*)aip.ToPointer();
				var templ = (long)tempus;
				p = templ;
			}
			else if (ps.EndsWith("short*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("shortp", StringComparison.OrdinalIgnoreCase))
			{
				var temps = *(short*)aip.ToPointer();
				var templ = (long)temps;
				p = templ;
			}
			else if (ps.EndsWith("uchar*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("ucharp", StringComparison.OrdinalIgnoreCase))
			{
				var tempub = *(byte*)aip.ToPointer();
				var templ = (long)tempub;
				p = templ;
			}
			else if (ps.EndsWith("char*", StringComparison.OrdinalIgnoreCase) || ps.EndsWith("charp", StringComparison.OrdinalIgnoreCase))
			{
				var tempb = *(sbyte*)aip.ToPointer();
				var templ = (long)tempb;
				p = templ;
			}
			else if (ps.EndsWith("str*", StringComparison.OrdinalIgnoreCase))
			{
				var s = (long*)aip.ToPointer();
				p = Strings.StrGet(new IntPtr(*s));
			}
			else if (ps.EndsWith('*') || ps.EndsWith("p", StringComparison.OrdinalIgnoreCase))//Last attempt if nothing else worked.
			{
				var pp = (long*)aip.ToPointer();
				p = *pp;
			}
		}

		private static unsafe void FixParamTypesAndCopyBack(object[] parameters, object[] args)
		{
			//Ensure arguments passed in are in the proper format when writing back.
			for (int pi = 0, ai = 0; pi < parameters.Length; pi += 2, ++ai)
			{
				if (pi < parameters.Length - 1)
				{
					var p0 = parameters[pi];
					//var p1 = parameters[pi + 1];

					//If they passed in a ComObject with Ptr as an address, make that address into a __ComObject.
					if (parameters[pi + 1] is ComObject co)
					{
						object obj = co.Ptr;
						co.Ptr = obj;//Reassign to ensure pointers are properly cast to __ComObject.
					}
					else if (p0 is string ps)
					{
						if (args[ai] is IntPtr aip && (ps[^1] == '*' || ps[^1] == 'p'))
						{
							FixParamTypeAndCopyBack(ref parameters[pi + 1], ps, aip);//Must reference directly into the array, not a temp variable.
						}
					}
				}
			}
		}

		/// <summary>
		/// Compose a string that uniquely identifies a call to a DLL function with specific arguments.
		/// This is used as a key to the <see cref="dllCache"/> dictionary to optimize performance.
		/// </summary>
		/// <param name="name">The name of the function.</param>
		/// <param name="path">The path to the DLL the function resides in.</param>
		/// <param name="helper">The helper which contains the argument types and names.</param>
		/// <returns>A string which uniquely identifies a DLL call.</returns>
		private static string GetDllCallId(string name, string path, DllArgumentHelper helper)
		{
			return $"{name}{path}{helper.ReturnType}{string.Join(',', helper.names)}-{string.Join(',', helper.types.Select(t => t.Name))}";
		}
	}

	/// <summary>
	/// A cached DLL assembly/module/type/method to be reused when
	/// the same function is repeatedly called with the same number and type of arguments.
	/// </summary>
	internal class DllCache
	{
		/// <summary>
		/// The assembly to cache.
		/// </summary>
		internal AssemblyBuilder assembly;

		/// <summary>
		/// The container to cache.
		/// </summary>
		internal TypeBuilder container;

		/// <summary>
		/// The created type to cache.
		/// </summary>
		internal Type created;

		/// <summary>
		/// The pinvoke method to cache.
		/// </summary>
		internal MethodBuilder invoke;

		/// <summary>
		/// The created pinvoke method from the type to cache.
		/// </summary>
		internal MethodInfo method;

		/// <summary>
		/// The created dynamic module to cache.
		/// </summary>
		internal ModuleBuilder module;
	}
}

#endif