#if LINUX

using static Keysharp.Core.Linux.LinuxAPI;

namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of WindowItem for the linux platfrom.
	/// Always attempt to interact with the underlying window using Winforms before dropping down to the raw X11 API
	/// because our custom implementation of Winforms does a lot of extra work under the hood before and after it makes
	/// raw API calls.
	/// </summary>
	internal class WindowItem : Common.Window.WindowItemBase//Do we want to prefix each of these derived classes with Windws/Linux?//TODO
	{
		private XWindow _xwindow = null;
		private WindowManager manager = (Keysharp.Core.Linux.WindowManager)WindowProvider.Manager;

		internal override bool Active
		{
			get
			{
				if (IsSpecified && manager.ActiveWindow is WindowItem item)
				{
					//Keysharp.Scripting.Script.OutputDebug($"item.Handle: {item.Handle.ToInt64()}, item.Title: {item.Title}, Handle: {Handle.ToInt64()}, Title: {Title}");
					//Keysharp.Core.File.FileAppend($"item.Handle: {item.Handle.ToInt64()}, item.Title: {item.Title}, Handle: {Handle.ToInt64()}, Title: {Title}\n", "out.txt");
					if (item.Handle.ToInt64() == Handle.ToInt64())
						return true;
				}

				return false;
			}
			set
			{
				if (IsSpecified)
				{
					if (manager.ActiveWindow.Handle.ToInt64() != Handle.ToInt64())
					{
						if (IsIconic)
						{
							WindowState = FormWindowState.Normal;
						}
						else
						{
							lock (WindowManager.xLibLock)
							{
								manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_ACTIVE_WINDOW, 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
							}
						}
					}
				}
			}
		}

		internal override bool AlwaysOnTop
		{
			get
			{
				if (!IsSpecified)
					return false;

				var onTop = false;
				ReadStateProps((atom) =>
				{
					if (atom == _xwindow.XDisplay._NET_WM_STATE_ABOVE)
					{
						onTop = true;
						return false;
					}
					else
						return true;
				});
				return onTop;
			}
			set
			{
				manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_WM_STATE, value ? 1 : 0, _xwindow.XDisplay._NET_WM_STATE_ABOVE, IntPtr.Zero, IntPtr.Zero);
			}
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
				return new Rectangle(attr.x, attr.y, attr.width, attr.height);//These need to be client values, unsure how.//TODO
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

		internal override bool IsIconic => WindowState == FormWindowState.Minimized;

		internal override Rectangle Location
		{
			get
			{
				var attr = _xwindow.Attributes;
				return new Rectangle(attr.x, attr.y, attr.width, attr.height);
			}

			set => throw new NotImplementedException();
		}

		internal override string NetClassName => throw new NotImplementedException();

		internal override string NetClassNN => throw new NotImplementedException();

		internal override WindowItemBase NonChildParentWindow => throw new NotImplementedException();

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

		internal override List<string> Text
		{
			get
			{
				if (!IsSpecified)
					return [];

				var prop = new XTextProperty();
				var attr = new XWindowAttributes();
				var tv = Threads.GetThreadVariables();
				var doHidden = ThreadAccessors.A_DetectHiddenWindows;
				var filter = (long id) =>
				{
					if (Xlib.XGetWindowAttributes(_xwindow.XDisplay.Handle, id, ref attr) != 0)
						if (tv.detectHiddenText || attr.map_state == MapState.IsViewable)
							return true;

					return false;
				};
				return _xwindow.XDisplay.XQueryTreeRecursive(_xwindow, filter).Select(x =>
				{
					if (Xlib.XGetTextProperty(_xwindow.XDisplay.Handle, x.ID, ref prop, XAtom.XA_WM_NAME) != 0)
					{
						var text = prop.GetText();
						prop.Free();
						return text;
					}
					else
						return "";
				}).ToList();
			}
		}

		internal override string Title
		{
			get
			{
				if (!IsSpecified)
					return "";

				if (Control.FromHandle(Handle) is Control control)
					return control.Text;

				try
				{
					return Xlib.GetWMName(_xwindow.XDisplay.Handle, _xwindow.ID);
				}
				catch (Exception ex)
				{
					Keysharp.Scripting.Script.OutputDebug($"XGetWMName() failed: {ex.Message}");
				}

				return "";
			}
			set
			{
				if (IsSpecified)
				{
					if (Control.FromHandle(Handle) is Control control)
					{
						control.Text = value;
					}
					else
					{
						var prop = new XTextProperty();

						try
						{
							_ = prop.SetText(value);
							Xlib.XSetTextProperty(_xwindow.XDisplay.Handle, _xwindow.ID, ref prop, XAtom.XA_WM_NAME);
						}
						catch (Exception ex)
						{
							Keysharp.Scripting.Script.OutputDebug($"XSetTextProperty() failed: {ex.Message}");
						}
						finally
						{
							prop.Free();
						}
					}
				}
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
			get
			{
				if (!IsSpecified)
					return false;

				if (Control.FromHandle(Handle) is Control control)
					return control.Visible;

				return _xwindow.Attributes.map_state == MapState.IsViewable;
			}
			set
			{
				if (IsSpecified)
				{
					if (Control.FromHandle(Handle) is Control control)
					{
						control.Visible = value;
						return;
					}

					_ = value ? Show() : Hide();
				}
			}
		}

		internal override FormWindowState WindowState
		{
			get
			{
				if (!IsSpecified)
					return FormWindowState.Normal;

				if (Control.FromHandle(Handle) is Form form)
					return form.WindowState;

				var maximized = 0;
				var minimized = false;
				ReadStateProps((atom) =>
				{
					if ((atom == _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ) || (atom == _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT))
					{
						maximized++;
					}
					else if (atom == _xwindow.XDisplay._NET_WM_STATE_HIDDEN)
					{
						minimized = true;
					}

					return true;
				});

				if (minimized)
					return FormWindowState.Minimized;
				else if (maximized == 2)
					return FormWindowState.Maximized;
				else
					return FormWindowState.Normal;
			}
			set
			{
				if (Control.FromHandle(Handle) is Form form)
					form.WindowState = value;

				var current_state = WindowState;

				if (current_state == value)
				{
					//Keysharp.Scripting.Script.OutputDebug($"Window {current_state} == {value}, so doing nothing.");
					return;
				}

				//Got this logic from Winforms.
				switch (value)
				{
					case FormWindowState.Normal:
					{
						lock (WindowManager.xLibLock)
						{
							if (current_state == FormWindowState.Minimized)
							{
								_ = Xlib.XMapWindow(_xwindow.XDisplay.Handle, Handle);
								//Keysharp.Scripting.Script.OutputDebug($"Window was minimized, so setting to normal.");
								manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
							}
							else if (current_state == FormWindowState.Maximized)
							{
								//Keysharp.Scripting.Script.OutputDebug($"Window was maximized, so setting to normal.");
								manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_WM_STATE, 2 /* toggle */, _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ, _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT, IntPtr.Zero);
								manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
							}

							//else
							//  Keysharp.Scripting.Script.OutputDebug($"Window was {current_state}, so doing nothing.");
						}

						//Active = true;
						return;
					}

					case FormWindowState.Minimized:
					{
						lock (WindowManager.xLibLock)
						{
							if (current_state == FormWindowState.Maximized)
							{
								manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_WM_STATE, 2 /* toggle */, _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ, _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT, IntPtr.Zero);
							}

							_ = Xlib.XIconifyWindow(_xwindow.XDisplay.Handle, Handle.ToInt64(), _xwindow.XDisplay.ScreenNumber);
						}

						return;
					}

					case FormWindowState.Maximized:
					{
						lock (WindowManager.xLibLock)
						{
							if (current_state == FormWindowState.Minimized)
							{
								_ = Xlib.XMapWindow(_xwindow.XDisplay.Handle, Handle);
							}

							manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_WM_STATE, 1 /* Add */, _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ, _xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT, IntPtr.Zero);
						}

						manager.SendNetWMMessage(Handle, _xwindow.XDisplay._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
						return;
					}
				}
			}
		}

		internal WindowItem(XWindow uxwindow)
			: base(new IntPtr(uxwindow.ID))
		{
			_xwindow = uxwindow;
		}

		internal WindowItem(IntPtr handle)
			: this(new XWindow(XDisplay.Default, handle.ToInt64()))
		{
		}

		internal override void ChildFindPoint(PointAndHwnd pah) => throw new NotImplementedException();

		/// <summary>
		/// Left-Clicks on this window/control
		/// </summary>
		/// <param name="location"></param>
		internal override void Click(Point? location = null)
		{
			SendMouseEvent(XEventName.ButtonPress, EventMasks.ButtonPress, Buttons.Left, location);
			_ = Xlib.XFlush(_xwindow.XDisplay.Handle);
			//Might need a sleep here.
			SendMouseEvent(XEventName.ButtonRelease, EventMasks.ButtonRelease, Buttons.Left, location);
		}

		/// <summary>
		/// Right-Clicks on this window/control
		/// </summary>
		/// <param name="location"></param>
		internal override void ClickRight(Point? location = null)
		{
			SendMouseEvent(XEventName.ButtonPress, EventMasks.ButtonPress, Buttons.Right, location);//Assume button 2 is the right button.
			_ = Xlib.XFlush(_xwindow.XDisplay.Handle);
			//Might need a sleep here.
			SendMouseEvent(XEventName.ButtonRelease, EventMasks.ButtonRelease, Buttons.Right, location);
		}

		internal override System.Drawing.Point ClientToScreen() => throw new NotImplementedException();

		internal override void ClientToScreen(ref System.Drawing.Point pt) => throw new NotImplementedException();

		internal override bool Close()
		{
			if (!IsSpecified)
				return false;

			if (Control.FromHandle(Handle) is Form form)
			{
				if (form.Disposing || form.IsDisposed)
					return false;

				form.Close();
				return true;
			}

			var ev = new XEvent();
			ev.ClientMessageEvent.type = X11.Events.XEventName.ClientMessage;
			ev.ClientMessageEvent.window = Handle;
			ev.ClientMessageEvent.message_type = _xwindow.XDisplay.WM_PROTOCOLS;
			ev.ClientMessageEvent.format = 32;
			ev.ClientMessageEvent.ptr1 = _xwindow.XDisplay.WM_DELETE_WINDOW;
			//ev.ClientMessageEvent.data.l [1] = CurrentTime;
			return Xlib.XSendEvent(_xwindow.XDisplay.Handle, _xwindow.ID, false, EventMasks.NoEvent, ref ev) != 0;
		}

		internal override uint GetMenuItemId(params string[] items) => throw new NotImplementedException();

		internal override bool Hide()
		{
			if (!IsSpecified)
				return false;

			if (Control.FromHandle(Handle) is Control control)
			{
				control.Visible = false;
				return true;
			}

			return Xlib.XUnmapWindow(_xwindow.XDisplay.Handle, _xwindow.ID) != 0;
		}

		internal override bool Kill()
		{
			if (!IsSpecified)
				return false;

			_ = Close();
			var i = 0;

			while (Exists && i++ < 5)
			{
				if ((i & 1) == 1)
					System.Threading.Thread.Sleep(0);
				else
					System.Threading.Thread.Sleep(10);
			}

			if (!Exists)
				return true;

			_ = Xlib.XKillClient(_xwindow.XDisplay.Handle, _xwindow.ID);
			return !Exists;
		}

		internal bool ReadStateProps(Func<long, bool> func)
		{
			IntPtr prop = IntPtr.Zero;

			if (Xlib.XGetWindowProperty(_xwindow.XDisplay.Handle,
										_xwindow.ID,
										_xwindow.XDisplay._NET_WM_STATE,
										IntPtr.Zero,
										new IntPtr(256),
										false,
										(IntPtr)XAtom.XA_ATOM,
										out var actualAtom,
										out var actualFormat,
										out var nitems,
										out var bytesAfter,
										ref prop) == 0)
			{
				var itemCount = nitems.ToInt64();

				if (itemCount > 0 && prop != IntPtr.Zero)
				{
					for (int i = 0; i < nitems; i++)
					{
						var atom = (IntPtr)Marshal.ReadInt64(prop, i * 8);
						//Keysharp.Scripting.Script.OutputDebug($"ReadStateProps() item {i} was atom {atom}.");

						if (!func(atom))
							break;
					}

					_ = Xlib.XFree(prop);
				}

				//else
				//  Keysharp.Scripting.Script.OutputDebug($"ReadStateProps() contained zero atoms.");
				return true;
			}
			else
				Keysharp.Scripting.Script.OutputDebug($"ReadStateProps() XGetWindowProperty failed.");

			return false;
		}

		internal override WindowItemBase RealChildWindowFromPoint(System.Drawing.Point location) => throw new NotImplementedException();

		internal override bool Redraw()
		{
			if (!IsSpecified)
				return false;

			return Xlib.XClearWindow(_xwindow.XDisplay.Handle, _xwindow.ID) != 0;
		}

		internal void SendMouseEvent(XEventName evName, EventMasks evMask, Buttons button, System.Drawing.Point? location = null)
		{
			var click = new Point();

			if (location.HasValue)
			{
				click = location.Value;
			}
			else
			{
				// if not specified find middlepoint of this window/control
				var size = Size;
				click.X = size.Width / 2;
				click.Y = size.Height / 2;
			}

			//var lparam = new IntPtr(Conversions.MakeInt(click.X, click.Y));
			var ev = new XEvent();
			ev.ButtonEvent = new XButtonEvent();
			ev.ButtonEvent.type = evName;
			ev.ButtonEvent.send_event = true;
			ev.ButtonEvent.display = _xwindow.XDisplay.Handle;
			ev.ButtonEvent.window = Handle;
			ev.ButtonEvent.subwindow = Handle;
			ev.ButtonEvent.x = click.X;
			ev.ButtonEvent.y = click.Y;
			ev.ButtonEvent.root = new IntPtr(_xwindow.XDisplay.Root.ID);
			ev.ButtonEvent.same_screen = true;
			ev.ButtonEvent.button = button;
			//Unsure if propagate should be true or false here. The documentation is confusing.
			//Mask might also need to be 0xfff?
			_ = Xlib.XSendEvent(_xwindow.XDisplay.Handle, Handle.ToInt64(), true, evMask, ref ev);
		}

		internal override void SetTransparency(byte level, System.Drawing.Color color) => throw new NotImplementedException();

		internal override bool Show()
		{
			if (!IsSpecified)
				return false;

			if (Control.FromHandle(Handle) is Control control)
			{
				control.Visible = true;
				return true;
			}

			return Xlib.XMapWindow(_xwindow.XDisplay.Handle, _xwindow.ID) != 0;
		}
	}
}

#endif