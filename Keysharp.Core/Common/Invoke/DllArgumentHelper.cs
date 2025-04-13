#if WINDOWS
namespace Keysharp.Core.Common.Invoke
{
    internal class DllArgumentHelper : ArgumentHelper
    {
        internal object[] args;
        internal string[] names;
        internal Type[] types;
        internal Dictionary<int, object> refs = new();

        internal DllArgumentHelper(object[] parameters)
            : base(parameters)
        {
        }

		protected override void ConvertParameters(object[] parameters)
		{
			void SetupPointerArg(int i, int n, object obj = null)
			{
				var p = parameters[i];
				var pm1 = parameters[i - 1].ToString();
				var gch = GCHandle.Alloc(obj ?? p, GCHandleType.Pinned);
				_ = gcHandles.Add(gch);
				var intptr = gch.AddrOfPinnedObject();
				//Numbers being passed in will always be of type long or double, however that won't work
				//when a DLL function expects a pointer to a smaller type. So advance the pointer by the appropriate amount so it
				//accesses the intended part.
				//if (p is long || p is ulong || p is double)
				//{
				//  var amt = 0;
				//  if (pm1.Contains("int", StringComparison.OrdinalIgnoreCase) || pm1.Contains("float", StringComparison.OrdinalIgnoreCase))
				//      amt = 4;
				//  else if (pm1.Contains("short", StringComparison.OrdinalIgnoreCase))
				//      amt = 6;
				//  else if (pm1.Contains("char", StringComparison.OrdinalIgnoreCase))
				//      amt = 7;
				//  intptr = IntPtr.Add(intptr, amt);
				//}
				args[n] = intptr;
			}
			Error err;
			types = new Type[parameters.Length / 2];
			args = new object[types.Length];
			names = new string[types.Length];
			hasreturn = (parameters.Length & 1) == 1;

            for (var i = 0; i < parameters.Length; i++)
            {
                var name0 = parameters[i].ToString().ToLowerInvariant().Trim(Spaces);
                var name = name0;
                var isreturn = hasreturn && i == parameters.Length - 1;

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

                Type type;
                var usePtr = false;

                switch (name[name.Length - 1])
                {
                    case '*':
                    case 'P':
                    case 'p':
                        usePtr = true;
                        name = name.Substring(0, name.Length - 1).Trim();
                        type = typeof(IntPtr);//Pointers must manually be handled. Setting ParameterAttributes.Out on the parameter doesn't seem to work.
                        goto TypeDetermined;
                }

                switch (name)
                {
                    case "astr":
                    case "wstr":
                    case "str": type = typeof(string); usePtr = true; break;

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

                    case "ptr":
                    case "uptr": type = typeof(IntPtr); break;

                    default:
                        _ = Errors.ErrorOccurred(err = new ValueError($"Arg or return type of {name} is invalid.")) ? throw err : "";
                        return;
                }

            TypeDetermined:
                i++;

                if (isreturn)
                {
                    returnType = name.EndsWith("str") ? typeof(IntPtr) : type;//Native functions that return strings should be treated as pointers which will be converted manually after the call is done.
                    returnName = name;
                }
                else if (!isreturn && i < parameters.Length)
                {
                    if (usePtr && parameters[i] is KeysharpObject kso && !(kso is ComObject))
                    {
                        refs[i] = kso;
                        parameters[i] = Script.GetPropertyValue(kso, "__Value");
                    }

                    var n = i / 2;
                    var p = parameters[i];
                    names[n] = name0;
                    types[n] = type;

                    try
                    {
                        if (type == typeof(IntPtr))
                        {
                            if (name == "ptr" || name == "uptr")
                            {
								if (p is KeysharpObject && Script.GetPropertyValue(p, "ptr", false) is object argPtr && argPtr != null)
									p = argPtr;

								if (p is null)
                                    args[n] = IntPtr.Zero;
                                else if (p is IntPtr)
                                {
                                    if (usePtr)
                                        SetupPointerArg(i, n);
                                    else
                                        args[n] = p;
                                }
                                else if (p is int || p is long || p is uint)
                                {
                                    if (usePtr)
                                        SetupPointerArg(i, n);
                                    else
                                        args[n] = new IntPtr((long)Convert.ChangeType(p, typeof(long)));
                                }
                                else if (p is Buffer buf)
                                {
                                    args[n] = buf.Ptr;
                                }
                                else if (p is DelegateHolder delholder)
                                {
                                    args[n] = delholder.delRef;
                                    types[n] = delholder.delRef.GetType();
                                }
                                else if (p is StringBuffer sb)
                                {
                                    sb.UpdateBufferFromEntangledString();
                                    args[n] = sb.sb;
                                    types[n] = typeof(StringBuilder);
                                }
                                else if (p is Delegate del)
                                {
                                    args[n] = del;
                                    types[n] = del.GetType();
                                }
                                else if (p is System.Array arr)
                                //else if (p is ComObjArray arr)
                                {
                                    args[n] = arr;
                                    types[n] = arr.GetType();
                                }
                                else if (p is ComObject co)
                                {
                                    if (co.Ptr is IntPtr ip || co.Ptr is long || co.Ptr is int)
                                    {
                                        if (usePtr)
                                            SetupPointerArg(i, n, co.Ptr);
                                        else
                                            args[n] = co.Ptr;
                                    }
                                    else if (Marshal.IsComObject(co.Ptr))
                                    {
                                        var pUnk = Marshal.GetIUnknownForObject(co.Ptr);//Subsequent calls like DllCall() and NumGet() will dereference to get entries in the vtable.
                                        args[n] = pUnk;
                                        _ = Marshal.Release(pUnk);
                                    }
                                    else
                                    {
                                        _ = Errors.ErrorOccurred(err = new TypeError($"COM object with ptr type {co.Ptr.GetType()} could not be converted into a DLL argument.")) ? throw err : "";
                                        return;
                                    }
                                }
                                else if (Marshal.IsComObject(p))
                                {
                                    var pUnk = Marshal.GetIUnknownForObject(p);
                                    args[n] = pUnk;
                                    _ = Marshal.Release(pUnk);
                                }
                                else if (p is Array array)
                                    SetupPointerArg(i, n, array.array);
                                else
                                    SetupPointerArg(i, n);//If it wasn't any of the above types, just take the address, which ends up being the same as int* etc...
                            }
                            else if (name == "uint" || name == "int")
                            {
                                if (usePtr)
                                    SetupPointerArg(i, n);
                                else if (p is IntPtr ip)
                                    args[n] = ip.ToInt64();
                                else if (p is long l && l > 0)
                                    args[n] = l;
                                else if (p is int ii && ii > 0)
                                    args[n] = ii;
                            }
                            else
                                SetupPointerArg(i, n);
                        }
                        else if (type == typeof(int))
                        {
                            if (p is null)
                                args[n] = 0;
                            else if (p is IntPtr ip)
                                args[n] = ip.ToInt32();
                            else
                                args[n] = (int)p.Al();
                        }
                        else if (type == typeof(uint))
                        {
                            if (p is null)
                                args[n] = 0u;
                            else if (p is IntPtr ip)
                                args[n] = (uint)ip.ToInt64();
                            else
                                args[n] = (uint)p.Al();
                        }
                        else
                            args[n] = Convert.ChangeType(p, type);
                    }
                    catch (Exception e)
                    {
                        _ = Errors.ErrorOccurred(err = new TypeError($"Argument type conversion failed: {e.Message}")) ? throw err : "";
                        return;
                    }
                }
            }
        }
    }
}

#endif