#if LINUX
using System;

namespace Keysharp.Core.Linux.X11
{
	[Flags]
	internal enum SetWindowValuemask
	{
		Nothing = 0,
		BackPixmap = 1,
		BackPixel = 2,
		BorderPixmap = 4,
		BorderPixel = 8,
		BitGravity = 16,
		WinGravity = 32,
		BackingStore = 64,
		BackingPlanes = 128,
		BackingPixel = 256,
		OverrideRedirect = 512,
		SaveUnder = 1024,
		EventMask = 2048,
		DontPropagate = 4096,
		ColorMap = 8192,
		Cursor = 16384
	}
}
#endif