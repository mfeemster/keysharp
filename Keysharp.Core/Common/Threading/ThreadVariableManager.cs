namespace Keysharp.Core.Common.Threading
{
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
		internal static SlimStack<ThreadVariables> threadVars = new ((int)Script.MaxThreadsTotal);

		internal ConcurrentStackPool<ThreadVariables> threadVarsPool = new ((int)Script.MaxThreadsTotal); //Will start off with this many fully created/initialized objects.

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

					_ = threadVarsPool.Return(tv);
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
								tv.threadStartTime = DateTime.UtcNow;
								tv.uninterruptibleDuration = Script.uninterruptibleTime;
							}
						}
					}
				}

				//Script.OutputDebug($"About to push with {threadVars.Index} existing threads");
				var pushed = threadVars.Push(tv);//Push it onto the stack for this thread.

				if (!pushed)//Thread limit exceeded, so immediately return the rented ThreadVariables object. Caller should check pushed so as to not return twice.
					_ = threadVarsPool.Return(tv);

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

/*
    An alternative that internally stores the objects rather than getting them from an outside source.
    internal class ThreadVariableManager
    {
        /// <summary>
        /// This is a simulated approach to thread local variables.
        /// Since AHK doesn't actually use threads, and instead behaves more as a stack-based system, using the built-in [ThreadStatic] attribute for members
        /// will not work.
        /// Since ThreadVariableManager is always used in a ThreadLocal context then this
        /// class is not built for concurrency but instead maximal efficiency.
        /// </summary>
        internal static ThreadVariableStack threadVars = new ((int)Script.MaxThreadsTotal);

        internal ThreadVariables GetThreadVariables()
        {
            Error err;
            return threadVars.TryPeek() ?? (Errors.ErrorOccurred(err = new Error("Severe threading error: Tried to get an existing thread variable object but there were none. This should never happen."), Keyword_ExitApp) ? throw err : null);
        }

        internal void PopThreadVariables(bool pushed, bool checkThread = true)
        {
            Error err;
            var ctid = Thread.CurrentThread.ManagedThreadId;

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
                ThreadVariables tv = null;
                var pushed = threadVars.Push(out tv);

                if (pushed)
                {

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

                return (pushed, tv);
            }
            else
            {
                //Script.OutputDebug($"Wanted push, but only peeking with {threadVars.Index} existing threads");
                return (false, threadVars.TryPeek());
            }
        }
    }

    /// <summary>
    /// This class is meant to be a highly optimized stack of ThreadVariables (or in essence, pseudo-threads).<br/>
    /// It does not resize and is meant to be used in a scenario where the stack is known to never exceed a given size.<br/>
    /// ThreadVariableStack is initialized to a constant size s, at which point s ThreadVariables<br/>
    /// instances are created. Then when Push is called we increase an internal counter, call Init<br/>
    /// on the ThreadVariables object, and return it. TryPop simply decreases the counter.</br>
    [PublicForTestOnly]
    public class ThreadVariableStack
    {
        /// <summary>
        /// The internal array used to store the ThreadVariables.
        /// </summary>
        private readonly ThreadVariables[] list;

        /// <summary>
        /// The size of the list which is set once during construction and does not change.
        /// </summary>
        private readonly int size;

        /// <summary>
        /// The current 1-based size/index of the stack which may be less than or equal to the size of the list.
        /// </summary>
        private int index;

        /// <summary>
        /// Gets the current 1-based index of the stack.
        /// </summary>
        public int Index => index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadVariableStack"/> class.<br/>
        /// Creates an array of size <paramref name="s"/> and fills it with empty ThreadVariables instances.
        /// </summary>
        /// <param name="s">The maximum size of the stack.</param>
        public ThreadVariableStack(int s)
        {
            index = 0;
            size = s;
            list = new ThreadVariables[size];

            for (var i = 0; i < size; i++)
                list[i] = new ThreadVariables();//Empty placeholder slots.
        }

        /// <summary>
        /// Pushes a new thread onto the stack and initializes it.
        /// </summary>
        /// <param name="obj">The variable to set to the newly pushed thread.</param>
        /// <returns>True if the thread was pushed, else false if there was no available space.</returns>
        public bool Push(out ThreadVariables obj)
        {
            if (index < size)
            {
                obj = list[++index];
                obj.Init();
                return true;
            }
            obj = null;
            return false;//No room
        }

        /// <summary>
        /// Returns the most recent thread in the stack if it exists.<br/>
        /// </summary>
        /// <returns>The most recent thread, else null if empty.</returns>
        public ThreadVariables TryPeek()
        {
            var i = index;
            return i > 0 && i <= list.Length ? list[i - 1] : null;
        }

        /// <summary>
        /// Removes and return the most recent thread in the stack if it exists.
        /// </summary>
        /// <param name="obj">A reference to the object which will hold the popped thread.
        /// This will be null if no element was popped.
        /// </param>
        /// <returns>True if a thread was successfully removed and assigned to obj, else false.</returns>
        public bool TryPop(out ThreadVariables obj)
        {
            if (index >= 1)
            {
                obj = list[--index];
                return true;
            }

            obj = null;
            return false;
        }
    }
*/