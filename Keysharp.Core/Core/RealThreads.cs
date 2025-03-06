namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for real threads-releated functions.
	/// These differ than the pseudo-threads used throughout the rest of the library.
	/// </summary>
	public static class RealThreads
	{
		/// <summary>
		/// Runs a function object inside of a lock statement.
		/// This is useful for calling a function inside of a real thread.
		/// </summary>
		/// <param name="lockObj">The object to lock on when calling the function.</param>
		/// <param name="obj1">The name of the function or a function object.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>The object the function object returned.</returns>
		public static object LockRun(object lockObj, object obj1, params object[] args)
		{
			lock (lockObj)
			{
				var funcObj = Functions.GetFuncObj(obj1, null, true);
				return funcObj.Call(funcObj.Inst == null ? args : new[] { funcObj.Inst }.Concat(args));
			}
		}

		/// <summary>
		/// Runs a function object inside of a C# task.
		/// </summary>
		/// <param name="obj">The name of the function or a function object.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>The <see cref="RealThread"/> object.</returns>
		public static object StartRealThread(object obj, params object[] args)
		{
			var funcObj = Functions.GetFuncObj(obj, null, true);
			var tsk = Task.Run(() => funcObj.Call(args));
			return new RealThread(tsk);
		}
	}

	/// <summary>
	/// An object that encapsulates a C# Task.
	/// </summary>
	public sealed class RealThread : KeysharpObject
	{
		internal Task<object> task;

		/// <summary>
		/// The implementation for <see cref="KeysharpObject.super"/> for this class to return this type.
		/// </summary>
		public new (Type, object) super => (typeof(KeysharpObject), this);

		/// <summary>
		/// Constructor that takes a task to keep a reference to.
		/// </summary>
		/// <param name="t">The task to hold.</param>
		public RealThread(Task<object> t)
		{
			task = t;
		}

		/// <summary>
		/// Encapsulates a call to <see cref="Task.ContinueWith()"/>.
		/// </summary>
		/// <param name="obj">The name of the function or a function object.</param>
		/// <param name="args">The arguments to pass to the function.</param>
		/// <returns>The new <see cref="RealThread"/> object</returns>
		public object ContinueWith(object obj, params object[] args)
		{
			var fo = Functions.GetFuncObj(obj, null, true);
			var rt = task.ContinueWith((to) => fo.Call(args));
			return new RealThread(rt);
		}

		/// <summary>
		/// Wait for the existing task either indefinitely or for a timeout period.
		/// </summary>
		/// <param name="obj">The timeout duration to wait. Default: wait indefinitely.</param>
		/// <returns>The result of the task.</returns>
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