using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Core.COM;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Patterns;
using Keysharp.Scripting;

namespace Keysharp.Core
{
	internal class ComArgumentHelper : ArgumentHelper
	{
		internal IntPtr[] args;
		internal HashSet<IntPtr> bstrs;
		internal static char[] pointerChars = new char[] { ' ', '*', 'p', 'P' };

		internal ComArgumentHelper(object[] parameters)
			: base(parameters)
		{
			returnType = typeof(int);
		}

		~ComArgumentHelper()
		{
			if (bstrs != null)
			{
				foreach (var bstr in bstrs)
					Marshal.FreeBSTR(bstr);
			}
		}

		protected unsafe override void ConvertParameters(object[] parameters)
		{
			void SetupPointerArg(int i, int n, object obj = null)
			{
				GCHandle gch;

				if (obj != null)
					gch = GCHandle.Alloc(obj != null ? obj : parameters[i], GCHandleType.Pinned);
				else if (parameters[i] is ComObject co)
					gch = GCHandle.Alloc(co.Ptr, GCHandleType.Pinned);
				else
					gch = GCHandle.Alloc(parameters[i], GCHandleType.Pinned);

				_ = gcHandles.Add(gch);
				var intptr = gch.AddrOfPinnedObject();
				args[n] = intptr;
			}
			var len  = parameters.Length / 2;
			args = new IntPtr[len];
			hasreturn = (parameters.Length & 1) == 1;

			for (var i = 0; i < parameters.Length; i++)
			{
				var isreturn = hasreturn && i == parameters.Length - 1;
				var name = parameters[i].ToString().ToLowerInvariant().Trim(Keywords.Spaces);

				if (isreturn)
				{
					if (name.StartsWith(cdeclstr, StringComparison.OrdinalIgnoreCase))
					{
						name = name.Substring(cdeclstr.Length).Trim();
						cdecl = true;
						returnName = name;

						if (name?.Length == 0)
							continue;
					}
				}

				i++;
				Type type;
				var n = i / 2;
				var p = parameters[i];
				//var usePtr = false;

				switch (name[name.Length - 1])
				{
					case '*':
					case 'P':
					case 'p':
						name = name.TrimEnd(pointerChars);
						type = typeof(IntPtr);
						//usePtr = true;
						SetupPointerArg(i, n);
						goto TypeDetermined;
				}

				switch (name)
				{
					case "bstr":
					{
						type = typeof(string);

						if (p is string s)
						{
							var bstr = Marshal.StringToBSTR(s);

							if (bstrs == null)
								bstrs = new HashSet<nint>();

							_ = bstrs.Add(bstr);
							args[n] = bstr;
						}
						else
							throw new TypeError($"Argument had type {name} but was not a string.");

						break;
					}

					case "wstr":
					case "str":
					{
						type = typeof(string);

						if (p is string s)
							SetupPointerArg(i, n, UnicodeEncoding.UTF8.GetBytes(s + char.MinValue));
						else
							throw new TypeError($"Argument had type {name} but was not a string.");

						break;
					}

					case "astr":
					{
						type = typeof(IntPtr);

						if (p is string s)
							SetupPointerArg(i, n, ASCIIEncoding.ASCII.GetBytes(s + char.MinValue));
						else
							throw new TypeError($"Argument had type {name} but was not a string.");

						break;
					}

					case "int64":
					{
						type = typeof(long);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr(p.Al());

						break;
					}

					case "uint64":
					{
						type = typeof(ulong);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr(p.Al());//No real way to make an unsigned long here.

						//args[n] = new UIntPtr(p.Al());//No real way to make an unsigned long here.
						break;
					}

					case "hresult":
					case "int":
					{
						type = typeof(int);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr(p.Ai());

						break;
					}

					case "uint":
					{
						type = typeof(uint);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr(p.Aui());

						break;
					}

					case "short":
					{
						type = typeof(short);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr((short)p.Al());

						break;
					}

					case "ushort":
					{
						type = typeof(ushort);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr((ushort)p.Al());

						break;
					}

					case "char":
					{
						type = typeof(sbyte);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr((sbyte)p.Al());

						break;
					}

					case "uchar":
					{
						type = typeof(byte);

						if (p is IntPtr ip)
							args[n] = ip;
						else
							args[n] = new IntPtr((byte)p.Al());

						break;
					}

					case "float":
					{
						type = typeof(float);
						var f = p.Af();
						var iref = (int*)&f;
						args[n] = new IntPtr(*iref);
						break;
					}

					case "double":
					{
						type = typeof(double);
						var d = p.Ad();
						var lref = (long*)&d;
						args[n] = new IntPtr(*lref);
						break;
					}

					case "uptr":
					case "ptr":
					{
						type = typeof(IntPtr);

						if (p is IntPtr ip)
							args[n] = ip;
						else if (p is int || p is long || p is uint)
							args[n] = new IntPtr((long)Convert.ChangeType(p, typeof(long)));
						else if (p is Buffer buf)
							args[n] = buf.Ptr;
						else if (p is Keysharp.Core.Array array)
							SetupPointerArg(i, n, array.array);
						else if (p is ComObject co)
						{
							IntPtr pUnk;

							if (co.Ptr is IntPtr ip2)
								pUnk = ip2;
							else
								pUnk = Marshal.GetIUnknownForObject(co.Ptr);//Subsequent calls like DllCall() and NumGet() will dereference to get entries in the vtable.

							args[n] = pUnk;
							_ = Marshal.Release(pUnk);
						}
						else if (Marshal.IsComObject(p))
						{
							var pUnk = Marshal.GetIUnknownForObject(p);
							args[n] = pUnk;
							_ = Marshal.Release(pUnk);
						}
						//else if (p is DelegateHolder delholder)
						//{
						//  args[n] = delholder.delRef;
						//}
						//else if (p is StringBuffer sb)
						//{
						//  args[n] = sb.sb;
						//  types[n] = typeof(StringBuilder);
						//}
						//else if (p is Delegate del)
						//{
						//  args[n] = del;
						//  types[n] = del.GetType();
						//}
						//else if (p is System.Array arr)
						//  //else if (p is ComObjArray arr)
						//{
						//  //args[n] = arr;
						//}
						else
							SetupPointerArg(i, n);//If it wasn't any of the above types, just take the address, which ends up being the same as int* etc...

						break;
					}

					default:
						throw new ValueError($"Arg or return type of {name} is invalid.");
				}

				TypeDetermined:

				if (isreturn)
				{
					returnType = type;
				}
			}
		}
	}
}
