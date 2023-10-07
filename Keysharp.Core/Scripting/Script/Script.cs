using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
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
		public static bool ForceKeybdHook;
		internal const int INTERVAL_UNSPECIFIED = int.MinValue + 303;

		internal const int SLEEP_INTERVAL = 10;

		internal const int SLEEP_INTERVAL_HALF = SLEEP_INTERVAL / 2;

		internal static bool calledByIsDialogMessageOrDispatch;

		internal static uint calledByIsDialogMessageOrDispatchMsg;

		internal static List<IFuncObj> ClipFunctions = new List<IFuncObj>();

		internal static bool deferMessagesForUnderlyingPump;

		[ThreadStatic]
		internal static IFuncObj hotCriterion;

		internal static List<IFuncObj> hotCriterions = new List<IFuncObj>();
		internal static IntPtr hotExprLFW = IntPtr.Zero;
		internal static List<IFuncObj> hotExprs = new List<IFuncObj>();
		internal static bool hsResetUponMouseClick = Parser.HotstringNoMouse;

		internal static bool hsSameLineAction;

		[ThreadStatic]
		internal static IntPtr hwndLastUsed = IntPtr.Zero;

		internal static InputType input;

		// Last Found Window of last #HotIf expression.
		internal static DateTime inputTimeoutAt = DateTime.Now;

		internal static bool inputTimerExists;

		internal static bool isReadyToExecute;

		internal static IFuncObj lastHotFunc;

		internal static MainWindow mainWindow;

		internal static Gui mainWindowGui;

		internal static int MAX_THREADS_LIMIT = 0xFF;

		internal static Keysharp.Core.Common.Keyboard.MenuType menuIsVisible = MenuType.None;

		internal static int nLayersNeedingTimer;

		internal static List<IFuncObj> OnErrorHandlers;
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

		internal static bool validateThenExit;

		private static IntPtr mainWindowHandle;

		public static bool ResetUponMouseClick => hsResetUponMouseClick;

		public static Variables Vars { get; private set; }

		internal static Keysharp.Core.Common.Threading.HookThread HookThread { get; private set; }

		internal static bool IsMainWindowClosing => mainWindow == null || mainWindow.IsClosing;

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
				_ = Dialogs.MsgBox("Cannot edit a compiled script.");
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

				object pid = null;

				if (!string.IsNullOrEmpty(ed))
				{
					var splits = ed.Split('%');
					ed = splits.Length > 0 ? splits[0] : ed;
					_ = Processes.Run(ed, Accessors.A_ScriptDir, "", ref pid, Accessors.A_ScriptFullPath);
				}
				else
					_ = Processes.Run($"Notepad.exe", Accessors.A_ScriptDir, "", ref pid, Accessors.A_ScriptFullPath);
			}
			else
			{
				wi.Active = true;
			}
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
						_ = Flow.ExitApp(Flow.ExitReasons.SingleInstance);

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
							_ = Flow.ExitApp(Flow.ExitReasons.SingleInstance);
					}

					break;
			}
		}

		public static string ListKeyHistory()
		{
			var sb = new StringBuilder(2048);
			var target_window = Core.Common.Window.WindowManagerProvider.Instance.GetForeGroundWindow();
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
			_ = sb.AppendLine($"Enabled timers: {enabledTimers} of {Flow.timers.Count} ({timerlist})");
			_ = sb.AppendLine($"Threads: {totalExistingThreads}");
			_ = sb.AppendLine($"Modifiers (GetKeyState() now) = {mod}");
			_ = sb.AppendLine(hookstatus);
			_ = sb.Append(cont);
			return sb.ToString();
		}

		public static void ListLines(params object[] obj) => throw new Error("ListLines() is not supported in Keysharp because it's a compiled program, not an interpreted one.");

		public static void ListVars() => mainWindow?.SetText(Reflections.GetVariableInfo(), MainWindow.MainFocusedTab.Stack);

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
					mainWindow.SetText(text, MainWindow.MainFocusedTab.Debug);
				else
					mainWindow.AddText(text, MainWindow.MainFocusedTab.Debug);
		}

		public static void RunMainWindow(string title, Func<object> userInit)
		{
			mainWindow = new MainWindow();

			if (!string.IsNullOrEmpty(title))
				mainWindow.Text = title + " - Keysharp v" + Accessors.A_AhkVersion;

			mainWindow.ClipboardUpdate += PrivateClipboardUpdate;
			mainWindow.WindowState = FormWindowState.Minimized;
			//mainWindow.WindowState = FormWindowState.Maximized;
			//mainWindow.ShowInTaskbar = false;//The main window is a system tray window only.
			mainWindow.Icon = Core.Properties.Resources.Keysharp_ico;
			Parser.Persistent = true;
			mainWindowGui = new Gui(null, null, null, mainWindow);
			mainWindow.Show();
			_ = mainWindow.BeginInvoke(() =>
			{
				_ = userInit();
				//This has to be done here because it uses the window handle to register hotkeys, and the handle isn't valid until mainWindow.Load() is called.
				HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();//We want these active now in case auto-execute never returns (e.g. loop));
				isReadyToExecute = true;
			});
			Application.Run(mainWindow);
		}

		public static void SetName(string s) => scriptName = s;

		public static void SimulateKeyPress(uint key) => HookThread.SimulateKeyPress(key);

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