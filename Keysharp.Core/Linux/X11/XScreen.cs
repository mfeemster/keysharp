#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XScreen
	{
		internal IntPtr ext_data;
		internal IntPtr display;
		internal IntPtr root;
		internal int width;
		internal int height;
		internal int mwidth;
		internal int mheight;
		internal int ndepths;
		internal IntPtr depths;
		internal int root_depth;
		internal IntPtr root_visual;
		internal IntPtr default_gc;
		internal IntPtr cmap;
		internal IntPtr white_pixel;
		internal IntPtr black_pixel;
		internal int max_maps;
		internal int min_maps;
		internal int backing_store;
		internal bool save_unders;
		internal IntPtr root_input_mask;
	}
}
#endif