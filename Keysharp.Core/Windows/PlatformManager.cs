#if WINDOWS
namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Concrete implementation of PlatformManager for the Windows platfrom.
	/// </summary>
	internal class PlatformManager : PlatformManagerBase
	{
		internal override uint CurrentThreadId() => WindowsAPI.GetCurrentThreadId();

		internal override bool DestroyIcon(nint icon) => WindowsAPI.DestroyIcon(icon);

		internal override bool ExitProgram(uint flags, uint reason) => WindowsAPI.ExitWindowsEx(flags, reason);

		internal override nint GetKeyboardLayout(uint idThread)
		=> WindowsAPI.GetKeyboardLayout(idThread);

		internal override nint LoadLibrary(string path) => WindowsAPI.LoadLibrary(path);

		internal override bool PostHotkeyMessage(nint hWnd, uint wParam, uint lParam) => WindowsAPI.PostMessage(hWnd, WindowsAPI.WM_HOTKEY, wParam, lParam);

		internal override bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam) => WindowsAPI.PostMessage(hWnd, msg, wParam, lParam);

		internal override bool PostMessage(nint hWnd, uint msg, uint wParam, uint lParam) => WindowsAPI.PostMessage(hWnd, msg, wParam, lParam);

		internal override bool RegisterHotKey(nint hWnd, uint id, KeyModifiers fsModifiers, uint vk) => WindowsAPI.RegisterHotKey(hWnd, id, fsModifiers, vk);

		internal override bool SetDllDirectory(string path) => WindowsAPI.SetDllDirectory(path);

		internal override int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out] char[] pwszBuff, int cchBuff, uint wFlags, nint dwhkl)
		=> WindowsAPI.ToUnicodeEx(wVirtKey, wScanCode, lpKeyState, pwszBuff, cchBuff, wFlags, dwhkl);

		internal override bool UnregisterHotKey(nint hWnd, uint id) => WindowsAPI.UnregisterHotKey(hWnd, id);
	}
}

#endif