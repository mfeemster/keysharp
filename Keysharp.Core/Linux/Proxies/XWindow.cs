#if LINUX
namespace Keysharp.Core.Linux.Proxies
{
	/// <summary>
	/// represents a single xwindow - proxy for actions affecting x windows
	/// </summary>
	internal class XWindow : IComparable<XWindow>
	{
		private XWindowAttributes _attributes;

		internal XWindowAttributes Attributes
		{
			get
			{
				if (Xlib.XGetWindowAttributes(XDisplay.Handle, ID, ref _attributes) == 0)
					throw new XWindowException();

				return _attributes;
			}
		}

		/// <summary>
		/// ID of the window
		/// </summary>
		internal long ID { get; set; }

		/// <summary>
		/// Backreference to the XDisplay from this Window
		/// </summary>
		internal XDisplay XDisplay { get; } = null;

		internal XWindow(XDisplay display, long window)
		{
			XDisplay = display;
			ID = window;
		}

		public XWindow()
		{
		}

		int IComparable<XWindow>.CompareTo(XWindow other) => ID.CompareTo(other.ID);
	}

	internal class XWindowException : Exception
	{
		//
	}
}

#endif