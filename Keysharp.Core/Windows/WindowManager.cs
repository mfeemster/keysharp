﻿#if WINDOWS
namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Concrete implementation of WindowManager for the Windows platfrom.
	/// </summary>
	internal class WindowManager : WindowManagerBase
	{
		internal override WindowItemBase ActiveWindow => TheScript.WindowProvider.Manager.CreateWindow(WindowsAPI.GetForegroundWindow());
		private int lastWindowCount = 64;

		/// <summary>
		/// Return all top level windows. This does not recurse into child windows.
		/// </summary>
		internal override IEnumerable<WindowItemBase> AllWindows
		{
			get
			{
				var windows = new List<WindowItemBase>(lastWindowCount);
				var doHidden = ThreadAccessors.A_DetectHiddenWindows;
				var mgr = Script.TheScript.WindowProvider.Manager;
				_ = WindowsAPI.EnumWindows(delegate (nint hwnd, int lParam)
				{
					if (doHidden || WindowsAPI.IsWindowVisible(hwnd))
						windows.Add(mgr.CreateWindow(hwnd));

					return true;
				}, 0);
				lastWindowCount = windows.Count;
				return windows;
			}
		}

		internal WindowManager() => Script.TheScript.ProcessesData.CurrentThreadID = WindowsAPI.GetCurrentThreadId();

		internal override WindowItemBase CreateWindow(nint id) => new WindowItem(id);

		internal override IEnumerable<WindowItemBase> FilterForGroups(IEnumerable<WindowItemBase> windows)
		{
			return windows.Where((w) =>
			{
				var style = w.Style;
				var exstyle = w.ExStyle;
				return w.Enabled &&
					   (exstyle & WindowsAPI.WS_EX_TOPMOST) == 0 &&
					   (exstyle & WindowsAPI.WS_EX_NOACTIVATE) == 0 &&
					   (exstyle & (WindowsAPI.WS_EX_TOOLWINDOW | WindowsAPI.WS_EX_APPWINDOW)) != WindowsAPI.WS_EX_TOOLWINDOW &&
					   WindowsAPI.IsWindowVisible(w.Handle) &&
					   WindowsAPI.GetLastActivePopup(w.Handle) != w.Handle &&
					   WindowsAPI.GetWindow(w.Handle, WindowsAPI.GW_OWNER) == 0 &&
					   WindowsAPI.GetShellWindow() != w.Handle &&
					   !WindowsAPI.IsWindowCloaked(w.Handle);
			});
		}

		internal override WindowItemBase FindWindow(SearchCriteria criteria, bool last = false)
		{
			WindowItemBase found = null;

			if (criteria.IsEmpty)
				return found;

			var mm = ThreadAccessors.A_TitleMatchMode;

			if (mm < 4) //If the matching mode is not RegEx then try to take an optimized path
			{
				var hasTitle = !criteria.Title.IsNullOrEmpty();

				if (!criteria.ClassName.IsNullOrEmpty() || (mm == 3 && hasTitle))
				{
					var hwnd = WindowsAPI.FindWindow(criteria.ClassName == "" ? null : criteria.ClassName, !hasTitle || mm != 3 ? null : criteria.Title);

					if (hwnd == 0) //If there is no match with FindWindow then there can't be a match among AllWindows
						return found;

					found = Script.TheScript.WindowProvider.Manager.CreateWindow(hwnd);

					if (found.Equals(criteria)) //Evaluate any other criteria as well before accepting the match
						return found;

					found = null;
				}
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

		internal override uint GetFocusedCtrlThread(ref nint apControl, nint aWindow)
		{
			// Determine the thread for which we want the keyboard layout.
			// When no foreground window, the script's own layout seems like the safest default.
			var thread_id = 0u;

			if (aWindow == 0)
				aWindow = WindowsAPI.GetForegroundWindow();

			if (aWindow != 0)
			{
				// Get thread of aWindow (which should be the foreground window).
				thread_id = WindowsAPI.GetWindowThreadProcessId(aWindow, out var _);
				// Get focus.  Benchmarks showed this additional step added only 6% to the time,
				// and the total was only around 4�s per iteration anyway (on a Core i5-4460).
				// It is necessary for UWP apps such as Microsoft Edge, and any others where
				// the top-level window belongs to a different thread than the focused control.
				var thread_info = GUITHREADINFO.Default;

				if (WindowsAPI.GetGUIThreadInfo(thread_id, out thread_info) && thread_info.hwndFocus != 0)
				{
					// Use the focused control's thread.
					thread_id = WindowsAPI.GetWindowThreadProcessId(thread_info.hwndFocus, out var _);

					if (apControl != 0)
						apControl = thread_info.hwndFocus;
				}
			}

			return thread_id;
		}

		internal override nint GetForeGroundWindowHwnd() => WindowsAPI.GetForegroundWindow();

		internal override bool IsWindow(nint handle) => WindowsAPI.IsWindow(handle) || handle == WindowsAPI.HWND_BROADCAST;

		internal override void MaximizeAll()
		{
			foreach (var window in AllWindows)
				window.WindowState = FormWindowState.Maximized;
		}

		internal override void MinimizeAll()
		{
			var window = FindWindow(new SearchCriteria { ClassName = "Shell_TrayWnd" });
			_ = WindowsAPI.PostMessage(window.Handle, WindowsAPI.WM_COMMAND, new nint(419), 0);
			WindowItemBase.DoWinDelay();
		}

		internal override void MinimizeAllUndo()
		{
			var window = FindWindow(new SearchCriteria { ClassName = "Shell_TrayWnd" });
			_ = WindowsAPI.PostMessage(window.Handle, WindowsAPI.WM_COMMAND, new nint(416), 0);
			WindowItemBase.DoWinDelay();
		}

		internal override WindowItemBase WindowFromPoint(Point location)
		{
			var ctrl = WindowsAPI.WindowFromPoint(location);

			if (ctrl != 0)
				return Script.TheScript.WindowProvider.Manager.CreateWindow(ctrl);

			return null;
		}
	}
}

#endif