using Timer = System.Timers.Timer;

namespace Keysharp.Core
{
	public static class Flow
	{
		internal static ConcurrentDictionary<string, IFuncObj> cachedFuncObj = new ConcurrentDictionary<string, IFuncObj>();

		internal static bool callingCritical;

		internal static volatile bool hasExited;

		// Use some negative value unlikely to ever be passed explicitly:
		internal static int IntervalUnspecified = int.MinValue + 303;

		internal static Timer mainTimer;
		internal static int NoSleep = -1;
		internal static bool persistentValueSetByUser;
		internal static ConcurrentDictionary<IFuncObj, System.Windows.Forms.Timer> timers = new ConcurrentDictionary<IFuncObj, System.Windows.Forms.Timer>();
		internal static bool AllowInterruption { get; set; } = true;

		/// <summary>
		/// Is the Script currently suspended?
		/// </summary>
		internal static bool Suspended { get; set; }

		/// <summary>
		/// Prevents the current thread from being interrupted by other
		/// </summary>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>On</term>: <description>give the current thread the highest priority.</description></item>
		/// <item><term>Off</term>: <description>resets the current thread priority to normal.</description></item>
		/// </list>
		/// </param>
		public static object Critical(object obj = null)
		{
			callingCritical = true;
			var tv = Threads.GetThreadVariables();
			var on = obj == null;
			long freq = !on ? (obj.ParseLong(false) ?? 0L) : 0L;

			if (!on)
			{
				var b = Options.OnOff(obj.As());

				if (b != null)
				{
					on = b.Value;

					if (on)
						freq = 16L;
				}
			}

			var ret = tv.isCritical ? tv.peekFrequency : 0L;
			// v1.0.46: When the current thread is critical, have the script check messages less often to
			// reduce situations where an OnMessage or GUI message must be discarded due to "thread already
			// running".  Using 16 rather than the default of 5 solves reliability problems in a custom-menu-draw
			// script and probably many similar scripts -- even when the system is under load (though 16 might not
			// be enough during an extreme load depending on the exact preemption/timeslice dynamics involved).
			// DON'T GO TOO HIGH because this setting reduces response time for ALL messages, even those that
			// don't launch script threads (especially painting/drawing and other screen-update events).
			// Some hardware has a tickcount granularity of 15 instead of 10, so this covers more variations.
			// v1.0.48: Below supports "Critical 0" as meaning "Off" to improve compatibility with A_IsCritical.
			// In fact, for performance, only the following are no recognized as turning on Critical:
			//     - "On"
			//     - ""
			//     - Integer other than 0.
			// Everything else is considered to be "Off", including "Off", any non-blank string that
			// doesn't start with a non-zero number, and zero itself.
			tv.isCritical = obj == null // i.e. omitted or blank is the same as "ON". See comments above.
							|| on
							|| freq > 0L; // Non-zero integer also turns it on. Relies on short-circuit boolean order.

			if (tv.isCritical) // Critical has been turned on. (For simplicity even if it was already on, the following is done.)
			{
				tv.peekFrequency = freq;
				tv.allowThreadToBeInterrupted = false;
				// Ensure uninterruptibility never times out.  IsInterruptible() relies on this to avoid the
				// need to check g->ThreadIsCritical, which in turn allows global_maximize_interruptibility()
				// and DialogPrep() to avoid resetting g->ThreadIsCritical, which allows it to reliably be
				// used as the default setting for new threads, even when the auto-execute thread itself
				// (or the idle thread) needs to be interruptible, such as while displaying a dialog->
				// In other words, g->ThreadIsCritical only represents the desired setting as set by the
				// script, and isn't the actual mechanism used to make the thread uninterruptible.
				tv.uninterruptibleDuration = -1;
			}
			else // Critical has been turned off.
			{
				// Since Critical is being turned off, allow thread to be immediately interrupted regardless of
				// any "Thread Interrupt" settings.
				tv.peekFrequency = 5;
				tv.allowThreadToBeInterrupted = true;
			}

			// The thread's interruptibility has been explicitly set; so the script is now in charge of
			// managing this thread's interruptibility.
			callingCritical = false;
			return ret;
		}

		public static long EnabledTimerCount()
		{
			var ct = 0L;

			foreach (var kv in timers)
				if (kv.Value.Enabled)//This won't work if we're enabling and disabling timers in the tick event.//TODO
					ct++;

			return ct;
		}

		/// <summary>
		/// Exits the current thread or the entire program if non-persistent.
		/// </summary>
		/// <param name="exitCode">An integer that is returned to the caller.</param>
		public static void Exit(object obj = null)
		{
			var exitCode = obj.Al();
			Accessors.A_ExitReason = exitCode;
			throw new Error("Exiting thread")
			{
				ExcType = Keywords.Keyword_Return
			};
		}

		/// <summary>
		/// Terminates the program unconditionally.
		/// </summary>
		/// <param name="exitCode">An integer exit code to be passed to the caller of this program when it exits.</param>
		public static void ExitApp(object obj = null)
		{
			Script.mainWindow.CheckedInvoke(() =>
			{
				_ = ExitAppInternal(ExitReasons.Exit, obj);
			}, true);

			while (!Flow.hasExited)
				Sleep(500);
		}

		public static void Init()
		{
			hasExited = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsTrueAndRunning(object obj) => !hasExited&& Keysharp.Scripting.Script.ForceBool(obj);

		/// <summary>
		/// Specifies a label to run automatically when the program exits.
		/// </summary>
		/// <param name="label">The name of a label. Leave blank to remove an existing label, if any.</param>
		public static void OnExit(object obj0, object obj1 = null) => Script.onExitHandlers.ModifyEventHandlers(Function.GetFuncObj(obj0, null, true), obj1.Al(1L));

		/// <summary>
		/// Specifies a function to call automatically when the program receives the specified message.
		/// </summary>
		/// <param name="number">The number of the message to monitor.</param>
		/// <param name="function">The name of a function to call whenever the specified message is received.</param>
		/// <param name="maxThreads">The maximum number of concurrent threads to launch per message number.</param>
		public static void OnMessage(object obj0, object obj1, object obj2 = null)
		{
			var msg = obj0.Al();
			var maxInstances = obj2.Al(1);
			var monitor = GuiHelper.onMessageHandlers.GetOrAdd(msg);

			if (maxInstances > 0)
				monitor.maxInstances = Math.Clamp((int)maxInstances, 1, MsgMonitor.MAX_INSTANCES);
			else if (maxInstances < 0)
				monitor.maxInstances = (int)(-maxInstances);

			monitor.funcs.ModifyEventHandlers(Function.GetFuncObj(obj1, null, true), maxInstances);

			if (maxInstances == 0 && monitor.funcs.Count == 0)
				_ = GuiHelper.onMessageHandlers.TryRemove(msg, out var _);
		}

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
		    //#pragma warning disable 612, 618

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

		    //#pragma warning restore 612, 618
		    }
		*/

		public static object Persistent(object obj = null)
		{
			var b = obj.Ab(true);
			var old = Script.persistent;
			Script.persistent = persistentValueSetByUser = b;
			return old;
		}

		/// <summary>
		/// Replaces the currently running instance of the program with a new one.
		/// </summary>
		public static void Reload()
		{
			//Just calling Application.Restart will trigger ExitAppInternal().
			//So it doesn't need to be called directly. Further, it will cause problems if called
			//so just let the natural chain of closing events handle it.
			Script.mainWindow.CheckedBeginInvoke(() =>
			{
				Accessors.A_ExitReason = ExitReasons.Reload;
				Application.Restart();//This will pass the same command line args to the new instance that were passed to this instance.
			}, true, true);

			while (!hasExited)
				Sleep(500);
		}

		public static void SetTimer(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var function = obj0;
			var period = obj1.Al(long.MaxValue);
			var priority = obj2.Al();
			var once = period < 0;
			IFuncObj func = null;
			System.Windows.Forms.Timer timer = null;

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

			if (func != null && timers.TryGetValue(func, out timer))
			{
			}
			else if (function == null)
				timer = Threads.GetThreadVariables().currentTimer;//This means: use the timer which has already been created for this thread/timer event which we are currently inside of.

			if (timer != null)
			{
				if (period == 0)
				{
					if (func == null)
					{
						var temptimer = timers.Where((kv) => kv.Value == timer).FirstOrDefault();

						if (temptimer.Key != null)
							_ = timers.TryRemove(temptimer.Key, out var _);
					}
					else
						_ = timers.TryRemove(func, out _);

					timer.Stop();
					timer.Dispose();
					return;
				}
				else
				{
					if (period == long.MaxValue)
					{
						if (priority != timer.Tag.Al())//Period omitted and timer existed, but priority was specified, so just update the priority.
						{
							timer.Tag = priority;
						}
						else//Period omitted, timer did exist, and priority was omitted so reset it at its current interval.
						{
							timer.Stop();
							timer.Start();
						}
					}
					else
						timer.Interval = (int)period;

					return;
				}
			}
			else if (period != 0)
			{
				if (period == long.MaxValue)//Period omitted and timer didn't exist, so create one with a 250ms interval.
					period = 250;

				_ = timers.TryAdd(func, timer = new System.Windows.Forms.Timer());
				timer.Tag = (int)priority;
				timer.Interval = (int)period;
			}
			else//They tried to stop a timer that didn't exist
				return;

			timer.Tick += (ss, ee) =>
			{
				if ((!Accessors.A_AllowTimers.Ab() && Script.totalExistingThreads > 0)
						|| !Threads.AnyThreadsAvailable() || !Threads.IsInterruptible())
					return;

				if (ss is System.Windows.Forms.Timer t)
				{
					if (!t.Enabled)//A way of checking to make sure the timer is not already executing.
						return;

					var pri = t.Tag.Ai();
					var tv = Threads.GetThreadVariables();

					if (pri >= tv.priority)
					{
						t.Enabled = false;

						var remove = !Misc.TryCatch(() =>
						{
							_ = Interlocked.Increment(ref Script.totalExistingThreads);
							(bool, ThreadVariables) btv = Threads.PushThreadVariables(pri, true, false);
							tv.currentTimer = timer;
							var ret = func.Call(func, Conversions.ToYYYYMMDDHH24MISS(DateTime.Now));
							Threads.EndThread(btv.Item1);
						}, true);

						if (once || remove)
						{
							_ = timers.TryRemove(func, out _);
							t.Stop();
							t.Dispose();
						}
						else if (timers.TryGetValue(func, out var existing))//They could have disabled it, in which case it wouldn't be in the dictionary.
							existing.Enabled = true;
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
				try
				{
					Application.DoEvents();//Can sometimes throw on linux.
				}
				catch
				{
				}
			}
			else if (delay == -2)//Sleep indefinitely until all InputHooks are finished.
			{
				while (!hasExited && Keysharp.Scripting.Script.input != null && Keysharp.Scripting.Script.input.InProgress())
				{
					try
					{
						Application.DoEvents();//Can sometimes throw on linux.
					}
					catch
					{
					}

					System.Threading.Thread.Sleep(10);
				}
			}
			else
			{
				var stop = DateTime.Now.AddMilliseconds(delay);//Using Application.DoEvents() is a pseudo-sleep that blocks until the timeout, but doesn't freeze the window.

				while (DateTime.Now < stop && !hasExited)
				{
					try
					{
						//if (System.Threading.Thread.CurrentThread.ManagedThreadId == Processes.ManagedMainThreadID)
						Application.DoEvents();//Can sometimes throw on linux.
					}
					catch
					{
					}

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
			var state = Conversions.ConvertOnOffToggle(obj.As());
			Suspended = state == Common.Keyboard.ToggleValueType.Toggle ? !Suspended : (state == Common.Keyboard.ToggleValueType.On);

			if (!(bool)Accessors.A_IconFrozen && !Script.NoTrayIcon)
				Script.Tray.Icon = Suspended ? Keysharp.Core.Properties.Resources.Keysharp_s_ico : Keysharp.Core.Properties.Resources.Keysharp_ico;
		}

		public static void Thread(object obj0, object obj1 = null, object obj2 = null)
		{
			var subFunc = obj0.As();

			if (string.Compare(subFunc, "notimers", true) == 0)
			{
				Accessors.A_AllowTimers = !(Options.OnOff(obj1.As()) ?? false);
			}
			else if (string.Compare(subFunc, "priority", true) == 0)
			{
				Threads.GetThreadVariables().priority = obj1.Al();
			}
			else if (string.Compare(subFunc, "interrupt", true) == 0)
			{
				Keysharp.Scripting.Script.uninterruptibleTime = obj1.Ai(Keysharp.Scripting.Script.uninterruptibleTime);
			}
		}

		internal static bool ExitAppInternal(ExitReasons obj0, object obj1 = null)
		{
			if (hasExited)//This can be called multiple times, so ensure it only runs through once.
				return false;

			var exitCode = obj1.Al();
			Accessors.A_ExitReason = obj0.ToString();
			var allowInterruption_prev = AllowInterruption;//Save current setting.
			AllowInterruption = false;
			var result = Script.onExitHandlers.InvokeEventHandlers(Accessors.A_ExitReason, exitCode);

			//If it wasn't a critical shutdown and any exit handlers returned a non empty value, abort the exit.
			if (obj0 >= ExitReasons.None && result.IsCallbackResultNonEmpty())
			{
				Accessors.A_ExitReason = "";
				AllowInterruption = allowInterruption_prev;
				return true;
			}
			else
				Keysharp.Scripting.Script.onExitHandlers.Clear();

			hasExited = true;//At this point, we are clear to exit, so do not allow any more calls to this function.
			AllowInterruption = allowInterruption_prev;
			Keysharp.Core.Common.Keyboard.HotkeyDefinition.AllDestruct();

			if (Script.HookThread is Common.Threading.HookThread ht)
				ht.Stop();

			StopMainTimer();

			foreach (var kv in timers)
				kv.Value.Stop();

			//Check against 1 instead of 0, because this may be launched in a thread as a result of a hotkey.
			//If this gets stuck in a loop it means we have a thread imbalance/mismatch somewhere.
			//We added them, but never removed. While seemingly dangerous to have, it's a handy
			//way to know we've found a bug.
			while (Keysharp.Scripting.Script.totalExistingThreads > 1)
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

			Environment.ExitCode = (int)exitCode;
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

		internal enum ExitReasons
		{
			Critical = -2, Destroy = -1, None = 0, Error, LogOff, Shutdown, Close, Menu, Exit, Reload, Single
		}
	}
}