using System;
using System.Globalization;
using System.Windows.Forms;
using Keysharp.Core;
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
			//Keysharp.Core.Window.SetProcessDPIAware();
			//Reflections.Initialize();
		}
	}

	public partial class Scripting
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test, Category("BuiltInVars")]
		public void PropsSpecialChars() => Assert.IsTrue(TestScript("props-special-chars", true));

		[Test, Category("BuiltInVars")]
		public void PropsScriptProperties() => Assert.IsTrue(TestScript("props-script-properties", false));

		[Test, Category("BuiltInVars")]
		public void PropsDateTime()
		{
			var now = DateTime.Now;
			var ss = Accessors.A_Sec;
			var min = Accessors.A_Min;
			var hh = Accessors.A_Hour;
			var yyyy = Accessors.A_YYYY;
			var mm = Accessors.A_MM;
			var dd = Accessors.A_DD;
			var mmmm = Accessors.A_MMMM;
			var mmm = Accessors.A_MMM;
			var dddd = Accessors.A_DDDD;
			var ddd = Accessors.A_DDD;
			var wday = Accessors.A_WDay;
			var yday = Accessors.A_YDay;
			var yweek = Accessors.A_YWeek;
			var cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
			var week = cal.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
			Assert.AreEqual(now.Year, yyyy);
			Assert.AreEqual(now.ToString("MM"), mm);
			Assert.AreEqual(now.ToString("dd"), dd);
			Assert.AreEqual(now.ToString("MMMM"), mmmm);
			Assert.AreEqual(now.ToString("MMM"), mmm);
			Assert.AreEqual(now.ToString("dddd"), dddd);
			Assert.AreEqual(now.ToString("ddd"), ddd);
			Assert.AreEqual((long)now.DayOfWeek + 1L, wday);
			Assert.AreEqual(now.DayOfYear, yday);
			Assert.AreEqual($"{now:yyyy}{week:D2}", yweek);
			Assert.AreEqual(now.ToString("HH"), hh);
			Assert.AreEqual(now.ToString("mm"), min);
			Assert.AreEqual(now.ToString("ss"), ss);
			//Don't test Accessors.A_MSec because it'll probably be different between calls.
			var full = Accessors.A_Now;
			var dt = Conversions.FromYYYYMMDDHH24MISS(full);
			var full2 = dt.ToString("yyyyMMddHHmmss");
			Assert.AreEqual(full, full2);
			full = Accessors.A_NowUTC;
			dt = Conversions.FromYYYYMMDDHH24MISS(full);
			full2 = dt.ToString("yyyyMMddHHmmss");
			Assert.AreEqual(full, full2);
			Assert.IsTrue(Accessors.A_TickCount > 0);
			Assert.IsTrue(TestScript("props-date-time", true));
		}

		[Test, Category("BuiltInVars")]
		public void PropsScriptSettings() => Assert.IsTrue(TestScript("props-script-settings", false));

		[Test, Category("BuiltInVars")]
		public void ReflectionsInit() => Reflections.Initialize();

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
