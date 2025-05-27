#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal struct XColor
	{
		internal nint pixel;
		internal ushort red;
		internal ushort green;
		internal ushort blue;
		internal byte flags;
		internal byte pad;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XStandardColormap
	{
		internal nint colormap;
		internal nint red_max;
		internal nint red_mult;
		internal nint green_max;
		internal nint green_mult;
		internal nint blue_max;
		internal nint blue_mult;
		internal nint base_pixel;
		internal nint visualid;
		internal nint killid;
	}

	[Flags]
	internal enum ColorFlags
	{
		DoRed = 1 << 0,
		DoGreen = 1 << 1,
		DoBlue = 1 << 2
	}
}
#endif