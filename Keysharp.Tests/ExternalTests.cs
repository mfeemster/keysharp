using Keysharp.Core;
using Keysharp.Core.Windows;
using Keysharp.Scripting;
using NUnit.Framework;
using static Keysharp.Core.DllHelper;
using static Keysharp.Core.Strings;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("External")]
		public void NumPutNumGet() => Assert.IsTrue(TestScript("external-numput-numget", true));

		[Test, Category("External")]
		public void DllCall()
		{
			var desktop = WindowsAPI.GetDesktopWindow();
			var rect = new RECT();
			var buf = Script.Buffer(16, 0);
			_ = DllHelper.DllCall("user32.dll\\GetWindowRect", "ptr", desktop, "ptr", buf);
			_ = WindowsAPI.GetWindowRect(desktop, out rect);
			var l = (long)NumGet(buf, 0, "UInt");
			var t = (long)NumGet(buf, 4, "UInt");
			var r = (long)NumGet(buf, 8, "UInt");
			var b = (long)NumGet(buf, 12, "UInt");
			Assert.IsTrue(r > 0 && b > 0);
			Assert.AreEqual(rect.Left, l);
			Assert.AreEqual(rect.Right, r);
			Assert.AreEqual(rect.Top, t);
			Assert.AreEqual(rect.Bottom, b);
			var str = "lower";
			var len = str.Length;
			var strbuf = Keysharp.Scripting.Script.StringBuffer(str);
			_ = DllHelper.DllCall("user32.dll\\CharUpperBuff", "ptr", strbuf, "UInt", len);
			Assert.AreEqual(strbuf.ToString(), str.ToUpper());
			Assert.IsTrue(TestScript("external-dllcall", true));
		}
	}
}