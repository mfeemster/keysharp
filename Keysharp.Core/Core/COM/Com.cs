#if WINDOWS
namespace Keysharp.Core.COM
{
	unsafe public static class Com
	{
		public const int vt_empty = 0; //No value
		public const int vt_null = 1; //SQL-style Null
		public const int vt_i2 = 2; //16-bit signed int
		public const int vt_i4 = 3; //32-bit signed int
		public const int vt_r4 = 4; //32-bit floating-point number
		public const int vt_r8 = 5; //64-bit floating-point number
		public const int vt_cy = 6; //Currency
		public const int vt_date = 7; //Date
		public const int vt_bstr = 8; //COM string (Unicode string with length prefix)
		public const int vt_dispatch = 9; //COM object
		public const int vt_error = 0xA; //Error code(32-bit integer)
		public const int vt_bool = 0xB; //Boolean True(-1) or False(0)
		public const int vt_variant = 0xC; //VARIANT(must be combined with VT_ARRAY or VT_BYREF)
		public const int vt_unknown = 0xD; //IUnknown interface pointer
		public const int vt_decimal = 0xE; //(not supported)
		public const int vt_i1 = 0x10; //8-bit signed int
		public const int vt_ui1 = 0x11; //8-bit unsigned int
		public const int vt_ui2 = 0x12; //16-bit unsigned int
		public const int vt_ui4 = 0x13; //32-bit unsigned int
		public const int vt_i8 = 0x14; //64-bit signed int
		public const int vt_ui8 = 0x15; //64-bit unsigned int
		public const int vt_int = 0x16; //Signed machine int
		public const int vt_uint = 0x17; //Unsigned machine int
		public const int vt_record = 0x24; //User-defined type -- NOT SUPPORTED
		public const int vt_array = 0x2000; //SAFEARRAY
		public const int vt_byref = 0x4000; //Pointer to another type of value
		public const int vt_typemask = 0xfff;
		internal static Guid IID_IDispatch = new Guid(0x00020400, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
		internal static Guid IID_IServiceProvider = new Guid("6d5140c1-7436-11ce-8034-00aa006009fa");
		internal const int CLSCTX_INPROC_SERVER = 0x1;
		internal const int CLSCTX_INPROC_HANDLER = 0x2;
		internal const int CLSCTX_LOCAL_SERVER = 0x4;
		internal const int CLSCTX_INPROC_SERVER16 = 0x8;
		internal const int CLSCTX_REMOTE_SERVER = 0x10;
		internal const int CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER; //16;
		internal static HashSet<ComEvent> comEvents = new HashSet<ComEvent>();

		//private static Dictionary<int,

		[DllImport(WindowsAPI.ole32)]
		public static extern int CoCreateInstance(ref Guid clsid,
				[MarshalAs(UnmanagedType.IUnknown)] object inner,
				uint context,
				ref Guid uuid,
				[MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);

		public static object ComObjActive(object progId) => GetActiveObject(progId.As());

		public static object ComObjArray(object obj0, object obj1, params object[] args)
		{
			var varType = obj0.Ai();//Need a switch statement on type.
			var dim1Size = obj1.Ai();
			var lengths = new int[args != null ? args.Length + 1 : 1];
			var t = typeof(object);

			if (lengths.Length > 8)
				throw new KeysharpException($"COM array dimensions of {lengths.Length} is greater than the maximum allowed number of 8.");

			lengths[0] = dim1Size;

			for (var i = 0; i < args.Length; i++)
				lengths[i + 1] = args[i].Ai();

			switch (varType)
			{
				case vt_dispatch: t = typeof(System.Runtime.InteropServices.DispatchWrapper); break;

				//case VT_UNKNOWN: System.__ComObject or null if (punkVal == null)
				case vt_error: t = typeof(uint); break;

				case vt_bool: t = typeof(bool); break;

				case vt_i1: t = typeof(sbyte); break;

				case vt_ui1: t = typeof(byte); break;

				case vt_i2: t = typeof(short); break;

				case vt_ui2: t = typeof(ushort); break;

				case vt_i4: t = typeof(int); break;

				case vt_ui4: t = typeof(uint); break;

				case vt_i8: t = typeof(long); break;

				case vt_ui8: t = typeof(ulong); break;

				case vt_r4: t = typeof(float); break;

				case vt_r8: t = typeof(double); break;

				case vt_decimal: t = typeof(decimal); break;

				case vt_date: t = typeof(System.DateTime); break;

				case vt_bstr: t = typeof(string); break;

				case vt_int: t = typeof(int); break;

				case vt_uint: t = typeof(uint); break;

				case vt_cy: t = typeof(decimal); break;

				//case VT_RECORD:   Corresponding boxed value type.
				//case vt_variant: t = typeof(VARIANT); break;
				//case vt_variant: t = typeof(IntPtr); break;
				case vt_variant: t = typeof(object); break;

				default:
					throw new KeysharpException($"The supplied COM type of {varType} is not supported.");
			}

			return new ComObjArray(System.Array.CreateInstance(t, lengths));
		}

		public static void ComObjConnect(object obj0, object obj1 = null, object obj2 = null)
		{
			if (obj0 is ComObject co)
			{
				if (co.VarType != vt_dispatch && co.VarType != vt_unknown)// || Marshal.GetIUnknownForObject(co.Ptr) == IntPtr.Zero)
				{
					throw new ValueError($"COM object type of {co.VarType} was not VT_DISPATCH or VT_UNKNOWN, and was not IUnknown.");
				}

				//If it existed, whether obj1 was null or not, remove it.
				if (comEvents.FirstOrDefault(ce => ReferenceEquals(ce.dispatcher.Co, co)) is ComEvent ev)
				{
					_ = comEvents.Remove(ev);
					ev.Unwire();
					ev.dispatcher.Dispose();
				}

				if (obj1 != null)//obj1 not being null means add it.
					_ = comEvents.Add(new ComEvent(new Dispatcher(co), obj1, obj2 != null ? obj2.Ab() : false));
			}
		}

		public static object ComObject(object obj0, object obj1 = null)//progId, string iid)
		{
			var cls = obj0.As();
			var iidStr = obj1.As();
			var hr = 0;
			var clsId = Guid.Empty;
			var iid = Guid.Empty;

			while (true)
			{
				// It has been confirmed on Windows 10 that both CLSIDFromString and CLSIDFromProgID
				// were unable to resolve a ProgID starting with '{', like "{Foo", though "Foo}" works.
				// There are probably also guidelines and such that prohibit it.
				if (cls[0] == '{')
					hr = CLSIDFromString(cls, out clsId);
				else
					// CLSIDFromString is known to be able to resolve ProgIDs via the registry,
					// but fails on registration-free classes such as "Microsoft.Windows.ActCtx".
					// CLSIDFromProgID works for that, but fails when given a CLSID string
					// (consistent with VBScript and JScript in both cases).
					hr = CLSIDFromProgIDEx(cls, out clsId);

				if (hr < 0)
					break;

				if (iidStr.Length > 0)
				{
					hr = CLSIDFromString(iidStr, out iid);

					if (hr < 0)
						break;
				}
				else
					iid = IID_IDispatch;

				hr = CoCreateInstance(ref clsId, null, CLSCTX_SERVER, ref iid, out var inst);

				if (hr < 0)
					break;

				//If it was a specific interface, make sure we are pointing to that interface, otherwise the vtable
				//will be off in ComCall() and the program will crash.
				if (iid != Guid.Empty && iid != Dispatcher.IID_IDispatch)
				{
					var iptr = Marshal.GetIUnknownForObject(inst);

					if (Marshal.QueryInterface(iptr, ref iid, out var ptr) >= 0)
						inst = ptr;

					_ = Marshal.Release(iptr);
				}

				return new ComObject()
				{
					Ptr = inst,
					VarType = iid == IID_IDispatch ? vt_dispatch : vt_unknown
				};
			}

			return null;//Should be a call to ComError() here.//TODO
		}

		public static object ComObjFlags(object obj0, object obj1 = null, object obj2 = null)
		{
			if (obj0 is ComObject co)
			{
				var flags = obj1 != null ? obj1.Al() : 0L;
				var mask = obj2 != null ? obj2.Al() : 0L;

				if (obj1 == null && obj2 == null)
				{
					if (flags < 0)
					{
						flags = 0;
						mask = -flags;
					}
					else
						mask = flags;
				}

				co.Flags = (co.Flags & ~mask) | (flags & mask);
				return co.Flags;
			}

			return 0L;
		}

		public static ComObject ComObjFromPtr(object obj0)
		{
			if (obj0 is IntPtr ip)
				obj0 = Marshal.GetObjectForIUnknown(ip);
			else if (obj0 is long l)
				obj0 = Marshal.GetObjectForIUnknown(new IntPtr(l));

			if (obj0 is IDispatch id)
				return new ComObject(vt_dispatch, id);
			else if (Marshal.IsComObject(obj0))
				return new ComObject(vt_unknown, obj0);

			throw new ValueError($"Passed in value {obj0} of type {obj0.GetType()} was not of type IDispatch.");
		}

		public static object ComObjGet(object progId) => Marshal.BindToMoniker(progId.As());

		public static object ComObjQuery(object obj0, object obj1 = null, object obj2 = null)
		{
			object ptr;

			if (obj0 is ComObject co)
				ptr = co.Ptr;
			else if (Marshal.IsComObject(obj0))
				ptr = obj0;
			else if (obj0 is IntPtr ip)
				ptr = Marshal.GetObjectForIUnknown(ip);
			else if (obj0 is long l)
				ptr = Marshal.GetObjectForIUnknown(new IntPtr(l));
			else
				throw new ValueError($"The passed in object {obj0} of type {obj0.GetType()} was not a ComObject or a raw COM interface.");

			if (obj1 != null && obj2 != null)
			{
				var sidstr = obj1.As();
				var iidstr = obj2.As();

				if (CLSIDFromString(sidstr, out var sid) >= 0 && CLSIDFromString(iidstr, out var iid) >= 0)
				{
					if (ptr is IServiceProvider isp)
					{
						_ = isp.QueryService(ref sid, ref iid, out var ppv);

						if (ppv != IntPtr.Zero)
						{
							var ob = Marshal.GetObjectForIUnknown(ppv);
							return new ComObject(iid == IID_IDispatch ? vt_dispatch : vt_unknown, ob);
						}
					}
				}
			}
			else if (obj1 != null)
			{
				var iidstr = obj1.As();

				if (CLSIDFromString(iidstr, out var iid) >= 0)
				{
					var iptr = Marshal.GetIUnknownForObject(ptr);

					if (Marshal.QueryInterface(iptr, ref iid, out var ppv) >= 0)
					{
						_ = Marshal.Release(iptr);

						if (ppv != IntPtr.Zero)
						{
							var ob = Marshal.GetObjectForIUnknown(ppv);
							return new ComObject(iid == IID_IDispatch ? vt_dispatch : vt_unknown, ob);
						}
					}

					_ = Marshal.Release(iptr);
				}
			}

			throw new Error($"Unable to get COM interface with arguments {obj1}, {obj2}.");
		}

		public static object ComObjType(object obj, object name = null)
		{
			var s = name.As().ToLower();

			if (obj is ComObject co)
			{
				System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo = null;

				if (s.Length == 0)
				{
					//if (obj is System.Runtime.InteropServices.ComTypes.IUnknown)
					//{
					//}
					//_ = idisp.GetTypeInfo(0, 0, out typeInfo);
					//typeInfo.GetTypeAttr(out var typeAttr);
					//System.Runtime.InteropServices.ComTypes.TYPEATTR attr = (System.Runtime.InteropServices.ComTypes.TYPEATTR)Marshal.PtrToStructure(typeAttr, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));
					//var vt = (long)attr.tdescAlias.vt;
					//typeInfo.ReleaseTypeAttr(typeAttr);
					//return vt;
					return (long)co.VarType;
				}

				if (s.StartsWith('c'))
				{
					if (co.Ptr is IProvideClassInfo ipci)
						_ = ipci.GetClassInfo(out typeInfo);

					if (s == "class")
						s = "name";
					else if (s == "clsid")
						s = "iid";
				}
				else if (co.Ptr is IDispatch idisp)
					_ = idisp.GetTypeInfo(0, 0, out typeInfo);

				if (typeInfo != null)
				{
					if (s == "name")
					{
						typeInfo.GetDocumentation(-1, out var typeName, out var documentation, out var helpContext, out var helpFile);
						return typeName;
					}
					else if (s == "iid")
					{
						typeInfo.GetTypeAttr(out var typeAttr);
						System.Runtime.InteropServices.ComTypes.TYPEATTR attr = (System.Runtime.InteropServices.ComTypes.TYPEATTR)Marshal.PtrToStructure(typeAttr, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));
						var guid = attr.guid.ToString("B").ToUpper();
						typeInfo.ReleaseTypeAttr(typeAttr);
						return guid;
					}
				}
			}

			return null;
		}

		public static object ComObjValue(object obj0)
		{
			if (obj0 is ComObject co)
			{
				return co.Ptr;
			}
			else if (obj0 is IntPtr ip)
			{
				return ip.ToInt64();
			}
			else//Unsure if this logic even makes sense.
			{
				var gch = GCHandle.Alloc(obj0, GCHandleType.Pinned);
				var val = gch.AddrOfPinnedObject();
				gch.Free();
				return val;
			}
		}

		public static ComObject ComValue(object obj0, object obj1, object obj2 = null) => new ComObject(obj0, obj1, obj2);

		public static object ObjAddRef(object obj0)
		{
			var unk = IntPtr.Zero;

			if (obj0 is ComObject co)
				obj0 = co.Ptr;
			else if (obj0 is IntPtr ip)
				obj0 = ip;

			if (obj0 is IntPtr ip2)
			{
				unk = ip2;
			}
			else
			{
				unk = Marshal.GetIUnknownForObject(obj0);
				_ = Marshal.Release(unk);//Need this or else it will add 2.
			}

			return (long)Marshal.AddRef(unk);
		}

		public static object ObjRelease(object obj0)
		{
			if (obj0 is ComObject co)
				obj0 = co.Ptr;
			else if (obj0 is IntPtr ip)
				obj0 = ip;

			if (obj0 is IntPtr ip2)
				obj0 = Marshal.GetObjectForIUnknown(ip2);

			return (long)Marshal.ReleaseComObject(obj0);
		}

		/// <summary>
		/// Gotten loosely from https://social.msdn.microsoft.com/Forums/vstudio/en-US/cbb92470-979c-4d9e-9555-f4de7befb42e/how-to-directly-access-the-virtual-method-table-of-a-com-interface-pointer?forum=csharpgeneral
		/// </summary>
		public static object ComCall(object obj0, object obj1, params object[] parameters)
		{
			var index = obj0.Ai();
			var indexPlus1 = index + 1;//Index is zero based, so add 1.

			if (index < 0)
				throw new ValueError($"Index value of {index} was less than zero.");

			object ptr = null;
			var pUnk = IntPtr.Zero;

			if (obj1 is ComObject co)
				ptr = co.Ptr;
			else if (Marshal.IsComObject(obj1))
				ptr = obj1;
			else if (obj1 is IntPtr ip)
				ptr = ip;
			else
				throw new ValueError($"The passed in object was not a ComObject or a raw COM interface.");

			if (ptr is IntPtr ip2)
				pUnk = ip2;
			else
			{
				pUnk = Marshal.GetIUnknownForObject(ptr);
				_ = Marshal.Release(pUnk);
			}

			var pVtbl = Marshal.ReadIntPtr(pUnk);
			var helper = new ComArgumentHelper(parameters);
			var ret = CallDel(pUnk, Marshal.ReadIntPtr(IntPtr.Add(pVtbl, index * sizeof(IntPtr))), helper.args);

			for (int p = 0, a = 0; p < parameters.Length; p += 2, a++)
			{
				var ps = parameters[p].ToString().ToLower();

				if (ps == "float*")
				{
					var fptr = (float*)helper.args[a].ToPointer();
					var f = *fptr;
					var d = (double)f;
					parameters[p + 1] = d;
				}
				else if (ps == "double*")
				{
					var dptr = (double*)helper.args[a].ToPointer();
					parameters[p + 1] = *dptr;
				}
				else if (ps == "int*")
				{
					var pp = (int*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
				else if (ps == "uint*")
				{
					var pp = (uint*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
				else if (ps == "short*")
				{
					var pp = (short*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
				else if (ps == "ushort*")
				{
					var pp = (ushort*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
				else if (ps == "char*")
				{
					var pp = (sbyte*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
				else if (ps == "uchar*")
				{
					var pp = (byte*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
				else if (ps.EndsWith('*') || ps.EndsWith("p"))
				{
					var pp = (long*)helper.args[a].ToPointer();
					parameters[p + 1] = *pp;
				}
			}

			return ret.ToInt64();
		}

		internal static IntPtr CallDel(IntPtr objPtr, IntPtr vtbl, IntPtr[] args)
		{
			switch (args.Length)
			{
				case 0:
					var del0 = (Del0)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del0));
					return del0(objPtr);

				case 1:
					var del1 = (Del1)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del1));
					return del1(objPtr, args[0]);

				case 2:
					var del2 = (Del2)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del2));
					return del2(objPtr, args[0], args[1]);

				case 3:
					var del3 = (Del3)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del3));
					return del3(objPtr, args[0], args[1], args[2]);

				case 4:
					var del4 = (Del4)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del4));
					return del4(objPtr, args[0], args[1], args[2], args[3]);

				case 5:
					var del5 = (Del5)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del5));
					return del5(objPtr, args[0], args[1], args[2], args[3], args[4]);

				case 6:
					var del6 = (Del6)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del6));
					return del6(objPtr, args[0], args[1], args[2], args[3], args[4], args[5]);

				case 7:
					var del7 = (Del7)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del7));
					return del7(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

				case 8:
					var del8 = (Del8)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del8));
					return del8(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

				case 9:
					var del9 = (Del9)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del9));
					return del9(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);

				case 10:
					var del10 = (Del10)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del10));
					return del10(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);

				case 11:
					var del11 = (Del11)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del11));
					return del11(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);

				case 12:
					var del12 = (Del12)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del12));
					return del12(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);

				case 13:
					var del13 = (Del13)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del13));
					return del13(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);

				case 14:
					var del14 = (Del14)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del14));
					return del14(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);

				case 15:
					var del15 = (Del15)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del15));
					return del15(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);

				case 16:
					var del16 = (Del16)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del16));
					return del16(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);

				case 17:
					var del17 = (Del17)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del17));
					return del17(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15], args[16]);

				case 18:
					var del18 = (Del18)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del18));
					return del18(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15], args[16], args[17]);
			}

			return IntPtr.Zero;
		}


		[DllImport(WindowsAPI.oleaut)]
		internal static extern int VariantChangeTypeEx([MarshalAs(UnmanagedType.Struct)] out object pvargDest,
				[In, MarshalAs(UnmanagedType.Struct)] ref object pvarSrc, int lcid, short wFlags, [MarshalAs(UnmanagedType.I2)] short vt);

		[DllImport(WindowsAPI.ole32, CharSet = CharSet.Unicode)]
		private static extern int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid clsid);

		[DllImport(WindowsAPI.ole32, CharSet = CharSet.Unicode)]
		private static extern int CLSIDFromString(string lpsz, out Guid guid);

		/// <summary>
		/// This used to be a built in function in earlier versions of .NET but now needs to be added manually.
		/// Gotten from: https://stackoverflow.com/questions/64823199/is-there-a-substitue-for-system-runtime-interopservices-marshal-getactiveobject
		/// </summary>
		/// <param name="progId"></param>
		/// <param name="throwOnError"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		private static object GetActiveObject(string progId)
		{
			if (!Guid.TryParse(progId, out var clsid))
				_ = CLSIDFromProgIDEx(progId, out clsid);

			GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
			return obj;
		}

		[DllImport(WindowsAPI.oleaut, PreserveSig = false)]
		private static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
	}
}
#endif