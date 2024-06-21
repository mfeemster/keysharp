#if LINUX
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

		internal override bool SetDllDirectory(string path) => throw new NotImplementedException();

		internal override IntPtr LoadLibrary(string path) => throw new NotImplementedException();

		internal override int GetModuleHandleEx(uint flags, string moduleName, out IntPtr module) => throw new NotImplementedException();

		internal override uint CurrentThreadId() => Keysharp.Core.Windows.WindowsAPI.GetCurrentThreadId() => throw new NotImplementedException();

		internal override bool DestroyIcon(IntPtr icon) => true;

		internal override bool ExitProgram(uint flags, uint reason) => true;

	}
}
#endif