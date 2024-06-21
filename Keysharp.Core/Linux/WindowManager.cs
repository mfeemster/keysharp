#if LINUX
using System;
using System.Collections.Generic;
using System.Drawing;
using Keysharp.Core.Common.Platform;
using Keysharp.Core.Common.Window;
using Keysharp.Core.Linux.Proxies;


namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of WindowManager for the linux platfrom.
	/// </summary>
	internal class WindowManager : WindowManagerBase
	{
		// ToDo: There may be more than only one xDisplay
		private XDisplay _display = null;

		internal override WindowItemBase ActiveWindow => new WindowItem(_display.XGetInputFocus());

		internal override IEnumerable<WindowItemBase> AllWindows => throw new NotImplementedException();

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

		internal override WindowItemBase CreateWindow(IntPtr id) => throw new NotImplementedException();

		internal override IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows) => windows;

		internal override WindowItemBase FindWindow(SearchCriteria criteria, bool last = false) => throw new NotImplementedException();

		internal override List<WindowItemBase> FindWindowGroup(SearchCriteria criteria, bool forceAll = false) => null;

		internal override uint GetFocusedCtrlThread(ref IntPtr apControl, IntPtr aWindow) => throw new NotImplementedException();

		internal override IntPtr GetForeGroundWindowHwnd() => throw new NotImplementedException();

		internal override bool IsWindow(IntPtr handle) => throw new NotImplementedException();

		internal override void MinimizeAll() => throw new NotImplementedException();

		internal override void MinimizeAllUndo() => throw new NotImplementedException();

		internal override WindowItemBase WindowFromPoint(Point location) => throw new NotImplementedException();
	}
}

#endif