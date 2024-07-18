#if WINDOWS
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

		internal override bool SetDllDirectory(string path) => WindowsAPI.SetDllDirectory(path);

		internal override IntPtr LoadLibrary(string path) => WindowsAPI.LoadLibrary(path);

		internal override int GetModuleHandleEx(uint flags, string moduleName, out IntPtr module) =>
		WindowsAPI.GetModuleHandleEx(flags, moduleName, out module);

		internal override uint CurrentThreadId() => Keysharp.Core.Windows.WindowsAPI.GetCurrentThreadId();

		internal override bool DestroyIcon(IntPtr icon) => WindowsAPI.DestroyIcon(icon);

		internal override bool ExitProgram(uint flags, uint reason) => WindowsAPI.ExitWindowsEx(flags, reason);

		internal override bool UnregisterHotKey(IntPtr hWnd, uint id) => WindowsAPI.UnregisterHotKey(hWnd, id);

		internal override bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) => WindowsAPI.PostMessage(hWnd, msg, wParam, lParam);

		internal override bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam) => WindowsAPI.PostMessage(hWnd, msg, wParam, lParam);

		internal override bool PostHotkeyMessage(IntPtr hWnd, uint wParam, uint lParam) => WindowsAPI.PostMessage(hWnd, WindowsAPI.WM_HOTKEY, wParam, lParam);

		internal override bool RegisterHotKey(IntPtr hWnd, uint id, KeyModifiers fsModifiers, uint vk) => WindowsAPI.RegisterHotKey(hWnd, id, fsModifiers, vk);
	}
}
#endif