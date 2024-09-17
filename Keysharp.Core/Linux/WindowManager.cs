#if LINUX
namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of WindowManager for the linux platfrom.
	/// </summary>
	internal class WindowManager : WindowManagerBase
	{
		internal static Lock xLibLock = new (); //The X11 Winforms implementation uses this, so attempt to do the same here.

		// ToDo: There may be more than only one xDisplay
		private XDisplay _display = null;

		internal override WindowItemBase ActiveWindow => new WindowItem(_display.XGetInputFocusWindow());

		/// <summary>
		/// Return all top level windows.
		/// This behaves differently on linux in that it *does* recurse into child windows.
		/// This is needed because otherwise none of the windows will be properly found.
		/// </summary>
		internal override IEnumerable<WindowItemBase> AllWindows
		{
			get
			{
				var attr = new XWindowAttributes();
				var doHidden = ThreadAccessors.A_DetectHiddenWindows;
				var filter = (long id) =>
				{
					if (Xlib.XGetWindowAttributes(_display.Handle, id, ref attr) != 0)
						if (doHidden || attr.map_state == MapState.IsViewable)
							return true;

					return false;
				};
				//return _display.XQueryTree(filter).Select(w => new WindowItem(w));
				return _display.XQueryTreeRecursive(filter).Select(w => new WindowItem(w));
			}
		}

		internal WindowManager()
		{
			_display = XDisplay.Default;
			Processes.CurrentThreadID = (uint)Xlib.gettid();
		}

		internal override WindowItemBase CreateWindow(IntPtr id) => new WindowItem(id);

		internal override IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows) => windows;

		internal override uint GetFocusedCtrlThread(ref IntPtr apControl, IntPtr aWindow) => throw new NotImplementedException();

		internal override IntPtr GetForeGroundWindowHwnd() => new IntPtr(_display.XGetInputFocusHandle());

		internal override bool IsWindow(IntPtr handle)
		{
			var attr = new XWindowAttributes();
			return Xlib.XGetWindowAttributes(_display.Handle, handle.ToInt64(), ref attr) != 0;
		}

		internal override void MaximizeAll()
		{
			foreach (var window in AllWindows)
			{
				//Keysharp.Scripting.Script.OutputDebug($"MaximizeAll(): Examiniming window: {window.Title}");
				window.WindowState = FormWindowState.Maximized;
			}
		}

		internal override void MinimizeAll()
		{
			foreach (var window in AllWindows)
			{
				//Keysharp.Scripting.Script.OutputDebug($"MinimizeAll(): Examiniming window: {window.Title}");
				window.WindowState = FormWindowState.Minimized;
			}
		}

		internal override void MinimizeAllUndo()
		{
			foreach (var window in AllWindows)
			{
				//Keysharp.Scripting.Script.OutputDebug($"MinimizeAllUndo(): Examiniming window: {window.Title}");
				window.WindowState = FormWindowState.Normal;
			}
		}

		internal void SendNetClientMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2)
		{
			var xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			_ = Xlib.XSendEvent(_display.Handle, window, false, EventMasks.NoEvent, ref xev);
		}

		internal void SendNetWMMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2, IntPtr l3)
		{
			var xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			xev.ClientMessageEvent.ptr4 = l3;
			_ = Xlib.XSendEvent(_display.Handle, _display.Root.ID, false, EventMasks.SubstructureRedirect | EventMasks.SubstructureNofity, ref xev);
		}

		internal override WindowItemBase WindowFromPoint(Point location)
		{
			var x = location.X;
			var y = location.Y;

			//Manually searched windows, but that likely is not what we need, given the context it's used in. Mostl likely will need to revisit using AtSpi.//TODO
			foreach (var window in AllWindows)
			{
				var wloc = window.Location;

				if (x >= wloc.X && x < wloc.X + wloc.Width &&
						y >= wloc.Y && y < wloc.Y + wloc.Height)
					return window;
			}

			return null;
		}
	}
}
#endif