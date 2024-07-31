#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XTextProperty
	{
		internal IntPtr value;
		internal IntPtr encoding;
		internal int format;
		internal IntPtr nitems;

		internal unsafe string GetText() => Marshal.PtrToStringAuto(value);
		//{
		//  var strings = new byte[1024];
		//  Keysharp.Scripting.Script.OutputDebug($"GetText() ptr: {value.ToInt64()}, format: {format}, nitems: {nitems.ToInt64()}.");

		//  if (Xlib.XTextPropertyToStringList(value, ref strings, out int countReturn) != 0)
		//  {
		//      if (countReturn > 0)
		//      {
		//          fixed (void* ptr = strings)
		//          {
		//              return Marshal.PtrToStringUTF8(new IntPtr(ptr));
		//          }
		//      }
		//  }
		//  else
		//      Keysharp.Scripting.Script.OutputDebug($"XTextPropertyToStringList() failed.");

		//  return "";
		//}

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