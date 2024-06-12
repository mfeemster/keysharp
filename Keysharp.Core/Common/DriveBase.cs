using System;
using System.IO;

namespace Keysharp.Core.Common
{
	internal abstract class DriveBase
	{
		protected DriveInfo drive;

		internal abstract long Serial { get; }

		internal string VolumeLabel
		{
			get => drive.VolumeLabel;
			set => drive.VolumeLabel = value;
		}

		internal abstract string StatusCD { get; }

		internal DriveBase(DriveInfo udrive) => drive = udrive;

		/// <summary>
		/// Ejects the CD Drive
		/// </summary>
		internal abstract void Eject();

		internal bool IsCDDrive()
		{
			try
			{
				return drive.DriveType == DriveType.CDRom;
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

		/// <summary>
		/// Status of the CD
		/// </summary>
		//internal enum StatusCD
		//{
		//  NotReady,
		//  Open,
		//  Playing,
		//  Paused,
		//  Seeking,
		//  Stopped
		//}
	}
}