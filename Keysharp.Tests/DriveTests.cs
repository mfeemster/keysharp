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
		[Test, Category("Drive")]
		public void DriveGetSpaceFree()
		{
			var free = Disk.DriveGetSpaceFree("C:\\");
			Assert.IsTrue(free > 10);//Assume anyone who is running this has at least 10MB of disk space left.
			Assert.IsTrue(TestScript("drive-getspacefree", true));
		}

		[Test, Category("Drive")]
		public void DriveGetCapacity()
		{
			var free = Disk.DriveGetCapacity("C:\\");
			Assert.IsTrue(free > 1000);//Assume anyone who is running this has at least 1MB of total disk space.
			Assert.IsTrue(TestScript("drive-getcapacity", true));
		}

		[Test, Category("Drive")]
		public void DriveGetFileSystem()
		{
			var sys = Disk.DriveGetFileSystem("C:\\");
			Assert.IsTrue(sys == "NTFS" || sys == "FAT32" || sys == "FAT" || sys == "CDFS" || sys == "UDF");//Assume it's at least one of the common file system types.
			Assert.IsTrue(TestScript("drive-getfilesystem", true));
		}

		[Test, Category("Drive")]
		public void DriveGetList()
		{
			var sys = Disk.DriveGetList();
			Assert.IsTrue(sys.StartsWith("C"));//Assume it's at least one of the common drive names.
			Assert.IsTrue(TestScript("drive-getlist", true));
		}

		[Test, Category("Drive")]
		public void DriveGetSerial()
		{
			var sys = Disk.DriveGetSerial("C:\\");
			Assert.IsTrue(sys > 1);//It will be some large hex number.
			Assert.IsTrue(TestScript("drive-getserial", true));
		}

		[Test, Category("Drive")]
		public void DriveGetType()
		{
			var type = Disk.DriveGetType("C:\\");
			Assert.AreEqual("Fixed", type);
			Assert.IsTrue(TestScript("drive-gettype", true));
		}

		[Test, Category("Drive")]
		public void DriveGetStatus()
		{
			var ready = Disk.DriveGetStatus("C:\\");
			Assert.AreEqual("Ready", ready);
			Assert.IsTrue(TestScript("drive-getstatus", true));
		}

		[Test, Category("Drive")]
		public void DriveLockUnlock()//Hard to test because many machines do not have CD drives.
		{
			//Disk.DriveLock("C:\\");
			//Assert.IsTrue(true);
			//Disk.DriveUnlock("C:\\");
			//Assert.IsTrue(true);
			//Assert.IsTrue(TestScript("drive-lock-unlock", true));
		}

		[Test, Category("Drive")]
		public void DriveEject()//Hard to test because many machines do not have CD drives.
		{
			//Disk.DriveEject("C:\\", false);
			//Assert.IsTrue(true);
			//Disk.DriveEject("C:\\", true);
			//Assert.IsTrue(true);
			//Assert.IsTrue(TestScript("drive-driveeject", true));
		}

		[Test, Category("Drive")]
		public void DriveGetSetLabel()
		{
			var origlabel = Disk.DriveGetLabel("C:\\");
			Disk.DriveSetLabel("C:\\", "a test label");//Visual Studio needs to be running as administrator for this to work.
			var newlabel = Disk.DriveGetLabel("C:\\");
			Assert.AreEqual("a test label", newlabel);
			Disk.DriveSetLabel("C:\\", origlabel);
			newlabel = Disk.DriveGetLabel("C:\\");
			Assert.AreEqual(origlabel, newlabel);
			Assert.IsTrue(TestScript("drive-getsetlabel", true));
		}

		[Test, Category("Drive")]
		public void DriveGetStatusCD()
		{
			var ready = Disk.DriveGetStatusCD("C:\\");
			Assert.AreEqual("error", ready);//Hard to test because many machines do not have CD drives.
			Assert.IsTrue(TestScript("drive-getstatuscd", true));
		}
	}
}