using System.Threading;
using System.Threading.Tasks;

namespace Keysharp.Core.Common.Threading
{
	public static class Threads
	{
		internal static Task<int> LaunchInThread(object func, object[] o)//Determine later the optimal threading model.//TODO
		{
			Interlocked.Increment(ref Keysharp.Scripting.Script.totalExistingThreads);

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
			else if (func is GenericFunction gf)
			{
				var tsk = Task.Factory.StartNew(() => gf(o));
				return tsk.ContinueWith((_) => Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads));
				//return tsk;
			}
			else if (func is HotFunction hf)
			{
				var tsk = Task.Factory.StartNew(() => hf(o));
				return tsk.ContinueWith((_) => Interlocked.Decrement(ref Keysharp.Scripting.Script.totalExistingThreads));
				//return tsk;
			}

			return Task.FromResult<int>(1);
		}
	}

	public delegate object ClipFunction(params object[] o);

	public delegate void ClipUpdateDel(params object[] o);

	/// <summary>
	/// A function.
	/// </summary>
	/// <param name="args">Parameters.</param>
	/// <returns>A value.</returns>
	public delegate object GenericFunction(params object[] args);

	//Original used GenericFunction, but it appears hotkey/string functions don't ever use their return values, and having return statements makes them hard to generate.
	//Revisit this later if needed.//MATT
	public delegate void HotFunction(object[] o);//As written IronAHK does not pass args, but once we redesign, we probably will.//MATT

	public delegate void SimpleDelegate();
}