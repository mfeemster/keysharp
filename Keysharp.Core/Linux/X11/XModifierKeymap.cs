#if LINUX
namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XModifierKeymap
	{
		internal int max_keypermod;
		internal nint modifiermap;
	}
}
#endif