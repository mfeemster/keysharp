using System;

namespace Keysharp.Core.Common.Window
{
	internal static class ControlProvider
	{
		/// <summary>
		/// Creates a ControlManager for the current Platform
		/// </summary>
		public static ControlManagerBase CreateControlManager() => Environment.OSVersion.Platform == PlatformID.Win32NT ? new Windows.ControlManager() : new Linux.ControlManager();
	}

	internal static class WindowProvider
	{
		/// <summary>
		/// Creates a WindowManager for the current Platform
		/// </summary>
		public static WindowManagerBase CreateWindowManager() => Environment.OSVersion.Platform == PlatformID.Win32NT ? new Windows.WindowManager() : new Linux.WindowManager();
	}
}