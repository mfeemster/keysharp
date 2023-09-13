using System;
using Keysharp.Core.Linux.X11;
using Keysharp.Core.Linux.X11.Types;

namespace Keysharp.Core.Linux.Proxies
{
	/// <summary>
	/// represents a single xwindow - proxy for actions affecting x windows
	/// </summary>
	internal class XWindow
	{
		private XWindowAttributes _attributes;

		internal XWindowAttributes Attributes
		{
			get
			{
				if (Xlib.XGetWindowAttributes(XDisplay.Handle, ID, ref _attributes) == 0)
				{
					throw new XWindowException();
				}

				return _attributes;
			}
		}

		/// <summary>
		/// ID of the window
		/// </summary>
		internal int ID { get; set; }

		/// <summary>
		/// Backreference to the XDisplay from this Window
		/// </summary>
		internal XDisplay XDisplay { get; } = null;

		internal XWindow(XDisplay display, int window)
		{
			XDisplay = display;
			ID = window;
		}
	}

	internal class XWindowException : Exception
	{
		//
	}
}