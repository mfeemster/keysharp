#if !LINUX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Core.Common.Platform;
using Keysharp.Core.Windows;

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of PlatformManager for the linux platfrom.
	/// </summary>
	internal class PlatformManager : PlatformManagerBase
	{
		internal override IntPtr GetKeyboardLayout(uint idThread)
		=> throw new NotImplementedException();

		internal override int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl)
		=> throw new NotImplementedException();
	}
}
#endif