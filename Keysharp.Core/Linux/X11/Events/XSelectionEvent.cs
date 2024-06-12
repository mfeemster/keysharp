#if LINUX
using System;
using System.Runtime.InteropServices;

namespace Keysharp.Core.Linux.X11.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionClearEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr window;
		internal IntPtr selection;
		internal IntPtr time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr requestor;
		internal IntPtr selection;
		internal IntPtr target;
		internal IntPtr property;
		internal IntPtr time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionRequestEvent
	{
		internal XEventName type;
		internal IntPtr serial;
		internal bool send_event;
		internal IntPtr display;
		internal IntPtr owner;
		internal IntPtr requestor;
		internal IntPtr selection;
		internal IntPtr target;
		internal IntPtr property;
		internal IntPtr time;
	}
}
#endif