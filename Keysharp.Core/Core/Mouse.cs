using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;
using static Keysharp.Core.Windows.WindowsAPI;//Code in Core probably shouldn't be referencing windows specific code.//TODO

namespace Keysharp.Core
{
	public static class Mouse
	{
		[ThreadStatic]
		private static CoordModes coords = new CoordModes();

		internal static CoordModes Coords => coords ?? (coords = new CoordModes());//All thread static members need to be initialized like this.//TOOD

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
		public static void Click(object obj)
		{
			var options = obj.As();
			int x = 0, y = 0, vk = 0;
			var event_type = Keysharp.Core.Common.Keyboard.KeyEventTypes.KeyDown;
			var repeat_count = 0L;
			var move_offset = false;
			var ht = Keysharp.Scripting.Script.HookThread;
			ht.ParseClickOptions(options, ref x, ref y, ref vk, ref event_type, ref repeat_count, ref move_offset);
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			ht.kbdMsSender.PerformMouseCommon(repeat_count < 1 ? Actions.ACT_MOUSEMOVE : Actions.ACT_MOUSECLICK // Treat repeat-count<1 as a move (like {click}).
											  , vk, x, y, 0, 0, repeat_count, event_type, (long)Accessors.A_DefaultMouseSpeed, move_offset);//, true, true);
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
		public static void CoordMode(object obj0, object obj1 = null)
		{
			var target = obj0.As();
			var mode = obj1.As(Core.Keyword_Screen);
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

		public static void MouseClick(object obj0 = null, object obj1 = null, object obj2 = null, object obj3 = null, object obj4 = null, object obj5 = null, object obj6 = null)
		{
			var whichButton = obj0.As();
			var x = (int)obj1.Al(KeyboardMouseSender.CoordUnspecified);// If no starting coords are specified, mark it as "use the current mouse position".
			var y = (int)obj2.Al(KeyboardMouseSender.CoordUnspecified);
			var repeatCount = obj3.Al(1);
			var speed = (int)obj4.Al((long)Accessors.A_DefaultMouseSpeed);
			var downOrUp = obj5.As();
			var relative = obj6.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSECLICK, whichButton, x, y, KeyboardMouseSender.CoordUnspecified, KeyboardMouseSender.CoordUnspecified,
						 speed, relative, repeatCount, downOrUp);//, true, true);
		}

		public static void MouseClickDrag(object obj0, object obj1, object obj2, object obj3, object obj4, object obj5 = null, object obj6 = null)
		{
			var whichButton = obj0.As();
			var x1 = (int)obj1.Al(KeyboardMouseSender.CoordUnspecified);//If no starting coords are specified, mark it as "use the current mouse position".
			var y1 = (int)obj2.Al(KeyboardMouseSender.CoordUnspecified);
			var x2 = (int)obj3.Al(KeyboardMouseSender.CoordUnspecified);
			var y2 = (int)obj4.Al(KeyboardMouseSender.CoordUnspecified);
			var speed = (int)obj5.Al((long)Accessors.A_DefaultMouseSpeed);
			var relative = obj6.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSECLICKDRAG, whichButton, x1, y1, x2, y2,
						 speed, relative, 1, "");//, true, true);
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
		public static Keysharp.Core.Map MouseGetPos(object obj = null)
		{
			var mode = obj.Al();
			var pos = Cursor.Position;
			var found = Window.WindowManager.WindowFromPoint(pos);
			var win = found.Handle.ToInt64();
			var x = (long)pos.X;
			var y = (long)pos.Y;

			if ((mode & 0x01) == 0)
			{
				var pah = new Keysharp.Core.Common.Window.PointAndHwnd(new POINT() { x = pos.X, y = pos.Y });//Find topmost control containing point.
				found.ChildFindPoint(pah);

				if (pah.hwndFound != IntPtr.Zero)
					found = Keysharp.Core.Common.Window.WindowManagerProvider.Instance.CreateWindow(pah.hwndFound);
			}

			var control = (mode & 2) == 2 ? found.Handle.ToInt64().ToString() : found.ClassNN;

			if (Coords.Mouse == CoordModeType.Window)
			{
				var location = Window.WindowManager.GetForeGroundWindow().Location;
				x -= location.X;
				y -= location.Y;
			}

			return new Keysharp.Core.Map(new Dictionary<object, object>()
			{
				{ "X", x },
				{ "Y", y },
				{ "Win", win },
				{ "Control", control }
			});
		}

		public static void MouseMove(object obj0, object obj1, object obj2 = null, object obj3 = null)
		{
			var x = (int)obj0.Al(KeyboardMouseSender.CoordUnspecified);
			var y = (int)obj1.Al(KeyboardMouseSender.CoordUnspecified);
			var speed = (int)obj2.Al((long)Accessors.A_DefaultMouseSpeed);
			var relative = obj3.As();
			//Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
			PerformMouse(Actions.ACT_MOUSEMOVE, "", x, y, KeyboardMouseSender.CoordUnspecified, KeyboardMouseSender.CoordUnspecified,
						 speed, relative, 1, "");//, true, true);
		}

		public static void SetDefaultMouseSpeed(object obj) => Accessors.A_DefaultMouseSpeed = obj;

		public static void SetMouseDelay(object obj0, object obj1 = null)
		{
			var play = string.Empty;
			play = obj1.As().ToLowerInvariant();
			var isplay = play == "play";
			var del = isplay ? Accessors.A_MouseDelayPlay : Accessors.A_MouseDelay;

			if (obj0 != null)
				del = obj0.Al();

			if (isplay)
				Accessors.A_MouseDelayPlay = del;
			else
				Accessors.A_MouseDelay = del;
		}

		internal static void AdjustPoint(ref int x, ref int y)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				_ = WindowsAPI.GetWindowRect(WindowsAPI.GetForegroundWindow(), out var rect);//Need a cross platform way to do this.//TODO
				x += rect.Left;
				y += rect.Top;
			}
		}

		internal static void AdjustRect(ref int x1, ref int y1, ref int x2, ref int y2)
		{
			if (Coords.Mouse == CoordModeType.Window)
			{
				_ = WindowsAPI.GetWindowRect(WindowsAPI.GetForegroundWindow(), out var rect);//Need a cross platform way to do this.//TODO
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
				_ = WindowsAPI.GetWindowRect(WindowsAPI.GetForegroundWindow(), out var rect);//Need a cross platform way to do this.//TODO
				return new Point(p.X - rect.Left, p.Y - rect.Top);
			}

			return p;
		}

		private static void PerformMouse(Actions actionType, string button, int x1, int y1, int x2, int y2
										 , int speed, string relative, long repeatCount, string downUp)
		{
			int vk;
			var ht = Keysharp.Scripting.Script.HookThread;

			if (actionType == Actions.ACT_MOUSEMOVE)
			{
				vk = 0;
			}
			else
			{
				if ((vk = ht.ConvertMouseButton(button, actionType == Actions.ACT_MOUSECLICK)) == 0)
					vk = VK_LBUTTON;
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
											  , relative.Length > 0 && char.ToUpper(relative[0]) == 'R');//, true, true);
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