using static Keysharp.Core.Common.Keyboard.KeyboardUtils;
using static Keysharp.Core.Common.Keyboard.VirtualKeys;

namespace Keysharp.Core.Common.Threading
{
	internal abstract class HookThread//Fill in base stuff here later, but this serves as the thread which attaches/detaches the keyboard hooks.
	{
		internal const uint END_KEY_ENABLED = END_KEY_WITH_SHIFT | END_KEY_WITHOUT_SHIFT;
		internal const uint END_KEY_WITH_SHIFT = 0x01;
		internal const uint END_KEY_WITHOUT_SHIFT = 0x02;
		internal const uint INPUT_KEY_DOWN_SUPPRESSED = 0x80;
		internal const uint INPUT_KEY_IGNORE_TEXT = 0x10;
		internal const uint INPUT_KEY_IS_TEXT = 0x40;
		internal const uint INPUT_KEY_NOTIFY = 0x20;
		internal const uint INPUT_KEY_OPTION_MASK = 0x3F;
		internal const uint INPUT_KEY_SUPPRESS = 0x04;
		internal const uint INPUT_KEY_VISIBILITY_MASK = INPUT_KEY_SUPPRESS | INPUT_KEY_VISIBLE;
		internal const uint INPUT_KEY_VISIBLE = 0x08;
		internal const int SC_ARRAY_COUNT = SC_MAX + 1;
		internal const int SC_MAX = 0x1FF;
		internal const int VK_ARRAY_COUNT = VK_MAX + 1;
		internal const int VK_MAX = 0xFF;

		internal static readonly Channel<object> channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
		{
			SingleReader = true
		});

		internal static Mutex keybdMutex = null, mouseMutex = null;
		internal static string KeybdMutexName = "Keysharp Keybd";
		internal static Dictionary<string, uint> keyToSc = null;
		internal static Dictionary<string, uint>.AlternateLookup<ReadOnlySpan<char>> keyToScAlt;
		internal static Dictionary<string, uint> keyToVk = null;
		internal static Dictionary<string, uint>.AlternateLookup<ReadOnlySpan<char>> keyToVkAlt;
		internal static int KSCM_SIZE = (int)((MODLR_MAX + 1) * SC_ARRAY_COUNT);
		internal static int KVKM_SIZE = (int)((MODLR_MAX + 1) * VK_ARRAY_COUNT);
		internal static string MouseMutexName = "Keysharp Mouse";
		internal static Dictionary<uint, string> vkToKey = [];
		internal bool blockWinKeys = false;
		internal IntPtr hsHwnd = IntPtr.Zero;
		internal KeyboardMouseSender kbdMsSender = null;
		internal byte[] physicalKeyState = new byte[VK_ARRAY_COUNT];

		// The prefix key that's currently down (i.e. in effect).
		// It's tracked this way, rather than as a count of the number of prefixes currently down, out of
		// concern that such a count might accidentally wind up above zero (due to a key-up being missed somehow)
		// and never come back down, thus penalizing performance until the program is restarted:
		internal KeyType prefixKey = null;

		protected internal static PlatformManagerBase mgr = PlatformProvider.Manager;

		// Whether the alt-tab menu was shown by an AltTab hotkey or alt-tab was detected
		// by the hook.  This might be inaccurate if the menu was displayed before the hook
		// was installed or the keys weren't detected because of UIPI.  If this turns out to
		// be a problem, the accuracy could be improved by additional checks with FindWindow(),
		// keeping in mind that there are at least 3 different window classes to check,
		// depending on OS and the "AltTabSettings" registry value.
		protected internal bool altTabMenuIsVisible = false;

		protected internal Task<Task> channelReadThread;

		protected internal uint channelThreadID = 0u;

		// Whether to disguise the next up-event for lwin/rwin to suppress Start Menu.
		// There is only one variable because even if multiple modifiers are pressed
		// simultaneously and they do not cancel each other out, disguising one will
		// have the effect of disguising all (with the exception that key-repeat can
		// reset LWin/RWin after the other modifier is released, so this variable
		// should not be reset until all Win keys are released).
		// These are made global, rather than static inside the hook function, so that
		// we can ensure they are initialized by the keyboard init function every
		// time it's called (currently it can be only called once):
		protected internal bool disguiseNextMenu = false;

		protected internal bool hookSynced = false;
		protected internal List<uint> hotkeyUp = new List<uint>(256);
		protected internal IntPtr kbdHook = IntPtr.Zero;
		protected internal KeyHistory keyHistory = new KeyHistory();
		protected internal KeyType[] ksc;
		protected internal uint[] kscm;
		protected internal KeyType[] kvk;
		protected internal uint[] kvkm;
		protected internal IntPtr mouseHook = IntPtr.Zero;
		protected internal bool undisguisedMenuInEffect = false;
		protected volatile bool running;

		internal HookThread()
		{
		}

		public abstract void SimulateKeyPress(uint key);

		internal abstract void AddRemoveHooks(HookType hooksToBeActive, bool changeIsTemporary = false);

		internal abstract void ChangeHookState(List<HotkeyDefinition> hk, HookType whichHook, HookType whichHookAlways);

		internal abstract uint CharToVKAndModifiers(char ch, ref uint? modifiersLR, IntPtr keybdLayout, bool enableAZFallback = false);

		internal uint ConvertMouseButton(string buf, bool allowWheel = true) => ConvertMouseButton(buf.AsSpan(), allowWheel);

		internal abstract uint ConvertMouseButton(ReadOnlySpan<char> buf, bool allowWheel = true);

		internal HookType GetActiveHooks()
		{
			var hookscurrentlyactive = HookType.None;

			if (HasKbdHook())
				hookscurrentlyactive |= HookType.Keyboard;

			if (HasMouseHook())
				hookscurrentlyactive |= HookType.Mouse;

			return hookscurrentlyactive;
		}

		internal string GetHookStatus()
		{
			var sb = new StringBuilder(2048);
			_ = sb.AppendLine($"Modifiers (Hook's Logical) = {kbdMsSender.ModifiersLRToText(kbdMsSender.modifiersLRLogical)}");
			_ = sb.AppendLine($"Modifiers (Hook's Physical) = {kbdMsSender.ModifiersLRToText(kbdMsSender.modifiersLRPhysical)}");
			_ = sb.AppendLine($"Prefix key is down: {(prefixKey != null ? "yes" : "no")}");

			if (!HasKbdHook())
			{
				_ = sb.Append("NOTE: Only the script's own keyboard events are shown");
				_ = sb.Append("(not the user's), because the keyboard hook isn't installed.");
			}

			// Add the below even if key history is already disabled so that the column headings can be seen.
			_ = sb.AppendLine();
			_ = sb.Append("NOTE: To disable the key history shown below, call KeyHistory(0). ");
			_ = sb.Append("The same method can be used to change the size of the history buffer. ");
			_ = sb.AppendLine("For example: KeyHistory 100 (Default is 40, Max is 500)");
			_ = sb.AppendLine();
			_ = sb.Append("The oldest are listed first. VK=Virtual Key, SC=Scan Code, Elapsed=Seconds since the previous event");
			_ = sb.Append(". Types: h=Hook Hotkey, s=Suppressed (blocked), i=Ignored because it was generated by an AHK script");
			_ = sb.AppendLine(", a=Artificial, #=Disabled via #HotIf, U=Unicode character (SendInput).");
			_ = sb.AppendLine();
			_ = sb.AppendLine("VK  SC\tType\tUp/Dn\tElapsed\tKey\t\tWindow");
			_ = sb.Append("-------------------------------------------------------------------------------------------------------------");

			if (keyHistory != null && keyHistory.Size > 0)
				_ = sb.Append(keyHistory.ToString());

			return sb.ToString();
		}

		internal bool HasEitherHook() => GetActiveHooks() != HookType.None;

		internal bool HasKbdHook() => kbdHook != IntPtr.Zero;

		internal bool HasMouseHook() => mouseHook != IntPtr.Zero;

		internal virtual bool IsHotstringWordChar(char ch) => char.IsLetterOrDigit(ch) ? true : !char.IsWhiteSpace(ch);

		internal abstract bool IsKeyDown(uint vk);

		internal abstract bool IsKeyDownAsync(uint vk);

		internal abstract bool IsKeyToggledOn(uint vk);

		internal abstract bool IsMouseVK(uint vk);

		internal bool IsReadThreadCompleted()
		=> channelReadThread != null&&
		channelReadThread.Result != null&&
		channelReadThread.Result.IsCompleted&&
		channelReadThread.IsCompleted;

		internal bool IsReadThreadRunning()
		=> running&&
		channelReadThread != null&&
		channelReadThread.Result != null&&
		!channelReadThread.Result.IsCompleted;

		internal abstract bool IsWheelVK(uint vk);

		/// <summary>
		/// Convert the given virtual key / scan code to its equivalent bitwise modLR value.
		/// Callers rely upon the fact that we convert a neutral key such as VK_SHIFT into MOD_LSHIFT,
		/// not the bitwise combo of MOD_LSHIFT|MOD_RSHIFT.
		/// v1.0.43: VK_SHIFT should yield MOD_RSHIFT if the caller explicitly passed the right vs. left scan code.
		/// The SendPlay method relies on this to properly release AltGr, such as after "SendPlay @" in German.
		/// Other things may also rely on it because it is more correct.
		/// </summary>
		/// <param name="vk"></param>
		/// <param name="sc"></param>
		/// <param name="pIsNeutral"></param>
		/// <returns></returns>
		internal virtual uint KeyToModifiersLR(uint vk, uint sc, ref bool? isNeutral)
		{
			if (vk == 0 && sc == 0)
				return 0;

			if (vk != 0) // Have vk take precedence over any non-zero sc.
			{
				switch (vk)
				{
					case VK_SHIFT:
						if (sc == ScanCodes.RShift)
							return MOD_RSHIFT;

						//else aSC is omitted (0) or SC_LSHIFT.  Either way, most callers would probably want that considered "neutral".
						if (isNeutral != null)
							isNeutral = true;

						return MOD_LSHIFT;

					case VK_LSHIFT: return MOD_LSHIFT;

					case VK_RSHIFT: return MOD_RSHIFT;

					case VK_CONTROL:
						if (sc == ScanCodes.RControl)
							return MOD_RCONTROL;

						//else aSC is omitted (0) or SC_LCONTROL.  Either way, most callers would probably want that considered "neutral".
						if (isNeutral != null)
							isNeutral = true;

						return MOD_LCONTROL;

					case VK_LCONTROL: return MOD_LCONTROL;

					case VK_RCONTROL: return MOD_RCONTROL;

					case VK_MENU:
						if (sc == ScanCodes.RAlt)
							return MOD_RALT;

						//else aSC is omitted (0) or SC_LALT.  Either way, most callers would probably want that considered "neutral".
						if (isNeutral != null)
							isNeutral = true;

						return MOD_LALT;

					case VK_LMENU: return MOD_LALT;

					case VK_RMENU: return MOD_RALT;

					case VK_LWIN: return MOD_LWIN;

					case VK_RWIN: return MOD_RWIN;

					default:
						return 0;
				}
			}

			// If above didn't return, rely on the scan code instead, which is now known to be non-zero.
			switch (sc)
			{
				case ScanCodes.LShift: return MOD_LSHIFT;

				case ScanCodes.RShift: return MOD_RSHIFT;

				case ScanCodes.LControl: return MOD_LCONTROL;

				case ScanCodes.RControl: return MOD_RCONTROL;

				case ScanCodes.LAlt: return MOD_LALT;

				case ScanCodes.RAlt: return MOD_RALT;

				case ScanCodes.LWin: return MOD_LWIN;

				case ScanCodes.RWin: return MOD_RWIN;
			}

			return 0;
		}

		internal ref uint Kscm(uint i, uint j) => ref kscm[(i * SC_ARRAY_COUNT) + j];

		internal ref uint Kvkm(uint i, uint j) => ref kvkm[(i * VK_ARRAY_COUNT) + j];

		internal void LinkKeysForCustomCombo(uint neutral, uint left, uint right)
		{
			var first_neutral = kvk[neutral].firstHotkey;

			if (first_neutral == HotkeyDefinition.HOTKEY_ID_INVALID)
				return;

			// Append the neutral key's list to the lists of the left and right keys.
			HotkeyDefinition.CustomComboLast(ref kvk[left].firstHotkey) = first_neutral;
			HotkeyDefinition.CustomComboLast(ref kvk[right].firstHotkey) = first_neutral;
		}

		internal abstract uint MapScToVk(uint sc);

		internal abstract uint MapVkToSc(uint vk, bool returnSecondary = false);

		internal void ParseClickOptions(string options, ref int x, ref int y, ref uint vk, ref KeyEventTypes eventType, ref long repeatCount, ref bool moveOffset) =>
		ParseClickOptions(options.AsSpan(), ref x, ref y, ref vk, ref eventType, ref repeatCount, ref moveOffset);

		internal abstract void ParseClickOptions(ReadOnlySpan<char> options, ref int x, ref int y, ref uint vk, ref KeyEventTypes eventType, ref long repeatCount, ref bool moveOffset);

		internal bool PostMessage(KeysharpMsg msg)
		=> IsReadThreadRunning()&& channel.Writer.TryWrite(msg);

		internal string SCtoKeyName(uint sc, bool useFallback)
		{
			foreach (var kv in keyToSc)
				if (kv.Value == sc)
					return kv.Key;

			// Since above didn't return, no match was found.  Use the default format for an unknown scan code:
			return useFallback ? "sc" + sc.ToString("X3") : "";
		}

		internal void Stop()
		{
			if (running)
			{
				try
				{
					channel?.Writer?.Complete();
				}
				catch (ChannelClosedException) { }

				if (channelReadThread != null && channelReadThread.Result != null && channelReadThread.Result.Status != TaskStatus.WaitingForActivation && !channelReadThread.Result.IsCompleted)
					channelReadThread?.Result?.Wait();

				channelReadThread?.Wait();
				channelReadThread = null;
				running = false;
			}
		}

		//This is relied upon to be unsigned; e.g. many places omit a check for ID < 0.
		internal abstract bool SystemHasAnotherKeybdHook();

		internal abstract bool SystemHasAnotherMouseHook();

		internal uint TextToSC(string text, ref bool? specifiedByNumber) =>
		TextToSC(text.AsSpan(), ref specifiedByNumber);

		internal abstract uint TextToSC(ReadOnlySpan<char> text, ref bool? specifiedByNumber);

		internal uint TextToSpecial(string text, ref KeyEventTypes eventType, ref uint modifiersLR, bool updatePersistent) =>
		TextToSpecial(text.AsSpan(), ref eventType, ref modifiersLR, updatePersistent);

		internal abstract uint TextToSpecial(ReadOnlySpan<char> text, ref KeyEventTypes eventType, ref uint modifiersLR, bool updatePersistent);

		internal uint TextToVK(string text, ref uint? modifiersLR, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout) =>
		TextToVK(text.AsSpan(), ref modifiersLR, excludeThoseHandledByScanCode, allowExplicitVK, keybdLayout);

		internal abstract uint TextToVK(ReadOnlySpan<char> text, ref uint? modifiersLR, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout);

		internal bool TextToVKandSC(string text, ref uint vk, ref uint sc, ref uint? modifiersLR, IntPtr keybdLayout) =>
		TextToVKandSC(text.AsSpan(), ref vk, ref sc, ref modifiersLR, keybdLayout);

		internal abstract bool TextToVKandSC(ReadOnlySpan<char> text, ref uint vk, ref uint sc, ref uint? modifiersLR, IntPtr keybdLayout);

		internal abstract void Unhook();

		internal abstract char VKtoChar(uint vk, IntPtr keybdLayout);

		internal string VKtoKeyName(uint vk, bool useFallback)
		{
			if (vkToKey.TryGetValue(vk, out var name))
				return name;

			// Since above didn't return, no match was found.  Try to map it to
			// a character or use the default format for an unknown key code:
			var ch = VKtoChar(vk, IntPtr.Zero);

			if (ch != (char)0)
				return ch.ToString();
			else if (useFallback && vk != 0)
				return "vk" + vk.ToString("X2");
			else
				return "";
		}

		internal abstract void WaitHookIdle();

		protected internal uint ChannelThreadId() => channelThreadID;

		protected internal abstract void DeregisterHooks();

		protected internal void FreeHookMem()
		{
			kvkm = System.Array.Empty<uint>();
			kscm = System.Array.Empty<uint>();
			hotkeyUp.Clear();
			kvk = System.Array.Empty<KeyType>();
			ksc = System.Array.Empty<KeyType>();
		}

		protected internal abstract void Start();
	}

	// WM_USER (0x0400) is the lowest number that can be a user-defined message.  Anything above that is also valid.
	// NOTE: Any msg about WM_USER will be kept buffered (unreplied-to) whenever the script is uninterruptible.
	// If this is a problem, try making the msg have an ID less than WM_USER via a technique such as that used
	// for AHK_USER_MENU (perhaps WM_COMMNOTIFY can be "overloaded" to contain more than one type of msg).
	// Also, it has been announced in OnMessage() that message numbers between WM_USER and 0x1000 are earmarked
	// for possible future use by the program, so don't use a message above 0x1000 without good reason.
	//Make sure the minimum value of WM_USER (0x0400) is ok for linux.//TODO
	internal enum UserMessages : uint
	{
		AHK_HOOK_HOTKEY = 0x0400, AHK_HOTSTRING, AHK_USER_MENU, AHK_DIALOG, AHK_NOTIFYICON
		, AHK_UNUSED_MSG, AHK_EXIT_BY_RELOAD, AHK_EXIT_BY_SINGLEINSTANCE, AHK_CHECK_DEBUGGER

		// Allow some room here in between for more "exit" type msgs to be added in the future (see below comment).
		, AHK_GUI_ACTION = 0x0400 + 20 // Avoid WM_USER+100/101 and vicinity.  See below comment.

		// v1.0.43.05: On second thought, it seems better to stay close to WM_USER because the OnMessage page
		// documents, "it is best to choose a number greater than 4096 (0x1000) to the extent you have a choice.
		// This reduces the chance of interfering with messages used internally by current and future versions..."
		// v1.0.43.03: Changed above msg number because Micha reports that previous number (WM_USER+100) conflicts
		// with msgs sent by HTML control (AHK_CLIPBOARD_CHANGE) and possibly others (I think WM_USER+100 may be the
		// start of a range used by other common controls too).  So trying a higher number that's (hopefully) very
		// unlikely to be used by OS features.
		, AHK_CLIPBOARD_CHANGE, AHK_HOOK_TEST_MSG, AHK_CHANGE_HOOK_STATE, AHK_GETWINDOWTEXT

		, AHK_HOT_IF_EVAL   // HotCriterionAllowsFiring uses this to ensure expressions are evaluated only on the main thread.
		, AHK_HOOK_SYNC // For WaitHookIdle().
		, AHK_INPUT_END, AHK_INPUT_KEYDOWN, AHK_INPUT_CHAR, AHK_INPUT_KEYUP
		, AHK_HOOK_SET_KEYHISTORY
		, AHK_START_LOOP
	}

	// NOTE: TRY NEVER TO CHANGE the specific numbers of the above messages, since some users might be
	// using the Post/SendMessage commands to automate AutoHotkey itself.  Here is the original order
	// that should be maintained:
	// AHK_HOOK_HOTKEY = WM_USER, AHK_HOTSTRING, AHK_USER_MENU, AHK_DIALOG, AHK_NOTIFYICON, AHK_RETURN_PID
}