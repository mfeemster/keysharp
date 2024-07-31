#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XCirculateEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr xevent;
		internal IntPtr window;
		internal int place;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XCirculateRequestEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr parent;
		internal IntPtr window;
		internal int place;
	}
}
#endif