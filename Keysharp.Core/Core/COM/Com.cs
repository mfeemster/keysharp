#if WINDOWS
namespace Keysharp.Core.COM
{
	unsafe public static class Com
	{
		public const int variantTypeMask = 0xfff;
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
			var vt = (VarEnum)varType.Ai();//Need a switch statement on type.
			var dim1Size = count1.Ai();
			var lengths = new int[args != null ? args.Length + 1 : 1];
			var t = typeof(object);

			if (lengths.Length > 8)
				_ = Errors.ErrorOccurred(err = new Error($"COM array dimensions of {lengths.Length} is greater than the maximum allowed number of 8.")) ? throw err : "";

			lengths[0] = dim1Size;

			for (var i = 0; i < args.Length; i++)
				lengths[i + 1] = args[i].Ai();

			t = ConvertVarTypeToCLRType(vt);

			if (t == typeof(object)) //Some special handling for objects
			{
				switch (vt)
				{
					case VarEnum.VT_DISPATCH: t = typeof(DispatchWrapper); break;
					case VarEnum.VT_VARIANT: break;
					default:
						return Errors.ErrorOccurred(err = new ValueError($"The supplied COM type of {varType} is not supported.")) ? throw err : null;
				}
			}

			return new ComObjArray(System.Array.CreateInstance(t, lengths));
		}

		internal static object ConvertToCOMType(object ret)
		{
			if (ret is long ll && ll < int.MaxValue)
				ret = (int)ll;
			else if (ret is bool bl)
				ret = bl ? 1 : 0;
			return ret;
		}

		internal static Type ConvertVarTypeToCLRType(VarEnum vt) =>
			vt switch
			{
				VarEnum.VT_I1 => typeof(sbyte),
				VarEnum.VT_UI1 => typeof(byte),
				VarEnum.VT_I2 => typeof(short),
				VarEnum.VT_UI2 => typeof(ushort),
				VarEnum.VT_I4 or VarEnum.VT_INT => typeof(int),
				VarEnum.VT_UI4 or VarEnum.VT_UINT or VarEnum.VT_ERROR => typeof(uint),
				VarEnum.VT_I8 => typeof(long),
				VarEnum.VT_UI8 => typeof(ulong),
				VarEnum.VT_R4 => typeof(float),
				VarEnum.VT_R8 or VarEnum.VT_DATE => typeof(double), //should VT_DATE be converted to DateTime?
				VarEnum.VT_DECIMAL or VarEnum.VT_CY => typeof(decimal),
				VarEnum.VT_BOOL => typeof(bool),
				VarEnum.VT_BSTR => typeof(string),
				_ => typeof(object),
			};


		public static object ComObjConnect(object comObj, object prefixOrSink = null, object debug = null)
		{
			if (comObj is ComObject co)
			{
				if (co.vt != VarEnum.VT_DISPATCH && co.vt != VarEnum.VT_UNKNOWN)// || Marshal.GetIUnknownForObject(co.Ptr) == 0)
				{
					Error err;
					return Errors.ErrorOccurred(err = new ValueError($"COM object type of {co.vt} was not VT_DISPATCH or VT_UNKNOWN, and was not IUnknown.")) ? throw err : null;
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
						inst = (long)ptr;

					_ = Marshal.Release(iptr);
				}

				return new ComObject()
				{
					vt = id == IID_IDispatch ? VarEnum.VT_DISPATCH : VarEnum.VT_UNKNOWN,
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

			if (dispPtr is long l)
				dispPtr = Marshal.GetObjectForIUnknown(new nint(l));

			if (dispPtr is IDispatch id)
				return new ComObject(VarEnum.VT_DISPATCH, id);
			else if (Marshal.IsComObject(dispPtr))
				return new ComObject(VarEnum.VT_UNKNOWN, dispPtr);

			return Errors.ErrorOccurred(err = new TypeError($"Passed in value {dispPtr} of type {dispPtr.GetType()} was not of type IDispatch.")) ? throw err : null;
		}

		public static object ComObjGet(object name) => Marshal.BindToMoniker(name.As());

		public static object ComObjQuery(object comObj, object sidiid = null, object iid = null)
		{
			Error err;
			nint ptr;

			if (comObj is KeysharpObject kso && Script.GetPropertyValue(kso, "ptr", false) is object kptr && kptr != null)
				comObj = kptr;

			if (Marshal.IsComObject(comObj))
				ptr = Marshal.GetIUnknownForObject(comObj);
			else if (comObj is long l)
				ptr = new nint(l);
			else
				return Errors.ErrorOccurred(err = new ValueError($"The passed in object {comObj} of type {comObj.GetType()} was not a ComObject or a raw COM interface.")) ? throw err : null;

			nint resultPtr = 0;
			Guid id = Guid.Empty;
			int hr;

			if (sidiid != null && iid != null)
			{
				var sidstr = sidiid.As();
				var iidstr = iid.As();

				if (CLSIDFromString(sidstr, out var sid) >= 0 && CLSIDFromString(iidstr, out id) >= 0)
				{
					// Query for a service: use IServiceProvider::QueryService.
					IServiceProvider sp = (IServiceProvider)Marshal.GetObjectForIUnknown(ptr);
					hr = sp.QueryService(ref sid, ref id, out resultPtr);
				}
			}
			else if (sidiid != null)
			{
				var iidstr = sidiid.As();

				if (CLSIDFromString(iidstr, out id) >= 0)
				{
					hr = Marshal.QueryInterface(ptr, id, out resultPtr);
				}
			}

			if (resultPtr == 0)
				return Errors.ErrorOccurred(err = new Error($"Unable to get COM interface with arguments {sidiid}, {iid}.")) ? throw err : null;

			return new ComObject(id == IID_IDispatch ? VarEnum.VT_DISPATCH : VarEnum.VT_UNKNOWN, (long)resultPtr);
		}

		public static object ComObjType(object comObj, object infoType = null)
		{
			var s = infoType.As().ToLower();

			if (comObj is ComObject co)
			{
				ITypeInfo typeInfo = null;

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
					return (long)co.vt;
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
						var attr = Marshal.PtrToStructure<TYPEATTR>(typeAttr);
						var guid = attr.guid.ToString("B").ToUpper();
						typeInfo.ReleaseTypeAttr(typeAttr);
						return guid;
					}
				}
			}
			else if (Marshal.IsComObject(comObj))
			{
				if (comObj is IDispatch dispatch)
				{
					var ret = dispatch.GetTypeInfo(0, 0, out var typeInfo);

					if (typeInfo != null)
					{
						typeInfo.GetTypeAttr(out var pTypeAttr);
						var typeAttr = Marshal.PtrToStructure<TYPEATTR>(pTypeAttr);
						var vtType = typeAttr.tdescAlias.vt;
						typeInfo.ReleaseTypeAttr(pTypeAttr);
						return (long)vtType;
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
			nint unk = 0;

			if (ptr is ComObject co)
				ptr = co.Ptr;

			if (ptr is long l)
			{
				unk = new nint(l);
			}
			else
			{
				unk = Marshal.GetIUnknownForObject(ptr);
				_ = Marshal.AddRef(unk);
				return (long)Marshal.Release(unk);//GetIUnknownForObject already added 1.
			}

			return (long)Marshal.AddRef(unk);
		}

		public static object ObjRelease(object ptr)
		{
			var co = ptr as ComObject;

			if (co != null)
			{
				ptr = co.Ptr;

				if (Marshal.IsComObject(ptr))
				{
					ptr = Marshal.GetIUnknownForObject(ptr); // Make sure we decrease the COM object not RCW
					_ = Marshal.Release((nint)ptr);
				}
			}

			if (ptr is long l)
			{
				ptr = new nint(l);
			}
			else
			{
				Error err;
				return Errors.ErrorOccurred(err = new TypeError($"Argument of type {ptr.GetType()} was not a pointer or ComObject.")) ? throw err : false;
			}

			return (long)Marshal.Release((nint)ptr);
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

			nint pUnk = 0;

			if (comObj is KeysharpObject kso && Script.GetPropertyValue(comObj, "ptr", false) is object propPtr && propPtr != null)
				comObj = propPtr;

			if (Marshal.IsComObject(comObj))
			{
				pUnk = Marshal.GetIUnknownForObject(comObj);
				_ = Marshal.Release(pUnk);
			}
			else if (comObj is long l)
				pUnk = new nint(l);
			else
				return Errors.ErrorOccurred(err = new ValueError($"The passed in object was not a ComObject or a raw COM interface.")) ? throw err : null;

			var pVtbl = Marshal.ReadIntPtr(pUnk);
			var helper = new ArgumentHelper(parameters);
			var value = NativeInvoke(pUnk.ToInt64(), Marshal.ReadIntPtr(nint.Add(pVtbl, idx * sizeof(nint))), helper.args, helper.floatingTypeMask);
			Dll.FixParamTypesAndCopyBack(parameters, helper);
			helper.Dispose();

			// If the return type was omitted then it should be treated as HRESULT
			// and if that is a negative value then throw an OSError
			if (!helper.HasReturn)
			{
				long hrLong = (long)value;                // unbox the raw long
				int hr32 = unchecked((int)hrLong);   // keep only the low 32 bits

				if (hr32 < 0)
					throw new OSError(hr32);
			}

			//Special conversion for the return value.
			if (helper.ReturnType == typeof(int))
			{
				long l = (long)value;
				int ii = *(int*)&l;
				value = ii;
			}
			else if (helper.ReturnType == typeof(float))
			{
				double d = (double)value;
				float f = *(float*)&d;
				return f;
			}
			else if (helper.ReturnType == typeof(string))
			{
				var str = Marshal.PtrToStringUni((nint)((long)value));
				_ = Objects.ObjFree(value);//If this string came from us, it will be freed, else no action.
				return str;
			}

			return value;
		}

		internal static object NativeInvoke(long objPtr, nint vtbl, long[] args, ulong mask)
		{
			if (objPtr == 0L || vtbl == 0)
				throw new Error("Invalid object pointer or vtable number");

			object ret = 0L;
			//First attempt to call the normal way. This will succeed with any normal COM call.
			//However, it will throw an exception if we've passed a fake COM function using DelegateHolder.
			//This can be reproduced with the following Script.TheScript.
			/*
			    ReturnInt() => 123

			    ; Create dummy vtable without a defined AddRef, Release etc
			    vtbl := Buffer(4*A_PtrSize)
			    NumPut("ptr", CallbackCreate(ReturnInt), vtbl, 3*A_PtrSize)
			    ; Add the vtbl to our COM object
			    dummyCOM := Buffer(A_PtrSize, 0)
			    NumPut("ptr", vtbl.Ptr, dummyCOM)

			    MsgBox ComCall(3, dummyCOM.Ptr, "int")
			*/
			// This could potentially be optimized by compiling a specific delegate
			// for the ComCall scenario with the signature Func<nint, long, long[], long>
			long[] newArgs = new long[args.Length + 1];
			newArgs[0] = objPtr;
			System.Array.Copy(args, 0, newArgs, 1, args.Length);
			mask = mask << 1; // since we inserted an extra argument at the beginning
			ret = Dll.NativeInvoke(vtbl, newArgs, mask);
			// Copy back.
			System.Array.Copy(newArgs, 1, args, 0, args.Length);
			return ret;
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

			GetActiveObject(ref clsid, 0, out var obj);
			return obj;
		}

		[DllImport(WindowsAPI.oleaut, CharSet = CharSet.Unicode, PreserveSig = false)]
		private static extern void GetActiveObject(ref Guid rclsid, nint pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
	}
}
#endif