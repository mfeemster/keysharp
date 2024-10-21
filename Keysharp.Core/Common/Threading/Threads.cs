namespace Keysharp.Core.Common.Threading
{
	public static class Threads
	{
		private static readonly ThreadVariableManager tvm = new ThreadVariableManager();

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
			{
				// Once the thread becomes interruptible by any means, g->ThreadStartTime/UninterruptibleDuration
				// can never matter anymore because only Critical (never "Thread Interrupt") can turn off the
				// interruptibility again, and it resets g->UninterruptibleDuration.
				tv.allowThreadToBeInterrupted = true; // Avoids issues with 49.7 day limit of 32-bit TickCount, and also helps performance future callers of this function (they can skip most of the checking above).

				if (!tv.isCritical)
					tv.peekFrequency = ThreadVariables.DefaultPeekFrequency;
			}

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
				//tsk = Task.Factory.StartNew(() =>
				tsk = StaTask.Run(() =>//This appears to be necessary to use the clipboard within hotkey/string events.
				{
					object ret = null;
					(bool, ThreadVariables) btv = (false, null);
					_ = Interlocked.Increment(ref Script.totalExistingThreads);//Will be decremented in EndThread().
					btv = PushThreadVariables(priority, skipUninterruptible, isCritical);//Always start each thread with one entry.

					if (btv.Item1)
					{
						if (tryCatch)
						{
							_ = Flow.TryCatch(() =>
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
				});
				//,
				//This is needed to make a variety of functionality work inside of hotkey/string handlers.
				//Such as clipboard and window functions.
				//CancellationToken.None, TaskCreationOptions.None,
				//SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current);
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
}