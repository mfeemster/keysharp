using System;
using System.Runtime.InteropServices;

namespace Keysharp.Core.Linux.X11
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XModifierKeymap
	{
		internal int max_keypermod;
		internal IntPtr modifiermap;
	}
}