#if LINUX
namespace Keysharp.Core.Linux.X11.Events
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XTimerNotifyEvent
	{
		internal XEventName type;
		internal EventHandler handler;
	}
}
#endif