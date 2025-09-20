#if WINDOWS
namespace Keysharp.Core.COM
{
	//The VARIANT structure with an explicit layout.
	[StructLayout(LayoutKind.Explicit)]
	internal struct VARIANT
	{
		[FieldOffset(0)]
		internal ushort vt;
		[FieldOffset(2)]
		internal ushort wReserved1;
		[FieldOffset(4)]
		internal ushort wReserved2;
		[FieldOffset(6)]
		internal ushort wReserved3;
		[FieldOffset(8)]
		internal VARIANTUnion data;//he union of data fields.
	}

	//The union portion of VARIANT.
	[StructLayout(LayoutKind.Explicit)]
	internal struct VARIANTUnion
	{
		[FieldOffset(0)]
		internal sbyte cVal;
		[FieldOffset(0)]
		internal byte bVal;
		[FieldOffset(0)]
		internal short iVal;
		[FieldOffset(0)]
		internal ushort uiVal;
		[FieldOffset(0)]
		internal int lVal;
		[FieldOffset(0)]
		internal uint ulVal;
		[FieldOffset(0)]
		internal long llVal;
		[FieldOffset(0)]
		internal ulong ullVal;
		[FieldOffset(0)]
		internal float fltVal;
		[FieldOffset(0)]
		internal double dblVal;
		//VARIANT_BOOL is a 16-bit value: -1 (TRUE) or 0 (FALSE).
		[FieldOffset(0)]
		internal short boolVal;
		//BSTR is represented as an nint.
		[FieldOffset(0)]
		internal nint bstrVal;
		//For COM interfaces.
		[FieldOffset(0)]
		internal nint pdispVal;
		//For SAFEARRAYs or other pointer types.
		[FieldOffset(0)]
		internal nint parray;
	}

	internal static class VariantConstants
	{
		internal const ushort VT_EMPTY = 0;
		internal const ushort VT_NULL = 1;
		internal const ushort VT_I2 = 2;
		internal const ushort VT_I4 = 3;
		internal const ushort VT_R4 = 4;
		internal const ushort VT_R8 = 5;
		internal const ushort VT_CY = 6;
		internal const ushort VT_DATE = 7;
		internal const ushort VT_BSTR = 8;
		internal const ushort VT_DISPATCH = 9;
		internal const ushort VT_ERROR = 10;
		internal const ushort VT_BOOL = 11;
		internal const ushort VT_VARIANT = 12;
		internal const ushort VT_UNKNOWN = 13;
		//Extend as needed…
	}
	internal static partial class VariantHelper
	{
		//Clears the VARIANT and frees any allocated memory (such as BSTRs).
		[LibraryImport(WindowsAPI.oleaut, EntryPoint = "VariantClear")]
		internal static partial int VariantClear(ref VARIANT variant);
		[LibraryImport(WindowsAPI.oleaut, EntryPoint = "VariantClear")]
		internal static partial int VariantClear(nint pvarg);

		internal static VARIANT CreateVariantFromInt(int value)
		{
			VARIANT variant = new VARIANT();
			variant.vt = VariantConstants.VT_I4;
			variant.data.lVal = value;
			return variant;
		}

		internal static VARIANT CreateVariantFromDouble(double value)
		{
			VARIANT variant = new VARIANT();
			variant.vt = VariantConstants.VT_R8;
			variant.data.dblVal = value;
			return variant;
		}

		internal static VARIANT CreateVariantFromBool(bool value)
		{
			VARIANT variant = new VARIANT();
			variant.vt = VariantConstants.VT_BOOL;
			//In COM, VARIANT_TRUE is -1 and VARIANT_FALSE is 0.
			variant.data.boolVal = (short)(value ? -1 : 0);
			return variant;
		}

		internal static VARIANT CreateVariantFromString(string value)
		{
			VARIANT variant = new VARIANT();
			variant.vt = VariantConstants.VT_BSTR;
			//Marshal the string to a BSTR. Remember to clear the VARIANT later to free this memory.
			variant.data.bstrVal = Marshal.StringToBSTR(value);
			return variant;
		}

		internal static void ClearVariant(ref VARIANT variant)
		{
			_ = VariantClear(ref variant);
		}
	}
}
#endif