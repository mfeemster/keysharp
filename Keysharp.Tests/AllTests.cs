using System.Windows.Forms;
using Keysharp.Core.Windows;
using NUnit.Framework;
using Buffer = Keysharp.Core.Buffer;

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
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			WindowsAPI.SetProcessDPIAware();
		}
	}

	public partial class Scripting
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void SimplePass() => Assert.IsTrue(TestScript("pass", true));

		[Test, Category("BuiltInVars")]
		public void PropsSpecialChars() => Assert.IsTrue(TestScript("props-special-chars", true));

		[Test, Category("BuiltInVars")]
		public void PropsScriptProperties() => Assert.IsTrue(TestScript("props-script-properties", false));

		[Test, Category("BuiltInVars")]
		public void PropsDateTime() => Assert.IsTrue(TestScript("props-date-time", true));

		[Test, Category("BuiltInVars")]
		public void PropsScriptSettings() => Assert.IsTrue(TestScript("props-script-settings", false));

		[Test, Category("BuiltInVars")]
		public void Suspend() => Assert.IsTrue(TestScript("suspend", true));
		/*
		            [Test]
		            public void Functions()
		            {
		            Assert.IsTrue(TestScript("functions"));
		            }

		            [Test]
		            public void Comments()
		            {
		            Assert.IsTrue(TestScript("comments"));
		            }

		            [Test]
		            public void Directive()
		            {
		            Assert.IsTrue(TestScript("directive"));
		            }

		            [Test]
		            public void Expressions()
		            {
		            Assert.IsTrue(TestScript("expressions"));
		            }

		            [Test]
		            public void Command()
		            {
		            Assert.IsTrue(TestScript("command"));
		            }

		            [Test]
		            public void Line()
		            {
		            Assert.IsTrue(TestScript("line"));
		            }

		            [Test]
		            public void Loop()
		            {
		            Assert.IsTrue(TestScript("loop"));
		            }

		            [Test]
		            public void Objects()
		            {
		            Assert.IsTrue(TestScript("objects"));
		            }

		            [Test]
		            public void Assign()
		            {
		            Assert.IsTrue(TestScript("assign"));
		            }

		            [Test]
		            public void Goto()
		            {
		            Assert.IsTrue(TestScript("goto"));
		            }

		            [Test]
		            public void Hotkey()
		            {
		            Assert.IsTrue(ValidateScript("hotkey"));
		            }

		            [Test]
		            public void VanillaExpressions()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-Expressions"));
		            }

		            [Test]
		            public void VanillaContinuation()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-Line Continuation"));
		            }

		            [Test]
		            public void VanillaMain()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-MAIN"));
		            }

		            [Test]
		            public void VanillaRegex()
		            {
		            Assert.IsTrue(ValidateScript("vanilla-RegExMatch & RegExReplace"));
		            }*/
	}
}
