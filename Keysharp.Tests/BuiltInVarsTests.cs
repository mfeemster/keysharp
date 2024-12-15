using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public class BuiltInVarTests : TestRunner
	{
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
			var dt = Conversions.ToDateTime(full);
			var full2 = dt.ToString("yyyyMMddHHmmss");
			Assert.AreEqual(full, full2);
			full = Accessors.A_NowUTC;
			dt = Conversions.ToDateTime(full);
			full2 = dt.ToString("yyyyMMddHHmmss");
			Assert.AreEqual(full, full2);
			Assert.IsTrue(Accessors.A_TickCount > 0);
			Assert.IsTrue(TestScript("props-date-time", true));
		}

		[Test, Category("BuiltInVars"), NonParallelizable]
		public void PropsScriptProperties() => Assert.IsTrue(TestScript("props-script-properties", false));

		[Test, Category("BuiltInVars"), NonParallelizable]
		public void PropsScriptSettings() => Assert.IsTrue(TestScript("props-script-settings", false));

		[Test, Category("BuiltInVars")]
		public void PropsSpecialChars() => Assert.IsTrue(TestScript("props-special-chars", true));

		[Test, Category("BuiltInVars")]
		public void ReflectionsInit() => Reflections.Initialize();
	}
}
