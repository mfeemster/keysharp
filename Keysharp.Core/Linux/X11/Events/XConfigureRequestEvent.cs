#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XConfigureRequestEvent
	{
		internal XEventName type;
		internal nint serial;
		internal bool send_event;
		internal nint display;
		internal nint parent;
		internal nint window;
		internal int x;
		internal int y;
		internal int width;
		internal int height;
		internal int border_width;
		internal nint above;
		internal int detail;
		internal nint value_mask;
	}
}
#endif