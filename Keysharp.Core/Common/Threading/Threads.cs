using System;
using System.Threading;
using System.Threading.Tasks;
using static Keysharp.Core.Misc;

namespace Keysharp.Core.Common.Threading
{
	public static class Threads
	{
		internal static async Task<object> LaunchInThread(object func, object[] o)//Determine later the optimal threading model.//TODO
		{
			Task<object> tsk = null;
			_ = Interlocked.Increment(ref Keysharp.Scripting.Script.totalExistingThreads);

			try
			{
				tsk = Task.Factory.StartNew<object>(() =>
				{
					//throw new System.Exception("ASDf");
					//throw new Error("ASDf");
					System.Threading.Thread.CurrentThread.Priority = Accessors.threadPriorityDef;

					if (func is VariadicFunction vf)
						return vf(o);
					else if (func is IFuncObj ifo)
						return ifo.Call(o);
					else
						return "";
				},
				//This is needed to make a variety of functionality work inside of hotkey/string handlers.
				//Such as clipboard and window functions.
				//Unsure if this is exactly how we'll do threading, vs doing it some other way such as async.
				CancellationToken.None, TaskCreationOptions.None,
				SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current);
				_ = await tsk;
			}
			//catch (AggregateException aex)
			catch (Exception ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw;//Do not pass ex because it will reset the stack information.
			}
			finally
			{
				_ = Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads);
			}

			return tsk;
		}
	}
}