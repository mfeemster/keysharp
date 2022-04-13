using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
		internal static ConcurrentDictionary<FuncObj, Timer> timers;
		private static EventHandler onExit;

		/// <summary>
		/// Is the Script currently suspended?
		/// </summary>
		public static bool Suspended { get; private set; }

		internal static bool AllowInterruption { get; set; }

		/// <summary>
		/// Prevents the current thread from being interrupted by other
		/// </summary>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>On</term>: <description>give the current thread the highest priority.</description></item>
		/// <item><term>Off</term>: <description>resets the current thread priority to normal.</description></item>
		/// </list>
		/// </param>
		public static void Critical(params object[] obj)
		{
			var mode = obj.L().S1();
			var on = Options.OnOff(mode) ?? true;
			System.Threading.Thread.CurrentThread.Priority = on ? ThreadPriority.Highest : ThreadPriority.Normal;
		}

		public static long EnabledTimerCount()
		{
			var ct = 0L;

			if (timers != null)
			{
				foreach (var kv in timers)
					if (kv.Value.Enabled)
						ct++;
			}

			return ct;
		}

		/// <summary>
		/// Exits the current thread or the entire program if non-persistent.
		/// </summary>
		/// <param name="exitCode">An integer that is returned to the caller.</param>
		public static void Exit(params object[] obj)
		{
			var exitCode = obj.L().I1();
			ApplicationExit?.Invoke(null, null);
			Environment.ExitCode = exitCode;
			Application.ExitThread();
		}

		/// <summary>
		/// Terminates the program unconditionally.
		/// </summary>
		/// <param name="exitCode">An integer that is returned to the caller.</param>
		public static void ExitApp(params object[] obj)
		{
			var exitCode = obj.L().I1();
			Script.mainWindow?.Close();
			ApplicationExit?.Invoke(null, null);
			//Environment.Exit(exitCode);//This seems too harsh, and also prevents compiled unit tests from properly being run.
		}

		/// <summary>
		/// Specifies a label to run automatically when the program exits.
		/// </summary>
		/// <param name="label">The name of a label. Leave blank to remove an existing label, if any.</param>
		public static void OnExit(string label)
		{
			Accessors.A_ErrorLevel = 0;

			if (onExit != null)
			{
				AppDomain.CurrentDomain.ProcessExit -= onExit;
				onExit = null;
			}

			if (string.IsNullOrEmpty(label))
				return;

			var method = Reflections.FindLocalRoutine(label);

			if (method == null)
			{
				Accessors.A_ErrorLevel = 1;
				return;
			}

			var handler = new EventHandler(delegate
			{
				try { _ = method.Invoke(null, Keysharp.Core.Core.EmptyVariadicArgs); }
				catch (Exception) { }
			});
			AppDomain.CurrentDomain.ProcessExit += handler;
			onExit = handler;
		}

		/// <summary>
		/// Specifies a function to call automatically when the program receives the specified message.
		/// </summary>
		/// <param name="number">The number of the message to monitor.</param>
		/// <param name="function">The name of a function to call whenever the specified message is received.</param>
		/// <param name="maxThreads">The maximum number of concurrent threads to launch per message number.</param>
		public static void OnMessage(string number, string function, string maxThreads)
		{
			// TODO: onmessage
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
		public static void Pause(params object[] obj)
		{
			var (mode, parentThread) = obj.L().Sb();
			var thread = System.Threading.Thread.CurrentThread;
			var state = Options.OnOff(mode);

			if (state == null && mode.Equals(Core.Keyword_Toggle, System.StringComparison.OrdinalIgnoreCase))
				state = !(thread.ThreadState == ThreadState.Suspended || thread.ThreadState == ThreadState.SuspendRequested);

			//Should figure out the right way to do this.//MATT
#pragma warning disable 612, 618

			if (state == true)
			{
				if (!(bool)Accessors.A_IconFrozen && !Parser.NoTrayIcon)
					Script.Tray.Icon = Keysharp.Core.Properties.Resources.Keysharp_p;

				thread.Suspend();
			}
			else if (state == false)
			{
				thread.Resume();

				if (!(bool)Accessors.A_IconFrozen && !Parser.NoTrayIcon)
					Script.Tray.Icon = Keysharp.Core.Properties.Resources.Keysharp_ico;
			}

#pragma warning restore 612, 618

			// UNDONE: correct handling of pause on underlying thread

			if (parentThread)
				thread.Join();
		}

		public static object Persistent(params object[] obj)
		{
			var b = obj.L().B1(true);
			var old = Parser.Persistent;
			Parser.Persistent = b;
			return old;
		}

		/// <summary>
		/// Replaces the currently running instance of the program with a new one.
		/// </summary>
		public static void Reload()
		{
			Application.Restart();//What about the cmd line args?//MATT
		}

		public static void SetTimer(params object[] obj)
		{
			var (function, period, priority) = obj.L().O1L2(null, long.MaxValue);
			FuncObj func = null;
			var once = period < 0;

			if (timers == null)
				timers = new ConcurrentDictionary<FuncObj, Timer>();

			if (once)
				period = -period;

			if (function is string s)//Make sure they don't keep adding the same function object via string.
			{
				if (cachedFuncObj.TryGetValue(s, out var tempfunc))
					func = tempfunc;
				else
					cachedFuncObj[s] = func = Keysharp.Scripting.Script.FuncObj(s);
			}

			if (func == null)
			{
				func = function is FuncObj fo ?
					   fo :
					   throw new TypeError($"Parameter {function} of type {function.GetType()} was not a string or a function object.");
			}

			//func can now only ever be accessed by the caller from within the timer event, as the first argument.
			if (timers.TryGetValue(func, out var timer))
			{
				if (period == 0)
				{
					_ = timers.TryRemove(func, out _);
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
						timer.Interval = period;

					return;
				}
			}
			else if (period != 0)
			{
				if (period == long.MaxValue)
					period = 250;

				_ = timers.TryAdd(func, timer = new Timer());
				timer.Interval = period;
			}
			else//They tried to stop a timer that didn't exist
				return;

			var level = ThreadPriority.Normal;

			if (priority > -1 && priority < 5)
				level = (ThreadPriority)priority;

			timer.Elapsed += (ss, ee) =>
			{
				timer.Enabled = false;

				var remove = false;

				System.Threading.Thread.CurrentThread.Priority = level;

				//If there are threads and NoTimers is set, then this shouldn't run. Revisit when threads are implemented.//TODO
				try
				{
					_ = func.Call(func, Conversions.ToYYYYMMDDHH24MISS(ee.SignalTime));

					if (timers.TryGetValue(func, out var existing))//They could have disabled it, in which case it wouldn't be in the dictionary.
						existing.Enabled = true;
				}
				catch (Exception)
				{
					remove = true;
				}

				if (once || remove)
				{
					_ = timers.TryRemove(func, out _);
					timer.Stop();
					timer.Dispose();
				}
			};
			timer.Start();
		}

		/// <summary>
		/// Waits the specified amount of time before continuing.
		/// </summary>
		/// <param name="Delay">The amount of time to pause in milliseconds.</param>
		public static void Sleep(params object[] obj)
		{
			var delay = obj.L().L1();
			//_ = System.Threading.Thread.CurrentThread.Join(delay);
			//var stop = Environment.TickCount + delay;
			//
			//while (Environment.TickCount < stop)
			//{
			//  Application.DoEvents();
			//  System.Threading.Thread.Sleep(10);
			//}
			System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(delay));
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
		public static void Suspend(params object[] obj)
		{
			var mode = obj.L().S1();
			var state = Options.OnOff(mode);
			Suspended = state == null && mode.Equals(Core.Keyword_Toggle, System.StringComparison.OrdinalIgnoreCase) ? !Suspended : (bool)state;

			if (!(bool)Accessors.A_IconFrozen && !Parser.NoTrayIcon)
				Script.Tray.Icon = Suspended ? Keysharp.Core.Properties.Resources.Keysharp_s : Keysharp.Core.Properties.Resources.Keysharp_ico;

			// UNDONE: permit mode for suspend
		}

		/// <summary>
		/// This method is obsolete, use <see cref="Critical"/>.
		/// </summary>
		[Obsolete]
		public static void Thread()
		{
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

		internal static void SleepWithoutInterruption(params object[] obj)
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
			Critical = -2, Destroy = -1, None = 0, Error, LogOff, Shutdown
			, Close, Menu, Exit, Reload, SingleInstance
		}

		public static event EventHandler ApplicationExit;
	}
}