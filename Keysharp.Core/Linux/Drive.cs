using System;
using System.IO;

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Implementation for native Linux Drive Operations
	/// </summary>
	internal class Drive : Common.Drive
	{
		internal override long Serial => throw new NotImplementedException();

		internal override StatusCD Status => throw new NotImplementedException();

		internal Drive(DriveInfo drv)
			: base(drv) { }

		internal override void Eject()
		{
			throw new NotImplementedException();
		}

		internal override void Lock()
		{
			throw new NotImplementedException();
		}

		internal override void Retract()
		{
			throw new NotImplementedException();
		}

		internal override void UnLock()
		{
			throw new NotImplementedException();
		}
	}
}