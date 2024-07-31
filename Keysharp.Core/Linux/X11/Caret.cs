#if LINUX
#pragma warning disable 649

namespace Keysharp.Core.Linux.X11
{
	internal struct CaretStruct
	{
		internal System.Windows.Forms.Timer Timert;  // Blink interval
		internal IntPtr Hwnd;                // Window owning the caret
		internal IntPtr Window;                // Actual X11 handle of the window
		internal int X;                // X position of the caret
		internal int Y;                // Y position of the caret
		internal int Width;                // Width of the caret; if no image used
		internal int Height;                // Height of the caret, if no image used
		internal bool Visible;            // Is caret visible?
		internal bool On;                // Caret blink display state: On/Off
		internal IntPtr gc;                // Graphics context
		internal bool Paused;                // Don't update right now
	}
}
#endif