#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[Flags]
	internal enum GCFunction
	{
		GCFunction = 1 << 0,
		GCPlaneMask = 1 << 1,
		GCForeground = 1 << 2,
		GCBackground = 1 << 3,
		GCLineWidth = 1 << 4,
		GCLineStyle = 1 << 5,
		GCCapStyle = 1 << 6,
		GCJoinStyle = 1 << 7,
		GCFillStyle = 1 << 8,
		GCFillRule = 1 << 9,
		GCTile = 1 << 10,
		GCStipple = 1 << 11,
		GCTileStipXOrigin = 1 << 12,
		GCTileStipYOrigin = 1 << 13,
		GCFont = 1 << 14,
		GCSubwindowMode = 1 << 15,
		GCGraphicsExposures = 1 << 16,
		GCClipXOrigin = 1 << 17,
		GCClipYOrigin = 1 << 18,
		GCClipMask = 1 << 19,
		GCDashOffset = 1 << 20,
		GCDashList = 1 << 21,
		GCArcMode = 1 << 22
	}

	internal enum GCJoinStyle
	{
		JoinMiter = 0,
		JoinRound = 1,
		JoinBevel = 2
	}

	internal enum GCLineStyle
	{
		LineSolid = 0,
		LineOnOffDash = 1,
		LineDoubleDash = 2
	}

	internal enum GCCapStyle
	{
		CapNotLast = 0,
		CapButt = 1,
		CapRound = 2,
		CapProjecting = 3
	}

	internal enum GCFillStyle
	{
		FillSolid = 0,
		FillTiled = 1,
		FillStippled = 2,
		FillOpaqueStppled = 3
	}

	internal enum GCFillRule
	{
		EvenOddRule = 0,
		WindingRule = 1
	}

	internal enum GCArcMode
	{
		ArcChord = 0,
		ArcPieSlice = 1
	}

	internal enum GCSubwindowMode
	{
		ClipByChildren = 0,
		IncludeInferiors = 1
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XGCValues
	{
		internal GXFunction function;
		internal nint plane_mask;
		internal nint foreground;
		internal nint background;
		internal int line_width;
		internal GCLineStyle line_style;
		internal GCCapStyle cap_style;
		internal GCJoinStyle join_style;
		internal GCFillStyle fill_style;
		internal GCFillRule fill_rule;
		internal GCArcMode arc_mode;
		internal nint tile;
		internal nint stipple;
		internal int ts_x_origin;
		internal int ts_y_origin;
		internal nint font;
		internal GCSubwindowMode subwindow_mode;
		internal bool graphics_exposures;
		internal int clip_x_origin;
		internal int clib_y_origin;
		internal nint clip_mask;
		internal int dash_offset;
		internal byte dashes;
	}

	internal enum GXFunction
	{
		GXclear = 0x0,        /* 0 */
		GXand = 0x1,        /* src AND dst */
		GXandReverse = 0x2,        /* src AND NOT dst */
		GXcopy = 0x3,        /* src */
		GXandInverted = 0x4,        /* NOT src AND dst */
		GXnoop = 0x5,        /* dst */
		GXxor = 0x6,        /* src XOR dst */
		GXor = 0x7,        /* src OR dst */
		GXnor = 0x8,        /* NOT src AND NOT dst */
		GXequiv = 0x9,        /* NOT src XOR dst */
		GXinvert = 0xa,        /* NOT dst */
		GXorReverse = 0xb,        /* src OR NOT dst */
		GXcopyInverted = 0xc,        /* NOT src */
		GXorInverted = 0xd,        /* NOT src OR dst */
		GXnand = 0xe,        /* NOT src OR NOT dst */
		GXset = 0xf        /* 1 */
	}

}
#endif