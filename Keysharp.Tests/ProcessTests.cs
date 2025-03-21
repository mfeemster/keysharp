using static Keysharp.Core.Processes;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class ProcessTests : TestRunner
	{
		[Test, Category("Process")]
		public void ProcessRunWaitClose()
		{
			Misc.VarRef pid = new(null);
#if WINDOWS
			_ = Run("notepad.exe", "", "max", pid);
			_ = ProcessWait(pid.__Value);
			_ = ProcessSetPriority("H", pid.__Value);

			if (ProcessExist(pid.__Value) != 0)
			{
				Thread.Sleep(2000);
				_ = ProcessClose(pid.__Value);
				_ = ProcessWaitClose(pid.__Value);
			}

			Thread.Sleep(1000);
			pid.__Value = ProcessExist("notepad.exe");
			Assert.AreEqual(0L, pid.__Value);
			_ = RunWait("notepad.exe", "", "max");
			Thread.Sleep(1000);
			Assert.AreEqual(0L, ProcessExist("notepad.exe"));
			//Admin tools.
			Run("shell:::{D20EA4E1-3957-11D2-A40B-0C5020524153}");
			//This PC.
			Run("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
			//edit verb.
			Run("edit \"..\\..\\..\\Keysharp.Tests\\Code\\testini.ini\"");
			//explore verb.
			Run("explore " + Accessors.A_ProgramFiles.ToString());
			//find verb.
			Run("find D:\\");
			//Open file with default.
			Run("..\\..\\..\\Keysharp.Tests\\Code\\test-text-file.txt");
			//Run program as admin to open file.
			//Relative paths don't work with *runas, so convert to absolute.
			var fullPath = Path.GetFullPath("..\\..\\..\\Keysharp.Tests\\Code\\DirCopy\\file1.txt");
			Run("*runas C:\\Windows\\Notepad.exe \"" + fullPath + "\"");
			//URL.
			Run("https://www.google.com");
			//Create command line.
			Run(Accessors.A_ComSpec);

			//Create command line and run program.
			if (File.Exists("./DirTest.txt"))
				File.Delete("./DirTest.txt");

			RunWait(Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.Concat, Accessors.A_ComSpec, " /c dir C:\\ >>./DirTest.txt"), null, "Min");
			MessageBox.Show("Close everything that was opened by the process test before proceeding.");
			Assert.IsTrue(TestScript("process-run-wait-close", false));
			//Can't really test RunAs() or Shutdown(), but they have been manually tested individually.
#else
			_ = Run("xed", "", "max", pid);
			_ = ProcessWait(pid);

			//Skip process priority raising on linux, it can't be raised
			//above normal without being root.
			if (ProcessExist(pid) != 0)
			{
				System.Threading.Thread.Sleep(2000);
				_ = ProcessClose(pid);
				_ = ProcessWaitClose(pid);
			}

			System.Threading.Thread.Sleep(1000);
			pid = ProcessExist("xed");
			Assert.AreEqual(0L, pid);
			_ = RunWait("xed", "", "max");
			System.Threading.Thread.Sleep(1000);
			Assert.AreEqual(0L, ProcessExist("xed"));
			Assert.IsTrue(TestScript("process-run-wait-close", false));
#endif
		}
	}
}