using System.Collections.Generic;
using Keysharp.Scripting;
using NUnit.Framework;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Monitor")]
		public void MonitorGet()
		{
			object l = null, t = null, r = null, b = null;
			var monget = Keysharp.Core.Monitor.MonitorGet(null, ref l, ref t, ref r, ref b);
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
				names += Keysharp.Core.Monitor.MonitorGetName(i) + Keywords.newlineToUse;

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
			object l = null, t = null, r = null, b = null;
			var monget = Keysharp.Core.Monitor.MonitorGetWorkArea(null, ref l, ref t, ref r, ref b);
			Assert.IsTrue(l.Ai() >= 0);
			Assert.IsTrue(r.Ai() >= 0);
			Assert.IsTrue(t.Ai() >= 0);
			Assert.IsTrue(b.Ai() >= 0);
			Assert.IsTrue(monget.Ai() > 0);
			Assert.IsTrue(TestScript("monitor-monitorgetworkarea", true));
		}
	}
}