using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using Keysharp.Core;
using static Keysharp.Core.Dialogs;
using NUnit.Framework;
using System.Threading;

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

	}
}
