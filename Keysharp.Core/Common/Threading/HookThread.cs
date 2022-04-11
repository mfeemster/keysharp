using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Keysharp.Core.Common.Keyboard;

namespace Keysharp.Core.Common.Threading
{
	internal abstract class HookThread//Fill in base stuff here later, but this serves as the thread which attaches/detaches the keyboard hooks.
	{
		internal const int END_KEY_ENABLED = END_KEY_WITH_SHIFT | END_KEY_WITHOUT_SHIFT;
		internal const int END_KEY_WITH_SHIFT = 0x01;
		internal const int END_KEY_WITHOUT_SHIFT = 0x02;
		internal const int INPUT_KEY_DOWN_SUPPRESSED = 0x80;
		internal const int INPUT_KEY_IGNORE_TEXT = 0x10;
		internal const int INPUT_KEY_IS_TEXT = 0x40;
		internal const int INPUT_KEY_NOTIFY = 0x20;
		internal const int INPUT_KEY_OPTION_MASK = 0x3F;
		internal const int INPUT_KEY_SUPPRESS = 0x04;
		internal const int INPUT_KEY_VISIBILITY_MASK = INPUT_KEY_SUPPRESS | INPUT_KEY_VISIBLE;
		internal const int INPUT_KEY_VISIBLE = 0x08;
		internal const int SC_ARRAY_COUNT = SC_MAX + 1;
		internal const int SC_MAX = 0x1FF;
		internal const int VK_ARRAY_COUNT = VK_MAX + 1;

		// The maximum number of virtual keys and scan codes that can ever exist.
		internal const int VK_MAX = 0xFF;

		internal static readonly Channel<object> channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
		{
			SingleReader = true
		});

		internal static System.Threading.Mutex keybdMutex, mouseMutex;
		internal static string KeybdMutexName = "Keysharp Keybd";
		internal static Dictionary<string, int> keyToSc;
		internal static Dictionary<string, int> keyToVk;
		internal static int KSCM_SIZE = (KeyboardMouseSender.MODLR_MAX + 1) * SC_ARRAY_COUNT;
		internal static int KVKM_SIZE = (KeyboardMouseSender.MODLR_MAX + 1) * VK_ARRAY_COUNT;
		internal static string MouseMutexName = "Keysharp Mouse";
		internal static string[] vksc = new string[] { "vk", "sc" };
		internal static Dictionary<int, string> vkToKey = new Dictionary<int, string>();
		internal bool blockWinKeys;
		internal IntPtr hsHwnd = IntPtr.Zero;
		internal bool hsResetUponMouseClick = true;
		internal Keysharp.Core.Common.Keyboard.KeyboardMouseSender kbdMsSender;
		internal byte[] physicalKeyState = new byte[VK_ARRAY_COUNT];

		// The prefix key that's currently down (i.e. in effect).
		// It's tracked this way, rather than as a count of the number of prefixes currently down, out of
		// concern that such a count might accidentally wind up above zero (due to a key-up being missed somehow)
		// and never come back down, thus penalizing performance until the program is restarted:
		internal KeyType prefixKey;

		// Whether the alt-tab menu was shown by an AltTab hotkey or alt-tab was detected
		// by the hook.  This might be inaccurate if the menu was displayed before the hook
		// was installed or the keys weren't detected because of UIPI.  If this turns out to
		// be a problem, the accuracy could be improved by additional checks with FindWindow(),
		// keeping in mind that there are at least 3 different window classes to check,
		// depending on OS and the "AltTabSettings" registry value.
		protected internal bool altTabMenuIsVisible;

		protected internal System.Threading.Tasks.Task channelReadThread;

		// Whether to disguise the next up-event for lwin/rwin to suppress Start Menu.
		// There is only one variable because even if multiple modifiers are pressed
		// simultaneously and they do not cancel each other out, disguising one will
		// have the effect of disguising all (with the exception that key-repeat can
		// reset LWin/RWin after the other modifier is released, so this variable
		// should not be reset until all Win keys are released).
		// These are made global, rather than static inside the hook function, so that
		// we can ensure they are initialized by the keyboard init function every
		// time it's called (currently it can be only called once):
		protected internal bool disguiseNextMenu;

		protected internal bool hookSynced;
		protected internal uint hookThreadID;
		protected internal List<uint> hotkeyUp = new List<uint>(256);

		// Hot-string vars (initialized when ResetHook() is first called):
		protected internal char[] hsBuf = new char[HotstringDefinition.HS_BUF_SIZE];

		protected internal IntPtr kbdHook = IntPtr.Zero;
		protected internal List<KeyHistoryItem> keyHistory = new List<KeyHistoryItem>(40);
		protected internal int keyHistoryNext;
		protected internal KeyType[] ksc;
		protected internal uint[] kscm;
		protected internal KeyType[] kvk;
		protected internal uint[] kvkm;
		protected internal IntPtr mouseHook = IntPtr.Zero;

		// Initialized by ResetHook().
		protected internal bool undisguisedMenuInEffect;

		protected volatile bool running;

		internal HookThread()
		{
			for (var i = 0; i < keyHistory.Capacity; i++)
				keyHistory.Add(new KeyHistoryItem());
		}

		internal abstract void AddRemoveHooks(HookType hooksToBeActive, bool changeIsTemporary = false);

		//~HookThread()
		//{
		//  Stop();
		//}
		internal abstract void ChangeHookState(List<HotkeyDefinition> hk, HookType whichHook, HookType whichHookAlways);

		internal abstract int CharToVKAndModifiers(char ch, ref int? modifiersLR, IntPtr keybdLayout, bool enableAZFallback = false);

		internal void ClearKeyHistory()
		{
			keyHistory.Clear();
			Keysharp.Scripting.Script.historyTickPrev = DateTime.Now;
			Keysharp.Scripting.Script.historyHwndPrev = IntPtr.Zero;
		}

		internal HookType GetActiveHooks()
		{
			var hookscurrentlyactive = HookType.None;

			if (HasKbdHook())
				hookscurrentlyactive |= HookType.Keyboard;

			if (HasMouseHook())
				hookscurrentlyactive |= HookType.Mouse;

			return hookscurrentlyactive;
		}

		public abstract void SimulateKeyPress(uint key);

		internal bool HasKbdHook() => kbdHook != IntPtr.Zero;

		internal bool HasMouseHook() => mouseHook != IntPtr.Zero;

		internal virtual bool IsHotstringWordChar(char ch) => char.IsLetterOrDigit(ch) ? true : !char.IsWhiteSpace(ch);

		internal abstract bool IsKeyDown(int vk);

		internal abstract bool IsKeyDownAsync(int vk);

		internal abstract bool IsKeyToggledOn(int vk);

		internal abstract bool IsMouseVK(int vk);

		internal abstract bool IsWheelVK(int vk);

		internal abstract int KeyToModifiersLR(int vk, int sc, ref bool? isNeutral);

		internal ref uint Kscm(int i, int j) => ref kscm[(i * SC_ARRAY_COUNT) + j];

		internal ref uint Kvkm(int i, int j) => ref kvkm[(i * VK_ARRAY_COUNT) + j];

		//
		// Initialized by ResetHook().
		internal void LinkKeysForCustomCombo(int neutral, int left, int right)
		{
			var first_neutral = kvk[neutral].firstHotkey;

			if (first_neutral == HotkeyDefinition.HOTKEY_ID_INVALID)
				return;

			// Append the neutral key's list to the lists of the left and right keys.
			HotkeyDefinition.CustomComboLast(ref kvk[left].firstHotkey) = first_neutral;
			HotkeyDefinition.CustomComboLast(ref kvk[right].firstHotkey) = first_neutral;
		}

		internal abstract int MapScToVk(int sc);

		internal abstract int MapVkToSc(int vk, bool returnSecondary = false);

		internal abstract void ParseClickOptions(string options, ref int x, ref int y, ref int vk, ref KeyEventTypes eventType, ref int repeatCount, ref bool moveOffset);

		internal string SCtoKeyName(int sc, bool useFallback)
		{
			foreach (var kv in keyToSc)
				if (kv.Value == sc)
					return kv.Key;

			// Since above didn't return, no match was found.  Use the default format for an unknown scan code:
			return useFallback ? "sc" + sc.ToString("X3") : "";
		}

		internal void Stop()
		{
			//DeregisterHooks();
			channel?.Writer?.Complete();
			channelReadThread?.Wait();
			//DeregisterKeyboardHook();
			//DeregisterMouseHook();
			running = false;
		}

		//This is relied upon to be unsigned; e.g. many places omit a check for ID < 0.
		internal abstract bool SystemHasAnotherKeybdHook();

		internal abstract bool SystemHasAnotherMouseHook();

		internal abstract int TextToSC(string text, ref bool? specifiedByNumber);

		internal abstract int TextToSpecial(string text, ref KeyEventTypes eventType, ref int modifiersLR, bool updatePersistent);

		internal abstract int TextToVK(string text, ref int? modifiersLR, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout);

		internal abstract bool TextToVKandSC(string text, ref int vk, ref int sc, ref int? modifiersLR, IntPtr keybdLayout);

		internal void UpdateKeyEventHistory(bool keyUp, int vk, int sc)
		{
			if (keyHistory == null) // Don't access the array if it doesn't exist (i.e. key history is disabled).
				return;

			keyHistory[keyHistoryNext].keyUp = keyUp;
			keyHistory[keyHistoryNext].vk = vk;
			keyHistory[keyHistoryNext].sc = sc;
			keyHistory[keyHistoryNext].eventType = 'i'; // Callers all want this.
			Keysharp.Scripting.Script.historyTickNow = DateTime.Now;
			keyHistory[keyHistoryNext].elapsedTime = (Keysharp.Scripting.Script.historyTickNow - Keysharp.Scripting.Script.historyTickPrev).TotalMilliseconds / 1000;
			Keysharp.Scripting.Script.historyTickPrev = Keysharp.Scripting.Script.historyTickNow;
			var item = Keysharp.Core.Common.Window.WindowManagerProvider.Instance.GetForeGroundWindow();
			var forewin = item.Handle;

			if (forewin != IntPtr.Zero)
			{
				if (forewin != Keysharp.Scripting.Script.historyHwndPrev)
					keyHistory[keyHistoryNext].targetWindow = item.Title;
				else // i.e. avoid the call to GetWindowText() if possible.
					keyHistory[keyHistoryNext].targetWindow = "";
			}
			else
				keyHistory[keyHistoryNext].targetWindow = "N/A";

			Keysharp.Scripting.Script.historyHwndPrev = forewin; // Update unconditionally in case it's NULL.

			if (++keyHistoryNext >= keyHistory.Count)
				keyHistoryNext = 0;
		}

		internal abstract char VKtoChar(int vk, IntPtr keybdLayout);

		internal string VKtoKeyName(int vk, bool useFallback)
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

		//protected internal abstract void DeregisterKeyboardHook();

		//protected internal abstract void DeregisterMouseHook();

		protected internal abstract void DeregisterHooks();

		protected internal void FreeHookMem()
		{
			kvkm = System.Array.Empty<uint>();
			kscm = System.Array.Empty<uint>();
			hotkeyUp.Clear();
			kvk = System.Array.Empty<KeyType>();
			ksc = System.Array.Empty<KeyType>();
		}

		//Need to figure out how to put the keyboard hook into a thread, then assign the thread id to this, like AHK does. Also need to determine if it should be instance or static.//TODO
		protected internal uint HookThreadID() => hookThreadID;

		protected internal abstract void Start();
	}

	internal class KeyHistoryItem
	{
		internal double elapsedTime;
		internal char? eventType = '\0';

		// space=none, i=ignored, s=suppressed, h=hotkey, etc.
		internal bool keyUp;

		internal int sc;

		// Time since prior key or mouse button, in seconds.
		// It seems better to store the foreground window's title rather than its HWND since keystrokes
		// might result in a window closing (being destroyed), in which case the displayed key history
		// would not be able to display the title at the time the history is displayed, which would
		// be undesirable.
		// To save mem, could point this into a shared buffer instead, but if that buffer were to run
		// out of space (perhaps due to the target window changing frequently), window logging would
		// no longer be possible without adding complexity to the logging function.  Seems best
		// to keep it simple:
		internal string targetWindow = "";

		internal int vk;
	}

	internal class KeysharpMsg
	{
		internal bool completed;
		internal GenericFunction func;
		internal IntPtr hwnd = IntPtr.Zero;
		internal IntPtr lParam = IntPtr.Zero;
		internal uint message;
		internal object obj;
		internal System.Drawing.Point pt;
		internal uint time;
		internal IntPtr wParam = IntPtr.Zero;
	}
}