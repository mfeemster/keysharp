using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Scripting;
using static Keysharp.Core.Misc;

namespace Keysharp.Core.Common.Threading
{
	public static class Threads
	{
		private static ThreadVariableManager tvm = new ThreadVariableManager();

		public static void BeginThread(bool onlyIfEmpty = false)
		{
			var skip = Flow.AllowInterruption == false;//This will be false when exiting the program.
			_ = Interlocked.Increment(ref Script.totalExistingThreads);
			_ = CreateIfNeeded(0, skip, false);
			_ = PushThreadVariables(0, skip, false, onlyIfEmpty);
		}

		public static void EndThread()
		{
			PopThreadVariables();
			_ = Interlocked.Decrement(ref Script.totalExistingThreads);
		}

		[PublicForTestOnly]
		public static ThreadVariables PushThreadVariables(int priority, bool skipUninterruptible,
				bool isCritical = false, bool onlyIfEmpty = false) => tvm.PushThreadVariables(priority, skipUninterruptible, isCritical, onlyIfEmpty);

		internal static bool AnyThreadsAvailable() => Script.totalExistingThreads < Script.MaxThreadsTotal;

		internal static ThreadVariables CreateIfNeeded(int priority = 0
				, bool skipUninterruptible = false
				, bool isCritical = false) => tvm.CreateIfNeeded(priority, skipUninterruptible, isCritical);

		internal static ThreadVariables GetThreadVariables() => tvm.GetThreadVariables();

		internal static bool IsInterruptible()
		{
			if (!Flow.AllowInterruption)
				return false;

			if (Script.totalExistingThreads == 0)
				return true;

			var tv = GetThreadVariables();

			if (!tv.allowThreadToBeInterrupted // Those who check whether g->AllowThreadToBeInterrupted==false should then check whether it should be made true.
					&& tv.uninterruptibleDuration > -1 // Must take precedence over the below.  g_script.mUninterruptibleTime is not checked because it's supposed to go into effect during thread creation, not after the thread is running and has possibly changed the timeout via 'Thread "Interrupt"'.
					&& (DateTime.Now - tv.threadStartTime).TotalMilliseconds >= tv.uninterruptibleDuration // See big comment section above.
					&& !Flow.callingCritical // In case of "Critical" on the first line.  See v2.0 comment above.
			   )
				// Once the thread becomes interruptible by any means, g->ThreadStartTime/UninterruptibleDuration
				// can never matter anymore because only Critical (never "Thread Interrupt") can turn off the
				// interruptibility again, and it resets g->UninterruptibleDuration.
				tv.allowThreadToBeInterrupted = true; // Avoids issues with 49.7 day limit of 32-bit TickCount, and also helps performance future callers of this function (they can skip most of the checking above).

			return tv.allowThreadToBeInterrupted;
		}

		internal static async Task<object> LaunchInThread(int priority, bool skipUninterruptible,
				bool isCritical, object func, object[] o)//Determine later the optimal threading model.//TODO
		{
			Task<object> tsk = null;
			SlimStack<ThreadVariables> threadStackVars = null;
			_ = Interlocked.Increment(ref Script.totalExistingThreads);

			try
			{
				tsk = Task.Factory.StartNew<object>(() =>
													//tsk = Task.Run<object>(() =>
				{
					//throw new System.Exception("ASDf");
					//throw new Error("ASDf");
					object ret;
					_ = tvm.CreateIfNeeded(priority, skipUninterruptible, isCritical);
					_ = PushThreadVariables(priority, skipUninterruptible, isCritical); //Always start each thread with one entry.
					threadStackVars = ThreadVariableManager.threadVars;//Get a reference to the object for this thread.

					if (func is VariadicFunction vf)
						ret = vf(o);
					else if (func is IFuncObj ifo)
						ret = ifo.Call(o);
					else
						ret = "";

					tvm.PopThreadVariables();
					return ret;
				},
				//This is needed to make a variety of functionality work inside of hotkey/string handlers.
				//Such as clipboard and window functions.
				CancellationToken.None, TaskCreationOptions.None,
				SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current);
				_ = await tsk;
			}
			//catch (AggregateException aex)
			catch (Exception ex)
			{
				tvm.PopThreadVariablesFromOtherThread(threadStackVars);

				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;//Do not pass ex because it will reset the stack information.
			}
			finally
			{
				_ = Interlocked.Decrement(ref Script.totalExistingThreads);
			}

			return tsk;
		}

		internal static void PopThreadVariables() => tvm.PopThreadVariables();
	}

	public class ThreadVariables : IClearable
	{
		internal bool allowThreadToBeInterrupted = true;
		internal long controlDelay = 20L;
		internal CoordModes coords;
		internal System.Windows.Forms.Timer currentTimer;
		internal string defaultGui;
		internal long defaultMouseSpeed = 2L;
		internal bool detectHiddenText = true;
		internal bool detectHiddenWindows;
		internal Form dialogOwner;
		internal object eventInfo;
		internal Encoding fileEncoding;
		internal string formatNumeric;
		internal IFuncObj hotCriterion;
		internal IntPtr hwndLastUsed = IntPtr.Zero;
		internal bool isCritical = false;
		internal long keyDelay = 10L;
		internal long keyDelayPlay = -1L;
		internal long keyDuration = -1L;
		internal long keyDurationPlay = -1L;
		internal long lastFoundForm = 0L;
		internal Stack<LoopInfo> loops;
		internal long mouseDelay = 10L;
		internal long mouseDelayPlay = -1L;
		internal long peekFrequency = 5L;
		internal long priority;
		internal Random randomGenerator;
		internal StringBuilder regsb;
		internal long regView = 64L;
		internal string runDomain;
		internal SecureString runPassword;
		internal string runUser;
		internal uint sendLevel;
		internal SendModes sendMode = SendModes.Input;
		internal bool storeCapsLockMode = true;
		internal int threadId;
		internal DateTime threadStartTime = DateTime.MinValue;
		internal object titleMatchMode = 2L;
		internal bool titleMatchModeSpeed = true;
		internal int uninterruptibleDuration = 17;
		internal long winDelay = 100L;

		/// <summary>
		/// The fields in this function must be kept in sync with the fields declared above.
		/// </summary>
		public void Clear()
		{
			isCritical = false;
			allowThreadToBeInterrupted = true;
			uninterruptibleDuration = 17;
			threadStartTime = DateTime.MinValue;
			controlDelay = 20L;
			coords = null;
			currentTimer = null;
			defaultGui = null;
			defaultMouseSpeed = 2L;
			detectHiddenText = true;
			detectHiddenWindows = false;
			dialogOwner = null;
			eventInfo = null;
			fileEncoding = null;
			formatNumeric = null;
			hotCriterion = null;
			hwndLastUsed = IntPtr.Zero;
			keyDelay = 10L;
			keyDelayPlay = -1L;
			keyDuration = -1L;
			keyDurationPlay = -1L;
			lastFoundForm = 0L;
			loops?.Clear();
			mouseDelay = 10L;
			mouseDelayPlay = -1L;
			peekFrequency = 5L;
			priority = 0L;
			randomGenerator = null;
			_ = (regsb?.Clear());
			regView = 64L;
			runDomain = null;
			runPassword = null;
			runUser = null;
			sendLevel = 0;
			sendMode = SendModes.Input;
			storeCapsLockMode = true;
			threadId = 0;
			titleMatchMode = 2L;
			titleMatchModeSpeed = true;
			winDelay = 100L;
		}

		public void Init()
		{
			isCritical = false;
			allowThreadToBeInterrupted = true;
			uninterruptibleDuration = Keysharp.Scripting.Script.uninterruptibleTime;
			threadStartTime = DateTime.MinValue;
			controlDelay = Accessors.A_ControlDelay.Al();
			coords = null;
			currentTimer = null;
			defaultGui = null;
			defaultMouseSpeed = Accessors.A_DefaultMouseSpeed.Al();
			detectHiddenText = Accessors.A_DetectHiddenText.Ab();
			detectHiddenWindows = Accessors.A_DetectHiddenWindows.Ab();
			dialogOwner = null;
			eventInfo = null;
			fileEncoding = null;
			formatNumeric = null;
			hotCriterion = null;
			hwndLastUsed = IntPtr.Zero;
			keyDelay = Accessors.A_KeyDelay.Al();
			keyDelayPlay = Accessors.A_KeyDelayPlay.Al();
			keyDuration = Accessors.A_KeyDuration.Al();
			keyDurationPlay = Accessors.A_KeyDurationPlay.Al();
			lastFoundForm = IntPtr.Zero;
			loops = null;
			mouseDelay = Accessors.A_MouseDelay.Al();
			mouseDelayPlay = Accessors.A_MouseDelayPlay.Al();
			peekFrequency = Accessors.A_PeekFrequency.Al();
			priority = Accessors.A_Priority.Al();
			randomGenerator = null;
			_ = (regsb?.Clear());
			regView = Accessors.A_RegView.Al();
			runDomain = null;
			runPassword = null;
			runUser = null;
			sendLevel = Accessors.A_SendLevel.Aui();

			if (Enum.TryParse<SendModes>(Accessors.A_SendMode.As(), out var temp))
				sendMode = temp;

			storeCapsLockMode = Accessors.A_StoreCapsLockMode.Ab();
			threadId = 0;
			titleMatchMode = Accessors.A_TitleMatchMode;
			titleMatchModeSpeed = Accessors.A_TitleMatchModeSpeed.ToString() == Keywords.Keyword_Fast;
			winDelay = Accessors.A_WinDelay.Al();
		}
	}

	internal class ThreadVariableManager
	{
		/// <summary>
		/// These two work in conjuction to provide a simulated approach to thread local variables.
		/// Since AHK doesn't actually use threads, and instead behaves more as a stack-based system, using the built-in [ThreadStatic] attribute for members
		/// will not work.
		/// So we implement our own, but take a few steps for efficiency. We don't want to have to allocate an object every time a pseudo-thread is launched
		/// in response to a button click, hotkey, timer event, etc...
		/// So we create an object pool named threadVarsPool which we pull from and push onto a stack inside of threadVars that is local to the thread being launched.
		/// When the thread finishes, the object is popped from the thread's stack in threadVars, and returned to the object pool threadVarsPool.
		/// </summary>
		//internal static readonly int initialCapacity = 16;
		//[ThreadStatic]
		internal static SlimStack<ThreadVariables> threadVars;

		internal ConcurrentStackPool<ThreadVariables> threadVarsPool = new ConcurrentStackPool<ThreadVariables>((int)Script.MaxThreadsTotal);//Will start off with this many fully created/initialized objects.

		internal ThreadVariables CreateIfNeeded(int priority = 0
												, bool skipUninterruptible = false
												, bool isCritical = false)
		{
			if (threadVars == null)
				threadVars = new SlimStack<ThreadVariables>((int)Script.MaxThreadsTotal);

			return PushThreadVariables(priority, skipUninterruptible, isCritical, true); //Always start each thread with one entry.
		}

		internal ThreadVariables GetThreadVariables() => CreateIfNeeded();

		internal void PopThreadVariables(bool checkThread = true)
		{
			var ctid = Thread.CurrentThread.ManagedThreadId;

			//Do not check threadVars for null, because it should have always been created with a call to PushThreadVariables() before this.
			if (ctid != Processes.ManagedMainThreadID
					|| threadVars.Index > 0)//Never pop the last object on the main thread.
			{
				if (threadVars.TryPop(out var tv))
				{
					if (checkThread && ctid != tv.threadId)
						throw new Error($"Severe threading error. ThreadVariables.threadId {tv.threadId} did not match the current thread id {ctid}.");

					threadVarsPool.Return(tv);
				}
			}
		}

		internal void PopThreadVariablesFromOtherThread(SlimStack<ThreadVariables> otherThreadVars)
		{
			if (otherThreadVars.Index > 0)//Never pop the last object on the main thread.
				if (otherThreadVars.TryPop(out var tv))
					threadVarsPool.Return(tv);
		}

		internal ThreadVariables PushThreadVariables(int priority, bool skipUninterruptible,
				bool isCritical = false, bool onlyIfEmpty = false)
		{
			if (!onlyIfEmpty || threadVars.Index == 0)
			{
				var tv = threadVarsPool.Rent();//Get an object from the global pool.
				tv.threadId = Thread.CurrentThread.ManagedThreadId;
				tv.priority = priority;

				if (!skipUninterruptible)
				{
					if (!tv.isCritical)
						tv.isCritical = isCritical;

					if (Script.uninterruptibleTime != 0 || tv.isCritical) // v1.0.38.04.
					{
						tv.allowThreadToBeInterrupted = false;

						if (!tv.isCritical)
						{
							if (Script.uninterruptibleTime < 0) // A setting of -1 (or any negative) means the thread's uninterruptibility never times out.
							{
								tv.uninterruptibleDuration = -1; // "Lock in" the above because for backward compatibility, above is not supposed to affect threads after they're created. Override the default value contained in g_default.
								//g.ThreadStartTime doesn't need to be set when g.UninterruptibleDuration < 0.
							}
							else // It's now known to be >0 (due to various checks above).
							{
								// For backward compatibility, "lock in" the time this thread will become interruptible
								// because that's how previous versions behaved (i.e. 'Thread "Interrupt", NewTimeout'
								// doesn't affect the current thread, only the thread creation behavior in the future).
								// This also makes it more predictable, since AllowThreadToBeInterrupted is only changed
								// when IsInterruptible() is called, which might not happen in between changes to the setting.
								// For explanation of why two fields instead of one are used, see comments in IsInterruptible().
								tv.threadStartTime = DateTime.Now;
								tv.uninterruptibleDuration = Script.uninterruptibleTime;
							}
						}
					}
				}

				_ = threadVars.Push(tv);//Push it onto the stack for this thread.
				return tv;
			}
			else
				return threadVars.TryPeek();
		}
	}
}