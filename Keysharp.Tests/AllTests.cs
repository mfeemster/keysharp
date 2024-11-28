using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	[SetUpFixture]
	public class MySetUpClass
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			//var dir = Path.GetDirectoryName(typeof(MySetUpClass).Assembly.Location);
			//Environment.CurrentDirectory = dir;
			// or
			//Directory.SetCurrentDirectory(dir);
			_ = Window.SetProcessDPIAware();
			//Reflections.Initialize();
			_ = Core.Common.Threading.Threads.PushThreadVariables(0, true, false, true);//Ensure there is always one thread in existence for reference purposes, but do not increment the actual thread counter.
		}
	}
}