using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Implementation for native Windows Drive Operations
	/// </summary>
	internal class Drive : Common.Drive
	{
		private static readonly string IOPathPrefix = @"\\.\";

		internal string CreateDeviceIOPath => IOPathPrefix + drive.Name.Substring(0, 1) + ":";

		internal override long Serial => throw new NotImplementedException();

		internal override StatusCD Status => throw new NotImplementedException();

		internal Drive(DriveInfo drv)
			: base(drv) { }

		internal override void Eject() => EjectRetract(WindowsAPI.IOCTL_STORAGE_EJECT_MEDIA, 0L, 0L);

		internal override void Lock() => EjectRetract(WindowsAPI.IOCTL_STORAGE_EJECTION_CONTROL, 1L, 0L);

		internal override void Retract() => EjectRetract(WindowsAPI.IOCTL_STORAGE_LOAD_MEDIA, 0L, 0L);

		internal override void UnLock() => EjectRetract(WindowsAPI.IOCTL_STORAGE_EJECTION_CONTROL, 0L, 0L);

		private void EjectRetract(uint control, long l, long lo)
		{
			var fileHandle = IntPtr.Zero;

			try
			{
				//Create an handle to the drive.
				fileHandle = WindowsAPI.CreateFile(CreateDeviceIOPath,
												   WindowsAPI.GENERICREAD, 0, IntPtr.Zero,
												   WindowsAPI.OPENEXISTING, 0, IntPtr.Zero);

				if ((int)fileHandle != WindowsAPI.INVALID_HANDLE)
				{
					//Eject the disk.
					var returnedBytes = 0;
					var ovl = new System.Threading.NativeOverlapped();
					_ = WindowsAPI.DeviceIoControl(fileHandle, control,
												   ref l, 0,
												   ref lo, 0,
												   ref returnedBytes,
												   ref ovl);
				}
			}
			catch
			{
				throw new Exception(Marshal.GetLastWin32Error().ToString());
			}
			finally
			{
				//Close Drive Handle.
				_ = WindowsAPI.CloseHandle(fileHandle);
				fileHandle = IntPtr.Zero;
			}
		}
	}
}