#if LINUX
namespace Keysharp.Core.Linux.Proxies
{
	/// <summary>
	/// Proxy around a X11 xDisplay
	/// </summary>
	internal class XDisplay : IDisposable
	{
		private static IntPtr _defaultDisp = IntPtr.Zero;
		private int screenNumber;
		internal IntPtr WM_PROTOCOLS;
		internal IntPtr WM_DELETE_WINDOW;
		internal IntPtr WM_TAKE_FOCUS;
		//internal IntPtr _NET_SUPPORTED;
		//internal IntPtr _NET_CLIENT_LIST;
		//internal IntPtr _NET_NUMBER_OF_DESKTOPS;
		internal IntPtr _NET_DESKTOP_GEOMETRY;
		//internal IntPtr _NET_DESKTOP_VIEWPORT;
		internal IntPtr _NET_CURRENT_DESKTOP;
		//internal IntPtr _NET_DESKTOP_NAMES;
		internal IntPtr _NET_ACTIVE_WINDOW;
		internal IntPtr _NET_WORKAREA;
		//internal IntPtr _NET_SUPPORTING_WM_CHECK;
		//internal IntPtr _NET_VIRTUAL_ROOTS;
		//internal IntPtr _NET_DESKTOP_LAYOUT;
		//internal IntPtr _NET_SHOWING_DESKTOP;
		//internal IntPtr _NET_CLOSE_WINDOW;
		//internal IntPtr _NET_MOVERESIZE_WINDOW;
		internal IntPtr _NET_WM_MOVERESIZE;
		//internal IntPtr _NET_RESTACK_WINDOW;
		//internal IntPtr _NET_REQUEST_FRAME_EXTENTS;
		internal IntPtr _NET_WM_NAME;
		//internal IntPtr _NET_WM_VISIBLE_NAME;
		//internal IntPtr _NET_WM_ICON_NAME;
		//internal IntPtr _NET_WM_VISIBLE_ICON_NAME;
		//internal IntPtr _NET_WM_DESKTOP;
		internal IntPtr _NET_WM_WINDOW_TYPE;
		internal IntPtr _NET_WM_STATE;
		//internal IntPtr _NET_WM_ALLOWED_ACTIONS;
		//internal IntPtr _NET_WM_STRUT;
		//internal IntPtr _NET_WM_STRUT_PARTIAL;
		//internal IntPtr _NET_WM_ICON_GEOMETRY;
		internal IntPtr _NET_WM_ICON;
		//internal IntPtr _NET_WM_PID;
		//internal IntPtr _NET_WM_HANDLED_ICONS;
		internal IntPtr _NET_WM_USER_TIME;
		internal IntPtr _NET_FRAME_EXTENTS;
		//internal IntPtr _NET_WM_PING;
		//internal IntPtr _NET_WM_SYNC_REQUEST;
		internal IntPtr _NET_SYSTEM_TRAY_S;
		//internal IntPtr _NET_SYSTEM_TRAY_ORIENTATION;
		internal IntPtr _NET_SYSTEM_TRAY_OPCODE;
		internal IntPtr _NET_WM_STATE_MAXIMIZED_HORZ;
		internal IntPtr _NET_WM_STATE_MAXIMIZED_VERT;
		internal IntPtr _XEMBED;
		internal IntPtr _XEMBED_INFO;
		internal IntPtr _MOTIF_WM_HINTS;
		internal IntPtr _NET_WM_STATE_SKIP_TASKBAR;
		internal IntPtr _NET_WM_STATE_ABOVE;
		internal IntPtr _NET_WM_STATE_MODAL;
		internal IntPtr _NET_WM_STATE_HIDDEN;
		internal IntPtr _NET_WM_CONTEXT_HELP;
		internal IntPtr _NET_WM_WINDOW_OPACITY;
		//internal IntPtr _NET_WM_WINDOW_TYPE_DESKTOP;
		//internal IntPtr _NET_WM_WINDOW_TYPE_DOCK;
		//internal IntPtr _NET_WM_WINDOW_TYPE_TOOLBAR;
		//internal IntPtr _NET_WM_WINDOW_TYPE_MENU;
		internal IntPtr _NET_WM_WINDOW_TYPE_UTILITY;
		//internal IntPtr _NET_WM_WINDOW_TYPE_SPLASH;
		// internal IntPtr _NET_WM_WINDOW_TYPE_DIALOG;
		internal IntPtr _NET_WM_WINDOW_TYPE_NORMAL;
		internal IntPtr CLIPBOARD;
		internal IntPtr PRIMARY;
		//internal IntPtr DIB;
		internal IntPtr OEMTEXT;
		internal IntPtr UTF8_STRING;
		internal IntPtr UTF16_STRING;
		internal IntPtr RICHTEXTFORMAT;
		internal IntPtr TARGETS;
		internal IntPtr PostAtom;       // PostMessage atom
		internal IntPtr AsyncAtom;      // Support for async messages
		internal IntPtr HoverAtom;       // PostMessage atom

		internal static XDisplay Default
		{
			get
			{
				if (_defaultDisp == IntPtr.Zero)
					_defaultDisp = Xlib.XOpenDisplay(IntPtr.Zero);

				return new XDisplay(_defaultDisp);
			}
		}

		internal IntPtr Handle { get; } = IntPtr.Zero;

		internal XWindow Root => new XWindow(this, Xlib.XDefaultRootWindow(Handle));

		internal int ScreenNumber => screenNumber;

		internal XDisplay(IntPtr prt)
		{
			Handle = prt;
			screenNumber = Xlib.XDefaultScreen(Handle);
			SetupAtoms();
		}

		public void Dispose()
		{
			if (Handle != IntPtr.Zero && Handle != _defaultDisp)
				Xlib.XCloseDisplay(Handle);
		}

		/// <summary>
		/// Returns the window which currently has input focus
		/// </summary>
		/// <returns></returns>
		internal XWindow XGetInputFocusWindow()
		{
			_ = Xlib.XGetInputFocus(Handle, out var hwndWnd, out var focusState);
			return new XWindow(this, hwndWnd);
		}

		/// <summary>
		/// Returns the handle of the window which currently has input focus
		/// </summary>
		/// <returns></returns>
		internal long XGetInputFocusHandle()
		{
			_ = Xlib.XGetInputFocus(Handle, out var hwndWnd, out var focusState);
			return hwndWnd;
		}

		/// <summary>
		/// Returns all Windows of this XDisplay
		/// </summary>
		/// <returns></returns>
		internal IEnumerable<XWindow> XQueryTree(Func<long, bool> filter = null) => XQueryTree(Root, filter);

		/// <summary>
		/// Return all child xWindows from given xWindow
		/// </summary>
		/// <param name="windowToObtain"></param>
		/// <returns></returns>
		internal unsafe IEnumerable<XWindow> XQueryTree(XWindow windowToObtain, Func<long, bool> filter = null)
		{
			var windows = new List<XWindow>();
			var childrenReturn = IntPtr.Zero;

			try
			{
				_ = Xlib.XQueryTree(Handle, windowToObtain.ID, out var rootReturn, out var parentReturn, out childrenReturn, out var nChildrenReturn);
				var pSource = (long*)childrenReturn.ToPointer();

				for (var i = 0; i < nChildrenReturn; i++)
				{
					try
					{
						var id = pSource[i];

						if (filter == null || filter(id))
						{
							var window = new XWindow(this, id);
							windows.Add(window);
							//var tempItem = new WindowItem(window);
							//Keysharp.Scripting.Script.OutputDebug($"Adding window from XQueryTree() with id: {id}, title: {tempItem.Title}");
						}
					}
					catch (Exception ex)
					{
						Keysharp.Scripting.Script.OutputDebug($"Error when applying XQueryTree() filter: {ex.Message}");
					}
				}
			}
			catch (Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
			}
			finally
			{
				if (childrenReturn != IntPtr.Zero)
					_ = Xlib.XFree(childrenReturn);
			}

			//Keysharp.Scripting.Script.OutputDebug($"Exiting XQueryTree().");
			return windows;
		}

		/// <summary>
		/// Return all child xWindows from given xWindow, recursively.
		/// </summary>
		/// <param name="windowToObtain"></param>
		/// <returns></returns>
		internal unsafe IEnumerable<XWindow> XQueryTreeRecursive(Func<long, bool> filter = null) => XQueryTreeRecursive(Root, filter);
		internal unsafe IEnumerable<XWindow> XQueryTreeRecursive(XWindow windowToObtain, Func<long, bool> filter = null)
		{
			var childrenReturn = IntPtr.Zero;
			var windows = new HashSet<XWindow>();

			try
			{
				_ = Xlib.XQueryTree(Handle, windowToObtain.ID, out var rootReturn, out var parentReturn, out childrenReturn, out var nChildrenReturn);
				var pSource = (long*)childrenReturn.ToPointer();

				for (var i = 0; i < nChildrenReturn; i++)
				{
					var id = pSource[i];

					//We are assuming that if this window didn't pass the filter test, then its child windows won't either.
					if (filter == null || filter(id))
					{
						var window = new XWindow(this, id);
						windows.Add(window);
						//var tempItem = new WindowItem(window);
						//Keysharp.Scripting.Script.OutputDebug($"Adding window from XQueryTree() with id: {id}, title: {tempItem.Title}");
						windows.AddRange(XQueryTreeRecursive(window, filter));
					}
				}
			}
			catch (Exception e)
			{
				Keysharp.Scripting.Script.OutputDebug(e.Message);
			}
			finally
			{
				if (childrenReturn != IntPtr.Zero)
					_ = Xlib.XFree(childrenReturn);
			}

			//Keysharp.Scripting.Script.OutputDebug($"Exiting XQueryTreeRecursive().");
			return windows;
		}

		internal void SetupAtoms()
		{
			// make sure this array stays in sync with the statements below
			string[] atom_names = new string[]
			{
				"WM_PROTOCOLS",
				"WM_DELETE_WINDOW",
				"WM_TAKE_FOCUS",
				//"_NET_SUPPORTED",
				//"_NET_CLIENT_LIST",
				//"_NET_NUMBER_OF_DESKTOPS",
				"_NET_DESKTOP_GEOMETRY",
				//"_NET_DESKTOP_VIEWPORT",
				"_NET_CURRENT_DESKTOP",
				//"_NET_DESKTOP_NAMES",
				"_NET_ACTIVE_WINDOW",
				"_NET_WORKAREA",
				//"_NET_SUPPORTING_WM_CHECK",
				//"_NET_VIRTUAL_ROOTS",
				//"_NET_DESKTOP_LAYOUT",
				//"_NET_SHOWING_DESKTOP",
				//"_NET_CLOSE_WINDOW",
				//"_NET_MOVERESIZE_WINDOW",
				"_NET_WM_MOVERESIZE",
				//"_NET_RESTACK_WINDOW",
				//"_NET_REQUEST_FRAME_EXTENTS",
				"_NET_WM_NAME",
				//"_NET_WM_VISIBLE_NAME",
				//"_NET_WM_ICON_NAME",
				//"_NET_WM_VISIBLE_ICON_NAME",
				//"_NET_WM_DESKTOP",
				"_NET_WM_WINDOW_TYPE",
				"_NET_WM_STATE",
				//"_NET_WM_ALLOWED_ACTIONS",
				//"_NET_WM_STRUT",
				//"_NET_WM_STRUT_PARTIAL",
				//"_NET_WM_ICON_GEOMETRY",
				"_NET_WM_ICON",
				//"_NET_WM_PID",
				//"_NET_WM_HANDLED_ICONS",
				"_NET_WM_USER_TIME",
				"_NET_FRAME_EXTENTS",
				//"_NET_WM_PING",
				//"_NET_WM_SYNC_REQUEST",
				"_NET_SYSTEM_TRAY_OPCODE",
				//"_NET_SYSTEM_TRAY_ORIENTATION",
				"_NET_WM_STATE_MAXIMIZED_HORZ",
				"_NET_WM_STATE_MAXIMIZED_VERT",
				"_NET_WM_STATE_HIDDEN",
				"_XEMBED",
				"_XEMBED_INFO",
				"_MOTIF_WM_HINTS",
				"_NET_WM_STATE_SKIP_TASKBAR",
				"_NET_WM_STATE_ABOVE",
				"_NET_WM_STATE_MODAL",
				"_NET_WM_CONTEXT_HELP",
				"_NET_WM_WINDOW_OPACITY",
				//"_NET_WM_WINDOW_TYPE_DESKTOP",
				//"_NET_WM_WINDOW_TYPE_DOCK",
				//"_NET_WM_WINDOW_TYPE_TOOLBAR",
				//"_NET_WM_WINDOW_TYPE_MENU",
				"_NET_WM_WINDOW_TYPE_UTILITY",
				// "_NET_WM_WINDOW_TYPE_DIALOG",
				//"_NET_WM_WINDOW_TYPE_SPLASH",
				"_NET_WM_WINDOW_TYPE_NORMAL",
				"CLIPBOARD",
				"PRIMARY",
				"COMPOUND_TEXT",
				"UTF8_STRING",
				"UTF16_STRING",
				"RICHTEXTFORMAT",
				"TARGETS",
				"_SWF_AsyncAtom",
				"_SWF_PostMessageAtom",
				"_SWF_HoverAtom"
			};
			IntPtr[] atoms = new IntPtr[atom_names.Length];
			_ = Xlib.XInternAtoms(Handle, atom_names, atom_names.Length, false, atoms);
			int off = 0;
			WM_PROTOCOLS = atoms[off++];
			WM_DELETE_WINDOW = atoms[off++];
			WM_TAKE_FOCUS = atoms[off++];
			//_NET_SUPPORTED = atoms [off++];
			//_NET_CLIENT_LIST = atoms [off++];
			//_NET_NUMBER_OF_DESKTOPS = atoms [off++];
			_NET_DESKTOP_GEOMETRY = atoms[off++];
			//_NET_DESKTOP_VIEWPORT = atoms [off++];
			_NET_CURRENT_DESKTOP = atoms[off++];
			//_NET_DESKTOP_NAMES = atoms [off++];
			_NET_ACTIVE_WINDOW = atoms[off++];
			_NET_WORKAREA = atoms[off++];
			//_NET_SUPPORTING_WM_CHECK = atoms [off++];
			//_NET_VIRTUAL_ROOTS = atoms [off++];
			//_NET_DESKTOP_LAYOUT = atoms [off++];
			//_NET_SHOWING_DESKTOP = atoms [off++];
			//_NET_CLOSE_WINDOW = atoms [off++];
			//_NET_MOVERESIZE_WINDOW = atoms [off++];
			_NET_WM_MOVERESIZE = atoms[off++];
			//_NET_RESTACK_WINDOW = atoms [off++];
			//_NET_REQUEST_FRAME_EXTENTS = atoms [off++];
			_NET_WM_NAME = atoms[off++];
			//_NET_WM_VISIBLE_NAME = atoms [off++];
			//_NET_WM_ICON_NAME = atoms [off++];
			//_NET_WM_VISIBLE_ICON_NAME = atoms [off++];
			//_NET_WM_DESKTOP = atoms [off++];
			_NET_WM_WINDOW_TYPE = atoms[off++];
			_NET_WM_STATE = atoms[off++];
			//_NET_WM_ALLOWED_ACTIONS = atoms [off++];
			//_NET_WM_STRUT = atoms [off++];
			//_NET_WM_STRUT_PARTIAL = atoms [off++];
			//_NET_WM_ICON_GEOMETRY = atoms [off++];
			_NET_WM_ICON = atoms[off++];
			//_NET_WM_PID = atoms [off++];
			//_NET_WM_HANDLED_ICONS = atoms [off++];
			_NET_WM_USER_TIME = atoms[off++];
			_NET_FRAME_EXTENTS = atoms[off++];
			//_NET_WM_PING = atoms [off++];
			//_NET_WM_SYNC_REQUEST = atoms [off++];
			_NET_SYSTEM_TRAY_OPCODE = atoms[off++];
			//_NET_SYSTEM_TRAY_ORIENTATION = atoms [off++];
			_NET_WM_STATE_MAXIMIZED_HORZ = atoms[off++];
			_NET_WM_STATE_MAXIMIZED_VERT = atoms[off++];
			_NET_WM_STATE_HIDDEN = atoms[off++];
			_XEMBED = atoms[off++];
			_XEMBED_INFO = atoms[off++];
			_MOTIF_WM_HINTS = atoms[off++];
			_NET_WM_STATE_SKIP_TASKBAR = atoms[off++];
			_NET_WM_STATE_ABOVE = atoms[off++];
			_NET_WM_STATE_MODAL = atoms[off++];
			_NET_WM_CONTEXT_HELP = atoms[off++];
			_NET_WM_WINDOW_OPACITY = atoms[off++];
			//_NET_WM_WINDOW_TYPE_DESKTOP = atoms [off++];
			//_NET_WM_WINDOW_TYPE_DOCK = atoms [off++];
			//_NET_WM_WINDOW_TYPE_TOOLBAR = atoms [off++];
			//_NET_WM_WINDOW_TYPE_MENU = atoms [off++];
			_NET_WM_WINDOW_TYPE_UTILITY = atoms[off++];
			// _NET_WM_WINDOW_TYPE_DIALOG = atoms [off++];
			//_NET_WM_WINDOW_TYPE_SPLASH = atoms [off++];
			_NET_WM_WINDOW_TYPE_NORMAL = atoms[off++];
			CLIPBOARD = atoms[off++];
			PRIMARY = atoms[off++];
			OEMTEXT = atoms[off++];
			UTF8_STRING = atoms[off++];
			UTF16_STRING = atoms[off++];
			RICHTEXTFORMAT = atoms[off++];
			TARGETS = atoms[off++];
			AsyncAtom = atoms[off++];
			PostAtom = atoms[off++];
			HoverAtom = atoms[off++];
			//DIB = (IntPtr)Atom.XA_PIXMAP;
			_NET_SYSTEM_TRAY_S = Xlib.XInternAtom(Handle, "_NET_SYSTEM_TRAY_S" + screenNumber.ToString(), false);
			//for (var i = 0; i < atom_names.Length; i++)
			//  Keysharp.Scripting.Script.OutputDebug($"Atom {atom_names[i]} = {atoms[i].ToInt64()}.");
		}

	}
}
#endif