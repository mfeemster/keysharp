using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keysharp.Core.Windows;

namespace Keysharp.Core.Common.Keyboard
{
	/// <summary>
	/// Screen coordinates, which can be negative.  SHORT vs. INT is used because the likelihood
	/// have having a virtual display surface wider or taller than 32,767 seems too remote to
	/// justify increasing the struct size, which would impact the stack space and dynamic memory
	/// used by every script every time it uses the playback method to send keystrokes or clicks.
	/// Note: WM_LBUTTONDOWN uses WORDs vs. SHORTs, but they're not really comparable because
	/// journal playback/record both use screen coordinates but WM_LBUTTONDOWN et. al. use client
	/// coordinates.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct Pt
	{
		internal short x, y;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct ScVk
	{
		internal ushort sc; // Placed above vk for possibly better member stacking/alignment.
		internal byte vk;
	}

	/// <summary>
	/// Platform independent keyboard Hook base.
	/// This Class is abstract.
	/// </summary>
	internal abstract class KeyboardMouseSender
	{
		//Need to figure out if these should be signed or not. Weird bugs can happen with wraparound comparisons if you get it wrong.//TODO
		internal const int CoordCentered = int.MinValue + 1;
		internal const int CoordModeCaret = 6;
		internal const int CoordModeClient = 0;
		internal const int CoordModeInvalid = -1;
		internal const int CoordModeMask = 3;
		internal const int CoordModeMenu = 8;
		internal const int CoordModeMouse = 2;
		internal const int CoordUnspecified = int.MinValue;
		internal const short CoordUnspecifiedShort = short.MinValue;
		internal const int HookFail = 0xFF;
		internal const int HookKeyboard = 0x01;
		internal const int HookMouse = 0x02;
		internal const uint KeyBlockThis = KeyIgnore + 1;

		// Below uses a pseudo-random value.It's best that this be constant so that if multiple instances
		// of the app are running, they will all ignore each other's keyboard & mouse events.  Also, a value
		// close to UINT_MAX might be a little better since it's might be less likely to be used as a pointer
		// value by any apps that send keybd events whose ExtraInfo is really a pointer value.
		internal const uint KeyIgnore = 0xFFC3D44F;

		internal const uint KeyIgnoreAllExceptModifier = KeyIgnore - 2;
		internal const uint KeyIgnoreMax = KeyIgnore;
		internal const uint KeyPhysIgnore = KeyIgnore - 1;
		internal const int MaxMouseSpeed = 100;
		internal const int MOD_ALT = 0x0001;
		internal const int MOD_CONTROL = 0x0002;
		internal const int MOD_LALT = 0x04;
		internal const int MOD_LCONTROL = 0x01;
		internal const int MOD_LSHIFT = 0x10;
		internal const int MOD_LWIN = 0x40;
		internal const int MOD_RALT = 0x08;
		internal const int MOD_RCONTROL = 0x02;
		internal const int MOD_RSHIFT = 0x20;
		internal const int MOD_RWIN = 0x80;
		internal const int MOD_SHIFT = 0x0004;
		internal const int MOD_WIN = 0x0008;
		internal const int MODLR_COUNT = 8;
		internal const int MODLR_MAX = 0xFF;
		internal const string ModLRString = "<^>^<!>!<+>+<#>#";
		internal const uint MsgOffsetMouseMove = 0x80000000;
		internal const uint SendLevelMax = 100u;
		internal const int StateDown = 0x80;
		internal const int StateOn = 0x01;
		internal static uint altGrExtraInfo;
		internal static char[] bracechars = "{}".ToCharArray();
		internal static string[] CoordModes = new string[] { "Client", "Window", "Screen" };
		internal static char[] llChars = "Ll".ToCharArray();
		internal static KeyType prefixKey;
		internal static string sendKeyChars = "^+!#{}";
		internal static int thisHotkeyModifiersLR;
		internal bool abortArraySend;
		internal DateTime lastPeekTime;
		internal int maxEvents;
		internal int modifiersLRCtrlAltDelMask;
		internal int modifiersLRLogical;
		internal int modifiersLRLogicalNonIgnored;
		internal int modifiersLRNumpadMask;
		internal int modifiersLRPhysical;
		protected SendModes sendMode = SendModes.Event;//Note this is different than the one in Accessors and serves as a temporary.
		protected ArrayPool<byte> keyStatePool = ArrayPool<byte>.Create(256, 100);
		private const int retention = 1024;
		private readonly StringBuilder caser = new StringBuilder(32);
		private readonly List<HotstringDefinition> expand = new List<HotstringDefinition>();
		private readonly StringBuilder history;
		private readonly List<HotkeyDefinition> hotkeys;
		private readonly List<HotstringDefinition> hotstrings;
		private readonly Dictionary<Keys, bool> pressed;

		public KeyboardMouseSender()
		{
			hotkeys = new List<HotkeyDefinition>();
			hotstrings = new List<HotstringDefinition>();
			history = new StringBuilder(retention);
			pressed = new Dictionary<Keys, bool>();

			foreach (int i in Enum.GetValues(typeof(Keys)))
				_ = pressed.TryAdd((Keys)i, false);

			RegisterHook();
		}

		public string ApplyCase(string typedstr, string hotstr)
		{
			var typedlen = typedstr.Length;
			var hotlen = hotstr.Length;
			_ = caser.Clear();

			for (int i = 0, j = 0; i < typedlen && j < hotlen;)
			{
				var ch1 = typedstr[i];
				var ch2 = hotstr[j];

				if (char.ToUpperInvariant(ch1) == char.ToUpperInvariant(ch2))
				{
					_ = caser.Append(ch1);
					i++;
				}
				else
					_ = caser.Append(ch2);

				j++;
			}

			return caser.ToString();
		}

		public bool IsPressed(Keys key)
		{
			if (pressed.ContainsKey(key))
				return pressed[key];
			else
			{
				Debug.Fail("Thre should'nt be any key not in this table...");
				return false;
			}
		}

		internal static bool HotInputLevelAllowsFiring(uint inputLevel, ulong aEventExtraInfo, ref char? aKeyHistoryChar)
		{
			if (InputLevelFromInfo(aEventExtraInfo) <= inputLevel)
			{
				if (aKeyHistoryChar != null)
					aKeyHistoryChar = 'i'; // Mark as ignored in KeyHistory

				return false;
			}

			return true;
		}

		internal static int InputLevelFromInfo(ulong aExtraInfo)
		{
			if (aExtraInfo >= KeyIgnoreMin() && aExtraInfo <= KeyIgnoreMax)
				return (int)(KeyIgnoreLevel(0) - aExtraInfo);

			return (int)(SendLevelMax + 1);
		}

		/// <summary>
		/// KEY_PHYS_IGNORE events must be mostly ignored because currently there is no way for a given
		/// hook instance to detect if it sent the event or some other instance.  Therefore, to treat
		/// such events as true physical events might cause infinite loops or other side-effects in
		/// the instance that generated the event.  More review of this is needed if KEY_PHYS_IGNORE
		/// events ever need to be treated as true physical events by the instances of the hook that
		/// didn't originate them. UPDATE: The foregoing can now be accomplished using SendLevel.
		/// </summary>
		/// <param name="aExtraInfo"></param>
		/// <returns></returns>
		internal static bool IsIgnored(ulong val) => val == KeyIgnore || val == KeyPhysIgnore || val == KeyIgnoreAllExceptModifier;

		// Same as above but marked as physical for other instances of the hook.
		// Non-physical and ignored only if it's not a modifier.
		// Same as KEY_IGNORE_ALL_EXCEPT_MODIFIER, but only ignored by Hotkeys & Hotstrings at InputLevel LEVEL and below.
		// The levels are set up to use negative offsets from KEY_IGNORE_ALL_EXCEPT_MODIFIER so that we can leave
		// the values above unchanged and have KEY_IGNORE_LEVEL(0) == KEY_IGNORE_ALL_EXCEPT_MODIFIER.
		//
		// In general, KEY_IGNORE_LEVEL(g->SendLevel) should be used for any input that's meant to be treated as "real",
		// as opposed to input generated for side effects (e.g., masking modifier keys to prevent default OS responses).
		// A lot of the calls that generate input fall into the latter category, so KEY_IGNORE_ALL_EXCEPT_MODIFIER
		// (aka KEY_IGNORE_LEVEL(0)) still gets used quite often.
		//
		// Note that there are no "level" equivalents for KEY_IGNORE or KEY_PHYS_IGNORE (only KEY_IGNORE_ALL_EXCEPT_MODIFIER).
		// For the KEY_IGNORE_LEVEL use cases, there isn't a need to ignore modifiers or differentiate between physical
		// and non-physical, and leaving them out keeps the code much simpler.
		internal static uint KeyIgnoreLevel(uint level) => KeyIgnoreAllExceptModifier - level;

		internal static uint KeyIgnoreMin() => KeyIgnoreLevel(SendLevelMax);

		internal static bool SendLevelIsValid(uint level) => level >= 0 && level <= SendLevelMax;

		internal HotkeyDefinition Add(HotkeyDefinition hotkey)
		{
			hotkeys.Add(hotkey);
			return hotkey;
		}

		internal HotstringDefinition Add(HotstringDefinition hotstring)
		{
			hotstrings.Add(hotstring);//This will not check for duplicates.
			return hotstring;
		}

		internal abstract void CleanupEventArray(long aFinalKeyDelay);

		internal abstract int ConvertModifiers(int aModifiers);

		internal abstract int ConvertModifiersLR(int aModifiersLR);

		internal abstract void DoMouseDelay();

		internal abstract IntPtr GetFocusedKeybdLayout(IntPtr aWindow);

		internal abstract int GetModifierLRState(bool aExplicitlyGet = false);

		internal abstract void InitEventArray(int maxEvents, int aModifiersLR);

		internal abstract string ModifiersLRToText(int aModifiersLR);

		internal abstract void MouseClick(int aVK, int aX, int aY, long aRepeatCount, long aSpeed, KeyEventTypes aEventType, bool aMoveOffset);

		internal abstract void MouseClickDrag(int vk, int x1, int y1, int x2, int y2, long speed, bool relative);

		internal abstract void MouseEvent(uint aEventFlags, uint aData, int aX = CoordUnspecified, int aY = CoordUnspecified);

		internal abstract void MouseMove(ref int aX, ref int aY, ref uint aEventFlags, long aSpeed, bool aMoveOffset);

		internal abstract int PbEventCount();

		internal void PerformMouseCommon(Actions actionType, int vk, int x1, int y1, int x2, int y2,
										 long repeatCount, Keysharp.Core.Common.Keyboard.KeyEventTypes eventType, long speed, bool relative)
		{
			// The maximum number of events, which in this case would be from a MouseClickDrag.  To be conservative
			// (even though INPUT is a much larger struct than PlaybackEvent and SendInput doesn't use mouse-delays),
			// include room for the maximum number of mouse delays too.
			// Drag consists of at most:
			// 1) Move; 2) Delay; 3) Down; 4) Delay; 5) Move; 6) Delay; 7) Delay (dupe); 8) Up; 9) Delay.
			const int MAX_PERFORM_MOUSE_EVENTS = 10;
			var ht = Keysharp.Scripting.Script.HookThread;
			sendMode = Accessors.SendMode;

			if (sendMode == Common.Keyboard.SendModes.Input || sendMode == Common.Keyboard.SendModes.InputThenPlay)
			{
				if (ht.SystemHasAnotherMouseHook()) // See similar section in SendKeys() for details.
					sendMode = (sendMode == Common.Keyboard.SendModes.Input) ? Common.Keyboard.SendModes.Event : Common.Keyboard.SendModes.Play;
				else
					sendMode = Common.Keyboard.SendModes.Input; // Resolve early so that other sections don't have to consider SM_INPUT_FALLBACK_TO_PLAY a valid value.
			}

			if (sendMode != Common.Keyboard.SendModes.Event) // We're also responsible for setting sSendMode to SM_EVENT prior to returning.
				InitEventArray(MAX_PERFORM_MOUSE_EVENTS, 0);

			// Turn it on unconditionally even if it was on, since Ctrl-Alt-Del might have disabled it.
			// Turn it back off only if it wasn't ON before we started.
			var blockinput_prev = Keysharp.Core.Keyboard.blockInput;
			var do_selective_blockinput = (Keysharp.Core.Keyboard.blockInputMode == Common.Keyboard.ToggleValueType.Mouse
										   || Keysharp.Core.Keyboard.blockInputMode == Common.Keyboard.ToggleValueType.SendAndMouse)
										  && sendMode == Common.Keyboard.SendModes.Event;

			if (do_selective_blockinput) // It seems best NOT to use g_BlockMouseMove for this, since often times the user would want keyboard input to be disabled too, until after the mouse event is done.
				_ = Keysharp.Core.Keyboard.ScriptBlockInput(true); // Turn it on unconditionally even if it was on, since Ctrl-Alt-Del might have disabled it.

			switch (actionType)
			{
				case Actions.ACT_MOUSEMOVE:
					var unused = 0u;
					MouseMove(ref x1, ref y1, ref unused, speed, relative); // Does nothing if coords are invalid.
					break;

				case Actions.ACT_MOUSECLICK:
					MouseClick(vk, x1, y1, repeatCount, speed, eventType, relative); // Does nothing if coords are invalid.
					break;

				case Actions.ACT_MOUSECLICKDRAG:
					MouseClickDrag(vk, x1, y1, x2, y2, speed, relative); // Does nothing if coords are invalid.
					break;
			}

			if (sendMode != Common.Keyboard.SendModes.Event)
			{
				var final_key_delay = -1L; // Set default.

				if (!abortArraySend && TotalEventCount() > 0)
					SendEventArray(ref final_key_delay, 0); // Last parameter is ignored because keybd hook isn't removed for a pure-mouse SendInput.

				CleanupEventArray(final_key_delay);
			}

			if (do_selective_blockinput && !blockinput_prev)  // Turn it back off only if it was off before we started.
				_ = Keysharp.Core.Keyboard.ScriptBlockInput(false);
		}

		internal async Task<object> ProcessHotkey(int wParamVal, int lParamVal, HotkeyVariant variant, uint msg)
		{
			var hkId = wParamVal & HotkeyDefinition.HOTKEY_ID_MASK;

			if (hkId < HotkeyDefinition.shk.Count)//Ensure hotkey ID is valid.
			{
				var ht = Keysharp.Scripting.Script.HookThread;
				var hk = HotkeyDefinition.shk[hkId];
				// Check if criterion allows firing.
				// For maintainability, this is done here rather than a little further down
				// past the g_MaxThreadsTotal and thread-priority checks.  Those checks hardly
				// ever abort a hotkey launch anyway.
				//
				// If message is WM_HOTKEY, it's either:
				// 1) A joystick hotkey from TriggerJoyHotkeys(), in which case the lParam is ignored.
				// 2) A hotkey message sent by the OS, in which case lParam contains currently-unused info set by the OS.
				//
				// An incoming WM_HOTKEY can be subject to #HotIf Win. at this stage under the following conditions:
				// 1) Joystick hotkey, because it relies on us to do the check so that the check is done only
				//    once rather than twice.
				// 2) #HotIf Win. keybd hotkeys that were made non-hook because they have a non-suspended, global variant.
				//
				// If message is AHK_HOOK_HOTKEY:
				// Rather than having the hook pass the qualified variant to us, it seems preferable
				// to search through all the criteria again and rediscover it.  This is because conditions
				// may have changed since the message was posted, and although the hotkey might still be
				// eligible for firing, a different variant might now be called for (e.g. due to a change
				// in the active window).  Since most criteria hotkeys have at most only a few criteria,
				// and since most such criteria are #HotIf WinActive rather than Exist, the performance will
				// typically not be reduced much at all.  Furthermore, trading performance for greater
				// reliability seems worth it in this case.
				//
				// The inefficiency of calling HotCriterionAllowsFiring() twice for each hotkey --
				// once in the hook and again here -- seems justified for the following reasons:
				// - It only happens twice if the hotkey a hook hotkey (multi-variant keyboard hotkeys
				//   that have a global variant are usually non-hook, even on NT/2k/XP).
				// - The hook avoids doing its first check of WinActive/Exist if it sees that the hotkey
				//   has a non-suspended, global variant.  That way, hotkeys that are hook-hotkeys for
				//   reasons other than #HotIf Win. (such as mouse, overriding OS hotkeys, or hotkeys
				//   that are too fancy for RegisterHotkey) will not have to do the check twice.
				// - It provides the ability to set the last-found-window for #HotIf WinActive/Exist
				//   (though it's not needed for the "Not" counterparts).  This HWND could be passed
				//   via the message, but that would require malloc-there and free-here, and might
				//   result in memory leaks if its ever possible for messages to get discarded by the OS.
				// - It allows hotkeys that were eligible for firing at the time the message was
				//   posted but that have since become ineligible to be aborted.  This seems like a
				//   good precaution for most users/situations because such hotkey subroutines will
				//   often assume (for scripting simplicity) that the specified window is active or
				//   exists when the subroutine executes its first line.
				// - Most criterion hotkeys use #HotIf WinActive(), which is a very fast call.  Also, although
				//   WinText and/or "SetTitleMatchMode 'Slow'" slow down window searches, those are rarely
				//   used too.
				//
				char? dummy = null;
				var criterion_found_hwnd = IntPtr.Zero;

				if (!(variant != null || (variant = hk.CriterionAllowsFiring(msg == (uint)UserMessages.AHK_HOOK_HOTKEY ? KeyboardMouseSender.KeyIgnoreLevel((uint)Conversions.HighWord(lParamVal)) : 0, ref dummy)) != null))
					return ""; // No criterion is eligible, so ignore this hotkey event (see other comments).

				// If this is AHK_HOOK_HOTKEY, criterion was eligible at time message was posted,
				// but not now.  Seems best to abort (see other comments).
				// Due to the key-repeat feature and the fact that most scripts use a value of 1
				// for their #MaxThreadsPerHotkey, this check will often help average performance
				// by avoiding a lot of unnecessary overhead that would otherwise occur:
				if (!hk.PerformIsAllowed(variant))
				{
					// The key is buffered in this case to boost the responsiveness of hotkeys
					// that are being held down by the user to activate the keyboard's key-repeat
					// feature.  This way, there will always be one extra event waiting in the queue,
					// which will be fired almost the instant the previous iteration of the subroutine
					// finishes (this above description applies only when MaxThreadsPerHotkey is 1,
					// which it usually is).
					variant.RunAgainAfterFinished(); // Wheel notch count (g->EventInfo below) should be okay because subsequent launches reuse the same thread attributes to do the repeats.
					return "";
				}

				// Now that above has ensured variant is non-NULL:
				var hc = variant.hotCriterion;
				var priority = variant.priority;
				Keysharp.Scripting.Script.SetHotNamesAndTimes(hk.Name);

				if (ht.IsWheelVK(hk.vk)) // If this is true then also: msg.message==AHK_HOOK_HOTKEY
					Accessors.A_EventInfo = Conversions.LowWord(lParamVal); // v1.0.43.03: Override the thread default of 0 with the number of notches by which the wheel was turned.

				// Above also works for RunAgainAfterFinished since that feature reuses the same thread attributes set above.
				//Keysharp.Scripting.Script.hWndLastUsed = criterion_found_hwnd; // v1.0.42. Even if the window is invalid for some reason, IsWindow() and such are called whenever the script accesses it (GetValidLastUsedWindow()).
				//Accessors.A_SendLevel = variant.inputLevel;
				//Keysharp.Scripting.Script.hotCriterion = variant.hotCriterion; // v2: Let the Hotkey command use the criterion of this hotkey variant by default.
				return await hk.PerformInNewThreadMadeByCallerAsync(variant, criterion_found_hwnd);
			}

			return "";
		}

		internal void Remove(HotkeyDefinition hotkey) => _ = hotkeys.Remove(hotkey);

		internal void Remove(HotstringDefinition hotstring) => _ = hotstrings.Remove(hotstring);

		internal abstract void SendEventArray(ref long aFinalKeyDelay, int aModsDuringSend);

		internal abstract void SendKey(int aVK, int aSC, int aModifiersLR, int aModifiersLRPersistent
									   , long aRepeatCount, KeyEventTypes aEventType, int aKeyAsModifiersLR, IntPtr aTargetWindow
									   , int aX = CoordUnspecified, int aY = CoordUnspecified, bool aMoveOffset = false);

		internal abstract void SendKeyEventMenuMask(KeyEventTypes aEventType, uint aExtraInfo = KeyIgnoreAllExceptModifier);

		internal abstract void SendKeys(string aKeys, SendRawModes aSendRaw, SendModes aSendModeOrig, IntPtr aTargetWindow);

		internal abstract int SiEventCount();

		internal abstract ToggleValueType ToggleKeyState(int vk, ToggleValueType toggleValue);

		internal int TotalEventCount() => PbEventCount() + SiEventCount();

		protected internal abstract void LongOperationUpdate();

		protected internal abstract void LongOperationUpdateForSendKeys();

		//protected internal abstract void Send(string keys);

		//protected internal abstract void Send(Keys key);
		protected internal abstract void SendKeyEvent(KeyEventTypes aEventType, int aVK, int aSC = 0, IntPtr aTargetWindow = default, bool aDoKeyDelay = false, uint aExtraInfo = KeyIgnoreAllExceptModifier);

		protected abstract void RegisterHook();
	}

	internal class KeyType
	{
		internal const byte AS_PREFIX = 1;
		internal const byte AS_PREFIX_FOR_HOTKEY = 2;
		internal const byte PREFIX_ACTUAL = 1; // Values for used_as_prefix below, for places that need to distinguish between type of prefix.
		internal const byte PREFIX_FORCED = 2; // v1.0.44: Added so that a neutral hotkey like Control can be forced to fire on key-up even though it isn't actually a prefix key.
		internal byte asModifiersLR;// If this key is a modifier, this will have the corresponding bit(s) for that key.
		internal bool downPerformedAction;
		internal uint firstHotkey;
		internal ToggleStates forceToggle;  // Pointer to a global variable for toggleable keys only.  NULL for others.
		internal bool hotkeyDownWasSuppressed;// the last key-down resulted in an action (modifiers matched those of a valid hotkey)
		internal uint hotkeyToFireUponRelease; // A up-event hotkey queued by a prior down-event.
		internal bool isDown;// this key is currently down.
		internal bool itPutAltDown;// this key resulted in ALT being pushed down (due to alt-tab).
		internal bool itPutShiftDown;
		internal int noSuppress;
		internal bool scTakesPrecedence;// And these are the values for the above (besides 0):
		internal bool usedAsKeyUp;
		internal byte usedAsPrefix; // Whether a given virtual key or scan code is even used by a hotkey.
		internal bool usedAsSuffix;// The first hotkey using this key as a suffix.

		// Whether this suffix also has an enabled key-up hotkey.
		// Contains bitwise flags such as NO_SUPPRESS_PREFIX.
		// this key resulted in SHIFT being pushed down (due to shift-alt-tab).
		// Whether the down-event for a key was suppressed (thus its up-event should be too).
		// The values for "was_just_used" (zero is the initialized default, meaning it wasn't just used):
		internal byte wasJustUsed; // a non-modifier key of any kind was pressed while this prefix key was down.

		// used only by the scan code array: this scan code should take precedence over vk.

		internal void ResetKeyTypeAttrib()
		{
			firstHotkey = HotkeyDefinition.HOTKEY_ID_INVALID;
			usedAsPrefix = 0;
			usedAsSuffix = false;
			usedAsKeyUp = false;
			noSuppress &= HotkeyDefinition.NO_SUPPRESS_STATES;
			scTakesPrecedence = false;
		}

		internal void ResetKeyTypeState()
		{
			isDown = false;
			itPutAltDown = false;
			itPutShiftDown = false;
			downPerformedAction = false;
			wasJustUsed = 0;
			hotkeyToFireUponRelease = HotkeyDefinition.HOTKEY_ID_INVALID;
			// ABOVE line was added in v1.0.48.03 to fix various ways in which the hook didn't receive the key-down
			// hotkey that goes with this key-up, resulting in hotkey_to_fire_upon_release being left at its initial
			// value of zero (which is a valid hotkey ID).  Examples include:
			// The hotkey command being used to create a key-up hotkey while that key is being held down.
			// The script being reloaded or (re)started while the key is being held down.
		}

		/// <summary>
		/// This is all done because C# doesn't allow class members to be references.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		internal ToggleValueType? ToggleVal(int key)
		{
			if (forceToggle != null) // Key is a toggleable key.
			{
				if (key == WindowsAPI.VK_SCROLL)
					return forceToggle.forceScrollLock;
				else if (key == WindowsAPI.VK_CAPITAL)
					return forceToggle.forceCapsLock;
				else if (key == WindowsAPI.VK_NUMLOCK)
					return forceToggle.forceNumLock;
			}

			return null;
		}
	}
}