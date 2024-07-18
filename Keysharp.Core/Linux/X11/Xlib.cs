#if LINUX
namespace Keysharp.Core.Linux.X11
{
	internal class Xlib
	{
		private const string libraryName = "libX11";

		[DllImport(libraryName)]
		internal static extern void XCloseDisplay(IntPtr display);

		[DllImport(libraryName)]
		internal static extern uint XDefaultRootWindow(IntPtr display);

		[DllImport(libraryName)]
		internal static extern int XDefaultScreen(IntPtr display);

		[DllImport("X11")]
		internal static extern int XFree(IntPtr ptr);

		[DllImport(libraryName)]
		internal static extern int XGetInputFocus(IntPtr display, out uint window, out int focusState);

		[DllImport(libraryName)]
		internal static extern int XGetTextProperty(IntPtr display, uint window, ref XTextProperty ret, XAtom atom);

		[DllImport(libraryName)]
		internal static extern int XGetWindowAttributes(IntPtr display, uint window, ref XWindowAttributes attributes);

		[DllImport(libraryName)]
		internal static extern int XGetWindowProperty(IntPtr display, uint window, IntPtr atom, IntPtr longOffset, IntPtr longLength, bool delete, IntPtr reqType, out IntPtr actualType, out int actualFormat, out IntPtr nitems, out IntPtr bytesAfter, ref IntPtr prop);

		[DllImport(libraryName)]
		internal static extern uint XKeysymToKeycode(IntPtr display, uint keySym);

		[DllImport(libraryName)]
		internal static extern int XLookupString(ref XEvent key, StringBuilder buffer, int count, IntPtr keySym, IntPtr useless);

		[DllImport(libraryName)]
		internal static extern void XNextEvent(IntPtr display, ref XEvent ev);

		[DllImport(libraryName)]
		internal static extern IntPtr XOpenDisplay(IntPtr from);

		[DllImport(libraryName)]
		internal static extern int XIconifyWindow(IntPtr display, uint window, int screenNumber);

		[DllImport(libraryName)]
		internal static extern int XMapWindow(IntPtr display, uint window);

		[DllImport(libraryName)]
		internal static extern int XUnmapWindow(IntPtr display, uint window);

		[DllImport(libraryName)]
		internal static extern int XClearWindow(IntPtr display, uint window);

		[DllImport(libraryName)]
		internal static extern int XKillClient(IntPtr display, uint window);

		[DllImport(libraryName)]
		internal static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

		[DllImport(libraryName)]
		internal static extern int XInternAtoms(IntPtr display, string[] atomNames, int atomCount, bool onlyIfExists, IntPtr[] atoms);

		[DllImport(libraryName)]
		internal static extern int XSendEvent(IntPtr display, uint window, bool propagate, EventMasks eventMask, ref XEvent sendEvent);

		[DllImport(libraryName)]
		internal static extern int XFlush(IntPtr display);

		/// <summary>
		/// The XQueryTree() function returns the root ID, the parent window ID,
		/// a pointer to the list of children windows (NULL when there are no children),
		/// and the number of children in the list for the specified window.
		/// The children are listed in current stacking order, from bottommost (first) to topmost (last).
		/// XQueryTree() returns zero if it fails and nonzero if it succeeds.
		/// To free a non-NULL children list when it is no longer needed, use XFree().
		/// </summary>
		/// <param name="display">Specifies the connection to the X server.</param>
		/// <param name="window">Specifies the window whose list of children, root, parent, and number of children you want to obtain.</param>
		/// <param name="root_return">Returns the root window.</param>
		/// <param name="parent_return">Returns the parent window.</param>
		/// <param name="children_return">Returns the list of children.</param>
		/// <param name="nchildren_return">Returns the number of children.</param>
		/// <returns></returns>
		[DllImport(libraryName)]
		internal static extern int XQueryTree(IntPtr display, uint window, out int rootReturn, out int parentReturn,
											  out IntPtr childrenReturn, out int nchildrenReturn);

		[DllImport(libraryName)]
		internal static extern IntPtr XSelectInput(IntPtr display, uint window, EventMasks eventMask);

		[DllImport(libraryName)]
		internal static extern XErrorHandler XSetErrorHandler(XErrorHandler handler);

		[DllImport(libraryName)]
		internal static extern void XSetTextProperty(IntPtr display, uint window, ref XTextProperty textProp, XAtom atom);

		[DllImport(libraryName)]
		internal static extern int XStringListToTextProperty(ref IntPtr argv, int argc, ref XTextProperty textProp);

		[DllImport(libraryName)]
		internal static extern int XStringListToTextProperty(IntPtr[] argv, int argc, ref XTextProperty textProp);

		[DllImport(libraryName)]
		internal static extern uint XStringToKeysym(string convert);

		[DllImport("libXtst.so.6")]
		internal static extern void XTestFakeKeyEvent(IntPtr display, uint keyCode, bool isPress, ulong delay);
	}

	internal delegate int XErrorHandler(IntPtr displayHandle, ref XErrorEvent errorEvent);
}
#endif