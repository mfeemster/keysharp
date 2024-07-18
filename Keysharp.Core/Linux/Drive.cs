#if LINUX
namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of Drive for the linux platfrom.
	/// </summary>
	internal class Drive : Common.DriveBase
	{
		internal override long Serial => throw new NotImplementedException();

		internal override string StatusCD
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		internal Drive(DriveInfo drv)
			: base(drv) { }

		internal override void Eject() => throw new NotImplementedException();

		internal override void Lock() => throw new NotImplementedException();

		internal override void Retract() => throw new NotImplementedException();

		internal override void UnLock() => throw new NotImplementedException();
	}
}
#endif