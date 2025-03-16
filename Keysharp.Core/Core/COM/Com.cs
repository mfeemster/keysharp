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
		public const int vt_void = 0x18;
		public const int vt_hresult = 0x19;
		public const int vt_ptr = 0x001A;
		public const int vt_record = 0x24; //User-defined type -- NOT SUPPORTED
		public const int vt_array = 0x2000; //SAFEARRAY
		public const int vt_byref = 0x4000; //Pointer to another type of value
		public const int vt_typemask = 0xfff;
		internal static Guid IID_IDispatch = new (0x00020400, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
		internal static Guid IID_IServiceProvider = new ("6d5140c1-7436-11ce-8034-00aa006009fa");
		internal const int CLSCTX_INPROC_SERVER = 0x1;
		internal const int CLSCTX_INPROC_HANDLER = 0x2;
		internal const int CLSCTX_LOCAL_SERVER = 0x4;
		internal const int CLSCTX_INPROC_SERVER16 = 0x8;
		internal const int CLSCTX_REMOTE_SERVER = 0x10;
		internal const int CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER; //16;
		internal const int LOCALE_SYSTEM_DEFAULT = 0x800;
		internal static HashSet<ComEvent> comEvents = [];

		//private static Dictionary<int,

		[DllImport(WindowsAPI.ole32, CharSet = CharSet.Unicode)]
		public static extern int CoCreateInstance(ref Guid clsid,
				[MarshalAs(UnmanagedType.IUnknown)] object inner,
				uint context,
				ref Guid uuid,
				[MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);

		public static object ComObjActive(object clsid) => GetActiveObject(clsid.As());

		public static object ComObjArray(object varType, object count1, params object[] args)
		{
			Error err;
			var vt = varType.Ai();//Need a switch statement on type.
			var dim1Size = count1.Ai();
			var lengths = new int[args != null ? args.Length + 1 : 1];
			var t = typeof(object);

			if (lengths.Length > 8)
				_ = Errors.ErrorOccurred(err = new Error($"COM array dimensions of {lengths.Length} is greater than the maximum allowed number of 8.")) ? throw err : "";

			lengths[0] = dim1Size;

			for (var i = 0; i < args.Length; i++)
				lengths[i + 1] = args[i].Ai();

			switch (vt)
			{
				case vt_dispatch: t = typeof(DispatchWrapper); break;

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

				case vt_date: t = typeof(DateTime); break;

				case vt_bstr: t = typeof(string); break;

				case vt_int: t = typeof(int); break;

				case vt_uint: t = typeof(uint); break;

				case vt_cy: t = typeof(decimal); break;

				//case VT_RECORD:   Corresponding boxed value type.
				//case vt_variant: t = typeof(VARIANT); break;
				//case vt_variant: t = typeof(IntPtr); break;
				case vt_variant: t = typeof(object); break;

				default:
					return Errors.ErrorOccurred(err = new ValueError($"The supplied COM type of {varType} is not supported.")) ? throw err : null;
			}

			return new ComObjArray(System.Array.CreateInstance(t, lengths));
		}

		public static object ComObjConnect(object comObj, object prefixOrSink = null, object debug = null)
		{
			if (comObj is ComObject co)
			{
				if (co.VarType != vt_dispatch && co.VarType != vt_unknown)// || Marshal.GetIUnknownForObject(co.Ptr) == IntPtr.Zero)
				{
					Error err;
					return Errors.ErrorOccurred(err = new ValueError($"COM object type of {co.VarType} was not VT_DISPATCH or VT_UNKNOWN, and was not IUnknown.")) ? throw err : null;
				}

				//If it existed, whether obj1 was null or not, remove it.
				if (comEvents.FirstOrDefault(ce => ReferenceEquals(ce.dispatcher.Co, co)) is ComEvent ev)
				{
					_ = comEvents.Remove(ev);
					ev.Unwire();
					ev.dispatcher.Dispose();
				}

				if (prefixOrSink != null)//obj1 not being null means add it.
					_ = comEvents.Add(new ComEvent(new Dispatcher(co), prefixOrSink, debug != null ? debug.Ab() : false));
			}

			return null;
		}

		public static object ComObject(object clsid, object iid = null)//progId, string iid)
		{
			var cls = clsid.As();
			var iidStr = iid.As();
			var hr = 0;
			var clsId = Guid.Empty;
			var id = Guid.Empty;

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
					hr = CLSIDFromString(iidStr, out id);

					if (hr < 0)
						break;
				}
				else
					id = IID_IDispatch;

				hr = CoCreateInstance(ref clsId, null, CLSCTX_SERVER, ref id, out var inst);

				if (hr < 0)
					break;

				//If it was a specific interface, make sure we are pointing to that interface, otherwise the vtable
				//will be off in ComCall() and the program will crash.
				if (id != Guid.Empty && id != Dispatcher.IID_IDispatch)
				{
					var iptr = Marshal.GetIUnknownForObject(inst);

					if (Marshal.QueryInterface(iptr, in id, out var ptr) >= 0)
						inst = ptr;

					_ = Marshal.Release(iptr);
				}

				return new ComObject()
				{
					VarType = id == IID_IDispatch ? vt_dispatch : vt_unknown,
					Ptr = inst
				};
			}

			return null;//Should be a call to ComError() here.//TODO
		}

		public static object ComObjFlags(object comObj, object newFlags = null, object mask = null)
		{
			if (comObj is ComObject co)
			{
				var flags = newFlags != null ? newFlags.Al() : 0L;
				var m = mask != null ? mask.Al() : 0L;

				if (newFlags == null && mask == null)
				{
					if (flags < 0)
					{
						flags = 0;
						m = -flags;
					}
					else
						m = flags;
				}

				co.Flags = (co.Flags & ~m) | (flags & m);
				return co.Flags;
			}

			return 0L;
		}

		public static ComObject ComObjFromPtr(object dispPtr)
		{
			Error err;

			if (dispPtr is IntPtr ip)
				dispPtr = Marshal.GetObjectForIUnknown(ip);
			else if (dispPtr is long l)
				dispPtr = Marshal.GetObjectForIUnknown(new IntPtr(l));

			if (dispPtr is IDispatch id)
				return new ComObject(vt_dispatch, id);
			else if (Marshal.IsComObject(dispPtr))
				return new ComObject(vt_unknown, dispPtr);

			return Errors.ErrorOccurred(err = new TypeError($"Passed in value {dispPtr} of type {dispPtr.GetType()} was not of type IDispatch.")) ? throw err : null;
		}

		public static object ComObjGet(object name) => Marshal.BindToMoniker(name.As());

		public static object ComObjQuery(object comObj, object sidiid = null, object iid = null)
		{
			Error err;
			object ptr;

			if (comObj is ComObject co)
				ptr = co.Ptr;
			else if (Marshal.IsComObject(comObj))
				ptr = comObj;
			else if (comObj is IntPtr ip)
				ptr = Marshal.GetObjectForIUnknown(ip);
			else if (comObj is long l)
				ptr = Marshal.GetObjectForIUnknown(new IntPtr(l));
			else
				return Errors.ErrorOccurred(err = new ValueError($"The passed in object {comObj} of type {comObj.GetType()} was not a ComObject or a raw COM interface.")) ? throw err : null;

			if (sidiid != null && iid != null)
			{
				var sidstr = sidiid.As();
				var iidstr = iid.As();

				if (CLSIDFromString(sidstr, out var sid) >= 0 && CLSIDFromString(iidstr, out var id) >= 0)
				{
					if (ptr is IServiceProvider isp)
					{
						_ = isp.QueryService(ref sid, ref id, out var ppv);

						if (ppv != IntPtr.Zero)
						{
							var ob = Marshal.GetObjectForIUnknown(ppv);
							return new ComObject(id == IID_IDispatch ? vt_dispatch : vt_unknown, ob);
						}
					}
				}
			}
			else if (sidiid != null)
			{
				var iidstr = sidiid.As();

				if (CLSIDFromString(iidstr, out var id) >= 0)
				{
					var iptr = Marshal.GetIUnknownForObject(ptr);

					if (Marshal.QueryInterface(iptr, in id, out var ppv) >= 0)
					{
						_ = Marshal.Release(iptr);

						if (ppv != IntPtr.Zero)
						{
							var ob = Marshal.GetObjectForIUnknown(ppv);
							return new ComObject(id == IID_IDispatch ? vt_dispatch : vt_unknown, ob);
						}
					}

					_ = Marshal.Release(iptr);
				}
			}

			return Errors.ErrorOccurred(err = new Error($"Unable to get COM interface with arguments {sidiid}, {iid}.")) ? throw err : null;
		}

		public static object ComObjType(object comObj, object infoType = null)
		{
			var s = infoType.As().ToLower();

			if (comObj is ComObject co)
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

		public static object ComObjValue(object comObj)
		{
			if (comObj is ComObject co)
			{
				return co.Ptr;
			}
			else if (comObj is IntPtr ip)
			{
				return ip.ToInt64();
			}
			else//Unsure if this logic even makes sense.
			{
				var gch = GCHandle.Alloc(comObj, GCHandleType.Pinned);
				var val = gch.AddrOfPinnedObject();
				gch.Free();
				return val;
			}
		}

		public static ComObject ComValue(object varType, object value, object flags = null) => new (varType, value, flags);

		public static object ObjAddRef(object ptr)
		{
			var unk = IntPtr.Zero;

			if (ptr is ComObject co)
				ptr = co.Ptr;
			else if (ptr is IntPtr ip)
				ptr = ip;
			else if (ptr is long l)
				ptr = new IntPtr(l);

			if (ptr is IntPtr ip2)
			{
				unk = ip2;
			}
			else
			{
				unk = Marshal.GetIUnknownForObject(ptr);
				_ = Marshal.Release(unk);//Need this or else it will add 2.
			}

			return (long)Marshal.AddRef(unk);
		}

		public static object ObjRelease(object ptr)
		{
			if (ptr is ComObject co)
				ptr = co.Ptr;
			else if (ptr is IntPtr ip)
				ptr = ip;
			else if (ptr is long l)
				ptr = new IntPtr(l);

			if (ptr is IntPtr ip2)
				ptr = Marshal.GetObjectForIUnknown(ip2);

			return (long)Marshal.ReleaseComObject(ptr);
		}

		/// <summary>
		/// Gotten loosely from https://social.msdn.microsoft.com/Forums/vstudio/en-US/cbb92470-979c-4d9e-9555-f4de7befb42e/how-to-directly-access-the-virtual-method-table-of-a-com-interface-pointer?forum=csharpgeneral
		/// </summary>
		public static object ComCall(object index, object comObj, params object[] parameters)
		{
			Error err;
			var idx = index.Ai();
			var indexPlus1 = idx + 1;//Index is zero based, so add 1.

			if (idx < 0)
				return Errors.ErrorOccurred(err = new ValueError($"Index value of {idx} was less than zero.")) ? throw err : null;

			object ptr = null;
			var pUnk = IntPtr.Zero;

			if (comObj is ComObject co)
				ptr = co.Ptr;
			else if (Marshal.IsComObject(comObj))
				ptr = comObj;
			else if (comObj is IntPtr ip)
				ptr = ip;
			else if (comObj is long l)
				ptr = new IntPtr(l);
			else
				return Errors.ErrorOccurred(err = new ValueError($"The passed in object was not a ComObject or a raw COM interface.")) ? throw err : null;

			if (ptr is IntPtr ip2)
				pUnk = ip2;
			else
			{
				pUnk = Marshal.GetIUnknownForObject(ptr);
				_ = Marshal.Release(pUnk);
			}

			var pVtbl = Marshal.ReadIntPtr(pUnk);
			var helper = new ComArgumentHelper(parameters);
			var ret = CallDel(pUnk, Marshal.ReadIntPtr(IntPtr.Add(pVtbl, idx * sizeof(IntPtr))), helper.args);

			for (int pi = 0, ai = 0; pi < parameters.Length; pi += 2, ++ai)
			{
				if (pi < parameters.Length - 1)
				{
					var p0 = parameters[pi];

					//If they passed in a ComObject with Ptr as an address, make that address into a __ComObject.
					/*  if (parameters[pi + 1] is ComObject co2)
					    {
					    object obj = co2.Ptr;
					    co2.Ptr = obj;//Reassign to ensure pointers are properly cast to __ComObject.
					    }

					    else*/
					if (p0 is string ps)
					{
						var aip = helper.args[ai];

						if (ps[ ^ 1] == '*' || ps[ ^ 1] == 'p')
							Dll.FixParamTypeAndCopyBack(ref parameters[pi + 1], ps, aip);//Must reference directly into the array, not a temp variable.
					}
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
					var del1 = (Del01)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del01));
					return del1(objPtr, args[0]);

				case 2:
					var del2 = (Del02)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del02));
					return del2(objPtr, args[0], args[1]);

				case 3:
					var del3 = (Del03)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del03));
					return del3(objPtr, args[0], args[1], args[2]);

				case 4:
					var del4 = (Del04)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del04));
					return del4(objPtr, args[0], args[1], args[2], args[3]);

				case 5:
					var del5 = (Del05)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del05));
					return del5(objPtr, args[0], args[1], args[2], args[3], args[4]);

				case 6:
					var del6 = (Del06)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del06));
					return del6(objPtr, args[0], args[1], args[2], args[3], args[4], args[5]);

				case 7:
					var del7 = (Del07)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del07));
					return del7(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

				case 8:
					var del8 = (Del08)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del08));
					return del8(objPtr, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);

				case 9:
					var del9 = (Del09)Marshal.GetDelegateForFunctionPointer(vtbl, typeof(Del09));
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


		[DllImport(WindowsAPI.oleaut, CharSet = CharSet.Unicode)]
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

		[DllImport(WindowsAPI.oleaut, CharSet = CharSet.Unicode, PreserveSig = false)]
		private static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
	}
}
#endif