using System;
using System.Runtime.InteropServices;

namespace Keysharp.Core.Linux.X11.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XCreateWindowEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr parent;
		internal int window;
		internal int x;
		internal int y;
		internal int width;
		internal int height;
		internal int border_width;
		internal bool override_redirect;
	}
}