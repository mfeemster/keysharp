#if LINUX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Platform;

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

		internal override uint CurrentThreadId() => throw new NotImplementedException();

		internal override bool DestroyIcon(IntPtr icon) => true;

		internal override bool ExitProgram(uint flags, uint reason) => true;

		internal override bool UnregisterHotKey(IntPtr hWnd, uint id) => true;

		internal override bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) => true;

		internal override bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam) => true;

		internal override bool PostHotkeyMessage(IntPtr hWnd, uint wParam, uint lParam) => true;

		internal override bool RegisterHotKey(IntPtr hWnd, uint id, KeyModifiers fsModifiers, uint vk) => true;
	}
}
#endif