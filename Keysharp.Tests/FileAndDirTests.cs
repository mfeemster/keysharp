using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using Keysharp.Core;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using static Keysharp.Core.Core;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		private object syncroot = new object();

		[Test, Category("FileAndDir")]
		public void DirCopy()
		{
			if (Directory.Exists("./DirCopy2"))
				Directory.Delete("./DirCopy2", true);

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./DirCopy2");
			Assert.IsTrue(Directory.Exists("./DirCopy2"));
			Assert.IsTrue(File.Exists("./DirCopy2/file1.txt"));
			Assert.IsTrue(File.Exists("./DirCopy2/file2.txt"));
			Assert.IsTrue(File.Exists("./DirCopy2/file3txt"));
			Disk.DirCopy(dir, "./DirCopy2", true);
			Assert.IsTrue(Directory.Exists("./DirCopy2"));
			Assert.IsTrue(File.Exists("./DirCopy2/file1.txt"));
			Assert.IsTrue(File.Exists("./DirCopy2/file2.txt"));
			Assert.IsTrue(File.Exists("./DirCopy2/file3txt"));

			if (Directory.Exists("./DirCopy2"))
				Directory.Delete("./DirCopy2", true);

			Assert.IsTrue(TestScript("file-dircopy", true));
		}

		[Test, Category("FileAndDir")]
		public void DirCreate()
		{
			if (Directory.Exists("./DirCreate"))
				Directory.Delete("./DirCreate", true);

			var dir = "./DirCreate/SubDir1/SubDir2/SubDir3";
			Disk.DirCreate(dir);
			Assert.AreEqual(Core.Core.ErrorLevel, 0);
			Assert.IsTrue(Directory.Exists("./DirCreate"));
			Assert.IsTrue(Directory.Exists("./DirCreate/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirCreate/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirCreate/SubDir1/SubDir2/SubDir3"));
			Assert.IsTrue(TestScript("file-dircreate", true));
		}

		[Test, Category("FileAndDir")]
		public void DirDelete()
		{
			if (Directory.Exists("./DirDelete"))
				Directory.Delete("./DirDelete", true);

			var dir = "./DirDelete/SubDir1/SubDir2/SubDir3";
			Disk.DirCreate(dir);
			Assert.AreEqual(Core.Core.ErrorLevel, 0);
			Assert.IsTrue(Directory.Exists("./DirDelete"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2/SubDir3"));
			Disk.DirDelete("./DirDelete");
			Assert.AreEqual(Core.Core.ErrorLevel, 1);
			Assert.IsTrue(Directory.Exists("./DirDelete"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2/SubDir3"));
			Disk.DirDelete("./DirDelete", true);
			Assert.AreEqual(Core.Core.ErrorLevel, 0);
			Assert.IsTrue(!Directory.Exists("./DirDelete"));
			Assert.IsTrue(!Directory.Exists("./DirDelete/SubDir1"));
			Assert.IsTrue(!Directory.Exists("./DirDelete/SubDir1/SubDir2"));
			Assert.IsTrue(!Directory.Exists("./DirDelete/SubDir1/SubDir2/SubDir3"));
			Assert.IsTrue(TestScript("file-dirdelete", true));
		}

		[Test, Category("FileAndDir")]
		public void DirExist()
		{
			if (Directory.Exists("./DirExist"))
				Directory.Delete("./DirExist", true);

			var dir = "./DirExist/SubDir1/SubDir2/SubDir3";
			Disk.DirCreate(dir);
			Assert.AreEqual(Core.Core.ErrorLevel, 0);
			Assert.IsTrue(Directory.Exists("./DirExist"));
			Assert.IsTrue(Directory.Exists("./DirExist/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirExist/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirExist/SubDir1/SubDir2/SubDir3"));
			var val = Disk.DirExist(dir);
			Assert.AreEqual(val, "D");
			dir = string.Concat(path, "DirCopy/file1.txt");
			Assert.IsTrue(File.Exists(dir));
			val = Disk.DirExist(dir);
			Assert.AreEqual(val, "A");
			dir = string.Concat(path, "DirCopy/file2.txt");
			Assert.IsTrue(File.Exists(dir));
			val = Disk.DirExist(dir);
			Assert.AreEqual(val, "A");
			dir = string.Concat(path, "DirCopy/file3txt");
			Assert.IsTrue(File.Exists(dir));
			val = Disk.DirExist(dir);
			Assert.AreEqual(val, "A");
			Assert.IsTrue(TestScript("file-direxist", true));
		}

		[Test, Category("FileAndDir")]
		public void DirMove()
		{
			if (Directory.Exists("./DirMove"))
				Directory.Delete("./DirMove", true);

			if (Directory.Exists("./DirCopy3"))
				Directory.Delete("./DirCopy3", true);

			if (Directory.Exists("./DirCopy3-rename"))
				Directory.Delete("./DirCopy3-rename", true);

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./DirMove");
			Assert.IsTrue(Directory.Exists("./DirMove"));
			Assert.IsTrue(File.Exists("./DirMove/file1.txt"));
			Assert.IsTrue(File.Exists("./DirMove/file2.txt"));
			Assert.IsTrue(File.Exists("./DirMove/file3txt"));
			Disk.DirMove("./DirMove", "./DirCopy3");
			Assert.IsTrue(Directory.Exists("./DirCopy3"));
			Assert.IsTrue(!Directory.Exists("./DirMove"));
			Assert.IsTrue(!File.Exists("./DirMove/file1.txt"));
			Assert.IsTrue(!File.Exists("./DirMove/file2.txt"));
			Assert.IsTrue(!File.Exists("./DirMove/file3txt"));
			Assert.IsTrue(File.Exists("./DirCopy3/file1.txt"));
			Assert.IsTrue(File.Exists("./DirCopy3/file2.txt"));
			Assert.IsTrue(File.Exists("./DirCopy3/file3txt"));
			Disk.DirMove("./DirCopy3", "./DirCopy3");//Both of these should just return, do nothing, and not throw because ./DirCopy3 exists.
			Disk.DirMove("./DirCopy3", "./DirCopy3", 0);
			Disk.DirCopy(dir, "./DirMove");
			Disk.DirMove("./DirMove", "./DirCopy3", 1);//Will copy into because ./DirCopy3 already exists.
			Assert.IsTrue(Directory.Exists("./DirCopy3/DirMove"));
			Assert.IsTrue(File.Exists("./DirCopy3/DirMove/file1.txt"));
			Assert.IsTrue(File.Exists("./DirCopy3/DirMove/file2.txt"));
			Assert.IsTrue(File.Exists("./DirCopy3/DirMove/file3txt"));
			Disk.DirMove("./DirCopy3", "./DirCopy3-rename", "R");

			if (Directory.Exists("./DirMove"))
				Directory.Delete("./DirMove", true);

			if (Directory.Exists("./DirCopy3"))
				Directory.Delete("./DirCopy3", true);

			if (Directory.Exists("./DirCopy3-rename"))
				Directory.Delete("./DirCopy3-rename", true);

			Assert.IsTrue(TestScript("file-dirmove", true));
		}

		[Test, Category("FileAndDir")]
		public void FileAppend()
		{
			if (File.Exists("./fileappend.txt"))
				File.Delete("./fileappend.txt");

			if (File.Exists("./fileappend2.txt"))
				File.Delete("./fileappend2.txt");

			Disk.FileAppend("test file text", "./fileappend.txt");
			Assert.IsTrue(File.Exists("./fileappend.txt"));
			Disk.FileAppend("test file text", "./fileappend.txt");
			var text = File.ReadAllText("./fileappend.txt");
			Assert.AreEqual("test file texttest file text", text);
			//
			var data = new ArrayList(new byte[] { 1, 2, 3, 4 });
			Disk.FileAppend(data, "./fileappend2.txt", "utf-8-raw");
			Assert.IsTrue(File.Exists("./fileappend2.txt"));
			var data2 = new ArrayList(File.ReadAllBytes("./fileappend2.txt"));
			Assert.AreEqual(data, data2);
			Disk.FileAppend("abcd", "./fileappend2.txt", "utf-16-raw");
			data2 = new ArrayList(File.ReadAllBytes("./fileappend2.txt"));
			data.AddRange(new UnicodeEncoding(false, false).GetBytes("abcd"));
			Assert.AreEqual(data, data2);

			if (File.Exists("./fileappend.txt"))
				File.Delete("./fileappend.txt");

			if (File.Exists("./fileappend2.txt"))
				File.Delete("./fileappend2.txt");

			Assert.IsTrue(TestScript("file-fileappend", true));
		}

		[Test, Category("FileAndDir")]
		public void FileCopy()
		{
			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			var dir = string.Concat(path, "DirCopy");
			Disk.FileCopy($"{dir}/file1.txt", "./FileCopy/file1.txt");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			File.Delete("./FileCopy/file1.txt");
			Assert.IsTrue(!File.Exists("./FileCopy/file1.txt"));
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy/");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy/*.*");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy/*.bak");
			Assert.IsTrue(File.Exists("./FileCopy/file1.bak"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.bak"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3.bak"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy/*");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy/*.");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/file3.txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*.txt", "./FileCopy/NonExistentDir/*");
			Assert.IsTrue(!File.Exists("./FileCopy/NonExistentDir/file1.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/NonExistentDir/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/NonExistentDir/file3.txt"));
			Assert.IsTrue(!File.Exists("./FileCopy/NonExistentDir/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Disk.FileCopy($"{dir}/*", "./FileCopy/*");
			Assert.IsTrue(File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(File.Exists("./FileCopy/file3txt"));
			Assert.IsTrue(TestScript("file-filecopy", true));
		}

		[Test, Category("FileAndDir")]
		public void FileCreateShortcut()
		{
			if (Directory.Exists("./FileCreateShortcut"))
				Directory.Delete("./FileCreateShortcut", true);

			if (File.Exists("./testshortcut.lnk"))
				File.Delete("./testshortcut.lnk");

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./FileCreateShortcut/");
			Disk.FileCreateShortcut
			(
				"./FileCreateShortcut/file1.txt",
				"./testshortcut.lnk",
				"",
				"",
				"TestDescription",
				"../../../Keysharp.ico",
				""
			);
			Assert.IsTrue(File.Exists("./testshortcut.lnk"));

			if (Directory.Exists("./FileCreateShortcut"))
				Directory.Delete("./FileCreateShortcut", true);

			if (File.Exists("./testshortcut.lnk"))
				File.Delete("./testshortcut.lnk");

			Assert.IsTrue(TestScript("file-filecreateshortcut", true));
		}

		[Test, Category("FileAndDir")]
		public void FileDelete()
		{
			if (Directory.Exists("./FileDelete"))
				Directory.Delete("./FileDelete", true);

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./FileDelete/");
			Disk.FileDelete("./FileDelete/*.txt");
			Assert.IsTrue(Directory.Exists("./FileDelete"));
			Assert.IsTrue(!File.Exists("./FileDelete/file1.txt"));
			Assert.IsTrue(!File.Exists("./FileDelete/file2.txt"));
			Assert.IsTrue(File.Exists("./FileDelete/file3txt"));
			Disk.FileDelete("./FileDelete/*");
			Assert.IsTrue(!File.Exists("./FileDelete/file3txt"));

			if (Directory.Exists("./FileDelete"))
				Directory.Delete("./FileDelete", true);

			Assert.IsTrue(TestScript("file-filedelete", true));
		}

		[Test, Category("FileAndDir")]
		public void FileEncoding()
		{
			Window.FileEncoding("utf-8");
			var fe = A_FileEncoding;
			Assert.AreEqual(fe, Encoding.UTF8.BodyName);
			Window.FileEncoding("utf-8-raw");
			fe = A_FileEncoding;
			Assert.AreEqual(fe, "utf-8-raw");
			Window.FileEncoding("utf-16");
			fe = A_FileEncoding;
			Assert.AreEqual(fe, "utf-16");
			Assert.AreEqual(fe, Encoding.Unicode.BodyName);
			Window.FileEncoding("unicode");
			fe = A_FileEncoding;
			Assert.AreEqual(fe, "utf-16");
			Window.FileEncoding("utf-16-raw");
			fe = A_FileEncoding;
			Assert.AreEqual(fe, "utf-16-raw");
			Window.FileEncoding("ascii");
			fe = A_FileEncoding;
			Assert.AreEqual(fe, "us-ascii");
			Assert.AreEqual(fe, Encoding.ASCII.BodyName);
			Window.FileEncoding("us-ascii");
			fe = A_FileEncoding;
			Assert.AreEqual(fe, "us-ascii");
			Assert.IsTrue(TestScript("file-fileencoding", true));
		}

		[Test, Category("FileAndDir")]
		public void FileExist()
		{
			var dir = string.Concat(path, "DirCopy");
			dir += "/*.txt";
			var attr = Disk.FileExist(dir);
			Assert.AreEqual("A", attr);
			Assert.IsTrue(TestScript("file-fileexist", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetAttrib()
		{
			var dir = string.Concat(path, "DirCopy");
			var attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("D", attr);
			dir = string.Concat(path, "DirCopy/file1.txt");
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			dir = string.Concat(path, "DirCopy/file2.txt");
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			dir = string.Concat(path, "DirCopy/file3txt");
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			Assert.IsTrue(TestScript("file-filegetattrib", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetShortcut()
		{
			if (Directory.Exists("./FileGetShortcut"))
				Directory.Delete("./FileGetShortcut", true);

			if (File.Exists("./testshortcut.lnk"))
				File.Delete("./testshortcut.lnk");

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./FileGetShortcut/");
			var patharg = Path.GetDirectoryName(Path.GetFullPath("./FileGetShortcut/file1.txt"));
			Disk.FileCreateShortcut
			(
				"./FileGetShortcut/file1.txt",
				"./testshortcut.lnk",
				patharg,
				"",
				"TestDescription",
				"../../../Keysharp.ico",
				""
			);
			var shortcut = Disk.FileGetShortcut("./testshortcut.lnk");
			Assert.AreEqual(Path.GetFullPath("./FileGetShortcut/file1.txt"), shortcut.OutTarget.ToString());
			Assert.AreEqual(patharg, shortcut.OutDir.ToString());
			Assert.AreEqual("TestDescription", shortcut.OutDescription.ToString());
			Assert.AreEqual("", shortcut.OutArgs.ToString());
			Assert.AreEqual(Path.GetFullPath("../../../Keysharp.ico"), shortcut.OutIcon.ToString());
			Assert.AreEqual("0", shortcut.OutIconNum.ToString());
			Assert.AreEqual("1", shortcut.OutRunState.ToString());

			if (File.Exists("./testshortcut.lnk"))
				File.Delete("./testshortcut.lnk");

			Assert.IsTrue(TestScript("file-filegetshortcut", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetSize()
		{
			var dir = string.Concat(path, "DirCopy/file1.txt");
			var size = Disk.FileGetSize(dir);
			Assert.AreEqual(14, size);
			size = Disk.FileGetSize(dir, "k");
			Assert.AreEqual(0, size);
			size = Disk.FileGetSize(dir, "m");
			Assert.AreEqual(0, size);
			size = Disk.FileGetSize(dir, "g");
			Assert.AreEqual(0, size);
			size = Disk.FileGetSize(dir, "t");
			Assert.AreEqual(0, size);
			Assert.IsTrue(TestScript("file-filegetsize", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetTime()
		{
			var dir = string.Concat(path, "DirCopy/file1.txt");
			var filetime = Disk.FileGetTime(dir);
			var dt = Keysharp.Core.Conversions.FromYYYYMMDDHH24MISS(filetime);
			Assert.AreNotEqual(dt, DateTime.MinValue);
			Assert.IsTrue(TestScript("file-filegettime", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetVersion()
		{
			var file = "./Keysharp.Core.dll";
			var fileversion = Disk.FileGetVersion(file);
			var expected = FileVersionInfo.GetVersionInfo(file).FileVersion;
			Assert.AreEqual(fileversion, expected);
			Assert.IsTrue(TestScript("file-filegetversion", true));
		}

		[Test, Category("FileAndDir")]
		public void FileInstall()
		{
		}

		[Test, Category("FileAndDir")]
		public void FileMove()
		{
			if (Directory.Exists("./FileMove"))
				Directory.Delete("./FileMove", true);

			_ = Directory.CreateDirectory("./FileMove");
			var dir = string.Concat(path, "DirCopy");
			Disk.FileCopy($"{dir}/*", "./FileMove/");
			Assert.IsTrue(File.Exists("./FileMove/file1.txt"));
			Assert.IsTrue(File.Exists("./FileMove/file2.txt"));
			Assert.IsTrue(File.Exists("./FileMove/file3txt"));

			if (Directory.Exists("./FileMove2"))
				Directory.Delete("./FileMove2", true);

			_ = Directory.CreateDirectory("./FileMove2");
			Assert.IsTrue(Directory.Exists("./FileMove2"));
			Disk.FileMove($"./FileMove/*", "./FileMove2");
			Assert.IsTrue(!File.Exists("./FileMove/file1.txt"));
			Assert.IsTrue(!File.Exists("./FileMove/file2.txt"));
			Assert.IsTrue(!File.Exists("./FileMove/file3txt"));
			Assert.IsTrue(File.Exists("./FileMove2/file1.txt"));
			Assert.IsTrue(File.Exists("./FileMove2/file2.txt"));
			Assert.IsTrue(File.Exists("./FileMove2/file3txt"));
			Directory.Delete("./FileMove", true);
			Directory.Delete("./FileMove2", true);
			Assert.IsTrue(TestScript("file-filemove", true));
		}

		[Test, Category("FileAndDir")]
		public void FileOpen()
		{
		}

		[Test, Category("FileAndDir")]
		public void FileRead()
		{
			var dir = string.Concat(path, "DirCopy/file1.txt");
			var text = Disk.FileRead(dir);
			Assert.AreEqual("this is file 1", text);
			text = Disk.FileRead(dir, "m4");
			Assert.AreEqual("this", text);
			text = Disk.FileRead(dir, "m4 utf-8");
			Assert.AreEqual("this", text);
			var buf = Disk.FileRead(dir, "m4 raw");
			var buf2 = new byte[] { (int)'t', (int)'h', (int)'i', (int)'s' };
			Assert.AreEqual(buf2, buf);
			Assert.IsTrue(TestScript("file-fileread", true));
		}

		[Test, Category("FileAndDir")]
		public void FileRecycle()
		{
			lock (syncroot)
			{
				if (Directory.Exists("./FileRecycle"))
					Directory.Delete("./FileRecycle", true);

				_ = Directory.CreateDirectory("./FileRecycle");
				var dir = string.Concat(path, "DirCopy");
				Disk.FileCopy($"{dir}/*", "./FileRecycle/");
				Assert.IsTrue(File.Exists("./FileRecycle/file1.txt"));
				Assert.IsTrue(File.Exists("./FileRecycle/file2.txt"));
				Assert.IsTrue(File.Exists("./FileRecycle/file3txt"));
				Disk.FileRecycle("./FileRecycle/file1.txt");
				Assert.IsTrue(!File.Exists("./FileRecycle/file1.txt"));
				Assert.IsTrue(File.Exists("./FileRecycle/file2.txt"));
				Assert.IsTrue(File.Exists("./FileRecycle/file3txt"));
				Disk.FileRecycle("./FileRecycle/*.txt");
				Assert.IsTrue(!File.Exists("./FileRecycle/file2.txt"));
				Assert.IsTrue(File.Exists("./FileRecycle/file3txt"));
				Disk.FileRecycle("./FileRecycle/*");
				Assert.IsTrue(!File.Exists("./FileRecycle/file3txt"));
				Directory.Delete("./FileRecycle", true);
				Assert.IsTrue(TestScript("file-filerecycle", true));
			}
		}

		[Test, Category("FileAndDir")]
		public void FileRecycleEmpty()
		{
			lock (syncroot)
			{
				if (Directory.Exists("./FileRecycleEmpty"))
					Directory.Delete("./FileRecycleEmpty", true);

				_ = Directory.CreateDirectory("./FileRecycleEmpty");
				var dir = string.Concat(path, "DirCopy");
				Disk.FileCopy($"{dir}/*", "./FileRecycleEmpty/");
				Assert.IsTrue(File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(File.Exists("./FileRecycleEmpty/file3txt"));
				Disk.FileRecycle("./FileRecycleEmpty/*");
				Assert.IsTrue(!File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(!File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(!File.Exists("./FileRecycleEmpty/file3txt"));
				Disk.FileRecycleEmpty();

				if (Directory.Exists("./FileRecycleEmpty"))
					Directory.Delete("./FileRecycleEmpty", true);

				_ = Directory.CreateDirectory("./FileRecycleEmpty");
				Disk.FileCopy($"{dir}/*", "./FileRecycleEmpty/");
				Assert.IsTrue(File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(File.Exists("./FileRecycleEmpty/file3txt"));
				Disk.FileRecycle("./FileRecycleEmpty/*");
				Assert.IsTrue(!File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(!File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(!File.Exists("./FileRecycleEmpty/file3txt"));
				Disk.FileRecycleEmpty("C:\\");
				Assert.IsTrue(TestScript("file-filerecycleempty", true));
			}
		}

		[Test, Category("FileAndDir")]
		public void FileSetAttrib()
		{
			if (Directory.Exists("./FileSetAttrib"))
				Directory.Delete("./FileSetAttrib", true);

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./FileSetAttrib");
			Assert.IsTrue(Directory.Exists("./FileSetAttrib"));
			Assert.IsTrue(File.Exists("./FileSetAttrib/file1.txt"));
			Assert.IsTrue(File.Exists("./FileSetAttrib/file2.txt"));
			Assert.IsTrue(File.Exists("./FileSetAttrib/file3txt"));
			dir = "./FileSetAttrib";
			var attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("D", attr);
			dir = "./FileSetAttrib/file1.txt";
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			Disk.FileSetAttrib('r', dir);
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("RA", attr);
			Disk.FileSetAttrib("-r", dir);
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			Disk.FileSetAttrib("^r", dir);
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("RA", attr);
			Disk.FileSetAttrib("^r", dir);
			attr = Disk.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);

			if (Directory.Exists("./FileSetAttrib"))
				Directory.Delete("./FileSetAttrib", true);

			Assert.IsTrue(TestScript("file-filesetattrib", true));
		}

		[Test, Category("FileAndDir")]
		public void FileSetTime()
		{
			if (Directory.Exists("./FileSetTime"))
				Directory.Delete("./FileSetTime", true);

			var dir = string.Concat(path, "DirCopy");
			Disk.DirCopy(dir, "./FileSetTime");
			Assert.IsTrue(Directory.Exists("./FileSetTime"));
			Assert.IsTrue(File.Exists("./FileSetTime/file1.txt"));
			Assert.IsTrue(File.Exists("./FileSetTime/file2.txt"));
			Assert.IsTrue(File.Exists("./FileSetTime/file3txt"));
			Disk.FileSetTime("20200101131415", "./FileSetTime/file1.txt", 'm');
			var filetime = Disk.FileGetTime("./FileSetTime/file1.txt", 'm');
			Assert.AreEqual("20200101131415", filetime);
			Disk.FileSetTime("20200101131416", "./FileSetTime/file1.txt", 'c');
			filetime = Disk.FileGetTime("./FileSetTime/file1.txt", 'c');
			Assert.AreEqual("20200101131416", filetime);
			Disk.FileSetTime("20200101131417", "./FileSetTime/file1.txt", 'a');
			filetime = Disk.FileGetTime("./FileSetTime/file1.txt", 'a');
			Assert.AreEqual("20200101131417", filetime);

			if (Directory.Exists("./FileSetTime"))
				Directory.Delete("./FileSetTime", true);

			Assert.IsTrue(TestScript("file-filesettime", true));
		}

		[Test, Category("FileAndDir")]
		public void IniReadWriteDelete()
		{
			if (File.Exists("./testini2.ini"))
				File.Delete("./testini2.ini");

			var dir = string.Concat(path, "/testini.ini");
			Disk.FileCopy(dir, "./testini2.ini", true);
			Assert.IsTrue(File.Exists("./testini2.ini"));
			var val = Ini.IniRead("./testini2.ini", "sectionone", "keyval");
			Assert.AreEqual("theval", val);
			val = Ini.IniRead("./testini2.ini", "sectiontwo");
			Assert.AreEqual("groupkey1=groupval1\r\ngroupkey2=groupval2\r\ngroupkey3=groupval3\r\n", val);
			val = Ini.IniRead("./testini2.ini");
			Assert.AreEqual("sectionone\r\nsectiontwo\r\nsectionthree\r\n", val);
			Ini.IniWrite("thevalnew", "./testini2.ini", "sectionone", "keyval");
			val = Ini.IniRead("./testini2.ini", "sectionone", "keyval");
			Assert.AreEqual("thevalnew", val);
			var str = @"groupkey11=groupval11
groupkey12=groupval12
groupkey13=groupval13
"
					  ;
			Ini.IniWrite(str, "./testini2.ini", "sectiontwo");
			val = Ini.IniRead("./testini2.ini", "sectiontwo");
			Assert.AreEqual("groupkey11=groupval11\r\ngroupkey12=groupval12\r\ngroupkey13=groupval13\r\n", val);
			Ini.IniDelete("./testini2.ini", "sectiontwo", "groupkey11");
			val = Ini.IniRead("./testini2.ini", "sectiontwo");
			Assert.AreEqual("groupkey12=groupval12\r\ngroupkey13=groupval13\r\n", val);
			Ini.IniDelete("./testini2.ini", "sectiontwo");
			val = Ini.IniRead("./testini2.ini", "sectiontwo");
			Assert.AreEqual("", val);

			if (File.Exists("./testini2.ini"))
				File.Delete("./testini2.ini");

			Assert.IsTrue(TestScript("file-inireadwritedelete", true));
		}

		[Test, Category("FileAndDir")]
		public void SetWorkingDir()
		{
			var origdir = Accessors.A_WorkingDir;
			var fullpath = Path.GetFullPath(string.Concat(path, "DirCopy"));
			Disk.SetWorkingDir(fullpath);
			Assert.AreEqual(fullpath, Accessors.A_WorkingDir);
			Disk.SetWorkingDir("C:\\a\\fake\\path");//Non-existent folders don't get assigned.
			Assert.AreEqual(fullpath, Accessors.A_WorkingDir);//So it should remain unchanged.
			Disk.SetWorkingDir(origdir);
			Assert.IsTrue(TestScript("file-filesetworkingdir", true));
		}

		[Test, Category("FileAndDir")]
		public void SplitPath()
		{
			var fullpath = string.Concat(path, "DirCopy/file1.txt");
			var splitpath = Disk.SplitPath(fullpath);
			var filename = splitpath.OutFileName;
			var dir = splitpath.OutDir;
			var ext = splitpath.OutExtension;
			var namenoext = splitpath.OutNameNoExt;
			var drive = splitpath.OutDrive;
			Assert.AreEqual("file1.txt", filename);
			Assert.AreEqual("C:\\D\\Dev\\Keysharp\\Keysharp.Tests\\Code\\DirCopy", dir);//This will be different on non-windows.//MATT
			Assert.AreEqual("txt", ext);
			Assert.AreEqual("file1", namenoext);
			Assert.AreEqual("C:\\", drive);
			Assert.IsTrue(TestScript("file-filesplitpath", true));
		}
	}
}