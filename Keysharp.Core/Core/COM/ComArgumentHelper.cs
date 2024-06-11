#if WINDOWS
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Keysharp.Scripting;

namespace Keysharp.Core.COM
{
	internal class ComArgumentHelper : ArgumentHelper
	{
		internal nint[] args;
		internal HashSet<nint> bstrs;
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
			var len = parameters.Length / 2;
			args = new nint[len];
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
						type = typeof(nint);
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
							SetupPointerArg(i, n, Encoding.UTF8.GetBytes(s + char.MinValue));
						else
							throw new TypeError($"Argument had type {name} but was not a string.");

						break;
					}

					case "astr":
					{
						type = typeof(nint);

						if (p is string s)
							SetupPointerArg(i, n, Encoding.ASCII.GetBytes(s + char.MinValue));
						else
							throw new TypeError($"Argument had type {name} but was not a string.");

						break;
					}

					case "int64":
					{
						type = typeof(long);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint(p.Al());

						break;
					}

					case "uint64":
					{
						type = typeof(ulong);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint(p.Al());//No real way to make an unsigned long here.

						//args[n] = new UIntPtr(p.Al());//No real way to make an unsigned long here.
						break;
					}

					case "hresult":
					case "int":
					{
						type = typeof(int);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint(p.Ai());

						break;
					}

					case "uint":
					{
						type = typeof(uint);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint(p.Aui());

						break;
					}

					case "short":
					{
						type = typeof(short);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint((short)p.Al());

						break;
					}

					case "ushort":
					{
						type = typeof(ushort);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint((ushort)p.Al());

						break;
					}

					case "char":
					{
						type = typeof(sbyte);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint((sbyte)p.Al());

						break;
					}

					case "uchar":
					{
						type = typeof(byte);

						if (p is nint ip)
							args[n] = ip;
						else
							args[n] = new nint((byte)p.Al());

						break;
					}

					case "float":
					{
						type = typeof(float);
						var f = p.Af();
						var iref = (int*)&f;
						args[n] = new nint(*iref);
						break;
					}

					case "double":
					{
						type = typeof(double);
						var d = p.Ad();
						var lref = (long*)&d;
						args[n] = new nint(*lref);
						break;
					}

					case "uptr":
					case "ptr":
					{
						type = typeof(nint);

						if (p is nint ip)
							args[n] = ip;
						else if (p is int || p is long || p is uint)
							args[n] = new nint((long)Convert.ChangeType(p, typeof(long)));
						else if (p is Buffer buf)
							args[n] = buf.Ptr;
						else if (p is Array array)
							SetupPointerArg(i, n, array.array);
						else if (p is ComObject co)
						{
							nint pUnk;

							if (co.Ptr is nint ip2)
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
#endif