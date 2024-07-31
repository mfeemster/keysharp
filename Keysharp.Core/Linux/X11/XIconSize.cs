#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XIconSize
	{
		internal int min_width;
		internal int min_height;
		internal int max_width;
		internal int max_height;
		internal int width_inc;
		internal int height_inc;
	}
}
#endif