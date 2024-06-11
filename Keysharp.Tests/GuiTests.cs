using System;
using System.IO;
using Keysharp.Core;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using Keysharp.Core.Windows;
using System.Windows.Forms;
using Keysharp.Core.Common.Threading;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Gui")]
		[Apartment(ApartmentState.STA)]
		public void FileSelect()
		{
			var fullpath = Path.GetFullPath(string.Concat(path, "DirCopy/file1.txt"));
			var files = Dialogs.FileSelect();
			//MsgBox(files);
			files = Dialogs.FileSelect("", fullpath, "Filename - Path and File", "");
			fullpath = Path.GetFullPath(string.Concat(path, "DirCopy/"));
			files = Dialogs.FileSelect("M", fullpath, "Filename - Path only - Multiselect", "");
			//MsgBox(files);
			fullpath = Path.GetFullPath(string.Concat(path, "DirCopy/file1.txt"));
			files = Dialogs.FileSelect("S16", fullpath, "", "Text files |*.txt;*.wri;*.ini");
			fullpath = Path.GetFullPath(string.Concat(path, "DirCopy/"));
			files = Dialogs.FileSelect("S16", fullpath, "", "Text files |*.txt;*.wri;*.ini");
			//MsgBox(files);
			files = Dialogs.FileSelect("D", "C:\\D\\", "", "");
			files = Dialogs.FileSelect("D", "C:\\D", "", "");
		}

		[Test, Category("Gui")]
		public void DirSelect()
		{
		}

		[Test, Category("Gui")]
		public void MsgBox()
		{
			var run = true;
			var title = "this is a sample title";
			var task = Task.Factory.StartNew(() =>
			{
				while (run)
				{
					var wnd = WindowsAPI.FindWindow(null, title);

					if (wnd != IntPtr.Zero && wnd.ToInt64() != 0)
					{
						_ = WindowsAPI.SetForegroundWindow(wnd);
						SendKeys.SendWait(" ");
						Thread.Sleep(100);
					}
				}
			});
			var form = new Form
			{
				Size = new System.Drawing.Size(500, 500),
				StartPosition = FormStartPosition.CenterScreen
			};
			form.Shown += Form_Shown;
			Application.Run(form);
			run = false;
			task.Wait();
			var ret = Keysharp.Core.Dialogs.MsgBox("ok, hand, def: 1, timeout: 5", "this is a sample title", "0 16 t5");
			Assert.AreEqual(ret, "Timeout");
		}

		private void Form_Shown(object sender, EventArgs e)
		{
			var ret = Keysharp.Core.Dialogs.MsgBox("ok, hand, def: 1", "this is a sample title", "0 16");
			Assert.AreEqual(ret, "OK");
			ret = Keysharp.Core.Dialogs.MsgBox("ok, hand, def: 1", "this is a sample title", 16);
			Assert.AreEqual(ret, "OK");
			ret = Keysharp.Core.Dialogs.MsgBox("ok-cancel, question, def: 2", "this is a sample title", "1 32 256");
			Assert.AreEqual(ret, "Cancel");
			ret = Keysharp.Core.Dialogs.MsgBox("ok-cancel, question, def: 2", "this is a sample title", 1 | 32 | 256);
			Assert.AreEqual(ret, "Cancel");
			ret = Keysharp.Core.Dialogs.MsgBox("abort-retry-ignore, exclamation, def: 3, just: right", "this is a sample title", "2 48 512 524288");
			Assert.AreEqual(ret, "Ignore");
			ret = Keysharp.Core.Dialogs.MsgBox("abort-retry-ignore, exclamation, def: 3, just: right", "this is a sample title", 2 | 48 | 512 | 524288);
			Assert.AreEqual(ret, "Ignore");
			ret = Keysharp.Core.Dialogs.MsgBox("yes-no-cancel, asterisk/info, def: 1", "this is a sample title", "3 64");
			Assert.AreEqual(ret, "Yes");
			ret = Keysharp.Core.Dialogs.MsgBox("yes-no-cancel, asterisk/info, def: 1", "this is a sample title", 3 | 64);
			Assert.AreEqual(ret, "Yes");
			ret = Keysharp.Core.Dialogs.MsgBox("yes-no, asterisk/info, def: 2", "this is a sample title", "4 64 256");
			Assert.AreEqual(ret, "No");
			ret = Keysharp.Core.Dialogs.MsgBox("yes-no, asterisk/info, def: 2", "this is a sample title", 4 | 64 | 256);
			Assert.AreEqual(ret, "No");
			ret = Keysharp.Core.Dialogs.MsgBox("retry-cancel, asterisk/info, def: 1", "this is a sample title", "5 64");
			Assert.AreEqual(ret, "Retry");
			ret = Keysharp.Core.Dialogs.MsgBox("retry-cancel, asterisk/info, def: 1", "this is a sample title", 5 | 64);
			Assert.AreEqual(ret, "Retry");
			(sender as Form)?.Close();
		}
	}
}
