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
		internal const string ModLRString = "<^>^<!>!<+>+<#>#";
		internal const uint MsgOffsetMouseMove = 0x80000000;
		internal const uint SendLevelMax = 100u;
		internal const int StateDown = 0x80;
		internal const int StateOn = 0x01;
		internal uint altGrExtraInfo = 0u;
		internal static char[] bracechars = "{}".ToCharArray();
		internal SearchValues<char> bracecharsSv = SearchValues.Create(bracechars);
		internal static string[] CoordModes = ["Client", "Window", "Screen"];
		internal static char[] llChars = "Ll".ToCharArray();
		internal SearchValues<char> llCharsSv = SearchValues.Create(llChars);
		internal KeyType prefixKey = null;
		internal string sendKeyChars = "^+!#{}";
		internal uint thisHotkeyModifiersLR;
		internal bool abortArraySend = false;
		internal int maxEvents = 0;
		internal uint modifiersLRCtrlAltDelMask = 0u;
		internal uint modifiersLRLastPressed = 0u;
		internal DateTime modifiersLRLastPressedTime = DateTime.UtcNow;
		internal uint modifiersLRLogical = 0u;
		internal uint modifiersLRLogicalNonIgnored = 0u;
		internal uint modifiersLRNumpadMask = 0u;
		internal uint modifiersLRPhysical = 0u;
		protected internal PlatformManagerBase mgr = script.PlatformProvider.Manager;
		protected ArrayPool<byte> keyStatePool = ArrayPool<byte>.Create(256, 100);
		protected SendModes sendMode = SendModes.Event;//Note this is different than the one in Accessors and serves as a temporary.
		private const int retention = 1024;
		private readonly StringBuilder caser = new (32);
		private readonly List<HotkeyDefinition> hotkeys;
		private readonly List<HotstringDefinition> hotstrings;
		private readonly Dictionary<Keys, bool> pressed;

		public KeyboardMouseSender()
		{
			hotkeys = [];
			hotstrings = [];
			pressed = [];

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
				System.Diagnostics.Debug.Fail("Thre should'nt be any key not in this table...");
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

		internal abstract void DoMouseDelay();

		internal abstract IntPtr GetFocusedKeybdLayout(IntPtr aWindow);

		internal abstract uint GetModifierLRState(bool aExplicitlyGet = false);

		internal abstract void InitEventArray(int maxEvents, uint aModifiersLR);

		internal abstract string ModifiersLRToText(uint aModifiersLR);

		internal abstract void MouseClick(uint aVK, int aX, int aY, long aRepeatCount, long aSpeed, KeyEventTypes aEventType, bool aMoveOffset);

		internal abstract void MouseClickDrag(uint vk, int x1, int y1, int x2, int y2, long speed, bool relative);

		internal abstract void MouseEvent(uint aEventFlags, uint aData, int aX = CoordUnspecified, int aY = CoordUnspecified);

		internal abstract void MouseMove(ref int aX, ref int aY, ref uint aEventFlags, long aSpeed, bool aMoveOffset);

		internal abstract int PbEventCount();

		internal void PerformMouseCommon(Actions actionType, uint vk, int x1, int y1, int x2, int y2,
										 long repeatCount, KeyEventTypes eventType, long speed, bool relative)
		{
			// The maximum number of events, which in this case would be from a MouseClickDrag.  To be conservative
			// (even though INPUT is a much larger struct than PlaybackEvent and SendInput doesn't use mouse-delays),
			// include room for the maximum number of mouse delays too.
			// Drag consists of at most:
			// 1) Move; 2) Delay; 3) Down; 4) Delay; 5) Move; 6) Delay; 7) Delay (dupe); 8) Up; 9) Delay.
			const int MAX_PERFORM_MOUSE_EVENTS = 10;
			var ht = script.HookThread;
			sendMode = ThreadAccessors.A_SendMode;

			if (sendMode == SendModes.Input || sendMode == SendModes.InputThenPlay)
			{
				if (ht.SystemHasAnotherMouseHook()) // See similar section in SendKeys() for details.
					sendMode = (sendMode == SendModes.Input) ? SendModes.Event : SendModes.Play;
				else
					sendMode = SendModes.Input; // Resolve early so that other sections don't have to consider SM_INPUT_FALLBACK_TO_PLAY a valid value.
			}

			if (sendMode != SendModes.Event) // We're also responsible for setting sSendMode to SM_EVENT prior to returning.
				InitEventArray(MAX_PERFORM_MOUSE_EVENTS, 0);

			// Turn it on unconditionally even if it was on, since Ctrl-Alt-Del might have disabled it.
			// Turn it back off only if it wasn't ON before we started.
			var blockinputPrev = script.KeyboardData.blockInput;
			var doSelectiveBlockinput = (script.KeyboardData.blockInputMode == ToggleValueType.Mouse
										 || script.KeyboardData.blockInputMode == ToggleValueType.SendAndMouse)
										&& sendMode == SendModes.Event;

			if (doSelectiveBlockinput) // It seems best NOT to use g_BlockMouseMove for this, since often times the user would want keyboard input to be disabled too, until after the mouse event is done.
				_ = Core.Keyboard.ScriptBlockInput(true); // Turn it on unconditionally even if it was on, since Ctrl-Alt-Del might have disabled it.

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

			if (sendMode != SendModes.Event)
			{
				var finalKeyDelay = -1L; // Set default.

				if (!abortArraySend && TotalEventCount() > 0)
					SendEventArray(ref finalKeyDelay, 0); // Last parameter is ignored because keybd hook isn't removed for a pure-mouse SendInput.

				CleanupEventArray(finalKeyDelay);
			}

			if (doSelectiveBlockinput && !blockinputPrev)  // Turn it back off only if it was off before we started.
				_ = Core.Keyboard.ScriptBlockInput(false);
		}

		internal void ProcessHotkey(int wParamVal, int lParamVal, HotkeyVariant variant, uint msg)
		{
			var hkId = wParamVal & HotkeyDefinition.HOTKEY_ID_MASK;
			var shk = script.HotkeyData.shk;

			if (hkId < shk.Count)//Ensure hotkey ID is valid.
			{
				var hk = shk[hkId];
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
				var criterion_found_hwnd = 0L;

				if (!(variant != null || (variant = hk.CriterionAllowsFiring(ref criterion_found_hwnd, msg == (uint)UserMessages.AHK_HOOK_HOTKEY ? KeyIgnoreLevel((uint)Conversions.HighWord(lParamVal)) : 0, ref dummy)) != null))
					return;

				if (!script.Threads.AnyThreadsAvailable())//First test global thread count.
					return;

				// If this is AHK_HOOK_HOTKEY, criterion was eligible at time message was posted,
				// but not now.  Seems best to abort (see other comments).
				// Due to the key-repeat feature and the fact that most scripts use a value of 1
				// for their #MaxThreadsPerHotkey, this check will often help average performance
				// by avoiding a lot of unnecessary overhead that would otherwise occur:
				if (!variant.AnyThreadsAvailable())//Then test local thread count.
				{
					// The key is buffered in this case to boost the responsiveness of hotkeys
					// that are being held down by the user to activate the keyboard's key-repeat
					// feature.  This way, there will always be one extra event waiting in the queue,
					// which will be fired almost the instant the previous iteration of the subroutine
					// finishes (this above description applies only when MaxThreadsPerHotkey is 1,
					// which it usually is).
					variant.RunAgainAfterFinished(); // Wheel notch count (g->EventInfo below) should be okay because subsequent launches reuse the same thread attributes to do the repeats.
					return;
				}

				var tv = script.Threads.GetThreadVariables();

				// Now that above has ensured variant is non-NULL:
				if (variant.priority >= tv.priority)//Finally, test priority.
				{
					// Above also works for RunAgainAfterFinished since that feature reuses the same thread attributes set above.
					hk.PerformInNewThreadMadeByCallerAsync(variant, criterion_found_hwnd, lParamVal);
				}
			}
		}

		internal void Remove(HotkeyDefinition hotkey) => _ = hotkeys.Remove(hotkey);

		internal void Remove(HotstringDefinition hotstring) => _ = hotstrings.Remove(hotstring);

		internal abstract void SendEventArray(ref long aFinalKeyDelay, uint aModsDuringSend);

		internal abstract void SendKey(uint aVK, uint aSC, uint aModifiersLR, uint aModifiersLRPersistent
									   , long aRepeatCount, KeyEventTypes aEventType, uint aKeyAsModifiersLR, IntPtr aTargetWindow
									   , int aX = CoordUnspecified, int aY = CoordUnspecified, bool aMoveOffset = false);

		internal abstract void SendKeyEventMenuMask(KeyEventTypes aEventType, uint aExtraInfo = KeyIgnoreAllExceptModifier);

		internal abstract void SendKeys(string aKeys, SendRawModes aSendRaw, SendModes aSendModeOrig, IntPtr aTargetWindow);

		internal abstract int SiEventCount();

		internal abstract ToggleValueType ToggleKeyState(uint vk, ToggleValueType toggleValue);

		internal int TotalEventCount() => PbEventCount() + SiEventCount();

		protected internal abstract void LongOperationUpdate();

		protected internal abstract void LongOperationUpdateForSendKeys();

		//protected internal abstract void Send(string keys);

		//protected internal abstract void Send(Keys key);
		protected internal abstract void SendKeyEvent(KeyEventTypes aEventType, uint aVK, uint aSC = 0u, IntPtr aTargetWindow = default, bool aDoKeyDelay = false, uint aExtraInfo = KeyIgnoreAllExceptModifier);

		protected abstract void RegisterHook();
	}
}