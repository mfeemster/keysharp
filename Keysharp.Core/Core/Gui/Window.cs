using System.Windows.Forms;

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

		public static long ControlAddItem(object @string,
										  object control,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null) => ControlProvider.Manager.ControlAddItem(
											  @string.As(),
											  control,
											  winTitle,
											  winText,
											  excludeTitle,
											  excludeText);

		public static object ControlChooseIndex(object n,
												object control,
												object winTitle = null,
												object winText = null,
												object excludeTitle = null,
												object excludeText = null)
		{
			ControlProvider.Manager.ControlChooseIndex(
				n.Ai(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static long ControlChooseString(object @string,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null) => ControlProvider.Manager.ControlChooseString(
													   @string.As(),
													   control,
													   winTitle,
													   winText,
													   excludeTitle,
													   excludeText);

		public static object ControlClick(object ctrlOrPos = null,
										  object title = null,
										  object text = null,
										  object whichButton = null,
										  object clickCount = null,
										  object options = null,
										  object excludeTitle = null,
										  object excludeText = null)
		{
			ControlProvider.Manager.ControlClick(
				ctrlOrPos,
				title,
				text,
				whichButton.As(),
				clickCount.Ai(1),
				options.As(),
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlDeleteItem(object n,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			ControlProvider.Manager.ControlDeleteItem(
				n.Ai(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static long ControlFindItem(object @string,
										   object control,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => ControlProvider.Manager.ControlFindItem(
											   @string.As(),
											   control,
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static object ControlFocus(object control,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null)
		{
			ControlProvider.Manager.ControlFocus(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static long ControlGetChecked(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => ControlProvider.Manager.ControlGetChecked(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static string ControlGetChoice(object control,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null) => ControlProvider.Manager.ControlGetChoice(
													  control,
													  winTitle,
													  winText,
													  excludeTitle,
													  excludeText);

		public static string ControlGetClassNN(object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null) => ControlProvider.Manager.ControlGetClassNN(
													   control,
													   winTitle,
													   winText,
													   excludeTitle,
													   excludeText);

		public static long ControlGetEnabled(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => ControlProvider.Manager.ControlGetEnabled(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static long ControlGetExStyle(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => ControlProvider.Manager.ControlGetExStyle(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static long ControlGetFocus(object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => ControlProvider.Manager.ControlGetFocus(
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static long ControlGetHwnd(object control,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null) => ControlProvider.Manager.ControlGetHwnd(
											  control,
											  winTitle,
											  winText,
											  excludeTitle,
											  excludeText);

		public static long ControlGetIndex(object control,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => ControlProvider.Manager.ControlGetIndex(
											   control,
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static Array ControlGetItems(object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) => ControlProvider.Manager.ControlGetItems(
												control,
												winTitle,
												winText,
												excludeTitle,
												excludeText);

		public static object ControlGetPos([Optional()][DefaultParameterValue(0)] ref object outX,
										   [Optional()][DefaultParameterValue(0)] ref object outY,
										   [Optional()][DefaultParameterValue(0)] ref object outWidth,
										   [Optional()][DefaultParameterValue(0)] ref object outHeight,
										   object ctrl = null, object title = null, object text = null, object excludeTitle = null, object excludeText = null)
		{
			ControlProvider.Manager.ControlGetPos(
				ref outX,
				ref outY,
				ref outWidth,
				ref outHeight,
				ctrl,
				title?.ToString(),
				text?.ToString(),
				excludeTitle?.ToString(),
				excludeText?.ToString());
			return null;
		}

		public static long ControlGetStyle(object control,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null) => ControlProvider.Manager.ControlGetStyle(
											   control,
											   winTitle,
											   winText,
											   excludeTitle,
											   excludeText);

		public static string ControlGetText(object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) => ControlProvider.Manager.ControlGetText(
												control,
												winTitle,
												winText,
												excludeTitle,
												excludeText);

		public static long ControlGetVisible(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => ControlProvider.Manager.ControlGetVisible(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static object ControlHide(object control,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			ControlProvider.Manager.ControlHide(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlHideDropDown(object control,
				object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null)
		{
			ControlProvider.Manager.ControlHideDropDown(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlMove(object x = null,
										 object y = null,
										 object width = null,
										 object height = null,
										 object control = null,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			ControlProvider.Manager.ControlMove(
				x.Ai(int.MinValue),
				y.Ai(int.MinValue),
				width.Ai(int.MinValue),
				height.Ai(int.MinValue),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSend(object keys,
										 object control = null,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			ControlProvider.Manager.ControlSend(
				keys.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSendText(object keys,
											 object control = null,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null)
		{
			ControlProvider.Manager.ControlSendText(
				keys.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSetChecked(object newSetting,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			ControlProvider.Manager.ControlSetChecked(
				newSetting,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSetEnabled(object newSetting,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			ControlProvider.Manager.ControlSetEnabled(
				newSetting,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSetExStyle(object value,
											   object control,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			ControlProvider.Manager.ControlSetExStyle(
				value,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSetStyle(object value,
											 object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null)
		{
			ControlProvider.Manager.ControlSetStyle(
				value,
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlSetText(object newText,
											object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null)
		{
			ControlProvider.Manager.ControlSetText(
				newText.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlShow(object control,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			ControlProvider.Manager.ControlShow(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object ControlShowDropDown(object control,
				object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null)
		{
			ControlProvider.Manager.ControlShowDropDown(
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object DetectHiddenText(object mode)
		{
			var oldVal = Accessors.A_DetectHiddenText;
			Accessors.A_DetectHiddenText = mode;
			return oldVal;
		}

		public static object DetectHiddenWindows(object mode)
		{
			var oldVal = Accessors.A_DetectHiddenWindows;
			Accessors.A_DetectHiddenWindows = mode;
			return oldVal;
		}

		public static long EditGetCurrentCol(object control,
											 object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => ControlProvider.Manager.EditGetCurrentCol(
													 control,
													 winTitle,
													 winText,
													 excludeTitle,
													 excludeText);

		public static long EditGetCurrentLine(object control,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null) => ControlProvider.Manager.EditGetCurrentLine(
													  control,
													  winTitle,
													  winText,
													  excludeTitle,
													  excludeText);

		public static string EditGetLine(object n,
										 object control,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null) => ControlProvider.Manager.EditGetLine(
											 n.Ai(),
											 control,
											 winTitle,
											 winText,
											 excludeTitle,
											 excludeText);

		public static long EditGetLineCount(object control,
											object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) => ControlProvider.Manager.EditGetLineCount(
												control,
												winTitle,
												winText,
												excludeTitle,
												excludeText);

		public static string EditGetSelectedText(object control,
				object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null) => ControlProvider.Manager.EditGetSelectedText(
					control,
					winTitle,
					winText,
					excludeTitle,
					excludeText);

		public static object EditPaste(object @string,
									   object control,
									   object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null)
		{
			ControlProvider.Manager.EditPaste(
				@string.As(),
				control,
				winTitle,
				winText,
				excludeTitle,
				excludeText);
			return null;
		}

		public static long GroupActivate(object groupName, object mode = null)
		{
			var name = groupName.As().ToLowerInvariant();
			var m = mode.As();

			if (windowGroups.TryGetValue(name, out var group))
			{
				if (group.sc.Count == 0)
					return 0L;

				var windows = SearchWindows($"ahk_group {name}");

				if (windows.Count != 0 && windows.Count == group.activated.Count)
					group.activated.Clear();

				if (windows.Count == 1 && windows[0].Handle.ToInt64() == WindowProvider.Manager.GetForeGroundWindowHwnd().ToInt64())
					return 0L;

				if (!m.Equals(Keyword_R, StringComparison.OrdinalIgnoreCase) && !windows.Any(w => w.Active))
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

		public static object GroupAdd(object groupName,
									  object winTitle = null,
									  object winText = null,
									  object excludeTitle = null,
									  object excludeText = null)
		{
			var name = groupName.As();

			if (string.IsNullOrEmpty(name))
				return null;

			if (!windowGroups.ContainsKey(name))
				windowGroups.Add(name, new WindowGroup());

			if (name != "AllWindows")
			{
				var group = windowGroups[name];
				group.sc.Add(SearchCriteria.FromString(winTitle, winText, excludeTitle, excludeText));
				group.activated.Clear();
				group.deactivated.Clear();
			}

			return null;
		}

		public static object GroupClose(object groupName, object mode = null)
		{
			var name = groupName.As().ToLowerInvariant();
			var m = mode.As();

			if (windowGroups.TryGetValue(name, out var group))
			{
				if (group.sc.Count == 0)
					return null;

				var stack = group.lastWasDeactivate ? group.deactivated : group.activated;
				var windows = SearchWindows($"ahk_group {name}");

				switch (m.ToLowerInvariant())
				{
					case Keyword_A:
						while (stack.Count != 0)
							_ = new WindowItem(new IntPtr(stack.Pop())).Close();

						_ = windowGroups.Remove(name);
						break;

					case Keyword_R:
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

			return null;
		}

		public static object GroupDeactivate(object groupName, object mode = null)
		{
			var name = groupName.As().ToLowerInvariant();
			var m = mode.As();

			if (windowGroups.TryGetValue(name, out var group))
			{
				if (group.sc.Count == 0)
					return null;

				var windows = SearchWindows($"ahk_group {name}");
				var allwindows = WindowProvider.Manager.FilterForGroups(WindowProvider.Manager.AllWindows.Where(w => !windows.Any(ww => ww.Handle.ToInt64() == w.Handle.ToInt64()))).ToList();

				if (allwindows.Count != 0 && windows.Count == group.deactivated.Count)
					group.deactivated.Clear();

				if (allwindows.Count == 1 && allwindows[0].Handle.ToInt64() == WindowProvider.Manager.GetForeGroundWindowHwnd().ToInt64())
					return null;

				if (!m.Equals(Keyword_R, StringComparison.OrdinalIgnoreCase) && windows.Any(w => w.Active))
					allwindows.Reverse();

				foreach (var win in allwindows)
				{
					var h = win.Handle.ToInt64();

					if (!group.deactivated.Contains(h))
					{
						win.Active = true;
						group.deactivated.Push(h);
						group.lastWasDeactivate = true;
						return null;
					}
				}
			}

			return null;
		}

		public static object ListViewGetContent(object options = null,
												object control = null,
												object winTitle = null,
												object winText = null,
												object excludeTitle = null,
												object excludeText = null) => ControlProvider.Manager.ListViewGetContent(
														options.As(),
														control,
														winTitle,
														winText,
														excludeTitle,
														excludeText);

		public static object MenuSelect(object winTitle,
										object winText,
										object menu,
										object subMenu1 = null,
										object subMenu2 = null,
										object subMenu3 = null,
										object subMenu4 = null,
										object subMenu5 = null,
										object subMenu6 = null,
										object excludeTitle = null,
										object excludeText = null)
		{
			ControlProvider.Manager.MenuSelect(
				winTitle,
				winText,
				menu,
				subMenu1,
				subMenu2,
				subMenu3,
				subMenu4,
				subMenu5,
				subMenu6,
				excludeTitle,
				excludeText);
			return null;
		}

		public static object PostMessage(object msg,
										 object wparam = null,
										 object lparam = null,
										 object control = null,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			ControlProvider.Manager.PostMessage(
				msg.Ai(),
				wparam.Ai(),
				lparam.Ai(),
				control,
				winTitle,
				winText.As(),
				excludeTitle.As(),
				excludeText.As());
			return null;
		}

		public static long SendMessage(object msg,
									   object wparam = null,
									   object lparam = null,
									   object control = null,
									   object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null,
									   object timeout = null) => ControlProvider.Manager.SendMessage(
										   msg.Ai(),
										   wparam,
										   lparam,
										   control,
										   winTitle,
										   winText.As(),
										   excludeTitle.As(),
										   excludeText.As(),
										   timeout.Ai(5000));

		public static object SetControlDelay(object obj)
		{
			var oldVal = Accessors.A_ControlDelay = obj;
			Accessors.A_ControlDelay = obj;
			return oldVal;
		}

		public static object SetProcessDPIAware()
		{
#if LINUX//Don't have Gtk working on Windows yet, but just in case we ever get it working.//TODO
			Environment.SetEnvironmentVariable("MONO_VISUAL_STYLES", "gtkplus");//This must come first.
#endif
			Application.EnableVisualStyles();

			if (!dpimodeset)
			{
				dpimodeset = true;

				try
				{
					Application.SetCompatibleTextRenderingDefault(false);
				}
				catch { } // Fails if a window already exists, like when running from Keyview
			}

#if WINDOWS
			_ = Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
			//_ = Application.SetHighDpiMode(HighDpiMode.SystemAware);
#endif
			return null;
		}

		/// <summary>
		/// Sets the matching behavior of the WinTitle parameter in commands such as WinWait.
		/// This function's behavior is somewhat bizarre in that it changes which global variable gets set
		/// based on the value of the parameter passed in.
		/// </summary>
		/// <param name="obj">String or integers 1, 2, 3, or string RegEx to set TitleMatchMode, else strings fast/slow to set TitleMatchModeSpeed.</param>
		public static object SetTitleMatchMode(object matchModeSpeed)
		{
			var val = matchModeSpeed.As();

			if (string.Compare(val, "fast", true) == 0 || string.Compare(val, "slow", true) == 0)
				Accessors.A_TitleMatchModeSpeed = val;
			else
				Accessors.A_TitleMatchMode = val;

			return null;
		}

		public static object SetWinDelay(object delay)
		{
			var oldVal = Accessors.A_WinDelay;
			Accessors.A_WinDelay = delay;
			return oldVal;
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
		public static string StatusBarGetText(object partNumber = null,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null)
		{
			WindowItemBase ctrl;
			var part = partNumber.Ai(1);
			var text = winText.As();
			var title = excludeTitle.As();
			var exclude = excludeText.As();
#if WINDOWS

			//These class names will be something else on Linux. Need a cross platform way to do this.//TODO
			if ((ctrl = SearchControl("msctls_statusbar321", winTitle, text, title, exclude, false)) != null)
			{
				var sb = StatusBarProvider.CreateStatusBar(ctrl.Handle);
				part = Math.Max(0, part - 1);

				if (part < sb.Captions.Length)
				{
					WindowItemBase.DoWinDelay();
					return sb.Captions[part];
				}
			}
			else if ((ctrl = SearchControl("WindowsForms10.Window.8.app.0.2b89eaa_r3_ad1", winTitle, text, title, exclude, true)) != null)
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

			if ((ctrl = SearchControl("", winTitle, text, title, exclude, false)) != null)//Unsure of the exact control name on linux.//TODO
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
		public static long StatusBarWait(object barText = null,
										 object timeout = null,
										 object partNumber = null,
										 object winTitle = null,
										 object winText = null,
										 object interval = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			var bartext = barText.As();
			var seconds = timeout.Ai();
			var intvl = interval.Ai();
			var start = DateTime.Now;
			var matchfound = false;

			if (intvl == 0)
				intvl = 50;

			do
			{
				var sbtext = StatusBarGetText(partNumber, winTitle, winText, excludeTitle, excludeText);

				if (sbtext == bartext)
				{
					matchfound = true;
					break;
				}

				if (seconds != 0 && (DateTime.Now - start).TotalSeconds >= seconds)
					break;

				_ = Flow.Sleep(interval);
			} while (true);

			WindowItemBase.DoWinDelay();
			return matchfound ? 1 : 0;
		}

		public static object WinActivate(object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
				win.Active = true;

			return null;
		}

		public static object WinActivateBottom(object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true, true) is WindowItem win)
				win.Active = true;

			return null;
		}

		/// <summary>
		/// Returns the Unique ID (HWND) of the active window if it matches the specified criteria.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="excludeTitle"></param>
		/// <param name="excludeText"></param>
		/// <returns></returns>
		public static long WinActive(object winTitle = null,
									 object winText = null,
									 object excludeTitle = null,
									 object excludeText = null)
		{
			var criteria = SearchCriteria.FromString(winTitle, winText, excludeTitle, excludeText);
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
		public static object WinClose(object winTitle = null,
									  object winText = null,
									  object secondsToWait = null,
									  object excludeTitle = null,
									  object excludeText = null)
		{
			var seconds = secondsToWait.Ad(double.MinValue);
			var (windows, crit) = WindowProvider.Manager.FindWindowGroup(winTitle, winText, excludeTitle, excludeText);

			if (crit == null && string.IsNullOrEmpty(crit.Group) && windows.Count == 0 && !IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			foreach (var win in windows)
			{
				_ = win.Close();

				if (seconds != double.MinValue)
					_ = win.WaitClose(seconds == 0 ? 0.5 : seconds);
			}

			WindowItemBase.DoWinDelay();
			return null;
		}

		/// <summary>
		/// Returns the Unique ID (HWND) of the first matching window (0 if none) as a hexademinal integer.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="text"></param>
		/// <param name="excludeTitle"></param>
		/// <param name="excludeText"></param>
		/// <returns></returns>
		public static long WinExist(object winTitle = null,
									object winText = null,
									object excludeTitle = null,
									object excludeText = null)
		{
			var win = SearchWindow(winTitle, winText, excludeTitle, excludeText, false);
			return win != null ? win.Handle.ToInt64() : 0;
		}

		public static long WinGetAlwaysOnTop(object winTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null) => (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win && win.AlwaysOnTop) ? 1L : 0L;

		public static string WinGetClass(object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null) => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.ClassName : "";

		public static object WinGetClientPos([Optional()][DefaultParameterValue(0)] ref object outX,
											 [Optional()][DefaultParameterValue(0)] ref object outY,
											 [Optional()][DefaultParameterValue(0)] ref object outWidth,
											 [Optional()][DefaultParameterValue(0)] ref object outHeight,
											 object wintTitle = null,
											 object winText = null,
											 object excludeTitle = null,
											 object excludeText = null)
		{
			WinPosHelper(true, ref outX, ref outY, ref outWidth, ref outHeight, wintTitle, winText, excludeTitle, excludeText);
			return null;
		}

		public static object WinGetControls(object winTitle = null,
											object winText = null,
											object excludeTitle = null,
											object excludeText = null) =>
		WinGetControlsHelper(true, winTitle, winText, excludeTitle, excludeText);

		public static object WinGetControlsHwnd(object winTitle = null,
												object winText = null,
												object excludeTitle = null,
												object excludeText = null) =>
		WinGetControlsHelper(false, winTitle, winText, excludeTitle, excludeText);

		public static long WinGetCount(object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null) =>
		DoDelayedFunc(() => SearchWindows(winTitle, winText, excludeTitle, excludeText).Count);

		public static long WinGetExStyle(object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.ExStyle : 0L);

		public static object WinGetID(object winTitle = null,
									  object winText = null,
									  object excludeTitle = null,
									  object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.Handle.ToInt64() : 0L);

		public static long WinGetIDLast(object winTitle = null,
										object winText = null,
										object excludeTitle = null,
										object excludeText = null)
		{
			var (windows, criteria) = WindowProvider.Manager.FindWindowGroup(winTitle, winText, excludeTitle, excludeText);

			if (windows != null && windows.Count > 0)
			{
				WindowItemBase.DoWinDelay();
				return windows[ ^ 1].Handle.ToInt64();
			}
			else if (!IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return 0L;
		}

		public static Array WinGetList(object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null) =>
		DoDelayedFunc(() => new Array(SearchWindows(winTitle, winText, excludeTitle, excludeText).Select(item => item.Handle.ToInt64()).ToList()));

		public static long WinGetMinMax(object winTitle = null,
										object winText = null,
										object excludeTitle = null,
										object excludeText = null)
		{
			var val = 0L;

			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
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

		public static object WinGetPID(object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.PID : 0L);

		public static object WinGetPos([Optional()][DefaultParameterValue(0)] ref object outX,
									   [Optional()][DefaultParameterValue(0)] ref object outY,
									   [Optional()][DefaultParameterValue(0)] ref object outWidth,
									   [Optional()][DefaultParameterValue(0)] ref object outHeight,
									   object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null)
		{
			WinPosHelper(false, ref outX, ref outY, ref outWidth, ref outHeight, winTitle, winText, excludeTitle, excludeText);
			return null;
		}

		public static string WinGetProcessName(object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.ProcessName : "");

		public static string WinGetProcessPath(object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.Path : "");

		public static long WinGetStyle(object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.Style : 0L);

		public static string WinGetText(object winTitle = null,
										object winText = null,
										object excludeTitle = null,
										object excludeText = null) =>
		DoDelayedFunc(() => string.Join(Keyword_Linefeed, SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.Text : [""]));

		public static string WinGetTitle(object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null) =>
		DoDelayedFunc(() => SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win ? win.Title : "");

		public static string WinGetTransColor(object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				var color = (int)win.TransparentColor.Al();
				var tempbgr = Color.FromArgb(color);
				color = Color.FromArgb(tempbgr.A, tempbgr.B, tempbgr.G, tempbgr.R).ToArgb();
				WindowItemBase.DoWinDelay();
				return color != int.MinValue ? $"0x{color:X6}" : "";
			}

			return "";
		}

		public static object WinGetTransparent(object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				var color = win.Transparency.Al();
				WindowItemBase.DoWinDelay();
				return color != -1 ? color : "";
			}

			return "";
		}

		public static object WinHide(object winTitle = null,
									 object winText = null,
									 object excludeTitle = null,
									 object excludeText = null)
		{
			DoDelayedAction(() => SearchWindows(winTitle, winText, excludeTitle, excludeText).ForEach(win => win.Hide()));
			return null;
		}

		public static object WinKill(object winTitle = null,
									 object winText = null,
									 object secondsToWait = null,
									 object excludeTitle = null,
									 object excludeText = null)
		{
			var seconds = secondsToWait.Ad(double.MinValue);
			var (windows, crit) = WindowProvider.Manager.FindWindowGroup(winTitle, winText, excludeTitle, excludeText);

			if (crit == null && string.IsNullOrEmpty(crit.Group) && windows.Count == 0 && !IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			foreach (var win in windows)
			{
				_ = win.Kill();

				if (seconds != double.MinValue)
					_ = win.WaitClose(seconds == 0 ? 0.5 : seconds);
			}

			WindowItemBase.DoWinDelay();
			return null;
		}

		public static object WinMaximize(object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			DoDelayedAction(() => SearchWindows(winTitle, winText, excludeTitle, excludeText).ForEach(win => win.WindowState = FormWindowState.Maximized));
			return null;
		}

		public static object WinMaximizeAll()
		{
			DoDelayedAction(WindowProvider.Manager.MaximizeAll);
			return null;
		}

		public static object WinMinimize(object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			DoDelayedAction(() => SearchWindows(winTitle, winText, excludeTitle, excludeText).ForEach(win => win.WindowState = FormWindowState.Minimized));
			return null;
		}

		public static object WinMinimizeAll()
		{
			DoDelayedAction(WindowProvider.Manager.MinimizeAll);
			return null;
		}

		public static object WinMinimizeAllUndo(params object[] obj)
		{
			DoDelayedAction(WindowProvider.Manager.MinimizeAllUndo);
			return null;
		}

		public static object WinMove(object x = null,
									 object y = null,
									 object width = null,
									 object height = null,
									 object winTitle = null,
									 object winText = null,
									 object excludeTitle = null,
									 object excludeText = null)
		{
			var _x = x.Ai(int.MinValue);
			var _y = y.Ai(int.MinValue);
			var w = width.Ai(int.MinValue);
			var h = height.Ai(int.MinValue);

			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				var loc = win.Location;

				if (_x != int.MinValue)
					loc.X = _x;

				if (_y != int.MinValue)
					loc.Y = _y;

				var size = win.Size;

				if (w != int.MinValue)
					size.Width = w;

				if (h != int.MinValue)
					size.Height = h;

				win.Location = loc;
				win.Size = size;
				WindowItemBase.DoWinDelay();
			}

			return null;
		}

		public static object WinMoveBottom(object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null)
		{
			DoDelayedAction(() => { if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win) win.Bottom = true; });
			return null;
		}
		public static object WinMoveTop(object winTitle = null,
										object winText = null,
										object excludeTitle = null,
										object excludeText = null)
		{
			DoDelayedAction(() => { if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win) win.Bottom = false; });
			return null;
		}
		public static object WinRedraw(object winTitle = null,
									   object winText = null,
									   object excludeTitle = null,
									   object excludeText = null)
		{
			DoDelayedAction(() => { if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win) _ = win.Redraw(); });
			return null;
		}

		public static object WinRestore(object winTitle = null,
										object winText = null,
										object excludeTitle = null,
										object excludeText = null)
		{
			DoDelayedAction(() => SearchWindows(winTitle, winText, excludeTitle, excludeText).ForEach(win => win.WindowState = FormWindowState.Normal));
			return null;
		}

		public static object WinSetAlwaysOnTop(object newSetting,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			WinSetToggleX((win, b) => win.AlwaysOnTop = b, win => win.AlwaysOnTop, newSetting, winTitle, winText, excludeTitle, excludeText);
			return null;
		}
		public static object WinSetEnabled(object newSetting,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null)
		{
			WinSetToggleX((win, b) => win.Enabled = b, win => win.Enabled, newSetting, winTitle, winText, excludeTitle, excludeText);
			return null;
		}

		public static object WinSetExStyle(object value,
										   object winTitle = null,
										   object winText = null,
										   object excludeTitle = null,
										   object excludeText = null)
		{
			WinSetStyleHelper(true, value, winTitle, winText, excludeTitle, excludeText);
			return null;
		}
#if WINDOWS
		public static object WinSetRegion(object options,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null)
		{
			var opts = options.As();

			if (!(SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win))
				return null;

			var w = int.MinValue;
			var h = int.MinValue;
			var rw = 30;
			var rh = 30;
			var ellipse = false;
			var wind = false;
			var points = new List<Point>(16);

			foreach (Range r in opts.AsSpan().SplitAny(SpaceTabSv))
			{
				var tempstr = "";
				var opt = opts.AsSpan(r).Trim();

				if (Options.TryParse(opt, "w", ref w)) { }
				else if (Options.TryParse(opt, "h", ref h)) { }
				else if (opt.Equals("e", StringComparison.OrdinalIgnoreCase)) { ellipse = true; }
				else if (opt.Equals("Wind", StringComparison.OrdinalIgnoreCase)) { wind = true; }
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
					var splits = opt.ToString().Split('-', StringSplitOptions.None);
					var vals = Conversions.ParseRange(splits);

					if (vals.Count > 1)
						points.Add(new Point(vals[0], vals[1]));
				}
			}

			var hrgn = IntPtr.Zero;

			if (points.Count == 0)
			{
				if (WindowsAPI.SetWindowRgn(win.Handle, IntPtr.Zero, true) == 0)
					throw new OSError("", $"Could not reset window region with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

				WindowItemBase.DoWinDelay();
				return null;
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
					throw new OSError("", $"Could not set region for window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");
				}
			}
			else
				throw new ValueError($"Could not create region for window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			WindowItemBase.DoWinDelay();
			return null;
		}
#endif
		public static object WinSetStyle(object value,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			WinSetStyleHelper(false, value, winTitle, winText, excludeTitle, excludeText);
			return null;
		}
		public static object WinSetTitle(object newTitle,
										 object winTitle = null,
										 object winText = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				win.Title = newTitle.As();
				WindowItemBase.DoWinDelay();
			}

			return null;
		}
		public static object WinSetTransColor(object color,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				win.TransparentColor = color;
				WindowItemBase.DoWinDelay();
			}

			return null;
		}
		public static object WinSetTransparent(object n,
											   object winTitle = null,
											   object winText = null,
											   object excludeTitle = null,
											   object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				win.Transparency = n;
				WindowItemBase.DoWinDelay();
			}

			return null;
		}
		public static object WinShow(object winTitle = null,
									 object winText = null,
									 object excludeTitle = null,
									 object excludeText = null)
		{
			var tv = Threads.GetThreadVariables();
			var prev = tv.detectHiddenWindows;
			tv.detectHiddenWindows = true;
			SearchWindows(winTitle, winText, excludeTitle, excludeText).ForEach(win => win.Show());
			tv.detectHiddenWindows = prev;
			WindowItemBase.DoWinDelay();
			return null;
		}
		public static long WinWait(object winTitle = null,
								   object winText = null,
								   object timeout = null,
								   object excludeTitle = null,
								   object excludeText = null)
		{
			var seconds = timeout.Ad();
			WindowItemBase win;
			var start = DateTime.Now;

			do
			{
				win = SearchWindow(winTitle, winText, excludeTitle, excludeText, false);

				if (win != null || (seconds != 0 && (DateTime.Now - start).TotalSeconds >= seconds))
					break;

				_ = Flow.Sleep(50);
			} while (win == null);

			if (win != null)
				WindowProvider.Manager.LastFound = win;

			WindowItemBase.DoWinDelay();
			return win != null ? win.Handle.ToInt64() : 0L;
		}
		public static long WinWaitActive(object winTitle = null,
										 object winText = null,
										 object timeout = null,
										 object excludeTitle = null,
										 object excludeText = null)
		{
			var b = false;
			var seconds = timeout.Ad();
			var start = DateTime.Now;
			var criteria = SearchCriteria.FromString(winTitle, winText, excludeTitle, excludeText);
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
					_ = Flow.Sleep(100);
			}

			WindowItemBase.DoWinDelay();
			return hwnd;
		}
		public static long WinWaitClose(object winTitle = null,
										object winText = null,
										object timeout = null,
										object excludeTitle = null,
										object excludeText = null)
		{
			var b = false;
			var seconds = timeout.Ad();
			var start = DateTime.Now;
			var criteria = SearchCriteria.FromString(winTitle, winText, excludeTitle, excludeText);
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
						_ = Flow.Sleep(100);
				}
			}

			WindowItemBase.DoWinDelay();
			return b ? 1L : 0L;
		}
		public static long WinWaitNotActive(object winTitle = null,
											object winText = null,
											object timeout = null,
											object excludeTitle = null,
											object excludeText = null)
		{
			var b = false;
			var seconds = timeout.Ad();
			var start = DateTime.Now;

			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
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

					_ = Flow.Sleep(100);
				}
			}

			WindowItemBase.DoWinDelay();
			return b ? 1L : 0L;
		}
#if LINUX
		[PublicForTestOnly]
		public static long zzzLinuxTester(params object[] obj)
		{
			return 1L;
		}
#endif
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
		internal static WindowItemBase SearchControl(object ctrl, object title, object text, object excludeTitle, object excludeText, bool throwifnull = true)
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

			var parent = SearchWindow(title, text, excludeTitle, excludeText, true);

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
		internal static WindowItemBase SearchWindow(object winTitle,
				object winText,
				object excludeTitle,
				object excludeText,
				bool throwifnull,
				bool last = false)
		{
			var win = WindowProvider.Manager.FindWindow(winTitle, winText, excludeTitle, excludeText, last);

			if (win == null && throwifnull && !IsMainWindowClosing)
				throw new TargetError($"Could not find window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return win;
		}
		internal static List<WindowItemBase> SearchWindows(object winTitle = null,
				object winText = null,
				object excludeTitle = null,
				object excludeText = null)
		{
			var (windows, crit) = WindowProvider.Manager.FindWindowGroup(winTitle, winText, excludeTitle, excludeText);
			return windows;
		}
		private static object WinGetControlsHelper(bool nn,
				object winTitle,
				object winText,
				object excludeTitle,
				object excludeText)
		{
			var win = WindowProvider.Manager.FindWindow(winTitle, winText, excludeTitle, excludeText);

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
				throw new TargetError($"Could not find window with criteria: title: {winTitle}, text: {winText}, exclude title: {excludeTitle}, exclude text: {excludeText}");

			return "";
		}
		private static void WinPosHelper(bool client,
										 ref object outX,
										 ref object outY,
										 ref object outWidth,
										 ref object outHeight,
										 object winTitle,
										 object winText,
										 object excludeTitle,
										 object excludeText)
		{
			//DoDelayedFunc(() =>
			{
				if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
				{
					var rect = client ? win.ClientLocation : win.Location;

					if (client)
					{
						var pt = win.ClientToScreen();
						outX = (long)(rect.Left + pt.X);
						outY = (long)(rect.Top + pt.Y);
					}
					else
					{
						outX = (long)rect.Left;
						outY = (long)rect.Top;
					}

					outWidth  = (long)rect.Width;
					outHeight = (long)rect.Height;
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
		private static void WinSetStyleHelper(bool ex,
											  object value,
											  object winTitle = null,
											  object winText = null,
											  object excludeTitle = null,
											  object excludeText = null)
		{
			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
			{
				var val = value;

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
		private static void WinSetToggleX(Action<WindowItemBase, bool> set, Func<WindowItemBase, bool> get,
										  object value,
										  object winTitle = null,
										  object winText = null,
										  object excludeTitle = null,
										  object excludeText = null)
		{
			var val = value.Ai();

			if (SearchWindow(winTitle, winText, excludeTitle, excludeText, true) is WindowItem win)
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