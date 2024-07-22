using static Keysharp.Scripting.Script;

namespace Keysharp.Core
{
	public static class Window
	{
		private static bool dpimodeset;

		/// <summary>
		/// easy access to the window groups
		/// </summary>
		private static Dictionary<string, WindowGroup> windowGroups => WindowProvider.Manager.Groups;

		public static long ControlAddItem(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ControlAddItem);

		public static void ControlChooseIndex(params object[] obj) => obj.I1O2S3().Splat(ControlProvider.Manager.ControlChooseIndex);

		public static long ControlChooseString(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ControlChooseString);

		public static void ControlClick(params object[] obj) => obj.O2S2I1S3(null, null, "", "", 1).Splat(ControlProvider.Manager.ControlClick);

		public static void ControlDeleteItem(params object[] obj) => obj.I1O2S3().Splat(ControlProvider.Manager.ControlDeleteItem);

		public static long ControlFindItem(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ControlFindItem);

		public static void ControlFocus(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlFocus);

		public static long ControlGetChecked(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetChecked);

		public static string ControlGetChoice(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetChoice);

		public static string ControlGetClassNN(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetClassNN);

		public static long ControlGetEnabled(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetEnabled);

		public static long ControlGetExStyle(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetExStyle);

		public static long ControlGetFocus(params object[] obj) => obj.O1S3().Splat(ControlProvider.Manager.ControlGetFocus);

		public static long ControlGetHwnd(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetHwnd);

		public static long ControlGetIndex(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetIndex);

		public static Array ControlGetItems(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetItems);

		public static void ControlGetPos(ref object outX, ref object outY, ref object outWidth, ref object outHeight, object ctrl = null, object title = null, object text = null, object excludeTitle = null, object excludeText = null) =>
		ControlProvider.Manager.ControlGetPos(ref outX, ref outY, ref outWidth, ref outHeight, ctrl, title?.ToString(), text?.ToString(), excludeTitle?.ToString(), excludeText?.ToString());

		public static long ControlGetStyle(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetStyle);

		public static string ControlGetText(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetText);

		public static long ControlGetVisible(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlGetVisible);

		public static void ControlHide(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlHide);

		public static void ControlHideDropDown(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlHideDropDown);

		public static void ControlMove(params object[] obj) => obj.I4O2S3(int.MinValue, int.MinValue, int.MinValue, int.MinValue).Splat(ControlProvider.Manager.ControlMove);

		public static void ControlSend(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ControlSend);

		public static void ControlSendText(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ControlSendText);

		public static void ControlSetChecked(params object[] obj) => obj.O3S3().Splat(ControlProvider.Manager.ControlSetChecked);

		public static void ControlSetEnabled(params object[] obj) => obj.O3S3().Splat(ControlProvider.Manager.ControlSetEnabled);

		public static void ControlSetExStyle(params object[] obj) => obj.O3S3().Splat(ControlProvider.Manager.ControlSetExStyle);

		public static void ControlSetStyle(params object[] obj) => obj.O3S3().Splat(ControlProvider.Manager.ControlSetStyle);

		public static void ControlSetText(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ControlSetText);

		public static void ControlShow(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlShow);

		public static void ControlShowDropDown(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.ControlShowDropDown);

		public static void DetectHiddenText(params object[] obj) => Accessors.A_DetectHiddenText = obj.O1();

		public static void DetectHiddenWindows(params object[] obj) => Accessors.A_DetectHiddenWindows = obj.O1();

		public static long EditGetCurrentCol(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.EditGetCurrentCol);

		public static long EditGetCurrentLine(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.EditGetCurrentLine);

		public static string EditGetLine(params object[] obj) => obj.I1O2S3().Splat(ControlProvider.Manager.EditGetLine);

		public static long EditGetLineCount(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.EditGetLineCount);

		public static string EditGetSelectedText(params object[] obj) => obj.O2S3().Splat(ControlProvider.Manager.EditGetSelectedText);

		public static void EditPaste(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.EditPaste);

		public static long GroupActivate(params object[] obj)
		{
			var (name, mode) = obj.S2();
			name = (name ?? string.Empty).ToLowerInvariant();

			if (windowGroups.TryGetValue(name, out var group))
			{
				if (group.sc.Count == 0)
					return 0L;

				var windows = SearchWindows($"ahk_group {name}");

				if (windows.Count != 0 && windows.Count == group.activated.Count)
					group.activated.Clear();

				if (windows.Count == 1 && windows[0].Handle.ToInt64() == WindowProvider.Manager.GetForeGroundWindowHwnd().ToInt64())
					return 0L;

				if (!mode.Equals(Keywords.Keyword_R, StringComparison.OrdinalIgnoreCase) && !windows.Any(w => w.Active))
					windows.Reverse();

				foreach (var win in windows)
				{
					var h = win.Handle.ToInt64();

					if (!group.activated.Contains(h))
					{
						win.Active = true;
						group.activated.Push(h);
						group.lastWasDeactivate = false;
						return h;
					}
				}
			}

			return 0L;
		}

		public static void GroupAdd(params object[] obj)
		{
			var (name, winTitle, winText, excludeTitle, excludeText) = obj.S1O1S3();

			if (string.IsNullOrEmpty(name))
				return;

			if (!windowGroups.ContainsKey(name))
				windowGroups.Add(name, new WindowGroup());

			if (name != "AllWindows")
			{
				var group = windowGroups[name];
				group.sc.Add(SearchCriteria.FromString(winTitle, winText, excludeTitle, excludeText));
				group.activated.Clear();
				group.deactivated.Clear();
			}
		}

		public static void GroupClose(params object[] obj)
		{
			var (name, mode) = obj.S2();
			name = (name ?? string.Empty).ToLowerInvariant();

			if (windowGroups.TryGetValue(name, out var group))
			{
				if (group.sc.Count == 0)
					return;

				var stack = group.lastWasDeactivate ? group.deactivated : group.activated;
				var windows = SearchWindows($"ahk_group {name}");

				switch (mode.ToLowerInvariant())
				{
					case Keywords.Keyword_A:
						while (stack.Count != 0)
							_ = new WindowItem(new IntPtr(stack.Pop())).Close();

						_ = windowGroups.Remove(name);
						break;

					case Keywords.Keyword_R:
						if (stack.Count > 0)
							_ = new WindowItem(new IntPtr(stack.Pop())).Close();

						if (stack.Count > 0 && !windows.Any(w => w.Active))
							_ = new WindowItem(new IntPtr(stack.Peek())).Active = true;

						break;

					case "":
						if (stack.Count > 0)
							_ = new WindowItem(new IntPtr(stack.Pop())).Close();

						if (stack.Count > 0)
							new WindowItem(new IntPtr(stack.ToArray()[stack.Count - 1])).Active = true;

						break;
				}
			}
		}

		public static void GroupDeactivate(params object[] obj)
		{
			var (name, mode) = obj.S2();
			name = (name ?? string.Empty).ToLowerInvariant();

			if (windowGroups.TryGetValue(name, out var group))
			{
				if (group.sc.Count == 0)
					return;

				var windows = SearchWindows($"ahk_group {name}");
				var allwindows = WindowProvider.Manager.FilterForGroups(WindowProvider.Manager.AllWindows.Where(w => !windows.Any(ww => ww.Handle.ToInt64() == w.Handle.ToInt64()))).ToList();

				if (allwindows.Count != 0 && windows.Count == group.deactivated.Count)
					group.deactivated.Clear();

				if (allwindows.Count == 1 && allwindows[0].Handle.ToInt64() == WindowProvider.Manager.GetForeGroundWindowHwnd().ToInt64())
					return;

				if (!mode.Equals(Keywords.Keyword_R, StringComparison.OrdinalIgnoreCase) && windows.Any(w => w.Active))
					allwindows.Reverse();

				foreach (var win in allwindows)
				{
					var h = win.Handle.ToInt64();

					if (!group.deactivated.Contains(h))
					{
						win.Active = true;
						group.deactivated.Push(h);
						group.lastWasDeactivate = true;
						return;
					}
				}
			}
		}

		public static object ListViewGetContent(params object[] obj) => obj.S1O2S3().Splat(ControlProvider.Manager.ListViewGetContent);

		public static void MenuSelect(params object[] obj) => obj.O1S10().Splat(ControlProvider.Manager.MenuSelect);

		public static void PostMessage(params object[] obj) => obj.I3O2S3().Splat(ControlProvider.Manager.PostMessage);

		public static long SendMessage(params object[] obj) => obj.I1O4S3I1(0, 0, 0, null, "", "", "", "", 5000).Splat(ControlProvider.Manager.SendMessage);

		public static void SetControlDelay(object obj) => Accessors.A_ControlDelay = obj;

		public static void SetProcessDPIAware()
		{
			Application.EnableVisualStyles();

			if (!dpimodeset)
			{
				dpimodeset = true;
				Application.SetCompatibleTextRenderingDefault(false);
			}

#if WINDOWS
			_ = Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//_ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
		}

		/// <summary>
		/// Sets the matching behavior of the WinTitle parameter in commands such as WinWait.
		/// This function's behavior is somewhat bizarre in that it changes which global variable gets set
		/// based on the value of the parameter passed in.
		/// </summary>
		/// <param name="obj">String or integers 1, 2, 3, or string RegEx to set TitleMatchMode, else strings fast/slow to set TitleMatchModeSpeed.</param>
		public static void SetTitleMatchMode(params object[] obj)
		{
			var val = obj.S1();

			if (string.Compare(val, "fast", true) == 0 || string.Compare(val, "slow", true) == 0)
				Accessors.A_TitleMatchModeSpeed = val;
			else
				Accessors.A_TitleMatchMode = val;
		}

		public static void SetWinDelay(object obj) => Accessors.A_WinDelay = obj;

		/// <summary>
		/// Retrieves the text from a standard status bar control.
		/// </summary>
		/// <param name="Part">Which part number of the bar to retrieve. Default 1, which is usually the part that contains the text of interest.</param>
		/// <param name="WinTitle">The title or partial title of the target window (the matching behavior is determined by SetTitleMatchMode). If this and the other 3 window parameters are blank or omitted, the Last Found Window will be used. If this is the letter A and the other 3 window parameters are blank or omitted, the active window will be used. To use a window class, specify ahk_class ExactClassName (shown by Window Spy). To use a process identifier (PID), specify ahk_pid %VarContainingPID%. To use a window group, specify ahk_group GroupName. To use a window's unique ID number, specify ahk_id %VarContainingID%. The search can be narrowed by specifying multiple criteria. For example: My File.txt ahk_class Notepad</param>
		/// <param name="WinText">If present, this parameter must be a substring from a single text element of the target window (as revealed by the included Window Spy utility). Hidden text elements are detected if DetectHiddenText is ON.</param>
		/// <param name="ExcludeTitle">Windows whose titles include this value will not be considered.</param>
		/// <param name="ExcludeText">Windows whose text include this value will not be considered.</param>
		/// <returns>The retrieved text</returns>
		public static string StatusBarGetText(params object[] obj)
		{
			var (part, title, text, excludeTitle, excludeText) = obj.I1O1S3();
			WindowItemBase ctrl;
#if WINDOWS

			//These class names will be something else on Linux. Need a cross platform way to do this.//TODO
			if ((ctrl = SearchControl("msctls_statusbar321", title, text, excludeTitle, excludeText, false)) != null)
			{
				var sb = StatusBarProvider.CreateStatusBar(ctrl.Handle);
				part = Math.Max(0, part - 1);

				if (part < sb.Captions.Length)
				{
					WindowItemBase.DoWinDelay();
					return sb.Captions[part];
				}
			}
			else if ((ctrl = SearchControl("WindowsForms10.Window.8.app.0.2b89eaa_r3_ad1", title, text, excludeTitle, excludeText, true)) != null)
			{
				if (Control.FromHandle(ctrl.Handle) is StatusStrip ss)
				{
					part = Math.Max(0, part - 1);

					if (part < ss.Items.Count)
					{
						WindowItemBase.DoWinDelay();
						return ss.Items[part].Text;
					}
				}
			}

#elif LINUX

			if ((ctrl = SearchControl("", title, text, excludeTitle, excludeText, false)) != null)//Unsure of the exact control name on linux.//TODO
			{
				var sb = StatusBarProvider.CreateStatusBar(ctrl.Handle);
				part = Math.Max(0, part - 1);

				if (part < sb.Captions.Length)
				{
					WindowItemBase.DoWinDelay();
					return sb.Captions[part];
				}
			}

#endif
			WindowItemBase.DoWinDelay();
			return "";
		}

		/// <summary>
		/// Waits until a window's status bar contains the specified string.
		/// </summary>
		/// <param name="BarText">
		/// <para>The text or partial text for the which the command will wait to appear. Default is blank (empty), which means to wait for the status bar to become blank. The text is case sensitive and the matching behavior is determined by SetTitleMatchMode, similar to WinTitle below.</para>
		/// <para>To instead wait for the bar's text to change, either use StatusBarGetText in a loop, or use the RegEx example at the bottom of this page.</para>
		/// </param>
		/// <param name="Seconds">The number of seconds (can contain a decimal point) to wait before timing out, in which case Accessors.A_ErrorLevel will be set to 1. Default is blank, which means wait indefinitely. Specifying 0 is the same as specifying 0.5.</param>
		/// <param name="Part">Which part number of the bar to retrieve. Default 1, which is usually the part that contains the text of interest.</param>
		/// <param name="WinTitle">The title or partial title of the target window (the matching behavior is determined by SetTitleMatchMode). If this and the other 3 window parameters are blank or omitted, the Last Found Window will be used. If this is the letter A and the other 3 window parameters are blank or omitted, the active window will be used. To use a window class, specify ahk_class ExactClassName (shown by Window Spy). To use a process identifier (PID), specify ahk_pid %VarContainingPID%. To use a window group, specify ahk_group GroupName. To use a window's unique ID number, specify ahk_id %VarContainingID%. The search can be narrowed by specifying multiple criteria. For example: My File.txt ahk_class Notepad</param>
		/// <param name="WinText">If present, this parameter must be a substring from a single text element of the target window (as revealed by the included Window Spy utility). Hidden text elements are detected if DetectHiddenText is ON.</param>
		/// <param name="Interval">How often the status bar should be checked while the command is waiting (in milliseconds), which can be an expression. Default is 50.</param>
		/// <param name="ExcludeTitle">Windows whose titles include this value will not be considered.</param>
		/// <param name="ExcludeText">Windows whose text include this value will not be considered.</param>
		public static long StatusBarWait(params object[] obj)
		{
			var (bartext, seconds, part, title, text, interval, excludeTitle, excludeText) = obj.S1D1I1O1S1I1S3();
			var winargs = new object[] { title, text, excludeTitle, excludeText };
			var sbargs = new object[] { part, title, text, excludeTitle, excludeText };
			var start = DateTime.Now;
			var matchfound = false;

			if (interval == 0)
				interval = 50;

			do
			{
				var sbtext = StatusBarGetText(sbargs);

				if (sbtext == bartext)
				{
					matchfound = true;
					break;
				}

				if (seconds != 0 && (DateTime.Now - start).TotalSeconds >= seconds)
					break;

				Flow.Sleep(interval);
			} while (true);

			WindowItemBase.DoWinDelay();
			return matchfound ? 1 : 0;
		}

		public static void WinActivate(params object[] obj)
		{
			if (SearchWindow(obj, true) is WindowItem win)
				win.Active = true;
		}

		public static void WinActivateBottom(params object[] obj)
		{
			if (SearchWindow(obj, true, true) is WindowItem win)
				win.Active = true;
		}

		/// <summary>
		/// Returns the Unique ID (HWND) of the active window if it matches the specified criteria.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="excludeTitle"></param>
		/// <param name="excludeText"></param>
		/// <returns></returns>
		public static long WinActive(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var criteria = SearchCriteria.FromString(title, text, excludeTitle, excludeText);
			var window = WindowProvider.Manager.ActiveWindow;
			return (window != null && window.Equals(criteria)) ? window.Handle.ToInt64() : 0L;
		}

		/// <summary>
		/// Closes the specified window.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="excludeTitle"></param>
		/// <param name="excludeText"></param>
		public static void WinClose(params object[] obj)
		{
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2("", "", double.MinValue);
			var (windows, crit) = WindowProvider.Manager.FindWindowGroup(title, text, excludeTitle, excludeText);

			if (crit == null && string.IsNullOrEmpty(crit.Group) && windows.Count == 0 && !IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			foreach (var win in windows)
			{
				_ = win.Close();

				if (seconds != double.MinValue)
					_ = win.WaitClose(seconds == 0 ? 0.5 : seconds);
			}

			WindowItemBase.DoWinDelay();
		}

		/// <summary>
		/// Returns the Unique ID (HWND) of the first matching window (0 if none) as a hexademinal integer.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="excludeTitle"></param>
		/// <param name="excludeText"></param>
		/// <returns></returns>
		public static long WinExist(params object[] obj)
		{
			var win = SearchWindow(obj, false);
			return win != null ? win.Handle.ToInt64() : 0;
		}

		public static long WinGetAlwaysOnTop(params object[] obj) => (SearchWindow(obj, true) is WindowItem win && win.AlwaysOnTop) ? 1L : 0L;

		public static string WinGetClass(params object[] obj) => SearchWindow(obj, true) is WindowItem win ? win.ClassName : "";

		public static void WinGetClientPos(ref object outX, ref object outY, ref object outWidth, ref object outHeight, object title = null, object text = null, object excludeTitle = null, object excludeText = null) =>
		WinPosHelper(true, ref outX, ref outY, ref outWidth, ref outHeight, title, text, excludeTitle, excludeText);

		public static object WinGetControls(params object[] obj) =>
		WinGetControlsHelper(true, obj);

		public static object WinGetControlsHwnd(params object[] obj) =>
		WinGetControlsHelper(false, obj);

		public static long WinGetCount(params object[] obj) => DoDelayedFunc(() => SearchWindows(obj).Count);

		public static long WinGetExStyle(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.ExStyle : 0L);

		public static object WinGetID(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.Handle.ToInt64() : 0L);

		public static long WinGetIDLast(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var (windows, criteria) = WindowProvider.Manager.FindWindowGroup(title, text, excludeTitle, excludeText);

			if (windows != null && windows.Count > 0)
			{
				WindowItemBase.DoWinDelay();
				return windows[ ^ 1].Handle.ToInt64();
			}
			else if (!IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return 0L;
		}

		public static Array WinGetList(params object[] obj) => DoDelayedFunc(() => new Array(SearchWindows(obj).Select(item => item.Handle.ToInt64()).ToList()));

		public static long WinGetMinMax(params object[] obj)
		{
			var val = 0L;

			if (SearchWindow(obj, true) is WindowItem win)
			{
				var state = win.WindowState;

				if (state == FormWindowState.Normal)
					val = 0L;
				else if (state == FormWindowState.Minimized)
					val = -1L;
				else
					val = 1L;

				WindowItemBase.DoWinDelay();
			}

			return val;
		}

		public static object WinGetPID(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.PID.ToInt64() : 0L);

		public static void WinGetPos(ref object outX, ref object outY, ref object outWidth, ref object outHeight, object title = null, object text = null, object excludeTitle = null, object excludeText = null) =>
		WinPosHelper(false, ref outX, ref outY, ref outWidth, ref outHeight, title, text, excludeTitle, excludeText);

		public static string WinGetProcessName(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.ProcessName : "");

		public static string WinGetProcessPath(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.Path : "");

		public static long WinGetStyle(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.Style : 0L);

		public static string WinGetText(params object[] obj) => DoDelayedFunc(() => string.Join(Keywords.Keyword_Linefeed, SearchWindow(obj, true) is WindowItem win ? win.Text : new string[] { "" }));

		public static string WinGetTitle(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true) is WindowItem win ? win.Title : "");

		public static string WinGetTransColor(params object[] obj)
		{
			if (SearchWindow(obj, true) is WindowItem win)
			{
				var color = (int)win.TransparentColor.Al();
				var tempbgr = Color.FromArgb(color);
				color = Color.FromArgb(tempbgr.A, tempbgr.B, tempbgr.G, tempbgr.R).ToArgb();
				WindowItemBase.DoWinDelay();
				return color != int.MinValue ? $"0x{color:X6}" : "";
			}

			return "";
		}

		public static object WinGetTransparent(params object[] obj)
		{
			if (SearchWindow(obj, true) is WindowItem win)
			{
				var color = win.Transparency.Al();
				WindowItemBase.DoWinDelay();
				return color != -1 ? color : "";
			}

			return "";
		}

		public static void WinHide(params object[] obj) => DoDelayedAction(() => SearchWindows(obj).ForEach(win => win.Hide()));

		public static void WinKill(params object[] obj)
		{
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2("", "", double.MinValue);
			var (windows, crit) = WindowProvider.Manager.FindWindowGroup(title, text, excludeTitle, excludeText);

			if (crit == null && string.IsNullOrEmpty(crit.Group) && windows.Count == 0 && !IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			foreach (var win in windows)
			{
				_ = win.Kill();

				if (seconds != double.MinValue)
					_ = win.WaitClose(seconds == 0 ? 0.5 : seconds);
			}

			WindowItemBase.DoWinDelay();
		}

		public static void WinMaximize(params object[] obj) => DoDelayedAction(() => SearchWindows(obj).ForEach(win => win.WindowState = FormWindowState.Maximized));

		public static void WinMinimize(params object[] obj) => DoDelayedAction(() => SearchWindows(obj).ForEach(win => win.WindowState = FormWindowState.Minimized));

		public static void WinMinimizeAll(params object[] obj) => DoDelayedAction(WindowProvider.Manager.MinimizeAll);

		public static void WinMinimizeAllUndo(params object[] obj) => DoDelayedAction(WindowProvider.Manager.MinimizeAllUndo);

		public static void WinMaximizeAll(params object[] obj) => DoDelayedAction(WindowProvider.Manager.MaximizeAll);

		//public static void PrintAllWindowText(params object[] obj)
		//{
		//  //Keysharp.Scripting.Script.OutputDebug($"About to iterate AllWindows in PrintAllWindowText()");
		//  var windows = WindowProvider.Manager.AllWindows;
		//
		//  foreach (var window in windows)
		//      Keysharp.Scripting.Script.OutputDebug(window.Title);
		//}

		public static void WinMove(params object[] obj)
		{
			var (x, y, width, height, title, text, excludeTitle, excludeText) = obj.I4O1S3(int.MinValue, int.MinValue, int.MinValue, int.MinValue);

			if (SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true) is WindowItem win)
			{
				var loc = win.Location;

				if (x != int.MinValue)
					loc.X = x;

				if (y != int.MinValue)
					loc.Y = y;

				var size = win.Size;

				if (width != int.MinValue)
					size.Width = width;

				if (height != int.MinValue)
					size.Height = height;

				win.Location = loc;
				win.Size = size;
				WindowItemBase.DoWinDelay();
			}
		}

		public static void WinMoveBottom(params object[] obj) => DoDelayedAction(() => { if (SearchWindow(obj, true) is WindowItem win) win.Bottom = true; });

		public static void WinMoveTop(params object[] obj) => DoDelayedAction(() => { if (SearchWindow(obj, true) is WindowItem win) win.Bottom = false; });

		public static void WinRedraw(params object[] obj) => DoDelayedAction(() => { if (SearchWindow(obj, true) is WindowItem win) _ = win.Redraw(); });

		public static void WinRestore(params object[] obj) => DoDelayedAction(() => SearchWindows(obj).ForEach(win => win.WindowState = FormWindowState.Normal));

		public static void WinSetAlwaysOnTop(params object[] obj) => WinSetToggleX((win, b) => win.AlwaysOnTop = b, win => win.AlwaysOnTop, obj);

		public static void WinSetEnabled(params object[] obj) => WinSetToggleX((win, b) => win.Enabled = b, win => win.Enabled, obj);

		public static void WinSetExStyle(params object[] obj) => WinSetStyleHelper(obj, true);

		public static void WinSetRegion(params object[] obj)
		{
#if WINDOWS
			var (options, title, text, excludeTitle, excludeText) = obj.S1O1S3();
			var winargs = new object[] { title, text, excludeTitle, excludeText };

			if (!(SearchWindow(winargs, true) is WindowItem win))
				return;

			var opts = Options.ParseOptions(options);
			var w = int.MinValue;
			var h = int.MinValue;
			var rw = 30;
			var rh = 30;
			var ellipse = false;
			var wind = false;
			var points = new List<Point>(16);

			foreach (var opt in opts)
			{
				var tempstr = "";

				if (Options.TryParse(opt, "w", ref w)) { }
				else if (Options.TryParse(opt, "h", ref h)) { }
				else if (string.Compare(opt, "e", true) == 0) { ellipse = true; }
				else if (string.Compare(opt, "Wind", true) == 0) { wind = true; }
				else if (Options.TryParseString(opt, "r", ref tempstr))
				{
					var splits = tempstr.Split('-', StringSplitOptions.None);
					var vals = Conversions.ParseRange(splits);

					if (vals.Count > 0)
						rw = vals[0];

					if (vals.Count > 1)
						rh = vals[1];
				}
				else if (opt.Contains('-'))
				{
					var splits = opt.Split('-', StringSplitOptions.None);
					var vals = Conversions.ParseRange(splits);

					if (vals.Count > 1)
						points.Add(new Point(vals[0], vals[1]));
				}
			}

			var hrgn = IntPtr.Zero;

			if (points.Count == 0)
			{
				if (WindowsAPI.SetWindowRgn(win.Handle, IntPtr.Zero, true) == 0)
					throw new OSError("", $"Could not reset window region with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoWinDelay();
				return;
			}
			else if (w != int.MinValue && h != int.MinValue)
			{
				w += points[0].X;//Make width become the right side of the rect.
				h += points[0].Y;//Make height become the bottom.

				if (ellipse)
					hrgn = WindowsAPI.CreateEllipticRgn(points[0].X, points[0].Y, w, h);
				else if (rw != int.MinValue)
					hrgn = WindowsAPI.CreateRoundRectRgn(points[0].X, points[0].Y, w, h, rw, rh);
				else
					hrgn = WindowsAPI.CreateRectRgn(points[0].X, points[0].Y, w, h);
			}
			else
				hrgn = WindowsAPI.CreatePolygonRgn(points.Select(p => new POINT { x = p.X, y = p.Y }).ToArray(), points.Count, wind ? WindowsAPI.WINDING : WindowsAPI.ALTERNATE);

			if (hrgn != IntPtr.Zero)
			{
				if (WindowsAPI.SetWindowRgn(win.Handle, hrgn, true) == 0)
				{
					_ = WindowsAPI.DeleteObject(hrgn);
					throw new OSError("", $"Could not set region for window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}
			}
			else
				throw new ValueError($"Could not create region for window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			WindowItemBase.DoWinDelay();
#else
			throw new NotImplementedException();
#endif
		}

		public static void WinSetStyle(params object[] obj) => WinSetStyleHelper(obj, false);

		public static void WinSetTitle(params object[] obj)
		{
			var o = obj.Cast<object>();
			var newtitle = obj.S1();

			if (SearchWindow(o.Skip(1).ToArray(), true) is WindowItem win)
			{
				win.Title = newtitle;
				WindowItemBase.DoWinDelay();
			}
		}

		public static void WinSetTransColor(params object[] obj)
		{
			if (SearchWindow(obj.Skip(1).ToArray(), true) is WindowItem win)
			{
				win.TransparentColor = obj[0];
				WindowItemBase.DoWinDelay();
			}
		}

		public static void WinSetTransparent(params object[] obj)
		{
			if (SearchWindow(obj.Skip(1).ToArray(), true) is WindowItem win)
			{
				win.Transparency = obj[0];
				WindowItemBase.DoWinDelay();
			}
		}

		public static void WinShow(params object[] obj)
		{
			var tv = Threads.GetThreadVariables();
			var prev = tv.detectHiddenWindows;
			tv.detectHiddenWindows = true;
			SearchWindows(obj).ForEach(win => win.Show());
			tv.detectHiddenWindows = prev;
			WindowItemBase.DoWinDelay();
		}

		public static long WinWait(params object[] obj)
		{
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2();
			var newargs = new object[] { title, text, excludeTitle, excludeText };
			WindowItemBase win;
			var start = DateTime.Now;

			do
			{
				win = SearchWindow(newargs, false);

				if (win != null || (seconds != 0 && (DateTime.Now - start).TotalSeconds >= seconds))
					break;

				Flow.Sleep(50);
			} while (win == null);

			if (win != null)
				WindowProvider.Manager.LastFound = win;

			WindowItemBase.DoWinDelay();
			return win != null ? win.Handle.ToInt64() : 0L;
		}

		public static long WinWaitActive(params object[] obj)
		{
			var b = false;
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2();
			var start = DateTime.Now;
			var criteria = SearchCriteria.FromString(title, text, excludeTitle, excludeText);
			var hwnd = 0L;

			while (!b && (seconds == 0 || (DateTime.Now - start).TotalSeconds < seconds))
			{
				var windows = WindowProvider.Manager.FindWindowGroup(criteria, true);//Pass true because we must inspect all matching windows to see if any of them are active.

				foreach (var win in windows)
				{
					if (win.Active)
					{
						WindowProvider.Manager.LastFound = win;
						hwnd = win.Handle.ToInt64();
						b = true;
						break;
					}
				}

				if (!b)
					Flow.Sleep(100);
			}

			WindowItemBase.DoWinDelay();
			return hwnd;
		}

		public static long WinWaitClose(params object[] obj)
		{
			var b = false;
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2();
			var start = DateTime.Now;
			var criteria = SearchCriteria.FromString(title, text, excludeTitle, excludeText);
			var windows = WindowProvider.Manager.FindWindowGroup(criteria);

			foreach (var win in windows)//In the case of WinWaitClose(), this loop won't execute and the function will return 1.
			{
				WindowProvider.Manager.LastFound = win;

				while (seconds == 0 || (DateTime.Now - start).TotalSeconds < seconds)
				{
					if (!win.Exists)
					{
						b = true;
						break;
					}
					else
						Flow.Sleep(100);
				}
			}

			WindowItemBase.DoWinDelay();
			return b ? 1L : 0L;
		}

		public static long WinWaitNotActive(params object[] obj)
		{
			var b = false;
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2();
			var start = DateTime.Now;

			if (SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true) is WindowItem win)
			{
				//Keysharp.Scripting.Script.OutputDebug($"The window to wait for is: {win.Handle.ToInt64()}, {win.Title}");
				//Keysharp.Core.File.FileAppend($"The window to wait for is: {win.Handle.ToInt64()}, {win.Title}\n", "out.txt");
				WindowProvider.Manager.LastFound = win;

				while (!b && (seconds == 0 || (DateTime.Now - start).TotalSeconds < seconds))
				{
					if (!win.Active)
					{
						b = true;
						break;
					}

					Flow.Sleep(100);
				}
			}
			WindowItemBase.DoWinDelay();
			return b ? 1L : 0L;
		}

		internal static (bool, IntPtr) CtrlToIntPtr(object ctrl)
		{
			if (ctrl == null)
			{
				return (false, IntPtr.Zero);
			}
			else if (ctrl is long l)
			{
				return (true, new IntPtr(l));
			}
			else if (ctrl is IntPtr ip)
			{
				return (true, ip);
			}
			else if (ctrl is int i)
			{
				return (true, i);
			}
			else if (!(ctrl is string))
			{
				object hwnd = null;

				try
				{
					hwnd = GetPropertyValue(ctrl, "Hwnd");
				}
				catch { }

				var ptr = IntPtr.Zero;

				if (hwnd is long ll)
					ptr = new IntPtr(ll);
				else if (hwnd is IntPtr p)
					ptr = p;
				else if (hwnd is int ii)
					ptr = new IntPtr(ii);

				return (true, ptr);
			}

			return (false, IntPtr.Zero);
		}

		internal static void DoDelayedAction(Action act)
		{
			act();
			WindowItemBase.DoWinDelay();
		}

		internal static T DoDelayedFunc<T>(Func<T> func)
		{
			var val = func();
			WindowItemBase.DoWinDelay();
			return val;
		}

		internal static WindowItemBase SearchControl(object ctrl, object title, string text, string excludeTitle, string excludeText, bool throwifnull = true)
		{
			var (parsed, ptr) = CtrlToIntPtr(ctrl);

			if (parsed)
			{
				if (WindowProvider.Manager.IsWindow(ptr))
					return WindowProvider.Manager.CreateWindow(ptr);
				else if (throwifnull && !IsMainWindowClosing)
					throw new TargetError($"Could not find child control with handle: {ptr}");
				else
					return null;
			}

			var parent = SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);

			if (ctrl == null)
				return parent;

			var sc = new SearchCriteria();
			string classortext = null;
			string s = ctrl as string;

			if (!string.IsNullOrEmpty(s))
			{
				var isClass = s.Length > 0 && char.IsDigit(s[ ^ 1]);

				if (isClass)
					sc.ClassName = s;
				else
					sc.Text = s;

				classortext = s;
			}

			var childitem = parent.FirstChild(sc);

			if (classortext != null && childitem == null)
			{
				if (string.IsNullOrEmpty(sc.Text))
				{
					sc.Text = sc.ClassName;
					sc.ClassName = "";
				}
				else
				{
					sc.ClassName = sc.Text;
					sc.Text = "";
				}

				childitem = parent.FirstChild(sc);

				if (childitem == null)//Final attempt, just use title.
				{
					if (string.IsNullOrEmpty(sc.Text))
					{
						sc.Title = sc.ClassName;
						sc.ClassName = "";
					}
					else
					{
						sc.Title = sc.Text;
						sc.Text = "";
					}

					childitem = parent.FirstChild(sc);
				}
			}

			if (childitem == null && throwifnull && !IsMainWindowClosing)
				throw new TargetError("Could not find child control using text or class name match \"" + s + $"\" in window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");//Can't use interpolated string here because the AStyle formatter misinterprets it.
			return childitem;
		}

		internal static WindowItemBase SearchWindow(object[] obj, bool throwifnull, bool last = false)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var win = WindowProvider.Manager.FindWindow(title, text, excludeTitle, excludeText, last);

			if (win == null && throwifnull && !IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return win;
		}

		internal static List<WindowItemBase> SearchWindows(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var (windows, crit) = WindowProvider.Manager.FindWindowGroup(title, text, excludeTitle, excludeText);
			return windows;
		}

		private static object WinGetControlsHelper(bool nn, params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var win = WindowProvider.Manager.FindWindow(title, text, excludeTitle, excludeText);

			if (win != null)
			{
				var controls = win.ChildWindows;

				if (controls.Count == 0)
					return "";

				var arr = new Array(controls.Count);

				if (nn)
				{
					foreach (var ctrl in controls)
						_ = arr.Add(ctrl.ClassNN);
				}
				else
				{
					foreach (var ctrl in controls)
						_ = arr.Add(ctrl.Handle.ToInt64());
				}

				WindowItemBase.DoWinDelay();
				return arr;
			}
			else if (!IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return "";
		}

		private static void WinPosHelper(bool client, ref object outX, ref object outY, ref object outWidth, ref object outHeight, object title, object text, object excludeTitle, object excludeText)
		{
			var obj = new[] { title, text, excludeTitle, excludeText };
			//DoDelayedFunc(() =>
			{
				if (SearchWindow(obj, true) is WindowItem win)
				{
					var scale = 1.0;// / Accessors.A_ScaledScreenDPI;
					var rect = client ? win.ClientLocation : win.Location;

					if (client)
					{
						var pt = win.ClientToScreen();
						outX = (long)(scale * (rect.Left + pt.X));
						outY = (long)(scale * (rect.Top + pt.Y));
					}
					else
					{
						outX = (long)(scale * rect.Left);
						outY = (long)(scale * rect.Top);
					}

					outWidth  = (long)(rect.Width * scale);
					outHeight = (long)(rect.Height * scale);
				}
				else
				{
					outX = 0L;
					outY = 0L;
					outWidth = 0L;
					outHeight = 0L;
				}
			}//);
		}

		private static void WinSetStyleHelper(object[] o, bool ex)
		{
			if (SearchWindow(o.Skip(1).ToArray(), true) is WindowItem win)
			{
				var val = o[0];

				if (ex)
				{
					if (val is int i)
						win.ExStyle = i;
					else if (val is uint ui)
						win.ExStyle = ui;
					else if (val is long l)
						win.ExStyle = l;
					else if (val is double d)
						win.ExStyle = (long)d;
					else if (val is string s)
					{
						long temp = 0;

						if (Options.TryParse(s, "+", ref temp)) { win.ExStyle |= temp; }
						else if (Options.TryParse(s, "-", ref temp)) { win.ExStyle &= ~temp; }
						else if (Options.TryParse(s, "^", ref temp)) { win.ExStyle ^= temp; }
						else win.ExStyle = val.ParseLong(true).Value;
					}
				}
				else
				{
					if (val is int i)
						win.Style = i;
					else if (val is uint ui)
						win.Style = ui;
					else if (val is long l)
						win.Style = l;
					else if (val is double d)
						win.Style = (long)d;
					else if (val is string s)
					{
						long temp = 0;

						if (Options.TryParse(s, "+", ref temp)) { win.Style |= temp; }
						else if (Options.TryParse(s, "-", ref temp)) { win.Style &= ~temp; }
						else if (Options.TryParse(s, "^", ref temp)) { win.Style ^= temp; }
						else win.Style = val.ParseLong(true).Value;
					}
				}

				WindowItemBase.DoWinDelay();
			}
		}

		private static void WinSetToggleX(Action<WindowItemBase, bool> set, Func<WindowItemBase, bool> get, params object[] obj)
		{
			var (val, title, text, excludeTitle, excludeText) = obj.I1O1S3();

			if (SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true) is WindowItem win)
			{
				if (val == 0)
					set(win, false);
				else if (val == 1)
					set(win, true);
				else if (val == -1)
					set(win, !get(win));

				WindowItemBase.DoWinDelay();
			}
		}
	}
}