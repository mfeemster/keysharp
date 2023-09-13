using System;

namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Helper class to get status bar info from a window in another process.
	/// Gotten from: http://www.pinvoke.net/default.aspx/user32/SB_GETTEXT.html and changed to use SendMessageTimeout().
	/// </summary>
	internal class StatusBar
	{
		private const int timeout = 2000;
		private string[] captions;
		private IntPtr handle;
		private int panelCount;
		private int pid;

		internal string Caption
		{
			get => string.Join(" | ", Captions);
			set => SetCaptions(-1, value);
		}

		internal string[] Captions
		{
			get
			{
				if (captions == null)
					captions = GetCaptions();

				return captions;
			}
		}

		internal int OwningPID
		{
			get
			{
				if (pid == -1)
					pid = GetOwningPid();

				return pid;
			}
		}

		internal int PanelCount
		{
			get
			{
				if (panelCount == -1)
					panelCount = GetPanelCount();

				return panelCount;
			}
		}

		internal StatusBar(IntPtr hWnd)
		{
			handle = hWnd;
			panelCount = -1;
			pid = -1;
		}

		internal void SetCaptions(int index, string caption)
		{
			if (index == -1)
			{
				var oldParts = Captions;
				var newParts = caption.Split(new string[] { " | " }, StringSplitOptions.None);

				if ((oldParts.Length == newParts.Length) && (newParts.Length > 0))
				{
					for (var i = 0; i < oldParts.Length; i++)
					{
						if (oldParts[i] != newParts[i])
							SetCaption(i, newParts[i]);
					}
				}
			}
			else
			{
				SetCaption(index, caption);
			}
		}

		//May need to add wait functionality here the way AHK does in StatusBarUtil().
		private string GetCaption(uint index)
		{
			//var length = WindowsAPI.SendMessage(_handle, WindowsAPI.SB_GETTEXTLENGTH, index, 0);
			_ = WindowsAPI.SendMessageTimeout(handle, WindowsAPI.SB_GETTEXTLENGTH, index, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out var result);
			var length = result.ToInt32();

			if (length == 0)
				return "";

			// Low part is the count. High part is the window type. Mask out the high bits.
			// The returned text will also be unicode so double the length to accomodate our buffer
			length = (length & 0x0000ffff) * 2;
			var hProcess = IntPtr.Zero;
			var allocated = IntPtr.Zero;
			KeysharpException ksexc = null;

			try
			{
				hProcess = WindowsAPI.OpenProcess(ProcessAccessTypes.PROCESS_ALL_ACCESS, false, OwningPID);

				if (hProcess != IntPtr.Zero)
				{
					// Allocate memory in the remote process
					allocated = WindowsAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)length, (VirtualAllocExTypes.MEM_COMMIT_OR_RESERVE), AccessProtectionFlags.PAGE_READWRITE);

					if (allocated != IntPtr.Zero)
					{
						var buffer = new byte[length];
						// SB_GETTEXT tells the remote process to write out text to the remote memory we allocated.
						//_ = WindowsAPI.SendMessage(_handle, WindowsAPI.SB_GETTEXT, (IntPtr)index, allocated);
						_ = WindowsAPI.SendMessageTimeout(handle, WindowsAPI.SB_GETTEXT, index, allocated, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out result);
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
				if (hProcess != IntPtr.Zero)
				{
					if (allocated != IntPtr.Zero)
						_ = WindowsAPI.VirtualFreeEx(hProcess, allocated, 0, VirtualAllocExTypes.MEM_RELEASE);

					_ = WindowsAPI.CloseHandle(hProcess);
				}
			}

			if (ksexc != null)
				throw ksexc;

			return string.Empty;
		}

		private string[] GetCaptions()
		{
			var count = PanelCount;
			var caps = new string[count];

			for (uint i = 0; i < count; i++)
				caps[i] = GetCaption(i);

			return caps;
		}

		private int GetOwningPid()
		{
			var ownpid = IntPtr.Zero;

			if (handle != IntPtr.Zero)
				_ = WindowsAPI.GetWindowThreadProcessId(handle, out ownpid);

			return ownpid.ToInt32();
		}

		private int GetPanelCount()
		{
			if (handle != IntPtr.Zero)
			{
				_ = WindowsAPI.SendMessageTimeout(handle, WindowsAPI.SB_GETPARTS, 0, 0, SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, timeout, out var val);
				return val.ToInt32();
			}

			return 0;
		}

		private void SetCaption(int index, string caption) => throw new NotImplementedException("Sorry... You'll have to figure out SB_SETTEXT.");
	}
}