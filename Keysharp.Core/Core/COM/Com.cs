using Keysharp.Scripting;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Keysharp.Core.COM
{
	public static class Com
	{
		//public static object ComValue(object obj0, object obj1, object obj2 = null)
		//{
		//}
		//public static IntPtr ComObjValue(object obj0)
		public static object ComObjValue(object obj0)
		{
			if (obj0 is ComObjArray coa)
			{
				return coa.array;
			}
			else
			{
				var gch = GCHandle.Alloc(obj0, GCHandleType.Pinned);
				var val = gch.AddrOfPinnedObject();
				gch.Free();
				return val;
			}
		}

		public const int vt_empty     =      0; //No value
		public const int vt_null      =      1; //SQL-style Null
		public const int vt_i2        =      2; //16-bit signed int
		public const int vt_i4        =      3; //32-bit signed int
		public const int vt_r4        =      4; //32-bit floating-point number
		public const int vt_r8        =      5; //64-bit floating-point number
		public const int vt_cy        =      6; //Currency
		public const int vt_date      =      7; //Date
		public const int vt_bstr      =      8; //COM string (Unicode string with length prefix)
		public const int vt_dispatch  =      9; //COM object
		public const int vt_error     =    0xA; //Error code(32-bit integer)
		public const int vt_bool      =    0xB; //Boolean True(-1) or False(0)
		public const int vt_variant   =    0xC; //VARIANT(must be combined with VT_ARRAY or VT_BYREF)
		public const int vt_unknown   =    0xD; //IUnknown interface pointer
		public const int vt_decimal   =    0xE; //(not supported)
		public const int vt_i1        =   0x10; //8-bit signed int
		public const int vt_ui1       =   0x11; //8-bit unsigned int
		public const int vt_ui2       =   0x12; //16-bit unsigned int
		public const int vt_ui4       =   0x13; //32-bit unsigned int
		public const int vt_i8        =   0x14; //64-bit signed int
		public const int vt_ui8       =   0x15; //64-bit unsigned int
		public const int vt_int       =   0x16; //Signed machine int
		public const int vt_uint      =   0x17; //Unsigned machine int
		public const int vt_record    =   0x24; //User-defined type -- NOT SUPPORTED
		public const int vt_array     = 0x2000; //SAFEARRAY
		public const int vt_byref     = 0x4000; //Pointer to another type of value

		public static object ComObject(string progId)
		{
			var type = Guid.TryParse(progId, out var clsid) ? Type.GetTypeFromCLSID(clsid, true) : Type.GetTypeFromProgID(progId, true);
			return Activator.CreateInstance(type);
		}

		public static object ComObjActive(object progId) => GetActiveObject(progId.As());

		public static object ComObjGet(object progId) => Marshal.BindToMoniker(progId.As());

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

			var ar = System.Array.CreateInstance(t, lengths);
			//return ar;
			return new ComObjArray()
			{
				array = ar
			};
		}

		//public static int MaxIndex(this System.Array arr, int index)
		//{
		//  return arr.GetLength(index);
		//}

		public static object ComObjType(object obj, object name = null)
		{
			var s = name.As().ToLower();

			if (obj is IDispatch idisp)
			{
				System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo = null;

				if (s.Length == 0)
				{
					//if (obj is System.Runtime.InteropServices.ComTypes.IUnknown)
					{
					}
					_ = idisp.GetTypeInfo(0, 0, out typeInfo);
					typeInfo.GetTypeAttr(out var typeAttr);
					System.Runtime.InteropServices.ComTypes.TYPEATTR attr = (System.Runtime.InteropServices.ComTypes.TYPEATTR)Marshal.PtrToStructure(typeAttr, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));
					var vt = (long)attr.tdescAlias.vt;
					typeInfo.ReleaseTypeAttr(typeAttr);
					return vt;
				}

				if (s.StartsWith('c'))
				{
					if (obj is IProvideClassInfo ipci)
						_ = ipci.GetClassInfo(out typeInfo);

					if (s == "class")
						s = "name";
					else if (s == "clsid")
						s = "iid";
				}
				else
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
						var guid = attr.guid.ToString();
						typeInfo.ReleaseTypeAttr(typeAttr);
						return guid;
					}
				}
			}

			return null;
		}

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
				CLSIDFromProgIDEx(progId, out clsid);

			GetActiveObject(ref clsid, IntPtr.Zero, out var obj);
			return obj;
		}

		[DllImport("ole32", PreserveSig = false)]
		private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid lpclsid);

		[DllImport("oleaut32.dll", PreserveSig = false)]
		static extern void GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
	}
}
