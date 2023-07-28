using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core.COM
{
	public class ComObject : KeysharpObject//ComValue
	{
		public ComObject(object varType, object val, object flags = null)
		{
			VarType = varType.Ai();
			//Need to inspect the specified type as well as what's passed in. For example, passing in an IntPtr.
			//if (VarType == Com.vt_dispatch || VarType == Com.vt_unknown)
			{
				//Original did something here with QueryInterface(), but unsure how to port that.
			}
			//else
			Value = val;
			Flags = flags != null ? flags.Ai() : 1;

			switch (VarType)
			{
				case Com.vt_empty: break;

				case Com.vt_null: break; //SQL-style Null

				case Com.vt_i2: break; //16-bit signed int

				case Com.vt_i4: break; //32-bit signed int

				case Com.vt_r4: break; //32-bit floating-point number

				case Com.vt_r8: break; //64-bit floating-point number

				case Com.vt_cy: break; //Currency

				case Com.vt_date: break; //Date

				case Com.vt_bstr: break; //COM string (Unicode string with length prefix)

				case Com.vt_dispatch: break; //COM object

				case Com.vt_error: break; //Error code(32-bit integer)

				case Com.vt_bool: break; //Boolean True(-1) or False(0)

				case Com.vt_variant: break; //VARIANT(must be combined with VT_ARRAY or VT_BYREF)

				case Com.vt_unknown: break; //IUnknown interface pointer

				case Com.vt_decimal: break; //(not supported)

				case Com.vt_i1: break; //8-bit signed int

				case Com.vt_ui1: break; //8-bit unsigned int

				case Com.vt_ui2: break; //16-bit unsigned int

				case Com.vt_ui4: break; //32-bit unsigned int

				case Com.vt_i8: break; //64-bit signed int

				case Com.vt_ui8: break; //64-bit unsigned int

				case Com.vt_int: break; //Signed machine int

				case Com.vt_uint: break; //Unsigned machine int

				case Com.vt_record: break; //User-defined type -- NOT SUPPORTED

				case Com.vt_array: break; //SAFEARRAY

				case Com.vt_byref: break; //Pointer to another type of value
			}
		}

		public int Flags { get; set; }
		public object Value { get; set; }
		public int VarType { get; set; }

		public object Item
		{
			get
			{
				return 123;
			}
			set
			{
			}
		}
	}
}
