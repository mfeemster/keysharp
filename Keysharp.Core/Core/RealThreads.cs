namespace Keysharp.Core
{
	public static class RealThreads
	{
		public static object LockRun(object obj0, object obj1, params object[] args)
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
			return new RealThread(tsk);
		}
	}

	public sealed class RealThread : KeysharpObject
	{
		internal Task<object> task;

		public RealThread(Task<object> t)
		{
			task = t;
		}

		public object ContinueWith(object obj, params object[] args)
		{
			var fo = Function.GetFuncObj(obj, null, true);
			var rt = task.ContinueWith((to) => fo.Call(args));
			return new RealThread(rt);
		}

		public object Wait(object obj = null)
		{
			var timeout = obj.Ai(-1);

			if (timeout > 0)
				_ = task.Wait(timeout);
			else
				task.Wait();

			return task.Result;
		}
	}
}