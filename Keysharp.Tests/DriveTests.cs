using Keysharp.Core;
using NUnit.Framework;

namespace Keysharp.Tests
{
	public partial class Scripting
	{
		[Test, Category("Drive")]
		public void DriveGetSpaceFree()
		{
			var free = Drive.DriveGetSpaceFree("C:\\");
			Assert.IsTrue(free > 10);//Assume anyone who is running this has at least 10MB of disk space left.
			Assert.IsTrue(TestScript("drive-getspacefree", true));
		}

		[Test, Category("Drive")]
		public void DriveGetCapacity()
		{
			var free = Drive.DriveGetCapacity("C:\\");
			Assert.IsTrue(free > 1000);//Assume anyone who is running this has at least 1MB of total disk space.
			Assert.IsTrue(TestScript("drive-getcapacity", true));
		}

		[Test, Category("Drive")]
		public void DriveGetFileSystem()
		{
			var sys = Drive.DriveGetFileSystem("C:\\");
			Assert.IsTrue(sys == "NTFS" || sys == "FAT32" || sys == "FAT" || sys == "CDFS" || sys == "UDF");//Assume it's at least one of the common file system types.
			Assert.IsTrue(TestScript("drive-getfilesystem", true));
		}

		[Test, Category("Drive")]
		public void DriveGetList()
		{
			var sys = Drive.DriveGetList();
			Assert.IsTrue(sys.StartsWith("C"));//Assume it's at least one of the common drive names.
			Assert.IsTrue(TestScript("drive-getlist", true));
		}

		[Test, Category("Drive")]
		public void DriveGetSerial()
		{
			var sys = Drive.DriveGetSerial("C:\\");
			Assert.IsTrue(sys > 1);//It will be some large hex number.
			Assert.IsTrue(TestScript("drive-getserial", true));
		}

		[Test, Category("Drive")]
		public void DriveGetType()
		{
			var type = Drive.DriveGetType("C:\\");
			Assert.AreEqual("Fixed", type);
			Assert.IsTrue(TestScript("drive-gettype", true));
		}

		[Test, Category("Drive")]
		public void DriveGetStatus()
		{
			var ready = Drive.DriveGetStatus("C:\\");
			Assert.AreEqual("Ready", ready);
			Assert.IsTrue(TestScript("drive-getstatus", true));
		}

		[Test, Category("Drive")]
		public void DriveLockUnlock()//Hard to test because many machines do not have CD drives.
		{
			//Drive.DriveLock("C:\\");
			//Assert.IsTrue(true);
			//Drive.DriveUnlock("C:\\");
			//Assert.IsTrue(true);
			//Assert.IsTrue(TestScript("drive-lock-unlock", true));
		}

		[Test, Category("Drive")]
		public void DriveEject()//Hard to test because many machines do not have CD drives.
		{
			//Drive.DriveEject("C:\\", false);
			//Assert.IsTrue(true);
			//Drive.DriveEject("C:\\", true);
			//Assert.IsTrue(true);
			//Assert.IsTrue(TestScript("drive-driveeject", true));
		}

		[Test, Category("Drive")]
		public void DriveGetSetLabel()
		{
			var origlabel = Drive.DriveGetLabel("C:\\");
			Drive.DriveSetLabel("C:\\", "a test label");//Visual Studio needs to be running as administrator for this to work.
			var newlabel = Drive.DriveGetLabel("C:\\");
			Assert.AreEqual("a test label", newlabel);
			Drive.DriveSetLabel("C:\\", origlabel);
			newlabel = Drive.DriveGetLabel("C:\\");
			Assert.AreEqual(origlabel, newlabel);
			Assert.IsTrue(TestScript("drive-getsetlabel", true));
		}

		[Test, Category("Drive")]
		public void DriveGetStatusCD()
		{
			//var ready = Drive.DriveGetStatusCD("C:\\");
			//Assert.AreEqual("error", ready);//Hard to test because many machines do not have CD drives.
			//Assert.IsTrue(TestScript("drive-getstatuscd", true));
		}
	}
}