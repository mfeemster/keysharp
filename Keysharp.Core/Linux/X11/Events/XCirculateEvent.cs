#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XCirculateEvent
	{
		internal XEventName type;
		internal nint serial;
		internal bool send_event;
		internal nint display;
		internal nint xevent;
		internal nint window;
		internal int place;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XCirculateRequestEvent
	{
		internal XEventName type;
		internal nint serial;
		internal bool send_event;
		internal nint display;
		internal nint parent;
		internal nint window;
		internal int place;
	}
}
#endif