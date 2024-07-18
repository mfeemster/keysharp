namespace Keysharp.Core.Common.Platform
{
	internal static class ControlProvider
	{
		internal static ControlManagerBase Manager { get; } =
#if WINDOWS
			new Windows.ControlManager()
#elif LINUX
			new Linux.ControlManager()
#endif
		;
	}

	internal static class WindowProvider
	{
		internal static WindowManagerBase Manager { get; } =
#if WINDOWS
			new Windows.WindowManager()
#elif LINUX
			new Linux.WindowManager()
#endif
		;
	}

	internal static class PlatformProvider
	{
		internal static PlatformManagerBase Manager { get; } =
#if WINDOWS
			new Windows.PlatformManager()
#elif LINUX
			new Linux.PlatformManager()
#endif
		;
	}

	internal static class DriveProvider
	{
		/// <summary>
		/// Creates a Drive for the current platform
		/// </summary>
		/// <param name="drive"></param>
		/// <returns></returns>
		internal static DriveBase CreateDrive(DriveInfo drive)
		{
#if WINDOWS
			return new Windows.Drive(drive);
#elif LINUX
			return new Linux.Drive(drive);
#endif
		}
	}

	internal static class StatusBarProvider
	{
		/// <summary>
		/// Creates a StatusBar for the current platform
		/// </summary>
		/// <param name="drive"></param>
		/// <returns></returns>
		internal static StatusBarBase CreateStatusBar(IntPtr hwnd)
		{
#if WINDOWS
			return new Windows.StatusBar(hwnd);
#elif LINUX
			return new Linux.StatusBar(hwnd);
#endif
		}
	}
}