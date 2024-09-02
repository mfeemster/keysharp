namespace Keysharp.Scripting
{
	public partial class Script
	{
		public static bool ForceKeybdHook;
		public static bool HotstringNoMouse = false;
		public static uint MaxThreadsPerHotkey = 1u;
		public static uint MaxThreadsTotal = 12u;
		public static bool NoTrayIcon = false;
		public static bool WinActivateForce = false;
		public static bool ValidateThenExit;
		internal const int INTERVAL_UNSPECIFIED = int.MinValue + 303;
		internal const int SLEEP_INTERVAL = 10;
		internal const int SLEEP_INTERVAL_HALF = SLEEP_INTERVAL / 2;
		internal static List<IFuncObj> ClipFunctions = new List<IFuncObj>();
		internal static List<IFuncObj> hotCriterions = new List<IFuncObj>();
		internal static IntPtr hotExprLFW = IntPtr.Zero;
		internal static List<IFuncObj> hotExprs = new List<IFuncObj>();
		internal static bool hsResetUponMouseClick = HotstringNoMouse;
		internal static InputType input;
		internal static DateTime inputTimeoutAt = DateTime.Now;
		internal static bool inputTimerExists;
		internal static bool isReadyToExecute;
		internal static DateTime lastPeekTime;
		internal static MainWindow mainWindow;
		internal static Gui mainWindowGui;
		internal static int maxThreadsLimit = 0xFF;
		internal static Keysharp.Core.Common.Keyboard.MenuType menuIsVisible = MenuType.None;
		internal static List<IFuncObj> onErrorHandlers;
		internal static List<IFuncObj> onExitHandlers = new List<IFuncObj>();
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
		internal static PlatformManagerBase mgr = PlatformProvider.Manager;
		private static IntPtr mainWindowHandle;

		public static bool ResetUponMouseClick => hsResetUponMouseClick;

		public static Variables Vars { get; private set; }

		internal static Keysharp.Core.Common.Threading.HookThread HookThread { get; private set; }

		internal static IntPtr HwndLastUsed
		{
			get => Threads.GetThreadVariables().hwndLastUsed;
			set => Threads.GetThreadVariables().hwndLastUsed = value;
		}

		internal static bool IsMainWindowClosing => mainWindow == null || mainWindow.IsClosing;

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

		public static void Edit()
		{
			if (Accessors.A_IsCompiled)
			{
				_ = Dialogs.MsgBox("Cannot edit a compiled script.");
				return;
			}

			var title = mainWindow != null ? mainWindow.Text : "";
			var tv = Threads.GetThreadVariables();
			var mm = tv.titleMatchMode;
			tv.titleMatchMode = 2L;//Match anywhere.
			var hwnd = Window.WinExist(Accessors.A_ScriptName, "", title, "");
			tv.titleMatchMode = mm;
			var wi = new WindowItem(new IntPtr(hwnd));
			var classname = wi.ClassName;//Logic taken from AHK.

			if (classname == "#32770" || classname == "AutoHotkey" || classname == "Keysharp")//MessageBox(), InputBox(), FileSelect(), or GUI/script-owned window.
				hwnd = 0;

			if (hwnd == 0)
			{
#if LINUX
				_ = $"$EDITOR {Accessors.A_ScriptFullPath}".Bash(false);
#elif WINDOWS
				var ed = "";

				try
				{
					ed = Registrys.RegRead(@"HKCR\KeysharpScript\Shell\Edit\Command") as string;
				}
				catch
				{
				}

				try
				{
					if (string.IsNullOrEmpty(ed))
						ed = Registrys.RegRead(@"HKCR\AutoHotkeyScript\Shell\Edit\Command") as string;
				}
				catch
				{
				}

				object pid = null;

				if (!string.IsNullOrEmpty(ed))
				{
					var splits = ed.Split('%');
					ed = splits.Length > 0 ? splits[0] : ed;
					_ = Processes.Run(ed, Accessors.A_ScriptDir, "", ref pid, Accessors.A_ScriptFullPath);
				}
				else
					_ = Processes.Run($"Notepad.exe", Accessors.A_ScriptDir, "", ref pid, Accessors.A_ScriptFullPath);

#endif
			}
			else
			{
				wi.Active = true;
			}
		}

		public static string GetPublicStaticPropertyNames()
		{
			var l1 = Reflections.flatPublicStaticMethods.Keys.ToList();
			l1.AddRange(Reflections.flatPublicStaticProperties.Keys);
			var hs = new HashSet<string>(l1);
			return string.Join(' ', hs);
		}

		public static string GetVars(object obj = null)
		{
			var tabLevel = 0;
			var doInternal = obj.Ab(true);
			var sbuf = new StringBuffer();
			var sb = sbuf.sb;
			var typesToProps = new SortedDictionary<string, List<PropertyInfo>>();
			_ = sb.AppendLine($"**User defined**\r\n");

			foreach (var typeKv in Reflections.staticFields.Where(tkv => tkv.Key.Name.StartsWith("program", StringComparison.OrdinalIgnoreCase)))
			{
				foreach (var fieldKv in typeKv.Value.OrderBy(f => f.Key))
				{
					var val = fieldKv.Value.GetValue(null);
					var fieldType = val != null ? val.GetType().Name : fieldKv.Value.FieldType.Name;
					_ = Misc.PrintProps(val, fieldKv.Key, sbuf, ref tabLevel);
				}
			}

			_ = sb.AppendLine("\r\n--------------------------------------------------\r\n**Internal**\r\n");

			if (doInternal)
			{
				foreach (var propKv in Reflections.flatPublicStaticProperties)
				{
					var list = typesToProps.GetOrAdd(propKv.Value.DeclaringType.Name);

					if (list.Count == 0)
						list.Capacity = 200;

					list.Add(propKv.Value);
				}

				foreach (var t2pKv in typesToProps)
				{
					var typeName = t2pKv.Key;
					_ = sb.AppendLine($"{typeName}:");

					foreach (var prop in t2pKv.Value.OrderBy(p => p.Name))
					{
						try
						{
							var val = prop.GetValue(null);
							var proptype = val != null ? val.GetType().Name : prop.PropertyType.Name;//If you ever want to see the types, add this back in.
							_ = Misc.PrintProps(val, prop.Name, sbuf, ref tabLevel);
						}
						catch (Exception ex)
						{
							Keysharp.Scripting.Script.OutputDebug($"GetVars(): exception thrown inside of nested loop inside of second internal loop: {ex.Message}");
						}
					}

					_ = sb.AppendLine("--------------------------------------------------");
					_ = sb.AppendLine();
				}
			}

			return sbuf.sb.ToString();
		}

		public static void HandleSingleInstance(string name, eScriptInstance inst)
		{
			if (name.Length == 0 || name == "*")//Happens when running in Keyview.
				return;

			if (Env.FindCommandLineArg("force") != null || Env.FindCommandLineArg("f") != null)
				inst = eScriptInstance.Off;

			if (Env.FindCommandLineArg("restart") != null || Env.FindCommandLineArg("r") != null)
				inst = eScriptInstance.Force;

			switch (inst)
			{
				case eScriptInstance.Force:
				{
					Window.WinClose(name, "", 2);
				}
				break;

				case eScriptInstance.Ignore:
					if (Window.WinExist(name) != 0)
						_ = Flow.ExitApp(Flow.ExitReasons.Single);

					break;

				case eScriptInstance.Off:
					break;

				case eScriptInstance.Prompt:
				default:
					var hwnd = Window.WinExist(name);

					if (hwnd != 0)
					{
						if (Dialogs.MsgBox("Do you want to close the existing instance before running this one?\nYes to exit that instance, No to exit this instance.", "", "YesNo") == "Yes")
							Window.WinClose(hwnd, "", 2);
						else
							_ = Flow.ExitApp(Flow.ExitReasons.Single);
					}

					break;
			}
		}

		public static string ListKeyHistory()
		{
			var sb = new StringBuilder(2048);
			var target_window = WindowProvider.Manager.ActiveWindow;
			var win_title = target_window.IsSpecified ? target_window.Title : "";
			var enabledTimers = 0;

			foreach (var timer in Flow.timers)
			{
				if (timer.Value.Enabled)
				{
					enabledTimers++;
					_ = sb.Append($"{timer.Key.Name} ");
				}
			}

			if (sb.Length > 123)
			{
				var tempstr = sb.ToString(0, 123).TrimEnd() + "...";
				_ = sb.Clear();
				_ = sb.Append(tempstr);
			}
			else if (sb.Length > 0)
			{
				if (sb[sb.Length - 1] == ' ')
				{
					var tempstr = sb.ToString().TrimEnd();
					_ = sb.Clear();
					_ = sb.Append(tempstr);
				}
			}

			var timerlist = sb.ToString();
			var mod = "";
			var hookstatus = "";
			var cont = "Key History has been disabled via KeyHistory(0).";
			_ = sb.Clear();

			if (HookThread != null)
			{
				mod = HookThread.kbdMsSender.ModifiersLRToText(HookThread.kbdMsSender.GetModifierLRState(true));
				hookstatus = HookThread.GetHookStatus();

				if (HookThread.keyHistory != null && HookThread.keyHistory.Size > 0)
					cont = "Press [F5] to refresh.";
			}

			_ = sb.AppendLine($"Window: {win_title}");
			_ = sb.AppendLine($"Keybd hook: {(HookThread != null && HookThread.HasKbdHook() ? "yes" : "no")}");
			_ = sb.AppendLine($"Mouse hook: {(HookThread != null && HookThread.HasMouseHook() ? "yes" : "no")}");
			_ = sb.AppendLine($"Enabled timers: {enabledTimers} of {Flow.timers.Count} ({timerlist})");
			_ = sb.AppendLine($"Threads: {totalExistingThreads}");
			_ = sb.AppendLine($"Modifiers (GetKeyState() now) = {mod}");
			_ = sb.AppendLine(hookstatus);
			_ = sb.Append(cont);
			return sb.ToString();
		}

		public static void ListLines(params object[] obj) => throw new Error("ListLines() is not supported in Keysharp because it's a compiled program, not an interpreted one.");

		public static void ListVars() => mainWindow?.ShowInternalVars(true);

		/// <summary>
		/// Sends a string to the debugger (if any) for display.
		/// </summary>
		/// <param name="obj0">The text to send to the debugger for display.</param>
		/// <param name="obj1">True to first clear the display, else false to append.</param>
		public static void OutputDebug(object obj0, object obj1 = null)
		{
			var text = obj0.As();
			var clear = obj1.Ab();
			System.Diagnostics.Debug.WriteLine(text);//Will print only in debug mode.

			//This will throw when running tests.
			try
			{
				Console.Out.WriteLine(text);//Will print to the console when piped to | more, even though this is a windows application.
			}
			catch
			{
			}

			if (!IsMainWindowClosing)
				if (clear)
					mainWindow.SetText(text, MainWindow.MainFocusedTab.Debug, false);
				else
					mainWindow.AddText(text, MainWindow.MainFocusedTab.Debug, false);
		}

		public static void RunMainWindow(string title, Func<object> userInit)
		{
			mainWindow = new MainWindow();

			if (!string.IsNullOrEmpty(title))
				mainWindow.Text = title + " - Keysharp " + Accessors.A_AhkVersion;

			mainWindow.ClipboardUpdate += PrivateClipboardUpdate;
			mainWindow.Icon = Core.Properties.Resources.Keysharp_ico;
			//Parser.Persistent = true;
			mainWindowGui = new Gui(null, null, null, mainWindow);
			//This combo of using Activate, the minimize, then making extra sure in the Load event handler seems to work well enough.
			//Sometimes the form will show in the bottom left corner very quickly, but it should barely be visible to users.
			mainWindow.WindowState = FormWindowState.Minimized;
			mainWindow.Activate();
			_ = mainWindow.BeginInvoke(() =>
			{
				Misc.TryCatch(() =>
				{
					var (__pushed, __btv) = Keysharp.Core.Common.Threading.Threads.BeginThread();
					_ = userInit();
					//This has to be done here because it uses the window handle to register hotkeys, and the handle isn't valid until mainWindow.Load() is called.
					HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();//We want these active now in case auto-execute never returns (e.g. loop));
					isReadyToExecute = true;
					Keysharp.Core.Common.Threading.Threads.EndThread(__pushed);
				}, true);//Pop on exception because EndThread() above won't be called.
			});
			Application.Run(mainWindow);
		}

		public static void SetName(string s) => scriptName = s;

		public static void ShowDebug() => mainWindow?.ShowDebug();

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

		public static void SimulateKeyPress(uint key) => HookThread.SimulateKeyPress(key);

		public static void VerifyVersion(string ver, bool plus, int line, string code)
		{
			var ahkver = Accessors.A_AhkVersion;
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

		internal static bool AnyPersistent()
		{
			if (Keysharp.Core.Gui.AnyExistingVisibleWindows())
				return true;

			if (HotkeyDefinition.shk.Count > 0)
				return true;

			if (HotstringDefinition.shs.Count > 0)
				return true;

			if (Keysharp.Core.Flow.timers.Count > 0)
				return true;

			if (Script.ClipFunctions.Count > 0)
				return true;

			if (totalExistingThreads > 0)
				return true;

			if (Keysharp.Core.Flow.persistentValueSetByUser)
				return true;

			if (Script.input != null)
			{
				for (var input = Script.input; ; input = input.Prev)
				{
					if (input != null)
						return true;
				}
			}

			return false;
		}

		internal static void ExitIfNotPersistent(Keysharp.Core.Flow.ExitReasons exitReason)
		{
			if (!AnyPersistent())
				_ = Flow.ExitApp((int)exitReason);
		}

		internal static bool InitHook()
		{
			if (HookThread != null)
				return false;

#if WINDOWS
			HookThread = new Keysharp.Core.Windows.WindowsHookThread();
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

		internal static int[] ParseVersionToInts(string ver)
		{
			var vers = new int[] { 0, 0, 0, 0 };
			var versplits = ver.Split('.', StringSplitOptions.RemoveEmptyEntries);

			if (versplits.Length > 0)
			{
				for (var i = 0; i < 4; i++)
				{
					if (versplits.Length > i)
						if (versplits[i].ParseInt(false) is int v)
							vers[i] = v;
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
			var ci = new CultureInfo(t.CurrentCulture.LCID);
			ci.NumberFormat.NumberDecimalDigits = 6;
			t.CurrentCulture = ci;
		}

		private static void PrivateClipboardUpdate(params object[] o)
		{
			var i = 0;
			var b = false;//False means keep going, true means stop.

			if (Clipboard.ContainsText() || Clipboard.ContainsFileDropList())
				while (!b && i < ClipFunctions.Count) b = IfTest(ClipFunctions[i++].Call(1));//Can't use foreach because the collection could be modified by the event.
			else if (!Env.IsClipboardEmpty())
				while (!b && i < ClipFunctions.Count) b = IfTest(ClipFunctions[i++].Call(2));
			else
				while (!b && i < ClipFunctions.Count) b = IfTest(ClipFunctions[i++].Call(0));
		}
	}
}