#if LINUX

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of PlatformManager for the linux platfrom.
	/// </summary>
	internal class PlatformManager : PlatformManagerBase
	{
		private readonly bool isGnome, isKde, isXfce, isMate, isCinnamon, isLxqt, isLxde;

		internal bool IsGnome => isGnome;
		internal bool IsKde => isKde;
		internal bool IsXfce => isXfce;
		internal bool IsMate => isMate;
		internal bool IsCinnamon => isCinnamon;
		internal bool IsLxqt => isLxqt;
		internal bool IsLxde => isLxde;

		internal PlatformManager()
		{
			var session = "echo $DESKTOP_SESSION".Bash().ToLower();

			if (session.Contains("gnome", StringComparison.OrdinalIgnoreCase))
				isGnome = true;
			else if (session.Contains("kde", StringComparison.OrdinalIgnoreCase))
				isKde = true;
			else if (session.Contains("xfce", StringComparison.OrdinalIgnoreCase))
				isXfce = true;
			else if (session.Contains("mate", StringComparison.OrdinalIgnoreCase))
				isMate = true;
			else if (session.Contains("cinnamon", StringComparison.OrdinalIgnoreCase))
				isCinnamon = true;
			else if (session.Contains("lxqt", StringComparison.OrdinalIgnoreCase))
				isLxqt = true;
			else if (session.Contains("lxde", StringComparison.OrdinalIgnoreCase))
				isLxde = true;
			else
				isGnome = true;//Assume Gnome if no other DE was found.
		}

		internal override nint GetKeyboardLayout(uint idThread)
		=> throw new NotImplementedException();

		internal override int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out] char[] pwszBuff, int cchBuff, uint wFlags, nint dwhkl)
		=> throw new NotImplementedException();

		internal override bool SetDllDirectory(string path)
		{
			if (path == null)
			{
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", Script.TheScript.ldLibraryPath);
				return Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") == Script.TheScript.ldLibraryPath;
			}
			else
			{
				var append = path;
				var orig = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? "";
				var newPath = "";

				if (orig != "")
				{
					append = ":" + append;
					newPath = orig + append;
					//newPath = "\"" + orig + "\"" + append;//Unsure if quotes are needed.
				}
				else
					newPath = append;

				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", newPath);
				return Environment.GetEnvironmentVariable("LD_LIBRARY_PATH").EndsWith(append);
			}
		}

		internal override nint LoadLibrary(string path) => Xlib.dlopen(path, Xlib.RTLD_LAZY);//Assume lazy is more efficient. Use RTLD_NOW if this doesn't work.

		internal override uint CurrentThreadId() => (uint)Xlib.gettid();

		internal override bool DestroyIcon(nint icon) => Xlib.GdipDisposeImage(icon) == 0;//Unsure if this works or is even needed on linux.

		internal override bool ExitProgram(uint flags, uint reason)
		{
			var cmd = "";
			var force = false;

			//Taken from this article: https://fostips.com/log-out-command-linux-desktops/
			if ((flags & 4) == 4)
			{
				force = true;//Close all programs.
			}

			if (flags == 0)//Logoff.
			{
				if (isGnome)
				{
					if (force)
						cmd = "gnome-session-quit --force";
					else
						cmd = "gnome-session-quit";
				}
				else if (isKde)
				{
					if (force)
						cmd = "qdbus org.kde.ksmserver /KSMServer logout 0 0 2";
					else
						cmd = "qdbus org.kde.ksmserver /KSMServer logout 1 0 3";
				}
				else if (isXfce)
				{
					if (force)
						cmd = "xfce4-session-logout --fast";
					else
						cmd = "xfce4-session-logout";
				}
				else if (isMate)
				{
					if (force)
						cmd = "mate-session-save --logout --force";
					else
						cmd = "mate-session-save --logout";
				}
				else if (isCinnamon)
				{
					if (force)
						cmd = "cinnamon-session-quit --no-prompt";
					else
						cmd = "cinnamon-session-quit";
				}
				else if (IsLxqt)
				{
					if (force)
						KeysharpEnhancements.OutputDebugLine($"LXQT doesn't support forced logouts.");

					cmd = "lxqt-leave";
				}
				else if (IsLxde)
				{
					if (force)
						KeysharpEnhancements.OutputDebugLine($"LXDE doesn't support forced logouts.");

					cmd = "lxde-logout";
				}

				_ = cmd.Bash();
			}
			else if ((flags & 1) == 1)//Halt/shutdown.
			{
				if ((flags & 8) == 8)//Power down.
					_ = "shutdown now".Bash();
				else
					_ = "halt".Bash();
			}
			else if ((flags & 2) == 2)//Reboot.
			{
				if (force)
					_ = "reboot -f".Bash();
				else
					_ = "reboot".Bash();
			}
			else if ((flags & 8) == 8)//Shutdown.
			{
				_ = "shutdown now".Bash();
			}

			return true;
		}

		internal override bool UnregisterHotKey(nint hWnd, uint id) => true;

		internal override bool PostMessage(nint hWnd, uint msg, nint wParam, nint lParam) => true;

		internal override bool PostMessage(nint hWnd, uint msg, uint wParam, uint lParam) => true;

		internal override bool PostHotkeyMessage(nint hWnd, uint wParam, uint lParam) => true;

		internal override bool RegisterHotKey(nint hWnd, uint id, KeyModifiers fsModifiers, uint vk) => true;
	}
}
#endif