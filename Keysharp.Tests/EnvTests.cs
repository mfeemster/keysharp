using System;
using System.Collections;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core;
using NUnit.Framework;
using static Keysharp.Core.Core;
using static Keysharp.Core.Env;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Env")]
		[Apartment(ApartmentState.STA)]
		public void ClipboardAll()
		{
			Accessors.A_Clipboard = "Asdf";
			var arr = Env.ClipboardAll();
			Accessors.A_Clipboard = "";
			Accessors.A_Clipboard = arr;
			var clip = Accessors.A_Clipboard as string;
			Assert.AreEqual("Asdf", clip);
		}

		[Test, Category("Env")]
		[Apartment(ApartmentState.STA)]
		public void ClipWait()
		{
			Clipboard.Clear();
			var dt = DateTime.Now;
			var b = Env.ClipWait(0.5);
			var dt2 = DateTime.Now;
			var ms = (dt2 - dt).TotalMilliseconds;
			Assert.AreEqual(false, b);//Will have timed out, so ErrorLevel will be 1.
			Assert.IsTrue(ms >= 500 && ms <= 3000);
			var tcs = new TaskCompletionSource<bool>();
			var thread = new Thread(() =>
			{
				Thread.Sleep(100);
				Clipboard.SetText("test text");
				tcs.SetResult(true);
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			dt = DateTime.Now;
			b = Env.ClipWait(null, true);//Will wait indefinitely for any type.
			dt2 = DateTime.Now;
			ms = (dt2 - dt).TotalMilliseconds;
			//Seems to take much longer than 100ms, but it's not too important.
			//Assert.IsTrue(ms >= 500 && ms <= 1100);
			tcs.Task.Wait();
			Assert.AreEqual(true, b);//Will have detected clipboard data, so ErrorLevel will be 0.
			//Now test with file paths.
			Clipboard.Clear();
			tcs = new TaskCompletionSource<bool>();
			thread = new Thread(() =>
			{
				Thread.Sleep(100);
				Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection
				{
					"./testfile1.txt",
					"./testfile2.txt",
					"./testfile3.txt",
				});
				tcs.SetResult(true);
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			dt = DateTime.Now;
			b = Env.ClipWait();//Will wait indefinitely for only text or file paths.
			dt2 = DateTime.Now;
			ms = (dt2 - dt).TotalMilliseconds;
			tcs.Task.Wait();
			Assert.AreEqual(true, b);//Will have detected clipboard data, so ErrorLevel will be 0.
			//Wait specifically for text/files, and copy an image. This should time out, and ErrorLevel should be set to 1.
			Clipboard.Clear();
			var bitmap = new Bitmap(640, 480);
			tcs = new TaskCompletionSource<bool>();
			thread = new Thread(() =>
			{
				Thread.Sleep(100);
				Clipboard.SetImage(bitmap);
				tcs.SetResult(true);
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			dt = DateTime.Now;
			b = Env.ClipWait(1);//Will wait for one second for only text or file paths.
			dt2 = DateTime.Now;
			ms = (dt2 - dt).TotalMilliseconds;
			tcs.Task.Wait();
			Assert.AreEqual(false, b);//Will have timed out, so ErrorLevel will be 1.
			Assert.IsTrue(TestScript("clipwait", true));//For this to work, the bitmap from above must be on the clipboard.
		}

		[Test, Category("Env")]
		public void EnvGet()
		{
			var path = Env.EnvGet("PATH");
			Assert.AreNotEqual(path, string.Empty);
			path = Env.EnvGet("dummynothing123");
			Assert.AreEqual(path, string.Empty);
			Assert.IsTrue(TestScript("envget", true));
		}

		[Test, Category("Env")]
		public void EnvSet()
		{
			var key = "dummynothing123";
			var s = "a test value";
			Env.EnvSet(key, s);//Add the variable.
			var val = Env.EnvGet(key);
			Assert.AreEqual(val, s);
			Env.EnvSet(key, null);//Delete the variable.
			val = Env.EnvGet(key);//Ensure it's deleted.
			Assert.AreEqual(val, string.Empty);
			Assert.IsTrue(TestScript("envset", true));
		}

		[Test, Category("Env")]
		public void EnvUpdate()
		{
			Env.EnvUpdate();
			Assert.IsTrue(TestScript("envupdate", true));
		}
	}
}
