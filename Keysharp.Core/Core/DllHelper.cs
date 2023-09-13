using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Keysharp.Core.COM;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Patterns;
using Keysharp.Core.Windows;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Keysharp.Core
{
	internal class DllArgumentHelper
	{
		const string Cdecl = "cdecl";
		internal bool cdecl = false;
		internal string returnName = "";
		internal object[] args;
		internal Type[] types;
		internal bool hasreturn;
		internal Type returnType = typeof(void);
		private HashSet<GCHandle> gcHandles = new HashSet<GCHandle>();
		private ScopeHelper gcHandlesScope;

		internal DllArgumentHelper(object[] parameters)
		{
			gcHandlesScope = new ScopeHelper(gcHandles);
			gcHandlesScope.eh += (sender, o) =>
			{
				if (o is HashSet<GCHandle> hs)
					foreach (var gch in hs)
						gch.Free();
			};
			ConvertKeysharpParametersToDllParameters(parameters);
		}
		private void ConvertKeysharpParametersToDllParameters(object[] parameters)
		{
			void SetupPointerArg(int i, int n, object obj = null)
			{
				var gch = GCHandle.Alloc(obj != null ? obj : parameters[i], GCHandleType.Pinned);
				_ = gcHandles.Add(gch);
				var intptr = gch.AddrOfPinnedObject();
				args[n] = intptr;
			}
			types = new Type[parameters.Length / 2];
			args = new object[types.Length];
			hasreturn = (parameters.Length & 1) == 1;

			for (var i = 0; i < parameters.Length; i++)
			{
				var name = parameters[i].ToString().ToLowerInvariant().Trim();
				var isreturn = hasreturn && i == parameters.Length - 1;

				if (isreturn)
				{
					if (name.StartsWith(Cdecl, StringComparison.OrdinalIgnoreCase))
					{
						name = name.Substring(Cdecl.Length).Trim();
						cdecl = true;
						returnName = name;

						if (name?.Length == 0)
							continue;
					}
				}

				Type type;

				switch (name[name.Length - 1])
				{
					case '*':
					case 'P':
					case 'p':
						name = name.Substring(0, name.Length - 1).Trim();
						type = typeof(IntPtr);
						goto TypeDetermined;
						//break;
				}

				switch (name)
				{
					case "wstr":
					case "str": type = typeof(string); break;

					case "astr": type = typeof(IntPtr); break;

					case "int64": type = typeof(long); break;

					case "uint64": type = typeof(ulong); break;

					case "hresult":
					case "int": type = typeof(int); break;

					case "uint": type = typeof(uint); break;

					case "short": type = typeof(short); break;

					case "ushort": type = typeof(ushort); break;

					case "char": type = typeof(char); break;

					case "uchar": type = typeof(char); break;

					case "float": type = typeof(float); break;

					case "double": type = typeof(double); break;

					case "ptr": type = typeof(IntPtr); break;

					default:
						throw new ValueError($"Arg or return type of {name} is invalid.");
				}

				TypeDetermined:
				i++;

				if (isreturn)
				{
					returnType = type;
				}
				else if (!isreturn && i < parameters.Length)
				{
					var n = i / 2;
					types[n] = type;

					try
					{
						if (type == typeof(IntPtr))
						{
							if (name == "ptr")
							{
								if (parameters[i] is null)
									args[n] = IntPtr.Zero;
								else if (parameters[i] is IntPtr)
									args[n] = parameters[i];
								else if (parameters[i] is int || parameters[i] is long || parameters[i] is uint)
									args[n] = new IntPtr((long)Convert.ChangeType(parameters[i], typeof(long)));
								else if (parameters[i] is Buffer buf)
									args[n] = buf.Ptr;
								else if (parameters[i] is DelegateHolder delholder)
								{
									args[n] = delholder.delRef;
									types[n] = delholder.delRef.GetType();
								}
								else if (parameters[i] is StringBuffer sb)
								{
									args[n] = sb.sb;
									types[n] = typeof(StringBuilder);
								}
								else if (parameters[i] is Delegate del)
								{
									args[n] = del;
									types[n] = del.GetType();
								}
								else if (parameters[i] is System.Array arr)
									//else if (parameters[i] is ComObjArray arr)
								{
									args[n] = arr;
									types[n] = arr.GetType();
								}
								else
									SetupPointerArg(i, n);//If it wasn't any of the above types, just take the address, which ends up being the same as int* etc...
							}
							else if (name == "uint" || name == "int")
							{
								if (parameters[i] is null)
									args[n] = 0;
								else if (parameters[i] is IntPtr ip)
									args[n] = ip.ToInt64();
								else
									args[n] = (int)parameters[i].Al();
							}
							else if (name == "astr")
							{
								if (parameters[i] is string s)
									SetupPointerArg(i, n, ASCIIEncoding.ASCII.GetBytes(s));
								else
									throw new TypeError($"Argument had type astr but was not a string.");
							}
							else
								SetupPointerArg(i, n);
						}
						else if (type == typeof(int))
						{
							if (parameters[i] is null)
								args[n] = 0;
							else if (parameters[i] is IntPtr ip)
								args[n] = (int)ip.ToInt32();
							else
								args[n] = (int)parameters[i].Al();
						}
						else if (type == typeof(uint))
						{
							if (parameters[i] is null)
								args[n] = 0u;
							else if (parameters[i] is IntPtr ip)
								args[n] = (uint)ip.ToInt64();
							else
								args[n] = (uint)parameters[i].Al();
						}
						else
							args[n] = Convert.ChangeType(parameters[i], type);
					}
					catch (Exception e)
					{
						throw new TypeError($"Argument type conversion failed: {e.Message}");
					}
				}
			}
		}
	}

	public static class DllHelper
	{
		//public static object DllCall(object function, string t1, ref object p1, string t2, ref object p2)
		//{
		//  return DllCallInternal(function, new object[] { t1, p1, t2, p2 });
		//}

		//public static object DllCall(object function, string t1, ref IntPtr p1, string t2, ref object p2)
		//{
		//  return DllCallInternal(function, new object[] { t1, p1, t2, p2 });
		//}
		//
		//public static object DllCall(object function, string t1, ref object p1, string t2, ref object p2, string ret)
		//{
		//  return DllCallInternal(function, new object[] { t1, p1, t2, p2, ret });
		//}

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
			var helper = new DllArgumentHelper(parameters);

			if (function is string path)
			{
				string name;
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

				var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("pinvokes"), AssemblyBuilderAccess.RunAndCollect);
				var module = assembly.DefineDynamicModule("module");
				var container = module.DefineType("container", TypeAttributes.Public | TypeAttributes.UnicodeClass);
				var invoke = container.DefinePInvokeMethod(
								 name,
								 path,
								 MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl,
								 CallingConventions.Standard,
								 helper.returnType,
								 helper.types,
								 helper.cdecl ? CallingConvention.Cdecl : CallingConvention.Winapi,
								 CharSet.Auto);
				invoke.SetImplementationFlags(invoke.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);

				for (var i = 0; i < helper.args.Length; i++)
				{
					if (helper.args[i] is System.Array array)
					{
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

				try
				{
					var method = created.GetMethod(name);

					if (method == null)
						throw new Error($"Method {name} could not be found.");

					var value = method.Invoke(null, helper.args);

					if (helper.returnName == "HRESULT" && value is int retval && retval < 0)
					{
						var ose = new OSError($"DllCall with return type of HRESULT returned {retval}.");
						ose.Extra = "0x" + ose.Number.ToString("X");
						throw ose;
					}

					return value;
				}
				catch (Exception e)
				{
					var inner = e.InnerException != null ? " " + e.InnerException.Message : "";

					if (e.InnerException is Keysharp.Core.Error err)
						inner += " " + err.Message;

					var error = new Error($"An error occurred when calling {name} in {path}: {e.Message}{inner}");
					error.Extra = "0x" + Accessors.A_LastError.ToString("X");
					throw error;
				}
			}
			else if (function is DelegateHolder dh)
			{
				var longs = new IntPtr[31];
				unsafe
				{
					//fixed (object* pin = args)
					{
						for (var i = 0; i < helper.args.Length && i < longs.Length; i++)
						{
							if (helper.types[i] == typeof(float))
							{
								var f = (float)helper.args[i];
								int* iref = (int*)&f;
								longs[i] = new IntPtr(*iref);
							}
							else if (helper.types[i] == typeof(double))
							{
								var d = (double)helper.args[i];
								long* lref = (long*)&d;
								longs[i] = new IntPtr(*lref);
							}
							else if (helper.types[i] == typeof(long))
							{
								var l = (long)helper.args[i];
								longs[i] = new IntPtr(l);
							}
							else if (helper.types[i] == typeof(IntPtr))
							{
								longs[i] = (IntPtr)helper.args[i];
							}
							else if (helper.types[i] == typeof(string))
							{
								var str = helper.args[i] as string;

								fixed (char* p = str)//If the string moves after this is assigned, the program will likely crash. Unsure what else to do.
								{
									longs[i] = new IntPtr(p);
								}
							}
							else if (helper.types[i] == typeof(ulong))
							{
								var ul = (ulong)helper.args[i];
								longs[i] = new IntPtr((long)ul);
							}
							else if (helper.types[i] == typeof(int))
							{
								var ii = (int)helper.args[i];
								longs[i] = new IntPtr((long)ii);
							}
							else if (helper.types[i] == typeof(uint))
							{
								var ui = (uint)helper.args[i];
								longs[i] = new IntPtr(ui);
							}
							else if (helper.types[i] == typeof(short))
							{
								var s = (short)helper.args[i];
								longs[i] = new IntPtr(s);
							}
							else if (helper.types[i] == typeof(ushort))
							{
								var us = (ushort)helper.args[i];
								longs[i] = new IntPtr(us);
							}
							else if (helper.types[i] == typeof(char))
							{
								var c = (char)helper.args[i];
								longs[i] = new IntPtr(c);
							}
						}
					}
				}
				return dh.DelegatePlaceholderArr(longs);
			}
			else if (function is Delegate del)
			{
				return del.DynamicInvoke(helper.args);
			}
			else
			{
				var address = 0L;

				if (function is IntPtr ip)
					address = ip.ToInt64();
				else if (function is int || function is long)
					address = function.Al();

				if (address > 0)//Nothing in this block works and the code below is the remnants of various attempts.
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
						var delType = Expression.GetDelegateType(helper.types.Concat(new[] { helper.returnType}));
						var ptrdel = GetDelegateForFunctionPointerFix(new IntPtr(address), delType);
						//System.Runtime.CompilerServices.
						//System.Linq.Expressions.Compiler.DelegateHelpers.MakeNewCustomDelegate
						//var ptrdel = Marshal.GetDelegateForFunctionPointer(new IntPtr(address), typeof(Action));
						var value = ptrdel.DynamicInvoke(helper.args.Length == 0 ? null : helper.args);
						//var value = ptrdel.Method.Invoke(null, args);
						return value;
					}
					catch (Exception e)
					{
						var error = new Error($"An error occurred when calling {function}: {e.Message}");
						error.Extra = "0x" + Accessors.A_LastError.ToString("X");
						throw error;
					}
				}
				else if (address < 0)
				{
					throw new ValueError($"Function argument of type {function.GetType()} was treated as an address and had a negative value of {address}. It must greater than 0.");
				}
				else if (function is float || function is double || function is decimal)
				{
					throw new TypeError($"Function argument was of type {function.GetType()}. It must be string, StringBuffer, int, long or an object with a Ptr member.");
				}
				else
				{
					var val = Keysharp.Core.Reflections.SafeGetProperty<IntPtr>(function, "Ptr");
					return val == IntPtr.Zero
						   ? throw new PropertyError($"Passed in object of type {function.GetType()} did not contain a property named Ptr.")
						   : val;
				}
			}
		}

		private static Func<IntPtr, Type, Delegate> GetDelegateForFunctionPointerInternalPointer;

		/// <summary>
		/// https://github.com/dotnet/runtime/issues/13578
		/// </summary>
		/// <param name="ptr"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static Delegate GetDelegateForFunctionPointerFix(IntPtr ptr, Type t)
		{
			//Validate the parameters (modified from https://referencesource.microsoft.com/#mscorlib/system/runtime/interopservices/marshal.cs)
			if (ptr == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(ptr));
			}

			if (t is null)
			{
				throw new ArgumentNullException(nameof(t));
			}

			//skip the IsRuntimeImplemented check as IsRuntimeImplemented is not visible and I cannot be bothered

			if (t.IsGenericType && !t.IsConstructedGenericType)
			{
				throw new ArgumentException("The specified Type must not be an open generic type definition.", nameof(t));
			}

			Type? c = t.BaseType;

			if (c != typeof(Delegate) && c != typeof(MulticastDelegate))
			{
				throw new ArgumentException("Type must derive from Delegate or MulticastDelegate.", nameof(t));
			}

			if (GetDelegateForFunctionPointerInternalPointer is null)
			{
				GetDelegateForFunctionPointerInternalPointer = typeof(Marshal).GetMethod("GetDelegateForFunctionPointerInternal", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate<Func<IntPtr, Type, Delegate>>();
			}

			return GetDelegateForFunctionPointerInternalPointer(ptr, t);
		}

		/// <summary>
		/// Returns a binary number stored at the specified address in memory.
		/// </summary>
		/// <param name="address">The address in memory.</param>
		/// <param name="offset">The offset from <paramref name="address"/>.</param>
		/// <param name="type">Any type outlined in <see cref="DllCall"/>.</param>
		/// <returns>The value stored at the address.</returns>
		//public static object NumGet(object address, long offset = 0, string type = "UInt")
		public static object NumGet(object obj0, object obj1 = null, object obj2 = null)
		{
			var address = obj0;
			var offset = obj1.Al();
			var type = obj2.As("UInt");
			IntPtr addr;
			var off = (int)offset;
			var buf = address as Buffer;

			if (buf != null)
				addr = buf.Ptr;
			else if (address is object[] objarr && objarr.Length > 0)//Assume the first element was a long which was an address.
				addr = new nint(objarr[0].Al());
			else if (address is IntPtr ptr)
				addr = ptr;
			else if (address is long l)
				addr = new IntPtr(l);
			else if (address is long i)
				addr = new IntPtr(i);
			else
				throw new TypeError($"Could not convert address argument of type {address.GetType()} into an IntPtr. Type must be int, long or Buffer.");

			switch (type.ToLower())
			{
				case "uint":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.");

					return (long)(uint)Marshal.ReadInt32(addr, off);

				case "int":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.");

					return (long)Marshal.ReadInt32(addr, off);

				case "short":
					if (buf != null && (off + 2 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}.");

					return (long)Marshal.ReadInt16(addr, off);

				case "ushort":
					if (buf != null && (off + 2 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}.");

					return (long)(ushort)Marshal.ReadInt16(addr, off);

				case "char":
					if (buf != null && (off + 1 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}.");

					return (long)(sbyte)Marshal.ReadByte(addr, off);

				case "uchar":
					if (buf != null && (off + 1 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}.");

					return (long)Marshal.ReadByte(addr, off);

				case "double":
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.");

					unsafe
					{
						var ptr = (double*)(addr + off).ToPointer();
						var val = *ptr;
						return val;
					}

				case "float":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}.");

					unsafe
					{
						var ptr = (float*)(addr + off).ToPointer();
						return double.Parse((*ptr).ToString());//Need to convert to string to make it exact, else there can be lots of rounding/trailing digits.
					}

				case "int64":
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.");

					return Marshal.ReadInt64(addr, off);

				case "ptr":
				case "uptr":
				default:
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}.");

					return Marshal.ReadIntPtr(addr, off).ToInt64();
			}
		}
		public static long NumPut(params object[] obj)
		{
			Buffer buf;
			var offset = 0;
			int lastPairIndex;
			var offsetSpecified = !((obj.Length & 1) == 1);

			if (offsetSpecified)
			{
				lastPairIndex = obj.Length - 4;
				offset = obj.Ai(obj.Length - 1);
				buf = obj[obj.Length - 2] as Buffer;
			}
			else
			{
				lastPairIndex = obj.Length - 3;
				buf = obj[obj.Length - 1] as Buffer;
			}

			for (var i = 0; i <= lastPairIndex; i += 2)
			{
				var type = obj[i] as string;
				var number = obj[i + 1];
				var inc = 0;
				byte[] bytes;

				switch (type.ToLower())
				{
					case "int":
						bytes = BitConverter.GetBytes((int)Convert.ToInt64(number));
						inc = 4;
						break;

					case "uint":
						bytes = BitConverter.GetBytes((uint)Convert.ToUInt64(number));
						inc = 4;
						break;

					case "float":
						bytes = BitConverter.GetBytes(Convert.ToSingle(number));
						inc = 4;
						break;

					case "short":
						bytes = BitConverter.GetBytes((short)Convert.ToInt64(number));
						inc = 2;
						break;

					case "ushort":
						bytes = BitConverter.GetBytes((ushort)Convert.ToUInt64(number));
						inc = 2;
						break;

					case "char":
						bytes = new byte[] { (byte)Convert.ToInt32(number) };
						inc = 1;
						break;
					case "uchar":
						bytes = new byte[] { (byte)Convert.ToInt32(number) };
						inc = 1;
						break;
					case "double":
						bytes = BitConverter.GetBytes(Convert.ToDouble(number));
						inc = 8;
						break;

					case "int64":
						bytes = BitConverter.GetBytes(Convert.ToInt64(number));
						inc = 8;
						break;

					case "uint64":
					case "ptr":
					case "uptr":
						bytes = BitConverter.GetBytes(Convert.ToUInt64(number));
						inc = 8;
						break;

					default:
						bytes = System.Array.Empty<byte>();
						inc = 0;
						break;
				}

				if ((offset + bytes.Length) <= (long)buf.Size)
				{
					Marshal.Copy(bytes, 0, buf.Ptr + offset, bytes.Length);
					offset += inc;
				}
				else
					throw new IndexError($"Memory access exceeded buffer size. Offset {offset} + length {bytes.Length} > buffer size {(long)buf.Size}.");
			}

			return buf.Ptr.ToInt64() + offset;
		}
		/// <summary>
		/// Converts a local function to a native function pointer.
		/// </summary>
		/// <param name="function">The name of the function.</param>
		/// <param name="args">Unused legacy parameters.</param>
		/// <returns>An integer address to the function callable by unmanaged code.</returns>
	}
}