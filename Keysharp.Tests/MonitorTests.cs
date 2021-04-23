using System;
using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Monitor")]
		public void MonitorGet()
		{
			var monget = Keysharp.Core.Monitor.MonitorGet();
			Assert.IsTrue(monget.Left >= 0);
			Assert.IsTrue(monget.Right >= 0);
			Assert.IsTrue(monget.Top >= 0);
			Assert.IsTrue(monget.Bottom >= 0);
			Assert.IsTrue(monget.N > 0);
			Assert.IsTrue(TestScript("monitor-monitorget", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetCount()
		{
			var ct = Keysharp.Core.Monitor.MonitorGetCount();
			Assert.IsTrue(ct > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetcount", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetName()
		{
			var names = "";
			var ct = Keysharp.Core.Monitor.MonitorGetCount();

			for (var i = 1; i <= ct; i++)
				names += Keysharp.Core.Monitor.MonitorGetName(i) + Environment.NewLine;

			Assert.IsTrue(names != "");
			Assert.IsTrue(TestScript("monitor-monitorgetname", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetPrimary()
		{
			var ct = Keysharp.Core.Monitor.MonitorGetPrimary();
			Assert.IsTrue(ct > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetprimary", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetWorkArea()
		{
			var monget = Keysharp.Core.Monitor.MonitorGetWorkArea();
			Assert.IsTrue(monget.Left >= 0);
			Assert.IsTrue(monget.Right >= 0);
			Assert.IsTrue(monget.Top >= 0);
			Assert.IsTrue(monget.Bottom >= 0);
			Assert.IsTrue(monget.N > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetworkarea", true));
		}
	}
}