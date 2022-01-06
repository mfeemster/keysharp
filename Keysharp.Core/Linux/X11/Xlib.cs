using System;
using System.Runtime.InteropServices;
using System.Text;
using Keysharp.Core.Linux.X11.Events;
using Keysharp.Core.Linux.X11.Types;

namespace Keysharp.Core.Linux.X11
{
	internal class Xlib
	{
		[DllImport("libX11")]
		internal static extern void XCloseDisplay(IntPtr Dpy);

		[DllImport("libX11")]
		internal static extern int XDefaultRootWindow(IntPtr Display);

		[DllImport("X11")]
		internal static extern int XFree(IntPtr ptr);

		[DllImport("libX11")]
		internal static extern int XGetInputFocus(IntPtr display, out int window, out int focusState);

		[DllImport("libX11")]
		internal static extern int XGetTextProperty(IntPtr Display, int Window, ref XTextProperty Return, XAtom Property);

		[DllImport("libX11")]
		internal static extern int XGetWindowAttributes(IntPtr display, int window, ref XWindowAttributes attributes);

		[DllImport("libX11")]
		internal static extern uint XKeysymToKeycode(IntPtr Display, uint Keysym);

		[DllImport("libX11")]
		internal static extern int XLookupString(ref XEvent Key, StringBuilder Buffer, int Count, IntPtr KeySym, IntPtr Useless);

		[DllImport("libX11")]
		internal static extern void XNextEvent(IntPtr Display, ref XEvent Event);

		[DllImport("libX11")]
		internal static extern IntPtr XOpenDisplay(IntPtr From);

		/// <summary>
		/// The XQueryTree() function returns the root ID, the parent window ID,
		/// a pointer to the list of children windows (NULL when there are no children),
		/// and the number of children in the list for the specified window.
		/// The children are listed in current stacking order, from bottommost (first) to topmost (last).
		/// XQueryTree() returns zero if it fails and nonzero if it succeeds.
		/// To free a non-NULL children list when it is no longer needed, use XFree().
		/// </summary>
		/// <param name="display">Specifies the connection to the X server.</param>
		/// <param name="w">Specifies the window whose list of children, root, parent, and number of children you want to obtain.</param>
		/// <param name="root_return">Returns the root window.</param>
		/// <param name="parent_return">Returns the parent window.</param>
		/// <param name="children_return">Returns the list of children.</param>
		/// <param name="nchildren_return">Returns the number of children.</param>
		/// <returns></returns>
		[DllImport("libX11")]
		internal static extern int XQueryTree(IntPtr display, int w, out int root_return, out int parent_return,
											  out IntPtr children_return, out int nchildren_return);

		[DllImport("libX11")]
		internal static extern IntPtr XSelectInput(IntPtr Display, int Window, EventMasks EventMask);

		[DllImport("libX11")]
		internal static extern XErrorHandler XSetErrorHandler(XErrorHandler Handler);

		[DllImport("libX11")]
		internal static extern void XSetTextProperty(IntPtr Display, int Window, ref XTextProperty Prop, XAtom property);

		[DllImport("X11")]
		internal static extern int XStringListToTextProperty(ref IntPtr argv, int argc, ref XTextProperty textprop);

		[DllImport("X11")]
		internal static extern int XStringListToTextProperty(IntPtr[] argv, int argc, ref XTextProperty textprop);

		[DllImport("libX11")]
		internal static extern uint XStringToKeysym(string Convert);

		[DllImport("libXtst.so.6")]
		internal static extern void XTestFakeKeyEvent(IntPtr Display, uint KeyCode, bool isPress, ulong delay);
	}

	internal delegate int XErrorHandler(IntPtr DisplayHandle, ref XErrorEvent error_event);
}