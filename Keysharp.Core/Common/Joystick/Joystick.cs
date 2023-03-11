using System;
using System.Runtime.InteropServices;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;

namespace Keysharp.Core.Common.Joystick
{
	internal static class Joystick
	{
		internal static uint[] buttonsPrev = new uint[MaxJoysticks];
		internal static int MaxJoyButtons = 32;
		internal static int MaxJoysticks = 16;  // The maximum allowed by any Windows operating system.

		/// <summary>
		/// The caller TextToKey() currently relies on the fact that when aAllowOnlyButtons==true, a value
		/// that can fit in a sc_type (USHORT) is returned, which is true since the joystick buttons
		/// are very small numbers (JOYCTRL_1==12).
		/// </summary>
		internal static JoyControls ConvertJoy(string buf, ref int? joystickID, bool allowOnlyButtons = false)
		{
			if (joystickID != null)
				joystickID = 0;  // Set default output value for the caller.

			if (string.IsNullOrEmpty(buf))
				return JoyControls.Invalid;

			var index = 0;

			for (; index < buf.Length && buf[index] >= '0' && buf[index] <= '9'; ++index) ; // self-contained loop to find the first non-digit.

			if (index < buf.Length) // The string starts with a number.
			{
				var val = buf.ParseInt(false);
				var joystick_id = val.HasValue ? val.Value - 1 : 0;

				if (joystick_id < 0 || joystick_id >= MaxJoysticks)
					return JoyControls.Invalid;

				if (joystickID != null)
					joystickID = joystick_id;  // Use ATOI vs. atoi even though hex isn't supported yet.
			}

			if (buf.StartsWith("Joy", StringComparison.OrdinalIgnoreCase))
			{
				var sub = buf.Substring(3);
				var val = sub.ParseInt(false);

				if (val.HasValue)
				{
					if (val.Value < 1 || val.Value > MaxJoyButtons)
						return JoyControls.Invalid;

					return JoyControls.Button1 + (val.Value - 1);
				}
			}

			if (allowOnlyButtons)
				return JoyControls.Invalid;

			if (buf.StartsWith("JoyX", StringComparison.OrdinalIgnoreCase)) return JoyControls.Xpos;

			if (buf.StartsWith("JoyY", StringComparison.OrdinalIgnoreCase)) return JoyControls.Ypos;

			if (buf.StartsWith("JoyZ", StringComparison.OrdinalIgnoreCase)) return JoyControls.Zpos;

			if (buf.StartsWith("JoyR", StringComparison.OrdinalIgnoreCase)) return JoyControls.Rpos;

			if (buf.StartsWith("JoyU", StringComparison.OrdinalIgnoreCase)) return JoyControls.Upos;

			if (buf.StartsWith("JoyV", StringComparison.OrdinalIgnoreCase)) return JoyControls.Vpos;

			if (buf.StartsWith("JoyPOV", StringComparison.OrdinalIgnoreCase)) return JoyControls.Pov;

			if (buf.StartsWith("JoyName", StringComparison.OrdinalIgnoreCase)) return JoyControls.Name;

			if (buf.StartsWith("JoyButtons", StringComparison.OrdinalIgnoreCase)) return JoyControls.Buttons;

			if (buf.StartsWith("JoyAxes", StringComparison.OrdinalIgnoreCase)) return JoyControls.Axes;

			if (buf.StartsWith("JoyInfo", StringComparison.OrdinalIgnoreCase)) return JoyControls.Info;

			return JoyControls.Invalid;
		}

		// Also the max that Windows supports.
		internal static bool IsJoystickButton(JoyControls joy) => joy >= JoyControls.Button1&& joy <= JoyControls.ButtonMax;

		/// <summary>
		/// It's best to call this function only directly from MsgSleep() or when there is an instance of
		/// MsgSleep() closer on the call stack than the nearest dialog's message pump (e.g. MsgBox).
		/// This is because events posted to the thread indirectly by us here would be discarded or mishandled
		/// by a non-standard (dialog) message pump.
		///
		/// Polling the joysticks this way rather than using joySetCapture() is preferable for several reasons:
		/// 1) I believe joySetCapture() internally polls the joystick anyway, via a system timer, so it probably
		///    doesn't perform much better (if at all) than polling "manually".
		/// 2) joySetCapture() only supports 4 buttons;
		/// 3) joySetCapture() will fail if another app is already capturing the joystick;
		/// 4) Even if the joySetCapture() succeeds, other programs (e.g. older games), would be prevented from
		///    capturing the joystick while the script in question is running.
		/// </summary>
		internal static void PollJoysticks()//Will need to figure out making this cross platform.//TODO
		{
			// Even if joystick hotkeys aren't currently allowed to fire, poll it anyway so that hotkey
			// messages can be buffered for later.
			var jie = JOYINFOEX.Default;

			for (var i = 0; i < MaxJoysticks; ++i)
			{
				if (!HotkeyDefinition.joystickHasHotkeys[i])
					continue;

				// Reset these every time in case joyGetPosEx() ever changes them. Also, most systems have only one joystick,
				// so this code will hardly ever be executed more than once (and sometimes zero times):
				jie.dwFlags = WindowsAPI.JOY_RETURNBUTTONS; // vs. JOY_RETURNALL

				if (WindowsAPI.joyGetPosEx(i, ref jie) != WindowsAPI.JOYERR_NOERROR) // Skip this joystick and try the others.
					continue;

				// The exclusive-or operator determines which buttons have changed state.  After that,
				// the bitwise-and operator determines which of those have gone from up to down (the
				// down-to-up events are currently not significant).
				var buttons_newly_down = (jie.dwButtons ^ buttonsPrev[i]) & jie.dwButtons;
				buttonsPrev[i] = jie.dwButtons;

				if (buttons_newly_down == 0)
					continue;

				// See if any of the joystick hotkeys match this joystick ID and one of the buttons that
				// has just been pressed on it.  If so, queue up (buffer) the hotkey events so that they will
				// be processed when messages are next checked:
				HotkeyDefinition.TriggerJoyHotkeys(i, buttons_newly_down);
			}
		}

		internal static object ScriptGetJoyState(JoyControls joy, int joystickID)
		// Caller must ensure that aToken.marker is a buffer large enough to handle the longest thing put into
		// it here, which is currently jc.szPname (size=32). Caller has set aToken.symbol to be SYM_STRING.
		// aToken is used for the value being returned by GetKeyState() to the script, while this function's
		// bool return value is used only by KeyWait, so is false for "up" and true for "down".
		// If there was a problem determining the position/state, aToken is made blank and false is returned.
		{
			// Set default in case of early return.
			if (joy == JoyControls.Invalid) // Currently never called this way.
				return false; // And leave aToken set to blank.

			var joyIsButton = Joystick.IsJoystickButton(joy);
			var jc = new JOYCAPS();

			if (!joyIsButton && joy != JoyControls.Pov)
			{
				// Get the joystick's range of motion so that we can report position as a percentage.
				if (WindowsAPI.joyGetDevCaps(new IntPtr(joystickID), ref jc, (uint)Marshal.SizeOf(jc)) != WindowsAPI.JOYERR_NOERROR)
					jc = new JOYCAPS();//Recreate on failure, for use of the zeroes later below.
			}

			// Fetch this struct's info only if needed:
			var jie = JOYINFOEX.Default;

			if (joy != JoyControls.Name && joy != JoyControls.Buttons && joy != JoyControls.Axes && joy != JoyControls.Info)
			{
				jie.dwFlags = WindowsAPI.JOY_RETURNALL;

				if (WindowsAPI.joyGetPosEx(joystickID, ref jie) != WindowsAPI.JOYERR_NOERROR)
					return false; // And leave aToken set to blank.

				if (joyIsButton)
					return ((jie.dwButtons >> ((int)joy - (int)JoyControls.Button1)) & 0x01) != 0;
			}

			// Otherwise:
			uint range;
			var str = "";
			var resultDouble = 0.0;  // Not initialized to help catch bugs.

			switch (joy)
			{
				case JoyControls.Xpos:
					range = (jc.wXmax > jc.wXmin) ? jc.wXmax - jc.wXmin : 0;
					return range != 0 ? 100 * (double)jie.dwXpos / range : jie.dwXpos;

				case JoyControls.Ypos:
					range = (jc.wYmax > jc.wYmin) ? jc.wYmax - jc.wYmin : 0;
					return range != 0 ? 100 * (double)jie.dwYpos / range : jie.dwYpos;

				case JoyControls.Zpos:
					range = (jc.wZmax > jc.wZmin) ? jc.wZmax - jc.wZmin : 0;
					return range != 0 ? 100 * (double)jie.dwZpos / range : jie.dwZpos;

				case JoyControls.Rpos:  // Rudder or 4th axis.
					range = (jc.wRmax > jc.wRmin) ? jc.wRmax - jc.wRmin : 0;
					return range != 0 ? 100 * (double)jie.dwRpos / range : jie.dwRpos;

				case JoyControls.Upos:  // 5th axis.
					range = (jc.wUmax > jc.wUmin) ? jc.wUmax - jc.wUmin : 0;
					return range != 0 ? 100 * (double)jie.dwUpos / range : jie.dwUpos;

				case JoyControls.Vpos:  // 6th axis.
					range = (jc.wVmax > jc.wVmin) ? jc.wVmax - jc.wVmin : 0;
					return range != 0 ? 100 * (double)jie.dwVpos / range : jie.dwVpos;

				case JoyControls.Pov:
					if (jie.dwPOV == WindowsAPI.JOY_POVCENTERED) // Need to explicitly compare against JOY_POVCENTERED because it's a WORD not a DWORD.
						return -1L;
					else
						return (long)jie.dwPOV;

				// No break since above always returns.

				case JoyControls.Name:
					return jc.szPname;

				case JoyControls.Buttons:
					return (long)jc.wNumButtons; // wMaxButtons is the *driver's* max supported buttons.

				case JoyControls.Axes:
					return (long)jc.wNumAxes; // wMaxAxes is the *driver's* max supported axes.

				case JoyControls.Info:
					if ((jc.wCaps & WindowsAPI.JOYCAPS_HASZ) != 0)
						str += 'Z';

					if ((jc.wCaps & WindowsAPI.JOYCAPS_HASR) != 0)
						str += 'R';

					if ((jc.wCaps & WindowsAPI.JOYCAPS_HASU) != 0)
						str += 'U';

					if ((jc.wCaps & WindowsAPI.JOYCAPS_HASV) != 0)
						str += 'V';

					if ((jc.wCaps & WindowsAPI.JOYCAPS_HASPOV) != 0)
					{
						str += 'P';

						if ((jc.wCaps & WindowsAPI.JOYCAPS_POV4DIR) != 0)
							str += 'D';

						if ((jc.wCaps & WindowsAPI.JOYCAPS_POVCTS) != 0)
							str += 'C';
					}

					return str;
			} // switch()

			return resultDouble;// If above didn't return, the result should now be in result_double.
		}
	}

	internal enum JoyControls
	{
		Invalid, Xpos, Ypos, Zpos
		, Rpos, Upos, Vpos, Pov
		, Name, Buttons, Axes, Info
		, Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8  // Buttons.
		, Button9, Button10, Button11, Button12, Button13, Button14, Button15, Button16
		, Button17, Button18, Button19, Button20, Button21, Button22, Button23, Button24
		, Button25, Button26, Button27, Button28, Button29, Button30, Button31, Button32
		, ButtonMax = 32
	};
}