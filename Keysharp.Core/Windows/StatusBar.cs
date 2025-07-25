﻿#if WINDOWS
namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Concrete implementation of StatusBar for the Windows platfrom.
	/// Helper class to get status bar info from a window in another process.
	/// Gotten from: http://www.pinvoke.net/default.aspx/user32/SB_GETTEXT.html and changed to use SendMessageTimeout().
	/// </summary>
	internal class StatusBar : StatusBarBase
	{
		internal StatusBar(nint hWnd)
			: base(hWnd)
		{
		}

		//May need to add wait functionality here the way AHK does in StatusBarUtil().
		protected override string GetCaption(uint index)
		{
			//var length = WindowsAPI.SendMessage(_handle, WindowsAPI.SB_GETTEXTLENGTH, index, 0);
			_ = WindowsAPI.SendMessageTimeout(handle, WindowsAPI.SB_GETTEXTLENGTH, (nint)index, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out var result);
			var length = result.ToInt32();

			if (length == 0)
				return DefaultObject;

			// Low part is the count. High part is the window type. Mask out the high bits.
			// The returned text will also be unicode so double the length to accomodate our buffer
			length = (length & 0x0000ffff) * 2;
			nint hProcess = 0;
			nint allocated = 0;
			KeysharpException ksexc = null;

			try
			{
				hProcess = WindowsAPI.OpenProcess(ProcessAccessTypes.PROCESS_ALL_ACCESS, false, OwningPID);

				if (hProcess != 0)
				{
					// Allocate memory in the remote process
					allocated = WindowsAPI.VirtualAllocEx(hProcess, 0, (uint)length, (VirtualAllocExTypes.MEM_COMMIT_OR_RESERVE), AccessProtectionFlags.PAGE_READWRITE);

					if (allocated != 0)
					{
						var buffer = new byte[length];
						// SB_GETTEXT tells the remote process to write out text to the remote memory we allocated.
						//_ = WindowsAPI.SendMessage(_handle, WindowsAPI.SB_GETTEXT, (nint)index, allocated);
						_ = WindowsAPI.SendMessageTimeout(handle, WindowsAPI.SB_GETTEXT, (nint)index, allocated, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out result);
						// Now we need to read that memory from the remote process into a local buffer.
						var success = WindowsAPI.ReadProcessMemory(hProcess, allocated, buffer, (uint)length, out var bytesRead);

						if (success)
						{
							// Each char takes 2 bytes.
							var characters = new char[length / 2];

							for (var i = 0; i < buffer.Length; i += 2)
							{
								// Even though the second byte will probably always be 0 for en-us let's do a bit shift
								// then "or" the first and second bytes together before casting to char.
								var a = (uint)buffer[i];
								var b = (uint)buffer[i + 1] << 8;
								characters[i / 2] = (char)(a | b);
							}

							return new string(characters);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ksexc = new KeysharpException(new OSError
				{
					Extra = ex.Message
				});
			}
			finally
			{
				if (hProcess != 0)
				{
					if (allocated != 0)
						_ = WindowsAPI.VirtualFreeEx(hProcess, allocated, 0, VirtualAllocExTypes.MEM_RELEASE);

					_ = WindowsAPI.CloseHandle(hProcess);
				}
			}

			if (ksexc != null)
				throw ksexc;

			return DefaultObject;
		}

		/// <summary>
		/// Might be able to make this common by abstracting thread ids.//TODO
		/// </summary>
		/// <returns></returns>
		protected override uint GetOwningPid()
		{
			uint ownpid = 0;

			if (handle != 0)
				_ = WindowsAPI.GetWindowThreadProcessId(handle, out ownpid);

			return ownpid;
		}

		protected override int GetPanelCount()
		{
			if (handle != 0)
			{
				_ = WindowsAPI.SendMessageTimeout(handle, WindowsAPI.SB_GETPARTS, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out var val);
				return val.ToInt32();
			}

			return 0;
		}
	}
}

#endif