#if LINUX
namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of WindowManager for the linux platfrom.
	/// </summary>
	internal class WindowManager : WindowManagerBase
	{
		// ToDo: There may be more than only one xDisplay
		private XDisplay _display = null;
		internal static object xLibLock = new object();//The X11 Winforms implementation uses this, so attempt to do the same here.

		internal override WindowItemBase ActiveWindow => new WindowItem(_display.XGetInputFocusWindow());

		/// <summary>
		/// Return all top level windows. This does not recurse into child windows.
		/// </summary>
		internal override IEnumerable<WindowItemBase> AllWindows
		{
			get
			{
				var attr = new XWindowAttributes();
				var doHidden = ThreadAccessors.A_DetectHiddenWindows;
				var filter = (uint id) =>
				{
					if (Xlib.XGetWindowAttributes(_display.Handle, id, ref attr) != 0)
						if (doHidden || attr.map_state == MapState.IsViewable)
							return true;

					return false;
				};
				return _display.XQueryTree(filter).Select(w => new WindowItem(w));
			}
		}

		internal override WindowItemBase LastFound//If Hwnds work on linux, then it might just be ok to use the base.
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal WindowManager()
		{
			_display = XDisplay.Default;
			//We do this for windows, need some type of equivalent for linux.
			//Processes.CurrentThreadID = WindowsAPI.GetCurrentThreadId();
		}

		internal override WindowItemBase CreateWindow(IntPtr id) => new WindowItem(id);

		internal override IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows) => windows;

		internal override WindowItemBase FindWindow(SearchCriteria criteria, bool last = false)
		{
			WindowItemBase found = null;

			if (criteria.IsEmpty)
				return found;

			if (criteria.HasID)
			{
				var temp = CreateWindow(criteria.ID);

				if (temp.Exists)
					return temp;
			}

			foreach (var window in AllWindows)
			{
				if (window.Equals(criteria))
				{
					found = window;

					if (!last)
						break;
				}
			}

			return found;
		}

		internal override List<WindowItemBase> FindWindowGroup(SearchCriteria criteria, bool forceAll = false) => null;

		internal override uint GetFocusedCtrlThread(ref IntPtr apControl, IntPtr aWindow) => throw new NotImplementedException();

		internal override IntPtr GetForeGroundWindowHwnd() => new IntPtr(_display.XGetInputFocusHandle());

		internal override bool IsWindow(IntPtr handle)
		{
			var attr = new XWindowAttributes();
			return Xlib.XGetWindowAttributes(_display.Handle, (uint)handle.ToInt64(), ref attr) != 0;
		}

		internal override void MinimizeAll() => throw new NotImplementedException();

		internal override void MinimizeAllUndo() => throw new NotImplementedException();

		internal override WindowItemBase WindowFromPoint(Point location) => throw new NotImplementedException();
	}
}

#endif