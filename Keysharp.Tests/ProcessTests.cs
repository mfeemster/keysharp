using static Keysharp.Core.Processes;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class ProcessTests : TestRunner
	{
		[Test, Category("Process")]
		public void ProcessRunWaitClose()
		{
			object pid = null;
			_ = Run("notepad.exe", "", "max", ref pid);
			_ = ProcessWait(pid);
			_ = ProcessSetPriority("H", pid);

			if (ProcessExist(pid) != 0)
			{
				System.Threading.Thread.Sleep(2000);
				_ = ProcessClose(pid);
				_ = ProcessWaitClose(pid);
			}

			System.Threading.Thread.Sleep(1000);
			pid = ProcessExist("notepad.exe");
			Assert.AreEqual(0L, pid);
			_ = RunWait("notepad.exe", "", "max");
			System.Threading.Thread.Sleep(1000);
			Assert.AreEqual(0L, ProcessExist("notepad.exe"));
			Assert.IsTrue(TestScript("process-run-wait-close", false));
			//Can't really test RunAs() or Shutdown(), but they have been manually tested individually.
		}
	}
}