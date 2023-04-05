using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using Keysharp.Core;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using static Keysharp.Core.Core;
using Array = Keysharp.Core.Array;
using Buffer = Keysharp.Core.Buffer;

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
			Dir.DirCopy(dir, "./DirCopy2");
			Assert.IsTrue(Directory.Exists("./DirCopy2"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy2/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy2/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy2/file3txt"));
			Dir.DirCopy(dir, "./DirCopy2", true);
			Assert.IsTrue(Directory.Exists("./DirCopy2"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy2/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy2/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy2/file3txt"));

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
			Dir.DirCreate(dir);
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
			Dir.DirCreate(dir);
			Assert.IsTrue(Directory.Exists("./DirDelete"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2/SubDir3"));

			try
			{
				Dir.DirDelete("./DirDelete");
			}
			catch
			{
			}

			Assert.IsTrue(Directory.Exists("./DirDelete"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirDelete/SubDir1/SubDir2/SubDir3"));
			Dir.DirDelete("./DirDelete", true);
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
			Dir.DirCreate(dir);
			Assert.IsTrue(Directory.Exists("./DirExist"));
			Assert.IsTrue(Directory.Exists("./DirExist/SubDir1"));
			Assert.IsTrue(Directory.Exists("./DirExist/SubDir1/SubDir2"));
			Assert.IsTrue(Directory.Exists("./DirExist/SubDir1/SubDir2/SubDir3"));
			var val = Dir.DirExist(dir);
			Assert.AreEqual(val, "D");
			dir = string.Concat(path, "DirCopy/file1.txt");
			Assert.IsTrue(System.IO.File.Exists(dir));
			val = Dir.DirExist(dir);
			Assert.AreEqual(val, "A");
			dir = string.Concat(path, "DirCopy/file2.txt");
			Assert.IsTrue(System.IO.File.Exists(dir));
			val = Dir.DirExist(dir);
			Assert.AreEqual(val, "A");
			dir = string.Concat(path, "DirCopy/file3txt");
			Assert.IsTrue(System.IO.File.Exists(dir));
			val = Dir.DirExist(dir);
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
			Dir.DirCopy(dir, "./DirMove");
			Assert.IsTrue(Directory.Exists("./DirMove"));
			Assert.IsTrue(System.IO.File.Exists("./DirMove/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirMove/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirMove/file3txt"));
			Dir.DirMove("./DirMove", "./DirCopy3");
			Assert.IsTrue(Directory.Exists("./DirCopy3"));
			Assert.IsTrue(!Directory.Exists("./DirMove"));
			Assert.IsTrue(!System.IO.File.Exists("./DirMove/file1.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./DirMove/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./DirMove/file3txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy3/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy3/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy3/file3txt"));
			Dir.DirMove("./DirCopy3", "./DirCopy3");//Both of these should just return, do nothing, and not throw because ./DirCopy3 exists.
			Dir.DirMove("./DirCopy3", "./DirCopy3", 0);
			Dir.DirCopy(dir, "./DirMove");
			Dir.DirMove("./DirMove", "./DirCopy3", 1);//Will copy into because ./DirCopy3 already exists.
			Assert.IsTrue(Directory.Exists("./DirCopy3/DirMove"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy3/DirMove/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy3/DirMove/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./DirCopy3/DirMove/file3txt"));
			Dir.DirMove("./DirCopy3", "./DirCopy3-rename", "R");

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
			if (System.IO.File.Exists("./fileappend.txt"))
				System.IO.File.Delete("./fileappend.txt");

			if (System.IO.File.Exists("./fileappend2.txt"))
				System.IO.File.Delete("./fileappend2.txt");

			Keysharp.Core.File.FileAppend("test file text", "./fileappend.txt");
			Assert.IsTrue(System.IO.File.Exists("./fileappend.txt"));
			Keysharp.Core.File.FileAppend("test file text", "./fileappend.txt");
			var text = System.IO.File.ReadAllText("./fileappend.txt");
			Assert.AreEqual("test file texttest file text", text);
			//
			var data = new Core.Array(new byte[] { 1, 2, 3, 4 });
			Keysharp.Core.File.FileAppend(data, "./fileappend2.txt", "utf-8-raw");
			Assert.IsTrue(System.IO.File.Exists("./fileappend2.txt"));
			var data2 = new Core.Array(System.IO.File.ReadAllBytes("./fileappend2.txt"));
			Assert.AreEqual(data, data2);
			Keysharp.Core.File.FileAppend("abcd", "./fileappend2.txt", "utf-16-raw");
			data2 = new Core.Array(System.IO.File.ReadAllBytes("./fileappend2.txt"));
			data.AddRange(new UnicodeEncoding(false, false).GetBytes("abcd"));
			Assert.AreEqual(data, data2);

			if (System.IO.File.Exists("./fileappend.txt"))
				System.IO.File.Delete("./fileappend.txt");

			if (System.IO.File.Exists("./fileappend2.txt"))
				System.IO.File.Delete("./fileappend2.txt");

			Assert.IsTrue(TestScript("file-fileappend", true));
		}

		[Test, Category("FileAndDir")]
		public void FileCopy()
		{
			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			var dir = string.Concat(path, "DirCopy");
			Keysharp.Core.File.FileCopy($"{dir}/file1.txt", "./FileCopy/file1.txt");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			System.IO.File.Delete("./FileCopy/file1.txt");
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file1.txt"));
			Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy/");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy/*.*");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy/*.bak");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.bak"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.bak"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3.bak"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy/*");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy/*.");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/file3.txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");

			try
			{
				Keysharp.Core.File.FileCopy($"{dir}/*.txt", "./FileCopy/NonExistentDir/*");
			}
			catch
			{
			}

			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/NonExistentDir/file1.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/NonExistentDir/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/NonExistentDir/file3.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileCopy/NonExistentDir/file3txt"));

			if (Directory.Exists("./FileCopy"))
				Directory.Delete("./FileCopy", true);

			_ = Directory.CreateDirectory("./FileCopy");
			Keysharp.Core.File.FileCopy($"{dir}/*", "./FileCopy/*");
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileCopy/file3txt"));
			Assert.IsTrue(TestScript("file-filecopy", true));
		}

		[Test, Category("FileAndDir")]
		public void FileCreateShortcut()
		{
			if (Directory.Exists("./FileCreateShortcut"))
				Directory.Delete("./FileCreateShortcut", true);

			if (System.IO.File.Exists("./testshortcut.lnk"))
				System.IO.File.Delete("./testshortcut.lnk");

			var dir = string.Concat(path, "DirCopy");
			Dir.DirCopy(dir, "./FileCreateShortcut/");
			Keysharp.Core.File.FileCreateShortcut
			(
				"./FileCreateShortcut/file1.txt",
				"./testshortcut.lnk",
				"",
				"",
				"TestDescription",
				"../../../Keysharp.ico",
				""
			);
			Assert.IsTrue(System.IO.File.Exists("./testshortcut.lnk"));

			if (Directory.Exists("./FileCreateShortcut"))
				Directory.Delete("./FileCreateShortcut", true);

			if (System.IO.File.Exists("./testshortcut.lnk"))
				System.IO.File.Delete("./testshortcut.lnk");

			Assert.IsTrue(TestScript("file-filecreateshortcut", true));
		}

		[Test, Category("FileAndDir")]
		public void FileDelete()
		{
			if (Directory.Exists("./FileDelete"))
				Directory.Delete("./FileDelete", true);

			var dir = string.Concat(path, "DirCopy");
			Dir.DirCopy(dir, "./FileDelete/");
			Keysharp.Core.File.FileDelete("./FileDelete/*.txt");
			Assert.IsTrue(Directory.Exists("./FileDelete"));
			Assert.IsTrue(!System.IO.File.Exists("./FileDelete/file1.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileDelete/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileDelete/file3txt"));
			Keysharp.Core.File.FileDelete("./FileDelete/*");
			Assert.IsTrue(!System.IO.File.Exists("./FileDelete/file3txt"));

			if (Directory.Exists("./FileDelete"))
				Directory.Delete("./FileDelete", true);

			Assert.IsTrue(TestScript("file-filedelete", true));
		}

		[Test, Category("FileAndDir")]
		public void FileEncoding()
		{
			Keysharp.Core.File.FileEncoding("utf-8");
			var fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, Encoding.UTF8.BodyName);
			Keysharp.Core.File.FileEncoding("utf-8-raw");
			fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, "utf-8-raw");
			Keysharp.Core.File.FileEncoding("utf-16");
			fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, "utf-16");
			Assert.AreEqual(fe, Encoding.Unicode.BodyName);
			Keysharp.Core.File.FileEncoding("unicode");
			fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, "utf-16");
			Keysharp.Core.File.FileEncoding("utf-16-raw");
			fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, "utf-16-raw");
			Keysharp.Core.File.FileEncoding("ascii");
			fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, "us-ascii");
			Assert.AreEqual(fe, Encoding.ASCII.BodyName);
			Keysharp.Core.File.FileEncoding("us-ascii");
			fe = Accessors.A_FileEncoding;
			Assert.AreEqual(fe, "us-ascii");
			Assert.IsTrue(TestScript("file-fileencoding", true));
		}

		[Test, Category("FileAndDir")]
		public void FileExist()
		{
			var dir = string.Concat(path, "DirCopy");
			dir += "/*.txt";
			var attr = Keysharp.Core.File.FileExist(dir);
			Assert.AreEqual("A", attr);
			Assert.IsTrue(TestScript("file-fileexist", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetAttrib()
		{
			var dir = string.Concat(path, "DirCopy");
			var attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("D", attr);
			dir = string.Concat(path, "DirCopy/file1.txt");
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			dir = string.Concat(path, "DirCopy/file2.txt");
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			dir = string.Concat(path, "DirCopy/file3txt");
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			Assert.IsTrue(TestScript("file-filegetattrib", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetShortcut()
		{
			if (Directory.Exists("./FileGetShortcut"))
				Directory.Delete("./FileGetShortcut", true);

			if (System.IO.File.Exists("./testshortcut.lnk"))
				System.IO.File.Delete("./testshortcut.lnk");

			var dir = string.Concat(path, "DirCopy");
			Keysharp.Core.Dir.DirCopy(dir, "./FileGetShortcut/");
			var patharg = Path.GetDirectoryName(Path.GetFullPath("./FileGetShortcut/file1.txt"));
			Keysharp.Core.File.FileCreateShortcut
			(
				"./FileGetShortcut/file1.txt",
				"./testshortcut.lnk",
				patharg,
				"",
				"TestDescription",
				"../../../Keysharp.ico",
				""
			);
			var shortcut = Keysharp.Core.File.FileGetShortcut("./testshortcut.lnk");
			Assert.AreEqual(Path.GetFullPath("./FileGetShortcut/file1.txt").ToLower(), shortcut.OutTarget.ToString().ToLower());
			Assert.AreEqual(patharg, shortcut.OutDir.ToString());
			Assert.AreEqual("TestDescription", shortcut.OutDescription.ToString());
			Assert.AreEqual("", shortcut.OutArgs.ToString());
			Assert.AreEqual(Path.GetFullPath("../../../Keysharp.ico"), shortcut.OutIcon.ToString());
			Assert.AreEqual("0", shortcut.OutIconNum.ToString());
			Assert.AreEqual("1", shortcut.OutRunState.ToString());

			if (System.IO.File.Exists("./testshortcut.lnk"))
				System.IO.File.Delete("./testshortcut.lnk");

			Assert.IsTrue(TestScript("file-filegetshortcut", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetSize()
		{
			var dir = string.Concat(path, "DirCopy/file1.txt");
			var size = Keysharp.Core.File.FileGetSize(dir);
			Assert.AreEqual(14, size);
			size = Keysharp.Core.File.FileGetSize(dir, "k");
			Assert.AreEqual(0, size);
			size = Keysharp.Core.File.FileGetSize(dir, "m");
			Assert.AreEqual(0, size);
			size = Keysharp.Core.File.FileGetSize(dir, "g");
			Assert.AreEqual(0, size);
			size = Keysharp.Core.File.FileGetSize(dir, "t");
			Assert.AreEqual(0, size);
			Assert.IsTrue(TestScript("file-filegetsize", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetTime()
		{
			var dir = string.Concat(path, "DirCopy/file1.txt");
			var filetime = Keysharp.Core.File.FileGetTime(dir);
			var dt = Keysharp.Core.Conversions.ToDateTime(filetime);
			Assert.AreNotEqual(dt, DateTime.MinValue);
			Assert.IsTrue(TestScript("file-filegettime", true));
		}

		[Test, Category("FileAndDir")]
		public void FileGetVersion()
		{
			var file = "./Keysharp.Core.dll";
			var fileversion = Keysharp.Core.File.FileGetVersion(file);
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
			Keysharp.Core.File.FileCopy($"{dir}/*", "./FileMove/");
			Assert.IsTrue(System.IO.File.Exists("./FileMove/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileMove/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileMove/file3txt"));

			if (Directory.Exists("./FileMove2"))
				Directory.Delete("./FileMove2", true);

			_ = Directory.CreateDirectory("./FileMove2");
			Assert.IsTrue(Directory.Exists("./FileMove2"));
			Keysharp.Core.File.FileMove($"./FileMove/*", "./FileMove2");
			Assert.IsTrue(!System.IO.File.Exists("./FileMove/file1.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileMove/file2.txt"));
			Assert.IsTrue(!System.IO.File.Exists("./FileMove/file3txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileMove2/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileMove2/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileMove2/file3txt"));
			Directory.Delete("./FileMove", true);
			Directory.Delete("./FileMove2", true);
			Assert.IsTrue(TestScript("file-filemove", true));
		}

		[Test, Category("FileAndDir")]
		public void FileOpen()
		{
			//Can't really test console in/out/err with * and ** here, but in manual testing it appears to work.
			var filename = "./testfileobject1.txt";

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw"))//Simplest first, read/write.
			{
				var w = "testing";
				var count = f.WriteLine(w);
				f.Seek(0);//Test seeking from beginning.
				var r = f.ReadLine();
				Assert.AreEqual("testing", r);
				Assert.AreEqual(w.Length + 1, (int)count);//Add one for the newline.
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw"))//Read/write integers.
			{
				uint val = 0x01020304;
				var count = f.WriteUInt(val);
				f.Seek(0);
				var r = f.ReadUInt();
				Assert.AreEqual(val, (uint)r);
				Assert.AreEqual(sizeof(uint), (int)count);
				var val2 = -12345678;
				count = f.WriteInt(val2);
				f.Seek(-4, 1);//Test seeking from current.
				var r2 = f.ReadInt();
				Assert.AreEqual(val2, (int)r2);
				Assert.AreEqual(sizeof(int), (int)count);
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw"))//Read/write buffers and arrays.
			{
				var buf = new Buffer(4);
				unsafe
				{
					var ptr = (byte*)buf.Ptr.ToPointer();

					for (var i = 0L; i < (long)buf.Size; i++)
						ptr[i] = (byte)i;
				}
				//
				var count = f.RawWrite(buf);
				f.Seek(0);
				var buf2 = new Buffer(4, 0);
				f.RawRead(buf2);
				unsafe
				{
					var p1 = (byte*)buf.Ptr.ToPointer();
					var p2 = (byte*)buf2.Ptr.ToPointer();

					for (var i = 0L; i < (long)buf.Size; i++)
					{
						Assert.AreEqual(p1[i], p2[i]);
						Assert.AreEqual(p1[i], i);
						Assert.AreEqual(i, p2[i]);
					}
				}
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw"))//Read/write buffers and arrays.
			{
				var arr = new Array(4);

				for (var i = 0; i < (long)arr.Count; i++)
					arr.Push((byte)i);

				var count = f.RawWrite(arr);
				f.Seek(0);
				var arr2 = new Array(4);
				f.RawRead(arr2);

				for (var i = 1; i <= arr.Count; i++)
				{
					Assert.AreEqual(arr[i], arr2[i]);//Array always expects a 1-based index.
					Assert.AreEqual(i - 1, arr[i]);
					Assert.AreEqual(i - 1, arr2[i]);
				}
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw", "Unicode"))//Test text encoding.
			{
				var w = "testing";
				var count = f.Write(w);
				f.Seek(2);//A unicode file will have a 2 byte long byte order mark.
				var r = f.ReadLine();
				Assert.AreEqual("testing", r);
				Assert.AreEqual(w.Length * sizeof(char), (int)count);//Unicode is two bytes per char.
				Assert.AreEqual(f.Length, 16);//BOM plus 2 bytes per char.
			}

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw", "Unicode"))//Ensure reading an existing file with a BOM works.
			{
				var w = "testing";
				var r = f.ReadLine();
				Assert.AreEqual("testing", r);
				Assert.AreEqual(w.Length, r.Length);
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw"))//Test position.
			{
				var w = "testing";
				var count = f.Write(w);
				var pos = f.Pos;
				Assert.AreEqual(w.Length, pos);
				var eof = f.AtEOF;
				Assert.AreEqual(eof, 1L);
				var len = f.Length;
				Assert.AreEqual(len, 7L);
				var enc = f.Encoding;
				Assert.AreEqual(enc, "utf-8");
			}

			//Do not delete here, file is used for appending.
			using (var f = Keysharp.Core.File.FileOpen(filename, "a"))//Test append.
			{
				var w = "testing";
				var count = f.Write(w);
				var pos = f.Pos;
				var eof = f.AtEOF;
				Assert.AreEqual(eof, 0L);//With append mode, you're never really at the "end" of the Keysharp.Core.File.
				var len = f.Length;
				Assert.AreEqual(pos, 14L);
				Assert.AreEqual(len, 14L);
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "w"))//Test write only.
			{
				var w = "testing";
				var count = f.Write(w);
			}

			using (var f = Keysharp.Core.File.FileOpen(filename, "w"))//Test write only on an existing file, which should clear it.
			{
				var pos = f.Pos;
				var eof = f.AtEOF;
				var len = f.Length;
				Assert.AreEqual(eof, 1L);//Overwrite should cause it to be an empty Keysharp.Core.File.
				Assert.AreEqual(pos, 0L);
				Assert.AreEqual(len, 0L);
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			using (var f = Keysharp.Core.File.FileOpen(filename, "w"))//Test write only.
			{
				var w = "testing";
				var count = f.Write(w);
			}

			using (var f = Keysharp.Core.File.FileOpen(filename, "rw"))//Test read/write on an existing file, which should not clear it.
			{
				var pos = f.Pos;
				var eof = f.AtEOF;
				var len = f.Length;
				Assert.AreEqual(eof, 0L);//At position zero, so not at EOF.
				Assert.AreEqual(pos, 0L);
				Assert.AreEqual(len, 7L);
			}

			//Test modes and permissions by checking for expected exceptions.
			TestException(() =>
			{
				using (var f = Keysharp.Core.File.FileOpen(filename, "r -r"))//Test read share.
				{
					using (var f2 = Keysharp.Core.File.FileOpen(filename, "r"))
					{
					}
				}
			});
			TestException(() =>
			{
				using (var f = Keysharp.Core.File.FileOpen(filename, "rw -w"))//Test write share.
				{
					using (var f2 = Keysharp.Core.File.FileOpen(filename, "rw"))
					{
					}
				}
			});

			using (var f = Keysharp.Core.File.FileOpen(filename, "r -r"))//Test read share create from handle.
			{
				var handle = f.Handle;

				using (var f2 = Keysharp.Core.File.FileOpen(handle, "r h"))
				{
				}
			}

			if (System.IO.File.Exists(filename))
				System.IO.File.Delete(filename);

			TestException(() =>
			{
				using (var f = Keysharp.Core.File.FileOpen(filename, "r"))//Test opening a non existent file in read mode which should crash.
				{
				}
			});
			Assert.IsTrue(TestScript("file-fileopen", true));
		}

		[Test, Category("FileAndDir")]
		public void FileRead()
		{
			var dir = string.Concat(path, "DirCopy/file1.txt");
			var text = Keysharp.Core.File.FileRead(dir);
			Assert.AreEqual("this is file 1", text);
			text = Keysharp.Core.File.FileRead(dir, "m4");
			Assert.AreEqual("this", text);
			text = Keysharp.Core.File.FileRead(dir, "m4 utf-8");
			Assert.AreEqual("this", text);
			var buf1 = Keysharp.Core.File.FileRead(dir, "m4 raw");
			var buf2 = new Keysharp.Core.Buffer(new byte[] { (byte)'t', (byte)'h', (byte)'i', (byte)'s' });
			Assert.IsTrue((bool)Keysharp.Scripting.Script.Operate(Keysharp.Scripting.Script.Operator.ValueEquality, buf1, buf2));
			Assert.IsTrue(TestScript("file-fileread", true));
		}

		[NonParallelizable]
		[Test, Category("FileAndDir")]
		public void FileRecycle()
		{
			lock (syncroot)
			{
				if (Directory.Exists("./FileRecycle"))
					Directory.Delete("./FileRecycle", true);

				_ = Directory.CreateDirectory("./FileRecycle");
				var dir = string.Concat(path, "DirCopy");
				Keysharp.Core.File.FileCopy($"{dir}/*", "./FileRecycle/");
				Assert.IsTrue(System.IO.File.Exists("./FileRecycle/file1.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycle/file2.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycle/file3txt"));
				Keysharp.Core.File.FileRecycle("./FileRecycle/file1.txt");
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycle/file1.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycle/file2.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycle/file3txt"));
				Keysharp.Core.File.FileRecycle("./FileRecycle/*.txt");
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycle/file2.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycle/file3txt"));
				Keysharp.Core.File.FileRecycle("./FileRecycle/*");
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycle/file3txt"));
				Directory.Delete("./FileRecycle", true);
				Assert.IsTrue(TestScript("file-filerecycle", true));
			}
		}

		[NonParallelizable]
		[Test, Category("FileAndDir")]
		public void FileRecycleEmpty()
		{
			lock (syncroot)
			{
				if (Directory.Exists("./FileRecycleEmpty"))
					Directory.Delete("./FileRecycleEmpty", true);

				_ = Directory.CreateDirectory("./FileRecycleEmpty");
				var dir = string.Concat(path, "DirCopy");
				Keysharp.Core.File.FileCopy($"{dir}/*", "./FileRecycleEmpty/");
				Assert.IsTrue(System.IO.File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycleEmpty/file3txt"));
				Keysharp.Core.File.FileRecycle("./FileRecycleEmpty/*");
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycleEmpty/file3txt"));
				Keysharp.Core.File.FileRecycleEmpty();

				if (Directory.Exists("./FileRecycleEmpty"))
					Directory.Delete("./FileRecycleEmpty", true);

				_ = Directory.CreateDirectory("./FileRecycleEmpty");
				Keysharp.Core.File.FileCopy($"{dir}/*", "./FileRecycleEmpty/");
				Assert.IsTrue(System.IO.File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(System.IO.File.Exists("./FileRecycleEmpty/file3txt"));
				Keysharp.Core.File.FileRecycle("./FileRecycleEmpty/*");
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycleEmpty/file1.txt"));
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycleEmpty/file2.txt"));
				Assert.IsTrue(!System.IO.File.Exists("./FileRecycleEmpty/file3txt"));
				Keysharp.Core.File.FileRecycleEmpty("C:\\");
				Assert.IsTrue(TestScript("file-filerecycleempty", true));
			}
		}

		[Test, Category("FileAndDir")]
		public void FileSetAttrib()
		{
			if (Directory.Exists("./FileSetAttrib"))
				Directory.Delete("./FileSetAttrib", true);

			var dir = string.Concat(path, "DirCopy");
			Dir.DirCopy(dir, "./FileSetAttrib");
			Assert.IsTrue(Directory.Exists("./FileSetAttrib"));
			Assert.IsTrue(System.IO.File.Exists("./FileSetAttrib/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileSetAttrib/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileSetAttrib/file3txt"));
			dir = "./FileSetAttrib";
			var attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("D", attr);
			dir = "./FileSetAttrib/file1.txt";
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			Keysharp.Core.File.FileSetAttrib('r', dir);
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("RA", attr);
			Keysharp.Core.File.FileSetAttrib("-r", dir);
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("A", attr);
			Keysharp.Core.File.FileSetAttrib("^r", dir);
			attr = Keysharp.Core.File.FileGetAttrib(dir);
			Assert.AreEqual("RA", attr);
			Keysharp.Core.File.FileSetAttrib("^r", dir);
			attr = Keysharp.Core.File.FileGetAttrib(dir);
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
			Dir.DirCopy(dir, "./FileSetTime");
			Assert.IsTrue(Directory.Exists("./FileSetTime"));
			Assert.IsTrue(System.IO.File.Exists("./FileSetTime/file1.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileSetTime/file2.txt"));
			Assert.IsTrue(System.IO.File.Exists("./FileSetTime/file3txt"));
			Keysharp.Core.File.FileSetTime("20200101131415", "./FileSetTime/file1.txt", 'm');
			var filetime = Keysharp.Core.File.FileGetTime("./FileSetTime/file1.txt", 'm');
			Assert.AreEqual("20200101131415", filetime);
			Keysharp.Core.File.FileSetTime("20200101131416", "./FileSetTime/file1.txt", 'c');
			filetime = Keysharp.Core.File.FileGetTime("./FileSetTime/file1.txt", 'c');
			Assert.AreEqual("20200101131416", filetime);
			Keysharp.Core.File.FileSetTime("20200101131417", "./FileSetTime/file1.txt", 'a');
			filetime = Keysharp.Core.File.FileGetTime("./FileSetTime/file1.txt", 'a');
			Assert.AreEqual("20200101131417", filetime);

			if (Directory.Exists("./FileSetTime"))
				Directory.Delete("./FileSetTime", true);

			Assert.IsTrue(TestScript("file-filesettime", true));
		}

		[Test, Category("FileAndDir")]
		public void FixFilters()
		{
			var str = Dialogs.FixFilters("");
			Assert.AreEqual("All Files (*.*)|*.*", str);
			str = Dialogs.FixFilters("Text (*.txt)");
			Assert.AreEqual("Text (*.txt)|*.txt", str);
			str = Dialogs.FixFilters("Text (*.txt)|*.txt");
			Assert.AreEqual("Text (*.txt)|*.txt", str);
			str = Dialogs.FixFilters("Images(*.png,*.jpg)");
			Assert.AreEqual("Images(*.png,*.jpg)|*.png;*.jpg", str);
			str = Dialogs.FixFilters("Images(*.png,*.jpg)|*.png;*.jpg");
			Assert.AreEqual("Images(*.png,*.jpg)|*.png;*.jpg", str);
			str = Dialogs.FixFilters("Text (*.txt)|Images(*.png,*.jpg)");
			Assert.AreEqual("Text (*.txt)|*.txt|Images(*.png,*.jpg)|*.png;*.jpg", str);
			str = Dialogs.FixFilters("Text (*.txt)|*.txt|Images(*.png,*.jpg)");
			Assert.AreEqual("Text (*.txt)|*.txt|Images(*.png,*.jpg)|*.png;*.jpg", str);
			str = Dialogs.FixFilters("Text (*.txt)|*.txt|Images(*.png,*.jpg)|*.png;*.jpg");
			Assert.AreEqual("Text (*.txt)|*.txt|Images(*.png,*.jpg)|*.png;*.jpg", str);
		}

		[Test, Category("FileAndDir")]
		public void IniReadWriteDelete()
		{
			if (System.IO.File.Exists("./testini2.ini"))
				System.IO.File.Delete("./testini2.ini");

			var dir = string.Concat(path, "/testini.ini");
			Keysharp.Core.File.FileCopy(dir, "./testini2.ini", true);
			Assert.IsTrue(System.IO.File.Exists("./testini2.ini"));
			var val = Ini.IniRead("./testini2.ini", "sectionone", "keyval");
			Assert.AreEqual("theval", val);
			val = Ini.IniRead("./testini2.ini", "SectionOne", "keyval");//Ensure the file is processed as case insensitive.
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

			if (System.IO.File.Exists("./testini2.ini"))
				System.IO.File.Delete("./testini2.ini");

			Assert.IsTrue(TestScript("file-inireadwritedelete", true));
		}

		[Test, Category("FileAndDir")]
		public void SetWorkingDir()
		{
			var origdir = Accessors.A_WorkingDir;
			var fullpath = Path.GetFullPath(string.Concat(path, "DirCopy"));
			Dir.SetWorkingDir(fullpath);
			Assert.AreEqual(fullpath, Accessors.A_WorkingDir);
			Dir.SetWorkingDir("C:\\a\\fake\\path");//Non-existent folders don't get assigned.
			Assert.AreEqual(fullpath, Accessors.A_WorkingDir);//So it should remain unchanged.
			Dir.SetWorkingDir(origdir);
			Assert.IsTrue(TestScript("file-filesetworkingdir", true));
		}

		[Test, Category("FileAndDir")]
		public void SplitPath()
		{
			var fullpath = string.Concat(path, "DirCopy/file1.txt");
			var splitpath = Dir.SplitPath(fullpath);
			var filename = splitpath.OutFileName;
			var dir = splitpath.OutDir;
			var ext = splitpath.OutExtension;
			var namenoext = splitpath.OutNameNoExt;
			var drive = splitpath.OutDrive;
			Assert.AreEqual("file1.txt", filename);
			Assert.AreEqual("H:\\Dev\\keysharp\\Keysharp.Tests\\Code\\DirCopy".ToLower(), dir.ToLower());//This will be different on non-windows or on other dev machines.
			Assert.AreEqual("txt", ext);
			Assert.AreEqual("file1", namenoext);
			Assert.AreEqual("H:\\", drive);
			Assert.IsTrue(TestScript("file-filesplitpath", true));
		}
	}
}