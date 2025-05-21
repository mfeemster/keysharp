namespace Keysharp.Core.Common.Threading
{
	public class Threads
	{
		/// <summary>
		/// Each thread has its own TVM. This means the main UI thread gets one, and the hook thread gets one.
		/// This allows the hook thread to run #HotIf evaluations separately without interfering with the main thread.
		/// It could conceivably lead to more threads being in existence than the user allowed, for a very brief moment.
		/// This shouldn't be a problem though.
		/// Note that we use ThreadLocal<T> here because it allows initialization for each thread, whereas
		/// [ThreadStatic] doesn't.
		/// </summary>
		private readonly Lock locker = new ();
		private ThreadVariableManager tvm = new ();

		public Threads()
		{
			_ = PushThreadVariables(0, true, false, true);//Ensure there is always one thread in existence for reference purposes, but do not increment the actual thread counter.
		}

		public (bool, ThreadVariables) BeginThread(bool onlyIfEmpty = false)
		{
			lock (locker)
			{
				var skip = Script.TheScript.FlowData.allowInterruption == false;//This will be false when exiting the program.
				return PushThreadVariables(0, skip, false, onlyIfEmpty, true);
			}
		}

		public object EndThread(bool pushed, bool checkThread = false)
		{
			lock (locker)
			{
				PopThreadVariables(pushed, checkThread);
				_ = Interlocked.Decrement(ref Script.TheScript.totalExistingThreads);
				return null;
			}
		}

		[PublicForTestOnly]
		public (bool, ThreadVariables) PushThreadVariables(int priority, bool skipUninterruptible,
				bool isCritical = false, bool onlyIfEmpty = false, bool inc = false)
		{
			lock (locker)
			{
				if (inc)
					_ = Interlocked.Increment(ref Script.TheScript.totalExistingThreads);//Will be decremented in EndThread().

				return tvm.PushThreadVariables(priority, skipUninterruptible, isCritical, onlyIfEmpty);
			}
		}

		internal bool AnyThreadsAvailable()
		{
			lock (locker)
			{
				var script = Script.TheScript;
				return script.totalExistingThreads < script.MaxThreadsTotal;
			}
		}

		internal ThreadVariables GetThreadVariables()
		{
			lock (locker)
			{
				return tvm.GetThreadVariables();
			}
		}

		internal bool IsInterruptible()
		{
			lock (locker)
			{
				var script = Script.TheScript;

				if (!script.FlowData.allowInterruption)
					return false;

				if (script.totalExistingThreads == 0)//Before _ks_UserMainCode() starts to run.1
					return true;

				var tv = GetThreadVariables();

				if (!tv.isCritical//Added this whereas AHK doesn't check it. We should never make a critical thread interruptible.
						&& !tv.allowThreadToBeInterrupted // Those who check whether g->AllowThreadToBeInterrupted==false should then check whether it should be made true.
						&& tv.uninterruptibleDuration > -1 // Must take precedence over the below.  g_script.mUninterruptibleTime is not checked because it's supposed to go into effect during thread creation, not after the thread is running and has possibly changed the timeout via 'Thread "Interrupt"'.
						&& (DateTime.UtcNow - tv.threadStartTime).TotalMilliseconds >= tv.uninterruptibleDuration // See big comment section above.
						&& !script.FlowData.callingCritical // In case of "Critical" on the first line.  See v2.0 comment above.
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
		}

		internal void LaunchInThread(int priority, bool skipUninterruptible,
									 bool isCritical, object func, object[] o, bool tryCatch)//Determine later the optimal threading model.//TODO
		{
			Task<object> tsk = null;

			try
			{
				var existingTv = GetThreadVariables();
				existingTv.WaitForCriticalToFinish();//Cannot launch a new task while a critical one is running.
				//void AssignTask(ThreadVariables localTv) => localTv.task = tsk;//Needed to capture tsk so it can be used from within the Task.
				//tsk = Task.Factory.StartNew(() =>
				//tsk = StaTask.Run(() =>//This appears to be necessary to use the clipboard within hotkey/string events.
				Script.TheScript.mainWindow.CheckedBeginInvoke(() =>
				{
					object ret = null;
					(bool, ThreadVariables) btv = (false, null);
					btv = PushThreadVariables(priority, skipUninterruptible, isCritical, false, true);//Always start each thread with one entry.
					btv.Item2.task = true;

					if (btv.Item1)
					{
						if (tryCatch)
						{
							_ = Flow.TryCatch(() =>
							{
								//AssignTask(btv.Item2);
								if (func is VariadicFunction vf)
									ret = vf(o);
								else if (func is IFuncObj ifo)
									ret = ifo.Call(o);
								else
									ret = "";

								_ = EndThread(btv.Item1);
							}, true);//Pop on exception because EndThread() above won't be called.
						}
						else
						{
							//AssignTask(btv.Item2);
							if (func is VariadicFunction vf)
								ret = vf(o);
							else if (func is IFuncObj ifo)
								ret = ifo.Call(o);
							else
								ret = "";

							_ = EndThread(btv.Item1);
						}
					}

					btv.Item2.task = false;
					//return ret;
				}, true, false);
				//,
				//This is needed to make a variety of functionality work inside of hotkey/string handlers.
				//Such as clipboard and window functions.
				//CancellationToken.None, TaskCreationOptions.None,
				//SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current);
				//tsk.Wait();
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

		internal void PopThreadVariables(bool pushed, bool checkThread = false) => tvm.PopThreadVariables(pushed, checkThread);
	}
}