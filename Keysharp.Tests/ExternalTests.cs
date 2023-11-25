using Keysharp.Core;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Windows;
using NUnit.Framework;
using static Keysharp.Core.Dll;
using static Keysharp.Core.Misc;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("External")]
		public void CallbackCreate() => Assert.IsTrue(TestScript("external-callbackcreate", false));

		[Test, Category("External")]
		public void DllCall_()
		{
			_ = Threads.PushThreadVariables(0, true, false, true);//Ensure there is always one thread in existence for reference purposes, but do not increment the actual thread counter.
			var desktop = WindowsAPI.GetDesktopWindow();
			var rect = new RECT();
			var buf = Keysharp.Core.Misc.Buffer(16, 0);
			_ = Dll.DllCall("user32.dll\\GetWindowRect", "ptr", desktop, "ptr", buf);
			_ = WindowsAPI.GetWindowRect(desktop, out rect);
			var l = (long)NumGet(buf, 0, "UInt");
			var t = (long)NumGet(buf, 4, "UInt");
			var r = (long)NumGet(buf, 8, "UInt");
			var b = (long)NumGet(buf, 12, "UInt");
			Assert.IsTrue(r > 0 && b > 0);
			Assert.AreEqual(rect.Left, l);
			Assert.AreEqual(rect.Right, r);
			Assert.AreEqual(rect.Top, t);
			Assert.AreEqual(rect.Bottom, b);
			var str = "lower";
			var len = str.Length;
			var strbuf = Keysharp.Core.Misc.StringBuffer(str);
			_ = Dll.DllCall("user32.dll\\CharUpperBuff", "ptr", strbuf, "UInt", len);
			Assert.AreEqual(strbuf.ToString(), str.ToUpper());
			//object freq = 0L;
			//object counterbefore = 0L;
			//object counterafter = 0L;
			//Keysharp.Scripting.Script.InvokeWithRefs(Keysharp.Scripting.Script.GetMethodOrProperty(null, "DllCall", 3), "QueryPerformanceFrequency", "Int64*", Mrh(2, freq, v => freq = v));
			//Keysharp.Scripting.Script.InvokeWithRefs(Keysharp.Scripting.Script.GetMethodOrProperty(null, "DllCall", 3), "QueryPerformanceCounter", "Int64*", Mrh(2, counterbefore, v => counterbefore = v));
			//Keysharp.Core.Flow.Sleep(1000L);
			//Keysharp.Scripting.Script.InvokeWithRefs(Keysharp.Scripting.Script.GetMethodOrProperty(null, "DllCall", 3), "QueryPerformanceCounter", "Int64*", Mrh(2, counterafter, v => counterafter = v));
			//var elapsed = Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Multiply, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Divide, Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Subtract, counterafter, counterbefore), freq), 1000L);
			Assert.IsTrue(TestScript("external-dllcall", true));
		}

		[Test, Category("External")]
		public void NumPutNumGet() => Assert.IsTrue(TestScript("external-numput-numget", true));
	}
}