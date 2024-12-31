using static Keysharp.Core.Errors;
using Timer = System.Timers.Timer;

namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for flow-related functions.
	/// </summary>
	public static class Flow
	{
		internal static ConcurrentDictionary<string, IFuncObj> cachedFuncObj = new ();
		internal static bool callingCritical;
		internal static volatile bool hasExited;
		internal static int IntervalUnspecified = int.MinValue + 303;// Use some negative value unlikely to ever be passed explicitly:
		internal static Timer mainTimer;
		internal static int NoSleep = -1;
		internal static bool persistentValueSetByUser;
		internal static ConcurrentDictionary<IFuncObj, System.Windows.Forms.Timer> timers = new ();

		/// <summary>
		/// Whether a thread can be interrupted/preempted by subsequent thread.
		/// </summary>
		internal static bool AllowInterruption { get; set; } = true;

		/// <summary>
		/// Internal property to track whether the script's hotkeys and hotstrings are suspended.
		/// </summary>
		internal static bool Suspended { get; set; }

		[PublicForTestOnly]
		public static void ResetState()
		{
			cachedFuncObj = new ();
			callingCritical = false;
			hasExited = false;
			IntervalUnspecified = int.MinValue + 303;// Use some negative value unlikely to ever be passed explicitly:
			mainTimer = null;
			NoSleep = -1;
			persistentValueSetByUser = false;
			timers = new ();
			AllowInterruption = true;
			Suspended = false;
		}

		/// <summary>
		/// Prevents the current thread from being interrupted by other threads, or enables it to be interrupted.
		/// </summary>
		/// <param name="onOffNumeric">
		/// If blank or omitted, it defaults to On. Otherwise, specify one of the following:<br/>
		///     On: The current thread is made critical, meaning that it cannot be interrupted by another thread.<br/>
		///     Off: The current thread immediately becomes interruptible, regardless of the settings of Thread Interrupt.<br/>
		///         See Critical Off for details.<br/>
		///     (Numeric): Specify a positive number to turn on Critical but also change the number of milliseconds between checks of the internal message queue.<br/>
		///             See Message Check Interval for details.<br/>
		///         Specifying 0 turns off Critical.<br/>
		///         Specifying -1 turns on Critical but disables message checks.<br/>
		/// </param>
		public static object Critical(object onOffNumeric = null)
		{
			callingCritical = true;
			var tv = Threads.GetThreadVariables();
			var on = onOffNumeric == null;
			long freq = !on ? (onOffNumeric.ParseLong(false) ?? 0L) : 0L;

			if (!on)
			{
				var b = Options.OnOff(onOffNumeric.As());

				if (b != null)
				{
					on = b.Value;

					if (on)
						freq = ThreadVariables.DefaultUninterruptiblePeekFrequency;
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
			tv.isCritical = onOffNumeric == null // i.e. omitted or blank is the same as "ON". See comments above.
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

		/// <summary>
		/// Iterates through all timers in existence and returns the number of them which are enabled.
		/// </summary>
		/// <returns>The number of currently enabled timers.</returns>
		public static long EnabledTimerCount()
		{
			var ct = 0L;

			foreach (var kv in timers)
				if (kv.Value.Enabled)//This won't work if we're enabling and disabling timers in the tick event.//TODO
					ct++;

			return ct;
		}

		/// <summary>
		/// Exits the current thread or the entire script if non-persistent.
		/// The exit is achieved by throwing an exception which will be caught in the catch
		/// clause that wraps all threads.
		/// </summary>
		/// <param name="exitCode">An integer that is returned to the caller.</param>
		public static object Exit(object exitCode = null)
		{
			Accessors.A_ExitReason = exitCode.Al();
			throw new Error("Exiting thread")
			{
				ExcType = Keyword_Return
			};
			//return null;
		}

		/// <summary>
		/// Terminates the script unconditionally.
		/// This is equivalent to choosing "Exit" from the script's tray menu or main menu.
		/// </summary>
		/// <param name="exitCode">If omitted, it defaults to 0 (zero is traditionally used to indicate success).<br/>
		/// Otherwise, specify an integer between -2147483648 and 2147483647 that is returned to its caller when the script exits.<br/>
		/// This code is accessible to any program that spawned the script, such as another script (via RunWait) or a batch (.bat) file.</param>
		public static object ExitApp(object exitCode = null)
		{
			if (!hasExited)//This can be called multiple times, so ensure it only runs through once.
			{
				Script.mainWindow.CheckedInvoke(() =>
				{
					_ = ExitAppInternal(ExitReasons.Exit, exitCode);
				}, true);
				var start = DateTime.Now;

				while (!hasExited && (DateTime.Now - start).TotalSeconds < 5)
					_ = Sleep(500);
			}

			return null;
		}

		/// <summary>
		/// Initializes the flow state of the script.
		/// This is used internally and is only public so tests can access it.
		/// </summary>
		[PublicForTestOnly]
		public static object Init()
		{
			hasExited = false;
			return null;
		}

		/// <summary>
		/// Returns whether the passed in value is true and the script is running.
		/// This is used in the generated code to evaluate loop variables.
		/// </summary>
		/// <param name="obj">The value to examine.</param>
		/// <returns>True if the value is true and the script is running, else false.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsTrueAndRunning(object obj) => !hasExited&& Script.ForceBool(obj);

		/// <summary>
		/// Registers a function to be called automatically whenever the script exits.
		/// </summary>
		/// <param name="callback">The function to call, which accepts two parameters<br/>
		/// 1: The exit reason (one of the words from the table below).<br/>
		/// 2: The exit code passed to <see cref="Exit"/> or <see cref="ExitApp"/>.<br/>
		/// </param>
		/// <param name="addRemove">If omitted, it defaults to 1. Otherwise, specify one of the following numbers:<br/>
		///     1: Call the callback after any previously registered callbacks.<br/>
		///    -1: Call the callback before any previously registered callbacks.<br/>
		///     0: Remove the callback if it was already contained in the list.
		/// </param>
		public static object OnExit(object callback, object addRemove = null)
		{
			Script.onExitHandlers.ModifyEventHandlers(Functions.GetFuncObj(callback, null, true), addRemove.Al(1L));
			return null;
		}

		/// <summary>
		/// Registers a function to be called automatically whenever the script receives the specified message.
		/// </summary>
		/// <param name="msgNumber">The number of the message to monitor or query, which should be between 0 and 4294967295 (0xFFFFFFFF).</param>
		/// <param name="callback">The name of a function to call whenever the specified message is received.<br/>
		/// The callback accepts four parameters as follows:<br/>
		///     The message's WPARAM value.<br/>
		///     The message's LPARAM value.<br/>
		///     The message number, which is useful in cases where a callback monitors more than one message.<br/>
		///     The HWND(unique ID) of the window or control to which the message was sent.The HWND can be used directly in a WinTitle parameter.
		/// </param>
		/// <param name="maxThreads">If omitted, it defaults to 1, meaning the callback is limited to one thread at a time.<br/>
		/// This is usually best because otherwise, the script would process messages out of chronological order whenever the callback interrupts itself.
		/// </param>
		public static object OnMessage(object msgNumber, object callback, object maxThreads = null)
		{
			var msg = msgNumber.Al();
			var mt = maxThreads.Al(1);
			var monitor = GuiHelper.onMessageHandlers.GetOrAdd(msg);

			if (mt > 0)
				monitor.maxInstances = Math.Clamp((int)mt, 1, MsgMonitor.MAX_INSTANCES);
			else if (mt < 0)
				monitor.maxInstances = (int)(-mt);

			monitor.funcs.ModifyEventHandlers(Functions.GetFuncObj(callback, null, true), mt);

			if (mt == 0 && monitor.funcs.Count == 0)
				_ = GuiHelper.onMessageHandlers.TryRemove(msg, out var _);

			return null;
		}

		/// <summary>
		/// Prevents the script from exiting automatically when its last thread completes, allowing it to stay running in an idle state.
		/// </summary>
		/// <param name="persist">If omitted, it defaults to true.<br/>
		/// If true, the script will be kept running after all threads have exited,<br/>
		/// even if none of the other conditions for keeping the script running are met.<br/>
		/// If false, the default behaviour is restored.
		/// </param>
		/// <returns>The previous persistence boolean value.</returns>
		public static object Persistent(object persist = null)
		{
			var b = persist.Ab(true);
			var old = Script.persistent;
			Script.persistent = persistentValueSetByUser = b;
			return old;
		}

		/// <summary>
		/// Replaces the currently running instance of the program with a new one.
		/// </summary>
		public static object Reload()
		{
			//Just calling Application.Restart will trigger ExitAppInternal().
			//So it doesn't need to be called directly. Further, it will cause problems if called
			//so just let the natural chain of closing events handle it.
			Script.mainWindow.CheckedBeginInvoke(() =>
			{
				Accessors.A_ExitReason = ExitReasons.Reload;
				Application.Restart();//This will pass the same command line args to the new instance that were passed to this instance.
			}, true, true);
			var start = DateTime.Now;

			while (!hasExited && (DateTime.Now - start).TotalSeconds < 5)
				_ = Sleep(500);

			return null;
		}


		/// <summary>
		/// Causes a function to be called automatically and repeatedly at a specified time interval.
		/// </summary>
		/// <param name="function">The function object to call.<br/>
		/// The callback accepts two parameters as follows:<br/>
		///     The function object itself.<br/>
		///     The date/time the timer was triggered as a YYYYMMDDHH24MISS string.<br/>
		/// </param>
		/// <param name="period">If omitted and the timer does not exist, it will be created with a period of 250.<br/>
		/// If omitted and the timer already exists, it will be reset at its former period unless Priority is specified.<br/>
		/// Otherwise, the absolute value of this parameter is used as the approximate number of milliseconds that must pass before<br/>
		/// the timer is executed. The timer will be automatically reset. It can be set to repeat automatically or run only once:<br/>
		///     If Period is greater than 0, the timer will automatically repeat until it is explicitly disabled by the script.<br/>
		///     If Period is less than 0, the timer will run only once.For example, specifying -100 would call Function 100 ms from now then delete the timer as though SetTimer Function, 0 had been used.<br/>
		///     If Period is 0, the timer is marked for deletion. If a thread started by this timer is still running, the timer is deleted after the thread finishes (unless it has been reenabled);<br/>
		///         otherwise, it is deleted immediately. In any case, the timer's previous Period and Priority are not retained.
		/// </param>
		/// <param name="priority">If omitted, it defaults to 0. Otherwise, specify an integer between 0 and 4<br/>
		/// to indicate this timer's thread priority. See <see cref="Threads"/> for details.<br/>
		/// To change the priority of an existing timer without affecting it in any other way, omit Period.
		/// </param>
		/// <exception cref="TypeError"></exception>
		public static object SetTimer(object function = null, object period = null, object priority = null)
		{
			var f = function;
			var p = period.Al(long.MaxValue);
			var pri = priority.Al();
			var once = p < 0;
			IFuncObj func = null;
			System.Windows.Forms.Timer timer = null;

			if (once)
				p = -p;

			if (f is string s)//Make sure they don't keep adding the same function object via string.
			{
				if (cachedFuncObj.TryGetValue(s, out var tempfunc))
					func = tempfunc;
				else
					cachedFuncObj[s] = func = Functions.FuncObj(s);
			}

			if (f != null && func == null)
			{
				func = f is FuncObj fo ?
					   fo : throw new TypeError($"Parameter {f} of type {f.GetType()} was not a string or a function object.");
			}

			if (func != null && timers.TryGetValue(func, out timer))
			{
			}
			else if (f == null)
				timer = Threads.GetThreadVariables().currentTimer;//This means: use the timer which has already been created for this thread/timer event which we are currently inside of.

			if (timer != null)
			{
				if (p == 0)
				{
					if (func == null)
					{
						var temptimer = timers.Where((kv) => kv.Value == timer).FirstOrDefault();

						if (temptimer.Key != null)
							_ = timers.TryRemove(temptimer.Key, out _);
					}
					else
						_ = timers.TryRemove(func, out _);

					timer.Stop();
					timer.Dispose();
					Script.ExitIfNotPersistent();
					return null;
				}
				else
				{
					if (p == long.MaxValue)
					{
						if (pri != timer.Tag.Al())//Period omitted and timer existed, but priority was specified, so just update the priority.
						{
							timer.Tag = pri;
						}
						else//Period omitted, timer did exist, and priority was omitted so reset it at its current interval.
						{
							timer.Stop();
							timer.Start();
						}
					}
					else
						timer.Interval = (int)p;

					return null;
				}
			}
			else if (p != 0)
			{
				if (p == long.MaxValue)//Period omitted and timer didn't exist, so create one with a 250ms interval.
					p = 250;

				_ = timers.TryAdd(func, timer = new System.Windows.Forms.Timer());
				timer.Tag = (int)pri;
				timer.Interval = (int)p;
			}
			else//They tried to stop a timer that didn't exist
				return null;

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

						var remove = !TryCatch(() =>
						{
							_ = Interlocked.Increment(ref Script.totalExistingThreads);
							(bool, ThreadVariables) btv = Threads.PushThreadVariables(pri, true, false);
							tv.currentTimer = timer;
							var ret = func.Call(func, Conversions.ToYYYYMMDDHH24MISS(DateTime.Now));
							_ = Threads.EndThread(btv.Item1);
						}, true);//Pop on exception because EndThread() above won't be called.

						if (once || remove)
						{
							_ = timers.TryRemove(func, out _);
							t.Stop();
							t.Dispose();
							Script.ExitIfNotPersistent();
						}
						else if (timers.TryGetValue(func, out var existing))//They could have disabled it, in which case it wouldn't be in the dictionary.
							existing.Enabled = true;
					}
				}
			};
			Script.mainWindow.CheckedInvoke(timer.Start, true);
			return null;
		}

		/// <summary>
		/// Waits the specified amount of time before continuing.
		/// </summary>
		/// <param name="delay">The amount of time to pause in milliseconds.</param>
		public static object Sleep(object delay = null)
		{
			var d = delay.Al(-1L);

			//Be careful with Application.DoEvents(), it has caused spurious crashes in my years of programming experience.
			if (d == 0L)
			{
				//0 tells this thread to relinquish the remainder of its time slice to any thread of equal priority that is ready to run.
				//If there are no other threads of equal priority that are ready to run, execution of the current thread is not suspended.
				System.Threading.Thread.Sleep(0);
			}
			else if (d == -1L)
			{
				try
				{
					Application.DoEvents();//Can sometimes throw on linux.
				}
				catch
				{
				}
			}
			else if (d == -2)//Sleep indefinitely until all InputHooks are finished.
			{
				while (!hasExited && Script.input != null && Script.input.InProgress())
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
				var stop = DateTime.Now.AddMilliseconds(d);//Using Application.DoEvents() is a pseudo-sleep that blocks until the timeout, but doesn't freeze the window.

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

			return null;
		}

		/// <summary>
		/// Disables or enables all or selected hotkeys.
		/// </summary>
		/// <param name="newState">
		/// If omitted, it defaults to -1. Otherwise, specify one of the following values:<br/>
		///     1: Suspends all hotkeys and hotstrings except those explained the Remarks section.<br/>
		///     0: Re-enables the hotkeys and hotstrings that were disable above.<br/>
		///    -1: Changes to the opposite of its previous state (On or Off).
		/// </param>
		public static object Suspend(object newState)
		{
			var state = Conversions.ConvertOnOffToggle(newState.As());
			Suspended = state == ToggleValueType.Toggle ? !Suspended : (state == ToggleValueType.On);

			if (!(bool)Accessors.A_IconFrozen && !Script.NoTrayIcon)
				Script.Tray.Icon = Suspended ? Properties.Resources.Keysharp_s_ico : Properties.Resources.Keysharp_ico;

			return null;
		}

		/// <summary>
		/// Sets the priority or interruptibility of threads. It can also temporarily disable all timers.<br/>
		/// The subFunction, Value1, and Value2 parameters are dependent upon each other and their usage is described below.
		/// </summary>
		/// <param name="subFunction">Specify one of the following:<br/>
		///     NoTimers: Prevents interruptions from any timers until the current thread either ends, executes Thread "NoTimers", false, or is interrupted by another thread that allows timers (in which case timers can interrupt the interrupting thread until it finishes).<br/>
		///     Priority: Changes the priority level of the current thread.<br/>
		///     Interrupt: Changes the duration of interruptibility for newly launched threads.
		/// </param>
		/// <param name="val1">Has a different meaning depending on subFunction:
		///     NoTimers: True to disallow timers, else false to allow timers.  Default: true.<br/>
		///     Priority: The thread priority as an integer in the range -2147483648 and 2147483647.<br/>
		///     Interrupt: The time in milliseconds that each newly launched thread is uninterruptible. Default: 17.
		/// </param>
		public static object Thread(object subFunction, object value1 = null)
		{
			var sf = subFunction.As();

			if (string.Compare(sf, "notimers", true) == 0)
				Accessors.A_AllowTimers = !(Options.OnOff(value1.As()) ?? false);
			else if (string.Compare(sf, "priority", true) == 0)
				Threads.GetThreadVariables().priority = value1.Al();
			else if (string.Compare(sf, "interrupt", true) == 0)
				Script.uninterruptibleTime = value1.Ai(Script.uninterruptibleTime);

			return null;
		}

		/// <summary>
		/// Internal helper to handle exiting the script.
		/// </summary>
		/// <param name="exitReason">The <see cref="ExitReason"/> for exiting the script.</param>
		/// <param name="exitCode">The exit code to return from the script when it exits.</param>
		/// <returns>True if exiting was interrupted by a non empty callback return value, else false.</returns>
		internal static bool ExitAppInternal(ExitReasons exitReason, object exitCode = null)
		{
			if (hasExited)//This can be called multiple times, so ensure it only runs through once.
				return false;

			Dialogs.CloseMessageBoxes();
			var ec = exitCode.Ai();
			Accessors.A_ExitReason = exitReason.ToString();
			var allowInterruption_prev = AllowInterruption;//Save current setting.
			AllowInterruption = false;
			var result = Script.onExitHandlers.InvokeEventHandlers(Accessors.A_ExitReason, exitCode);

			//If it wasn't a critical shutdown and any exit handlers returned a non empty value, abort the exit.
			if (exitReason >= ExitReasons.None && result.IsCallbackResultNonEmpty())
			{
				Accessors.A_ExitReason = "";
				AllowInterruption = allowInterruption_prev;
				return true;
			}
			else
				Script.onExitHandlers.Clear();

			hasExited = true;//At this point, we are clear to exit, so do not allow any more calls to this function.
			AllowInterruption = allowInterruption_prev;
			HotkeyDefinition.AllDestruct();

			if (Script.HookThread is HookThread ht)
				ht.Stop();

			StopMainTimer();

			foreach (var kv in timers)
				kv.Value.Stop();

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

			Environment.ExitCode = ec;
			//Environment.Exit(exitCode);//This seems too harsh, and also prevents compiled unit tests from properly being run.
			return false;
		}

		/// <summary>
		/// Internal helper to create and start the main timer.
		/// This is used whenever a joystick hook is present.
		/// </summary>
		internal static void SetMainTimer()
		{
			if (mainTimer == null)
			{
				mainTimer = new Timer(10);
				mainTimer.Elapsed += (o, e) => { };
				mainTimer.Start();
			}
		}

		/// <summary>
		/// Internal helper to call <see cref="Sleep"/> in between the toggling of <see cref="AllowInterruption"/> off then back oin.
		/// </summary>
		/// <param name="duration">The time in milliseconds to sleep for.</param>
		internal static void SleepWithoutInterruption(object duration = null)
		{
			AllowInterruption = false;
			_ = Sleep(duration);
			AllowInterruption = true;
		}

		/// <summary>
		/// Internal helper to stop the main timer and set it to null.
		/// This is used when exiting the script.
		/// </summary>
		internal static void StopMainTimer()
		{
			if (mainTimer != null)
			{
				mainTimer.Stop();
				mainTimer = null;
			}
		}

		/// <summary>
		/// Internal helper to wrap an <see cref="Action"/> within a try/catch block.<br/>
		/// This is done because the try/catch blocks have a lot of scaffolding code that is needed for handling<br/>
		/// threads and error conditions and it would be very verbose and unmaintainable to repeat it everywhere.
		/// </summary>
		/// <param name="action">The action to perform in the try block.</param>
		/// <param name="pop">True to pop the current thread by calling <see cref="Threads.EndThread(bool, bool)"/> when an exception is caught.<br/>
		/// Pass true for this when the action internally calls <see cref="Threads.BeginThread(bool)"/> before executing code that<br/>
		/// could potentially thrown an exception.
		/// </param>
		/// <returns>True if no errors occurred, else false if any catch blocks were reached.</returns>
		internal static bool TryCatch(Action action, bool pop)
		{
			try
			{
				action();
				return true;
			}
			catch (Error kserr)
			{
				if (pop)
					_ = Threads.EndThread(true);

				if (ErrorOccurred(kserr))
				{
					var (__pushed, __btv) = Threads.BeginThread();
					_ = Dialogs.MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
					_ = Threads.EndThread(__pushed);
				}

				return false;
			}
			catch (Exception mainex)
			{
				if (pop)
					_ = Threads.EndThread(true);

				var ex = mainex.InnerException ?? mainex;

				if (ex is Error kserr)
				{
					if (ErrorOccurred(kserr))
					{
						var (__pushed, __btv) = Threads.BeginThread();
						_ = Dialogs.MsgBox("Uncaught Keysharp exception:\r\n" + kserr, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
						_ = Threads.EndThread(__pushed);
					}
				}
				else
				{
					var (__pushed, __btv) = Threads.BeginThread();
					_ = Dialogs.MsgBox("Uncaught exception:\r\n" + "Message: " + ex.Message + "\r\nStack: " + ex.StackTrace, $"{Accessors.A_ScriptName}: Unhandled exception", "iconx");
					_ = Threads.EndThread(__pushed);
				}

				return false;
			}
		}

		/// <summary>
		/// The various reasons for exiting the script.
		/// </summary>
		internal enum ExitReasons
		{
			Critical = -2, Destroy = -1, None = 0, Error, LogOff, Shutdown, Close, Menu, Exit, Reload, Single
		}
	}
}