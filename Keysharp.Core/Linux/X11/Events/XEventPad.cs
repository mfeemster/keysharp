#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XEventPad
	{
		internal nint pad0;
		internal nint pad1;
		internal nint pad2;
		internal nint pad3;
		internal nint pad4;
		internal nint pad5;
		internal nint pad6;
		internal nint pad7;
		internal nint pad8;
		internal nint pad9;
		internal nint pad10;
		internal nint pad11;
		internal nint pad12;
		internal nint pad13;
		internal nint pad14;
		internal nint pad15;
		internal nint pad16;
		internal nint pad17;
		internal nint pad18;
		internal nint pad19;
		internal nint pad20;
		internal nint pad21;
		internal nint pad22;
		internal nint pad23;
	}
}
#endif