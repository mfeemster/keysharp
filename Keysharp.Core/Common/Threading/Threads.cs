using static Keysharp.Core.Misc;

namespace Keysharp.Core.Common.Threading
{
	public static class Threads
	{
		private static ThreadVariableManager tvm = new ThreadVariableManager();

		public static (bool, ThreadVariables) BeginThread(bool onlyIfEmpty = false)
		{
			var skip = Flow.AllowInterruption == false;//This will be false when exiting the program.
			_ = Interlocked.Increment(ref Script.totalExistingThreads);
			return PushThreadVariables(0, skip, false, onlyIfEmpty);
		}

		public static void EndThread(bool pushed, bool checkThread = false)
		{
			PopThreadVariables(pushed, checkThread);
			_ = Interlocked.Decrement(ref Script.totalExistingThreads);
		}

		[PublicForTestOnly]
		public static (bool, ThreadVariables) PushThreadVariables(int priority, bool skipUninterruptible,
				bool isCritical = false, bool onlyIfEmpty = false) => tvm.PushThreadVariables(priority, skipUninterruptible, isCritical, onlyIfEmpty);

		internal static bool AnyThreadsAvailable() => Script.totalExistingThreads < Script.MaxThreadsTotal;

		internal static ThreadVariables GetThreadVariables() => tvm.GetThreadVariables();

		internal static bool IsInterruptible()
		{
			if (!Flow.AllowInterruption)
				return false;

			if (Script.totalExistingThreads == 0)//Before UserMainCode() starts to run.
				return true;

			var tv = GetThreadVariables();

			if (!tv.isCritical//Added this whereas AHK doesn't check it. We should never make a critical thread interruptible.
					&& !tv.allowThreadToBeInterrupted // Those who check whether g->AllowThreadToBeInterrupted==false should then check whether it should be made true.
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

		internal static void LaunchInThread(int priority, bool skipUninterruptible,
											bool isCritical, object func, object[] o, bool tryCatch)//Determine later the optimal threading model.//TODO
		{
			Task<object> tsk = null;

			try
			{
				var existingTv = GetThreadVariables();
				existingTv.WaitForCriticalToFinish();//Cannot launch a new task while a critical one is running.
				void AssignTask(ThreadVariables localTv) => localTv.task = tsk;//Needed to capture tsk so it can be used from within the Task.
				tsk = Task.Factory.StartNew(() =>
											//var t2 = Task.Run(() =>
				{
					object ret = null;
					(bool, ThreadVariables) btv = (false, null);
					_ = Interlocked.Increment(ref Script.totalExistingThreads);//Will be decremented in EndThread().
					btv = PushThreadVariables(priority, skipUninterruptible, isCritical);//Always start each thread with one entry.

					if (btv.Item1)
					{
						if (tryCatch)
						{
							_ = Misc.TryCatch(() =>
							{
								AssignTask(btv.Item2);

								if (func is VariadicFunction vf)
									ret = vf(o);
								else if (func is IFuncObj ifo)
									ret = ifo.Call(o);
								else
									ret = "";

								//throw new Error("ASDf");
								EndThread(btv.Item1);
							}, true);
						}
						else
						{
							AssignTask(btv.Item2);

							if (func is VariadicFunction vf)
								ret = vf(o);
							else if (func is IFuncObj ifo)
								ret = ifo.Call(o);
							else
								ret = "";

							EndThread(btv.Item1);
						}
					}

					return ret;
				},
				//This is needed to make a variety of functionality work inside of hotkey/string handlers.
				//Such as clipboard and window functions.
				CancellationToken.None, TaskCreationOptions.None,
				SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current);
			}
			//catch (AggregateException aex)
			catch (Exception ex)
			{
				//tvm.PopThreadVariables(false);
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;//Do not pass ex because it will reset the stack information.
			}

			//return tsk;
		}

		internal static void PopThreadVariables(bool pushed, bool checkThread = false) => tvm.PopThreadVariables(pushed, checkThread);
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
		internal static SlimStack<ThreadVariables> threadVars = new SlimStack<ThreadVariables>((int)Script.MaxThreadsTotal);

		internal ConcurrentStackPool<ThreadVariables> threadVarsPool = new ConcurrentStackPool<ThreadVariables>((int)Script.MaxThreadsTotal);//Will start off with this many fully created/initialized objects.

		internal ThreadVariables GetThreadVariables() => threadVars.TryPeek() ?? throw new Error("Tried to get an existing thread variable object but there were none. This should never happen.");

		internal void PopThreadVariables(bool pushed, bool checkThread = true)
		{
			var ctid = Thread.CurrentThread.ManagedThreadId;

			//Do not check threadVars for null, because it should have always been created with a call to PushThreadVariables() before this.
			if (ctid != Processes.ManagedMainThreadID
					|| threadVars.Index > 0)//Never pop the last object on the main thread.
			{
				//Script.OutputDebug($"About to pop with {threadVars.Index} existing threads");
				if (pushed && threadVars.TryPop(out var tv))
				{
					if (checkThread && ctid != tv.threadId)
						throw new Error($"Severe threading error. ThreadVariables.threadId {tv.threadId} did not match the current thread id {ctid}.");

					threadVarsPool.Return(tv);
				}
			}
		}

		internal (bool, ThreadVariables) PushThreadVariables(int priority, bool skipUninterruptible,
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
						//tv.allowThreadToBeInterrupted = false;//This is really only for the line count feature, which is not supported.//TODO
						tv.allowThreadToBeInterrupted = !tv.isCritical;

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

				//Script.OutputDebug($"About to push with {threadVars.Index} existing threads");
				var pushed = threadVars.Push(tv);//Push it onto the stack for this thread.

				if (!pushed)//Thread limit exceeded, so immediately return the rented ThreadVariables object. Caller should check pushed so as to not return twice.
					threadVarsPool.Return(tv);

				return (pushed, tv);
			}
			else
			{
				//Script.OutputDebug($"Wanted push, but only peeking with {threadVars.Index} existing threads");
				return (false, threadVars.TryPeek());
			}
		}
	}
}