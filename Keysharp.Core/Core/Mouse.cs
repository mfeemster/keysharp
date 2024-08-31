using static Keysharp.Scripting.Keywords;

namespace Keysharp.Core
{
	public static class Mouse
	{
		internal static CoordModes Coords
		{
			get
			{
				var tv = Threads.GetThreadVariables();
				return tv.coords ?? (tv.coords = new CoordModes());
			}
		}

		/// <summary>
		/// Clicks a mouse button at the specified coordinates. It can also hold down a mouse button, turn the mouse wheel, or move the mouse.
		/// </summary>
		/// <param name="options">Can be one or more of the following options:
		/// <list type="bullet">
		/// <item><term><c>X</c> or <c>Y</c></term>: <description>the coordinates;</description></item>
		/// <item><term>Button</term>: <description><c>Left</c> (default), <c>Middle</c>, <c>Right</c>, <c>X1</c> or <c>X2</c>;</description></item>
		/// <item><term>Wheel</term>: <description><c>WheelUp</c>, <c>WheelDown</c>, <c>WheelLeft</c> or <c>WheelRight</c>;</description></item>
		/// <item><term>Count</term>: <description>number of times to send the click;</description></item>
		/// <item><term><c>Down</c> or <c>Up</c></term>: <description>if omitted send a down click followed by an up release;</description></item>
		/// <item><term><c>Relative</c></term>: <description>treat the coordinates as relative offsets from the current mouse position.</description></item>
		/// </list>
		/// </param>
		public static void Click(object options)
		{
			var opts = options.As();
			int x = 0, y = 0;
			var vk = 0u;
			var event_type = KeyEventTypes.KeyDown;
			var repeat_count = 0L;
			var move_offset = false;
			var ht = Script.HookThread;
			ht.ParseClickOptions(opts, ref x, ref y, ref vk, ref event_type, ref repeat_count, ref move_offset);
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			ht.kbdMsSender.PerformMouseCommon(repeat_count < 1 ? Actions.ACT_MOUSEMOVE : Actions.ACT_MOUSECLICK // Treat repeat-count<1 as a move (like {click}).
											  , vk, x, y, 0, 0, repeat_count, event_type, ThreadAccessors.A_DefaultMouseSpeed, move_offset);//, true, true);
		}

		/// <summary>
		/// Change the coordinate mode for a specific feature.
		/// </summary>
		/// <param name="target">
		/// <list type="bullet">
		/// <item><term>ToolTip</term></item>
		/// <item><term>Pixel</term></item>
		/// <item><term>Mouse</term></item>
		/// <item><term>Caret</term></item>
		/// <item><term>Menu</term></item>
		/// </list>
		/// </param>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>Screen</term>: <description>Coordinates are relative to the desktop (entire screen).</description></item>
		/// <item><term>Window</term>: <description>Coordinates are relative to the active window.</description></item>
		/// <item><term>Client</term>: <description>Coordinates are relative to the active window's client area, which excludes the window's title bar, menu (if it has a standard one) and borders. Client coordinates are less dependent on OS version and theme.</description></item>
		/// <item><term>Relative</term>: <description>Same as Window.</description></item>
		/// </list>
		/// </param>
		public static void CoordMode(object targetType, object relativeTo = null)
		{
			var target = targetType.As();
			var mode = relativeTo.As(Keyword_Screen);
			CoordModeType rel;

			if (Options.IsOption(mode, Keyword_Relative))
				rel = CoordModeType.Window;
			else if (Options.IsOption(mode, Keyword_Client))
				rel = CoordModeType.Client;
			else if (Options.IsOption(mode, Keyword_Window))
				rel = CoordModeType.Window;
			else if (Options.IsOption(mode, Keyword_Screen))
				rel = CoordModeType.Screen;
			else
				throw new ValueError($"Invalid RelativeTo value of '{mode}' passed to CoordMode().");

			switch (target.ToLowerInvariant())
			{
				case Keyword_ToolTip: Coords.Tooltip = rel; break;

				case Keyword_Pixel: Coords.Pixel = rel; break;

				case Keyword_Mouse: Coords.Mouse = rel; break;

				case Keyword_Caret: Coords.Caret = rel; break;

				case Keyword_Menu: Coords.Menu = rel; break;

				default:
					throw new ValueError($"Invalid TargetType value of '{target}' passed to CoordMode().");
			}
		}

		public static void MouseClick(object whichButton = null, object x = null, object y = null, object clickCount = null, object speed = null, object downOrUp = null, object relative = null)
		{
			var wb = whichButton.As();
			var ix = x.Ai(KeyboardMouseSender.CoordUnspecified);// If no starting coords are specified, mark it as "use the current mouse position".
			var iy = y.Ai(KeyboardMouseSender.CoordUnspecified);
			var repeatCount = clickCount.Al(1);
			var ispeed = (int)speed.Al(ThreadAccessors.A_DefaultMouseSpeed);
			var du = downOrUp.As();
			var rel = relative.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSECLICK, wb, ix, iy, KeyboardMouseSender.CoordUnspecified, KeyboardMouseSender.CoordUnspecified,
						 ispeed, rel, repeatCount, du);//, true, true);
		}

		public static void MouseClickDrag(object whichButton, object x1, object y1, object x2, object y2, object speed = null, object relative = null)
		{
			var wb = whichButton.As();
			var ix1 = x1.Ai(KeyboardMouseSender.CoordUnspecified);//If no starting coords are specified, mark it as "use the current mouse position".
			var iy1 = y1.Ai(KeyboardMouseSender.CoordUnspecified);
			var ix2 = x2.Ai(KeyboardMouseSender.CoordUnspecified);
			var iy2 = y2.Ai(KeyboardMouseSender.CoordUnspecified);
			var ispeed = (int)speed.Al(ThreadAccessors.A_DefaultMouseSpeed);
			var rel = relative.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSECLICKDRAG, wb, ix1, iy1, ix2, iy2,
						 ispeed, rel, 1, "");//, true, true);
		}

		/// <summary>
		/// Retrieves the current position of the mouse cursor, and optionally which window and control it is hovering over.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="win">The window ID.</param>
		/// <param name="control">The control ID.</param>
		/// <param name="mode">
		/// <list type="bullet">
		/// <item><term>2</term>: retrieve the <paramref name="control"/> ID rather than its class name.</item>
		/// </list>
		/// </param>
		public static void MouseGetPos(ref object outputVarX, ref object outputVarY, ref object outputVarWin, ref object outputVarControl, object flag = null)
		{
			var mode = flag.Al(0L);
			var pos = Cursor.Position;
			var aX = 0;
			var aY = 0;
			PlatformProvider.Manager.CoordToScreen(ref aX, ref aY, Core.CoordMode.Mouse);
			outputVarX = (long)(pos.X - aX);
			outputVarY = (long)(pos.Y - aY);
			var child = WindowProvider.Manager.WindowFromPoint(pos);

			if (child == null || child.Handle == IntPtr.Zero)
				return;

			var parent = child.NonChildParentWindow;
			outputVarWin = parent.Handle;
#if WINDOWS

			//Doing it this way overcomes the limitations of WindowFromPoint() and ChildWindowFromPoint()
			//and also better matches the control that Window Spy would think is under the cursor:
			if ((mode & 0x01) == 0)
			{
				var pah = new Keysharp.Core.Common.Window.PointAndHwnd(pos);//Find topmost control containing point.
				parent.ChildFindPoint(pah);

				if (pah.hwndFound != IntPtr.Zero)
					child = WindowProvider.Manager.CreateWindow(pah.hwndFound);
			}

#endif

			if (child.Handle == parent.Handle)//If there's no control per se, make it blank.
				return;

			if ((mode & 0x02) != 0)
			{
				outputVarControl = child.Handle;
				return;
			}

			outputVarControl = child.ClassNN;
		}

		public static void MouseMove(object x, object y, object speed = null, object relative = null)
		{
			var ix = x.Ai(KeyboardMouseSender.CoordUnspecified);
			var iy = y.Ai(KeyboardMouseSender.CoordUnspecified);
			var s = (int)speed.Al(ThreadAccessors.A_DefaultMouseSpeed);
			var r = relative.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSEMOVE, "", ix, iy, KeyboardMouseSender.CoordUnspecified, KeyboardMouseSender.CoordUnspecified,
						 s, r, 1, "");//, true, true);
		}

		public static void SetDefaultMouseSpeed(object speed) => Accessors.A_DefaultMouseSpeed = speed;

		public static void SetMouseDelay(object delay, object play = null)
		{
			var p = play.As().ToLowerInvariant();
			var isplay = p == "play";
			var del = isplay ? Accessors.A_MouseDelayPlay : Accessors.A_MouseDelay;

			if (delay != null)
				del = delay.Al();

			if (isplay)
				Accessors.A_MouseDelayPlay = del;
			else
				Accessors.A_MouseDelay = del;
		}

		internal static void AdjustPoint(ref int x, ref int y)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				var rect = WindowProvider.Manager.ActiveWindow.Location;
				x += rect.Left;
				y += rect.Top;
			}
		}

		internal static void AdjustRect(ref int x1, ref int y1, ref int x2, ref int y2)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				var rect = WindowProvider.Manager.ActiveWindow.Location;
				x1 += rect.Left;
				y1 += rect.Top;
				x2 += rect.Left;
				y2 += rect.Top;
			}
		}

		internal static Point RevertPoint(Point p, CoordModeType modeType)
		{
			//for cross platform purposes, should use something like Form.ActiveForm.PointToScreen() etc...
			if (modeType == CoordModeType.Window)//This does not account for the mode value of other coord settings, like menu.//TODO
			{
				var rect = WindowProvider.Manager.ActiveWindow.Location;
				return new Point(p.X - rect.Left, p.Y - rect.Top);
			}

			return p;
		}

		private static void PerformMouse(Actions actionType, string button, int x1, int y1, int x2, int y2
										 , int speed, string relative, long repeatCount, string downUp)
		{
			uint vk;
			var ht = Script.HookThread;

			if (actionType == Actions.ACT_MOUSEMOVE)
			{
				vk = 0;
			}
			else
			{
				if ((vk = ht.ConvertMouseButton(button, actionType == Actions.ACT_MOUSECLICK)) == 0)
					throw new ValueError($"Invalid mouse button type of {button}.");
			}

			// v1.0.43: Seems harmless (due to rarity) to treat invalid button names as "Left" (keeping in
			// mind that due to loadtime validation, invalid buttons are possible only when the button name is
			// contained in a variable, e.g. MouseClick %ButtonName%.
			var eventType = KeyEventTypes.KeyDownAndUp;  // Set defaults.

			if (actionType == Actions.ACT_MOUSECLICK)
			{
				if (downUp.Length > 0)
				{
					switch (char.ToUpper(downUp[0]))
					{
						case 'U':
							eventType = KeyEventTypes.KeyUp;
							break;

						case 'D':
							eventType = KeyEventTypes.KeyDown;
							break;

						case '\0':
							break;

						default:
							break;
					}
				}
			}

			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			ht.kbdMsSender.PerformMouseCommon(actionType
											  , vk
											  , x1
											  , y1
											  , x2//These two are blank except for dragging.
											  , y2
											  , repeatCount
											  , eventType
											  , speed
											  , relative.Length > 0 && char.ToUpper(relative[0]) == 'R');
		}
	}

	internal class CoordModes
	{
		public CoordModeType Caret { get; set; } = CoordModeType.Client;
		public CoordModeType Menu { get; set; } = CoordModeType.Client;
		public CoordModeType Mouse { get; set; } = CoordModeType.Client;
		public CoordModeType Pixel { get; set; } = CoordModeType.Client;
		public CoordModeType Tooltip { get; set; } = CoordModeType.Client;

		internal CoordModeType GetCoordMode(CoordMode mode)
		{
			switch (mode)
			{
				case CoordMode.Caret:
					return Caret;

				case CoordMode.Menu:
					return Menu;

				case CoordMode.Mouse:
					return Mouse;

				case CoordMode.Pixel:
					return Pixel;

				case CoordMode.Tooltip:
					return Tooltip;

				default:
					throw new ValueError($"Invalid coord mode type: {mode}");
			}
		}
	}

	internal enum CoordMode
	{
		Caret,
		Menu,
		Mouse,
		Pixel,
		Tooltip
	}

	internal enum CoordModeType
	{
		Client,//This order is important because it must match Keysharp.Core.Common.Keyboard.KeyboardHook.
		Window,
		Screen
	}
}