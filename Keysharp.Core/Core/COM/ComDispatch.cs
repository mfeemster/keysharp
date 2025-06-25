#if WINDOWS
#nullable enable
using ct = System.Runtime.InteropServices.ComTypes;

namespace Keysharp.Core.COM
{
	/// <summary>
	/// The IDispatch interface.
	/// This was taken loosely from https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/COM/
	/// under the MIT license.
	/// </summary>
	[ComImport]
	[Guid("00020400-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IDispatch
	{
		[PreserveSig]
		int GetTypeInfoCount(out uint info);

		[PreserveSig]
		int GetTypeInfo(int iTInfo, int lcid, out ct.ITypeInfo? ppTInfo);

		[PreserveSig]
		int GetIDsOfNames(
			[In] ref Guid guid,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)]
			string[] names,
			int cNames, int lcid,
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			int[] rgDispId);

		int Invoke(int dispIdMember,
				   [In] ref Guid riid,
				   int lcid,
				   ct.INVOKEKIND wFlags,
				   ref ct.DISPPARAMS pDispParams,
				   nint pVarResult, nint pExcepInfo, nint puArgErr);
	}

	[ComImport]
	[Guid("B196B283-BAB4-101A-B69C-00AA00341D07")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IProvideClassInfo
	{
		[PreserveSig]
		int GetClassInfo(out ct.ITypeInfo typeInfo);
	}

	[ComImport]
	[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IServiceProvider
	{
		[return: MarshalAs(UnmanagedType.I4)]
		[PreserveSig]
		int QueryService(
			[In] ref Guid guidService,
			[In] ref Guid riid,
			[Out] out nint ppvObject);
	}

	/// <summary>
	/// Solution for event handling taken from the answer to my post at:
	/// https://stackoverflow.com/questions/77010721/how-to-late-bind-an-event-sink-for-a-com-object-of-unknown-type-at-runtime-in-c
	/// </summary>
	internal class Dispatcher : IDisposable, IDispatch, ICustomQueryInterface
	{
		private const int E_NOTIMPL = unchecked((int)0x80004001);
		private static readonly Guid IID_IManagedObject = new ("{C3FCC19E-A970-11D2-8B5A-00A0C9B7C9C4}");
		internal static readonly Guid IID_IDispatch = new ("{00020400-0000-0000-c000-000000000046}");
		private ct.IConnectionPoint? connection;
		private int cookie;
		private bool disposedValue;
		private readonly Guid interfaceID;
		private readonly ct.ITypeInfo? typeInfo;

		public ComObject? Co { get; }

		internal Guid InterfaceId => interfaceID;

		internal Dispatcher(ComObject? cobj)
		{
			ArgumentNullException.ThrowIfNull(cobj);
			var container = cobj.Ptr;

			if (container is not ct.IConnectionPointContainer cpContainer)
			{
				_ = Errors.ValueErrorOccurred($"The passed in COM object of type {container.GetType()} was not of type IConnectionPointContainer.");
				return;
			}

			Co = cobj;
			ct.ITypeInfo? ti;

			if (container is IProvideClassInfo ipci)
				_ = ipci.GetClassInfo(out ti);
			else if (container is IDispatch idisp)
				_ = idisp.GetTypeInfo(0, 0, out ti);
			else
			{
				_ = Errors.ValueErrorOccurred($"The passed in COM object of type {container.GetType()} was not of type IProvideClassInfo or IDispatch");
				return;
			}

			if (ti == null)
			{
				_ = Errors.ValueErrorOccurred($"COM TypeInfo was null.");
				return;
			}

			ti.GetTypeAttr(out var typeAttr);
			var attr = Marshal.PtrToStructure<TYPEATTR>(typeAttr);
			var cImplTypes = attr.cImplTypes;
			ti.ReleaseTypeAttr(typeAttr);

			for (var j = 0; j < cImplTypes; j++)
			{
				try
				{
					ti.GetImplTypeFlags(j, out var typeFlags);

					if (typeFlags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FDEFAULT) && typeFlags.HasFlag(IMPLTYPEFLAGS.IMPLTYPEFLAG_FSOURCE))
					{
						ti.GetRefTypeOfImplType(j, out var href);
						ti.GetRefTypeInfo(href, out var ppTI);
						ppTI.GetTypeAttr(out typeAttr);
						attr = Marshal.PtrToStructure<TYPEATTR>(typeAttr);

						if (attr.typekind == TYPEKIND.TKIND_DISPATCH)
						{
							cpContainer.FindConnectionPoint(ref attr.guid, out var con);

							if (con != null)
							{
								interfaceID = attr.guid;
								typeInfo = ppTI;
								con.Advise(this, out cookie);
								ppTI.ReleaseTypeAttr(typeAttr);
								connection = con;
								break;
							}
						}

						ppTI.ReleaseTypeAttr(typeAttr);
					}
				}
				catch (COMException)
				{
				}
			}

			if (connection == null)
				_ = Errors.ErrorOccurred("Failed to connect dispatcher to COM interface.");
		}

		~Dispatcher()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[PreserveSig]
		public int GetIDsOfNames(
			[In] ref Guid guid,
			[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)]
			string[] names,
			int cNames, int lcid,
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]
			int[] rgDispId) => E_NOTIMPL;

		public int GetTypeInfo(int iTInfo, int lcid, out ct.ITypeInfo? ppTInfo)
		{ ppTInfo = null; return E_NOTIMPL; }

		public int GetTypeInfoCount(out uint pctinfo)
		{ pctinfo = 0; return 0; }

		public int Invoke(int dispIdMember,
						  [In] ref Guid riid,
						  int lcid,
						  ct.INVOKEKIND wFlags,
						  ref ct.DISPPARAMS pDispParams,
						  nint pVarResult, nint pExcepInfo, nint puArgErr)
		{
			var args = pDispParams.cArgs > 0 ? Marshal.GetObjectsForNativeVariants(pDispParams.rgvarg, pDispParams.cArgs) : [];
			var names = new string[1];
			typeInfo?.GetNames(dispIdMember, names, 1, out var pcNames);
			var evt = new DispatcherEventArgs(dispIdMember, names[0], args);
			OnEvent(this, evt);
			var result = evt.Result;

			if (pVarResult != 0)
			{
				Marshal.GetNativeVariantForObject(result, pVarResult);
			}

			return 0;
		}

		CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(ref Guid iid, out nint ppv)
		{
			if (iid == typeof(IDispatch).GUID || iid == InterfaceId)
			{
				ppv = Marshal.GetComInterfaceForObject(this, typeof(IDispatch), CustomQueryInterfaceMode.Ignore);
				return CustomQueryInterfaceResult.Handled;
			}

			ppv = 0;
			return iid == IID_IManagedObject ? CustomQueryInterfaceResult.Failed : CustomQueryInterfaceResult.NotHandled;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				var connection = Interlocked.Exchange(ref this.connection, null);

				if (connection != null)
				{
					connection.Unadvise(cookie);
					cookie = 0;
					_ = Marshal.ReleaseComObject(connection);
				}

				disposedValue = true;
			}
		}

		protected virtual void OnEvent(object sender, DispatcherEventArgs e) => EventReceived?.Invoke(sender, e);

		internal event EventHandler<DispatcherEventArgs>? EventReceived;
	}

	internal class DispatcherEventArgs : EventArgs
	{
		internal object?[] Arguments { get; }

		internal int DispId { get; }

		internal string Name { get; }

		internal object? Result { get; set; }

		internal DispatcherEventArgs(int dispId, string name, params object?[] arguments)
		{
			DispId = dispId;
			Name = name;
			Arguments = arguments ?? [];
		}
	}
}
#endif