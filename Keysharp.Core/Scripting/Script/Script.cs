namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static bool ForceKeybdHook;
		public static uint MaxThreadsTotal = 12u;
		public static bool NoTrayIcon = false;
		public static bool ValidateThenExit;
		public static bool WinActivateForce = false;
		internal const int INTERVAL_UNSPECIFIED = int.MinValue + 303;
		internal const int SLEEP_INTERVAL = 10;
		internal const int SLEEP_INTERVAL_HALF = SLEEP_INTERVAL / 2;
		internal static List<IFuncObj> ClipFunctions = [];
		internal static List<IFuncObj> hotCriterions = [];
		internal static IntPtr hotExprLFW = IntPtr.Zero;
		internal static List<IFuncObj> hotExprs = [];
		internal static InputType input;
		internal static int inputBeforeHotkeysCount;
		internal static DateTime inputTimeoutAt = DateTime.Now;
		internal static bool inputTimerExists;
		internal static DateTime lastPeekTime;
		internal static MainWindow mainWindow;
		internal static Gui mainWindowGui;
		internal static int maxThreadsLimit = 0xFF;
		internal static MenuType menuIsVisible = MenuType.None;
		internal static PlatformManagerBase mgr = PlatformProvider.Manager;
		internal static List<IFuncObj> onErrorHandlers;
		internal static List<IFuncObj> onExitHandlers = [];
		internal static Icon pausedIcon;
		internal static bool persistent;
		internal static IntPtr playbackHook = IntPtr.Zero;
		internal static DateTime priorHotkeyStartTime = DateTime.Now;
		internal static string scriptName = "";
		internal static Icon suspendedIcon;
		internal static string thisHotkeyName, priorHotkeyName;
		internal static DateTime thisHotkeyStartTime = DateTime.Now;
		internal static DateTime timeLastInputKeyboard = timeLastInputPhysical;
		internal static DateTime timeLastInputMouse = timeLastInputPhysical;
		internal static DateTime timeLastInputPhysical = DateTime.Now;
		internal static int totalExistingThreads;
		internal static int uninterruptibleTime = 17;
		internal static Dictionary<nint, GCHandle> gcHandles = [];
		private static bool isReadyToExecute;
		private static IntPtr mainWindowHandle;

		public static Variables Vars { get; private set; }

		internal static HookThread HookThread { get; private set; }

		internal static IntPtr HwndLastUsed
		{
			get => Threads.GetThreadVariables().hwndLastUsed;
			set => Threads.GetThreadVariables().hwndLastUsed = value;
		}

		internal static bool IsMainWindowClosing => mainWindow == null || mainWindow.IsClosing;

		internal static bool IsReadyToExecute => isReadyToExecute;

		internal static IntPtr MainWindowHandle
		{
			get
			{
				if (mainWindow == null)
					return IntPtr.Zero;

				if (mainWindowHandle == IntPtr.Zero)
					_ = mainWindow.Invoke(() => mainWindowHandle = mainWindow.Handle);

				return mainWindowHandle;
			}
		}

		static Script()
		{
			if (Vars == null)
				Vars = new Variables();

			_ = InitHook();//Why is this always being initialized even when there are no hooks? This is very inefficient.//TODO
		}

		public static string GetPublicStaticPropertyNames()
		{
			var l1 = Reflections.flatPublicStaticMethods.Keys.ToList();
			l1.AddRange(Reflections.flatPublicStaticProperties.Keys);
			var hs = new HashSet<string>(l1);
			return string.Join(' ', hs);
		}

        public static bool HandleSingleInstance(string title, eScriptInstance inst)
        {
            if (title.Length == 0 || title == "*")//Happens when running in Keyview.
                return false;

            if (Env.FindCommandLineArg("force") != null || Env.FindCommandLineArg("f") != null)
                inst = eScriptInstance.Off;

            if (Env.FindCommandLineArg("restart") != null || Env.FindCommandLineArg("r") != null)
                inst = eScriptInstance.Force;

            title = MakeTitleWithVersion(title);
            var exit = false;
            var oldDetect = WindowX.DetectHiddenWindows(true);
            var oldMatchMode = WindowX.SetTitleMatchMode(3);//Require exact match.

            switch (inst)
            {
                case eScriptInstance.Force:
                    {
                        _ = WindowX.WinClose(title, "", 2);
                    }
                    break;

                case eScriptInstance.Ignore:
                    if (WindowX.WinExist(title) != 0)
                        exit = true;

                    break;

                case eScriptInstance.Off:
                    break;

                case eScriptInstance.Prompt:
                default:
                    var hwnd = WindowX.WinExist(title);

                    if (hwnd != 0)
                    {
                        if (Dialogs.MsgBox("Do you want to close the existing instance before running this one?\nYes to exit that instance, No to exit this instance.", "", "YesNo") == "Yes")
                            _ = WindowX.WinClose(hwnd, "", 2);
                        else
                            exit = true;
                    }

                    break;
            }

            _ = WindowX.SetTitleMatchMode(oldMatchMode);
            _ = WindowX.DetectHiddenWindows(oldDetect);
            return exit;
        }

        public static void RunMainWindow(string title, Func<object> userInit, bool persistent)
		{
			mainWindow = new MainWindow();

			if (!string.IsNullOrEmpty(title))
				mainWindow.Text = MakeTitleWithVersion(title);

			mainWindow.ClipboardUpdate += PrivateClipboardUpdate;
			mainWindow.Icon = Core.Properties.Resources.Keysharp_ico;
			Script.persistent = persistent;
			mainWindowGui = new Gui(null, null, null, mainWindow);
			mainWindow.AllowShowDisplay = false; // Prevent show on script startup
			mainWindow.ShowInTaskbar = true; // Without this the main window won't have a taskbar icon
			_ = mainWindow.BeginInvoke(() =>
			{
				if (!Flow.TryCatch(() =>
			{
				var (__pushed, __btv) = Threads.BeginThread();
					_ = userInit();
					//HotkeyDefinition.ManifestAllHotkeysHotstringsHooks() will be called inside of userInit() because it
					//must be done:
					//  After the window handle is created and the handle isn't valid until mainWindow.Load() is called.
					//  Also right after all hotkeys and hotstrings are created.
					isReadyToExecute = true;
					_ = Threads.EndThread(__pushed);
				}, true))//Pop on exception because EndThread() above won't be called.
				{
					if (!Script.persistent)//An exception was thrown so the generated ExitApp() call in _ks_UserMainCode() will not have been called, so call it here.
					{
						_ = Flow.ExitApp(1);
					}
				}
				ExitIfNotPersistent();
			});
			Application.Run(mainWindow);
		}

		public static void SetName(string s) => scriptName = s;

		public static void SetReady() => isReadyToExecute = true;

		//public static void TestSomething()
		//{
		//  char[] SpaceTab = " \t".ToCharArray();
		//  SearchValues<char> SpaceTabSv = SearchValues.Create(SpaceTab);
		//  //var splits = val.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		//  var width = int.MinValue;
		//  var height = int.MinValue;
		//  var icon = "";
		//  object iconnumber = 0;
		//  var filename = "";// splits.Last();
		//  var val = "*w100 *h200 *icon2 C:\a filename.jpg";

		//  //for (var i = 0; i < splits.Length - 1; i++)
		//  foreach (Range r in val.AsSpan().SplitAny(SpaceTabSv))
		//  {
		//      var opt = val.AsSpan(r).Trim();

		//      if (Options.TryParse(opt, "*w", ref width)) { }
		//      else if (Options.TryParse(opt, "*h", ref height)) { }
		//      else if (Options.TryParseString(opt, "*icon", ref icon)) { iconnumber = ImageHelper.PrepareIconNumber(icon); }
		//      else
		//      {
		//          filename = val.Substring(r.Start.Value);
		//          break;
		//      }
		//  }
		//}

		/*
		    internal static string GetVariableInfo()
		    {
		    var sb = new StringBuilder(2048);
		    var stack = new StackTrace(false).GetFrames();

		    for (var i = stack.Length - 1; i >= 0; i--)
		    {
		        if (stack[i] != null &&
		                stack[i].GetMethod() != null &&
		                stack[i].GetMethod().DeclaringType.Attributes.HasFlag(TypeAttributes.Public))//Public is the script, everything else should be hidden.
		        {
		            if (stack[i].GetMethod().DeclaringType.Namespace != null &&
		                    stack[i].GetMethod().DeclaringType.Namespace.StartsWith("Keysharp", StringComparison.OrdinalIgnoreCase))
		            {
		                var meth = stack[i].GetMethod();
		                _ = sb.AppendLine($"Class: {meth.ReflectedType.Name}");
		                _ = sb.AppendLine();

		                foreach (var v in meth.ReflectedType.GetProperties(BindingFlags.Public | BindingFlags.Static))
		                {
		                    var val = v.GetValue(null);
		                    var varstr = $"\t{val?.GetType()} {v.Name}: ";

		                    if (val is string s)
		                        varstr += $"[{s.Length}] {s.Substring(0, Math.Min(s.Length, 60))}";
		                    else if (val is Keysharp.Core.Array arr)
		                    {
		                        var ct = Math.Min(100, arr.Count);
		                        var tempsb = new StringBuilder(ct * 100);

		                        for (var a = 1; a <= ct; a++)
		                        {
		                            var tempstr = arr[a].ToString();
		                            _ = tempsb.Append(tempstr.Substring(0, Math.Min(tempstr.Length, 60)));

		                            if (a < ct)
		                                _ = tempsb.Append(", ");
		                        }

		                        varstr += tempsb.ToString();
		                    }
		                    else if (val is Keysharp.Core.Map map)
		                    {
		                        var ct = Math.Min(100, map.Count);
		                        var a = 0;
		                        var tempsb = new StringBuilder(ct * 100);
		                        _ = tempsb.Append('{');

		                        foreach (var kv in map.map)
		                        {
		                            var tempstr = kv.Value.ToString();
		                            _ = tempsb.Append($"{kv.Key} : {tempstr.Substring(0, Math.Min(tempstr.Length, 60))}");

		                            if (++a < ct)
		                                _ = tempsb.Append(", ");
		                        }

		                        _ = tempsb.Append('}');
		                        varstr += tempsb.ToString();
		                    }
		                    else if (val == null)
		                        varstr += "null";
		                    else
		                        varstr += val.ToString();

		                    _ = sb.AppendLine(varstr);
		                }

		                _ = sb.AppendLine("");
		                _ = sb.AppendLine($"Method: {meth.Name}");
		                var mb = stack[i].GetMethod().GetMethodBody();

		                foreach (var lvi in mb.LocalVariables)
		                    _ = sb.AppendLine($"\t{lvi}");

		                _ = sb.AppendLine("--------------------------------------------------");
		                _ = sb.AppendLine();
		            }
		        }
		    }

		    return sb.ToString();
		    }
		*/

		[PublicForTestOnly]
		public static void SimulateKeyPress(uint key) => HookThread.SimulateKeyPress(key);

		public static void VerifyVersion(string ver, bool plus, int line, string code)
		{
			var ahkver = A_AhkVersion;
			var reqvers = ParseVersionToInts(ver);
			var thisvers = ParseVersionToInts(ahkver);

			for (var i = 0; i < 4; i++)
			{
				if (plus)
				{
					if (reqvers[i] > thisvers[i])
						throw new ParseException($"This script requires Keysharp >= v{ver}, but you have v{ahkver}", line, code);
				}
				else if (reqvers[i] != thisvers[i])
					throw new ParseException($"This script requires Keysharp == v{ver}, but you have v{ahkver}", line, code);

				if (thisvers[i] > reqvers[i])
					break;
			}
		}

		public static void WaitThreads()
		{
			//Check against 1 instead of 0, because this may be launched in a thread as a result of a hotkey.
			//If this gets stuck in a loop it means we have a thread imbalance/mismatch somewhere.
			//We added them, but never removed. While seemingly dangerous to have, it's a handy
			//way to know we've found a bug.
			while (totalExistingThreads > 1)
				_ = Flow.Sleep(200);
		}

		internal static bool AnyPersistent()
		{
			if (Gui.AnyExistingVisibleWindows())
				return true;

			if (HotkeyDefinition.shk.Count > 0)
				return true;

			if (HotstringManager.shs.Count > 0)
				return true;

			if (!Flow.timers.IsEmpty)
				return true;

			if (ClipFunctions.Count > 0)
				return true;

			if (totalExistingThreads > 0)
				return true;

			if (Flow.persistentValueSetByUser)
				return true;

			if (input != null)
			{
				for (var input = Script.input; ; input = input.prev)
				{
					if (input != null)
						return true;
				}
			}

			return false;
		}

		public static void ExitIfNotPersistent(Flow.ExitReasons exitReason = Flow.ExitReasons.Exit)
		{
			//Must use BeginInvoke() because this might be called from _ks_UserMainCode(),
			//so it needs to run after that thread has exited.
			if (!IsMainWindowClosing)
				mainWindow?.CheckedBeginInvoke(new Action(() =>
			{
				if (!IsMainWindowClosing && !AnyPersistent())
					_ = Flow.ExitAppInternal(exitReason, Environment.ExitCode, false);
			}), true, true);
		}

		internal static bool InitHook()
		{
			if (HookThread != null)
				return false;

#if WINDOWS
			HookThread = new WindowsHookThread();
			return true;
#elif LINUX
			HookThread = new Keysharp.Core.Linux.LinuxHookThread();
			return true;
#endif
		}

		internal static ResultType IsCycleComplete(int aSleepDuration, DateTime aStartTime, bool aAllowEarlyReturn)
		// This function is used just to make MsgSleep() more readable/understandable.
		{
			var kbdMouseSender = HookThread.kbdMsSender;//This should always be non-null if any hotkeys/strings are present.
			// Note: Even if TickCount has wrapped due to system being up more than about 49 days,
			// DWORD subtraction still gives the right answer as long as aStartTime itself isn't more
			// than about 49 days ago. Note: must cast to int or any negative result will be lost
			// due to DWORD type:
			var tick_now = DateTime.Now;

			if (!aAllowEarlyReturn && (int)(aSleepDuration - (tick_now - aStartTime).TotalMilliseconds) > SLEEP_INTERVAL_HALF)
				// Early return isn't allowed and the time remaining is large enough that we need to
				// wait some more (small amounts of remaining time can't be effectively waited for
				// due to the 10ms granularity limit of SetTimer):
				return ResultType.Fail; // Tell the caller to wait some more.

			// v1.0.38.04: Reset mLastPeekTime because caller has just done a GetMessage() or PeekMessage(),
			// both of which should have routed events to the keyboard/mouse hooks like LONG_OPERATION_UPDATE's
			// PeekMessage() and thus satisfied the reason that mLastPeekTime is tracked in the first place.
			// UPDATE: Although the hooks now have a dedicated thread, there's a good chance mLastPeekTime is
			// beneficial in terms of increasing GUI & script responsiveness, so it is kept.
			// The following might also improve performance slightly by avoiding extra Peek() calls, while also
			// reducing premature thread interruptions.
			lastPeekTime = tick_now;
			return ResultType.Ok;
		}

		internal static string MakeTitleWithVersion(string title) => title + " - Keysharp " + A_AhkVersion;

		internal static int[] ParseVersionToInts(string ver)
		{
			var i = 0;
			var vers = new int[] { 0, 0, 0, 0 };

			foreach (Range r in ver.AsSpan().Split('.'))
			{
				var split = ver.AsSpan(r).Trim();

				if (split.Length > 0)
				{
					if (int.TryParse(split, out var v))
						vers[i] = v;

					i++;
				}
			}

			return vers;
		}

		internal static void SetHotNamesAndTimes(string name)
		{
			// Just prior to launching the hotkey, update these values to support built-in
			// variables such as A_TimeSincePriorHotkey:
			priorHotkeyName = thisHotkeyName;//None of this will work until we come up with a way to manage thread order.//TODO
			priorHotkeyStartTime = thisHotkeyStartTime;
			// Unlike hotkeys -- which can have a name independent of their label by being created or updated
			// with the HOTKEY command -- a hot string's unique name is always its label since that includes
			// the options that distinguish between (for example) :c:ahk:: and ::ahk::
			thisHotkeyName = name;
			thisHotkeyStartTime = DateTime.Now; // Fixed for v1.0.35.10 to not happen for GUI
		}

		internal static void SetInitialFloatFormat()
		{
			var t = Thread.CurrentThread;
			var ci = new CultureInfo(t.CurrentCulture.Name);
			ci.NumberFormat.NumberDecimalDigits = 6;
			t.CurrentCulture = ci;
		}

		private static void PrivateClipboardUpdate(params object[] o)
		{
			var i = 0;
			var b = false;//False means keep going, true means stop.

			if (Clipboard.ContainsText() || Clipboard.ContainsFileDropList())
				while (!b && i < ClipFunctions.Count) b = IfTest(ClipFunctions[i++].Call(1));//Can't use foreach because the collection could be modified by the event.
			else if (!KeysharpEnhancements.IsClipboardEmpty())
				while (!b && i < ClipFunctions.Count) b = IfTest(ClipFunctions[i++].Call(2));
			else
				while (!b && i < ClipFunctions.Count) b = IfTest(ClipFunctions[i++].Call(0));
		}
	}
}