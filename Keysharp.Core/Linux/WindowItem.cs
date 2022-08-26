using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core.Common.Window;
using Keysharp.Core.Linux.Proxies;
using Keysharp.Core.Linux.X11;
using Keysharp.Core.Linux.X11.Types;
using Keysharp.Core.Windows;//Code in Linux probably shouldn't be referencing windows specific code.//MATT

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Represents a window under the Linux platform.
	/// </summary>
	internal class WindowItem : Common.Window.WindowItemBase//Do we want to prefix each of these derived classes with Windws/Linux?//MATT
	{
		private XWindow _xwindow = null;

		internal override bool Active
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal override bool AlwaysOnTop
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal override bool Bottom
		{
			set => throw new NotImplementedException();
		}

		internal override List<WindowItemBase> ChildWindows => throw new NotImplementedException();

		internal override string ClassName => throw new NotImplementedException();

		internal override string ClassNN => throw new NotImplementedException();

		internal override Rectangle ClientLocation
		{
			get
			{
				var attr = _xwindow.Attributes;
				return new Rectangle(attr.x, attr.y, attr.width, attr.height);//These need to be client values, unsure how.//MATT
			}
		}

		internal override bool Enabled
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal override bool Exists => throw new NotImplementedException();

		internal override long ExStyle
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal override bool IsHung => throw new NotImplementedException();

		internal override Rectangle Location
		{
			get
			{
				var attr = _xwindow.Attributes;
				return new Rectangle(attr.x, attr.y, attr.width, attr.height);
			}

			set => throw new NotImplementedException();
		}

		internal override WindowItemBase ParentWindow => throw new NotImplementedException();

		internal override IntPtr PID => throw new NotImplementedException();

		internal override WindowItemBase PreviousWindow => throw new NotImplementedException();

		internal override Size Size
		{
			get
			{
				var attr = _xwindow.Attributes;
				return new Size(attr.width, attr.height);
			}

			set => throw new NotImplementedException();
		}

		internal override long Style
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal override string[] Text => throw new NotImplementedException();

		internal override string Title
		{
			get
			{
				var prop = new XTextProperty();
				_ = Xlib.XGetTextProperty(_xwindow.XDisplay.Handle, _xwindow.ID, ref prop, XAtom.XA_WM_NAME);
				return prop.GetText();
			}
			set
			{
				var prop = new XTextProperty();
				_ = prop.SetText(value);
				Xlib.XSetTextProperty(_xwindow.XDisplay.Handle, _xwindow.ID, ref prop, XAtom.XA_WM_NAME);
			}
		}

		internal override object Transparency
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal override object TransparentColor
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal override bool Visible
		{
			get => false;
			set => _ = value ? Show() : Hide();
		}

		internal override FormWindowState WindowState
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal WindowItem(XWindow uxwindow)
			: this(new IntPtr(uxwindow.ID)) => _xwindow = uxwindow;

		private WindowItem(IntPtr handle) : base(handle)
		{
		}

		internal override bool Close() => throw new NotImplementedException();

		internal override uint GetMenuItemId(params string[] items) => throw new NotImplementedException();

		internal override bool Hide() => throw new NotImplementedException();

		internal override bool Kill() => throw new NotImplementedException();

		internal override WindowItemBase RealChildWindowFromPoint(System.Drawing.Point location) => throw new NotImplementedException();

		internal override bool Redraw() => throw new NotImplementedException();

		internal override void SendMouseEvent(MOUSEEVENTF mouseevent, System.Drawing.Point? location = null) => throw new NotImplementedException();

		internal override void SetTransparency(byte level, System.Drawing.Color color) => throw new NotImplementedException();

		internal override bool Show() => throw new NotImplementedException();
	}
}