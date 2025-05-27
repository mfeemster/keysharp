#if LINUX
namespace Keysharp.Core.Linux.X11
{
	internal class Xlib
	{
		private const string libCName = "libc";
		private const string libDlName = "libdl.so";
		private const string libGdiPlusName = "libgdiplus";
		private const string libPthreadName = "libpthread.so.0";
		private const string libX11Name = "libX11";
		private const string libXfixesName = "libXfixes";//"libXfixes.so.3",

		[DllImport(libXfixesName)]
		internal static extern void XFixesSelectSelectionInput(nint display, nint root, nint atom, SelectionNotifyMask mask);

		[DllImport(libX11Name)]
		internal static extern void XCloseDisplay(nint display);

		[DllImport(libX11Name)]
		internal static extern long XDefaultRootWindow(nint display);

		[DllImport(libX11Name)]
		internal static extern int XDefaultScreen(nint display);

		[DllImport("X11")]
		internal static extern int XFree(nint ptr);

		[DllImport(libX11Name)]
		internal static extern int XGetInputFocus(nint display, out long window, out int focusState);

		[DllImport(libX11Name)]
		internal static extern int XDeleteProperty(nint display, long window, nint property);

		[DllImport(libX11Name)]
		internal static extern int XChangeProperty(nint display, long window, nint property, nint type, int format, PropertyMode mode, ref nint value, int nelements);

		[DllImport(libX11Name)]
		internal static extern int XGetTextProperty(nint display, long window, ref XTextProperty ret, XAtom atom);

		[DllImport(libX11Name)]
		internal static extern int XGetWindowAttributes(nint display, long window, ref XWindowAttributes attributes);

		[DllImport(libX11Name)]
		internal static extern int XRaiseWindow(nint display, long window);

		[DllImport(libX11Name)]
		internal static extern int XLowerWindow(nint display, long window);

		[DllImport(libX11Name)]
		internal static extern int XGetClassHint(nint display, long window, ref XClassHint classHint);

		internal static int GetClassHint(nint display, long window, ref XClassHintStr classHint)
		{
			var localClassHint = XClassHint.Zero;

			if (XGetClassHint(display, window, ref localClassHint) == 0)
			{
				// Clean up unmanaged memory.
				// --------------------------
				// Typically: _classHint.res_name == 0
				// Freeing a 0 is not prohibited.
				// Use first member (at offset 0) to free the structure itself.
				_ = XFree(localClassHint.resName);
				classHint = XClassHintStr.Zero;
				return 0;
			}
			else
			{
				classHint = new XClassHintStr();
				// Marshal data from an unmanaged block of memory to a managed object.
				classHint.resName  = Marshal.PtrToStringAuto(localClassHint.resName);
				classHint.resClass = Marshal.PtrToStringAuto(localClassHint.resClass);
				// Clean up unmanaged memory.
				// --------------------------
				// Freeing a 0 is not prohibited.
				// First structure member (at offset 0) frees  the structure itself as well.
				_ = XFree(localClassHint.resName);
				_ = XFree(localClassHint.resClass);
				return 1;
			}
		}

		[DllImport(libX11Name)]
		internal static extern int XGetWindowProperty(nint display, long window, nint atom, nint longOffset, nint longLength, bool delete, nint reqType, out nint actualType, out int actualFormat, out nint nitems, out nint bytesAfter, ref nint prop);

		[DllImport(libX11Name)]
		public static extern uint XKeysymToKeycode(nint display, uint keySym);

		[DllImport(libX11Name)]
		internal static extern int XLookupString(ref XEvent key, StringBuilder buffer, int count, nint keySym, nint useless);

		[DllImport(libX11Name)]
		internal static extern void XNextEvent(nint display, ref XEvent ev);

		[DllImport(libX11Name)]
		internal static extern nint XOpenDisplay(nint from);

		[DllImport(libX11Name)]
		internal static extern int XIconifyWindow(nint display, long window, int screenNumber);

		[DllImport(libX11Name)]
		internal static extern int XMapWindow(nint display, long window);

		[DllImport(libX11Name)]
		internal static extern int XUnmapWindow(nint display, long window);

		[DllImport(libX11Name)]
		internal static extern int XMoveWindow(nint display, long window, int x, int y);

		[DllImport(libX11Name)]
		internal static extern int XResizeWindow(nint display, long window, int width, int height);

		[DllImport(libX11Name)]
		internal static extern int XMoveResizeWindow(nint display, long window, int x, int y, int width, int height);

		[DllImport(libX11Name)]
		internal static extern int XClearWindow(nint display, long window);

		[DllImport(libX11Name)]
		internal static extern int XKillClient(nint display, long window);

		[DllImport(libX11Name)]
		internal static extern nint XInternAtom(nint display, string atomName, bool onlyIfExists);

		[DllImport(libX11Name)]
		internal static extern int XInternAtoms(nint display, string[] atomNames, int atomCount, bool onlyIfExists, nint[] atoms);

		[DllImport(libX11Name)]
		internal static extern int XSendEvent(nint display, long window, bool propagate, EventMasks eventMask, ref XEvent sendEvent);

		[DllImport(libX11Name)]
		internal static extern int XGetWMName(nint display, long window, ref XTextProperty textProp);

		internal static string GetWMName(nint display, long window)
		{
			var titleNameProp = new XTextProperty();

			if (XGetWMName(display, window, ref titleNameProp) == 0)
			{
				return null;
			}
			else
			{
				var title = "";

				if (titleNameProp.value == 0 || titleNameProp.format != 8 || titleNameProp.nitems > 0)
					title = Marshal.PtrToStringAuto(titleNameProp.value);

				_ = XFree(titleNameProp.value);
				titleNameProp.value = 0;
				return title;
			}
		}

		[DllImport(libX11Name)]
		internal static extern nint XGetAtomName(nint display, nint atom);

		internal static string GetAtomName(nint display, nint atom)
		{
			var buf = XGetAtomName(display, atom);

			if (buf == 0)
				return null;

			var name = Marshal.PtrToStringAuto(buf);
			_ = XFree(buf);
			return name;
		}

		[DllImport(libX11Name)]
		internal static extern int XFlush(nint display);

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
		[DllImport(libX11Name)]
		internal static extern int XQueryTree(nint display, long window, out long rootReturn, out long parentReturn,
											  out nint childrenReturn, out int nchildrenReturn);

		[DllImport(libX11Name)]
		internal static extern nint XSelectInput(nint display, long window, EventMasks eventMask);

		[DllImport(libX11Name)]
		internal static extern XErrorHandler XSetErrorHandler(XErrorHandler handler);

		[DllImport(libX11Name)]
		internal static extern void XSetTextProperty(nint display, long window, ref XTextProperty textProp, XAtom atom);

		[DllImport(libX11Name)]
		internal static extern int XStringListToTextProperty(ref nint argv, int argc, ref XTextProperty textProp);

		[DllImport(libX11Name)]
		internal static extern int XTextPropertyToStringList(nint prop, ref byte[] listReturn, out int countReturn);

		[DllImport(libX11Name)]
		internal static extern uint XStringToKeysym(string convert);

		[DllImport(libX11Name)]
		internal extern static bool XTranslateCoordinates(nint display, long srcWin, long destWin, int srcX, int srcY, out int destXreturn, out int destYreturn, out nint childReturn);

		[DllImport(libX11Name)]
		internal extern static bool XGetGeometry(nint display, long window, out nint root, out int x, out int y, out int width, out int height, out int borderWidth, out int depth);

		[DllImport("libXtst.so.6")]
		internal static extern void XTestFakeKeyEvent(nint display, uint keyCode, bool isPress, ulong delay);

		[DllImport(libCName)]
		internal static extern int getpid();

		[DllImport(libCName)]
		internal static extern int gettid();

		[DllImport(libCName)]
		internal static extern uint geteuid();

		[DllImport(libPthreadName)]
		internal static extern ulong pthread_self();

		[DllImport(libGdiPlusName, ExactSpelling = true)]
		internal static extern int GdipDisposeImage(nint image);

		[DllImport(libDlName)]
		internal static extern nint dlopen(string filename, uint flags);

		[DllImport(libDlName)]
		internal static extern nint dlsym(nint handle, string symbol);

		internal const int RTLD_LAZY = 1;
		internal const int RTLD_NOW = 2;
	}

	internal delegate int XErrorHandler(nint displayHandle, ref XErrorEvent errorEvent);
}
#endif