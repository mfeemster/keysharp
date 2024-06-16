#if WINDOWS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Core.Common.Platform;

namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Concrete implementation of PlatformManager for the Windows platfrom.
	/// </summary>
	internal class PlatformManager : PlatformManagerBase
	{
		internal override IntPtr GetKeyboardLayout(uint idThread)
		=> WindowsAPI.GetKeyboardLayout(idThread);

		internal override int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl)
		=> WindowsAPI.ToUnicodeEx(wVirtKey, wScanCode, lpKeyState, pwszBuff, cchBuff, wFlags, dwhkl);
	}
}
#endif