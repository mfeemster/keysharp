namespace Keysharp.Core.Common.Threading
{
	internal class ThreadVariableManager
	{
		/// <summary>
		/// Since AHK doesn't actually use threads, and instead behaves more as a stack-based system, using the built-in [ThreadStatic] attribute for members
		/// will not work.
		/// So we implement our own, but take a few steps for efficiency. We don't want to have to allocate an object every time a pseudo-thread is launched
		/// in response to a button click, hotkey, timer event, etc...
		/// So we create a SlimStack to be used as an object pool which we push and pop each time a thread starts and finishes.
		/// </summary>
		internal static SlimStack<ThreadVariables> threadVars = new ((int)Script.MaxThreadsTotal, () => new ThreadVariables());

		internal ThreadVariables GetThreadVariables()
		{
			Error err;
			return threadVars.TryPeek() ?? (Errors.ErrorOccurred(err = new Error("Severe threading error: Tried to get an existing thread variable object but there were none. This should never happen."), Keyword_ExitApp) ? throw err : null);
		}

		internal void PopThreadVariables(bool pushed, bool checkThread = true)
		{
			Error err;
			var ctid = Thread.CurrentThread.ManagedThreadId;

			//Do not check threadVars for null, because it should have always been created with a call to PushThreadVariables() before this.
			if (ctid != Processes.ManagedMainThreadID
					|| threadVars.Index > 0)//Never pop the last object on the main thread.
			{
				//Script.OutputDebug($"About to pop with {threadVars.Index} existing threads");
				if (pushed && threadVars.TryPop(out var tv))
				{
					if (checkThread && ctid != tv.threadId)
					{
						_ = Errors.ErrorOccurred(err = new Error($"Severe threading error: ThreadVariables.threadId {tv.threadId} did not match the current thread id {ctid}. This should never happen."), Keyword_ExitApp) ? throw err : "";
						return;
					}
				}
			}
		}

		internal (bool, ThreadVariables) PushThreadVariables(int priority, bool skipUninterruptible,
				bool isCritical = false, bool onlyIfEmpty = false)
		{
			if (!onlyIfEmpty || threadVars.Index == 0)
			{
				var pushed = threadVars.TryPush(out var tv);

				if (pushed)
				{
					tv.Init();
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
									tv.threadStartTime = DateTime.UtcNow;
									tv.uninterruptibleDuration = Script.uninterruptibleTime;
								}
							}
						}
					}
				}
				else
					Debug.OutputDebug("Thread stack limit exceeded");

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