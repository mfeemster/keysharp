namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for mouse-related functions.
	/// </summary>
	public static class Mouse
	{
		/// <summary>
		/// Internal helper to lazy initialize and retrieve the <see cref="CoordModes"/> for the current thread.
		/// </summary>
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
		/// <param name="options">Specify one or more of the following components: Coords, WhichButton, ClickCount, DownOrUp and/or Relative.<br/>
		/// If all components are omitted, a single left click is performed at the mouse cursor's current position.<br/>
		/// The components can appear in any order except ClickCount, which must occur somewhere to the right of Coords, if present.<br/>
		///     Coords: If omitted, the cursor's current position is used. Otherwise, specify the X and Y coordinates to which the mouse cursor is moved prior to clicking.<br/>
		///     For example, Click "100 200" clicks the left mouse button at a specific position. Coordinates are relative to the active window's client area unless <see cref="CoordMode"/><br/>
		///     was used to change that.<br/>
		///     WhichButton: If omitted, it defaults to Left (the left mouse button). Otherwise, specify Left, Right, Middle (or just the first letter of each of these);<br/>
		///     or X1(fourth button) or X2(fifth button). For example, Click "Right" clicks the right mouse button at the mouse cursor's current position.<br/>
		///     Left and Right correspond to the primary button and secondary button. If the user swaps the buttons via system settings,<br/>
		///     the physical positions of the buttons are swapped but the effect stays the same.<br/>
		///     WhichButton can also be WheelUp or WU to turn the wheel upward(away from you), or WheelDown or WD to turn the wheel downward(toward you).<br/>
		///     WheelLeft(or WL) or WheelRight(or WR) may also be specified.ClickCount is the number of notches to turn the wheel.<br/>
		///     However, some applications do not obey a ClickCount value higher than 1 for the mouse wheel.For them, use the Click function multiple times by means such as Loop.<br/>
		///     ClickCount: If omitted, it defaults to 1. Otherwise, specify the number of times to click the mouse button or turn the mouse wheel.For example,<br/>
		///     Click 2 performs a double-click at the mouse cursor's current position. If Coords is specified, ClickCount must appear after it.<br/>
		///     Specify zero (0) to move the mouse without clicking; for example, Click "100 200 0".<br/>
		///     DownOrUp: If omitted, each click consists of a down-event followed by an up-event. Otherwise, specify the word Down (or the letter D)<br/>
		///     to press the mouse button down without releasing it.Later, use the word Up(or the letter U) to release the mouse button.<br/>
		///     For example, Click "Down" presses down the left mouse button and holds it.<br/>
		///     Relative: If omitted, the X and Y coordinates will be used for absolute positioning.<br/>
		///     Otherwise, specify the word Rel or Relative to treat the coordinates as offsets from the current mouse position.<br/>
		///     In other words, the cursor will be moved from its current position by X pixels to the right (left if negative) and Y pixels down(up if negative).
		/// </param>
		public static void Click(object options)
		{
			int x = 0, y = 0;
			var vk = 0u;
			var eventType = KeyEventTypes.KeyDown;
			var repeatCount = 0L;
			var moveOffset = false;
			var ht = Script.HookThread;
			ht.ParseClickOptions(options.As(), ref x, ref y, ref vk, ref eventType, ref repeatCount, ref moveOffset);
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			ht.kbdMsSender.PerformMouseCommon(repeatCount < 1 ? Actions.ACT_MOUSEMOVE : Actions.ACT_MOUSECLICK // Treat repeat-count<1 as a move (like {click}).
											  , vk, x, y, 0, 0, repeatCount, eventType, ThreadAccessors.A_DefaultMouseSpeed, moveOffset);//, true, true);
		}

		/// <summary>
		/// Sets coordinate mode for various built-in functions to be relative to either the active window or the screen.
		/// </summary>
		/// <param name="targetType">Specify one of the following words to indicate the type of target to affect:<br/>
		///     ToolTip: Affects <see cref="ToolTip"/>.<br/>
		///     Pixel: Affects <see cref="PixelGetColor"/>, <see cref="PixelSearch"/>, and <see cref="ImageSearch"/>.<br/>
		///     Mouse: Affects <see cref="MouseGetPos"/>, <see cref="Click"/>, <see cref="MouseMove"/>, <see cref="MouseClick"/>, and <see cref="MouseClickDrag"/>.<br/>
		///     Caret: Affects <see cref="CaretGetPos"/>.<br/>
		///     Menu: Affects the Menu.Show method when coordinates are specified for it.
		/// </param>
		/// <param name="relativeTo">If omitted, it defaults to Screen. Otherwise, specify one of the following words to indicate the area to which TargetType should be relative:<br/>
		///     Screen: Coordinates are relative to the desktop(entire screen).<br/>
		///     Window: Coordinates are relative to the active window.<br/>
		///     Client: Coordinates are relative to the active window's client area, which excludes the window's title bar, menu(if it has a standard one) and borders.<br/>
		///     Client coordinates are less dependent on OS version and theme.
		/// </param>
		/// <returns>The previous setting; either Screen, Window or Client.</returns>
		/// <exception cref="ValueError">A <see cref="ValueError"/> exception is thrown if targetType or relativeTo do not contain a valid values.</exception>
		public static CoordModeType CoordMode(object targetType, object relativeTo = null)
		{
			var target = targetType.As();
			var mode = relativeTo.As(Keyword_Screen);
			CoordModeType rel;

			if (relativeTo is CoordModeType cmt)
				rel = cmt;
			else if (Options.IsOption(mode, Keyword_Relative))
				rel = CoordModeType.Window;
			else if (Options.IsOption(mode, Keyword_Client))
				rel = CoordModeType.Client;
			else if (Options.IsOption(mode, Keyword_Window))
				rel = CoordModeType.Window;
			else if (Options.IsOption(mode, Keyword_Screen))
				rel = CoordModeType.Screen;
			else
				throw new ValueError($"Invalid RelativeTo value of '{mode}' passed to CoordMode().");

			CoordModeType prev;

			switch (target.ToLowerInvariant())
			{
				case Keyword_ToolTip:
					prev = Coords.Tooltip;
					Coords.Tooltip = rel;
					break;

				case Keyword_Pixel:
					prev = Coords.Pixel;
					Coords.Pixel = rel;
					break;

				case Keyword_Mouse:
					prev = Coords.Mouse;
					Coords.Mouse = rel;
					break;

				case Keyword_Caret:
					prev = Coords.Caret;
					Coords.Caret = rel;
					break;

				case Keyword_Menu:
					prev = Coords.Menu;
					Coords.Menu = rel;
					break;

				default:
					throw new ValueError($"Invalid TargetType value of '{target}' passed to CoordMode().");
			}

			return prev;
		}

		/// <summary>
		/// Clicks or holds down a mouse button, or turns the mouse wheel. Note: The Click function is generally more flexible and easier to use.
		/// </summary>
		/// <param name="whichButton">If blank or omitted, it defaults to Left (the left mouse button). Otherwise, specify the button to click or the rotate/push direction of the mouse wheel.<br/>
		///     Button: Left, Right, Middle(or just the first letter of each of these); or X1(fourth button) or X2(fifth button). For example: MouseClick "X1".<br/>
		///     Left and Right correspond to the primary button and secondary button.If the user swaps the buttons via system settings, the physical positions of the buttons are swapped but the effect stays the same.<br/>
		///     Mouse wheel: Specify WheelUp or WU to turn the wheel upward (away from you); specify WheelDown or WD to turn the wheel downward (toward you).<br/>
		///     Specify WheelLeft(or WL) or WheelRight(or WR) to push the wheel left or right, respectively.ClickCount is the number of notches to turn the wheel.
		/// </param>
		/// <param name="x">If omitted, the cursor's current position is used. Otherwise, specify the X and Y coordinates to which the mouse cursor is moved prior to clicking.<br/>
		/// Coordinates are relative to the active window's client area unless <see cref="CoordMode"/> was used to change that.
		/// </param>
		/// <param name="y">See <paramref name="x"/>.</param>
		/// <param name="clickCount">If omitted, it defaults to 1. Otherwise, specify the number of times to click the mouse button or turn the mouse wheel.</param>
		/// <param name="speed">If omitted, the default speed (as set by <see cref="SetDefaultMouseSpeed"/> or 2 otherwise) will be used. Otherwise, specify the speed to move the<br/>
		/// mouse in the range 0 (fastest) to 100 (slowest).<br/>
		/// A speed of 0 will move the mouse instantly.
		/// </param>
		/// <param name="downOrUp">If blank or omitted, each click consists of a down-event followed by an up-event. Otherwise, specify one of the following letters:<br/>
		///     D: Press the mouse button down but do not release it(i.e.generate a down-event).<br/>
		///     U: Release the mouse button (i.e.generate an up-event).
		/// </param>
		/// <param name="relative">If blank or omitted, the X and Y coordinates will be used for absolute positioning. Otherwise, specify the following letter:<br/>
		///     R: The X and Y coordinates will be treated as offsets from the current mouse position. In other words, the cursor will be moved from its current position<br/>
		///     by X pixels to the right (left if negative) and Y pixels down (up if negative).
		/// </param>
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

		/// <summary>
		/// Clicks and holds the specified mouse button, moves the mouse to the destination coordinates, then releases the button.
		/// </summary>
		/// <param name="whichButton">If blank or omitted, it defaults to Left (the left mouse button). Otherwise, specify Left, Right, Middle<br/>
		/// (or just the first letter of each of these); or X1 (fourth button) or X2 (fifth button). For example: MouseClickDrag "X1", 0, 0, 10, 10.<br/>
		/// Left and Right correspond to the primary button and secondary button.If the user swaps the buttons via system settings,<br/>
		/// the physical positions of the buttons are swapped but the effect stays the same.
		/// </param>
		/// <param name="x1">Specify the X and Y coordinates of the drag's starting position (the mouse will be moved to these coordinates right before the drag is started).<br/>
		/// Coordinates are relative to the active window's client area unless <see cref="CoordMode"/> was used to change that.<br/>
		/// If both X1 and Y1 are omitted, the mouse cursor's current position is used.
		/// </param>
		/// <param name="y1">See <paramref name="x1"/>.</param>
		/// <param name="x2">The X and Y coordinates to drag the mouse to (that is, while the button is held down).<br/>
		/// Coordinates are relative to the active window's client area unless <see cref="CoordMode"/> was used to change that.
		/// </param>
		/// <param name="y2">See <paramref name="x2"/>.</param>
		/// <param name="speed">If omitted, the default speed (as set by <see cref="SetDefaultMouseSpeed"/> or 2 otherwise) will be used. Otherwise, specify the speed to move the<br/>
		/// mouse in the range 0 (fastest) to 100 (slowest).<br/>
		/// A speed of 0 will move the mouse instantly.
		/// </param>
		/// <param name="relative">If blank or omitted, the X and Y coordinates will be used for absolute positioning. Otherwise, specify the following letter:<br/>
		///     R: The X and Y coordinates will be treated as offsets from the current mouse position. In other words, the cursor will be moved from its current position<br/>
		///     by X pixels to the right (left if negative) and Y pixels down (up if negative).
		/// </param>
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
		/// <param name="outputVarX">If omitted, the corresponding value will not be stored. Otherwise, specify references to the output variables in which to store the X and Y coordinates.<br/>
		/// The retrieved coordinates are relative to the active window's client area unless <see cref="CoordMode"/> was used to change to screen coordinates.
		/// </param>
		/// <param name="outputVarY">See <paramref name="outputVarX"/>.</param>
		/// <param name="outputVarWin">If omitted, the corresponding value will not be stored.<br/>
		/// Otherwise, specify a reference to the output variable in which to store the unique ID number of the window under the mouse cursor.<br/>
		/// If the window cannot be determined, this variable will be made blank.
		/// </param>
		/// <param name="outputVarControl">If omitted, the corresponding value will not be stored. Otherwise, specify a reference to the output variable in which to store the name (ClassNN)<br/>
		/// of the control under the mouse cursor. If the control cannot be determined, this variable will be made blank.
		/// </param>
		/// <param name="flag">If omitted, it defaults to 0, meaning the function uses the default method to determine OutputVarControl and stores the control's ClassNN.<br/>
		/// Otherwise, specify a combination (sum) of the following numbers:<br/>
		///     1: Uses a simpler method to determine OutputVarControl.This method correctly retrieves the active/topmost child window of an Multiple Document Interface(MDI)<br/>
		///     application such as SysEdit or TextPad. However, it is less accurate for other purposes such as detecting controls inside a GroupBox control.<br/>
		///     2: Stores the control's HWND in OutputVarControl rather than the control's ClassNN.
		/// </param>
		public static void MouseGetPos(ref object outputVarX, ref object outputVarY, ref object outputVarWin, ref object outputVarControl, object flag = null)
		{
			var mode = flag.Al(0L);
			var pos = Cursor.Position;
			var aX = 0;
			var aY = 0;
			PlatformProvider.Manager.CoordToScreen(ref aX, ref aY, Core.CoordMode.Mouse);//Determine where 0,0 in window or client coordinates are on the screen.
			outputVarX = (long)(pos.X - aX);//Convert the mouse position in screen coordinates to window coordinates.
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
				var pah = new PointAndHwnd(pos);//Find topmost control containing point.
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

		/// <summary>
		/// Moves the mouse cursor.
		/// </summary>
		/// <param name="x">The X and Y coordinates to move the mouse to. Coordinates are relative to the active window's client area unless <see cref="CoordMode"/> was used to change that.</param>
		/// <param name="y">See <paramref name="x"/>.</param>
		/// <param name="speed">If omitted, the default speed (as set by <see cref="SetDefaultMouseSpeed"/> or 2 otherwise) will be used. Otherwise, specify the speed to move the<br/>
		/// mouse in the range 0 (fastest) to 100 (slowest).<br/>
		/// A speed of 0 will move the mouse instantly.
		/// </param>
		/// <param name="relative">If blank or omitted, the X and Y coordinates will be used for absolute positioning. Otherwise, specify the following letter:<br/>
		///     R: The X and Y coordinates will be treated as offsets from the current mouse position. In other words, the cursor will be moved from its current position<br/>
		///     by X pixels to the right (left if negative) and Y pixels down (up if negative).
		/// </param>
		public static void MouseMove(object x, object y, object speed = null, object relative = null)
		{
			var ix = x.Ai(KeyboardMouseSender.CoordUnspecified);
			var iy = y.Ai(KeyboardMouseSender.CoordUnspecified);
			var s = (int)speed.Al(ThreadAccessors.A_DefaultMouseSpeed);
			var r = relative.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSEMOVE, "", ix, iy, KeyboardMouseSender.CoordUnspecified, KeyboardMouseSender.CoordUnspecified,
						 s, r, 1, "");
		}

		/// <summary>
		/// Sets the mouse speed that will be used if unspecified in <see cref="Click"/>, <see cref="MouseMove"/>, <see cref="MouseClick"/>, and <see cref="MouseClickDrag"/>.
		/// </summary>
		/// <param name="speed">The speed to move the mouse in the range 0 (fastest) to 100 (slowest). A speed of 0 will move the mouse instantly.</param>
		public static void SetDefaultMouseSpeed(object speed) => Accessors.A_DefaultMouseSpeed = speed;

		/// <summary>
		/// Sets the delay that will occur after each mouse movement or click.
		/// </summary>
		/// <param name="delay">Time in milliseconds. Specify -1 for no delay at all or 0 for the smallest possible delay<br/>
		/// (however, if the Play parameter is present, both 0 and -1 produce no delay).
		/// </param>
		/// <param name="play">If blank or omitted, the delay is applied to the traditional SendEvent mode. Otherwise, specify the word Play to apply the delay to the SendPlay mode.<br/>
		/// If a script never uses this parameter, the delay is always -1 for SendPlay.
		/// </param>
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

		/// <summary>
		/// Internal helper to adjust a point from being relative to the active window to relative to the top left of the screen.
		/// The adjustment will only be done if the current mouse coordinate mode is <see cref="CoordModeType.Window"/> or <see cref="CoordModeType.Client"/>.
		/// </summary>
		/// <param name="x">The x point to adjust.</param>
		/// <param name="y">The y point to adjust.</param>
		internal static void AdjustPoint(ref int x, ref int y)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				var rect = WindowProvider.Manager.ActiveWindow.Location;
				x += rect.Left;
				y += rect.Top;
			}
			else if (Coords.Mouse == CoordModeType.Client)
			{
				var pt = WindowProvider.Manager.ActiveWindow.ClientToScreen();
				x += pt.X;
				y += pt.Y;
			}
		}

		/// <summary>
		/// Internal helper to adjust a rect from being relative to the active window to relative to the top left of the screen.
		/// The adjustment will only be done if the current mouse coordinate mode is <see cref="CoordModeType.Window"/> or <see cref="CoordModeType.Client"/>.
		/// </summary>
		/// <param name="x1">The left of the rect to adjust.</param>
		/// <param name="y1">The top of the rect to adjust.</param>
		/// <param name="x2">The right of the rect to adjust.</param>
		/// <param name="y2">The bottom of the rect to adjust.</param>
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
			else if (Coords.Mouse == CoordModeType.Client)
			{
				var pt = WindowProvider.Manager.ActiveWindow.ClientToScreen();
				x1 += pt.X;
				y1 += pt.Y;
				x2 += pt.X;
				y2 += pt.Y;
			}
		}

		/// <summary>
		/// Internal helper to adjust a point from being relative to the top left of the screen to being relative to the active window.
		/// </summary>
		/// <param name="p">The point to adjust.</param>
		/// <param name="modeType">The current coordinate mode, which must be <see cref="CoordModeType.Window"/> for the adjustment to take place.</param>
		/// <returns>The adjusted point.</returns>
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

		/// <summary>
		/// Internal helper to carry out various mouse operations.
		/// See the mouse functions for descriptions of the parameters.
		/// </summary>
		/// <param name="actionType"></param>
		/// <param name="button"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="speed"></param>
		/// <param name="relative"></param>
		/// <param name="repeatCount"></param>
		/// <param name="downUp"></param>
		/// <exception cref="ValueError">A <see cref="ValueError"/> exception is thrown if an invalid button, repeat count or down/up value is specified.</exception>
		private static void PerformMouse(Actions actionType, string button, int x1, int y1, int x2, int y2
										 , int speed, string relative, long repeatCount, string downUp)
		{
			uint vk;
			var ht = Script.HookThread;

			if (actionType == Actions.ACT_MOUSEMOVE)
				vk = 0;
			else if ((vk = ht.ConvertMouseButton(button, actionType == Actions.ACT_MOUSECLICK)) == 0)
				throw new ValueError($"Invalid mouse button type of {button}.");

			// v1.0.43: Seems harmless (due to rarity) to treat invalid button names as "Left" (keeping in
			// mind that due to loadtime validation, invalid buttons are possible only when the button name is
			// contained in a variable, e.g. MouseClick %ButtonName%.
			var eventType = KeyEventTypes.KeyDownAndUp;  // Set defaults.

			if (repeatCount < 0)
				throw new ValueError($"Invalid repeat count of {repeatCount}. It must be >= 0.");

			//if (actionType == Actions.ACT_MOUSECLICK)
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
							throw new ValueError($"Invalid down/up value of {downUp[0]}. It must be 'u' or 'd'.");
					}
				}
			}

			if (!string.IsNullOrEmpty(relative) && relative != "R")
				throw new ValueError($"Invalid relative value of {relative}. It must be empty or 'R'.");

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

	/// <summary>
	/// Class to hold the coordinate modes for various operations.
	/// </summary>
	internal class CoordModes
	{
		/// <summary>
		/// The coordinate mode for caret operations.
		/// </summary>
		public CoordModeType Caret { get; set; } = CoordModeType.Client;

		/// <summary>
		/// The coordinate mode for menu operations.
		/// </summary>
		public CoordModeType Menu { get; set; } = CoordModeType.Client;

		/// <summary>
		/// The coordinate mode for mouse operations.
		/// </summary>
		public CoordModeType Mouse { get; set; } = CoordModeType.Client;

		/// <summary>
		/// The coordinate mode for pixel operations.
		/// </summary>
		public CoordModeType Pixel { get; set; } = CoordModeType.Client;

		/// <summary>
		/// The coordinate mode for tool tip operations.
		/// </summary>
		public CoordModeType Tooltip { get; set; } = CoordModeType.Client;

		/// <summary>
		/// Retrieves the coordinate mode for the specified operation.
		/// </summary>
		/// <param name="mode">The operation to retrieve the coordinate mode for.</param>
		/// <returns>The coordinate mode for the specified operation.</returns>
		/// <exception cref="ValueError">A <see cref="ValueError"/> exception is thrown if an invalid coordinate operation is specified.</exception>
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
					throw new ValueError($"Invalid coordinate mode type: {mode}");
			}
		}
	}

	/// <summary>
	/// An Enum for the various operations which use different coordinate modes.
	/// </summary>
	public enum CoordMode
	{
		Caret,
		Menu,
		Mouse,
		Pixel,
		Tooltip
	}

	/// <summary>
	/// An Enum for the various coordinate mode types.
	/// </summary>
	public enum CoordModeType
	{
		Client,//This order is important because it must match Keysharp.Core.Common.Keyboard.KeyboardHook.
		Window,
		Screen
	}
}