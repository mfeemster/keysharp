#if WINDOWS
namespace Keysharp.Core.COM
{
	internal class ComArgumentHelper : ArgumentHelper, IDisposable
	{
		internal static char[] pointerChars = [' ', '*', 'p', 'P'];
		internal long[] args;
		internal HashSet<nint> bstrs;
		internal bool isDisposed = false;
		internal ComArgumentHelper(object[] parameters)
		{
			ConvertParameters(parameters);
		}

		~ComArgumentHelper()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				if (bstrs != null)
				{
					foreach (var bstr in bstrs)
						Marshal.FreeBSTR(bstr);
				}
				if (gcHandles.Count > 0)
				{
					foreach (var gch in gcHandles)
						gch.Free();
				}
			}
			isDisposed = true;
		}

		protected unsafe override void ConvertParameters(object[] parameters)
		{
			void SetupPointerArg(int i, int n, object obj = null)
			{
				var p = parameters[i];
				var gch = GCHandle.Alloc(obj ?? p, GCHandleType.Pinned);
				gcHandles.Add(gch);
				var intptr = gch.AddrOfPinnedObject();
				args[n] = intptr;
			}
			Error err;
			var paramCount = parameters.Length;
			var len = paramCount / 2;
			args = new long[len];
			hasreturn = (paramCount & 1) == 1;
			var last = paramCount - 1;

			//Done slightly differently than in DllArgumentHelper.
			for (var i = 0; i < paramCount; i++)
			{
				var isreturn = hasreturn && i == last;
				var name = parameters[i].ToString().ToLower().Trim(Spaces);

				if (isreturn)
				{
					if (name.StartsWith(cdeclstr))
					{
						name = name.Substring(cdeclstr.Length).Trim();
						cdecl = true;
						returnName = name;

						if (name?.Length == 0)
							continue;
					}
				}
				else
					i++;

				Type type = null;
				var n = i / 2;
				var p = parameters[i];

				// This assumes that the ptr property contains a numeric value wrapped in the object type
				// or System.__ComObject. If the numeric value is not wrapped then using it
				// as an output variable will not work properly.
				if (p is KeysharpObject kso && Script.GetPropertyValue(kso, "ptr", false) is object kptr && kptr != null)
					p = parameters[i] = kptr;

				switch (name[name.Length - 1])
				{
					case '*':
					case 'P':
					case 'p':
						name = name.TrimEnd(pointerChars);
						type = typeof(nint);
						SetupPointerArg(i, n);
						goto TypeDetermined;
				}

				switch (name)
				{
					case "bstr":
					{
						type = typeof(string);

						if (!isreturn)
						{
							if (p is string s)
							{
								var bstr = Marshal.StringToBSTR(s);

								if (bstrs == null)
									bstrs = [];

								_ = bstrs.Add(bstr);
								args[n] = bstr;
							}
							else
							{
								_ = Errors.ErrorOccurred(err = new TypeError($"Argument had type {name} but was not a string.")) ? throw err : "";
								return;
							}
						}

						break;
					}

					case "wstr":
					case "str":
					{
						type = typeof(string);

						if (!isreturn)
						{
							if (p is string s)
								SetupPointerArg(i, n);
							else
							{
								_ = Errors.ErrorOccurred(err = new TypeError($"Argument had type {name} but was not a string.")) ? throw err : "";
								return;
							}
						}

						break;
					}

					case "astr":
					{
						type = typeof(nint);

						if (!isreturn)
						{
							if (p is string s)
								SetupPointerArg(i, n, Encoding.ASCII.GetBytes(s));
							else
							{
								_ = Errors.ErrorOccurred(err = new TypeError($"Argument had type {name} but was not a string.")) ? throw err : "";
								return;
							}
						}

						break;
					}

					case "int64":
					{
						type = typeof(long);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint(p.Al());
						}

						break;
					}

					case "uint64":
					{
						type = typeof(ulong);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint(p.Al());//No real way to make an unsigned long here.
						}

						break;
					}

					case "hresult":
					case "int":
					{
						type = typeof(int);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint(p.Ai());
						}

						break;
					}

					case "uint":
					{
						type = typeof(uint);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint(p.Aui());
						}

						break;
					}

					case "short":
					{
						type = typeof(short);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint((short)p.Al());
						}

						break;
					}

					case "ushort":
					{
						type = typeof(ushort);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint((ushort)p.Al());
						}

						break;
					}

					case "char":
					{
						type = typeof(sbyte);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint((sbyte)p.Al());
						}

						break;
					}

					case "uchar":
					{
						type = typeof(byte);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else
								args[n] = new nint((byte)p.Al());
						}

						break;
					}

					case "float":
					{
						type = typeof(float);

						if (!isreturn)
						{
							var f = p.Af();
							var iref = (int*)&f;
							args[n] = new nint(*iref);
						}

						break;
					}

					case "double":
					{
						type = typeof(double);

						if (!isreturn)
						{
							var d = p.Ad();
							var lref = (long*)&d;
							args[n] = new nint(*lref);
						}

						break;
					}

					case "uptr":
					case "ptr":
					{
						type = typeof(nint);

						if (!isreturn)
						{
							if (p is nint ip)
								args[n] = ip;
							else if (p is long)//p is int || p is long || p is uint)
								args[n] = new nint((long)Convert.ChangeType(p, typeof(long)));
							else if (p is Buffer buf)
								args[n] = buf.Ptr;
							else if (p is Array array)
								SetupPointerArg(i, n, array.array);
							else if (Marshal.IsComObject(p))
							{
								var pUnk = Marshal.GetIUnknownForObject(p);
								args[n] = pUnk;
								_ = Marshal.Release(pUnk);
							}
							else if (p is DelegateHolder delholder)
							{
								args[n] = Marshal.GetFunctionPointerForDelegate(delholder.DelRef);
							}
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
						}

						break;
					}

					default:
					{
						_ = Errors.ErrorOccurred(err = new ValueError($"Arg or return type of {name} is invalid.")) ? throw err : "";
						return;
					}
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