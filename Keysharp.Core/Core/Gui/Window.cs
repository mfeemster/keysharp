using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Keysharp.Core.Common.Window;
using Keysharp.Core.Windows;//This will make this file only work on windows. Need a way to reference linux.windows.//MATT
using Keysharp.Scripting;

namespace Keysharp.Core
{
	public static class Window
	{
		private static bool dpimodeset;

		internal static ControlManagerBase ControlManager => ControlManagerProvider.Instance;

		/// <summary>
		/// easy access to the WindowManager
		/// </summary>
		internal static WindowManagerBase WindowManager => WindowManagerProvider.Instance;

		/// <summary>
		/// easy access to the window groups
		/// </summary>
		private static Dictionary<string, WindowGroup> windowGroups => WindowManagerProvider.Instance.Groups;

		public static long ControlAddItem(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ControlAddItem);

		public static void ControlChooseIndex(params object[] obj) => obj.I1O2S3().Splat(ControlManager.ControlChooseIndex);

		public static long ControlChooseString(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ControlChooseString);

		public static void ControlClick(params object[] obj) => obj.O2S2I1S3().Splat(ControlManager.ControlClick);

		public static void ControlDeleteItem(params object[] obj) => obj.I1O2S3().Splat(ControlManager.ControlDeleteItem);

		public static long ControlFindItem(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ControlFindItem);

		public static void ControlFocus(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlFocus);

		public static long ControlGetChecked(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetChecked);

		public static string ControlGetChoice(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetChoice);

		public static string ControlGetClassNN(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetClassNN);

		public static long ControlGetEnabled(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetEnabled);

		public static long ControlGetExStyle(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetExStyle);

		public static long ControlGetFocus(params object[] obj) => obj.O1S3().Splat(ControlManager.ControlGetFocus);

		public static long ControlGetHwnd(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetHwnd);

		public static long ControlGetIndex(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetIndex);

		public static Array ControlGetItems(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetItems);

		public static Rectangle ControlGetPos(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetPos);

		public static long ControlGetStyle(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetStyle);

		public static string ControlGetText(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetText);

		public static long ControlGetVisible(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlGetVisible);

		public static void ControlHide(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlHide);

		public static void ControlHideDropDown(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlHideDropDown);

		public static void ControlMove(params object[] obj) => obj.I4O2S3(int.MinValue, int.MinValue, int.MinValue, int.MinValue).Splat(ControlManager.ControlMove);

		public static void ControlSend(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ControlSend);

		public static void ControlSendText(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ControlSendText);

		public static void ControlSetChecked(params object[] obj) => obj.O3S3().Splat(ControlManager.ControlSetChecked);

		public static void ControlSetEnabled(params object[] obj) => obj.O3S3().Splat(ControlManager.ControlSetEnabled);

		public static void ControlSetStyle(params object[] obj) => obj.O3S3().Splat(ControlManager.ControlSetStyle);

		public static void ControlSetStyleEx(params object[] obj) => obj.O3S3().Splat(ControlManager.ControlSetStyleEx);

		public static void ControlSetText(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ControlSetText);

		public static void ControlShow(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlShow);

		public static void ControlShowDropDown(params object[] obj) => obj.O2S3().Splat(ControlManager.ControlShowDropDown);

		public static void DetectHiddenText(params object[] obj) => Accessors.A_DetectHiddenText = obj.O1();

		public static void DetectHiddenWindows(params object[] obj) => Accessors.A_DetectHiddenWindows = obj.O1();

		public static long EditGetCurrentCol(params object[] obj) => obj.O2S3().Splat(ControlManager.EditGetCurrentCol);

		public static long EditGetCurrentLine(params object[] obj) => obj.O2S3().Splat(ControlManager.EditGetCurrentLine);

		public static string EditGetLine(params object[] obj) => obj.I1O2S3().Splat(ControlManager.EditGetLine);

		public static long EditGetLineCount(params object[] obj) => obj.O2S3().Splat(ControlManager.EditGetLineCount);

		public static string EditGetSelectedText(params object[] obj) => obj.O2S3().Splat(ControlManager.EditGetSelectedText);

		public static void EditPaste(params object[] obj) => obj.S1O2S3().Splat(ControlManager.EditPaste);

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

				if (windows.Count == 1 && windows[0].Handle.ToInt64() == WindowManager.ActiveWindow.Handle.ToInt64())
					return 0L;

				if (!mode.Equals(Core.Keyword_R, System.StringComparison.OrdinalIgnoreCase) && !windows.Any(w => w.Active))
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

			name = name.ToLowerInvariant();

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
					case Core.Keyword_A:
						while (stack.Count != 0)
							_ = new WindowItem(new IntPtr(stack.Pop())).Close();

						_ = windowGroups.Remove(name);
						break;

					case Core.Keyword_R:
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
				var allwindows = WindowManager.FilterForGroups(WindowManager.AllWindows.Where(w => !windows.Any(ww => ww.Handle.ToInt64() == w.Handle.ToInt64()))).ToList();

				if (allwindows.Count != 0 && windows.Count == group.deactivated.Count)
					group.deactivated.Clear();

				if (allwindows.Count == 1 && allwindows[0].Handle.ToInt64() == WindowManager.ActiveWindow.Handle.ToInt64())
					return;

				if (!mode.Equals(Core.Keyword_R, System.StringComparison.OrdinalIgnoreCase) && windows.Any(w => w.Active))
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

		public static object ListViewGetContent(params object[] obj) => obj.S1O2S3().Splat(ControlManager.ListViewGetContent);

		public static void MenuSelect(params object[] obj) => obj.O1S10().Splat(ControlManager.MenuSelect);

		public static void PostMessage(params object[] obj) => obj.I3O2S3().Splat(ControlManager.PostMessage);

		public static long SendMessage(params object[] obj) => obj.I1O4S3I1(0, 0, 0, 0, 0, "", "", "", 5000).Splat(ControlManager.SendMessage);

		public static void SetProcessDPIAware()
		{
			Application.EnableVisualStyles();

			if (!dpimodeset)
			{
				dpimodeset = true;
				Application.SetCompatibleTextRenderingDefault(false);
			}

			System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);
			//if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			//{
			//  _ = WindowsAPI.SetProcessDPIAware();
			//}
		}

		/// <summary>
		/// Sets the matching behavior of the WinTitle parameter in commands such as WinWait.
		/// This function's behavior is somewhat bizarre in that it changes which global variable gets set
		/// based on the value of the parameter passed in.
		/// </summary>
		/// <param name="obj">String or integers 1, 2, 3, or string RegEx to set TitleMatchMode, else strings fast/slow to set TitleMatchModeSpeed.</param>
		public static void SetTitleMatchMode(params object[] obj)
		{
			var val = obj.S1().ToLowerInvariant();

			if (val == "fast" || val == "slow")
				Accessors.A_TitleMatchModeSpeed = val;
			else
				Accessors.A_TitleMatchMode = val;
		}

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
			var win = WindowManager.FindWindow(title, text, excludeTitle, excludeText);

			if (SearchControl(win, "msctls_statusbar321", false) is WindowItemBase ctrl)//This class name will be something else on Linux.//MATT
			{
				var sb = new StatusBar(ctrl.Handle);
				part = Math.Max(0, part - 1);

				if (part < sb.Captions.Length)
				{
					WindowItemBase.DoWinDelay();
					return sb.Captions[part];
				}
			}
			else
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

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

				System.Threading.Thread.Sleep(interval);
			} while (true);

			WindowItemBase.DoWinDelay();
			return matchfound ? 1 : 0;
		}

		public static void WinActivate(params object[] obj) => SearchWindow(obj, true).Active = true;

		public static void WinActivateBottom(params object[] obj) => SearchWindow(obj, true, true).Active = true;

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
			var window = WindowManager.ActiveWindow;
			return (window != null && window.Equals(criteria)) ? window.Handle.ToInt64() : 0;
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
			var (windows, crit) = WindowManager.FindWindowGroup(title, text, excludeTitle, excludeText);

			if (crit == null && string.IsNullOrEmpty(crit.Group) && windows.Count == 0)
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

		public static string WinGetClass(params object[] obj) => SearchWindow(obj, true).ClassName;

		public static Rectangle WinGetClientPos(params object[] obj) => SearchWindow(obj, true).ClientLocation;

		public static object WinGetControls(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var win = WindowManager.FindWindow(title, text, excludeTitle, excludeText);

			if (win != null)
			{
				var controls = win.ChildWindows;

				if (controls.Count == 0)
					return "";

				var arr = new Array(controls.Count);

				foreach (var ctrl in controls)
					_ = arr.Add(ctrl.ClassNN);

				WindowItemBase.DoWinDelay();
				return arr;
			}
			else
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
		}

		public static object WinGetControlsHwnd(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var win = WindowManager.FindWindow(title, text, excludeTitle, excludeText);

			if (win != null)
			{
				var controls = win.ChildWindows;

				if (controls.Count == 0)
					return "";

				var arr = new Array(controls.Count);

				foreach (var ctrl in controls)
					_ = arr.Add(ctrl.Handle.ToInt64());

				WindowItemBase.DoWinDelay();
				return arr;
			}
			else
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
		}

		public static long WinGetCount(params object[] obj) => DoDelayedFunc(() => SearchWindows(obj).Count);

		public static long WinGetExStyle(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).ExStyle);

		public static object WinGetID(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).Handle.ToInt64());

		public static long WinGetIDLast(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var (windows, criteria) = WindowManager.FindWindowGroup(title, text, excludeTitle, excludeText, true);

			if (windows != null && windows.Count > 0)
			{
				WindowItemBase.DoWinDelay();
				return windows[ ^ 1].Handle.ToInt64();
			}
			else
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");
		}

		public static Array WinGetList(params object[] obj) => DoDelayedFunc(() => new Array(SearchWindows(obj).Select(item => item.Handle.ToInt64()).ToList()));

		public static long WinGetMinMax(params object[] obj)
		{
			var state = SearchWindow(obj, true).WindowState;
			var val = 0;

			if (state == FormWindowState.Normal)
				val = 0;
			else if (state == FormWindowState.Minimized)
				val = -1;
			else
				val = 1;

			WindowItemBase.DoWinDelay();
			return val;
		}

		public static object WinGetPID(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).PID.ToInt64());

		public static Rectangle WinGetPos(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).Location);

		public static string WinGetProcessName(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).ProcessName);

		public static string WinGetProcessPath(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).Path);

		public static long WinGetStyle(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).Style);

		public static string WinGetText(params object[] obj) => DoDelayedFunc(() => string.Join(Core.Keyword_Linefeed, SearchWindow(obj, true).Text));

		public static string WinGetTitle(params object[] obj) => DoDelayedFunc(() => SearchWindow(obj, true).Title);

		public static string WinGetTransColor(params object[] obj)
		{
			var win = SearchWindow(obj, true);
			var color = win.TransparentColor;
			WindowItemBase.DoWinDelay();
			return color != -1 ? $"0x{color.ToString("X").Substring(0, 6)}" : "";
		}

		public static object WinGetTransparent(params object[] obj)
		{
			var win = SearchWindow(obj, true);
			var color = win.Transparency;
			WindowItemBase.DoWinDelay();
			return color != -1 ? (object)color : "";
		}

		public static void WinHide(params object[] obj) => DoDelayedAction(() => SearchWindows(obj, true).ForEach(win => win.Hide()));

		public static void WinKill(params object[] obj)
		{
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2("", "", double.MinValue);
			var (windows, crit) = WindowManager.FindWindowGroup(title, text, excludeTitle, excludeText);

			if (crit == null && string.IsNullOrEmpty(crit.Group) && windows.Count == 0)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			foreach (var win in windows)
			{
				_ = win.Kill();

				if (seconds != double.MinValue)
					_ = win.WaitClose(seconds == 0 ? 0.5 : seconds);
			}

			WindowItemBase.DoWinDelay();
		}

		public static void WinMaximize(params object[] obj) => DoDelayedAction(() => SearchWindows(obj, true).ForEach(win => win.WindowState = FormWindowState.Maximized));

		public static void WinMinimize(params object[] obj) => DoDelayedAction(() => SearchWindows(obj, true).ForEach(win => win.WindowState = FormWindowState.Minimized));

		public static void WinMinimizeAll(params object[] obj) => DoDelayedAction(() => WindowManager.MinimizeAll());

		public static void WinMinimizeAllUndo(params object[] obj) => DoDelayedAction(() => WindowManager.MinimizeAllUndo());

		public static void WinMove(params object[] obj)
		{
			var (x, y, width, height, title, text, excludeTitle, excludeText) = obj.I4O1S3(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
			var win = SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);
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

		public static void WinMoveBottom(params object[] obj) => DoDelayedAction(() => SearchWindow(obj, true).Bottom = true);

		public static void WinMoveTop(params object[] obj) => DoDelayedAction(() => SearchWindow(obj, true).Bottom = false);

		public static void WinRedraw(params object[] obj) => DoDelayedAction(() => SearchWindow(obj, true).Redraw());

		public static void WinRestore(params object[] obj) => DoDelayedAction(() => SearchWindows(obj, true).ForEach(win => win.WindowState = FormWindowState.Normal));

		public static void WinSetAlwaysOnTop(params object[] obj) => WinSetToggleX((win, b) => win.AlwaysOnTop = b, win => win.AlwaysOnTop, obj);

		public static void WinSetEnabled(params object[] obj) => WinSetToggleX((win, b) => win.Enabled = b, win => win.Enabled, obj);

		public static void WinSetRegion(params object[] obj)//This function is entirely windows specific.
		{
			var (options, title, text, excludeTitle, excludeText) = obj.S1O1S3();
			var winargs = new object[] { title, text, excludeTitle, excludeText };
			var win = SearchWindow(winargs, true);
			var opts = Options.ParseOptions(options);
			var w = int.MinValue;
			var h = int.MinValue;
			var rw = 30;
			var rh = 30;
			var ellipse = false;
			//var polygon = false;//Is this ever used?//TODO
			//var rectangle = false;//Is this ever used?//TODO
			var wind = false;
			var points = new List<Point>(16);

			foreach (var opt in opts)
			{
				var tempstr = "";

				if (Options.TryParse(opt, "w", ref w)) { }
				else if (Options.TryParse(opt, "h", ref h)) { }
				else if (string.Compare(opt, "e", true) == 0) { ellipse = true; }
				//else if (string.Compare(opt, "Rectangle", true) == 0) { rectangle = true; }
				//else if (string.Compare(opt, "Polygon", true) == 0) { polygon = true; }
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
				w += points[0].X;   // Make width become the right side of the rect.
				h += points[0].Y;  // Make height become the bottom.

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
					_ = WindowsAPI.DeleteObject(hrgn);
			}
			else
				throw new OSError("", $"Could not set window region with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			WindowItemBase.DoWinDelay();
		}

		public static void WinSetTitle(params object[] obj)
		{
			var l = obj.L();
			var o = l.Cast<object>();
			var newtitle = l.S1();
			SearchWindow(o.Skip(1).ToArray(), true).Title = newtitle;
			WindowItemBase.DoWinDelay();
		}

		public static void WinShow(params object[] obj)
		{
			var prev = Accessors.A_DetectHiddenWindows;
			Accessors.A_DetectHiddenWindows = true;
			SearchWindows(obj, true).ForEach(win => win.Show());
			Accessors.A_DetectHiddenWindows = prev;
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

				System.Threading.Thread.Sleep(50);
			} while (win == null);

			WindowItemBase.DoWinDelay();
			return win != null ? win.Handle.ToInt64() : 0;
		}

		public static long WinWaitActive(params object[] obj) => WinWaitX((win, seconds) => win.WaitActive(seconds), obj);

		public static long WinWaitClose(params object[] obj) => WinWaitX((win, seconds) => win.WaitClose(seconds), obj);

		public static long WinWaitNotActive(params object[] obj) => WinWaitX((win, seconds) => win.WaitNotActive(seconds), obj);

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

		/// <summary>
		/// Taken from AHK.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="className"></param>
		/// <returns></returns>
		internal static WindowItemBase SearchControl(WindowItemBase parent, object obj, bool throwifnull = true)
		{
			if (parent == null || obj == null)
				return null;

			var isClass = false;
			var sc = new SearchCriteria();
			string classortext = null;

			if (obj is long l)
			{
				sc.ID = new IntPtr(l);
			}
			else if (obj is string s)
			{
				isClass = char.IsDigit(s[ ^ 1]);

				if (isClass)
					sc.ClassName = s;
				else
					sc.Text = s;

				classortext = s;
			}
			else
			{
				var hwnd = Script.InternalGetPropertyValue(obj, "Hwnd");

				if (hwnd.Item1 is long ll)
					sc.ID = new IntPtr(ll);
				else if (hwnd.Item1 is IntPtr ptr)
					sc.ID = ptr;
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

			if (childitem == null && throwifnull)
				throw new TargetError($"Could not find child control in window title: {parent.Title}");

			return childitem;
		}

		internal static WindowItemBase SearchWindow(object[] obj, bool throwifnull, bool last = false)
		{
			var l = obj.L();
			var (title, text, excludeTitle, excludeText) = l.O1S3();
			var win = WindowManager.FindWindow(title, text, excludeTitle, excludeText, last);

			if (win == null && throwifnull)
				throw new TargetError($"Could not find window with criteria: title: {title}, text: {text}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return win;
		}

		internal static List<WindowItemBase> SearchWindows(params object[] obj)
		{
			var (title, text, excludeTitle, excludeText) = obj.O1S3();
			var (windows, crit) = WindowManager.FindWindowGroup(title, text, excludeTitle, excludeText, true);
			return windows;
		}

		private static void WinSetToggleX(Action<WindowItemBase, bool> set, Func<WindowItemBase, bool> get, params object[] obj)
		{
			var (val, title, text, excludeTitle, excludeText) = obj.I1O1S3();
			var win = SearchWindow(new object[] { title, text, excludeTitle, excludeText }, true);

			if (val == 0)
				set(win, false);
			else if (val == 1)
				set(win, true);
			else if (val == -1)
				set(win, !get(win));

			WindowItemBase.DoWinDelay();
		}

		private static long WinWaitX(Func<WindowItemBase, double, bool> func, params object[] obj)
		{
			var b = true;
			SearchCriteria crit;
			List<WindowItemBase> windows;
			var (title, text, seconds, excludeTitle, excludeText) = obj.O1S1D1S2();

			do
			{
				(windows, crit) = WindowManager.FindWindowGroup(title, text, excludeTitle, excludeText, true);

				foreach (var win in windows)
					b &= func(win, seconds);
			} while (windows.Count > 0);

			WindowItemBase.DoWinDelay();
			return b ? 1 : 0;
		}
	}
}