#if LINUX
using System;
using System.Runtime.InteropServices;

namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XWindowChanges
	{
		internal int x;
		internal int y;
		internal int width;
		internal int height;
		internal int border_width;
		internal IntPtr sibling;
		internal StackMode stack_mode;
	}
}
#endif