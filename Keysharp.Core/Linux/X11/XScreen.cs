#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XScreen
	{
		internal nint ext_data;
		internal nint display;
		internal nint root;
		internal int width;
		internal int height;
		internal int mwidth;
		internal int mheight;
		internal int ndepths;
		internal nint depths;
		internal int root_depth;
		internal nint root_visual;
		internal nint default_gc;
		internal nint cmap;
		internal nint white_pixel;
		internal nint black_pixel;
		internal int max_maps;
		internal int min_maps;
		internal int backing_store;
		internal bool save_unders;
		internal nint root_input_mask;
	}
}
#endif