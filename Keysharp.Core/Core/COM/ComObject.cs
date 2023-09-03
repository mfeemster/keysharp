using Keysharp.Core.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Keysharp.Core.COM
{
	public class ComObject : KeysharpObject//ComValue
	{
		internal static long F_OWNVALUE = 1;
		internal List<IFuncObj> handlers;
		public void CallEvents()
		{
			var result = handlers?.InvokeEventHandlers(this);
		}

		public long Flags { get; set; }

		public object Item
		{
			get => Ptr;
			set => Ptr = value;
		}

		public object Ptr { get; set; }

		public int VarType { get; set; }

		public ComObject(object varType, object val, object flags = null)
		{
			var co = ValueToVarType(val, (int)varType.Al(), true);
			VarType = co.VarType;
			Flags = flags != null ? flags.Al() : 0L;

			if (VarType == Com.vt_bstr && val is not long)
				Flags |= F_OWNVALUE;

			Ptr = val;
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