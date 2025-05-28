#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XCrossingEvent
	{
		internal XEventName type;
		internal nint serial;
		internal bool send_event;
		internal nint display;
		internal nint window;
		internal nint root;
		internal nint subwindow;
		internal nint time;
		internal int x;
		internal int y;
		internal int x_root;
		internal int y_root;
		internal NotifyMode mode;
		internal NotifyDetail detail;
		internal bool same_screen;
		internal bool focus;
		internal int state;
	}
}
#endif