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

			if (func is IFuncObj ifo)
			{
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
						return ifo.Call(o);
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
			}
			else if (func is VariadicFunction vf)//Most likely no longer used.//TODO
			{
				_ = Interlocked.Increment(ref Keysharp.Scripting.Script.totalExistingThreads);

				try
				{
					tsk = Task.Factory.StartNew(() =>
					{
						System.Threading.Thread.CurrentThread.Priority = Accessors.threadPriorityDef;
						return vf(o);
					});
					_ = await tsk;
				}
				catch (Exception ex)
				{
				}
				finally
				{
					Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads);
				}

				return tsk;
				//return tsk.ContinueWith((_) => Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads));
			}

			return Task.FromResult<object>("");
		}
	}
}