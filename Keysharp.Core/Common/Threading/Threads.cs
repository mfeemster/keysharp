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
			//var context = TaskScheduler.FromCurrentSynchronizationContext();
			Task<object> tsk = null;
			_ = Interlocked.Increment(ref Keysharp.Scripting.Script.totalExistingThreads);

			//ifo.Call(o);
			//return Task.FromResult<int>(1);

			try
			{
				tsk = Task.Factory.StartNew<object>(() =>
				{
					//throw new System.Exception("ASDf");
					//throw new Error("ASDf");
					System.Threading.Thread.CurrentThread.Priority = Accessors.threadPriorityDef;

					//_ = ifo.Call(o);
					if (func is IFuncObj ifo)
						return ifo.Call(o);
					else if (func is VariadicFunction vf)
						return vf(o);
					else
						return "";
				});
				_ = await tsk;
			}
			//catch (AggregateException aex)
			catch (Exception ex)
			{
				if (ex.InnerException != null)
					throw ex.InnerException;
				else
					throw ex;
			}
			finally
			{
				Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads);
			}

			return tsk;
			//tsk.ContinueWith((_) => Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads));
			//return tsk.ContinueWith((t, tt) => { throw t.Exception; }, context, TaskContinuationOptions.OnlyOnFaulted);
			//return Task.FromResult<object>("");
		}
	}
}