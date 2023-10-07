using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Scripting;
using ThreadState = System.Threading.ThreadState;
using Timer = System.Timers.Timer;

namespace Keysharp.Core
{
	public static class Flow
	{
		internal static ConcurrentDictionary<string, FuncObj> cachedFuncObj = new ConcurrentDictionary<string, FuncObj>();

		// Use some negative value unlikely to ever be passed explicitly:
		internal static int IntervalUnspecified = int.MinValue + 303;

		internal static Timer mainTimer;
		internal static int NoSleep = -1;
		internal static ConcurrentDictionary<FuncObj, System.Windows.Forms.Timer> timers = new ConcurrentDictionary<FuncObj, System.Windows.Forms.Timer>();
		internal static bool hasExited;
		internal static bool persistentValueSetByUser;
		[ThreadStatic]
		private static System.Windows.Forms.Timer currentTimer;

		/// <summary>
		/// Is the Script currently suspended?
		/// </summary>
		public static bool Suspended { get; internal set; }

		internal static bool AllowInterruption { get; set; }

		public static void Init()
		{
			hasExited = false;
		}

		/// <summary>
		/// Prevents the current thread from being interrupted by other
		/// </summary>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>On</term>: <description>give the current thread the highest priority.</description></item>
		/// <item><term>Off</term>: <description>resets the current thread priority to normal.</description></item>
		/// </list>
		/// </param>
		public static void Critical(object obj = null)
		{
			var on = obj == null || (Options.OnOff(obj.As()) ?? true);
			var p = on ? ThreadPriority.Highest : ThreadPriority.Normal;

			if (!Keysharp.Scripting.Script.isReadyToExecute)
				Accessors.threadPriorityDef = p;

			System.Threading.Thread.CurrentThread.Priority = p;
		}

		public static long EnabledTimerCount()
		{
			var ct = 0L;

			foreach (var kv in timers)
				if (kv.Value.Enabled)
					ct++;

			return ct;
		}

		/// <summary>
		/// Exits the current thread or the entire program if non-persistent.
		/// </summary>
		/// <param name="exitCode">An integer that is returned to the caller.</param>
		public static void Exit(object obj = null)
		{
			var exitCode = obj.Ai();
			Environment.ExitCode = exitCode;
			Application.ExitThread();
		}

		/// <summary>
		/// Terminates the program unconditionally.
		/// </summary>
		/// <param name="exitCode">An integer that is returned to the caller.</param>
		public static bool ExitApp(object obj = null) => ExitAppInternal(ExitReasons.Exit, obj);

		/// <summary>
		/// Specifies a label to run automatically when the program exits.
		/// </summary>
		/// <param name="label">The name of a label. Leave blank to remove an existing label, if any.</param>
		public static void OnExit(object obj0, object obj1 = null) => Script.OnExitHandlers.ModifyEventHandlers(Function.GetFuncObj(obj0, null, true), obj1.Al(1L));

		/// <summary>
		/// Specifies a function to call automatically when the program receives the specified message.
		/// </summary>
		/// <param name="number">The number of the message to monitor.</param>
		/// <param name="function">The name of a function to call whenever the specified message is received.</param>
		/// <param name="maxThreads">The maximum number of concurrent threads to launch per message number.</param>
		public static void OnMessage(object obj0, object obj1, object obj2 = null) => GuiHelper.onMessageHandlers.GetOrAdd(obj0.Al()).ModifyEventHandlers(Function.GetFuncObj(obj1, null, true), obj2.Al(1));

		/// <summary>
		/// Pauses the current thread.
		/// </summary>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>Toggle</term> (default): <description>pauses the current thread unless the thread beneath it is paused, in which case the underlying thread is unpaused.</description></item>
		/// <item><term>On</term>: <description>pauses the current thread.</description></item>
		/// <item><term>Off</term>: <description>if the thread beneath the current thread is paused, it will be in an unpaused state when resumed.</description></item>
		/// </list>
		/// </param>
		/// <param name="parentThread">
		/// <list type="bullet">
		/// <item><term>0</term>: <description>pause the current thread.</description></item>
		/// <item><term>1</term>: <description>marks the thread beneath the current thread as paused so that when it resumes, it will finish the command it was running (if any) and then enter a paused state. If there is no thread beneath the current thread, the program itself is paused, which prevents timers from running.</description></item>
		/// </list>
		/// </param>
		/*
		    public static void Pause(object obj)
		    {
		    var mode = obj.As();
		    var thread = System.Threading.Thread.CurrentThread;
		    var state = Options.OnOff(mode);

		    if (state == null && mode.Equals(Core.Keyword_Toggle, System.StringComparison.OrdinalIgnoreCase))
		        state = !(thread.ThreadState == ThreadState.Suspended || thread.ThreadState == ThreadState.SuspendRequested);

		    //Should figure out the right way to do this.//TODO
		    #pragma warning disable 612, 618

		    if (state == true)
		    {
		        if (!(bool)Accessors.A_IconFrozen && !Parser.NoTrayIcon)
		            Script.Tray.Icon = Keysharp.Core.Properties.Resources.Keysharp_p_ico;

		        thread.Suspend();
		    }
		    else if (state == false)
		    {
		        thread.Resume();

		        if (!(bool)Accessors.A_IconFrozen && !Parser.NoTrayIcon)
		            Script.Tray.Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
		    }

		    #pragma warning restore 612, 618
		    }
		*/

		public static object Persistent(object obj = null)
		{
			var b = obj.Ab(true);
			var old = Parser.Persistent;
			Parser.Persistent = persistentValueSetByUser = b;
			return old;
		}

		/// <summary>
		/// Replaces the currently running instance of the program with a new one.
		/// </summary>
		public static void Reload()
		{
			if (!ExitAppInternal(ExitReasons.Reload))
				Application.Restart();//This will pass the same command line args to the new instance that were passed to this instance.y
		}

		public static void SetTimer(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var function = obj0;
			var period = obj1.Al(long.MaxValue);
			var priority = obj2.Al();
			FuncObj func = null;
			var once = period < 0;

			if (once)
				period = -period;

			if (function is string s)//Make sure they don't keep adding the same function object via string.
			{
				if (cachedFuncObj.TryGetValue(s, out var tempfunc))
					func = tempfunc;
				else
					cachedFuncObj[s] = func = Keysharp.Core.Misc.FuncObj(s);
			}

			if (function != null && func == null)
			{
				func = function is FuncObj fo ?
					   fo : throw new TypeError($"Parameter {function} of type {function.GetType()} was not a string or a function object.");
			}

			//func can now only ever be accessed by the caller from within the timer event, as the first argument.
			System.Windows.Forms.Timer timer = null;

			if (func != null && timers.TryGetValue(func, out timer))
			{
			}
			else if (function == null)
				timer = currentTimer;//Not thread safe;

			if (timer != null)
			{
				if (period == 0)
				{
					if (func != null)
						_ = timers.TryRemove(func, out _);
					else
					{
						var temptimer = timers.Where((kv) => kv.Value == timer).FirstOrDefault();

						if (temptimer.Key != null)
						{
							_ = timers.TryRemove(temptimer.Key, out var _);
						}
					}

					timer.Stop();
					timer.Dispose();
					return;
				}
				else
				{
					if (period == long.MaxValue)
					{
						timer.Stop();
						timer.Start();
					}
					else
						timer.Interval = (int)period;

					return;
				}
			}
			else if (period != 0)
			{
				if (period == long.MaxValue)
					period = 250;

				_ = timers.TryAdd(func, timer = new System.Windows.Forms.Timer());
				timer.Interval = (int)period;
			}
			else//They tried to stop a timer that didn't exist
				return;

			var level = ThreadPriority.Normal;

			if (priority > -1 && priority < 5)
				level = (ThreadPriority)priority;

			timer.Tick += (ss, ee) =>
						  //timer.Elapsed += (ss, ee) =>
			{
				if (ss is System.Windows.Forms.Timer t)
				{
					t.Enabled = false;

					var remove = false;

					System.Threading.Thread.CurrentThread.Priority = level;

					//If there are threads and NoTimers is set, then this shouldn't run. Revisit when threads are implemented, specifically the Thread() function.//TODO
					try
					{
						//_ = func.Call(func, Conversions.ToYYYYMMDDHH24MISS(ee.SignalTime));
						currentTimer = t;//Not thread safe;
						_ = func.Call(func, Conversions.ToYYYYMMDDHH24MISS(DateTime.Now));

						if (timers.TryGetValue(func, out var existing))//They could have disabled it, in which case it wouldn't be in the dictionary.
							existing.Enabled = true;
					}
					catch (Exception)
					{
						remove = true;
					}
					finally
					{
						currentTimer = null;
					}

					if (once || remove)
					{
						_ = timers.TryRemove(func, out _);
						t.Stop();
						t.Dispose();
					}
				}
			};
			Keysharp.Scripting.Script.mainWindow.CheckedInvoke(timer.Start, true);
		}

		/// <summary>
		/// Waits the specified amount of time before continuing.
		/// </summary>
		/// <param name="Delay">The amount of time to pause in milliseconds.</param>
		public static void Sleep(object obj = null)
		{
			var delay = obj.Al(-1L);

			//Be careful with Application.DoEvents(), it has caused spurious crashes in my years of programming experience.
			if (delay == 0L)
			{
				//0 tells this thread to relinquish the remainder of its time slice to any thread of equal priority that is ready to run.
				//If there are no other threads of equal priority that are ready to run, execution of the current thread is not suspended.
				System.Threading.Thread.Sleep(0);
			}
			else if (delay == -1L)
			{
				Application.DoEvents();
			}
			else if (delay == -2)//Sleep indefinitely until all InputHooks are finished.
			{
				while (!hasExited && Keysharp.Scripting.Script.input != null && Keysharp.Scripting.Script.input.InProgress())
				{
					Application.DoEvents();
					System.Threading.Thread.Sleep(10);
				}
			}
			else
			{
				var stop = DateTime.Now.AddMilliseconds(delay);//Using Application.DoEvents() is a pseudo-sleep that blocks until the timeout, but doesn't freeze the window.

				while (DateTime.Now < stop && !hasExited)
				{
					Application.DoEvents();
					System.Threading.Thread.Sleep(10);
				}
			}
		}

		/// <summary>
		/// Disables or enables all or selected hotkeys.
		/// </summary>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>On</term>: <description>suspends all hotkeys.</description></item>
		/// <item><term>Off</term>: <description>re-enables all hotkeys.</description></item>
		/// <item><term>Toggle</term> (default): <description>changes to the opposite of its previous state.</description></item>
		/// <item><term>Permit</term>: <description>marks the current subroutine as being exempt from suspension.</description></item>
		/// </list>
		/// </param>
		public static void Suspend(object obj)
		{
			var state = Options.ConvertOnOffToggle(obj.As());
			Suspended = state == Common.Keyboard.ToggleValueType.Toggle ? !Suspended : (state == Common.Keyboard.ToggleValueType.On);

			if (!(bool)Accessors.A_IconFrozen && !Parser.NoTrayIcon)
				Script.Tray.Icon = Suspended ? Keysharp.Core.Properties.Resources.Keysharp_s_ico : Keysharp.Core.Properties.Resources.Keysharp_ico;
		}

		/// <summary>
		/// This method is obsolete, use <see cref="Critical"/>.
		/// </summary>
		[Obsolete]
		public static void Thread()
		{
		}

		internal static bool ExitAppInternal(ExitReasons obj0, object obj1 = null)
		{
			if (hasExited)//This can be called multiple times, so ensure it only runs through once.
				return false;

			var exitReason = obj0.Ai();
			var exitCode = obj1.Ai();
			Accessors.A_ExitReason = exitReason;
			var result = Script.OnExitHandlers.InvokeEventHandlers(Accessors.A_ExitReason, exitCode);

			if (result.IsCallbackResultNonEmpty())//If any exit handlers returned a non empty value, abort the exit.
				return true;
			else
				Keysharp.Scripting.Script.OnExitHandlers.Clear();

			hasExited = true;//At this point, we are clear to exit, so do not allow any more calls to this function.
			Keysharp.Core.Common.Keyboard.HotkeyDefinition.AllDestruct();

			if (Script.HookThread is Common.Threading.HookThread ht)
				ht.Stop();

			StopMainTimer();

			foreach (var kv in timers)
				kv.Value.Stop();

			while (Keysharp.Scripting.Script.totalExistingThreads > 1)//Check against 1 instead of 0, because this may be launched in a thread as a result of a hotkey.
				Sleep(200);

			if (!Script.IsMainWindowClosing)
			{
				Script.mainWindow.CheckedInvoke(() =>
				{
					Script.mainWindow.Close();
					Script.mainWindow = null;
				}, false);
			}

			if (Script.Tray != null && Script.Tray.ContextMenuStrip != null)
			{
				Script.Tray.ContextMenuStrip.CheckedInvoke(() =>
				{
					Script.Tray.Visible = false;
					Script.Tray.Dispose();
					Script.Tray = null;
				}, true);
			}

			Environment.ExitCode = exitCode;
			//Environment.Exit(exitCode);//This seems too harsh, and also prevents compiled unit tests from properly being run.
			return false;
		}

		internal static void SetMainTimer()
		{
			if (mainTimer == null)
			{
				mainTimer = new Timer(10);
				mainTimer.Elapsed += (o, e) => { };
				mainTimer.Start();
			}
		}

		internal static void SleepWithoutInterruption(object obj = null)
		{
			AllowInterruption = false;
			Sleep(obj);
			AllowInterruption = true;
		}

		internal static void StopMainTimer()
		{
			if (mainTimer != null)
			{
				mainTimer.Stop();
				mainTimer = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsTrueAndRunning(object obj) => !hasExited&& Keysharp.Scripting.Script.ForceBool(obj);

		internal enum ExitReasons
		{
			Critical = -2, Destroy = -1, None = 0, Error, LogOff, Shutdown, Close, Menu, Exit, Reload, SingleInstance
		}
	}
}