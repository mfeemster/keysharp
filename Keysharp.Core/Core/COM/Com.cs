using System;
using System.Collections.Generic;
using System.Configuration;
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

		public static object ComObject(string progId)
		{
			var type = Guid.TryParse(progId, out var clsid) ? Type.GetTypeFromCLSID(clsid, true) : Type.GetTypeFromProgID(progId, true);
			return Activator.CreateInstance(type);
		}

		public static object ComObjActive(object progId) => GetActiveObject(progId.As());

		public static object ComObjGet(object progId) => Marshal.BindToMoniker(progId.As());

		//public static object ComObjArray(object obj0, object obj1, params object[] args)
		//{
		//  var varType = obj0.Ai();
		//  var dim1Size = obj1.Ai();
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
