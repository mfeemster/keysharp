#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XErrorEvent
	{
		internal XEventName type;
		internal nint display;
		internal nint resourceid;
		internal nint serial;
		internal byte error_code;
		internal XRequest request_code;
		internal byte minor_code;
	}
}
#endif