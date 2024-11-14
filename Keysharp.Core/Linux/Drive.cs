#if LINUX

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of Drive for the linux platfrom.
	/// </summary>
	internal class Drive : DriveBase
	{
		internal override long Serial
		{
			get
			{
				var serial = $"udevadm info --query=property --name={drive.Name} | grep ID_SERIAL_SHORT".Bash();

				if (!string.IsNullOrEmpty(serial))
				{
					var components = serial.Split('=');

					if (components.Length >= 2)
						return components[1].Al();
				}

				return 0L;
			}
		}

		internal override string StatusCD
		{
			get
			{
				Keysharp.Scripting.Script.OutputDebug($"Obtaining the status of the CD/DVD drive is not supported on linux.");
				return "";
			}
		}

		internal Drive(DriveInfo drv)
			: base(drv) { }

		internal override void Eject() => $"eject {drive.Name}".Bash();

		internal override void Lock() => $"eject -i 1 {drive.Name}".Bash();

		internal override void Retract() => $"eject -t {drive.Name}".Bash();

		internal override void UnLock() => $"eject -i 0 {drive.Name}".Bash();
	}
}
#endif