using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Keysharp.Core.Common.Joystick;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//TODO

namespace Keysharp.Core
{
	public static class Keyboard
	{
		internal static readonly ToggleStates toggleStates = new ToggleStates();
		internal static bool blockInput;
		internal static ToggleValueType blockInputMode = ToggleValueType.Default;
		internal static bool blockMouseMove;
		internal static Dictionary<string, HotkeyDefinition> hotkeys = new Dictionary<string, HotkeyDefinition>();
		internal static Dictionary<string, HotstringDefinition> hotstrings = new Dictionary<string, HotstringDefinition>();
		private static FuncObj keyCondition;
		//Make readonly so that only one instance can ever be created, because other code will refer to this object.

		/// <summary>
		/// Disables or enables the user's ability to interact with the computer via keyboard and mouse.
		/// </summary>
		/// <param name="Mode">
		/// <list type="bullet">
		/// <item>On: the user is prevented from interacting with the computer (mouse and keyboard input has no effect).</item>
		/// <item>Off: input is re-enabled.</item>
		/// </list>
		/// </param>
		public static void BlockInput(object obj)
		{
			var mode = obj.As();
			var toggle = ConvertBlockInput(mode);

			switch (toggle)
			{
				case ToggleValueType.On:
					_ = ScriptBlockInput(true);
					break;

				case ToggleValueType.Off:
					_ = ScriptBlockInput(false);
					break;

				case ToggleValueType.Send:
				case ToggleValueType.Mouse:
				case ToggleValueType.SendAndMouse:
				case ToggleValueType.Default:
					blockInputMode = toggle;
					break;

				case ToggleValueType.MouseMove:
					blockMouseMove = true;
					HotkeyDefinition.InstallMouseHook();
					break;

				case ToggleValueType.MouseMoveOff:
					blockMouseMove = false; // But the mouse hook is left installed because it might be needed by other things. This approach is similar to that used by the Input command.
					break;
					// default (NEUTRAL or TOGGLE_INVALID): do nothing.
			}
		}

		public static Keysharp.Core.Map CaretGetPos()
		{
			// I believe only the foreground window can have a caret position due to relationship with focused control.
			var targetWindow = WindowsAPI.GetForegroundWindow(); // Variable must be named targetwindow for ATTACH_THREAD_INPUT.

			if (targetWindow == IntPtr.Zero) // No window is in the foreground, report blank coordinate.
			{
				return new Keysharp.Core.Map(new Dictionary<object, object>()
				{
					{ "CaretFound", false },
					{ "X", "" },
					{ "Y", "" }
				});
			}

			var h = WindowsAPI.GetWindowThreadProcessId(targetWindow, out var _);
			var info = GUITHREADINFO.Default;//Must be initialized this way because the size field must be populated.
			var result = WindowsAPI.GetGUIThreadInfo(h, out info) && info.hwndCaret != IntPtr.Zero;

			if (!result)
			{
				return new Keysharp.Core.Map(new Dictionary<object, object>()
				{
					{ "CaretFound", false },
					{ "X", "" },
					{ "Y", "" }
				});
			}

			var pt = new Point
			{
				X = info.rcCaret.Left,
				Y = info.rcCaret.Top
			};
			_ = WindowsAPI.ClientToScreen(info.hwndCaret, ref pt);// Unconditionally convert to screen coordinates, for simplicity.
			int x = 0, y = 0;
			WindowsAPI.CoordToScreen(ref x, ref y, CoordMode.Caret);// Now convert back to whatever is expected for the current mode.
			pt.X -= x;
			pt.Y -= y;
			return new Keysharp.Core.Map(new Dictionary<object, object>()
			{
				{ "CaretFound", true },
				{ "X",  pt.X },
				{ "Y", pt.Y }
			});
		}

		public static string GetKeyName(object obj) => GetKeyNamePrivate(obj.As(), 0) as string;

		public static long GetKeySC(object obj) => (long)GetKeyNamePrivate(obj.As(), 1);

		/// <summary>
		/// Unlike the GetKeyState command -- which returns D for down and U for up -- this function returns (1) if the key is down and (0) if it is up.
		/// If <paramref name="KeyName"/> is invalid, an empty string is returned.
		/// </summary>
		/// <param name="KeyName">Use autohotkey definition or virtual key starting from "VK"</param>
		/// <param name="Mode"></param>
		public static object GetKeyState(object obj0, object obj1 = null)
		{
			var keyname = obj0.As();
			var mode = obj1.As();
			var ht = Keysharp.Scripting.Script.HookThread;
			Keysharp.Core.Common.Joystick.JoyControls joy;
			uint? joystickid = 0u;
			uint? dummy = null;
			var vk = ht.TextToVK(keyname, ref dummy, false, true, WindowsAPI.GetKeyboardLayout(0));

			if (vk == 0)
			{
				if ((joy = Joystick.ConvertJoy(keyname, ref joystickid)) == 0)
					throw new ValueError("Invalid value.");//It is neither a key name nor a joystick button/axis.

				return Joystick.ScriptGetJoyState(joy, joystickid.Value);
			}

			// Since above didn't return: There is a virtual key (not a joystick control).
			KeyStateTypes keystatetype;

			if (string.Compare(mode, "T", true) == 0)
				keystatetype = KeyStateTypes.Toggle;//Whether a toggleable key such as CapsLock is currently turned on.
			else if (string.Compare(mode, "P", true) == 0)
				keystatetype = KeyStateTypes.Physical;//Physical state of key.
			else
				keystatetype = KeyStateTypes.Logical;

			return ScriptGetKeyState(vk, keystatetype); // 1 for down and 0 for up.
		}

		public static long GetKeyVK(object obj) => (long)GetKeyNamePrivate(obj.As(), 2);

		/// <summary>
		/// Creates or modifies a hotkey.
		/// </summary>
		/// <param name="KeyName">Name of the hotkey's activation key including any modifier symbols.</param>
		/// <param name="Label">The name of the function or label whose contents will be executed when the hotkey is pressed.
		/// This parameter can be left blank if <paramref name="KeyName"/> already exists as a hotkey,
		/// in which case its label will not be changed. This is useful for changing only the <paramref name="options"/>.
		/// </param>
		/// <param name="options">
		/// <list type="bullet">
		/// <item><term>On</term>: <description>the hotkey becomes enabled.</description></item>
		/// <item><term>Off</term>: <description>the hotkey becomes disabled.</description></item>
		/// <item><term>Toggle</term>: <description>the hotkey is set to the opposite state (enabled or disabled).</description></item>
		/// </list>
		/// </param>

		public static void Hotkey(object obj0, object obj1 = null, object obj2 = null)
		{
			var keyname = obj0.As();
			var label = obj1.As();
			var options = obj2.As();
			IFuncObj fo = null;
			var hook_action = 0u;

			if (obj1 != null)
			{
				fo = Function.GetFuncObj(obj1, null);//Don't throw on failure because returning null is a valid action.

				if (fo == null && !string.IsNullOrEmpty(label) && ((hook_action = HotkeyDefinition.ConvertAltTab(label, true)) == 0))
				{
					for (var i = 0; i < HotkeyDefinition.shk.Count; ++i)
					{
						if (HotkeyDefinition.shk[i].Name == label)
							continue;

						for (var v = HotkeyDefinition.shk[i].firstVariant; v != null; v = v.nextVariant)
						{
							if (v.hotCriterion == Keysharp.Scripting.Script.hotCriterion)
							{
								fo = v.originalCallback;
								goto break_twice;
							}
						}
					}

break_twice:;

					if (fo == null)
						throw new Error($"Unable to find existing hotkey handler: {label}");
				}

				if (fo == null)
					hook_action = HotkeyDefinition.ConvertAltTab(label, true);
			}

			_ = HotkeyDefinition.Dynamic(keyname, options, fo, hook_action);
		}

		public static object Hotstring(object obj0, object obj1 = null, object obj2 = null)
		{
			var name = obj0.As();
			var replacement = obj1;
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;
			var xOption = false;
			var action = replacement as string;

			if (string.Compare(name, "EndChars", true) == 0) // Equivalent to #Hotstring EndChars <action>
			{
				var old = HotstringDefinition.defEndChars;

				if (!string.IsNullOrEmpty(action))
					HotstringDefinition.defEndChars = action;

				return old;//Return the old value.
			}
			else if (string.Compare(name, "MouseReset", true) == 0) // "MouseReset, true" seems more intuitive than "NoMouse, false"
			{
				var previousValue = Keysharp.Scripting.Script.hsResetUponMouseClick;

				if (replacement != null)
				{
					var val = Options.OnOff(replacement);

					if (val != null)
					{
						Keysharp.Scripting.Script.hsResetUponMouseClick = val.Value;

						if (Keysharp.Scripting.Script.hsResetUponMouseClick != previousValue && HotstringDefinition.enabledCount != 0) // No need if there aren't any hotstrings.
							HotkeyDefinition.ManifestAllHotkeysHotstringsHooks(); // Install the hook if needed, or uninstall if no longer needed.
					}
				}

				return previousValue;
			}
			else if (string.Compare(name, "Reset", true) == 0)
			{
				return HotstringDefinition.ClearBuf();
			}
			else if (replacement == null && obj2 == null && name.Length > 0 && name[0] != ':') //Check if only one param was passed. Equivalent to #Hotstring <name>.
			{
				HotstringDefinition.ParseOptions(name, ref HotstringDefinition.hsPriority, ref HotstringDefinition.hsKeyDelay, ref HotstringDefinition.hsSendMode, ref HotstringDefinition.hsCaseSensitive
												 , ref HotstringDefinition.hsConformToCase, ref HotstringDefinition.hsDoBackspace, ref HotstringDefinition.hsOmitEndChar, ref HotstringDefinition.hsSendRaw, ref HotstringDefinition.hsEndCharRequired
												 , ref HotstringDefinition.hsDetectWhenInsideWord, ref HotstringDefinition.hsDoReset, ref xOption, ref HotstringDefinition.DefaultHotstringSuspendExempt);
				return null;
			}

			// Parse the hotstring name.
			var hotstringStart = "";
			var hotstringOptions = ""; // Set default as "no options were specified for this hotstring".

			if (name.Length > 1 && name[0] == ':')
			{
				if (name[1] != ':')
				{
					hotstringOptions = name.Substring(1); // Point it to the hotstring's option letters.
					// The following relies on the fact that options should never contain a literal colon.
					var tempindex = hotstringOptions.IndexOf(':');

					if (tempindex != -1)
						hotstringStart = hotstringOptions.Substring(tempindex + 1); // Points to the hotstring itself.
				}
				else // Double-colon, so it's a hotstring if there's more after this (but this means no options are present).
					if (name.Length > 2)
						hotstringStart = name.Substring(2);

				//else it's just a naked "::", which is invalid.
			}

			if (hotstringStart.Length == 0)
				throw new ValueError("Hotstring definition did not contain a hotstring.");

			// Determine options which affect hotstring identity/uniqueness.
			var caseSensitive = HotstringDefinition.DefaultHotstringCaseSensitive;
			var detectInsideWord = HotstringDefinition.DefaultHotstringDetectWhenInsideWord;
			var un = false; var iun = 0; var sm = SendModes.Event; var sr = SendRawModes.NotRaw; // Unused.
			var executeAction = false;

			if (hotstringOptions.Length > 0)
				HotstringDefinition.ParseOptions(hotstringOptions, ref iun, ref iun, ref sm, ref caseSensitive, ref un, ref un, ref un, ref sr, ref un, ref detectInsideWord, ref un, ref executeAction, ref un);

			IFuncObj ifunc = null;

			if (replacement != null)
			{
				if (xOption)
					ifunc = Function.GetFuncObj(replacement, null);
				else if (executeAction)
					throw new ValueError("The 'X' option must be used together with a function object.");
			}

			var toggle = ToggleValueType.Neutral;

			if (obj2 != null && (toggle = Keysharp.Core.Options.ConvertOnOffToggle(obj2)) == ToggleValueType.Invalid)
				throw new ValueError($"Invalid value of {obj2} for parameter 3.");

			bool wasAlreadyEnabled;
			var existing = HotstringDefinition.FindHotstring(hotstringStart, caseSensitive, detectInsideWord, Keysharp.Scripting.Script.hotCriterion);

			if (existing != null)
			{
				wasAlreadyEnabled = existing.suspended == 0;

				// Update the replacement string or function, if specified.
				if (ifunc != null || !string.IsNullOrEmpty(action))
				{
					string newReplacement = null; // Set default: not auto-replace.

					if (ifunc == null && replacement is string rep) // Caller specified a replacement string ('E' option was handled above).
						newReplacement = rep;

					existing.suspended |= HotstringDefinition.HS_TEMPORARILY_DISABLED;
					ht.WaitHookIdle();
					// At this point it is certain the hook thread is not in the middle of reading this
					// hotstring's other properties, such as mReplacement.
					existing.replacement = newReplacement;
					existing.funcObj = ifunc;
				}

				// Update the hotstring's options.  Note that mCaseSensitive and mDetectWhenInsideWord
				// can't be changed this way since FindHotstring() would not have found it if they differed.
				// This is done after the above to avoid *partial* updates in the event of a failure.
				existing.ParseOptions(hotstringOptions);

				switch (toggle)
				{
					case ToggleValueType.Toggle: existing.suspended ^= HotstringDefinition.HS_TURNED_OFF; break;

					case ToggleValueType.On: existing.suspended &= ~HotstringDefinition.HS_TURNED_OFF; break;

					case ToggleValueType.Off: existing.suspended |= HotstringDefinition.HS_TURNED_OFF; break;
				}

				existing.suspended &= ~HotstringDefinition.HS_TEMPORARILY_DISABLED; // Re-enable if it was disabled above.
			}
			else // No matching hotstring yet.
			{
				if (ifunc == null && string.IsNullOrEmpty(action))
					throw new TargetError("Nonexistent hotstring.", name);

				var initialSuspendState = (toggle == ToggleValueType.Off) ? HotstringDefinition.HS_TURNED_OFF : 0;

				if (Accessors.A_IsSuspended)
					initialSuspendState |= HotstringDefinition.HS_SUSPENDED;

				if (HotstringDefinition.AddHotstring(name, ifunc, hotstringOptions, hotstringStart, action, false, initialSuspendState) == ResultType.Fail)
					return null;

				existing = HotstringDefinition.shs[HotstringDefinition.shs.Count - 1];
				wasAlreadyEnabled = false; // Because it didn't exist.
			}

			// Note that suspended must be 0 to count as enabled, meaning the hotstring was neither
			// turned off by us nor suspended by SuspendAll().  If it was suspended, there's no change
			// in status.
			var isenabled = existing.suspended == 0; // Important to avoid direct comparison with mSuspended becauses it isn't pure bool.

			if (isenabled != wasAlreadyEnabled)
			{
				// One of the following just happened:
				//  - a hotstring was created and enabled
				//  - an existing disabled hotstring was just enabled
				//  - an existing enabled hotstring was just disabled
				var previouslyEnabled = HotstringDefinition.enabledCount;
				HotstringDefinition.enabledCount = isenabled ? HotstringDefinition.enabledCount + 1 : HotstringDefinition.enabledCount - 1;

				if ((HotstringDefinition.enabledCount > 0) != (previouslyEnabled > 0)) // Change in status of whether the hotstring recognizer is needed.
				{
					if (isenabled)
						HotstringDefinition.hsBuf.Clear();

					if (!isenabled || ht.kbdHook == IntPtr.Zero) // Hook may not be needed anymore || hook is needed but not present.
						HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
				}
			}

			return null;
		}

		public static void KeyHistory(object obj0)
		{
			if (obj0 != null)
			{
				var max = Math.Clamp(obj0.Al(), 0, 500);

				if (Keysharp.Scripting.Script.HookThread is Keysharp.Core.Common.Threading.HookThread ht)
				{
					if (ht.HasEitherHook())
					{
						_ = ht.PostMessage(new Keysharp.Core.Common.Threading.KeysharpMsg()
						{
							message = (uint)UserMessages.AHK_HOOK_SET_KEYHISTORY,
							wParam = new IntPtr(max)
						});
					}
					else
						ht.keyHistory = new KeyHistory((int)max);
				}
			}
			else if (Keysharp.Scripting.Script.mainWindow != null)
			{
				Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() =>
				{
					Keysharp.Scripting.Script.mainWindow.Show();
					Keysharp.Scripting.Script.mainWindow.ShowHistory();
				});
			}
		}

		/// <summary>
		/// Waits for a key or mouse/joystick button to be released or pressed down.
		/// </summary>
		/// <param name="KeyName">
		/// <para>This can be just about any single character from the keyboard or one of the key names from the key list, such as a mouse/joystick button. Joystick attributes other than buttons are not supported.</para>
		/// <para>An explicit virtual key code such as vkFF may also be specified. This is useful in the rare case where a key has no name and produces no visible character when pressed. Its virtual key code can be determined by following the steps at the bottom fo the key list page.</para>
		/// </param>
		/// <param name="options">
		/// <para>If this parameter is blank, the command will wait indefinitely for the specified key or mouse/joystick button to be physically released by the user. However, if the keyboard hook is not installed and KeyName is a keyboard key released artificially by means such as the Send command, the key will be seen as having been physically released. The same is true for mouse buttons when the mouse hook is not installed.</para>
		/// <para>Options: A string of one or more of the following letters (in any order, with optional spaces in between):</para>
		/// <list type="">
		/// <item>D: Wait for the key to be pushed down.</item>
		/// <item>L: Check the logical state of the key, which is the state that the OS and the active window believe the key to be in (not necessarily the same as the physical state). This option is ignored for joystick buttons.</item>
		/// <item>T: Timeout (e.g. T3). The number of seconds to wait before timing out and setting Accessors.A_ErrorLevel to 1. If the key or button achieves the specified state, the command will not wait for the timeout to expire. Instead, it will immediately set Accessors.A_ErrorLevel to 0 and the script will continue executing.</item>
		/// </list>
		/// <para>The timeout value can be a floating point number such as 2.5, but it should not be a hexadecimal value such as 0x03.</para>
		/// </param>
		public static bool KeyWait(object obj0, object obj1 = null)
		{
			var keyname = obj0.As();
			var options = obj1.As();
			bool waitIndefinitely;
			int sleepDuration;
			DateTime startTime;
			uint vk; // For GetKeyState.
			bool waitForKeyDown;
			KeyStateTypes keyStateType;
			var joy = JoyControls.Invalid;
			uint? joystickId = 0;
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;
			uint? modLR = null;

			if ((vk = ht.TextToVK(keyname, ref modLR, false, true, WindowsAPI.GetKeyboardLayout(0))) == 0)
			{
				joy = Joystick.ConvertJoy(keyname, ref joystickId);

				if (!Joystick.IsJoystickButton(joy)) // Currently, only buttons are supported.
					// It's either an invalid key name or an unsupported Joy-something.
					throw new ValueError($"Invalid keyname parameter: {keyname}");
			}

			// Set defaults:
			waitForKeyDown = false;  // The default is to wait for the key to be released.
			keyStateType = KeyStateTypes.Physical;  // Since physical is more often used.
			waitIndefinitely = true;
			sleepDuration = 0;

			for (var i = 0; i < options.Length; ++i)
			{
				switch (char.ToUpper(options[i]))
				{
					case 'D':
						waitForKeyDown = true;
						break;

					case 'L':
						keyStateType = KeyStateTypes.Logical;
						break;

					case 'T':
						// Although ATOF() supports hex, it's been documented in the help file that hex should
						// not be used (see comment above) so if someone does it anyway, some option letters
						// might be misinterpreted:
						waitIndefinitely = false;
						var numstr = "";
						var cc = CultureInfo.CurrentCulture;

						for (var numi = i + 1; numi < options.Length; numi++)
						{
							if (char.IsDigit(options[numi]) || options[numi] == cc.NumberFormat.NumberDecimalSeparator[0])
								numstr += options[numi];
							else
								break;
						}

						sleepDuration = (int)(numstr.ParseDouble(false) * 1000);
						break;
				}
			}

			for (startTime = DateTime.Now; ;) // start_time is initialized unconditionally for use with v1.0.30.02's new logging feature further below.
			{
				// Always do the first iteration so that at least one check is done.
				if (vk != 0) // Waiting for key or mouse button, not joystick.
				{
					if (ScriptGetKeyState(vk, keyStateType) == waitForKeyDown)
						return true;
				}
				else // Waiting for joystick button
				{
					if (Keysharp.Core.Common.Joystick.Joystick.ScriptGetJoyState(joy, joystickId.Value) is bool b && b == waitForKeyDown)
						return true;
				}

				// Must cast to int or any negative result will be lost due to DWORD type:
				if (waitIndefinitely || (int)(sleepDuration - (DateTime.Now - startTime).TotalMilliseconds) > Keysharp.Scripting.Script.SLEEP_INTERVAL_HALF)
				{
					Keysharp.Core.Flow.Sleep(Keysharp.Scripting.Script.SLEEP_INTERVAL);
					//MsgSleep() might not even be needed if we use real threads//TODO
					//if (Keysharp.Scripting.Script.MsgSleep(Keysharp.Scripting.Script.INTERVAL_UNSPECIFIED)) // INTERVAL_UNSPECIFIED performs better.
					//{
					//}
				}
				else // Done waiting.
					return false; // Since it timed out, we override the default with this.
			}
		}

		//We initially had these using BeginInvoke(), but that is wrong, because these will often be launched from threads in responde to a hotkey/string.
		//The state of those threads needs to be preserved, but invoking will overwrite that state by putting the call on the main GUI thread.
		//So put them back to just straight calls, revisit if cross threading bugs occur.
		//public static void SendEvent(object obj) => Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, SendModes.Event, IntPtr.Zero), true, true);
		//public static void Send(object obj) => Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, Accessors.SendMode, IntPtr.Zero), true, true);
		//public static void SendInput(object obj) => Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, Accessors.SendMode == SendModes.InputThenPlay ? SendModes.InputThenPlay : SendModes.Input, IntPtr.Zero), true, true);
		//public static void SendPlay(object obj) => Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, SendModes.Play, IntPtr.Zero), true, true);
		//public static void SendText(object obj) => Keysharp.Scripting.Script.mainWindow.CheckedBeginInvoke(() => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.RawText, Accessors.SendMode, IntPtr.Zero), true, true);

		public static void Send(object obj) => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, Accessors.SendMode, IntPtr.Zero);

		public static void SendEvent(object obj) => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, SendModes.Event, IntPtr.Zero);

		public static void SendInput(object obj) => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, Accessors.SendMode == SendModes.InputThenPlay ? SendModes.InputThenPlay : SendModes.Input, IntPtr.Zero);

		public static void SendLevel(object obj) => Accessors.A_SendLevel = obj;

		public static void SendMode(object obj) => Accessors.A_SendMode = obj;

		public static void SendPlay(object obj) => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.NotRaw, SendModes.Play, IntPtr.Zero);

		public static void SendText(object obj) => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendKeys(obj.As(), SendRawModes.RawText, Accessors.SendMode, IntPtr.Zero);

		public static void SetCapsLockState(object obj) => SetToggleState(WindowsAPI.VK_CAPITAL, ref toggleStates.forceCapsLock, obj.As());//Shouldn't have windows code in a common location.//TODO

		public static void SetKeyDelay(object obj0 = null, object obj1 = null, object obj2 = null)
		{
			var play = obj2.As().ToLowerInvariant();
			var isPlay = play == "play";
			var del = isPlay ? Accessors.A_KeyDelayPlay : Accessors.A_KeyDelay;
			var dur = isPlay ? Accessors.A_KeyDurationPlay : Accessors.A_KeyDuration;

			if (obj0 != null)
				del = obj0.Al();

			if (obj1 != null)
				dur = obj1.Al();

			if (isPlay)
			{
				Accessors.A_KeyDelayPlay = del;
				Accessors.A_KeyDurationPlay = dur;
			}
			else
			{
				Accessors.A_KeyDelay = del;
				Accessors.A_KeyDuration = dur;
			}
		}

		public static void SetNumLockState(object obj) => SetToggleState(WindowsAPI.VK_NUMLOCK, ref toggleStates.forceNumLock, obj.As());//Shouldn't have windows code in a common location.//TODO

		public static void SetScrollLockState(object obj) => SetToggleState(WindowsAPI.VK_SCROLL, ref toggleStates.forceScrollLock, obj.As());//Shouldn't have windows code in a common location.//TODO

		public static void SetStoreCapsLockMode(object obj) => Accessors.A_StoreCapsLockMode = obj;

		internal static ToggleValueType ConvertBlockInput(string buf)
		{
			var toggle = Keysharp.Core.Options.ConvertOnOff(buf);

			if (toggle != ToggleValueType.Invalid)
				return toggle;

			if (string.Compare(buf, "Send", true) == 0) return ToggleValueType.Send;

			if (string.Compare(buf, "Mouse", true) == 0) return ToggleValueType.Mouse;

			if (string.Compare(buf, "SendAndMouse", true) == 0) return ToggleValueType.SendAndMouse;

			if (string.Compare(buf, "Default", true) == 0) return ToggleValueType.Default;

			if (string.Compare(buf, "MouseMove", true) == 0) return ToggleValueType.MouseMove;

			if (string.Compare(buf, "MouseMoveOff", true) == 0) return ToggleValueType.MouseMoveOff;

			return ToggleValueType.Invalid;
		}

		internal static string GetKeyNameHelper(uint vk, uint sc, string def = "not found")
		{
			var ht = Keysharp.Scripting.Script.HookThread;
			var buf = ""; // Set default.

			if (vk == 0 && sc == 0)
				return buf;

			if (vk == 0)
				vk = ht.MapScToVk(sc);
			else if (sc == 0 && (vk == WindowsAPI.VK_RETURN || (sc = ht.MapVkToSc(vk, true)) == 0)) // Prefer the non-Numpad name.
				sc = ht.MapVkToSc(vk);

			// Check SC first to properly differentiate between Home/NumpadHome, End/NumpadEnd, etc.
			// v1.0.43: WheelDown/Up store the notch/turn count in SC, so don't consider that to be a valid SC.
			if (sc != 0 && !ht.IsWheelVK(vk) && ht.SCtoKeyName(sc, false) != "")
			{
				return buf;
				// Otherwise this key is probably one we can handle by VK.
			}

			if ((buf = ht.VKtoKeyName(vk, false)) != "")
				return buf;

			return def;// Since this key is unrecognized, return the caller-supplied default value.
		}

		/// <summary>
		/// Always returns OK for caller convenience.
		/// </summary>
		/// <param name="enable"></param>
		/// <returns></returns>
		internal static ResultType ScriptBlockInput(bool enable)
		{
			// Always turn input ON/OFF even if g_BlockInput says its already in the right state.  This is because
			// BlockInput can be externally and undetectably disabled, e.g. if the user presses Ctrl-Alt-Del:
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)//Need to figure out how to make this cross platform.//TODO
				_ = WindowsAPI.BlockInput(enable);

			blockInput = enable;
			return ResultType.Ok;//By design, it never returns FAIL.
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="vk"></param>
		/// <param name="keyStateType"></param>
		/// <returns>true if down, else false</returns>
		internal static bool ScriptGetKeyState(uint vk, KeyStateTypes keyStateType)
		{
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			if (vk == 0) // Assume "up" if indeterminate.
				return false;

			switch (keyStateType)
			{
				case KeyStateTypes.Toggle: // Whether a toggleable key such as CapsLock is currently turned on.
					return ht.IsKeyToggledOn(vk); // This also works for non-"lock" keys, but in that case the toggle state can be out of sync with other processes/

				case KeyStateTypes.Physical: // Physical state of key.
					if (ht.IsMouseVK(vk)) // mouse button
					{
						return ht.HasMouseHook() ? (ht.physicalKeyState[vk] & KeyboardMouseSender.StateDown) != 0 : ht.IsKeyDownAsync(vk); // mouse hook is installed, so use it's tracking of physical state.
					}
					else // keyboard
					{
						if (ht.HasKbdHook())
						{
							bool? dummy = null;

							// Since the hook is installed, use its value rather than that from
							// GetAsyncKeyState(), which doesn't seem to return the physical state.
							// But first, correct the hook modifier state if it needs it.  See comments
							// in GetModifierLRState() for why this is needed:
							if (ht.KeyToModifiersLR(vk, 0, ref dummy) != 0)    // It's a modifier.
								_ = kbdMouseSender.GetModifierLRState(true); // Correct hook's physical state if needed.

							return (ht.physicalKeyState[vk] & KeyboardMouseSender.StateDown) != 0;
						}
						else
							return ht.IsKeyDownAsync(vk);
					}
			} // switch()

			// Otherwise, use the default state-type: KEYSTATE_LOGICAL
			// On XP/2K at least, a key can be physically down even if it isn't logically down,
			// which is why the below specifically calls IsKeyDown() rather than some more
			// comprehensive method such as consulting the physical key state as tracked by the hook:
			// v1.0.42.01: For backward compatibility, the following hasn't been changed to IsKeyDownAsync().
			// One example is the journal playback hook: when a window owned by the script receives
			// such a keystroke, only GetKeyState() can detect the changed state of the key, not GetAsyncKeyState().
			// A new mode can be added to KeyWait & GetKeyState if Async is ever explicitly needed.
			return ht.IsKeyDown(vk);
		}

		private static object GetKeyNamePrivate(string keyname, int callid)
		{
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;
			var vk = 0u;
			var sc = 0u;
			uint? modLR = null;
			_ = ht.TextToVKandSC(keyname, ref vk, ref sc, ref modLR, WindowsAPI.GetKeyboardLayout(0));//Need to make cross platform.

			switch (callid)
			{
				case 0:
					return GetKeyNameHelper(vk, sc, "");

				case 1:
					return sc != 0 ? sc : ht.MapVkToSc(vk);

				case 2:
					return vk != 0 ? vk : ht.MapScToVk(sc);
			}

			return "";
		}

		private static bool HotkeyPrecondition(string[,] win)
		{
			if (!string.IsNullOrEmpty(win[0, 0]) || !string.IsNullOrEmpty(win[0, 1]))
				if (Window.WinActive(win[0, 0], win[0, 1], string.Empty, string.Empty) == 0)
					return false;

			if (!string.IsNullOrEmpty(win[1, 0]) || !string.IsNullOrEmpty(win[1, 1]))
				if (Window.WinExist(win[1, 0], win[1, 1], string.Empty, string.Empty) == 0)
					return false;

			if (!string.IsNullOrEmpty(win[2, 0]) || !string.IsNullOrEmpty(win[2, 1]))
				if (Window.WinActive(win[2, 0], win[2, 1], string.Empty, string.Empty) != 0)
					return false;

			if (!string.IsNullOrEmpty(win[3, 0]) || !string.IsNullOrEmpty(win[3, 1]))
				if (Window.WinExist(win[3, 0], win[3, 1], string.Empty, string.Empty) != 0)
					return false;

			return true;
		}

		private static void SetToggleState(uint vk, ref ToggleValueType forceLock, string toggleText)
		{
			var toggle = Keysharp.Core.Options.ConvertOnOffAlways(toggleText, ToggleValueType.Neutral);
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			switch (toggle)
			{
				case ToggleValueType.On:
				case ToggleValueType.Off:
					// Turning it on or off overrides any prior AlwaysOn or AlwaysOff setting.
					// Probably need to change the setting BEFORE attempting to toggle the
					// key state, otherwise the hook may prevent the state from being changed
					// if it was set to be AlwaysOn or AlwaysOff:
					forceLock = ToggleValueType.Neutral;
					_ = kbdMouseSender.ToggleKeyState(vk, toggle);
					break;

				case ToggleValueType.AlwaysOn:
				case ToggleValueType.AlwaysOff:
					forceLock = (toggle == ToggleValueType.AlwaysOn) ? ToggleValueType.On : ToggleValueType.Off; // Must do this first.
					_ = kbdMouseSender.ToggleKeyState(vk, forceLock);
					// The hook is currently needed to support keeping these keys AlwaysOn or AlwaysOff, though
					// there may be better ways to do it (such as registering them as a hotkey, but
					// that may introduce quite a bit of complexity):
					HotkeyDefinition.InstallKeybdHook();
					break;

				case ToggleValueType.Neutral:
					// Note: No attempt is made to detect whether the keybd hook should be deinstalled
					// because it's no longer needed due to this change.  That would require some
					// careful thought about the impact on the status variables in the Hotkey class, etc.,
					// so it can be left for a future enhancement:
					forceLock = ToggleValueType.Neutral;
					break;
			}
		}
	}

	internal class ToggleStates
	{
		internal ToggleValueType forceCapsLock = ToggleValueType.Neutral;
		internal ToggleValueType forceNumLock = ToggleValueType.Neutral;
		internal ToggleValueType forceScrollLock = ToggleValueType.Neutral;
	}
}