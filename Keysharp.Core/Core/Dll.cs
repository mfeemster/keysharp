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
		private static readonly ConcurrentDictionary<int, Func<IntPtr, long[], long>> delegateCache = new();
		private static readonly ConcurrentDictionary<long, DelegateHolder> callbackCache = new();
		private static readonly Dictionary<string, IntPtr> loadedDlls = new()
		{
			{ "user32", NativeLibrary.Load("user32") },
			{ "kernel32", NativeLibrary.Load("kernel32") },
			{ "comctl32", NativeLibrary.Load("comctl32") },
			{ "gdi32", NativeLibrary.Load("gdi32") }
		};
		private static readonly ConcurrentDictionary<string, IntPtr> procAddressCache = new(StringComparer.OrdinalIgnoreCase);

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
		public static object CallbackCreate(object function, object options = null, object paramCount = null)
		{
			var o = options.As();
			return new DelegateHolder(function, o.Contains('f', StringComparison.OrdinalIgnoreCase), o.Contains('&'), paramCount.Ai(-1));
		}

		/// <summary>
		/// Frees the specified callback by internally setting it to null.
		/// </summary>
		/// <param name="address">The <see cref="DelegateHolder"/> to be freed.</param>
		public static object CallbackFree(object address)
		{
			if (address is DelegateHolder dh)
				dh.Clear();
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
			nint handle = IntPtr.Zero;
			nint address = IntPtr.Zero;

			if (function is string path)
			{
				string name;
				var z = path.LastIndexOf(Path.DirectorySeparatorChar);

				if (z == -1)
				{
					name = path;

					if (procAddressCache.TryGetValue(name, out address))
						goto AddressFound;

					if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					{
						foreach (var dll in loadedDlls)
						{
							if (NativeLibrary.TryGetExport(dll.Value, name, out address))
							{
								procAddressCache[name] = address;
								procAddressCache[dll.Key + Path.DirectorySeparatorChar + name] = address;
								goto AddressFound;
							}
						}

						var nameW = name + "W";
						foreach (var dll in loadedDlls)
						{
							if (NativeLibrary.TryGetExport(dll.Value, nameW, out address))
							{
								procAddressCache[name] = address;
								procAddressCache[dll.Key + Path.DirectorySeparatorChar + name] = address;
								goto AddressFound;
							}
						}
					}

					return Errors.ErrorOccurred(err = new Error($"Unable to locate dll with path {path}.")) ? throw err : null;
				} 
				else if (loadedDlls.Keys.FirstOrDefault(n => path.StartsWith(n, StringComparison.OrdinalIgnoreCase)) is string moduleName && moduleName != null)
				{
					name = path.Substring(z + 1);
					var key = moduleName + Path.DirectorySeparatorChar + name;

					if (procAddressCache.TryGetValue(key, out address))
						goto AddressFound;

					if (!NativeLibrary.TryGetExport(loadedDlls[moduleName], name, out address))
					{
						NativeLibrary.TryGetExport(loadedDlls[moduleName], name + "W", out address);
					}

					if (address == IntPtr.Zero)
						return Errors.ErrorOccurred(err = new Error($"Unable to locate dll with path {path}.")) ? throw err : null;
					else
					{
						procAddressCache[name] = address;
						procAddressCache[key] = address;
					}
				}
				else
				{
					z++;

					if (z >= path.Length)
						return Errors.ErrorOccurred(err = new Error($"Improperly formatted path of {path}.")) ? throw err : null;

					name = path.Substring(z);
					path = path.Substring(0, z - 1);

					if (Environment.OSVersion.Platform == PlatformID.Win32NT && path.Length != 0 && !Path.HasExtension(path))
						path += ".dll";

					NativeLibrary.TryLoad(path, out handle);

					if (handle != IntPtr.Zero && !NativeLibrary.TryGetExport(handle, name, out address))
						NativeLibrary.TryGetExport(handle, name + "W", out address);
				}
			}
			else
				address = Reflections.GetPtrProperty(function);

			AddressFound:

			if (address == IntPtr.Zero)
				return Errors.ErrorOccurred(err = new TypeError($"Function argument was of type {function.GetType()}. It must be string, StringBuffer, integer, Buffer or other object with a Ptr property that is an integer.")) ? throw err : null;

			try
			{
				var helper = new ArgumentHelper(parameters);
				var value = CallDel(address, helper.args);
				FixParamTypesAndCopyBack(parameters, helper);
				helper.Dispose();

				//Special conversion for the return value.
				if (helper.ReturnType == typeof(int))
				{
					int ii = *(int*)&value;
					value = ii;
				}
				else if (helper.ReturnType == typeof(string))
				{
					var str = Marshal.PtrToStringUni((nint)value);
					_ = Strings.FreeStrPtr(value);//If this string came from us, it will be freed, else no action.
					return str;
				}

				return value;
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(err = new Error($"An error occurred when calling {function}(): {ex.Message}")
				{
					Extra = "0x" + A_LastError.ToString("X")
				}) ? throw err : null;
			}
			finally
			{
				if (handle != IntPtr.Zero)
					NativeLibrary.Free(handle);
			}
		}

		/// <summary>
		/// A private helper to compile and cache a delegate with the appropriate number of<br/>
		/// parameters and then invoke it. All arguments are considered <see cref="long"/> internally.
		/// </summary>
		/// <param name="vtbl">The vtbl of the COM object.</param>
		/// <param name="args">The argument list.</param>
		/// <returns>An <see cref="IntPtr"/> which contains the return value of the COM call.</returns>
		internal static long CallDel(IntPtr vtbl, long[] args)
		{
			var del = delegateCache.GetOrAdd(args.Length, CreateDelegateForArgCount);
			return del(vtbl, args);
		}

		/// <summary>
		/// Generates (and returns) a delegate of type Func<IntPtr, long[], long>
		/// for a function pointer that accepts exactly n long arguments.
		/// It reads n arguments from the provided array (pushing 0 for missing entries),
		/// then loads the function pointer and calls it.
		/// </summary>
		/// <param name="n">The number of long arguments expected.</param>
		private static Func<IntPtr, long[], long> CreateDelegateForArgCount(int n)
		{
			// The dynamic method always has two parameters:
			//  - IntPtr: the function pointer,
			//  - long[]: the array of arguments.
			DynamicMethod dm = new DynamicMethod(
				"DynamicDllCall_" + n,
				typeof(long),
				new[] { typeof(IntPtr), typeof(long[]) },
				typeof(Dll).Module,
				skipVisibility: true);
			ILGenerator il = dm.GetILGenerator();

			// Unroll the loading of n arguments from the array.
			for (int i = 0; i < n; i++)
			{
				// Load the 'args' array (argument at index 1).
				il.Emit(OpCodes.Ldarg_1);
				// Push the constant index i onto the stack.
				il.Emit(OpCodes.Ldc_I4, i);
				// Load the long element at that index (expects each element is an 8-byte long).
				il.Emit(OpCodes.Ldelem_I8);
			}

			// Load the function pointer from the first parameter (IntPtr).
			il.Emit(OpCodes.Ldarg_0);
			// Build an array of parameter types (n longs).
			Type[] paramTypes = Enumerable.Repeat(typeof(long), n).ToArray();
			// Emit a calli instruction to call the unmanaged function.
			// This assumes a StdCall calling convention and that the function returns a long.
			il.EmitCalli(OpCodes.Calli, CallingConvention.StdCall, typeof(long), paramTypes);
			il.Emit(OpCodes.Ret);
			return (Func<IntPtr, long[], long>)dm.CreateDelegate(typeof(Func<IntPtr, long[], long>));
		}

		internal static unsafe void FixParamTypeAndCopyBack(ref object p, Type t, IntPtr aip)
		{
			if (t == typeof(uint))
			{
				var tempui = *(uint*)aip.ToPointer();
				var templ = (long)tempui;
				p = templ;
			}
			else if (t == typeof(int))
			{
				var tempi = *(int*)aip.ToPointer();
				var templ = (long)tempi;
				p = templ;
			}
			else if (t == typeof(long))
			{
				var templ = *(long*)aip.ToPointer();
				p = templ;
			}
			else if (t == typeof(double))
			{
				var tempd = *(double*)aip.ToPointer();
				p = tempd;
			}
			else if (t == typeof(float))
			{
				var tempf = *(float*)aip.ToPointer();
				var tempd = (double)tempf;
				p = tempd;
			}
			else if (t == typeof(ushort))
			{
				var tempus = *(ushort*)aip.ToPointer();
				var templ = (long)tempus;
				p = templ;
			}
			else if (t == typeof(short))
			{
				var temps = *(short*)aip.ToPointer();
				var templ = (long)temps;
				p = templ;
			}
			else if (t == typeof(byte))
			{
				var tempub = *(byte*)aip.ToPointer();
				var templ = (long)tempub;
				p = templ;
			}
			else if (t == typeof(sbyte))
			{
				var tempb = *(sbyte*)aip.ToPointer();
				var templ = (long)tempb;
				p = templ;
			}
			else if (t == typeof(string))
			{
				var s = (long*)aip.ToPointer();
				p = Strings.StrGet(new IntPtr(*s));
			}
			else
			{
				var pp = (long*)aip.ToPointer();
				p = *pp;
			}
		}

		internal static unsafe void FixParamTypesAndCopyBack(object[] parameters, ArgumentHelper helper)
		{
			//Ensure arguments passed in are in the proper format when writing back.
			foreach (var pair in helper.outputVars)
			{
				var pi = pair.Key;
				var n = pi / 2;

				//If they passed in a ComObject with Ptr as an address, make that address into a __ComObject.
				if (parameters[pi] is ComObject co)
				{
					object obj = co.Ptr;
					co.Ptr = obj;//Reassign to ensure pointers are properly cast to __ComObject.
				}
				else
				{
					var arg = helper.args[n];
					if (parameters[pi] is KeysharpObject kso)
					{
						object temp = arg;
						FixParamTypeAndCopyBack(ref temp, pair.Value, (nint)arg);
						_ = Script.SetPropertyValue(kso, "ptr", temp);//Write it back to the ptr property of the KeysharpObject.
					}
					else
					{
						FixParamTypeAndCopyBack(ref parameters[pi], pair.Value, (nint)arg);
					}
				}
				/*
				else
				{
					if (parameters[pi + 1] is string s)
					{
						if (arg is not string)
						{
							//var s = (long*)aip.ToPointer();
							//p = Strings.StrGet(new IntPtr(s));
							if (arg is IntPtr aip)
								parameters[pi + 1] = Strings.StrGet(aip);
							else if (arg is long l)
								parameters[pi + 1] = Strings.StrGet((nint)l);
						}
					}
				}
				*/
			}
		}
	}
}

#endif