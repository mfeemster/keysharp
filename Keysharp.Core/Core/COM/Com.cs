using Keysharp.Core.Windows;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Keysharp.Core.COM
{
	public static class Com
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
				if ((co.VarType != vt_dispatch && co.VarType != vt_unknown))// || Marshal.GetIUnknownForObject(co.Ptr) == IntPtr.Zero)
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
			if (obj0 is IDispatch id)
			{
				//AHK did something here with trying to query an interface, but I hope that the above IDispatch cast does the same thing.
				return new ComObject(vt_dispatch, id);
			}

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
			else
				throw new ValueError($"The passed in object was not a ComObject or a raw COM interface.");

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
					return co.VarType;
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
			if (obj0 is ComObject co)
				obj0 = co.Ptr;

			var unk = Marshal.GetIUnknownForObject(obj0);
			_ = Marshal.Release(unk);//Need this or else it will add 2.
			return (long)Marshal.AddRef(unk);
		}

		public static object ObjRelease(object obj0)
		{
			if (obj0 is ComObject co)
				obj0 = co.Ptr;

			return (long)Marshal.ReleaseComObject(obj0);
		}

		//[System.Security.SecurityCritical]  // auto-generated_required
		//[ResourceExposure(ResourceScope.None)]
		//[MethodImplAttribute(MethodImplOptions.InternalCall)]
		//public static extern MemberInfo GetMethodInfoForComSlot(Type t, int slot, ref ComMemberType memberType);

		/// <summary>
		/// Gotten loosely from https://social.msdn.microsoft.com/Forums/vstudio/en-US/cbb92470-979c-4d9e-9555-f4de7befb42e/how-to-directly-access-the-virtual-method-table-of-a-com-interface-pointer?forum=csharpgeneral
		/// </summary>
		//public static object ComCall(object obj0, object obj1, params object[] parameters)
		//{
		//  var index = obj0.Ai();
		//  var indexPlus1 = index + 1;

		//  if (index < 0)
		//      throw new ValueError($"Index value of {index} was less than zero.");

		//  object ptr;

		//  if (obj1 is ComObject co)
		//      ptr = co.Ptr;
		//  else if (Marshal.IsComObject(obj1))
		//      ptr = obj1;
		//  else
		//      throw new ValueError($"The passed in object was not a ComObject or a raw COM interface.");

		//  var pUnk = Marshal.GetIUnknownForObject(ptr);
		//  var pVtbl = Marshal.ReadIntPtr(pUnk);
		//  var vtbl = new IntPtr[indexPlus1];//Index is zero based.
		//  Marshal.Copy(pVtbl, vtbl, 0, indexPlus1);
		//  var helper = new DllArgumentHelper(parameters);
		//  var memberType = ComMemberType.Method;
		//  //var mi = GetMethodInfoForComSlot(ptr.GetType(), index, ref memberType);
		//  object val;
		//  var delType = Expression.GetFuncType(helper.types.Concat(new[] { helper.returnType}));
		//  var vtableTel = Marshal.GetDelegateForFunctionPointer(vtbl[index], delType);
		//  var ret = vtableTel.DynamicInvoke(helper.types.Length == 0 ? null : helper.types);
		//  _ = Marshal.Release(pUnk);
		//  return null;//val;
		//}
		//public static object ComValue(object obj0, object obj1, object obj2 = null)
		//{
		//}
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