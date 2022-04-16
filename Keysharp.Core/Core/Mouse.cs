using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT
using static Keysharp.Scripting.Script;

namespace Keysharp.Core
{
	public static class Mouse
	{
		[ThreadStatic]
		private static CoordModes coords = new CoordModes();

		internal static CoordModes Coords => coords ?? (coords = new CoordModes());//All thread statis members need to be initialized like this.//TOOD

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
		public static void Click(params object[] obj)
		{
			var options = obj.L().S1();
			int x = 0, y = 0, vk = 0;
			var event_type = Keysharp.Core.Common.Keyboard.KeyEventTypes.KeyDown;
			var repeat_count = 0;
			var move_offset = false;
			var ht = Keysharp.Scripting.Script.HookThread;
			ht.ParseClickOptions(options, ref x, ref y, ref vk, ref event_type, ref repeat_count, ref move_offset);
			PerformMouseCommon(repeat_count < 1 ? Actions.ACT_MOUSEMOVE : Actions.ACT_MOUSECLICK // Treat repeat-count<1 as a move (like {click}).
							   , vk, x, y, 0, 0, repeat_count, event_type, (int)Accessors.A_DefaultMouseSpeed, move_offset);
			/*
			    var ParamLine = string.Empty;
			    var MousePos = new Point(0, 0);
			    var ClickCount = 1;
			    const string delimiter = ",";
			    var RE_Coord = new Regex(@"(\d*?)\s*,\s*(\d*)[\s,\,]*", RegexOptions.IgnoreCase);
			    var RE_Num = new Regex(@"\d+", RegexOptions.IgnoreCase);
			    CaptureCollection Out;
			    Match Match;

			    //rebuild Argument string, as we have to parse this in a special way
			    foreach (var option in options)
			    {
			    if (option is string os)
			        ParamLine += os + delimiter;
			    else if (option is double od)
			        ParamLine += ((int)od) + delimiter;
			    }

			    ParamLine = ParamLine.ToLower().Substring(0, ParamLine.Length - 1);

			    //search coordinates, move mouse, remove them
			    if (RE_Coord.IsMatch(ParamLine))
			    {
			    Match = RE_Coord.Match(ParamLine);
			    MousePos.X = Convert.ToInt32(Match.Groups[1].Value);
			    MousePos.Y = Convert.ToInt32(Match.Groups[2].Value);
			    ParamLine = RE_Coord.Replace(ParamLine, string.Empty); //remove coord

			    if (Coords.Mouse == CoordModeType.Window)
			    {
			        var foreGroundWindow = WindowManagerProvider.Instance.ActiveWindow;

			        if (foreGroundWindow != null)
			        {
			            var location = foreGroundWindow.Location;
			            MousePos.X += location.X;
			            MousePos.Y += location.Y;
			        }
			    }

			    Cursor.Position = MousePos;
			    }

			    //click count
			    if (RE_Num.IsMatch(ParamLine))
			    {
			    Out = RE_Num.Match(ParamLine).Captures;
			    ClickCount = Convert.ToInt32(Out[0].Value);

			    if (ClickCount <= 0)
			        return;
			    }

			    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			    {
			    var aInput = new INPUT[2];

			    //right or left mouse
			    if (ParamLine.Contains(Core.Keyword_Right))
			    {
			        aInput[0].i.m.dwFlags = (uint)MOUSEEVENTF.RIGHTDOWN;
			        aInput[1].i.m.dwFlags = (uint)MOUSEEVENTF.RIGHTUP;
			    }
			    else
			    {
			        aInput[0].i.m.dwFlags = (uint)MOUSEEVENTF.LEFTDOWN;
			        aInput[1].i.m.dwFlags = (uint)MOUSEEVENTF.LEFTUP;
			    }

			    //down event
			    aInput[0].type = WindowsAPI.INPUT_MOUSE;
			    aInput[0].i.m.dwExtraInfo = 0;
			    aInput[0].i.m.mouseData = 0;
			    aInput[0].i.m.time = 0;
			    aInput[0].i.m.dx = MousePos.X;
			    aInput[0].i.m.dy = MousePos.Y;
			    //up event
			    aInput[1].type = WindowsAPI.INPUT_MOUSE;
			    aInput[1].i.m.dwExtraInfo = 0;
			    aInput[1].i.m.mouseData = 0;
			    aInput[1].i.m.time = 0;
			    aInput[1].i.m.dx = MousePos.X;
			    aInput[1].i.m.dy = MousePos.Y;

			    if (ParamLine.Contains(Core.Keyword_Up))
			    {
			        //just send the up event:
			        aInput[0] = aInput[1];

			        for (var i = 1; ClickCount >= i; i++)
			            _ = WindowsAPI.SendInput(1, aInput, Marshal.SizeOf(typeof(INPUT)));
			    }
			    else if (ParamLine.Contains(Core.Keyword_Down))
			    {
			        //just send the down event:
			        for (var i = 1; ClickCount >= i; i++)
			            _ = WindowsAPI.SendInput(1, aInput, Marshal.SizeOf(typeof(INPUT)));
			    }
			    else //send both events:
			        for (var i = 1; ClickCount >= i; i++)
			            _ = WindowsAPI.SendInput((uint)aInput.Length, aInput, Marshal.SizeOf(typeof(INPUT)));
			    }
			*/
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
		public static void CoordMode(params object[] obj)
		{
			var (target, mode) = obj.L().S2("", Core.Keyword_Screen);
			CoordModeType rel;

			if (Options.IsOption(mode, Core.Keyword_Relative))
				rel = CoordModeType.Window;
			else if (Options.IsOption(mode, Core.Keyword_Client))
				rel = CoordModeType.Client;
			else if (Options.IsOption(mode, Core.Keyword_Window))
				rel = CoordModeType.Window;
			else if (Options.IsOption(mode, Core.Keyword_Screen))
				rel = CoordModeType.Screen;
			else
				throw new ValueError("Invalid value.");

			switch (target.ToLowerInvariant())
			{
				case Core.Keyword_ToolTip: Coords.Tooltip = rel; break;

				case Core.Keyword_Pixel: Coords.Pixel = rel; break;

				case Core.Keyword_Mouse: Coords.Mouse = rel; break;

				case Core.Keyword_Caret: Coords.Caret = rel; break;

				case Core.Keyword_Menu: Coords.Menu = rel; break;
			}
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
		public static void MouseGetPos(out int x, out int y, out long win, out string control, int mode = 0)
		{
			win = 0;
			control = null;
			var cid = (mode & 2) == 2;
			var pos = Cursor.Position;
			var found = Window.WindowManager.WindowFromPoint(pos);
			win = found.Handle.ToInt32();
			var foundLocation = found.Location;
			var child = found.RealChildWindowFromPoint(new Point(pos.X - foundLocation.X, pos.Y - foundLocation.Y));
			control = cid ? child.Handle.ToInt64().ToString() : child.ClassNN;
			x = pos.X;
			y = pos.Y;

			if (Coords.Mouse == CoordModeType.Window)
			{
				var location = Window.WindowManager.GetForeGroundWindow().Location;
				x -= location.X;
				y -= location.Y;
			}
		}

		public static void SetDefaultMouseSpeed(params object[] obj)
		{
			var o = obj.L();

			if (o.Count > 0)
			{
				Accessors.A_DefaultMouseSpeed = o[0].ParseLong(true).Value;
			}
		}

		public static void SetMouseDelay(params object[] obj)
		{
			var o = obj.L();
			var play = string.Empty;

			if (o.Count > 1)
			{
				if (o[1] is string s)
					play = s.ToLowerInvariant();
			}

			var isplay = play == "play";
			var del = isplay ? Accessors.A_MouseDelayPlay : Accessors.A_MouseDelay;

			if (o.Count > 0)
			{
				var po = o[0];

				if (po != null)
					del = po.ParseLong(true).Value;
			}

			if (isplay)
				Accessors.A_MouseDelayPlay = del;
			else
				Accessors.A_MouseDelay = del;
		}

		internal static void AdjustPoint(ref int x, ref int y)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				_ = WindowsAPI.GetWindowRect(WindowsAPI.GetForegroundWindow(), out var rect);//Need a cross platform way to do this.//MATT
				x += rect.Left;
				y += rect.Top;
			}
		}

		internal static void AdjustRect(ref int x1, ref int y1, ref int x2, ref int y2)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				_ = WindowsAPI.GetWindowRect(WindowsAPI.GetForegroundWindow(), out var rect);//Need a cross platform way to do this.//MATT
				x1 += rect.Left;
				y1 += rect.Top;
				x2 += rect.Left;
				y2 += rect.Top;
			}
		}

		internal static void MouseClickDrag(int vk, int x1, int y1, int x2, int y2, int speed, bool moveOffset)
		{
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMsSender = ht.kbdMsSender;

			// Check if one of the coordinates is missing, which can happen in cases where this was called from
			// a source that didn't already validate it. Can't call Line::ValidateMouseCoords() because that accepts strings.
			if ((x1 == KeyboardMouseSender.CoordUnspecified && y1 != KeyboardMouseSender.CoordUnspecified) || (x1 != KeyboardMouseSender.CoordUnspecified && y1 == KeyboardMouseSender.CoordUnspecified)
					|| (x2 == KeyboardMouseSender.CoordUnspecified && y2 != KeyboardMouseSender.CoordUnspecified) || (x2 != KeyboardMouseSender.CoordUnspecified && y2 == KeyboardMouseSender.CoordUnspecified))
				return;

			// I asked Jon, "Have you discovered that insta-drags almost always fail?" and he said
			// "Yeah, it was weird, absolute lack of drag... Don't know if it was my config or what."
			// However, testing reveals "insta-drags" work ok, at least on my system, so leaving them enabled.
			// User can easily increase the speed if there's any problem:
			//if (aSpeed < 2)
			//  aSpeed = 2;
			// v2.0: Always translate logical buttons into physical ones.  Which physical button it becomes depends
			// on whether the mouse buttons are swapped via the Control Panel.  Note that journal playback doesn't
			// need the swap because every aspect of it is "logical".
			if ((vk == WindowsAPI.VK_LBUTTON || vk == WindowsAPI.VK_RBUTTON) && Accessors.SendMode != SendModes.Play && WindowsAPI.GetSystemMetrics(SystemMetric.SM_SWAPBUTTON) != 0)
				vk = (vk == WindowsAPI.VK_LBUTTON) ? WindowsAPI.VK_RBUTTON : WindowsAPI.VK_LBUTTON;//Need to figure out making this cross platform.//TODO

			// MSDN: If [event_flags] is not MOUSEEVENTF_WHEEL, [MOUSEEVENTF_HWHEEL,] MOUSEEVENTF_XDOWN,
			// or MOUSEEVENTF_XUP, then [event_data] should be zero.
			var eventdata = 0u; // Set defaults for some.
			var eventup = 0u;
			var eventdown = 0u;
			var eventflags = 0u;

			switch (vk)
			{
				case WindowsAPI.VK_LBUTTON:
					eventdown = (uint)MOUSEEVENTF.LEFTDOWN;
					eventup = (uint)MOUSEEVENTF.LEFTUP;
					break;

				case WindowsAPI.VK_RBUTTON:
					eventdown = (uint)MOUSEEVENTF.RIGHTDOWN;
					eventup = (uint)MOUSEEVENTF.RIGHTUP;
					break;

				case WindowsAPI.VK_MBUTTON:
					eventdown = (uint)MOUSEEVENTF.MIDDLEDOWN;
					eventup = (uint)MOUSEEVENTF.MIDDLEUP;
					break;

				case WindowsAPI.VK_XBUTTON1:
				case WindowsAPI.VK_XBUTTON2:
					eventdown = (uint)MOUSEEVENTF.XDOWN;
					eventup = (uint)MOUSEEVENTF.XUP;
					eventdata = (vk == WindowsAPI.VK_XBUTTON1) ? (uint)WindowsAPI.XBUTTON1 : (uint)WindowsAPI.XBUTTON2;
					break;
			}

			// If the drag isn't starting at the mouse's current position, move the mouse to the specified position:
			if (x1 != KeyboardMouseSender.CoordUnspecified && y1 != KeyboardMouseSender.CoordUnspecified)
			{
				// The movement must be a separate event from the click, otherwise it's completely unreliable with
				// SendInput() and probably keybd_event() too.  SendPlay is unknown, but it seems best for
				// compatibility and peace-of-mind to do it for that too.  For example, some apps may be designed
				// to expect mouse movement prior to a click at a *new* position, which is not unreasonable given
				// that this would be the case 99.999% of the time if the user were moving the mouse physically.
				kbdMsSender.MouseMove(ref x1, ref y1, ref eventflags, speed, moveOffset); // It calls DoMouseDelay() and also converts aX1 and aY1 to MOUSEEVENTF_ABSOLUTE coordinates.
				// v1.0.43: event_flags was added to improve reliability.  Explanation: Since the mouse was just moved to an
				// explicitly specified set of coordinates, use those coordinates with subsequent clicks.  This has been
				// shown to significantly improve reliability in cases where the user is moving the mouse during the
				// MouseClick/Drag commands.
			}

			kbdMsSender.MouseEvent(eventflags | eventdown, eventdata, x1, y1); // It ignores aX and aY when MOUSEEVENTF_MOVE is absent.
			kbdMsSender.DoMouseDelay(); // Inserts delay for all modes except SendInput, for which it does nothing.
			// Now that the mouse button has been pushed down, move the mouse to perform the drag:
			kbdMsSender.MouseMove(ref x2, ref y2, ref eventflags, speed, moveOffset); // It calls DoMouseDelay() and also converts aX2 and aY2 to MOUSEEVENTF_ABSOLUTE coordinates.
			kbdMsSender.DoMouseDelay(); // Duplicate, see below.
			// Above is a *duplicate* delay because MouseMove() already did one. But it seems best to keep it because:
			// 1) MouseClickDrag can be a CPU intensive operation for the target window depending on what it does
			//    during the drag (selecting objects, etc.)  Thus, and extra delay might help a lot of things.
			// 2) It would probably break some existing scripts to remove the delay, due to timing issues.
			// 3) Dragging is pretty rarely used, so the added performance of removing the delay wouldn't be
			//    a big benefit.
			kbdMsSender.MouseEvent(eventflags | eventup, eventdata, x2, y2); // It ignores aX and aY when MOUSEEVENTF_MOVE is absent.
			kbdMsSender.DoMouseDelay();
			// Above line: It seems best to always do this delay too in case the script line that
			// caused us to be called here is followed immediately by another script line which
			// is either another mouse click or something that relies upon this mouse drag
			// having been completed:
		}

		internal static void PerformMouseCommon(Actions actionType, int vk, int x1, int y1, int x2, int y2,
												int repeatCount, Keysharp.Core.Common.Keyboard.KeyEventTypes eventType, int speed, bool moveOffset)
		{
			// The maximum number of events, which in this case would be from a MouseClickDrag.  To be conservative
			// (even though INPUT is a much larger struct than PlaybackEvent and SendInput doesn't use mouse-delays),
			// include room for the maximum number of mouse delays too.
			// Drag consists of at most:
			// 1) Move; 2) Delay; 3) Down; 4) Delay; 5) Move; 6) Delay; 7) Delay (dupe); 8) Up; 9) Delay.
			const int MAX_PERFORM_MOUSE_EVENTS = 10;
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMsSender = ht.kbdMsSender;
			//Original differentiates between a thread specific value for sendmode and a global static one. Here, we just use the thread specific one.

			if (Accessors.SendMode == Common.Keyboard.SendModes.Input || Accessors.SendMode == Common.Keyboard.SendModes.InputThenPlay)
			{
				if (ht.SystemHasAnotherMouseHook()) // See similar section in SendKeys() for details.
					Accessors.SendMode = (Accessors.SendMode == Common.Keyboard.SendModes.Input) ? Common.Keyboard.SendModes.Event : Common.Keyboard.SendModes.Play;
				else
					Accessors.SendMode = Common.Keyboard.SendModes.Input; // Resolve early so that other sections don't have to consider SM_INPUT_FALLBACK_TO_PLAY a valid value.
			}

			if (Accessors.SendMode != Common.Keyboard.SendModes.Event) // We're also responsible for setting sSendMode to SM_EVENT prior to returning.
				kbdMsSender.InitEventArray(MAX_PERFORM_MOUSE_EVENTS, 0);

			// Turn it on unconditionally even if it was on, since Ctrl-Alt-Del might have disabled it.
			// Turn it back off only if it wasn't ON before we started.
			var blockinput_prev = Keysharp.Core.Keyboard.blockInput;
			var do_selective_blockinput = (Keysharp.Core.Keyboard.blockInputMode == Common.Keyboard.ToggleValueType.Mouse
										   || Keysharp.Core.Keyboard.blockInputMode == Common.Keyboard.ToggleValueType.SendAndMouse)
										  && Accessors.SendMode == Common.Keyboard.SendModes.Event;

			if (do_selective_blockinput) // It seems best NOT to use g_BlockMouseMove for this, since often times the user would want keyboard input to be disabled too, until after the mouse event is done.
				Keysharp.Core.Keyboard.ScriptBlockInput(true); // Turn it on unconditionally even if it was on, since Ctrl-Alt-Del might have disabled it.

			switch (actionType)
			{
				case Actions.ACT_MOUSEMOVE:
					var unused = 0u;
					kbdMsSender.MouseMove(ref x1, ref y1, ref unused, speed, moveOffset); // Does nothing if coords are invalid.
					break;

				case Actions.ACT_MOUSECLICK:
					kbdMsSender.MouseClick(vk, x1, y1, repeatCount, speed, eventType, moveOffset); // Does nothing if coords are invalid.
					break;

				case Actions.ACT_MOUSECLICKDRAG:
					MouseClickDrag(vk, x1, y1, x2, y2, speed, moveOffset); // Does nothing if coords are invalid.
					break;
			}

			if (Accessors.SendMode != Common.Keyboard.SendModes.Event)
			{
				var final_key_delay = -1; // Set default.

				if (!kbdMsSender.abortArraySend)
					kbdMsSender.SendEventArray(ref final_key_delay, 0); // Last parameter is ignored because keybd hook isn't removed for a pure-mouse SendInput.

				kbdMsSender.CleanupEventArray(final_key_delay);
			}

			if (do_selective_blockinput && !blockinput_prev)  // Turn it back off only if it was off before we started.
				Keysharp.Core.Keyboard.ScriptBlockInput(false);
		}

		/// <summary>
		/// Clicks and holds the specified mouse button, moves the mouse to the destination coordinates, then releases the button.
		/// </summary>
		/// <param name="button">Either <c>Left</c> (default), <c>Middle</c>, <c>Right</c>, <c>X1</c> or <c>X2</c>.</param>
		/// <param name="x1">The starting x-coordinate.</param>
		/// <param name="y1">The starting y-coordinate.</param>
		/// <param name="x2">The final x-coordinate.</param>
		/// <param name="y2">The final y-coordinate.</param>
		/// <param name="speed">The speed to move the mouse from 0 (fastest) to 100 (slowest).
		/// The default speed is determined by <see cref="A_DefaultMouseSpeed"/>.</param>
		/// <param name="relative"><c>true</c> to treat the first set of coordinates as relative offsets from the current mouse position
		/// and the second set as offsets from the first, <c>false</c> otherwise.</param>
		//public static void MouseClickDrag(string button, int x1, int y1, int x2, int y2, int? speed = null, bool relative = false)//Might need to go in, or use, kbdmssender.//TODO
		//{
		//  var oldSpeed = Accessors.A_DefaultMouseSpeed;
		//
		//  if (speed != null)
		//      Accessors.A_DefaultMouseSpeed = (int)speed;
		//
		//  var opts = new[] { x1.ToString(), y1.ToString(), relative ? Core.Keyword_Relative : string.Empty };
		//  Click(opts);
		//  opts[0] = x2.ToString();
		//  opts[1] = y2.ToString();
		//  Click(opts);
		//
		//  if (speed != null)
		//      Accessors.A_DefaultMouseSpeed = oldSpeed;
		//}
		internal static Point RevertPoint(Point p, CoordModeType modeType)
		{
			//for cross platform purposes, should use something like Form.ActiveForm.PointToScreen() etc...
			if (modeType == CoordModeType.Window)//This does not account for the mode value of other coord settings, like menu.//MATT
			{
				_ = WindowsAPI.GetWindowRect(WindowsAPI.GetForegroundWindow(), out var rect);//Need a cross platform way to do this.//MATT
				return new Point(p.X - rect.Left, p.Y - rect.Top);
			}

			return p;
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