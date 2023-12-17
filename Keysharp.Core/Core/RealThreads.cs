using System.Collections.Generic;
using System.Threading.Tasks;

namespace Keysharp.Core
{
	public static class RealThreads
	{
		public static object LockObject(object obj0, object obj1, params object[] args)
		{
			lock (obj0)
			{
				var funcObj = Function.GetFuncObj(obj1, null, true);
				return funcObj.Call(args);
			}
		}

		public static object StartRealThread(object obj, params object[] args)
		{
			var funcObj = Function.GetFuncObj(obj, null, true);
			var tsk = Task.Run(() => funcObj.Call(args));
			return tsk;
		}

		public static object WaitRealThread(object obj0, object obj1 = null)
		{
			if (obj0 is Task<object> t)
			{
				var timeout = obj1.Ai(-1);

				if (timeout > 0)
					_ = t.Wait(timeout);
				else
					t.Wait();

				return t.Result;
			}
			else
				throw new Error($"Object of type {obj0.GetType()} was not of type Task<object>.");
		}
	}
}