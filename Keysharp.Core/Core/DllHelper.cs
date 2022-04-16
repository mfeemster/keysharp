using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Keysharp.Core.Common.Patterns;
using Keysharp.Core.Windows;

namespace Keysharp.Core
{
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
			//You should some day add the ability to use this with .NET dlls, exposing some type of reflection to the script.//MATT
			function = function;
			var types = new Type[parameters.Length / 2];
			var args = new object[types.Length];
			var returnType = typeof(int);
			var returnName = "";
			var cdecl = false;
			const string Cdecl = "cdecl";
			var hasreturn = (parameters.Length & 1) == 1;
			var gcHandles = new HashSet<GCHandle>();
			var gcHandlesScope = new ScopeHelper(gcHandles);
			gcHandlesScope.eh += (sender, o) =>
			{
				if (o is HashSet<GCHandle> hs)
					foreach (var gch in hs) gch.Free();
			};
			void SetupPointerArg(int i, int n)
			{
				var gch = GCHandle.Alloc(parameters[i], GCHandleType.Pinned);
				_ = gcHandles.Add(gch);
				var intptr = gch.AddrOfPinnedObject();
				args[n] = intptr;
			}

			for (var i = 0; i < parameters.Length; i++)
			{
				Type type = null;
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
					case "str": type = typeof(string); break;

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
								else if (parameters[i] is int || parameters[i] is long)
									args[n] = new IntPtr((long)Convert.ChangeType(parameters[i], typeof(long)));
								else if (parameters[i] is Buffer buf)
									args[n] = buf.Ptr;
								else if (parameters[i] is DelegateHolder delholder)
								{
									args[n] = delholder.thisdel;
									types[n] = delholder.thisdel.GetType();
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
								else
									SetupPointerArg(i, n);//If it wasn't any of the above types, just take the address, which ends up being the same as int* etc...
							}
							else
								SetupPointerArg(i, n);
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
						throw new Error($"Unable to locate dll with path {name}");
					}
				}
				else
				{
					z++;

					if (z >= path.Length)
						throw new Error($"Improperly formatted path of {path}");

					name = path.Substring(z);
					path = path.Substring(0, z - 1);
				}

				if (Environment.OSVersion.Platform == PlatformID.Win32NT && path.Length != 0 && !Path.HasExtension(path))
					path += ".dll";

				//if (!System.IO.File.Exists(path))
				//throw new Error($"{path} does not exist.");
				//Changed this from AppDomain.CurrentDomain.DefineDynamicAssembly to AssemblyBuilder.//MATT
				var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("pinvokes"), AssemblyBuilderAccess.Run);
				var module = assembly.DefineDynamicModule("module");
				var container = module.DefineType("container", TypeAttributes.Public | TypeAttributes.UnicodeClass);
				var invoke = container.DefinePInvokeMethod(
								 name,
								 path,
								 MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl,
								 CallingConventions.Standard,
								 returnType,
								 types,
								 cdecl ? CallingConvention.Cdecl : CallingConvention.Winapi,
								 CharSet.Auto);
				invoke.SetImplementationFlags(invoke.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);
				var created = container.CreateType();
				// TODO: pinvoke method caching

				try
				{
					var method = created.GetMethod(name);

					if (method == null)
						throw new Error($"Method {name} could not be found.");

					var value = method.Invoke(null, args);

					if (returnName == "HRESULT" && value is int retval && retval < 0)
					{
						var ose = new OSError($"DllCall with return type of HRESULT returned {retval}");
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
			else if (function is Delegate del)
			{
				return del.DynamicInvoke(args);
			}
			else if (function is int || function is long)
			{
				var address = (int)function;

				if (address < 0)
					throw new ValueError($"Function argument of type {function.GetType()} was treated as an address and had a negative value of {address}. It must greater than 0.");

				try
				{
					var value = Marshal.GetDelegateForFunctionPointer(new IntPtr(address), typeof(Delegate)).Method.Invoke(null, args);
					return value;
				}
				catch (Exception e)
				{
					var error = new Error($"An error occurred when calling {function}: {e.Message}");
					error.Extra = "0x" + Accessors.A_LastError.ToString("X");
					throw error;
				}
			}
			else if (function is float || function is double || function is decimal)
			{
				throw new TypeError($"Function argument was of type {function.GetType()}. It must be string, StringBuffer, int, long or an object with a Ptr member.");
			}
			else
			{
				var val = Keysharp.Core.Reflections.SafeGetProperty<IntPtr>(function, "Ptr");
				return val == IntPtr.Zero
					   ?                   throw new PropertyError($"Passed in object of type {function.GetType()} did not contain a property named Ptr")
					   : val;
			}
		}

		/// <summary>
		/// Find a function in the local scope.
		/// </summary>
		/// <param name="name">The name of the function.</param>
		/// <returns>A delegate (function pointer).</returns>
		//public static Delegate FunctionReference(string name)
		//{
		//  var method = Reflections.FindLocalMethod(name);
		//  var info = Expression.GetDelegateType(
		//                 //var info = Expression.GetFuncType(
		//                 (from parameter in method.GetParameters() select parameter.ParameterType)
		//                 .Concat(new[] { method.ReturnType })
		//                 .ToArray());
		//  return method?.CreateDelegate(info);
		//  //return method == null ? null : (Core.GenericFunction)Delegate.CreateDelegate(method.ReflectedType, method);
		//  //return method == null ? null : (Core.GenericFunction)Delegate.CreateDelegate(typeof(Core.GenericFunction), method);
		//}

		/// <summary>
		/// Returns a binary number stored at the specified address in memory.
		/// </summary>
		/// <param name="address">The address in memory.</param>
		/// <param name="offset">The offset from <paramref name="address"/>.</param>
		/// <param name="type">Any type outlined in <see cref="DllCall"/>.</param>
		/// <returns>The value stored at the address.</returns>
		//public static object NumGet(object address, long offset = 0, string type = "UInt")
		public static object NumGet(params object[] obj)
		{
			var (address, offset, type) = obj.L().O1L1S1("", 0, "UInt");
			IntPtr addr;
			var off = (int)offset;
			var buf = address as Buffer;

			if (buf != null)
				addr = buf.Ptr;
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
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}");

					return (long)(uint)Marshal.ReadInt32(addr, off);

				case "int":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}");

					return (long)Marshal.ReadInt32(addr, off);

				case "short":
					if (buf != null && (off + 2 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}");

					return (long)Marshal.ReadInt16(addr, off);

				case "ushort":
					if (buf != null && (off + 2 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 2 > buffer size {(long)buf.Size}");

					return (long)(ushort)Marshal.ReadInt16(addr, off);

				case "char":
					if (buf != null && (off + 1 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}");

					return (long)(sbyte)Marshal.ReadByte(addr, off);

				case "uchar":
					if (buf != null && (off + 1 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 1 > buffer size {(long)buf.Size}");

					return (long)Marshal.ReadByte(addr, off);

				case "double":
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}");

					unsafe
					{
						var ptr = (double*)(addr + off).ToPointer();
						var val = *ptr;
						return val;

					}

				case "float":
					if (buf != null && (off + 4 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 4 > buffer size {(long)buf.Size}");

					unsafe
					{
						var ptr = (float*)(addr + off).ToPointer();
						return double.Parse((*ptr).ToString());//Need to convert to string to make it exact, else there can be lots of rounding/trailing digits.

					}

				case "int64":
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}");

					return Marshal.ReadInt64(addr, off);

				case "ptr":
				case "uptr":
				default:
					if (buf != null && (off + 8 > (long)buf.Size))
						throw new IndexError($"Memory access exceeded buffer size. Offset {off} + length 8 > buffer size {(long)buf.Size}");

					return Marshal.ReadIntPtr(addr, off).ToInt64();
			}
		}

		public static long NumPut(params object[] obj)
		{
			Buffer buf;
			var l = obj.L();
			var offset = 0;
			int lastPairIndex;
			var offsetSpecified = !((l.Count & 1) == 1);

			if (offsetSpecified)
			{
				lastPairIndex = l.Count - 4;
				offset = l.Ai(l.Count - 1);
				buf = l[l.Count - 2] as Buffer;
			}
			else
			{
				lastPairIndex = l.Count - 3;
				buf = l[l.Count - 1] as Buffer;
			}

			for (var i = 0; i <= lastPairIndex; i += 2)
			{
				var type = l[i] as string;
				var number = l[i + 1];
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
					throw new IndexError($"Memory access exceeded buffer size. Offset {offset} + length {bytes.Length} > buffer size {(long)buf.Size}");
			}

			return buf.Ptr.ToInt64() + offset;
		}

		/// <summary>
		/// Converts a local function to a native function pointer.
		/// </summary>
		/// <param name="function">The name of the function.</param>
		/// <param name="args">Unused legacy parameters.</param>
		/// <returns>An integer address to the function callable by unmanaged code.</returns>

		public static DelegateHolder CallbackCreate(params object[] obj)
		{
			var (function, options, paramcount) = obj.L().Osi();
			return new DelegateHolder(function, options.Contains("f", StringComparison.OrdinalIgnoreCase), options.Contains("&"));
		}

		/// <summary>
		/// No need for this function, the garbage collector handles memory. Keep it here for backward compatibility.
		/// </summary>
		/// <param name="obj">Unused</param>
		public static void CallbackFree(params object[] obj)
		{
		}

		/// <summary>
		/// Enlarges a variable's holding capacity. Usually only necessary for <see cref="DllCall"/>.
		/// </summary>
		/// <param name="variable">The variable to change.</param>
		/// <param name="capacity">Specify zero to return the current capacity.
		/// Otherwise <paramref name="variable"/> will be recreated as a byte array with this total length.</param>
		/// <param name="pad">Specify a value between 0 and 255 to initalise each index with this number.</param>
		/// <returns>The total capacity of <paramref name="variable"/>.</returns>
		//public static int VarSetCapacity(ref object variable, int capacity = 0, int pad = -1)
		//{
		//  if (capacity == 0)
		//      return Marshal.SizeOf(variable);
		//
		//  var bytes = new byte[capacity];
		//  var fill = (byte)pad;
		//
		//  if (pad > -1 && pad < 256)
		//      for (var i = 0; i < bytes.Length; i++)
		//          bytes[i] = fill;
		//
		//  variable = bytes;
		//  return bytes.Length;
		//}
	}
}