#if LINUX
namespace Keysharp.Core.Linux.X11.Types
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XTextProperty
	{
		internal IntPtr value;
		internal IntPtr encoding;
		internal int format;
		internal IntPtr nitems;

		internal string GetText() => Marshal.PtrToStringAnsi(value);

		internal bool SetText(string value)
		{
			if (value == null)
			{
				value = string.Empty;
			}

			var str = Marshal.StringToHGlobalAnsi(value);

			if (str == IntPtr.Zero)
			{
				return false;
			}

			if (Xlib.XStringListToTextProperty(ref str, 1, ref this) == 0)
			{
				Marshal.FreeHGlobal(str);
				return false;
			}

			Marshal.FreeHGlobal(str);
			return true;
		}

		/// <summary>
		/// Free the text
		/// </summary>
		internal void Free()
		{
			if (value != IntPtr.Zero)
			{
				_ = Xlib.XFree(value);
				value = IntPtr.Zero;
			}
		}
	}
}
#endif