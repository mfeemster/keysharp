#if LINUX
namespace Keysharp.Core.Linux.X11.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XGravityEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr xevent;
		internal IntPtr window;
		internal int x;
		internal int y;
	}
}
#endif