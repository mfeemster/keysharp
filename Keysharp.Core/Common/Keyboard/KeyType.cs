namespace Keysharp.Core.Common.Keyboard
{
	internal class KeyType
	{
		internal const int AS_PASSTHROUGH_PREFIX = -1;
		internal const int AS_PREFIX = 1;
		internal const int AS_PREFIX_FOR_HOTKEY = 2;
		internal const int PREFIX_ACTUAL = 1; // Values for used_as_prefix below, for places that need to distinguish between type of prefix.
		internal const int PREFIX_FORCED = 2; // v1.0.44: Added so that a neutral hotkey like Control can be forced to fire on key-up even though it isn't actually a prefix key.
		internal uint asModifiersLR = 0u;// If this key is a modifier, this will have the corresponding bit(s) for that key.
		internal bool downPerformedAction;
		internal uint firstHotkey;
		internal ToggleStates forceToggle = null;  // Pointer to a global variable for toggleable keys only.  NULL for others.
		internal bool hotkeyDownWasSuppressed = false;// the last key-down resulted in an action (modifiers matched those of a valid hotkey)
		internal uint hotkeyToFireUponRelease; // A up-event hotkey queued by a prior down-event.
		internal bool isDown;// this key is currently down.
		internal bool itPutAltDown;// this key resulted in ALT being pushed down (due to alt-tab).
		internal bool itPutShiftDown;
		internal uint noSuppress;
		internal bool scTakesPrecedence;// used only by the scan code array: this scan code should take precedence over vk.
		internal bool usedAsKeyUp;
		internal byte usedAsPrefix; // Whether a given virtual key or scan code is even used by a hotkey.
		internal bool usedAsSuffix;// The first hotkey using this key as a suffix.

		// Whether this suffix also has an enabled key-up hotkey.
		// Contains bitwise flags such as NO_SUPPRESS_PREFIX.
		// this key resulted in SHIFT being pushed down (due to shift-alt-tab).
		// Whether the down-event for a key was suppressed (thus its up-event should be too).
		// The values for "was_just_used" (zero is the initialized default, meaning it wasn't just used):
		internal int wasJustUsed;

		internal uint Pos { get; }

		// a non-modifier key of any kind was pressed while this prefix key was down.

		internal KeyType(uint p)
		{
			Pos = p;
		}

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
		/// <param name="vk"></param>
		/// <returns></returns>
		internal ToggleValueType? ToggleVal(uint vk)
		{
			var key = (Keys)vk;

			if (forceToggle != null) // Key is a toggleable key.
			{
				if (key == Keys.Scroll)
					return forceToggle.forceScrollLock;
				else if (key == Keys.Capital)
					return forceToggle.forceCapsLock;
				else if (key == Keys.NumLock)
					return forceToggle.forceNumLock;
			}

			return null;
		}
	}
}
