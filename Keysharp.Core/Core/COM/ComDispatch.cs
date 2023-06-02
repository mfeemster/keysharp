using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using COM = System.Runtime.InteropServices.ComTypes;

namespace Keysharp.Core.COM
{
	/// <summary>
	/// The IDispatch interface.
	/// This was taken from https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/COM/
	/// under the MIT license.
	/// </summary>
	[Guid("00020400-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport]
	public interface IDispatch
	{
		[PreserveSig]
		int GetTypeInfoCount(out int info);

		[PreserveSig]
		int GetTypeInfo(int iTInfo, int lcid, out System.Runtime.InteropServices.ComTypes.ITypeInfo? ppTInfo);

		void GetIDsOfNames(
			[MarshalAs(UnmanagedType.LPStruct)] Guid iid,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames,
			int cNames,
			int lcid,
			[Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4)] int[] rgDispId);

		//void Invoke(
		//  int dispIdMember,
		//  [MarshalAs(UnmanagedType.LPStruct)] Guid iid,
		//  int lcid,
		//  System.Runtime.InteropServices.ComTypes.INVOKEKIND wFlags,
		//  [In, Out][MarshalAs(UnmanagedType.LPArray)] System.Runtime.InteropServices.ComTypes.DISPPARAMS[] paramArray,
		//  out object? pVarResult,
		//  out ComInvoker.EXCEPINFO pExcepInfo,
		//  out uint puArgErr);
	}

	//[ComImport, Guid("0000010c-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	//public interface IPersist
	//{
	//  [PreserveSig]
	//  void GetClassID(out Guid pClassID);
	//}

	[ComImport]
	[Guid("B196B283-BAB4-101A-B69C-00AA00341D07")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	interface IProvideClassInfo
	{
		[PreserveSig]
		//int GetClassInfo(out IntPtr typeInfo);
		int GetClassInfo(out System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo);
	}
}
