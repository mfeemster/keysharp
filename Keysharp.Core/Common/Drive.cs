using System;
using System.IO;

namespace Keysharp.Core.Common
{
	internal abstract class Drive
	{
		protected DriveInfo drive;

		internal abstract long Serial { get; }

		internal abstract StatusCD Status { get; }

		internal Drive(DriveInfo udrive) => drive = udrive;

		/// <summary>
		/// Ejects the CD Drive
		/// </summary>
		internal abstract void Eject();

		internal bool IsCDDrive()
		{
			try
			{
				return (drive.DriveType == DriveType.CDRom);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Locks the drives Eject ability
		/// </summary>
		internal abstract void Lock();

		/// <summary>
		/// Retracts the CD Drive
		/// </summary>
		internal abstract void Retract();

		/// <summary>
		/// Unlocks the drives Eject ability
		/// </summary>
		internal abstract void UnLock();

		internal static class DriveProvider
		{
			// UNDONE: organise DriveProvider

			/// <summary>
			/// Creates platform specific SystemDrive Instance
			/// </summary>
			/// <param name="drive"></param>
			/// <returns></returns>
			internal static Drive CreateDrive(DriveInfo drive)
			{
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					return new Windows.Drive(drive);
				else
					return new Linux.Drive(drive);
			}
		}

		/// <summary>
		/// Status of the CD
		/// </summary>
		internal enum StatusCD
		{
			NotReady,
			Open,
			Playing,
			Paused,
			Seeking,
			Stopped
		}
	}
}