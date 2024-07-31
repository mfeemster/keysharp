#if LINUX
using WindowStyles = System.Windows.Forms.WindowStyles;
using WindowExStyles = System.Windows.Forms.WindowExStyles;

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
		private readonly XWindow xwindow = null;
		private readonly WindowManager manager = (Keysharp.Core.Linux.WindowManager)WindowProvider.Manager;

		internal override bool Active
		{
			get
			{
				if (IsSpecified && manager.ActiveWindow is WindowItem item)
				{
					//Keysharp.Scripting.Script.OutputDebug($"item.Handle: {item.Handle.ToInt64()}, item.Title: {item.Title}, Handle: {Handle.ToInt64()}, Title: {Title}");
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
								manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_ACTIVE_WINDOW, 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
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
				_ = ReadProps(xwindow.XDisplay._NET_WM_STATE, (IntPtr)XAtom.XA_ATOM, (atom) =>
				{
					if (atom == xwindow.XDisplay._NET_WM_STATE_ABOVE)
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
				if (IsSpecified)
					manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_WM_STATE, value ? 1 : 0, xwindow.XDisplay._NET_WM_STATE_ABOVE, IntPtr.Zero, IntPtr.Zero);
			}
		}

		internal override bool Bottom
		{
			set
			{
				if (!IsSpecified)
					return;

				if (value)
				{
					//Keysharp.Scripting.Script.OutputDebug($"Bottom: about to call XLowerWindow().");
					_ = Xlib.XLowerWindow(xwindow.XDisplay.Handle, xwindow.ID);
				}
				else
				{
					//Keysharp.Scripting.Script.OutputDebug($"Bottom: about to call XRaiseWindow().");
					_ = Xlib.XRaiseWindow(xwindow.XDisplay.Handle, xwindow.ID);
				}
			}
		}

		internal override List<WindowItemBase> ChildWindows
		{
			get
			{
				var windows = new List<WindowItemBase>();

				if (!IsSpecified)
					return windows;

				var attr = new XWindowAttributes();
				var detectHiddenText = ThreadAccessors.A_DetectHiddenText;
				var filter = (long id) =>
				{
					if (Xlib.XGetWindowAttributes(xwindow.XDisplay.Handle, id, ref attr) != 0)
						if (detectHiddenText || attr.map_state == MapState.IsViewable)
							return true;

					return false;
				};
				windows.AddRange(xwindow.XDisplay.XQueryTreeRecursive(xwindow, filter).Select(w => new WindowItem(w)));
				return windows;
			}
		}

		internal override string ClassName
		{
			get
			{
				if (!IsSpecified)
					return "";

				var hint = new XClassHintStr();

				if (Xlib.GetClassHint(xwindow.XDisplay.Handle, xwindow.ID, ref hint) != 0)
				{
					return hint.resClass;
				}

				return "";
			}
		}

		internal override Rectangle ClientLocation
		{
			get
			{
				if (IsSpecified)
				{
					var attr = xwindow.Attributes;
					var pt = ClientToScreen();
					var scale = 1.0 / Accessors.A_ScaledScreenDPI;
					//Width and heigh do not include the border.
					//pt is already scaled.
					return new Rectangle(pt.X, pt.Y, (int)(scale * attr.width), (int)(scale * attr.height));
				}
				else
					return new Rectangle();
			}
		}

		internal override bool Enabled
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		internal override bool Exists => IsSpecified && xwindow.XDisplay.XQueryTreeRecursive().Any(xw => xw.ID == xwindow.ID);

		internal override long ExStyle
		{
			get
			{
				if (!IsSpecified)
					return 0L;

				var ctrl = Control.FromHandle(Handle);

				if (ctrl == null)
				{
					Keysharp.Scripting.Script.OutputDebug($"Window with handle {Handle} was not a .NET Form or Control, so the ex style could not be retrieved. Returning 0.");
					return 0;
				}
				else if (ctrl is Form form)
					return ConvertFormPropsToCreateParams(form).ExStyle;
				else
					return ConvertControlPropsToCreateParams(ctrl).ExStyle;
			}
			set
			{
				Keysharp.Scripting.Script.OutputDebug($"ExStyles cannot be set on linux.");
			}
		}

		internal override bool IsHung => false;

		internal override Rectangle Location
		{
			get
			{
				var attr = xwindow.Attributes;
				var scale = 1.0 / Accessors.A_ScaledScreenDPI;
				return new Rectangle((int)(scale * attr.x), (int)(scale * attr.y), (int)(scale * (attr.width + attr.border_width)), (int)(scale * (attr.height + attr.border_width)));
			}
			set
			{
				if (IsSpecified)
				{
					var loc = Location;
					var x = loc.X;
					var y = loc.Y;

					if (value.X != int.MinValue)
						x = value.X;

					if (value.Y != int.MinValue)
						y = value.Y;

					_ = Xlib.XMoveWindow(xwindow.XDisplay.Handle, xwindow.ID, x, y);
				}
			}
		}

		internal override WindowItemBase NonChildParentWindow
		{
			get
			{
				if (!IsSpecified)
					return null;

				var parent = ParentWindow;
				var tempParent = parent;

				while (tempParent != null && tempParent.Handle.ToInt64() != xwindow.XDisplay.Root.ID)
				{
					parent = tempParent;
					tempParent = parent.ParentWindow;
				}

				return parent;
			}
		}

		internal override WindowItemBase ParentWindow
		{
			get
			{
				var parentReturn = 0L;
				var childrenReturn = IntPtr.Zero;

				try
				{
					_ = Xlib.XQueryTree(Handle, xwindow.ID, out var rootReturn, out parentReturn, out childrenReturn, out var nChildrenReturn);
				}
				catch
				{
				}
				finally
				{
					if (childrenReturn != IntPtr.Zero)
						_ = Xlib.XFree(childrenReturn);
				}

				return new WindowItem(new XWindow(xwindow.XDisplay, parentReturn));
			}
		}

		internal override long PID
		{
			get
			{
				var pid = 0L;

				if (IsSpecified)
				{
					_ = ReadProps(xwindow.XDisplay._NET_WM_PID, (IntPtr)XAtom.AnyPropertyType, (atom) =>
					{
						pid = atom;
						return false;
					});
				}

				return pid;
			}
		}

		internal override Size Size
		{
			get
			{
				var attr = xwindow.Attributes;
				var scale = 1.0 / Accessors.A_ScaledScreenDPI;
				return new Size((int)(scale * (attr.width + attr.border_width)), (int)(scale * (attr.height + attr.border_width)));
			}
			set
			{
				if (IsSpecified)
				{
					var scale = 1.0 / Accessors.A_ScaledScreenDPI;
					_ = Xlib.XResizeWindow(xwindow.XDisplay.Handle, xwindow.ID, value.Width, value.Height);
				}
			}
		}

		internal override long Style
		{
			get
			{
				if (!IsSpecified)
					return 0L;

				var ctrl = Control.FromHandle(Handle);

				if (ctrl == null)
				{
					Keysharp.Scripting.Script.OutputDebug($"Window with handle {Handle} was not a .NET Form or Control, so the style could not be retrieved. Returning 0.");
					return 0;
				}
				else if (ctrl is Form form)
					return ConvertFormPropsToCreateParams(form).Style;
				else
					return ConvertControlPropsToCreateParams(ctrl).Style;
			}
			set
			{
				Keysharp.Scripting.Script.OutputDebug($"Styles cannot be set on linux.");
			}
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
					if (Xlib.XGetWindowAttributes(xwindow.XDisplay.Handle, id, ref attr) != 0)
						if (tv.detectHiddenText || attr.map_state == MapState.IsViewable)
							return true;

					return false;
				};
				return xwindow.XDisplay.XQueryTreeRecursive(xwindow, filter).Select(x =>
				{
					if (Xlib.XGetTextProperty(xwindow.XDisplay.Handle, x.ID, ref prop, XAtom.XA_WM_NAME) != 0)
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
					return Xlib.GetWMName(xwindow.XDisplay.Handle, xwindow.ID);
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
							Xlib.XSetTextProperty(xwindow.XDisplay.Handle, xwindow.ID, ref prop, XAtom.XA_WM_NAME);
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
				long alpha = 0xFF;

				if (!IsSpecified)
					return alpha;

				_ = ReadProps(xwindow.XDisplay._NET_WM_WINDOW_OPACITY, (IntPtr)XAtom.XA_CARDINAL, (atom) =>
				{
					alpha = atom;
					return false;
				});
				return alpha;
			}
			set
			{
				if (!IsSpecified)
					return;

				if (value is string s)
				{
					if (s.ToLower() == "off")
						_ = Xlib.XDeleteProperty(xwindow.XDisplay.Handle, xwindow.ID, xwindow.XDisplay._NET_WM_WINDOW_OPACITY);
				}
				else
				{
					var alpha = (IntPtr)Math.Clamp((int)value.Al(), 0, 255);
					_ = Xlib.XChangeProperty(xwindow.XDisplay.Handle, xwindow.ID, xwindow.XDisplay._NET_WM_WINDOW_OPACITY, (IntPtr)XAtom.XA_CARDINAL, 32, PropertyMode.Replace, ref alpha, 1);
				}
			}
		}

		internal override object TransparentColor
		{
			get
			{
				Keysharp.Scripting.Script.OutputDebug($"Transparency key/color not supported on linux, returning 0.");
				return 0L;
			}
			set
			{
				Keysharp.Scripting.Script.OutputDebug($"Transparency key/color not supported on linux.");
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

				return xwindow.Attributes.map_state == MapState.IsViewable;
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
				_ = ReadProps(xwindow.XDisplay._NET_WM_STATE, (IntPtr)XAtom.XA_ATOM, (atom) =>
				{
					if ((atom == xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ) || (atom == xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT))
					{
						maximized++;
					}
					else if (atom == xwindow.XDisplay._NET_WM_STATE_HIDDEN)
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
				if (!IsSpecified)
					return;

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
								_ = Xlib.XMapWindow(xwindow.XDisplay.Handle, Handle);
								//Keysharp.Scripting.Script.OutputDebug($"Window was minimized, so setting to normal.");
								manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
							}
							else if (current_state == FormWindowState.Maximized)
							{
								//Keysharp.Scripting.Script.OutputDebug($"Window was maximized, so setting to normal.");
								manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_WM_STATE, 2 /* toggle */, xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ, xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT, IntPtr.Zero);
								manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
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
								manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_WM_STATE, 2 /* toggle */, xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ, xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT, IntPtr.Zero);
							}

							_ = Xlib.XIconifyWindow(xwindow.XDisplay.Handle, Handle.ToInt64(), xwindow.XDisplay.ScreenNumber);
						}

						return;
					}

					case FormWindowState.Maximized:
					{
						lock (WindowManager.xLibLock)
						{
							if (current_state == FormWindowState.Minimized)
							{
								_ = Xlib.XMapWindow(xwindow.XDisplay.Handle, Handle);
							}

							manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_WM_STATE, 1 /* Add */, xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_HORZ, xwindow.XDisplay._NET_WM_STATE_MAXIMIZED_VERT, IntPtr.Zero);
						}

						manager.SendNetWMMessage(Handle, xwindow.XDisplay._NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
						return;
					}
				}
			}
		}

		internal WindowItem(XWindow uxwindow)
			: base(new IntPtr(uxwindow.ID))
		{
			xwindow = uxwindow;
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
			_ = Xlib.XFlush(xwindow.XDisplay.Handle);
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
			_ = Xlib.XFlush(xwindow.XDisplay.Handle);
			//Might need a sleep here.
			SendMouseEvent(XEventName.ButtonRelease, EventMasks.ButtonRelease, Buttons.Right, location);
		}

		internal override System.Drawing.Point ClientToScreen()
		{
			if (IsSpecified)
			{
				_ = Xlib.XTranslateCoordinates(xwindow.XDisplay.Handle, xwindow.ID, xwindow.XDisplay.Root.ID, 0, 0, out var x, out var y, out var dummy);
				var pt = new System.Drawing.Point(x, y);
				var scale = 1.0 / Accessors.A_ScaledScreenDPI;
				pt.X = (int)(scale * pt.X);
				pt.Y = (int)(scale * pt.Y);
				return pt;
			}
			else
				return new System.Drawing.Point(0, 0);
		}

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
			ev.ClientMessageEvent.type = X11.XEventName.ClientMessage;
			ev.ClientMessageEvent.window = Handle;
			ev.ClientMessageEvent.message_type = xwindow.XDisplay.WM_PROTOCOLS;
			ev.ClientMessageEvent.format = 32;
			ev.ClientMessageEvent.ptr1 = xwindow.XDisplay.WM_DELETE_WINDOW;
			//ev.ClientMessageEvent.data.l [1] = CurrentTime;
			return Xlib.XSendEvent(xwindow.XDisplay.Handle, xwindow.ID, false, EventMasks.NoEvent, ref ev) != 0;
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

			return Xlib.XUnmapWindow(xwindow.XDisplay.Handle, xwindow.ID) != 0;
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

			_ = Xlib.XKillClient(xwindow.XDisplay.Handle, xwindow.ID);
			return !Exists;
		}

		internal bool ReadProps(IntPtr state, IntPtr type, Func<long, bool> func)
		{
			IntPtr prop = IntPtr.Zero;

			if (Xlib.XGetWindowProperty(xwindow.XDisplay.Handle,
										xwindow.ID,
										state,
										IntPtr.Zero,
										new IntPtr(256),
										false,
										type,
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

		//internal override WindowItemBase RealChildWindowFromPoint(System.Drawing.Point location) => throw new NotImplementedException();

		internal override bool Redraw()
		{
			if (!IsSpecified)
				return false;

			return Xlib.XClearWindow(xwindow.XDisplay.Handle, xwindow.ID) != 0;
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
			ev.ButtonEvent.display = xwindow.XDisplay.Handle;
			ev.ButtonEvent.window = Handle;
			ev.ButtonEvent.subwindow = Handle;
			ev.ButtonEvent.x = click.X;
			ev.ButtonEvent.y = click.Y;
			ev.ButtonEvent.root = new IntPtr(xwindow.XDisplay.Root.ID);
			ev.ButtonEvent.same_screen = true;
			ev.ButtonEvent.button = button;
			//Unsure if propagate should be true or false here. The documentation is confusing.
			//Mask might also need to be 0xfff?
			_ = Xlib.XSendEvent(xwindow.XDisplay.Handle, Handle.ToInt64(), true, evMask, ref ev);
		}

		internal override bool Show()
		{
			if (!IsSpecified)
				return false;

			if (Control.FromHandle(Handle) is Control control)
			{
				control.Visible = true;
				return true;
			}

			return Xlib.XMapWindow(xwindow.XDisplay.Handle, xwindow.ID) != 0;
		}

		private enum ClassStyle
		{
			CS_VREDRAW = 0x00000001,
			CS_HREDRAW = 0x00000002,
			CS_KEYCVTWINDOW = 0x00000004,
			CS_DBLCLKS = 0x00000008,
			CS_OWNDC = 0x00000020,
			CS_CLASSDC = 0x00000040,
			CS_PARENTDC = 0x00000080,
			CS_NOKEYCVT = 0x00000100,
			CS_NOCLOSE = 0x00000200,
			CS_SAVEBITS = 0x00000800,
			CS_BYTEALIGNCLIENT = 0x00001000,
			CS_BYTEALIGNWINDOW = 0x00002000,
			CS_GLOBALCLASS = 0x00004000,
			CS_IME = 0x00010000,
			// Windows XP+
			CS_DROPSHADOW = 0x00020000
		}

		/// <summary>
		/// Translate the various properties of a control to the equivalent Windows style.
		/// Copied this from Control.cs in Mono.
		/// </summary>
		/// <returns></returns>
		private CreateParams ConvertControlPropsToCreateParams(Control control)
		{
			CreateParams cp = new CreateParams();

			try
			{
				cp.Caption = control.Text;
			}
			catch
			{
				cp.Caption = "";
			}

			try
			{
				cp.X = control.Left;
			}
			catch
			{
			}

			try
			{
				cp.Y = control.Top;
			}
			catch
			{
			}

			try
			{
				cp.Width = control.Width;
			}
			catch
			{
			}

			try
			{
				cp.Height = control.Height;
			}
			catch
			{
			}

			cp.ClassName = "SWFClass" + Thread.GetDomainID().ToString() + "." + control.GetType().ToString();
			cp.ClassStyle = (int)(ClassStyle.CS_OWNDC | ClassStyle.CS_DBLCLKS);
			cp.ExStyle = 0;
			cp.Param = 0;

			if (control.AllowDrop)
			{
				cp.ExStyle |= (int)WindowExStyles.WS_EX_ACCEPTFILES;
			}

			if ((control.Parent != null) && (control.Parent.IsHandleCreated))
			{
				cp.Parent = control.Parent.Handle;
			}

			cp.Style = (int)WindowStyles.WS_CHILD | (int)WindowStyles.WS_CLIPCHILDREN | (int)WindowStyles.WS_CLIPSIBLINGS;

			if (control.Visible)
			{
				cp.Style |= (int)WindowStyles.WS_VISIBLE;
			}

			if (!control.Enabled)
			{
				cp.Style |= (int)WindowStyles.WS_DISABLED;
			}

			var props = control.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
			var prop = props.FirstOrDefault(p => p.Name == "InternalBorderStyle");
			var borderStyle = (BorderStyle)prop.GetValue(control, null);

			switch (borderStyle)
			{
				case BorderStyle.FixedSingle:
					cp.Style |= (int) WindowStyles.WS_BORDER;
					break;

				case BorderStyle.Fixed3D:
					cp.ExStyle |= (int) WindowExStyles.WS_EX_CLIENTEDGE;
					break;
			}

			return cp;
		}

		/// <summary>
		/// Translate the various properties of a window to the equivalent Windows style.
		/// Copied this from Form.cs in Mono.
		/// </summary>
		/// <returns></returns>
		private CreateParams ConvertFormPropsToCreateParams(Form form)
		{
			CreateParams cp = new CreateParams();

			if (form.Text != null)
				cp.Caption = form.Text.Replace(Environment.NewLine, string.Empty);

			cp.ClassName = "SWFClass" + Thread.GetDomainID().ToString() + "." + form.GetType().ToString();
			cp.ClassStyle = 0;
			cp.Style = 0;
			cp.ExStyle = 0;
			cp.Param = 0;
			cp.Parent = IntPtr.Zero;

			if (((form.Parent != null || !form.TopLevel) && !form.IsMdiChild))
			{
				// Parented forms and non-toplevel forms always gets the specified location, no matter what
				cp.X = form.Left;
				cp.Y = form.Top;
			}
			else
			{
				switch (form.StartPosition)
				{
					case FormStartPosition.Manual:
						cp.X = form.Left;
						cp.Y = form.Top;
						break;

					case FormStartPosition.CenterScreen:
						if (form.IsMdiChild)
						{
							var mdiContainer = form.MdiParent.Controls.Cast<Control>().FirstOrDefault(c => c is MdiClient);

							if (mdiContainer != null)
							{
								cp.X = Math.Max((mdiContainer.ClientSize.Width - form.Width) / 2, 0);
								cp.Y = Math.Max((mdiContainer.ClientSize.Height - form.Height) / 2, 0);
							}
						}
						else
						{
							cp.X = Math.Max((System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - form.Width) / 2, 0);
							cp.Y = Math.Max((System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - form.Height) / 2, 0);
						}

						break;

					case FormStartPosition.CenterParent:
					case FormStartPosition.WindowsDefaultBounds:
					case FormStartPosition.WindowsDefaultLocation:
						cp.X = int.MinValue;
						cp.Y = int.MinValue;
						break;
				}
			}

			cp.Width = form.Width;
			cp.Height = form.Height;
			cp.Style = (int)WindowStyles.WS_CLIPCHILDREN;

			if (!form.Modal)
			{
				cp.WindowStyle |= WindowStyles.WS_CLIPSIBLINGS;
			}

			if (form.Parent != null && form.Parent.IsHandleCreated)
			{
				cp.Parent = form.Parent.Handle;
				cp.Style |= (int)WindowStyles.WS_CHILD;
			}

			if (form.IsMdiChild)
			{
				cp.Style |= (int)(WindowStyles.WS_CHILD | WindowStyles.WS_CAPTION);

				if (form.Parent != null)
				{
					cp.Parent = form.Parent.Handle;
				}

				cp.ExStyle |= (int)(WindowExStyles.WS_EX_WINDOWEDGE | WindowExStyles.WS_EX_MDICHILD);

				switch (form.FormBorderStyle)
				{
					case FormBorderStyle.None:
						break;

					case FormBorderStyle.FixedToolWindow:
					case FormBorderStyle.SizableToolWindow:
						cp.ExStyle |= (int)WindowExStyles.WS_EX_TOOLWINDOW;
						goto default;

					default:
						cp.Style |= (int)WindowStyles.WS_OVERLAPPEDWINDOW;
						break;
				}
			}
			else
			{
				switch (form.FormBorderStyle)
				{
					case FormBorderStyle.Fixed3D:
					{
						cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
						cp.ExStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE;
						break;
					}

					case FormBorderStyle.FixedDialog:
					{
						cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
						cp.ExStyle |= (int)(WindowExStyles.WS_EX_DLGMODALFRAME | WindowExStyles.WS_EX_CONTROLPARENT);
						break;
					}

					case FormBorderStyle.FixedSingle:
					{
						cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
						break;
					}

					case FormBorderStyle.FixedToolWindow:
					{
						cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
						cp.ExStyle |= (int)WindowExStyles.WS_EX_TOOLWINDOW;
						break;
					}

					case FormBorderStyle.Sizable:
					{
						cp.Style |= (int)(WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME | WindowStyles.WS_CAPTION);
						break;
					}

					case FormBorderStyle.SizableToolWindow:
					{
						cp.Style |= (int)(WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME | WindowStyles.WS_CAPTION);
						cp.ExStyle |= (int)WindowExStyles.WS_EX_TOOLWINDOW;
						break;
					}

					case FormBorderStyle.None:
					{
						break;
					}
				}
			}

			switch (form.WindowState)
			{
				case FormWindowState.Maximized:
				{
					cp.Style |= (int)WindowStyles.WS_MAXIMIZE;
					break;
				}

				case FormWindowState.Minimized:
				{
					cp.Style |= (int)WindowStyles.WS_MINIMIZE;
					break;
				}
			}

			if (form.TopMost)
			{
				cp.ExStyle |= (int)WindowExStyles.WS_EX_TOPMOST;
			}

			if (form.ShowInTaskbar)
			{
				cp.ExStyle |= (int)WindowExStyles.WS_EX_APPWINDOW;
			}

			if (form.MaximizeBox)
			{
				cp.Style |= (int)WindowStyles.WS_MAXIMIZEBOX;
			}

			if (form.MinimizeBox)
			{
				cp.Style |= (int)WindowStyles.WS_MINIMIZEBOX;
			}

			if (form.ControlBox)
			{
				cp.Style |= (int)WindowStyles.WS_SYSMENU;
			}

			if (!form.ShowIcon)
			{
				cp.ExStyle |= (int)WindowExStyles.WS_EX_DLGMODALFRAME;
			}

			cp.ExStyle |= (int)WindowExStyles.WS_EX_CONTROLPARENT;

			if (form.HelpButton && !form.MaximizeBox && !form.MinimizeBox)
			{
				cp.ExStyle |= (int)WindowExStyles.WS_EX_CONTEXTHELP;
			}

			// bug 80775:
			//don't set WS_VISIBLE if we're changing visibility. We can't create forms visible,
			//since we have to set the owner before making the form visible
			//(otherwise Win32 will do strange things with task bar icons).
			//The problem is that we set the internal is_visible to true before creating the control,
			//so is_changing_visible_state is the only way of determining if we're
			//in the process of creating the form due to setting Visible=true.
			//This works because SetVisibleCore explicitly makes the form visibile afterwards anyways.
			// bug 81957:
			//only do this when on Windows, since X behaves weirdly otherwise
			//modal windows appear below their parent/owner/ancestor.
			//(confirmed on several window managers, so it's not a wm bug).
			//int p = (int)Environment.OSVersion.Platform;
			//bool is_unix = (p == 128) || (p == 4) || (p == 6);

			//if ((Visible && (is_changing_visible_state == 0 || is_unix)) || form.IsRecreating)
			//  cp.Style |= (int)WindowStyles.WS_VISIBLE;

			if (form.Opacity < 1.0 || form.TransparencyKey != Color.Empty)
			{
				cp.ExStyle |= (int)WindowExStyles.WS_EX_LAYERED;
			}

			if (!form.Enabled/* && context == null*/)
			{
				cp.Style |= (int)WindowStyles.WS_DISABLED;
			}

			if (!form.ControlBox && form.Text == string.Empty)
			{
				cp.WindowStyle &= ~WindowStyles.WS_DLGFRAME;
			}

			return cp;
		}
	}
}

#endif