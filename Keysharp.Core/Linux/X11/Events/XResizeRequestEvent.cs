#if LINUX
using System;
using System.Runtime.InteropServices;

namespace Keysharp.Core.Linux.X11.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XResizeRequestEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr window;
		internal int width;
		internal int height;
	}
}
#endif