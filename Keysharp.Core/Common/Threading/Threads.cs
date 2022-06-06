using System.Threading;
using System.Threading.Tasks;
using static Keysharp.Core.Misc;

namespace Keysharp.Core.Common.Threading
{
	public static class Threads
	{
		internal static Task<int> LaunchInThread(object func, object[] o)//Determine later the optimal threading model.//TODO
		{
			_ = Interlocked.Increment(ref Keysharp.Scripting.Script.totalExistingThreads);

			if (func is IFuncObj ifo)
			{
				//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
				//      //Keysharp.Scripting.Script.mainWindow.CheckedInvoke(() =>
				//{
				//  ifo.Call(o);
				//  Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads);
				//});
				var tsk = Task.Factory.StartNew(() => ifo.Call(o));
				return tsk.ContinueWith((_) => Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads));
			}
			else if (func is VariadicFunction vf)
			{
				var tsk = Task.Factory.StartNew(() => vf(o));
				return tsk.ContinueWith((_) => Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads));
				//return tsk;
			}
			
			return Task.FromResult<int>(1);
		}
	}
}