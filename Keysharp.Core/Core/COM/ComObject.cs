using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Metadata;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Core.COM
{
	internal delegate IntPtr DelNone();
	internal delegate IntPtr Del0(IntPtr a);
	internal delegate IntPtr Del1(IntPtr a, IntPtr a0);
	internal delegate IntPtr Del2(IntPtr a, IntPtr a0, IntPtr a1);
	internal delegate IntPtr Del3(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2);
	internal delegate IntPtr Del4(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3);
	internal delegate IntPtr Del5(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4);
	internal delegate IntPtr Del6(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5);
	internal delegate IntPtr Del7(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6);
	internal delegate IntPtr Del8(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7);
	internal delegate IntPtr Del9(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8);
	internal delegate IntPtr Del10(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9);
	internal delegate IntPtr Del11(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10);
	internal delegate IntPtr Del12(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11);
	internal delegate IntPtr Del13(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12);
	internal delegate IntPtr Del14(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13);
	internal delegate IntPtr Del15(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14);
	internal delegate IntPtr Del16(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14, IntPtr a15);
	internal delegate IntPtr Del17(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14, IntPtr a15, IntPtr a16);
	internal delegate IntPtr Del18(IntPtr a, IntPtr a0, IntPtr a1, IntPtr a2, IntPtr a3, IntPtr a4, IntPtr a5, IntPtr a6, IntPtr a7, IntPtr a8, IntPtr a9, IntPtr a10, IntPtr a11, IntPtr a12, IntPtr a13, IntPtr a14, IntPtr a15, IntPtr a16, IntPtr a17);

	unsafe public class ComObject : KeysharpObject//ComValue
	{
		internal static long F_OWNVALUE = 1;
		internal static int MaxVtableLen = 16;
		internal List<IFuncObj> handlers;
		internal object item;

		public long Flags { get; set; }

		public object Item
		{
			get => Ptr;
			set => Ptr = value;
		}

		public object Ptr
		{
			get
			{
				if (item is IntPtr ip)
					return ip;//.ToInt64();
				//else if (VarType == COM.Com.vt_unknown ||
				//       ((Flags & Com.vt_byref) == Com.vt_byref) ||
				//       ((Flags & Com.vt_array) == Com.vt_array))
				//{
				//  var pUnk = Marshal.GetIUnknownForObject(item);
				//  //var vtbl = new IntPtr[1];
				//  //var pUnk = Marshal.GetIUnknownForObject(ptr);
				//  //var pVtbl = Marshal.ReadIntPtr(pUnk);
				//  //Marshal.Copy(pVtbl, vtbl, 0, indexPlus1);
				//  //TypedReference tr = __makeref(pUnk);
				//  //IntPtr ptr = **(IntPtr**)(&tr);
				//  //var gch = GCHandle.Alloc(pUnk, GCHandleType.Pinned);
				//  //var intptr = gch.AddrOfPinnedObject();
				//  //var intptr2 = GCHandle.ToIntPtr(gch);
				//  //var pVtbl = Marshal.ReadIntPtr(intptr);
				//  //gch.Free();
				//  var pVtbl = pUnk;// Marshal.ReadIntPtr(pUnk);
				//  Marshal.Release(pUnk);
				//  return pVtbl;
				//}
				else
					return item;
			}
			set
			{
				//if ((VarType == COM.Com.vt_unknown ||
				//  VarType == COM.Com.vt_dispatch) && item == null)
				//{
				//  item = value;
				//}
				item = value;
			}
		}

		public int VarType { get; set; }


		public ComObject(object varType, object val, object flags = null)
		{
			var co = ValueToVarType(val, (int)varType.Al(), true);
			VarType = co.VarType;
			Flags = flags != null ? flags.Al() : 0L;

			if (VarType == Com.vt_bstr && val is not long)
				Flags |= F_OWNVALUE;

			Ptr = val;
			//vtbl = new IntPtr[MaxVtableLen];
			//if (Marshal.IsComObject(Ptr))
			//{
			//  pUnk = Marshal.GetIUnknownForObject(Ptr);
			//}
			//else if (Ptr is IntPtr ip)
			//{
			//  pUnk = ip;
			//}
			//pVtbl = Marshal.ReadIntPtr(pUnk);
			//Marshal.Copy(pVtbl, vtbl, 0, MaxVtableLen);
			//switch (VarType)
			//{
			//  case Com.vt_empty: break;
			//  case Com.vt_null: break; //SQL-style Null
			//  case Com.vt_i2: break; //16-bit signed int
			//  case Com.vt_i4: break; //32-bit signed int
			//  case Com.vt_r4: break; //32-bit floating-point number
			//  case Com.vt_r8: break; //64-bit floating-point number
			//  case Com.vt_cy: break; //Currency
			//  case Com.vt_date: break; //Date
			//  case Com.vt_bstr: break; //COM string (Unicode string with length prefix)
			//  case Com.vt_dispatch: break; //COM object
			//  case Com.vt_error: break; //Error code(32-bit integer)
			//  case Com.vt_bool: break; //Boolean True(-1) or False(0)
			//  case Com.vt_variant: break; //VARIANT(must be combined with VT_ARRAY or VT_BYREF)
			//  case Com.vt_unknown: break; //IUnknown interface pointer
			//  case Com.vt_decimal: break; //(not supported)
			//  case Com.vt_i1: break; //8-bit signed int
			//  case Com.vt_ui1: break; //8-bit unsigned int
			//  case Com.vt_ui2: break; //16-bit unsigned int
			//  case Com.vt_ui4: break; //32-bit unsigned int
			//  case Com.vt_i8: break; //64-bit signed int
			//  case Com.vt_ui8: break; //64-bit unsigned int
			//  case Com.vt_int: break; //Signed machine int
			//  case Com.vt_uint: break; //Unsigned machine int
			//  case Com.vt_record: break; //User-defined type -- NOT SUPPORTED
			//  case Com.vt_array: break; //SAFEARRAY
			//  case Com.vt_byref: break; //Pointer to another type of value
			//}
		}

		internal ComObject()
		{
		}

		~ComObject()
		{
			if (Marshal.IsComObject(Ptr))
				_ = Marshal.ReleaseComObject(Ptr);
			else if (Ptr is IntPtr ip)
				_ = Marshal.Release(ip);
		}

		public void CallEvents()
		{
			var result = handlers?.InvokeEventHandlers(this);
		}

		public void Clear()
		{
			VarType = 0;
			Ptr = null;
			Flags = 0L;
		}

		internal static void ValueToVariant(object val, ComObject variant)
		{
			if (val is string s)
			{
				variant.Ptr = s.Clone();
				variant.VarType = Com.vt_bstr;
			}
			else if (val is long l)
			{
				variant.Ptr = l;
				variant.VarType = Com.vt_i8;
			}
			else if (val is int i)
			{
				variant.Ptr = i;
				variant.VarType = Com.vt_i4;
			}
			else if (val is double d)
			{
				variant.Ptr = d;
				variant.VarType = Com.vt_r8;
			}
			else if (val is IntPtr ptr)
			{
				variant.Ptr = ptr;
				variant.VarType = Com.vt_i8;
			}
			else if (val is ComObject co)
			{
				variant.Ptr = co.Ptr;
				variant.VarType = co.VarType;

				if ((co.Flags & F_OWNVALUE) == F_OWNVALUE)
				{
					if ((variant.VarType & ~Com.vt_typemask) == Com.vt_array && co.Ptr is ComObjArray coa)
					{
						variant.Ptr = coa.array.Clone();//Copy array since both sides will call Destroy().
					}
					else if (variant.VarType == Com.vt_bstr)
					{
						variant.Ptr = co.Ptr.ToString().Clone();//Copy the string.
					}
				}

				return;
			}

			variant.VarType = Com.vt_dispatch;
			variant.Ptr = val;
		}

		internal static ComObject ValueToVarType(object val, int varType, bool callerIsComValue)
		{
			ComObject co;

			if (varType == Com.vt_variant)
			{
				co = new ComObject();//Use the empty constructors on purpose so they don't keep recursing into this method.
				ValueToVariant(val, co);
				return co;
			}

			if (varType == Com.vt_bool)
			{
				return new ComObject()
				{
					Ptr = val.Ab() ? -1 : 0,//The true value for a variant is actual -1. Not sure if this should be short, int or long?//TODO.
					VarType = Com.vt_bool
				};
			}

			if (val is IntPtr ptr)
				val = ptr.ToInt64();

			if (val is long l)
			{
				co = new ComObject()
				{
					Ptr = l,
					VarType = Com.vt_i8
				};

				switch (varType)
				{
					//case Com.vt_r4:
					//case Com.vt_r8:
					//case Com.vt_date:
					//  break;
					case Com.vt_cy:
						co.Ptr = l * 10000L;
						return co;

					case Com.vt_bstr://These seem not to apply here because we already assigned l above.
					case Com.vt_dispatch:
					case Com.vt_unknown:
						if (callerIsComValue)
							return co;

						break;

					default:
						return co;
				}
			}
			//else if (val is IntPtr ptr)
			//{
			//  return co = new ComObject()
			//  {
			//      Ptr = ptr,
			//      VarType = Com.vt_i8
			//  };
			//}
			else
				ValueToVariant(val, co = new ComObject());

			if (co.VarType != varType)//Attempt to coerce var to the correct type.
			{
				var origVal = co.Ptr;
				var newVal = co.Ptr;

				if (Com.VariantChangeTypeEx(out newVal, ref origVal, Thread.CurrentThread.CurrentCulture.LCID, 0, (short)varType) < 0)
				{
					co.Clear();
					return null;
				}

				co.Ptr = newVal;
				co.VarType = varType;
			}

			return co;
		}
	}
}