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
		internal static extern void XFixesSelectSelectionInput(IntPtr display, IntPtr root, IntPtr atom, SelectionNotifyMask mask);

		[DllImport(libX11Name)]
		internal static extern void XCloseDisplay(IntPtr display);

		[DllImport(libX11Name)]
		internal static extern long XDefaultRootWindow(IntPtr display);

		[DllImport(libX11Name)]
		internal static extern int XDefaultScreen(IntPtr display);

		[DllImport("X11")]
		internal static extern int XFree(IntPtr ptr);

		[DllImport(libX11Name)]
		internal static extern int XGetInputFocus(IntPtr display, out long window, out int focusState);

		[DllImport(libX11Name)]
		internal static extern int XDeleteProperty(IntPtr display, long window, IntPtr property);

		[DllImport(libX11Name)]
		internal static extern int XChangeProperty(IntPtr display, long window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref IntPtr value, int nelements);

		[DllImport(libX11Name)]
		internal static extern int XGetTextProperty(IntPtr display, long window, ref XTextProperty ret, XAtom atom);

		[DllImport(libX11Name)]
		internal static extern int XGetWindowAttributes(IntPtr display, long window, ref XWindowAttributes attributes);

		[DllImport(libX11Name)]
		internal static extern int XRaiseWindow(IntPtr display, long window);

		[DllImport(libX11Name)]
		internal static extern int XLowerWindow(IntPtr display, long window);

		[DllImport(libX11Name)]
		internal static extern int XGetClassHint(IntPtr display, long window, ref XClassHint classHint);

		internal static int GetClassHint(IntPtr display, long window, ref XClassHintStr classHint)
		{
			var localClassHint = XClassHint.Zero;

			if (XGetClassHint(display, window, ref localClassHint) == 0)
			{
				// Clean up unmanaged memory.
				// --------------------------
				// Typically: _classHint.res_name == IntPtr.Zero
				// Freeing a IntPtr.Zero is not prohibited.
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
				// Freeing a IntPtr.Zero is not prohibited.
				// First structure member (at offset 0) frees  the structure itself as well.
				_ = XFree(localClassHint.resName);
				_ = XFree(localClassHint.resClass);
				return 1;
			}
		}

		[DllImport(libX11Name)]
		internal static extern int XGetWindowProperty(IntPtr display, long window, IntPtr atom, IntPtr longOffset, IntPtr longLength, bool delete, IntPtr reqType, out IntPtr actualType, out int actualFormat, out IntPtr nitems, out IntPtr bytesAfter, ref IntPtr prop);

		[DllImport(libX11Name)]
		public static extern uint XKeysymToKeycode(IntPtr display, uint keySym);

		[DllImport(libX11Name)]
		internal static extern int XLookupString(ref XEvent key, StringBuilder buffer, int count, IntPtr keySym, IntPtr useless);

		[DllImport(libX11Name)]
		internal static extern void XNextEvent(IntPtr display, ref XEvent ev);

		[DllImport(libX11Name)]
		internal static extern IntPtr XOpenDisplay(IntPtr from);

		[DllImport(libX11Name)]
		internal static extern int XIconifyWindow(IntPtr display, long window, int screenNumber);

		[DllImport(libX11Name)]
		internal static extern int XMapWindow(IntPtr display, long window);

		[DllImport(libX11Name)]
		internal static extern int XUnmapWindow(IntPtr display, long window);

		[DllImport(libX11Name)]
		internal static extern int XMoveWindow(IntPtr display, long window, int x, int y);

		[DllImport(libX11Name)]
		internal static extern int XResizeWindow(IntPtr display, long window, int width, int height);

		[DllImport(libX11Name)]
		internal static extern int XMoveResizeWindow(IntPtr display, long window, int x, int y, int width, int height);

		[DllImport(libX11Name)]
		internal static extern int XClearWindow(IntPtr display, long window);

		[DllImport(libX11Name)]
		internal static extern int XKillClient(IntPtr display, long window);

		[DllImport(libX11Name)]
		internal static extern IntPtr XInternAtom(IntPtr display, string atomName, bool onlyIfExists);

		[DllImport(libX11Name)]
		internal static extern int XInternAtoms(IntPtr display, string[] atomNames, int atomCount, bool onlyIfExists, IntPtr[] atoms);

		[DllImport(libX11Name)]
		internal static extern int XSendEvent(IntPtr display, long window, bool propagate, EventMasks eventMask, ref XEvent sendEvent);

		[DllImport(libX11Name)]
		internal static extern int XGetWMName(IntPtr display, long window, ref XTextProperty textProp);

		internal static string GetWMName(IntPtr display, long window)
		{
			var titleNameProp = new XTextProperty();

			if (XGetWMName(display, window, ref titleNameProp) == 0)
			{
				return null;
			}
			else
			{
				var title = "";

				if (titleNameProp.value == IntPtr.Zero || titleNameProp.format != 8 || titleNameProp.nitems > 0)
					title = Marshal.PtrToStringAuto(titleNameProp.value);

				_ = XFree(titleNameProp.value);
				titleNameProp.value = IntPtr.Zero;
				return title;
			}
		}

		[DllImport(libX11Name)]
		internal static extern IntPtr XGetAtomName(IntPtr display, IntPtr atom);

		internal static string GetAtomName(IntPtr display, IntPtr atom)
		{
			var buf = XGetAtomName(display, atom);

			if (buf == IntPtr.Zero)
				return null;

			var name = Marshal.PtrToStringAuto(buf);
			_ = XFree(buf);
			return name;
		}

		[DllImport(libX11Name)]
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
		[DllImport(libX11Name)]
		internal static extern int XQueryTree(IntPtr display, long window, out long rootReturn, out long parentReturn,
											  out IntPtr childrenReturn, out int nchildrenReturn);

		[DllImport(libX11Name)]
		internal static extern IntPtr XSelectInput(IntPtr display, long window, EventMasks eventMask);

		[DllImport(libX11Name)]
		internal static extern XErrorHandler XSetErrorHandler(XErrorHandler handler);

		[DllImport(libX11Name)]
		internal static extern void XSetTextProperty(IntPtr display, long window, ref XTextProperty textProp, XAtom atom);

		[DllImport(libX11Name)]
		internal static extern int XStringListToTextProperty(ref IntPtr argv, int argc, ref XTextProperty textProp);

		[DllImport(libX11Name)]
		internal static extern int XTextPropertyToStringList(IntPtr prop, ref byte[] listReturn, out int countReturn);

		[DllImport(libX11Name)]
		internal static extern uint XStringToKeysym(string convert);

		[DllImport(libX11Name)]
		internal extern static bool XTranslateCoordinates(IntPtr display, long srcWin, long destWin, int srcX, int srcY, out int destXreturn, out int destYreturn, out IntPtr childReturn);

		[DllImport(libX11Name)]
		internal extern static bool XGetGeometry(IntPtr display, long window, out IntPtr root, out int x, out int y, out int width, out int height, out int borderWidth, out int depth);

		[DllImport("libXtst.so.6")]
		internal static extern void XTestFakeKeyEvent(IntPtr display, uint keyCode, bool isPress, ulong delay);

		[DllImport(libCName)]
		internal static extern int getpid();

		[DllImport(libCName)]
		internal static extern int gettid();

		[DllImport(libCName)]
		internal static extern uint geteuid();

		[DllImport(libCName)]
		[System.Security.SuppressUnmanagedCodeSecurity]
		internal static extern int errno();//Untested//TODO

		[DllImport(libPthreadName)]
		internal static extern ulong pthread_self();

		[DllImport(libGdiPlusName, ExactSpelling = true)]
		internal static extern int GdipDisposeImage(IntPtr image);

		[DllImport(libDlName)]
		internal static extern IntPtr dlopen(string filename, uint flags);

		[DllImport(libDlName)]
		internal static extern IntPtr dlsym(IntPtr handle, string symbol);

		internal const int RTLD_LAZY = 1;
		internal const int RTLD_NOW = 2;
	}

	internal delegate int XErrorHandler(IntPtr displayHandle, ref XErrorEvent errorEvent);
}
#endif