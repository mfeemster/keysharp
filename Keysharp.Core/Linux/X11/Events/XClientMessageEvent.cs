#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XClientMessageEvent
	{
		internal XEventName type;
		internal nint serial;
		internal bool send_event;
		internal nint display;
		internal nint window;
		internal nint message_type;
		internal int format;
		internal nint ptr1;
		internal nint ptr2;
		internal nint ptr3;
		internal nint ptr4;
		internal nint ptr5;
	}
}
#endif