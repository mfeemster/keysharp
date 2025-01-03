#if WINDOWS
namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Implementation for native Windows Drive Operations
	/// </summary>
	internal class Drive : DriveBase
	{
		private static readonly string IOPathPrefix = @"\\.\";

		internal string CreateDeviceIOPath => IOPathPrefix + drive.Name.Substring(0, 1) + ":";

		internal override long Serial
		{
			get
			{
				var serialstr = Wmi.Identifier("Win32_LogicalDisk", "VolumeSerialNumber", "SELECT * FROM Win32_LogicalDisk WHERE DeviceId = '" + drive.Name.Substring(0, 2) + "'");//Can't use interpolated string here because the AStyle formatter misinterprets it.
				return long.TryParse(serialstr, NumberStyles.HexNumber, Parser.culture, out var l) ? l : 0L;
			}
		}

		internal override string StatusCD
		{
			get
			{
				Error err;
				var sb = new StringBuilder(128);
				var str = $"open {drive.Name} type cdaudio alias cd wait shareable";

				if (WindowsAPI.mciSendString(str, sb, sb.Capacity, IntPtr.Zero) != 0)
					return Errors.ErrorOccurred(err = new Error($"Opening CD {drive.Name} failed.")) ? throw err : null;

				var res = WindowsAPI.mciSendString("status cdaudio mode", sb, sb.Capacity, IntPtr.Zero);
				_ = WindowsAPI.mciSendString("close cd wait", null, 0, IntPtr.Zero);

				if (res == 0)
					return sb.ToString();

				return Errors.ErrorOccurred(err = new Error($"Obtaining status for CD {drive.Name} failed.")) ? throw err : null;
			}
		}

		internal Drive(DriveInfo drv)
			: base(drv) { }

		internal override void Eject() => EjectRetract(WindowsAPI.IOCTL_STORAGE_EJECT_MEDIA, 0L, 0L);

		internal override void Lock() => EjectRetract(WindowsAPI.IOCTL_STORAGE_EJECTION_CONTROL, 1L, 0L);

		internal override void Retract() => EjectRetract(WindowsAPI.IOCTL_STORAGE_LOAD_MEDIA, 0L, 0L);

		internal override void UnLock() => EjectRetract(WindowsAPI.IOCTL_STORAGE_EJECTION_CONTROL, 0L, 0L);

		private void EjectRetract(uint control, long l, long lo)
		{
			var fileHandle = IntPtr.Zero;
			Exception exception = null;

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
					var ovl = new NativeOverlapped();
					_ = WindowsAPI.DeviceIoControl(fileHandle, control,
												   ref l, 0,
												   ref lo, 0,
												   ref returnedBytes,
												   ref ovl);
				}
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			finally
			{
				//Close Drive Handle.
				_ = WindowsAPI.CloseHandle(fileHandle);
				fileHandle = IntPtr.Zero;
			}

			if (exception != null)
			{
				Error err;
				_ = Errors.ErrorOccurred(err = new OSError(exception, "Error ejecting, retracting, locking or unlocking the drive.")) ? throw err : "";
			}
		}
	}
}

#endif