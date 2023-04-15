using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using Keysharp.Core;
using static Keysharp.Core.Processes;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using static Keysharp.Core.Core;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Process")]
		public void ProcessRunWaitClose()
		{
			var pid = Run("notepad.exe", "", "max");
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