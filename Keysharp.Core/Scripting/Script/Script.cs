using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core;
using Keysharp.Core.Common;
using Keysharp.Core.Common.Input;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Windows;

namespace Keysharp.Scripting
{
	public partial class Script : Core.Core
	{
		internal const int INTERVAL_UNSPECIFIED = int.MinValue + 303;

		internal const int SLEEP_INTERVAL = 10;

		internal const int SLEEP_INTERVAL_HALF = SLEEP_INTERVAL / 2;

		internal static bool calledByIsDialogMessageOrDispatch;

		internal static uint calledByIsDialogMessageOrDispatchMsg;

		internal static List<IFuncObj> ClipFunctions = new List<IFuncObj>();

		internal static bool deferMessagesForUnderlyingPump;

		internal static Keysharp.Core.Common.Keyboard.HotkeyCriterion firstHotCriterion, lastHotCriterion;

		internal static Keysharp.Core.Common.Keyboard.HotkeyCriterion firstHotExpr, lastHotExpr;

		internal static bool forceKeybdHook;

		[ThreadStatic]
		internal static Keysharp.Core.Common.Keyboard.HotkeyCriterion hotCriterion;

		internal static IntPtr hotExprLFW = IntPtr.Zero;

		internal static bool hsResetUponMouseClick = Keysharp.Scripting.Parser.HotstringNoMouse;

		internal static bool hsSameLineAction;

		internal static IntPtr hWndLastUsed = IntPtr.Zero;

		internal static InputType input;

		internal static uint inputLevel;

		// Last Found Window of last #HotIf expression.
		internal static DateTime inputTimeoutAt = DateTime.Now;

		internal static bool inputTimerExists;

		internal static bool isReadyToExecute;

		internal static FuncObj lastHotFunc;

		internal static MainWindow mainWindow;

		internal static Gui mainWindowGui;

		internal static int MAX_THREADS_LIMIT = 0xFF;

		internal static Keysharp.Core.Common.Keyboard.MenuType menuIsVisible = Keysharp.Core.Common.Keyboard.MenuType.None;

		internal static int nLayersNeedingTimer;

		internal static List<IFuncObj> OnExitHandlers = new List<IFuncObj>();

		internal static Icon PausedIcon;

		internal static IntPtr playbackHook = IntPtr.Zero;

		internal static DateTime priorHotkeyStartTime = DateTime.Now;

		internal static string scriptName = "";

		internal static Icon SuspendedIcon;

		internal static string thisHotkeyName, priorHotkeyName;

		internal static DateTime thisHotkeyStartTime = DateTime.Now;

		internal static DateTime timeLastInputKeyboard = timeLastInputPhysical;

		internal static DateTime timeLastInputMouse = timeLastInputPhysical;

		internal static DateTime timeLastInputPhysical = DateTime.Now;

		internal static int totalExistingThreads;

		//Unsure exactly where these globals should go. AHK spreads them across global vars, ThreadState (per thread) and ThreadSettings. Need to properly place.//TODO
		internal static bool validateThenExit;

		private static IntPtr mainWindowHandle;

		public static bool ResetUponMouseClick => hsResetUponMouseClick;

		public static Variables Vars { get; private set; }

		internal static Keysharp.Core.Common.Threading.HookThread HookThread { get; private set; }

		internal static IntPtr MainWindowHandle
		{
			get
			{
				if (mainWindowHandle == IntPtr.Zero)
					_ = mainWindow.Invoke(() => mainWindowHandle = mainWindow.Handle);

				return mainWindowHandle;
			}
		}

		static Script()
		{
			if (Vars == null)
				Vars = new Variables();

			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			_ = InitHook();
		}

		public static void Edit()
		{
			if (Accessors.A_IsCompiled)
			{
				_ = Keysharp.Core.Dialogs.MsgBox("Cannot edit a compiled script.");
				return;
			}

			var title = mainWindow != null ? mainWindow.Text : "";
			var mm = Accessors.A_TitleMatchMode;
			Accessors.A_TitleMatchMode = 2;//Match anywhere.
			var hwnd = Window.WinExist(Accessors.A_ScriptName, "", title, "");
			Accessors.A_TitleMatchMode = mm;
			var wi = new WindowItem(new IntPtr(hwnd));
			var classname = wi.ClassName;//Logic taken from AHK.

			if (classname == "#32770" || classname == "AutoHotkey" || classname == "Keysharp")//MessageBox(), InputBox(), FileSelect(), or GUI/script-owned window.
				hwnd = 0;

			if (hwnd == 0)
			{
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

				if (!string.IsNullOrEmpty(ed))
				{
					var splits = ed.Split('%');
					ed = splits.Length > 0 ? splits[0] : ed;
					_ = Processes.Run(ed, Accessors.A_ScriptDir, "", Accessors.A_ScriptFullPath);
				}
				else
					_ = Processes.Run($"Notepad.exe", Accessors.A_ScriptDir, "", Accessors.A_ScriptFullPath);
			}
			else
			{
				wi.Active = true;
			}
		}

		public static void HandleSingleInstance(string name, eScriptInstance inst)
		{
			if (name.Length == 0 || name == "*")//Happens when running in Keysharp
				return;

			if (Keysharp.Core.Env.FindCommandLineArg("force") != null || Keysharp.Core.Env.FindCommandLineArg("f") != null)
				inst = eScriptInstance.Off;

			if (Keysharp.Core.Env.FindCommandLineArg("restart") != null || Keysharp.Core.Env.FindCommandLineArg("r") != null)
				inst = eScriptInstance.Force;

			switch (inst)
			{
				case eScriptInstance.Force:
				{
					Keysharp.Core.Window.WinClose(name, "", 2);
				}
				break;

				case eScriptInstance.Ignore:
					if (Keysharp.Core.Window.WinExist(name) != 0)
						_ = Flow.ExitApp(Keysharp.Core.Flow.ExitReasons.SingleInstance);

					break;

				case eScriptInstance.Off:
					break;

				case eScriptInstance.Prompt:
				default:
					var hwnd = Keysharp.Core.Window.WinExist(name);

					if (hwnd != 0)
					{
						if (Dialogs.MsgBox("Do you want to close the existing instance before running this one?\nYes to exit that instance, No to exit this instance.", "", "YesNo") == "Yes")
							Keysharp.Core.Window.WinClose(hwnd, "", 2);
						else
							_ = Flow.ExitApp(Keysharp.Core.Flow.ExitReasons.SingleInstance);
					}

					break;
			}
		}

		public static void ListLines(params object[] obj) => throw new Error("ListLines() is not supported in Keysharp because it's a compiled program, not an interpreted one.");

		public static void ListVars() => mainWindow?.SetText(Reflections.GetVariableInfo(), MainWindow.MainFocusedTab.Stack);

		public static string ListKeyHistory()
		{
			var sb = new StringBuilder(2048);
			var target_window = Keysharp.Core.Common.Window.WindowManagerProvider.Instance.GetForeGroundWindow();
			var win_title = target_window.IsSpecified ? target_window.Title : "";
			var enabledTimers = 0;

			foreach (var timer in Keysharp.Core.Flow.timers)
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
			sb.Clear();

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
			_ = sb.AppendLine($"Enabled timers: {enabledTimers} of {Keysharp.Core.Flow.timers.Count} ({timerlist})");
			_ = sb.AppendLine($"Threads: {Keysharp.Scripting.Script.totalExistingThreads}");
			_ = sb.AppendLine($"Modifiers (GetKeyState() now) = {mod}");
			_ = sb.AppendLine(hookstatus);
			_ = sb.Append(cont);
			return sb.ToString();
		}

		/// <summary>
		/// Sends a string to the debugger (if any) for display.
		/// </summary>
		/// <param name="text">The text to send to the debugger for display.</param>
		public static void OutputDebug(object obj0, object obj1 = null)
		{
			var text = obj0.As();
			var clear = obj1.Ab();
			System.Diagnostics.Debug.WriteLine(text);

			if (Script.mainWindow != null)
				if (clear)
					Script.mainWindow.SetText(text, MainWindow.MainFocusedTab.Debug);
				else
					Script.mainWindow.AddText(text, MainWindow.MainFocusedTab.Debug);

			//if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			//WindowsAPI.OutputDebugString(text);//Unsure if this is really needed, or will Debug.WriteLine() suffice?
		}

		/*

		    LPTSTR Hotkey::ListHotkeys(LPTSTR aBuf, int aBufSize)
		    // Translates this script's list of variables into text equivalent, putting the result
		    // into aBuf and returning the position in aBuf of its new string terminator.
		    {
		    LPTSTR aBuf_orig = aBuf;
		    // Save vertical space by limiting newlines here:
		    aBuf += sntprintf(aBuf, BUF_SPACE_REMAINING, _T("Type\tOff?\tLevel\tRunning\tName\r\n")
		                     _T("-------------------------------------------------------------------\r\n"));
		    // Start at the oldest and continue up through the newest:
		    for (int i = 0; i < sHotkeyCount; ++i)
		    aBuf = shk[i]->ToText(aBuf, BUF_SPACE_REMAINING, true);
		    return aBuf;
		    }

		 * */
		public static void RunMainWindow(string title, Action userInit)
		{
			mainWindow = new MainWindow();

			if (!string.IsNullOrEmpty(title))
				mainWindow.Text = title + " - Keysharp v" + Accessors.A_AhkVersion;

			mainWindow.ClipboardUpdate += PrivateClipboardUpdate;
			mainWindow.WindowState = FormWindowState.Minimized;
			//mainWindow.WindowState = FormWindowState.Maximized;
			//mainWindow.ShowInTaskbar = false;//The main window is a system tray window only.
			mainWindow.Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
			Parser.Persistent = true;
			mainWindow.Load += MainWindow_Load;
			Parser.SuspendExempt = false;
			mainWindowGui = new Gui(null, null, null, mainWindow);
			mainWindow.Show();
			_ = mainWindow.BeginInvoke(() =>
			{
				userInit();
				//This has to be done here because it uses the window handle to register hotkeys, and the handle isn't valid until mainWindow.Load() is called.
				HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();//We want these active now in case auto-execute never returns (e.g. loop));
				isReadyToExecute = true;
			});
			Application.Run(mainWindow);
		}

		public static void SetName(string s) => scriptName = s;

		public static void SimulateKeyPress(uint key) => HookThread.SimulateKeyPress(key);

		internal static void ExitIfNotPersistent(Keysharp.Core.Flow.ExitReasons exitReason)
		{
			if (totalExistingThreads > 0 || Parser.Persistent || mainWindow != null)
				return;

			if (Script.input != null)//Only exit if the last InputHook has been removed.
				return;

			_ = Keysharp.Core.Flow.ExitApp((int)exitReason);
		}

		internal static bool InitHook()
		{
			if (HookThread != null)
				return false;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				HookThread = new Keysharp.Core.Windows.WindowsHookThread();
				return true;
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				HookThread = new Keysharp.Core.Linux.LinuxHookThread();
				Environment.SetEnvironmentVariable("MONO_VISUAL_STYLES", "gtkplus");
				return true;
			}

			return false;
		}

		//Also probably not needed.//TODO
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
			kbdMouseSender.lastPeekTime = tick_now;
			return ResultType.Ok;
		}

		internal static bool IsMainWindowClosing => mainWindow == null || mainWindow.IsClosing;

		internal static void SetHotNamesAndTimes(string name)
		{
			// Just prior to launching the hotkey, update these values to support built-in
			// variables such as A_TimeSincePriorHotkey:
			Keysharp.Scripting.Script.priorHotkeyName = Keysharp.Scripting.Script.thisHotkeyName;//None of this will work until we come up with a way to manage thread order.//TODO
			Keysharp.Scripting.Script.priorHotkeyStartTime = Keysharp.Scripting.Script.thisHotkeyStartTime;
			// Unlike hotkeys -- which can have a name independent of their label by being created or updated
			// with the HOTKEY command -- a hot string's unique name is always its label since that includes
			// the options that distinguish between (for example) :c:ahk:: and ::ahk::
			Keysharp.Scripting.Script.thisHotkeyName = name;
			Keysharp.Scripting.Script.thisHotkeyStartTime = DateTime.Now; // Fixed for v1.0.35.10 to not happen for GUI
		}

		private static void MainWindow_Load(object sender, EventArgs e)
		{
			//isReadyToExecute = true; // This is done only after the above to support error reporting in Hotkey.cpp.
		}

		/*

		        var we_turned_on_defer = false; // Set default.

		        if (aMode == MessageMode.ReturnAfterMessagesSpecialFilter)
		        {
		        aMode = MessageMode.ReturnAfterMessages; // To simplify things further below, eliminate the mode RETURN_AFTER_MESSAGES_SPECIAL_FILTER from further consideration.

		        // g_DeferMessagesForUnderlyingPump is a global because the instance of MsgSleep on the call stack
		        // that set it to true could launch new thread(s) that call MsgSleep again (i.e. a new layer), and a global
		        // is the easiest way to inform all such MsgSleeps that there's a non-standard msg pump beneath them on the
		        // call stack.
		        if (!deferMessagesForUnderlyingPump)
		        {
		            deferMessagesForUnderlyingPump = true;
		            we_turned_on_defer = true;
		        }

		        // So now either we turned it on or some layer beneath us did.  Therefore, we know there's at least one
		        // non-standard msg pump beneath us on the call stack.
		        }

		        // While in mode RETURN_AFTER_MESSAGES, there are different things that can happen:
		        // 1) We launch a new hotkey subroutine, interrupting/suspending the old one.  But
		        //    subroutine calls this function again, so now it's recursed.  And thus the
		        //    new subroutine can be interrupted yet again.
		        // 2) We launch a new hotkey subroutine, but it returns before any recursed call
		        //    to this function discovers yet another hotkey waiting in the queue.  In this
		        //    case, this instance/recursion layer of the function should process the
		        //    hotkey messages linearly rather than recursively?  No, this doesn't seem
		        //    necessary, because we can just return from our instance/layer and let the
		        //    caller handle any messages waiting in the queue.  Eventually, the queue
		        //    should be emptied, especially since most hotkey subroutines will run
		        //    much faster than the user could press another hotkey, with the possible
		        //    exception of the key-repeat feature triggered by holding a key down.
		        //    Even in that case, the worst that would happen is that messages would
		        //    get dropped off the queue because they're too old (I think that's what
		        //    happens).
		        // Based on the above, when mode is RETURN_AFTER_MESSAGES, we process
		        // all messages until a hotkey message is encountered, at which time we
		        // launch that subroutine only and then return when it returns to us, letting
		        // the caller handle any additional messages waiting on the queue.  This avoids
		        // the need to have a "run the hotkeys linearly" mode in a single iteration/layer
		        // of this function.  Note: The WM_QUIT message does not receive any higher
		        // precedence in the queue than other messages.  Thus, if there's ever concern
		        // that that message would be lost, as a future change perhaps can use PeekMessage()
		        // with a filter to explicitly check to see if our queue has a WM_QUIT in it
		        // somewhere, prior to processing any messages that might take result in
		        // a long delay before the remainder of the queue items are processed (there probably
		        // aren't any such conditions now, so nothing to worry about?)
		        // Above is somewhat out-of-date.  The objective now is to spend as much time
		        // inside GetMessage() as possible, since it's the keystroke/mouse engine
		        // whenever the hooks are installed.  Any time we're not in GetMessage() for
		        // any length of time (say, more than 20ms), keystrokes and mouse events
		        // will be lagged.  PeekMessage() is probably almost as good, but it probably
		        // only clears out any waiting keys prior to returning.  CONFIRMED: PeekMessage()
		        // definitely routes to the hook, perhaps only if called regularly (i.e. a single
		        // isolated call might not help much).
		        // Decided to support a true Sleep(0) for aSleepDuration == 0, as well
		        // as no delay at all if aSleepDuration < 0.  This is needed to implement
		        // "SetKeyDelay, 0" and possibly other things.  I believe a Sleep(0)
		        // is always <= Sleep(1) because both of these will wind up waiting
		        // a full timeslice if the CPU is busy.
		        // Reminder for anyone maintaining or revising this code:
		        // Giving each subroutine its own thread rather than suspending old ones is
		        // probably not a good idea due to the exclusive nature of the GUI
		        // (i.e. it's probably better to suspend existing subroutines rather than
		        // letting them continue to run because they might activate windows and do
		        // other stuff that would interfere with the window automation activities of
		        // other threads)
		        // If caller didn't specify, the exact amount of the Sleep() isn't
		        // critical to it, only that we handles messages and do Sleep()
		        // a little.
		        // Most of this initialization section isn't needed if aMode == WAIT_FOR_MESSAGES,
		        // but it's done anyway for consistency:
		        bool allow_early_return;

		        if (aSleepDuration == INTERVAL_UNSPECIFIED)
		        {
		        aSleepDuration = SLEEP_INTERVAL;  // Set interval to be the default length.
		        allow_early_return = true;
		        }
		        else
		        // The timer resolution makes waiting for half or less of an
		        // interval too chancy.  The correct thing to do on average
		        // is some kind of rounding, which this helps with:
		        allow_early_return = (aSleepDuration <= SLEEP_INTERVAL_HALF);

		        // Record the start time when the caller first called us so we can keep
		        // track of how much time remains to sleep (in case the caller's subroutine
		        // is suspended until a new subroutine is finished).  But for small sleep
		        // intervals, don't worry about it.
		        // Note: QueryPerformanceCounter() has very high overhead compared to GetTickCount():
		        var start_time = allow_early_return ? DateTime.MinValue : DateTime.Now;

		        // This check is also done even if the main timer will be set (below) so that
		        // an initial check is done rather than waiting 10ms more for the first timer
		        // message to come in.  Some of our many callers would want this, and although some
		        // would not need it, there are so many callers that it seems best to just do it
		        // unconditionally, especially since it's not a high overhead call (e.g. it returns
		        // immediately if the tickcount is still the same as when it was last run).
		        // Another reason for doing this check immediately is that our msg queue might
		        // contains a time-consuming msg prior to our WM_TIMER msg, e.g. a hotkey msg.
		        // In that case, the hotkey would be processed and launched without us first having
		        // emptied the queue to discover the WM_TIMER msg.  In other words, WM_TIMER msgs
		        // might get buried in the queue behind others, so doing this check here should help
		        // ensure that timed subroutines are checked often enough to keep them running at
		        // their specified frequencies.
		        // Note that ExecUntil() no longer needs to call us solely for prevention of lag
		        // caused by the keyboard & mouse hooks, so checking the timers early, rather than
		        // immediately going into the GetMessage() state, should not be a problem:
		        if (HotkeyDefinition.joyHotkeyCount != 0)
		        Keysharp.Core.Common.Joystick.Joystick.PollJoysticks();  // Do this first since it's much faster.

		        var return_value = false; //  Set default.  Also, this is used by the macro below.

		        if (Keysharp.Core.Flow.EnabledTimerCount() != 0)// && CheckScriptTimers())
		        return_value = true; // Change the existing value only if it returned true.

		        // Because this function is called recursively: for now, no attempt is
		        // made to improve performance by setting the timer interval to be
		        // aSleepDuration rather than a standard short interval.  That would cause
		        // a problem if this instance of the function invoked a new subroutine,
		        // suspending the one that called this instance.  The new subroutine
		        // might need a timer of a longer interval, which would mess up
		        // this layer.  One solution worth investigating is to give every
		        // layer/instance its own timer (the ID of the timer can be determined
		        // from info in the WM_TIMER message).  But that can be a real mess
		        // because what if a deeper recursion level receives our first
		        // WM_TIMER message because we were suspended too long?  Perhaps in
		        // that case we wouldn't our WM_TIMER pulse because upon returning
		        // from those deeper layers, we would check to see if the current
		        // time is beyond our finish time.  In addition, having more timers
		        // might be worse for overall system performance than having a single
		        // timer that pulses very frequently (because the system must keep
		        // them all up-to-date).  UPDATE: Timer is now also needed whenever an
		        // aSleepDuration greater than 0 is about to be done and there are some
		        // script timers that need to be watched (this happens when aMode == WAIT_FOR_MESSAGES).
		        // UPDATE: Make this a macro so that it is dynamically resolved every time, in case
		        // the value of g_script.mTimerEnabledCount changes on-the-fly.
		        // UPDATE #2: The below has been changed in light of the fact that the main timer is
		        // now kept always-on whenever there is at least one enabled timed subroutine.
		        // This policy simplifies ExecUntil() and long-running commands such as FileSetAttrib.
		        // UPDATE #3: Use aMode == RETURN_AFTER_MESSAGES, not g_nThreads > 0, because the
		        // "Edit This Script" menu item (and possibly other places) might result in an indirect
		        // call to us and we will need the timer to avoid getting stuck in the GetMessageState()
		        // with hotkeys being disallowed due to filtering:
		        var this_layer_needs_timer = aSleepDuration > 0 && aMode == MessageMode.ReturnAfterMessages;

		        if (this_layer_needs_timer)
		        {
		        ++nLayersNeedingTimer;  // IsCycleComplete() is responsible for decrementing this for us.
		        Keysharp.Core.Flow.SetMainTimer();
		        // Reasons why the timer might already have been on:
		        // 1) g_script.mTimerEnabledCount is greater than zero or there are joystick hotkeys.
		        // 2) another instance of MsgSleep() (beneath us in the stack) needs it (see the comments
		        //    in IsCycleComplete() near KILL_MAIN_TIMER for details).
		        }

		        // Only used when aMode == RETURN_AFTER_MESSAGES:
		        // True if the current subroutine was interrupted by another:
		        //bool was_interrupted = false;
		        var sleep0_was_done = false;
		        var empty_the_queue_via_peek = false;
		        var messages_received = 0; // This is used to ensure we Sleep() at least a minimal amount if no messages are received.
		        bool msg_was_handled;
		        IntPtr fore_window, focused_control, criterion_found_hwnd;
		        string wnd_class_name;
		        System.Windows.Forms.ToolStripMenuItem menu_item;
		        int hk_id;
		        HotkeyDefinition hk;
		        int variant_id;
		        HotkeyVariant variant;
		        int priority;
		        HotstringDefinition hs;
		        Gui pgui; // This is just a temp variable and should not be referred to once the below has been determined.
		        int gui_control_index;
		        Control pcontrol;
		        GuiControl pguicontrol = null;
		        GuiEventTypes gui_action;
		        GuiEventKinds gui_event_kind;
		        uint gui_event_code;
		        ulong gui_event_info;
		        int gui_event_byte;
		        bool event_is_control_generated;
		        MsgMonitorList check_if_running;
		        //ExprTokenType gui_event_args[6]; // Current maximum number of arguments for Gui event handlers.
		        int gui_event_arg_count;
		        POINT gui_point;
		        IntPtr hdrop_to_free;
		        InputType input_hook;
		        long msg_reply = 0;
		        var peek_result = false;
		        var msg = new Msg
		        {
		        hwnd = IntPtr.Zero
		        };

		        for (;;) // Main event loop.
		        {
		        if (aSleepDuration > 0 && !empty_the_queue_via_peek && !deferMessagesForUnderlyingPump) // g_Defer: Requires a series of Peeks to handle non-contiguous ranges, which is why GetMessage() can't be used.
		        {
		            // The following comment is mostly obsolete as of v1.0.39 (which introduces a thread
		            // dedicated to the hooks).  However, using GetMessage() is still superior to
		            // PeekMessage() for performance reason.  Add to that the risk of breaking things
		            // and it seems clear that it's best to retain GetMessage().
		            // Older comment:
		            // Use GetMessage() whenever possible -- rather than PeekMessage() or a technique such
		            // MsgWaitForMultipleObjects() -- because it's the "engine" that passes all keyboard
		            // and mouse events immediately to the low-level keyboard and mouse hooks
		            // (if they're installed).  Otherwise, there's greater risk of keyboard/mouse lag.
		            // PeekMessage(), depending on how, and how often it's called, will also do this, but
		            // I'm not as confident in it.
		            if (WindowsAPI.GetMessage(out msg, IntPtr.Zero, 0, (uint)Keysharp.Core.Processes.MsgFilterMax()) == -1) // -1 is an error, 0 means WM_QUIT
		                continue; // Error probably happens only when bad parameters were passed to GetMessage().

		            //else let any WM_QUIT be handled below.
		        }
		        else // aSleepDuration < 1 || empty_the_queue_via_peek || g_DeferMessagesForUnderlyingPump
		        {
		            bool do_special_msg_filter, peek_was_done = false; // Set default.
		            var dummy = IntPtr.Zero;

		            // Check the active window in each iteration in case a significant amount of time has passed since
		            // the previous iteration (due to launching threads, etc.)
		            if (deferMessagesForUnderlyingPump && (fore_window = WindowsAPI.GetForegroundWindow()) != IntPtr.Zero  // There is a foreground window.
		                    && WindowsAPI.GetWindowThreadProcessId(fore_window, out dummy) == Keysharp.Core.Processes.MainThreadID) // And it belongs to our main thread (the main thread is the only one that owns any windows).
		            {
		                do_special_msg_filter = false; // Set default.

		                if (Keysharp.Core.Dialogs.nFileDialogs > 0) // v1.0.44.12: Also do the special Peek/msg filter below for FileSelect because testing shows that frequently-running timers disrupt the ability to double-click.
		                {
		                    wnd_class_name = WindowsAPI.GetClassName(fore_window);
		                    do_special_msg_filter = wnd_class_name == "#32770";  // Due to checking g_nFileDialogs above, this means that this dialog is probably FileSelect rather than MsgBox/InputBox/DirSelect (even if this guess is wrong, it seems fairly inconsequential to filter the messages since other pump beneath us on the call-stack will handle them ok).
		                }

		                if (!do_special_msg_filter && (focused_control = WindowsAPI.GetFocus()) != IntPtr.Zero)
		                {
		                    wnd_class_name = WindowsAPI.GetClassName(focused_control);
		                    do_special_msg_filter = string.Compare(wnd_class_name, "SysTreeView32", true) == 0 // A TreeView owned by our thread has focus (includes DirSelect's TreeView).
		                                            || string.Compare(wnd_class_name, "SysListView32", true) == 0;
		                }

		                if (do_special_msg_filter)
		                {
		                    // v1.0.48.03: Below now applies to SysListView32 because otherwise a timer that runs
		                    // while the user is dragging a rectangle around a selection (Marquee) can cause the
		                    // mouse button to appear to be stuck down down after the user releases it.
		                    // v1.0.44.12: Below now applies to FileSelect dialogs too (see reason above).
		                    // v1.0.44.11: Since one of our thread's TreeViews has focus (even in DirSelect), this
		                    // section is a work-around for the fact that the TreeView's message pump (somewhere beneath
		                    // us on the call stack) is apparently designed to process some mouse messages directly rather
		                    // than receiving them indirectly (in its WindowProc) via our call to DispatchMessage() here
		                    // in this pump.  The symptoms of this issue are an inability of a user to reliably select
		                    // items in a TreeView (the selection sometimes snaps back to the previously selected item),
		                    // which can be reproduced by showing a TreeView while a 10ms script timer is running doing
		                    // a trivial single line such as x=1.
		                    // NOTE: This happens more often in DirSelect dialogs, I believe because it's msg
		                    // pump is ALWAYS running but that of a GUI TreeView is running only during mouse capture
		                    // (i.e. when left/right button is down).
		                    // This special handling for TreeView can someday be broadened so that focused control's
		                    // class isn't checked: instead, could check whether left and/or right mouse button is
		                    // logically down (which hasn't yet been tested).  Or it could be broadened to include
		                    // other system dialogs and/or common controls that have unusual processing in their
		                    // message pumps -- processing that requires them to directly receive certain messages
		                    // rather than having them dispatched directly to their WindowProc.
		                    peek_was_done = true;
		                    // Peek() must be used instead of Get(), and Peek() must be called more than once to handle
		                    // the two ranges on either side of the mouse messages.  But since it would be improper
		                    // to process messages out of order (and might lead to side-effects), force the retrieval
		                    // to be in chronological order by checking the timestamps of each Peek first message, and
		                    // then fetching the one that's oldest (since it should be the one that's been waiting the
		                    // longest and thus generally should be ahead of the other Peek's message in the queue):
		                    var filter_max = Keysharp.Core.Processes.IsInterruptible() ? int.MaxValue : WindowsAPI.WM_HOTKEY - 1; // Fixed in v1.1.16 to not use MSG_FILTER_MAX, which would produce 0 when IsInterruptible(). Although WM_MOUSELAST+1..0 seems to produce the right results, MSDN does not indicate that it is valid.
		                    bool Peek1(uint mode)
		                    {
		                        return WindowsAPI.PeekMessage(out msg, IntPtr.Zero, 0, WindowsAPI.WM_MOUSEFIRST - 1, mode);// Relies on the fact that WM_MOUSEFIRST < MSG_FILTER_MAX
		                    }
		                    bool Peek2(uint mode)
		                    {
		                        return WindowsAPI.PeekMessage(out msg, IntPtr.Zero, 0, WindowsAPI.WM_MOUSELAST + 1, mode);
		                    }

		                    if (!Peek1(WindowsAPI.PM_NOREMOVE))  // Since no message in Peek1, safe to always use Peek2's (even if it has no message either).
		                    {
		                        peek_result = Peek2(WindowsAPI.PM_REMOVE);
		                    }
		                    else // Peek1 has a message.  So if Peek2 does too, compare their timestamps.
		                    {
		                        var peek1_time = msg.time; // Save it due to overwrite in next line.

		                        if (!Peek2(WindowsAPI.PM_NOREMOVE)) // Since no message in Peek2, use Peek1's.
		                        {
		                            peek_result = Peek1(WindowsAPI.PM_REMOVE);
		                        }
		                        else // Both Peek2 and Peek1 have a message waiting, so to break the tie, retrieve the oldest one.
		                        {
		                            // In case tickcount has wrapped, compare it the better way (must cast to int to avoid
		                            // loss of negative values):
		                            peek_result = ((int)(msg.time - peek1_time) > 0) // Peek2 is newer than Peek1, so treat peak1 as oldest and thus first in queue.
		                                          ? Peek1(WindowsAPI.PM_REMOVE) : Peek2(WindowsAPI.PM_REMOVE);
		                        }
		                    }
		                }
		            }

		            if (!peek_was_done) // Since above didn't Peek(), fall back to doing the Peek with the standard filter.
		                peek_result = WindowsAPI.PeekMessage(out msg, IntPtr.Zero, 0, (uint)Keysharp.Core.Processes.MsgFilterMax(), WindowsAPI.PM_REMOVE);

		            if (!peek_result) // No more messages
		            {
		                // UPDATE: The section marked "OLD" below is apparently not quite true: although Peek() has been
		                // caught yielding our timeslice, it's now difficult to reproduce.  Perhaps it doesn't consistently
		                // yield (maybe it depends on the relative priority of competing processes) and even when/if it
		                // does yield, it might somehow not be as long or as good as Sleep(0).  This is evidenced by the fact
		                // that some of my script's WinWaitClose's finish too quickly when the Sleep(0) is omitted after a
		                // Peek() that returned FALSE.
		                // OLD (mostly obsolete in light of above): It is not necessary to actually do the Sleep(0) when
		                // aSleepDuration == 0 because the most recent PeekMessage() has just yielded our prior timeslice.
		                // This is because when Peek() doesn't find any messages, it automatically behaves as though it
		                // did a Sleep(0).
		                if (aSleepDuration == 0 && !sleep0_was_done)
		                {
		                    System.Threading.Thread.Sleep(0);
		                    sleep0_was_done = true;
		                    // Now start a new iteration of the loop that will see if we
		                    // received any messages during the up-to-20ms delay (perhaps even more)
		                    // that just occurred.  It's done this way to minimize keyboard/mouse
		                    // lag (if the hooks are installed) that will occur if any key or
		                    // mouse events are generated during that 20ms.  Note: It seems that
		                    // the OS knows not to yield our timeslice twice in a row: once for
		                    // the Sleep(0) above and once for the upcoming PeekMessage() (if that
		                    // PeekMessage() finds no messages), so it does not seem necessary
		                    // to check HIWORD(GetQueueStatus(QS_ALLEVENTS)).  This has been confirmed
		                    // via the following test, which shows that while BurnK6 (CPU maxing program)
		                    // is foreground, a Sleep(0) really does a Sleep(60).  But when it's not
		                    // foreground, it only does a Sleep(20).  This behavior is UNAFFECTED by
		                    // the added presence of a HIWORD(GetQueueStatus(QS_ALLEVENTS)) check here:
		                    //SplashTextOn,,, xxx
		                    //WinWait, xxx  ; set last found window
		                    //Loop
		                    //{
		                    //  start = %a_tickcount%
		                    //  Sleep, 0
		                    //  elapsed = %a_tickcount%
		                    //  elapsed -= %start%
		                    //  WinSetTitle, %elapsed%
		                    //}
		                    continue;
		                }

		                // Otherwise: aSleepDuration is non-zero or we already did the Sleep(0)
		                if (messages_received == 0 && allow_early_return)
		                {
		                    // Fix for v1.1.05.04: Since Peek() didn't find a message, avoid maxing the CPU.
		                    // This specific section is needed for BIF_Wait() when an underlying thread
		                    // is displaying a dialog, and perhaps in other cases.
		                    // Fix for v1.1.07.00: Avoid Sleep() if caller specified a duration of zero;
		                    // otherwise SendEvent with a key delay of 0 will be slower than expected.
		                    // This affects auto-replace hotstrings in SendEvent mode (which is the default
		                    // when SendInput is unavailable).  Note that if aSleepDuration == 0, Sleep(0)
		                    // was already called above or by a prior iteration.
		                    if (aSleepDuration > 0)
		                        System.Threading.Thread.Sleep(5); // This is a somewhat arbitrary value: the intent of a value below 10 is to avoid yielding more than one timeslice on all systems even if they have unusual timeslice sizes / system timers.

		                    ++messages_received; // Don't repeat this section.
		                    continue;
		                }

		                // Notes for the macro further below:
		                // Must decrement prior to every RETURN to balance it.
		                // Do this prior to checking whether timer should be killed, below.
		                // Kill the timer only if we're about to return OK to the caller since the caller
		                // would still need the timer if FAIL was returned above.  But don't kill it if
		                // there are any enabled timed subroutines, because the current policy it to keep
		                // the main timer always-on in those cases.  UPDATE: Also avoid killing the timer
		                // if there are any script threads running.  To do so might cause a problem such
		                // as in this example scenario: MsgSleep() is called for any reason with a delay
		                // large enough to require the timer.  The timer is set.  But a msg arrives that
		                // MsgSleep() dispatches to MainWindowProc().  If it's a hotkey or custom menu,
		                // MsgSleep() is called recursively with a delay of -1.  But when it finishes via
		                // IsCycleComplete(), the timer would be wrongly killed because the underlying
		                // instance of MsgSleep still needs it.  Above is even more wide-spread because if
		                // MsgSleep() is called recursively for any reason, even with a duration >10, it will
		                // wrongly kill the timer upon returning, in some cases.  For example, if the first call to
		                // MsgSleep(-1) finds a hotkey or menu item msg, and executes the corresponding subroutine,
		                // that subroutine could easily call MsgSleep(10+) for any number of reasons, which
		                // would then kill the timer.
		                // Also require that aSleepDuration > 0 so that MainWindowProc()'s receipt of a
		                // WM_HOTKEY msg, to which it responds by turning on the main timer if the script
		                // is uninterruptible, is not defeated here.  In other words, leave the timer on so
		                // that when the script becomes interruptible once again, the hotkey will take effect
		                // almost immediately rather than having to wait for the displayed dialog to be
		                // dismissed (if there is one).
		                //
		                // "we_turned_on_defer" is necessary to prevent us from turning it off if some other
		                // instance of MsgSleep beneath us on the calls stack turned it on.  Only it should
		                // turn it off because it might still need the "true" value for further processing.
		                bool RETURN_FROM_MSGSLEEP()
		                {
		                    if (we_turned_on_defer)
		                        deferMessagesForUnderlyingPump = false;

		                    if (this_layer_needs_timer)
		                    {
		                        --Keysharp.Scripting.Script.nLayersNeedingTimer;

		                        if (aSleepDuration > 0 && Keysharp.Scripting.Script.nLayersNeedingTimer == 0 && Keysharp.Core.Flow.EnabledTimerCount() == 0 && HotkeyDefinition.joyHotkeyCount == 0)
		                            Keysharp.Core.Flow.StopMainTimer();
		                    }

		                    return return_value;
		                }

		                // IsCycleComplete should always return OK in this case.  Also, was_interrupted
		                // will always be false because if this "aSleepDuration < 1" call really
		                // was interrupted, it would already have returned in the hotkey cases
		                // of the switch().  UPDATE: was_interrupted can now the hotkey case in
		                // the switch() doesn't return, relying on us to do it after making sure
		                // the queue is empty.
		                // The below is checked here rather than in IsCycleComplete() because
		                // that function is sometimes called more than once prior to returning
		                // (e.g. empty_the_queue_via_peek) and we only want this to be decremented once:
		                if (IsCycleComplete(aSleepDuration, start_time, allow_early_return) != ResultType.Fail) // v1.0.44.11: IsCycleComplete() must be called for all modes, but now its return value is checked due to the new g_DeferMessagesForUnderlyingPump mode.
		                    return RETURN_FROM_MSGSLEEP();

		                // Otherwise (since above didn't return) combined logic has ensured that all of the following are true:
		                // 1) aSleepDuration > 0
		                // 2) !empty_the_queue_via_peek
		                // 3) The above two combined with logic above means that g_DeferMessagesForUnderlyingPump==true.
		                System.Threading.Thread.Sleep(5); // Since Peek() didn't find a message, avoid maxing the CPU.  This is a somewhat arbitrary value: the intent of a value below 10 is to avoid yielding more than one timeslice on all systems even if they have unusual timeslice sizes / system timers.
		                continue;
		            }

		            // else Peek() found a message, so process it below.
		        } // PeekMessage() vs. GetMessage()

		        // Since above didn't return or "continue", a message has been received that is eligible
		        // for further processing.
		        ++messages_received;
		        var wparam = msg.wParam.ToInt32();

		        // For max. flexibility, it seems best to allow the message filter to have the first
		        // crack at looking at the message, before even TRANSLATE_AHK_MSG:
		        if (msgMonitor.mCount != 0 && MsgMonitor(msg.hwnd, msg.message, msg.wParam, msg.lParam, msg, ref msg_reply))  // Count is checked here to avoid function-call overhead.
		        {
		            continue; // MsgMonitor has returned "true", indicating that this message should be omitted from further processing.
		            // NOTE: Above does "continue" and ignores msg_reply.  This is because testing shows that
		            // messages received via Get/PeekMessage() were always sent via PostMessage.  If an
		            // another thread sends ours a message, MSDN implies that Get/PeekMessage() internally
		            // calls the message's WindowProc directly and sends the reply back to the other thread.
		            // That makes sense because it seems unlikely that DispatchMessage contains any means
		            // of replying to a message because it has no way of knowing whether the MSG struct
		            // arrived via Post vs. SendMessage.
		        }

		        // If this message might be for one of our GUI windows, check that before doing anything
		        // else with the message.  This must be done first because some of the standard controls
		        // also use WM_USER messages, so we must not assume they're generic thread messages just
		        // because they're >= WM_USER.  The exception is AHK_GUI_ACTION should always be handled
		        // here rather than by IsDialogMessage().
		        // UPDATE: MSDN isn't clear about which messages IsDialogMessage() dispatches, so it's
		        // now only used for keyboard handling.  This reduces code complexity a little and
		        // eliminates some uncertainty about message routing.  All of the cases for WM_USER
		        // range messages below already checked msg.hwnd to ensure it is one of our messages.
		        //if (g_firstGui // Checked first to help performance, since all messages must come through this bottleneck.
		        if (mainWindow != null//Assuming the Keysharp equivalent of g_firstGui is the main window.
		                && msg.message >= WindowsAPI.WM_KEYFIRST && msg.message <= WindowsAPI.WM_KEYLAST
		                && GuiHelper.GuiFromHwnd(msg.hwnd) is Gui tempgui) // Ordered for short-circuit performance.
		        {
		            pgui = tempgui;
		            focused_control = msg.hwnd; // Alias for maintainability.  Seems more appropriate (and efficient) to use this vs. GetFocus().
		            pcontrol = Control.FromHandle(focused_control);
		            //AHK checks here to see if they key combo is meant to be for an menu item accelerator, but I am not sure
		            //how to get the accelerator handle for a Winforms menu. Searching Google, Bing and DDG returned *zero* results for
		            //how do it. Unsure if it will even be possible in Keysharp. It may not be doable.//TODO
		            //if (pgui.mAccel) // v1.1.04: Keyboard accelerators.
		            //  if (TranslateAccelerator(pgui->mHwnd, pgui->mAccel, &msg))
		            //      continue; // Above call handled it.

		            // Relies heavily on short-circuit boolean order:
		            if (  msg.message == WindowsAPI.WM_KEYDOWN && pgui.form.GetAllControlsRecusrvive<TabControl>().Count > 0
		                    && (wparam == WindowsAPI.VK_NEXT || wparam == WindowsAPI.VK_PRIOR || wparam == WindowsAPI.VK_TAB
		                        || wparam == WindowsAPI.VK_LEFT || wparam == WindowsAPI.VK_RIGHT)
		                    && (pcontrol is Control) && !(pcontrol is HotkeyBox))
		            {
		                var ptab_control = pcontrol as TabControl;

		                if (ptab_control != null) // The focused control is a tab control itself.
		                {
		                    // For the below, note that Alt-left and Alt-right are automatically excluded,
		                    // as desired, since any key modified only by alt would be WM_SYSKEYDOWN vs. WM_KEYDOWN.
		                    if (wparam == WindowsAPI.VK_LEFT || wparam == WindowsAPI.VK_RIGHT)
		                    {
		                        ptab_control.SelectedIndex = Math.Clamp(ptab_control.SelectedIndex + (wparam == WindowsAPI.VK_RIGHT ? 1 : -1), 0, ptab_control.TabCount);
		                        // Pass false for both the above since that's the whole point of having arrow
		                        // keys handled separately from the below: Focus should stay on the tabs
		                        // rather than jumping to the first control of the tab, it focus should not
		                        // wrap around to the beginning or end (to conform to standard behavior for
		                        // arrow keys).
		                        continue; // Suppress this key even if the above failed (probably impossible in this case).
		                    }

		                    //else fall through to the next part.
		                }

		                // If focus is in a multiline edit control, don't act upon Control-Tab (and
		                // shift-control-tab -> for simplicity & consistency) since Control-Tab is a special
		                // keystroke that inserts a literal tab in the edit control:
		                if (   wparam != WindowsAPI.VK_LEFT && wparam != WindowsAPI.VK_RIGHT
		                        && (WindowsAPI.GetKeyState(WindowsAPI.VK_CONTROL) & 0x8000) != 0 // Even if other modifiers are down, it still qualifies. Use GetKeyState() vs. GetAsyncKeyState() because the former's definition is more suitable.
		                        && (wparam != WindowsAPI.VK_TAB || !(pcontrol is TextBox txtcontrol)
		                            || !(txtcontrol?.Multiline == true)))
		                {
		                    // If ptab_control wasn't determined above, check if focused control is owned by a tab control:
		                    if (ptab_control == null && (ptab_control = pgui.form.GetParentOfType<TabControl>()) == null)
		                        // Fall back to the first tab control (for consistency & simplicity, seems best
		                        // to always use the first rather than something fancier such as "nearest in z-order".
		                        ptab_control = pgui.form.GetNthControlRecusrvive<TabControl>(0);

		                    if (ptab_control != null && ptab_control.Enabled)
		                    {
		                        var tempindex = ptab_control.SelectedIndex + (wparam == WindowsAPI.VK_NEXT || (wparam == WindowsAPI.VK_TAB && (WindowsAPI.GetKeyState(WindowsAPI.VK_SHIFT) & 0x8000) != 0) ? 1 : -1);

		                        if (tempindex < 0)
		                            tempindex = ptab_control.TabCount - 1;
		                        else if (tempindex >= ptab_control.TabCount)
		                            tempindex = 0;

		                        ptab_control.SelectedIndex = tempindex;
		                        // Update to the below: Must suppress the tab key at least, to prevent it
		                        // from navigating *and* changing the tab.  And since this one is suppressed,
		                        // might as well suppress the others for consistency.
		                        // Older: Since WM_KEYUP is not handled/suppressed here, it seems best not to
		                        // suppress this WM_KEYDOWN either (it should do nothing in this case
		                        // anyway, but for balance this seems best): Fall through to the next section.
		                        continue;
		                    }

		                    //else fall through to the below.
		                }

		                //else fall through to the below.
		            } // Interception of keystrokes for navigation in tab control.

		            // v1.0.34: Fix for the fact that a multiline edit control will send WM_CLOSE to its parent
		            // when user presses ESC while it has focus.  The following check is similar to the block's above.
		            // The alternative to this approach would have been to override the edit control's WindowProc,
		            // but the following seemed to be less code. Although this fix is only necessary for multiline
		            // edits, its done for all edits since it doesn't do any harm.  In addition, there is no need to
		            // check what modifiers are down because we never receive the keystroke for Ctrl-Esc and Alt-Esc
		            // (the OS handles those beforehand) and both Win-Esc and Shift-Esc are identical to a naked Esc
		            // inside an edit.  The following check relies heavily on short-circuit eval. order.
		            if (   msg.message == WindowsAPI.WM_KEYDOWN
		                    && (wparam == WindowsAPI.VK_ESCAPE || wparam == WindowsAPI.VK_TAB // v1.0.38.03: Added VK_TAB handling for "WantTab".
		                        || (wparam == (int)'A' && (WindowsAPI.GetKeyState(WindowsAPI.VK_CONTROL) & 0x8000) != 0 // v1.0.44: Added support for "WantCtrlA".
		                            && (WindowsAPI.GetKeyState(WindowsAPI.VK_RMENU) & 0x8000) == 0)) // v1.1.17: Exclude AltGr+A (Ctrl+Alt+A).
		                    && pcontrol is TextBox txt
		                    && txt.Tag is GuiControl gctrl)
		            {
		                switch (wparam)
		                {
		                    case 'A': // v1.0.44: Support for Ctrl-A to select all text.
		                        if (!gctrl.AltSubmit)// i.e. presence of AltSubmit bit DISABLES Ctrl-A handling.
		                        {
		                            txt.SelectAll();
		                            continue;// Omit this keystroke from any further processing.
		                        }

		                        break;

		                    case WindowsAPI.VK_ESCAPE:
		                        gctrl.Gui.form.BeginInvoke(() => gctrl.Gui.Form_KeyDown(gctrl.Gui.form, new System.Windows.Forms.KeyEventArgs(Keys.Escape)));
		                        continue; // Omit this keystroke from any further processing.

		                    default: // VK_TAB
		                        if (txt.AcceptsTab) // It has the "WantTab" property.
		                        {
		                            // For flexibility, do this even for single-line edit controls, though in that
		                            // case the tab keystroke will produce an "empty box" character.
		                            // Strangely, if a message pump other than this one (MsgSleep) is running,
		                            // such as that of a MsgBox, "WantTab" is already in effect unconditionally,
		                            // perhaps because MsgBox and others respond to WM_GETDLGCODE with DLGC_WANTTAB.
		                            txt.SelectedText = "\t";
		                            continue; // Omit this keystroke from any further processing.
		                        }

		                        break;
		                } // switch()
		            }

		            if (pcontrol is TreeView tv && tv.SelectedNode is TreeNode node && node.IsEditing && msg.message == WindowsAPI.WM_KEYDOWN)
		            {
		                if (wparam == WindowsAPI.VK_RETURN)
		                {
		                    node.EndEdit(false);
		                    continue;
		                }
		                else if (wparam == WindowsAPI.VK_ESCAPE)
		                {
		                    node.EndEdit(true);
		                    continue;
		                }
		            }

		            // IsDialogMessage() takes care of standard keyboard handling within the dialog,
		            // such as tab to change focus and Enter to activate the default button.
		            // Avoid calling IsDialogMessage() for WM_SYSCHAR if the GUI has no controls.
		            // It seems that if a GUI has no controls, IsDialogMessage() will return true for
		            // Alt+n combinations without invoking the default processing, such as focusing
		            // a menu bar item.  IsDialogMessage() still needs to be called for some messages;
		            // at the very least, WM_KEYDOWN (VK_ESC) must be intercepted for GuiEscape to work.
		            if (pgui.form.Controls.Count > 0 || msg.message != WindowsAPI.WM_SYSCHAR)
		            {
		                calledByIsDialogMessageOrDispatch = true;
		                calledByIsDialogMessageOrDispatchMsg = msg.message; // Added in v1.0.44.11 because it's known that IsDialogMessage can change the message number (e.g. WM_KEYDOWN->WM_NOTIFY for UpDowns)
		                msg_was_handled = WindowsAPI.IsDialogMessage(pgui.form.Handle, msg); // Pass the dialog HWND, not msg.hwnd, which is often a control.
		                calledByIsDialogMessageOrDispatch = false;

		                if (msg_was_handled) // This message was handled by IsDialogMessage() above.
		                    continue; // Continue with the main message loop.
		            }
		        } // if (keyboard message posted to GUI)

		        // v1.0.44: There's no reason to call TRANSLATE_AHK_MSG here because all WM_COMMNOTIFY messages
		        // are sent to g_hWnd. Thus, our call to DispatchMessage() later below will route such messages to
		        // MainWindowProc(), which will then call TRANSLATE_AHK_MSG().
		        //TRANSLATE_AHK_MSG(msg.message, wparam)

		        // MSG_FILTER_MAX should prevent us from receiving this first group of messages whenever g_AllowInterruption or
		        // g->AllowThreadToBeInterrupted is false.
		        //Needed to pull this out of the switch because fall through is not allowed in C#.
		        if (msg.message == (uint)UserMessages.AHK_HOOK_HOTKEY ||  // Sent from this app's keyboard or mouse hook.
		                msg.message == (uint)UserMessages.AHK_HOTSTRING ||    // Sent from keybd hook to activate a non-auto-replace hotstring.
		                msg.message == (uint)UserMessages.AHK_CLIPBOARD_CHANGE)
		        {
		            // This extra handling is present because common controls and perhaps other OS features tend
		            // to use WM_USER+NN messages, a practice that will probably be even more common in the future.
		            // To cut down on message conflicts, dispatch such messages whenever their HWND isn't a what
		            // it should be for a message our own code generated. That way, the target control (if any)
		            // will still get the msg.
		            if (msg.hwnd != IntPtr.Zero && msg.hwnd != mainWindow.Handle) // v1.0.44: It's wasn't sent by our code; perhaps by a common control's code.
		                goto break_out_of_main_switch; // Dispatch it vs. discarding it, in case it's for a control.
		        }

		        switch (msg.message)
		        {
		            //ELSE FALL THROUGH:
		            case (uint)UserMessages.AHK_GUI_ACTION:   // The user pressed a button on a GUI window, or some other actionable event. Listed before the below for performance.
		            case WindowsAPI.WM_HOTKEY:        // As a result of this app having previously called RegisterHotkey(), or from TriggerJoyHotkeys().
		            case (uint)UserMessages.AHK_USER_MENU:    // The user selected a custom menu item.
		            case (uint)UserMessages.AHK_INPUT_END:    // Input ended (sent by the hook thread).
		            case (uint)UserMessages.AHK_INPUT_KEYDOWN:
		            case (uint)UserMessages.AHK_INPUT_CHAR:
		            case (uint)UserMessages.AHK_INPUT_KEYUP:
		            {
		                hdrop_to_free = IntPtr.Zero;  // Set default for this message's processing (simplifies code).

		                switch (msg.message)
		                {
		                    case (uint)UserMessages.AHK_GUI_ACTION: // Listed first for performance.

		                        // Assume that it is possible that this message's GUI window has been destroyed
		                        // (and maybe even recreated) since the time the msg was posted.  If this can happen,
		                        // that's another reason for finding which GUI this control is associate with (it also
		                        // needs to be found so that we can call the correct GUI window object to perform
		                        // the action):
		                        if ((pgui = GuiHelper.GuiFromHwnd(msg.hwnd, true)) == null) // No associated GUI object, so ignore this event.
		                            // v1.0.44: Dispatch vs. continue/discard since it's probably for a common control
		                            // whose msg number happens to be AHK_GUI_ACTION.  Do this *only* when HWND isn't recognized,
		                            // not when msg content is invalid, because dispatching a msg whose HWND is one of our own
		                            // GUI windows might cause GuiWindowProc to fwd it back to us, creating an infinite loop.
		                            goto break_out_of_main_switch; // Goto seems preferably in this case for code size & performance.

		                        gui_event_info = (ulong)msg.lParam.ToInt64();
		                        gui_action =        (GuiEventTypes)(wparam & 0xFF); // Byte 0.
		                        gui_event_byte =    (wparam >> 8) & 0xFF; // Byte 1.  Sometimes used as an additional parameter.
		                        gui_control_index = (wparam >> 16) & 0xFF; // Bytes 2-3.  Caller has set it to NO_CONTROL_INDEX if it isn't applicable.
		                        gui_event_arg_count = 0;
		                        gui_event_code = (uint)gui_action; // Set default.  Using both this and gui_action simplifies some things.
		                        var tempctrl = gui_control_index < pgui.form.Controls.Count ? pgui.form.Controls[gui_control_index] : null; // Set for use in other places below. This indexing won't work, need to figure it out.//TODO

		                        if (tempctrl != null)
		                            pguicontrol = tempctrl.Tag as GuiControl;

		                        check_if_running = pgui.monitorEvents; // Set default.
		                        event_is_control_generated = false; // Set default.
		                        gui_event_kind = GuiEventKinds.GUI_EVENTKIND_EVENT; // Set default.

		                        switch (gui_action)
		                        {
		                            case GuiEventTypes.GUI_EVENT_CLOSE:
		                            case GuiEventTypes.GUI_EVENT_ESCAPE:
		                            case GuiEventTypes.GUI_EVENT_RESIZE:
		                                break;

		                            case GuiEventTypes.GUI_EVENT_CONTEXTMENU:
		                                // Must allow multiple threads because otherwise the user cannot right-click twice consecutively
		                                // (the second click is blocked because the menu is still displayed at the instant of the click).
		                                check_if_running = null;
		                                break;

		                            case GuiEventTypes.GUI_EVENT_DROPFILES:
		                                //Unsure how or even if we should do this.//TODO

		                                    hdrop_to_free = pgui->mHdrop; // This variable simplifies the code further below.

		                                    if (   !hdrop_to_free // Checked just in case, so that the below can query it.
		                                        || !(gui_event_info = DragQueryFile(hdrop_to_free, 0xFFFFFFFF, NULL, 0))   ) // Probably impossible, but if it ever can happen, seems best to ignore it.
		                                    {
		                                    if (hdrop_to_free) // Checked again in case short-circuit boolean above never checked it.
		                                    {
		                                        DragFinish(hdrop_to_free); // Since the drop-thread will not be launched, free the memory.
		                                        pgui->mHdrop = NULL; // Indicate that this GUI window is ready for another drop.
		                                    }

		                                    continue;
		                                    }

		        // mHdrop != NULL prevents another DROPFILES event from being queued, up until the
		        // message is received here and we remove WS_EX_ACCEPTFILES.  After that the user
		        // can no longer drop files onto the window, but it may still be necessary to check
		        // for running instances in case of:
		        //  1) Another WM_DROPFILES already in the queue, but not yet processed.
		        //  2) Someone posting WM_DROPFILES (i.e. fake drag and drop).
		        break;

		                            case GuiEventTypes.GUI_EVENT_WM_COMMAND:
		                                gui_event_kind = GuiEventKinds.GUI_EVENTKIND_COMMAND;
		                                gui_event_code = (uint)gui_event_info;
		                                goto default;

		                            // Fall through:
		                            default: // This is an action from a particular control in the GUI window.
		                                if (pguicontrol == null) // gui_control_index was beyond the quantity of controls, possibly due to parent window having been destroyed since the msg was sent (or bogus msg).
		                                    continue;  // Discarding an invalid message here is relied upon both other sections below.

		                                // It seems best by default not to allow multiple threads for the same event.
		                                // Such events are discarded because it seems likely that most script designers
		                                // would want to see the effects of faulty design (e.g. long running timers or
		                                // hotkeys that interrupt gui threads) rather than having events for later,
		                                // when they might suddenly take effect unexpectedly.
		                                check_if_running = pguicontrol.monitorEvents;
		                                event_is_control_generated = true; // As opposed to a drag-and-drop or context-menu event that targets a specific control.
		                                break;
		                        } // switch(gui_action)

		                        // IsMonitoring() isn't checked at this stage for performance and code size, and because
		                        // it was checked prior to posting AHK_GUI_ACTION.  If the handler was removed since then,
		                        // the section below will do some unnecessary but harmless work.
		                        if (check_if_running != null && check_if_running.IsRunning(gui_event_code, (int)gui_event_kind))
		                        {
		                            //Unsure how to do drag/drop. Need to figure out later.//TODO

		                                if (hdrop_to_free) // Unlikely but possible, in theory.  Doesn't seem to affect code size.
		                                {
		                                DragFinish(hdrop_to_free); // Since the drop-thread will not be launched, free the memory.
		                                pgui->mHdrop = NULL; // Indicate that this GUI window is ready for another drop.
		                                }

		                            continue;
		                        }

		                        priority = 0;  // Always use default for now.
		                        break; // case AHK_GUI_ACTION

		                    case (uint)UserMessages.AHK_USER_MENU: // user-defined menu item
		                        if (wparam != 0) // Poster specified that this menu item was from a gui's menu bar (since wParam is unsigned, any incoming -1 is seen as greater than max).
		                        {
		                            // The menu type is passed with the message so that its value will be in sync with
		                            // the timestamp of the message (in case this message has been stuck in the queue
		                            // for a long time).
		                            // wparam is the HWND rather than a pointer to avoid any chance of problems with
		                            // a gui object having been destroyed while the msg was waiting in the queue.
		                            if (!(pgui = GuiType::FindGui((HWND)wparam)) // Not a GUI's menu bar item...
		                                    && msg.hwnd && msg.hwnd != g_hWnd) // ...and not a script menu item.
		                                goto break_out_of_main_switch; // See "goto break_out_of_main_switch" higher above for complete explanation.
		                        }
		                        else
		                            pgui = null; // Set for use in later sections.

		                        if (   !(menu_item = g_script.FindMenuItemByID((UINT)msg.lParam))   ) // Item not found.
		                            continue; // ignore the msg

		                        // And just in case a menu item that lacks a label (such as a separator) is ever
		                        // somehow selected (perhaps via someone sending direct messages to us, bypassing
		                        // the menu):
		                        if (!menu_item->mCallback)
		                            continue;

		                        // Ignore/discard a hotkey or custom menu item event if the current thread's priority
		                        // is higher than it's:
		                        priority = menu_item->mPriority;
		                        break;

		                            case AHK_HOTSTRING:
		                                if (wparam >= Hotstring::sHotstringCount) // Invalid hotstring ID (perhaps spoofed by external app)
		                                    continue; // Do nothing.

		                                hs = Hotstring::shs[wparam];

		                                if (hs->mHotCriterion)
		                                {
		                                    // For details, see comments in the hotkey section of this switch().
		                                    if (   !(criterion_found_hwnd = HotCriterionAllowsFiring(hs->mHotCriterion, hs->mName))   )
		                                        // Hotstring is no longer eligible to fire even though it was when the hook sent us
		                                        // the message.  Abort the firing even though the hook may have already started
		                                        // executing the hotstring by suppressing the final end-character or other actions.
		                                        // It seems preferable to abort midway through the execution than to continue sending
		                                        // keystrokes to the wrong window, or when the hotstring has become suspended.
		                                        continue;

		                                    // For details, see comments in the hotkey section of this switch().
		                                    if (!(hs->mHotCriterion->Type == HOT_IF_ACTIVE || hs->mHotCriterion->Type == HOT_IF_EXIST))
		                                        criterion_found_hwnd = NULL; // For "NONE" and "NOT", there is no last found window.
		                                }
		                                else // No criterion, so it's a global hotstring.  It can always fire, but it has no "last found window".
		                                    criterion_found_hwnd = NULL;

		                                // Do a simple replacement for the hotstring if that's all that's called for.
		                                // Don't create a new quasi-thread or any of that other complexity done further
		                                // below.  But also do the backspacing (if specified) for a non-autoreplace hotstring,
		                                // even if it can't launch due to MaxThreads, MaxThreadsPerHotkey, or some other reason:
		                                hs->DoReplace(msg.lParam);  // Does only the backspacing if it's not an auto-replace hotstring.

		                                if (hs->mReplacement) // Fully handled by the above; i.e. it's an auto-replace hotstring.
		                                    continue;

		                                // Otherwise, continue on and let a new thread be created to handle this hotstring.
		                                // Since this isn't an auto-replace hotstring, set this value to support
		                                // the built-in variable A_EndChar:
		                                g_script.mEndChar = hs->mEndCharRequired ? (TCHAR)LOWORD(msg.lParam) : 0; // v1.0.48.04: Explicitly set 0 when hs->mEndCharRequired==false because LOWORD is used for something else in that case.
		                                priority = hs->mPriority;
		                                break;

		                            case AHK_CLIPBOARD_CHANGE: // Due to the registration of an OnClipboardChange function in the script.
		                                if (g_script.mOnClipboardChangeIsRunning)
		                                    continue;

		                                priority = 0;  // Always use default for now.
		                                break;

		                            case AHK_INPUT_END:
		                                input_hook = InputRelease((input_type*)wparam);  // The function will verify that it is a valid input_type pointer.

		                                if (!input_hook)
		                                    continue; // No OnEnd callback/invalid message.

		                                priority = 0;
		                                break;

		                            case AHK_INPUT_KEYDOWN:
		                            case AHK_INPUT_CHAR:
		                            case AHK_INPUT_KEYUP:
		                                for (input_hook = g_input; input_hook && input_hook != (input_type*)wparam; input_hook = input_hook->Prev);

		                                if (!input_hook)
		                                    continue; // Invalid message or Input already ended (and therefore may have been deleted).

		                                if (!(msg.message == AHK_INPUT_KEYDOWN ? input_hook->ScriptObject->onKeyDown
		                                        : msg.message == AHK_INPUT_KEYUP ? input_hook->ScriptObject->onKeyUp
		                                        : input_hook->ScriptObject->onChar))
		                                    continue;

		                                priority = 0;
		                                break;

		                            default: // hotkey
		                                hk_id = wparam & HOTKEY_ID_MASK;

		                                if (hk_id >= Hotkey::sHotkeyCount) // Invalid hotkey ID.
		                                    continue;

		                                hk = Hotkey::shk[hk_id];
		                                // Check if criterion allows firing.
		                                // For maintainability, this is done here rather than a little further down
		                                // past the g_MaxThreadsTotal and thread-priority checks.  Those checks hardly
		                                // ever abort a hotkey launch anyway.
		                                //
		                                // If message is WM_HOTKEY, it's either:
		                                // 1) A joystick hotkey from TriggerJoyHotkeys(), in which case the lParam is ignored.
		                                // 2) A hotkey message sent by the OS, in which case lParam contains currently-unused info set by the OS.
		                                //
		                                // An incoming WM_HOTKEY can be subject to #HotIf Win. at this stage under the following conditions:
		                                // 1) Joystick hotkey, because it relies on us to do the check so that the check is done only
		                                //    once rather than twice.
		                                // 2) #HotIf Win. keybd hotkeys that were made non-hook because they have a non-suspended, global variant.
		                                //
		                                // If message is AHK_HOOK_HOTKEY:
		                                // Rather than having the hook pass the qualified variant to us, it seems preferable
		                                // to search through all the criteria again and rediscover it.  This is because conditions
		                                // may have changed since the message was posted, and although the hotkey might still be
		                                // eligible for firing, a different variant might now be called for (e.g. due to a change
		                                // in the active window).  Since most criteria hotkeys have at most only a few criteria,
		                                // and since most such criteria are #HotIf WinActive rather than Exist, the performance will
		                                // typically not be reduced much at all.  Furthermore, trading performance for greater
		                                // reliability seems worth it in this case.
		                                //
		                                // The inefficiency of calling HotCriterionAllowsFiring() twice for each hotkey --
		                                // once in the hook and again here -- seems justified for the following reasons:
		                                // - It only happens twice if the hotkey a hook hotkey (multi-variant keyboard hotkeys
		                                //   that have a global variant are usually non-hook, even on NT/2k/XP).
		                                // - The hook avoids doing its first check of WinActive/Exist if it sees that the hotkey
		                                //   has a non-suspended, global variant.  That way, hotkeys that are hook-hotkeys for
		                                //   reasons other than #HotIf Win. (such as mouse, overriding OS hotkeys, or hotkeys
		                                //   that are too fancy for RegisterHotkey) will not have to do the check twice.
		                                // - It provides the ability to set the last-found-window for #HotIf WinActive/Exist
		                                //   (though it's not needed for the "Not" counterparts).  This HWND could be passed
		                                //   via the message, but that would require malloc-there and free-here, and might
		                                //   result in memory leaks if its ever possible for messages to get discarded by the OS.
		                                // - It allows hotkeys that were eligible for firing at the time the message was
		                                //   posted but that have since become ineligible to be aborted.  This seems like a
		                                //   good precaution for most users/situations because such hotkey subroutines will
		                                //   often assume (for scripting simplicity) that the specified window is active or
		                                //   exists when the subroutine executes its first line.
		                                // - Most criterion hotkeys use #HotIf WinActive(), which is a very fast call.  Also, although
		                                //   WinText and/or "SetTitleMatchMode 'Slow'" slow down window searches, those are rarely
		                                //   used too.
		                                //
		                                variant = NULL; // Set default.

		                                // For #HotIf hotkey variants, we don't want to evaluate the expression a second time. If the hook
		                                // thread determined that a specific variant should fire, it is passed via the high word of wParam:
		                                if (variant_id = HIWORD(wparam))
		                                {
		                                    // The following relies on the fact that variants can't be removed or re-ordered;
		                                    // variant_id should always be the variant's one-based index in the linked list:
		                                    --variant_id; // i.e. index 1 should be mFirstVariant, not mFirstVariant->mNextVariant.

		                                    for (variant = hk->mFirstVariant; variant_id; variant = variant->mNextVariant, --variant_id);
		                                }

		                                if (   !(variant || (variant = hk->CriterionAllowsFiring(&criterion_found_hwnd
		                                                               , msg.message == AHK_HOOK_HOTKEY ? KEY_IGNORE_LEVEL(HIWORD(msg.lParam)) : 0)))   )
		                                    continue; // No criterion is eligible, so ignore this hotkey event (see other comments).

		                                // If this is AHK_HOOK_HOTKEY, criterion was eligible at time message was posted,
		                                // but not now.  Seems best to abort (see other comments).

		                                // Due to the key-repeat feature and the fact that most scripts use a value of 1
		                                // for their #MaxThreadsPerHotkey, this check will often help average performance
		                                // by avoiding a lot of unnecessary overhead that would otherwise occur:
		                                if (!hk->PerformIsAllowed(*variant))
		                                {
		                                    // The key is buffered in this case to boost the responsiveness of hotkeys
		                                    // that are being held down by the user to activate the keyboard's key-repeat
		                                    // feature.  This way, there will always be one extra event waiting in the queue,
		                                    // which will be fired almost the instant the previous iteration of the subroutine
		                                    // finishes (this above description applies only when MaxThreadsPerHotkey is 1,
		                                    // which it usually is).
		                                    hk->RunAgainAfterFinished(*variant); // Wheel notch count (g->EventInfo below) should be okay because subsequent launches reuse the same thread attributes to do the repeats.
		                                    continue;
		                                }

		                                // Now that above has ensured variant is non-NULL:
		                                HotkeyCriterion* hc = variant->mHotCriterion;

		                                if (!hc || hc->Type == HOT_IF_NOT_ACTIVE || hc->Type == HOT_IF_NOT_EXIST)
		                                    criterion_found_hwnd = NULL; // For "NONE" and "NOT", there is no last found window.
		                                else if (HOT_IF_REQUIRES_EVAL(hc->Type))
		                                    criterion_found_hwnd = g_HotExprLFW; // For #HotIf WinExist(WinTitle) and similar.

		                                priority = variant->mPriority;
		                } // switch(msg.message)

		                    if (g_nThreads >= g_MaxThreadsTotal)
		                    {
		                    // Allow only a limited number of recursion levels to avoid any chance of
		                    // stack overflow.  So ignore this message.  Later, can devise some way
		                    // to support "queuing up" these launch-thread events for use later when
		                    // there is "room" to run them, but that might cause complications because
		                    // in some cases, the user didn't intend to hit the key twice (e.g. due to
		                    // "fat fingers") and would have preferred to have it ignored.  Doing such
		                    // might also make "infinite key loops" harder to catch because the rate
		                    // of incoming hotkeys would be slowed down to prevent the subroutines from
		                    // running concurrently.
		                    if (hdrop_to_free) // This is only non-NULL when pgui is non-NULL and gui_action==GUI_EVENT_DROPFILES
		                    {
		                        DragFinish(hdrop_to_free); // Since the drop-thread will not be launched, free the memory.
		                        pgui->mHdrop = NULL; // Indicate that this GUI window is ready for another drop.
		                    }

		                    if (msg.message == AHK_INPUT_END)
		                        input_hook->ScriptObject->Release();

		                    continue;
		                    // If the above "continued", it seems best not to re-queue/buffer the key since
		                    // it might be a while before the number of threads drops back below the limit.
		                    }

		                    // Discard the event if it's priority is lower than that of the current thread:
		                    if (priority < g->Priority)
		                    {
		                    if (hdrop_to_free) // This is only non-NULL when pgui is non-NULL and gui_action==GUI_EVENT_DROPFILES
		                    {
		                        DragFinish(hdrop_to_free); // Since the drop-thread will not be launched, free the memory.
		                        pgui->mHdrop = NULL; // Indicate that this GUI window is ready for another drop.
		                    }

		                    if (msg.message == AHK_INPUT_END)
		                        input_hook->ScriptObject->Release();

		                    continue;
		                    }

		                    // Now it is certain that the new thread will be launched, so set everything up.
		                    // Perform the new thread's subroutine:
		                    return_value = true; // We will return this value to indicate that we launched at least one new thread.

		                    // UPDATE v1.0.48: The main timer is no longer killed because testing shows that
		                    // SetTimer() and/or KillTimer() are relatively slow calls.  Thus it is likely that
		                    // on average, it's better to receive some unnecessary WM_TIMER messages (especially
		                    // since WM_TIMER processing is fast when there's nothing to do) than it is to
		                    // KILL and then RE-SET the main timer for every new thread (especially rapid-fire
		                    // threads like some GUI threads can be).  This also makes the thread types that are
		                    // handled here more like other threads such as timers, callbacks, and OnMessage.
		                    // Also, not doing the following KILL_MAIN_TIMER avoids have to check whether
		                    // SET_MAIN_TIMER is needed in two places further below (e.g. RETURN_FROM_MSGSLEEP).
		                    // OLDER COMMENTS:
		                    // Always kill the main timer, for performance reasons and for simplicity of design,
		                    // prior to embarking on new subroutine whose duration may be long (e.g. the called
		                    // subroutine may not return to us for seconds, minutes, or more; during which time
		                    // we don't want the timer running because it will only fill up the queue with
		                    // WM_TIMER messages and thus hurt performance).
		                    // UPDATE: But don't kill it if it should be always-on to support the existence of
		                    // at least one enabled timed subroutine or joystick hotkey:
		                    //if (!g_script.mTimerEnabledCount && !Hotkey::sJoyHotkeyCount)
		                    //  KILL_MAIN_TIMER;

		                    switch (msg.message)
		                    {
		                    case AHK_GUI_ACTION: // Listed first for performance.
		                    case AHK_CLIPBOARD_CHANGE:
		                    case AHK_INPUT_END:
		                    case AHK_INPUT_KEYDOWN:
		                    case AHK_INPUT_CHAR:
		                    case AHK_INPUT_KEYUP:
		                    case AHK_USER_MENU: // user-defined menu item
		                        break; // Do nothing at this stage.

		                    default: // hotkey or hotstring
		                        // Just prior to launching the hotkey, update these values to support built-in
		                        // variables such as A_TimeSincePriorHotkey:
		                        g_script.mPriorHotkeyName = g_script.mThisHotkeyName;
		                        g_script.mPriorHotkeyStartTime = g_script.mThisHotkeyStartTime;
		                        // Unlike hotkeys -- which can have a name independent of their label by being created or updated
		                        // with the HOTKEY command -- a hot string's unique name is always its label since that includes
		                        // the options that distinguish between (for example) :c:ahk:: and ::ahk::
		                        g_script.mThisHotkeyName = (msg.message == AHK_HOTSTRING) ? hs->mName : hk->mName;
		                        g_script.mThisHotkeyStartTime = GetTickCount(); // Fixed for v1.0.35.10 to not happen for GUI
		                    }

		                    // Make every newly launched subroutine start off with the global default values that
		                    // the user set up in the auto-execute part of the script (e.g. KeyDelay, WinDelay, etc.).
		                    InitNewThread(priority, false, true);
		                    global_struct& g = *::g; // ONLY AFTER above is it safe to "lock in". Reduces code size a bit (31 bytes currently) and may improve performance.  Eclipsing ::g with local g makes compiler remind/enforce the use of the right one.
		                    // Do this nearly last, right before launching the thread:
		                    g_script.mLastPeekTime = GetTickCount();

		                    // v1.0.38.04: The above now resets mLastPeekTime too to reduce situations in which a thread
		                    // doesn't even run one line before being interrupted by another thread.  Here's how that would
		                    // happen: ExecUntil() would see that a Peek() is due and call PeekMessage().  The Peek() will
		                    // yield if we have no messages and the CPU is under heavy load, and thus the script might not
		                    // get another timeslice for 20ms (or even longer if there is more than one other needy process).
		                    // Even if the Peek() doesn't yield (i.e. we have messages), those messages might take a long time
		                    // to process (such as WM_PAINT) even though the script is uninterruptible.  Either way, when the
		                    // Peek-check completes, a long time might have passed, and the thread might now be interruptible
		                    // due to the interruptible-timer having expired (which is probably possible only in the no-yield
		                    // scenario above, since in the case of yield, ExecUntil wouldn't check messages again after the
		                    // yield).  Thus, the Peek-check's MsgSleep() might launch an interrupting thread before the prior
		                    // thread had a chance to execute even one line.  Resetting mLastPeekTime above should alleviate that,
		                    // perhaps even completely resolve it due to the way tickcounts tend not to change early on in
		                    // a timeslice (perhaps because timeslices fall exactly upon tick-count boundaries).  If it doesn't
		                    // completely resolve it, mLastPeekTime could instead be set to zero as a special value that
		                    // ExecUntil recognizes to do the following processing, but this processing reduces performance
		                    // by 2.5% in a simple addition-loop benchmark:
		                    //if (g_script.mLastPeekTime)
		                    //  LONG_OPERATION_UPDATE
		                    //else
		                    //  g_script.mLastPeekTime = GetTickCount();

		                    switch (msg.message)
		                    {
		                    case AHK_GUI_ACTION: // Listed first for performance.
		                    {
		                    #define EVT_ARG_ADD(_value) gui_event_args[gui_event_arg_count++].SetValue(_value)

		                        if (event_is_control_generated || pgui->mEventSink != pgui)
		                            // Set first argument
		                            EVT_ARG_ADD(event_is_control_generated ? (IObject*)pcontrol : (IObject*)pgui);

		                        switch (gui_action)
		                        {
		                            case GUI_EVENT_CONTEXTMENU:
		                                bool from_keyboard; // True if this context-menu was generated via the keyboard (such as AppsKey or Shift-F10):
		                                from_keyboard = gui_event_byte;
		                                gui_point = msg.pt; // Set default. v1.0.38: More accurate/customary to use msg.pt than GetCursorPos().

		                                if (pcontrol) // i.e. this context menu is for a control rather than a click somewhere in the parent window itself.
		                                {
		                                    // By definition, pcontrol should be the focused control.  However, testing shows that
		                                    // it can also be the only control in a window that lacks any focus-capable controls.
		                                    // If the window has no controls at all, testing shows that pcontrol will be NULL,
		                                    // in which case GuiPoint default set earlier is retained (for AppsKey too).
		                                    if (from_keyboard)
		                                        pgui->ControlGetPosOfFocusedItem(*pcontrol, gui_point); // Since pcontrol!=NULL, find out which item is focused in this control.

		                                    //else this is a context-menu event that was invoked via normal mouse click.  Leave
		                                    // g.GuiPoint at its default set earlier.
		                                    switch (pcontrol->type)
		                                    {
		                                        case GUI_CONTROL_LISTBOX:
		                                            // Retrieve the focused item, which is not necessarily the one which was clicked.
		                                            // LB_ITEMFROMPOINT could be used to get the item at the mouse position, but since
		                                            // the control doesn't provide any visual feedback that the item was clicked, it
		                                            // doesn't feel right.  We could provide feedback by automatically selecting the
		                                            // item, but that would be non-trivial due to single- vs. multi-select, and it
		                                            // might not be what the user wants anyway.
		                                            gui_event_info = 1 + (int)SendMessage(pcontrol->hwnd, LB_GETCARETINDEX, 0, 0); // Cast to int to preserve any -1 value.
		                                            break;

		                                        case GUI_CONTROL_LISTVIEW:
		                                            if (from_keyboard) // AppsKey or Shift+F10.
		                                                gui_event_info = 1 + ListView_GetNextItem(pcontrol->hwnd, -1, LVNI_FOCUSED);

		                                            // Otherwise, this event should have been generated by NM_RCLICK, in which case
		                                            // gui_event_info already contains the row number.
		                                            break;

		                                        case GUI_CONTROL_TREEVIEW:

		                                            // Retrieve the HTREEITEM that is the true target of this event.
		                                            if (from_keyboard) // AppsKey or Shift+F10.
		                                                gui_event_info = (DWORD_PTR)SendMessage(pcontrol->hwnd, TVM_GETNEXTITEM, TVGN_CARET, NULL); // Get focused item.
		                                            else // Context menu invoked via right-click.  Find out which item (if any) was clicked on.
		                                            {
		                                                // Use HitTest because the focused item isn't necessarily the one that was
		                                                // clicked on.  This is because unlike a ListView, right-clicking a TreeView
		                                                // item doesn't change the focus.  Instead, the target item is momentarily
		                                                // selected for visual confirmation and then unselected.
		                                                TVHITTESTINFO ht;
		                                                ht.pt = msg.pt;
		                                                ScreenToClient(pcontrol->hwnd, &ht.pt);
		                                                gui_event_info = (DWORD_PTR)TreeView_HitTest(pcontrol->hwnd, &ht);
		                                            }

		                                            break;
		                                    }
		                                }

		                                //else pcontrol==NULL: Since there is no focused control, it seems best to report the
		                                // cursor's position rather than some arbitrary center-point, or top-left point in the
		                                // parent window.  This is because it might be more convenient for the user to move the
		                                // mouse to select a menu item (since menu will be close to mouse cursor).
		                                // Convert to client coordinates, since that's what we use for positioning controls,
		                                // and it's usually the default Menu CoordMode.  Since the script might not use the
		                                // coordinates to display a menu, it seems best to use this even if the script has
		                                // changed the default CoordMode.
		                                ScreenToClient(pgui->mHwnd, &gui_point);

		                                // Build event arguments.
		                                if (pcontrol)
		                                    EVT_ARG_ADD(pcontrol);
		                                else
		                                    EVT_ARG_ADD(_T(""));

		                                EVT_ARG_ADD((__int64)gui_event_info);
		                                EVT_ARG_ADD(!from_keyboard); // True if mouse-activated.
		                                EVT_ARG_ADD(gui_point.x);
		                                EVT_ARG_ADD(gui_point.y);
		                                break; // case GUI_CONTEXT_MENU.

		                            case GUI_EVENT_DROPFILES:
		                                gui_point = msg.pt; // v1.0.38: More accurate/customary to use msg.pt than GetCursorPos().
		                                ScreenToClient(pgui->mHwnd, &gui_point); // Seems more useful/appropriate than window coordinates, especially now that Client CoordMode is the starting default.
		                                // Visually indicate that drops aren't allowed while and existing drop is still being
		                                // processed. Fix for v1.0.31.02: The window's current ExStyle is fetched every time
		                                // in case a non-GUI command altered it (such as making it transparent):
		                                SetWindowLong(pgui->mHwnd, GWL_EXSTYLE, GetWindowLong(pgui->mHwnd, GWL_EXSTYLE) & ~WS_EX_ACCEPTFILES);

		                                // Build event arguments.
		                                if (pcontrol)
		                                    EVT_ARG_ADD(pcontrol);
		                                else
		                                    EVT_ARG_ADD(_T(""));

		                                EVT_ARG_ADD(GuiType::CreateDropArray(hdrop_to_free)); // Currently must be the third-last arg; see "Free the drop array."
		                                EVT_ARG_ADD(pgui->Unscale(gui_point.x));
		                                EVT_ARG_ADD(pgui->Unscale(gui_point.y));
		                                // Free the drop object.
		                                DragFinish(hdrop_to_free);
		                                pgui->mHdrop = NULL;
		                                break;

		                            case GUI_EVENT_CLOSE:
		                            case GUI_EVENT_ESCAPE:
		                            case GUI_EVENT_RESIZE:
		                                if (gui_action == GUI_EVENT_RESIZE)
		                                {
		                                    // Build event arguments.
		                                    EVT_ARG_ADD((char)gui_event_byte); // MinMax state
		                                    EVT_ARG_ADD(pgui->Unscale(LOWORD(gui_event_info))); // Width
		                                    EVT_ARG_ADD(pgui->Unscale(HIWORD(gui_event_info))); // Height
		                                }

		                                break;

		                            case GUI_EVENT_WM_COMMAND: // Control-generated, but no additional parameters.
		                                break;

		                            default: // Other control-generated event (i.e. event_is_control_generated==true).
		                                if (pcontrol->type == GUI_CONTROL_LINK && gui_action == GUI_EVENT_CLICK)
		                                {
		                                    LITEM item;
		                                    item.mask = LIF_URL | LIF_ITEMID | LIF_ITEMINDEX;
		                                    item.iLink = (int)gui_event_info - 1;

		                                    if (!SendMessage(pcontrol->hwnd, LM_GETITEM, NULL, (LPARAM)&item))
		                                    {
		                                        // Make sure they are initialized to sensible values.
		                                        item.szID = '\0';
		                                        item.szUrl = '\0';
		                                    }

		                                    if (*item.szID)
		                                        EVT_ARG_ADD(item.szID); // ID attribute.
		                                    else
		                                        EVT_ARG_ADD((int)gui_event_info); // One-based index.

		                                    EVT_ARG_ADD(item.szUrl); // Empty unless it had a href which failed to execute.
		                                    break;
		                                }

		                                EVT_ARG_ADD((__int64)gui_event_info);

		                                switch (gui_action)
		                                {
		                                    case GUI_EVENT_ITEMSELECT:
		                                        if (pcontrol->type != GUI_CONTROL_LISTVIEW)
		                                            break; // Should be a TreeView, which does not raise an event for de-selection.

		                                    // Otherwise, this is a ListView, so add the parameter indicating whether this
		                                    // is selection or de-selection:
		                                    case GUI_EVENT_ITEMCHECK: // Parameter indicates checked (1) vs unchecked (0).
		                                    case GUI_EVENT_ITEMEXPAND: // Parameter indicates expanded (1) vs collapsed (0).
		                                        EVT_ARG_ADD((int)gui_event_byte - 1);
		                                        break;
		                                }
		                        } // switch (msg.message)

		                        pgui->AddRef(); // Keep the pointer valid at least until the thread finishes.
		                        ResultType result;
		                        INT_PTR retval;

		                        if (gui_action == GUI_EVENT_CONTEXTMENU && pcontrol)
		                        {
		                            // Call the control's context menu handler, if any, omitting the "Gui" parameter
		                            // for consistency with other Ctrl events (and perhaps convenience).
		                            result = pcontrol->events.Call(gui_event_args + 1, gui_event_arg_count - 1, gui_event_code, gui_event_kind, pgui);

		                            if (result == EARLY_RETURN // Suppress the GUI's handler for this event, if any.
		                                    || !pgui->mHwnd) // Gui was destroyed.
		                            {
		                                pgui->Release();
		                                break;
		                            }
		                        }

		                        MsgMonitorList& events = event_is_control_generated ? pcontrol->events : pgui->mEvents;
		                        result = events.Call(gui_event_args, gui_event_arg_count, gui_event_code, gui_event_kind, pgui, &retval);

		                        if (pgui->mHwnd) // i.e. GUI was not destroyed.
		                        {
		                            switch (gui_action)
		                            {
		                                case GUI_EVENT_CLOSE:

		                                    // If the return value is false/unspecified, hide/destroy the Gui.
		                                    if (!retval)
		                                        pgui->Cancel();

		                                    break;

		                                case GUI_EVENT_DROPFILES:
		                                    if (pgui->IsMonitoring(gui_action)) // Reapply the style only if we're still monitoring this event.
		                                    {
		                                        // The window's current ExStyle is fetched every time in case a non-GUI
		                                        // command altered it (such as making it transparent):
		                                        SetWindowLong(pgui->mHwnd, GWL_EXSTYLE, GetWindowLong(pgui->mHwnd, GWL_EXSTYLE) | WS_EX_ACCEPTFILES);
		                                    }

		                                    break;
		                            }
		                        }

		                        if (gui_action == GUI_EVENT_DROPFILES) // Must be done regardless of pgui->mHwnd.
		                            gui_event_args[gui_event_arg_count - 3].object->Release(); // Free the drop array.

		                        // Counteract the earlier AddRef(). If the Gui was destroyed (and none of this
		                        // Gui's other labels are still running), this will free the Gui structure.
		                        pgui->Release();
		                        break;
		                    } // case AHK_GUI_ACTION

		                    case AHK_USER_MENU: // user-defined menu item
		                    {
		                        // Below: the menu type is passed with the message so that its value will be in sync
		                        // with the timestamp of the message (in case this message has been stuck in the
		                        // queue for a long time):
		                        if (pgui) // Set by an earlier stage. It means poster specified that this menu item was from a gui's menu bar.
		                        {
		                            // As documented, set the last found window if possible/applicable.  It's not necessary to
		                            // check IsWindow/IsWindowVisible/DetectHiddenWindows since GetValidLastUsedWindow()
		                            // takes care of that whenever the script actually tries to use the last found window.
		                            g.hWndLastUsed = pgui->mHwnd; // OK if NULL.
		                            // This flags GUI menu items as being GUI so that the script has a way of detecting
		                            // whether a given submenu's item was selected from inside a menu bar vs. a popup:
		                            g.EventInfo = (EventInfoType)pgui->mHwnd;
		                            pgui->AddRef();
		                        }

		                        UserMenu* menu = menu_item->mMenu; // In case the item is deleted.
		                        menu->AddRef();
		                        ExprTokenType param[] =
		                        {
		                            menu_item->mName, // This should be immediately copied into a script variable, so it's okay if menu_item is deleted.
		                            (__int64)(menu_item->Pos() + 1), // +1 to convert zero-based to one-based.
		                            menu
		                        };
		                        menu_item->mCallback->ExecuteInNewThread(_T("Menu"), param, _countof(param));
		                        menu->Release();

		                        if (pgui)
		                            pgui->Release();

		                        break;
		                    }

		                    case AHK_HOTSTRING:
		                        g.hWndLastUsed = criterion_found_hwnd; // v1.0.42. Even if the window is invalid for some reason, IsWindow() and such are called whenever the script accesses it (GetValidLastUsedWindow()).
		                        g.SendLevel = hs->mInputLevel;
		                        g.HotCriterion = hs->mHotCriterion; // v2: Let the Hotkey command use the criterion of this hotstring by default.
		                        hs->PerformInNewThreadMadeByCaller();
		                        break;

		                    case AHK_CLIPBOARD_CHANGE:
		                    {
		                        int type = CountClipboardFormats() ? (IsClipboardFormatAvailable(CF_NATIVETEXT) || IsClipboardFormatAvailable(CF_HDROP) ? 1 : 2) : 0;
		                        ExprTokenType param ((__int64)type);
		                        g_script.mOnClipboardChangeIsRunning = true;
		                        DEBUGGER_STACK_PUSH(_T("OnClipboardChange"))
		                        g_script.mOnClipboardChange.Call(&param, 1, 1);
		                        DEBUGGER_STACK_POP()
		                        g_script.mOnClipboardChangeIsRunning = false;
		                        break;
		                    }

		                    case AHK_INPUT_END:
		                    {
		                        ExprTokenType param = input_hook->ScriptObject;
		                        IObjectPtr(input_hook->ScriptObject->onEnd)->ExecuteInNewThread(_T("InputHook"), &param, 1);
		                        input_hook->ScriptObject->Release();
		                        break;
		                    }

		                    case AHK_INPUT_KEYDOWN:
		                    case AHK_INPUT_KEYUP:
		                    {
		                        ExprTokenType params[] =
		                        {
		                            input_hook->ScriptObject,
		                            __int64(vk_type(msg.lParam)),
		                            __int64(sc_type(msg.lParam >> 16)),
		                        };
		                        IObjectPtr onKey = msg.message == AHK_INPUT_KEYDOWN ? input_hook->ScriptObject->onKeyDown : input_hook->ScriptObject->onKeyUp;
		                        onKey->ExecuteInNewThread(_T("InputHook"), params, _countof(params));
		                        break;
		                    }

		                    case AHK_INPUT_CHAR:
		                    {
		                        TCHAR chars[] = { TCHAR(msg.lParam), TCHAR(msg.lParam >> 16), '\0' };
		                        ExprTokenType params[] =
		                        {
		                            input_hook->ScriptObject,
		                            chars
		                        };
		                        IObjectPtr(input_hook->ScriptObject->onChar)->ExecuteInNewThread(_T("InputHook"), params, _countof(params));
		                        break;
		                    }

		                    default: // hotkey
		                        if (IS_WHEEL_VK(hk->mVK)) // If this is true then also: msg.message==AHK_HOOK_HOTKEY
		                            g.EventInfo = (DWORD)msg.lParam; // v1.0.43.03: Override the thread default of 0 with the number of notches by which the wheel was turned.

		                        // Above also works for RunAgainAfterFinished since that feature reuses the same thread attributes set above.
		                        g.hWndLastUsed = criterion_found_hwnd; // v1.0.42. Even if the window is invalid for some reason, IsWindow() and such are called whenever the script accesses it (GetValidLastUsedWindow()).
		                        g.SendLevel = variant->mInputLevel;
		                        g.HotCriterion = variant->mHotCriterion; // v2: Let the Hotkey command use the criterion of this hotkey variant by default.
		                        hk->PerformInNewThreadMadeByCaller(*variant);
		                    }

		                    // v1.0.37.06: Call ResumeUnderlyingThread() even if aMode==WAIT_FOR_MESSAGES; this is for
		                    // maintainability and also in case the pause command has been used to unpause the idle thread.
		                    ResumeUnderlyingThread();

		                    // DUE TO THE --g DONE BY THE LINE ABOVE, ANYTHING BETWEEN THIS POINT AND THE NEXT '}' MUST USE :;g INSTEAD OF g.

		                    if (aMode == WAIT_FOR_MESSAGES) // This is the idle state, so we were called directly from WinMain().
		                    continue; // The above condition is possible only when the AutoExec section had ended prior to the thread we just launched.

		                    // Otherwise a script thread other than the idle thread has just been resumed.
		                    if (IsCycleComplete(aSleepDuration, start_time, allow_early_return))
		                    {
		                    // Check for messages once more in case the subroutine that just completed
		                    // above didn't check them that recently.  This is done to minimize the time
		                    // our thread spends *not* pumping messages, which in turn minimizes keyboard
		                    // and mouse lag if the hooks are installed (even though this is no longer
		                    // true due to v1.0.39's dedicated hook thread, it seems best to continue this
		                    // practice to maximize responsiveness of hotkeys, the app itself [e.g. tray
		                    // menu], and also to retain backward compatibility).  Set the state of this
		                    // function/layer/instance so that it will use peek-mode.  UPDATE: Don't change
		                    // the value of aSleepDuration to -1 because IsCycleComplete() needs to know the
		                    // original sleep time specified by the caller to determine whether
		                    // to decrement g_nLayersNeedingTimer:
		                    empty_the_queue_via_peek = true;
		                    allow_early_return = true;
		                    // And now let it fall through to the "continue" statement below.
		                    }

		                    // and now if the cycle isn't complete, stay in the blessed GetMessage() state until the time
		                    // has expired.
		                    continue;
		                    } // End of cases that launch new threads, such as hotkeys and GUI events.

		                    case WM_TIMER:
		                    if (msg.lParam // This WM_TIMER is intended for a TimerProc...
		                        || msg.hwnd != g_hWnd) // ...or it's intended for a window other than the main window, which implies that it doesn't belong to program internals (i.e. the script is probably using it). This fix was added in v1.0.47.02 and it also fixes the ES_NUMBER balloon bug.
		                    break; // Fall through to let a later section do DispatchMessage() on it.

		                    // It seems best to poll the joystick for every WM_TIMER message (i.e. every 10ms or so on
		                    // NT/2k/XP).  This is because if the system is under load, it might be 20ms, 40ms, or even
		                    // longer before we get a timeslice again and that is a long time to be away from the poll
		                    // (a fast button press-and-release might occur in less than 50ms, which could be missed if
		                    // the polling frequency is too low):
		                    POLL_JOYSTICK_IF_NEEDED // Do this first since it's much faster.
		                    CHECK_SCRIPT_TIMERS_IF_NEEDED

		                    if (aMode == WAIT_FOR_MESSAGES)
		                    // Timer should have already been killed if we're in this state.
		                    // But there might have been some WM_TIMER msgs already in the queue
		                    // (they aren't removed when the timer is killed).  Or perhaps
		                    // a caller is calling us with this aMode even though there
		                    // are suspended subroutines (currently never happens).
		                    // In any case, these are always ignored in this mode because
		                    // the caller doesn't want us to ever return.  UPDATE: This can now
		                    // happen if there are any enabled timed subroutines we need to keep an
		                    // eye on, which is why the mTimerEnabledCount value is checked above
		                    // prior to starting a new iteration.
		                    continue;

		                    if (aSleepDuration < 1) // In this case, WM_TIMER messages have already fulfilled their function, above.
		                    continue;

		                    // Otherwise aMode == RETURN_AFTER_MESSAGES:
		                    // Realistically, there shouldn't be any more messages in our queue
		                    // right now because the queue was stripped of WM_TIMER messages
		                    // prior to the start of the loop, which means this WM_TIMER
		                    // came in after the loop started.  So in the vast majority of
		                    // cases, the loop would have had enough time to empty the queue
		                    // prior to this message being received.  Therefore, just return rather
		                    // than trying to do one final iteration in peek-mode (which might
		                    // complicate things, i.e. the below function makes certain changes
		                    // in preparation for ending this instance/layer, only to be possibly,
		                    // but extremely rarely, interrupted/recursed yet again if that final
		                    // peek were to detect a recursable message):
		                    if (IsCycleComplete(aSleepDuration, start_time, allow_early_return))
		                    RETURN_FROM_MSGSLEEP
		                    // Otherwise, stay in the blessed GetMessage() state until the time has expired:
		                    continue;

		                    case WM_CANCELJOURNAL:
		                    // IMPORTANT: It's tempting to believe that WM_CANCELJOURNAL might be lost/dropped if the script
		                    // is displaying a MsgBox or other dialog that has its own msg pump (since such a pump would
		                    // discard any msg with a NULL HWND).  However, that is not true in this case because such a dialog's
		                    // msg pump would be beneath this one on the call stack.  This is because our caller is calling us in
		                    // a loop that does not permit the script to display any *additional* dialogs.  Thus, our msg pump
		                    // here should always receive the OS-generated WM_CANCELJOURNAL msg reliably.
		                    // v1.0.44: This message is now received only when the user presses Ctrl-Alt-Del or Ctrl-Esc during
		                    // playback. For performance and simplicity, the playback hook itself no longer sends this message,
		                    // instead directly sets g_PlaybackHook = NULL to notify the installer of the hook that it's gone.
		                    g_PlaybackHook = NULL; // A signal for caller.
		                    empty_the_queue_via_peek = true;
		                    // Above is set to so that we return faster, since our caller should be SendKeys() whenever
		                    // WM_CANCELJOURNAL is received, and SendKeys() benefits from a faster return.
		                    continue;

		                    case WM_KEYDOWN:
		                    if (msg.hwnd == g_hWndEdit && wparam == VK_ESCAPE)
		                    {
		                    // This won't work if a MessageBox() window is displayed because its own internal
		                    // message pump will dispatch the message to our edit control, which will just
		                    // ignore it.  And avoiding setting the focus to the edit control won't work either
		                    // because the user can simply click in the window to set the focus.  But for now,
		                    // this is better than nothing:
		                    ShowWindow(g_hWnd, SW_HIDE);  // And it's okay if this msg gets dispatched also.
		                    continue;
		                    }

		                    // Otherwise, break so that the messages will get dispatched.  We need the other
		                    // WM_KEYDOWN msgs to be dispatched so that the cursor is keyboard-controllable in
		                    // the edit window:
		                    break;

		                    case WM_QUIT:
		                    // The app normally terminates before WM_QUIT is ever seen here because of the way
		                    // WM_CLOSE is handled by MainWindowProc().  However, this is kept here in case anything
		                    // external ever explicitly posts a WM_QUIT to our thread's queue:
		                    g_script.ExitApp(EXIT_CLOSE);
		                    continue; // Since ExitApp() won't necessarily exit.
		            } // switch()
		            break_out_of_main_switch:
		            ;

		                    // If a "continue" statement wasn't encountered somewhere in the switch(), we want to
		                    // process this message in a more generic way.
		                    // This little part is from the Miranda source code.  But it doesn't seem
		                    // to provide any additional functionality: You still can't use keyboard
		                    // keys to navigate in the dialog unless it's the topmost dialog.
		                    // UPDATE: The reason it doesn't work for non-topmost MessageBoxes is that
		                    // this message pump isn't even the one running.  It's the pump of the
		                    // top-most MessageBox itself, which apparently doesn't properly dispatch
		                    // all types of messages to other MessagesBoxes.  However, keeping this
		                    // here is probably a good idea because testing reveals that it does
		                    // sometimes receive messages intended for MessageBox windows (which makes
		                    // sense because our message pump here retrieves all thread messages).
		                    // It might cause problems to dispatch such messages directly, since
		                    // IsDialogMessage() is supposed to be used in lieu of DispatchMessage()
		                    // for these types of messages.
		                    // NOTE: THE BELOW IS CONFIRMED to be needed, at least for a FileSelect()
		                    // dialog whose quasi-thread has been suspended, and probably for some of the other
		                    // types of dialogs as well:
		                    if ((fore_window = GetForegroundWindow()) != NULL  // There is a foreground window.
		                    && GetWindowThreadProcessId(fore_window, NULL) == g_MainThreadID) // And it belongs to our main thread (the main thread is the only one that owns any windows).
		                    {
		                    GetClassName(fore_window, wnd_class_name, _countof(wnd_class_name));

		                    if (!_tcscmp(wnd_class_name, _T("#32770")))  // MsgBox, InputBox, FileSelect, DirSelect dialog.
		                    {
		                    g->CalledByIsDialogMessageOrDispatch = true; // In case there is any way IsDialogMessage() can call one of our own window proc's rather than that of a MsgBox, etc.
		                    g->CalledByIsDialogMessageOrDispatchMsg = msg.message; // Added in v1.0.44.11 because it's known that IsDialogMessage can change the message number (e.g. WM_KEYDOWN->WM_NOTIFY for UpDowns)

		                    if (IsDialogMessage(fore_window, &msg))  // This message is for it, so let it process it.
		                    {
		                    g->CalledByIsDialogMessageOrDispatch = false;
		                    continue;  // This message is done, so start a new iteration to get another msg.
		                    }

		                    g->CalledByIsDialogMessageOrDispatch = false;
		                    }
		                    }

		                    // Translate keyboard input for any of our thread's windows that need it:
		                    if (!g_hAccelTable || !TranslateAccelerator(g_hWnd, g_hAccelTable, &msg))
		                    {
		                    g->CalledByIsDialogMessageOrDispatch = true; // Relies on the fact that the types of messages we dispatch can't result in a recursive call back to this function.
		                    g->CalledByIsDialogMessageOrDispatchMsg = msg.message; // Added in v1.0.44.11. Do it prior to Translate & Dispatch in case either one of them changes the message number (it is already known that IsDialogMessage can change message numbers).
		                    TranslateMessage(&msg);
		                    DispatchMessage(&msg); // This is needed to send keyboard input and other messages to various windows and for some WM_TIMERs.
		                    g->CalledByIsDialogMessageOrDispatch = false;
		                    }
		        } // infinite-loop

		        return false;
		*/

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

		/// <summary>
		/// AHK does all sorts of complicated checking for this function which appears to be due to how it
		/// simulates threading. Keysharp uses real threads and real timers, so it doesn't appear that such complexity is needed.
		/// </summary>
		/// <returns></returns>
		//internal static bool CheckScriptTimers() => Keysharp.Core.Flow.timers != null&& Keysharp.Core.Flow.timers.Count > 0;
	}
}