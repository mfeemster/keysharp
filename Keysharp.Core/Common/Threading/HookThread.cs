using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
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

		protected internal System.Threading.Tasks.Task<System.Threading.Tasks.Task> channelReadThread;

		protected internal uint channelThreadID;

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
		protected internal List<uint> hotkeyUp = new List<uint>(256);
		protected internal IntPtr kbdHook = IntPtr.Zero;
		protected internal KeyHistory keyHistory = new KeyHistory();
		protected internal KeyType[] ksc;
		protected internal uint[] kscm;
		protected internal KeyType[] kvk;
		protected internal uint[] kvkm;
		protected internal IntPtr mouseHook = IntPtr.Zero;
		protected internal bool undisguisedMenuInEffect;
		protected volatile bool running;

		internal HookThread()
		{
		}

		public abstract void SimulateKeyPress(uint key);

		internal abstract void AddRemoveHooks(HookType hooksToBeActive, bool changeIsTemporary = false);

		internal abstract void ChangeHookState(List<HotkeyDefinition> hk, HookType whichHook, HookType whichHookAlways);

		internal abstract int CharToVKAndModifiers(char ch, ref int? modifiersLR, IntPtr keybdLayout, bool enableAZFallback = false);

		internal abstract int ConvertMouseButton(string buf, bool allowWheel = true);

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

		internal abstract bool IsKeyDown(int vk);

		internal abstract bool IsKeyDownAsync(int vk);

		internal abstract bool IsKeyToggledOn(int vk);

		internal abstract bool IsMouseVK(int vk);

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

		internal abstract bool IsWheelVK(int vk);

		internal abstract int KeyToModifiersLR(int vk, int sc, ref bool? isNeutral);

		internal ref uint Kscm(int i, int j) => ref kscm[(i * SC_ARRAY_COUNT) + j];

		internal ref uint Kvkm(int i, int j) => ref kvkm[(i * VK_ARRAY_COUNT) + j];

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

		internal abstract void ParseClickOptions(string options, ref int x, ref int y, ref int vk, ref KeyEventTypes eventType, ref long repeatCount, ref bool moveOffset);

		internal bool PostMessage(KeysharpMsg msg)
		=> IsReadThreadRunning()&& channel.Writer.TryWrite(msg);

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

		internal abstract int TextToSC(string text, ref bool? specifiedByNumber);

		internal abstract int TextToSpecial(string text, ref KeyEventTypes eventType, ref int modifiersLR, bool updatePersistent);

		internal abstract int TextToVK(string text, ref int? modifiersLR, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout);

		internal abstract bool TextToVKandSC(string text, ref int vk, ref int sc, ref int? modifiersLR, IntPtr keybdLayout);

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

	internal class HotstringMsg
	{
		internal CaseConformModes caseMode;
		internal char endChar;
		internal HotstringDefinition hs;
	}

	internal class KeysharpMsg
	{
		internal bool completed;
		internal IntPtr hwnd = IntPtr.Zero;
		internal IntPtr lParam = IntPtr.Zero;
		internal uint message;
		internal object obj;
		internal System.Drawing.Point pt;
		internal uint time;
		internal IntPtr wParam = IntPtr.Zero;
	}
}