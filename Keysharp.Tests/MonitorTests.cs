using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class MonitorTests : TestRunner
	{
		[Test, Category("Monitor")]
		public void MonitorGet()
		{
			object l = null, t = null, r = null, b = null;
			var monget = Core.Monitor.MonitorGet(null, ref l, ref t, ref r, ref b);
			Assert.IsTrue(l.Ai() >= 0);
			Assert.IsTrue(r.Ai() >= 0);
			Assert.IsTrue(t.Ai() >= 0);
			Assert.IsTrue(b.Ai() >= 0);
			Assert.IsTrue(monget.Ai() > 0);
			Assert.IsTrue(TestScript("monitor-monitorget", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetCount()
		{
			var ct = Core.Monitor.MonitorGetCount();
			Assert.IsTrue(ct > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetcount", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetName()
		{
			var names = "";
			var ct = Core.Monitor.MonitorGetCount();

			for (var i = 1; i <= ct; i++)
				names += Core.Monitor.MonitorGetName(i) + Environment.NewLine;

			Assert.IsTrue(names != "");
			Assert.IsTrue(TestScript("monitor-monitorgetname", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetPrimary()
		{
			var ct = Core.Monitor.MonitorGetPrimary();
			Assert.IsTrue(ct > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetprimary", true));
		}

		[Test, Category("Monitor")]
		public void MonitorGetWorkArea()
		{
			object l = null, t = null, r = null, b = null;
			var monget = Core.Monitor.MonitorGetWorkArea(null, ref l, ref t, ref r, ref b);
			Assert.IsTrue(l.Ai() >= 0);
			Assert.IsTrue(r.Ai() >= 0);
			Assert.IsTrue(t.Ai() >= 0);
			Assert.IsTrue(b.Ai() >= 0);
			Assert.IsTrue(monget.Ai() > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetworkarea", true));
		}
	}
}