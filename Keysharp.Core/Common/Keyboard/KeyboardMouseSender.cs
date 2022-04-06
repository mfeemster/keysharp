using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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

	public class HotstringEventArgs : EventArgs
	{
		public string Sequence { get; }

		public HotstringEventArgs() : this(string.Empty)
		{
		}

		public HotstringEventArgs(string sequence) => Sequence = sequence;
	}

	public class KeyEventArgs : EventArgs
	{
		/// <summary>
		/// Should this Key be blocked from the system
		/// </summary>
		public bool Block { get; set; } = false;

		public bool Down { get; private set; }

		/// <summary>
		/// Has this Key already processed enought
		/// </summary>
		public bool Handled { get; set; } = false;

		public Keys Keys { get; }

		public string Typed { get; private set; }

		public KeyEventArgs(Keys keys, string typed, bool down)
		{
			Keys = keys;
			Typed = typed;
			Down = down;
		}
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

		// Bit-field offsets:
		//internal const int CoordModePixel = 0;
		//internal const int CoordModeScreen = 2;
		//internal const int CoordModeTooltip = 4;
		//internal const int CoordModeWindow = 1;
		internal const int CoordUnspecified = int.MinValue;

		internal const short CoordUnspecifiedShort = short.MinValue;
		internal const int HookFail = 0xFF;
		internal const int HookKeyboard = 0x01;
		internal const int HookMouse = 0x02;

		// This is used to generate an Alt key-up event for the purpose of changing system state, but having the hook
		// block it from the active window to avoid unwanted side-effects:
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
		internal const int SendLevelMax = 100;
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

		//internal SendModes sendMode = SendModes.Input;
		private const int retention = 1024;

		private readonly StringBuilder caser = new StringBuilder(32);
		private readonly List<HotstringDefinition> expand = new List<HotstringDefinition>();
		private readonly StringBuilder history;
		private readonly List<HotkeyDefinition> hotkeys;
		private readonly List<HotstringDefinition> hotstrings;
		private readonly Dictionary<Keys, bool> pressed;

		public bool Block { get; set; }

		public string CurrentHotkey { get; set; }

		public int CurrentHotkeyTime { get; set; }

		public string CurrentHotstring { get; set; }

		public int CurrentHotstringTime { get; set; }

		public string PriorHotkey { get; set; }

		public int PriorHotkeyTime { get; set; }

		public string PriorHotstring { get; set; }

		public int PriorHotstringTime { get; set; }

		public KeyboardMouseSender()
		{
			hotkeys = new List<HotkeyDefinition>();
			hotstrings = new List<HotstringDefinition>();
			history = new StringBuilder(retention);
			pressed = new Dictionary<Keys, bool>();

			foreach (int i in Enum.GetValues(typeof(Keys)))
				pressed.TryAdd((Keys)i, false);

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

		public void SendMixed(string sequence)
		{
			//Using virual keys doesn't work, need real scan codes.
			//However, we must also be able to recognize things like {ctrl} from the stream.
			//For now, use this for testing and fix the rest later.//MATT
			Send(sequence);
			// TODO: modifiers in mixed mode send e.g. ^{a down}
			//var keys = KeyParser.ParseKeyStream(sequence);
			//foreach (var key in keys)
			//  if (key != Keys.None)
			//      Send(key);
		}

		internal static bool HotInputLevelAllowsFiring(int inputLevel, uint aEventExtraInfo, ref char? aKeyHistoryChar)
		{
			if (InputLevelFromInfo(aEventExtraInfo) <= inputLevel)
			{
				if (aKeyHistoryChar != null)
					aKeyHistoryChar = 'i'; // Mark as ignored in KeyHistory

				return false;
			}

			return true;
		}

		internal static uint InputLevelFromInfo(uint aExtraInfo)
		{
			if (aExtraInfo >= KeyIgnoreMin() && aExtraInfo <= KeyIgnoreMax)
				return KeyIgnoreLevel(0) - aExtraInfo;

			return SendLevelMax + 1;
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

		internal static bool SendLevelIsValid(int level) => level >= 0 && level <= SendLevelMax;

		internal HotkeyDefinition Add(HotkeyDefinition hotkey)
		{
			hotkeys.Add(hotkey);
			return hotkey;
		}

		internal HotstringDefinition Add(HotstringDefinition hotstring)
		{
			hotstrings.Add(hotstring);//What about dupes?//MATT
			return hotstring;
		}

		internal abstract void CleanupEventArray(int aFinalKeyDelay);

		internal abstract int ConvertModifiers(int aModifiers);

		internal abstract int ConvertModifiersLR(int aModifiersLR);

		internal abstract void DoMouseDelay();

		internal abstract IntPtr GetFocusedKeybdLayout(IntPtr aWindow);

		internal abstract int GetModifierLRState(bool aExplicitlyGet = false);

		internal abstract void InitEventArray(int maxEvents, int aModifiersLR);

		internal abstract void MouseClick(int aVK, int aX, int aY, int aRepeatCount, int aSpeed, KeyEventTypes aEventType, bool aMoveOffset);

		internal abstract void MouseEvent(uint aEventFlags, uint aData, int aX = CoordUnspecified, int aY = CoordUnspecified);

		internal abstract void MouseMove(ref int aX, ref int aY, ref uint aEventFlags, int aSpeed, bool aMoveOffset);

		internal void Remove(HotkeyDefinition hotkey)
		{
			_ = hotkeys.Remove(hotkey);
		}

		internal void Remove(HotstringDefinition hotstring)
		{
			_ = hotstrings.Remove(hotstring);
		}

		internal abstract void SendEventArray(ref int aFinalKeyDelay, int aModsDuringSend);

		internal abstract void SendKey(int aVK, int aSC, int aModifiersLR, int aModifiersLRPersistent
									   , int aRepeatCount, KeyEventTypes aEventType, int aKeyAsModifiersLR, IntPtr aTargetWindow
									   , int aX = CoordUnspecified, int aY = CoordUnspecified, bool aMoveOffset = false);

		internal abstract void SendKeyEventMenuMask(KeyEventTypes aEventType, uint aExtraInfo = KeyIgnoreAllExceptModifier);

		internal abstract void SendKeys(string aKeys, SendRawModes aSendRaw, SendModes aSendModeOrig, IntPtr aTargetWindow);

		/*  AHK defined these, but char in C# is 2 bytes, so using byte instead.
		    typedef USHORT sc_type; // Scan code.
		    typedef UCHAR vk_type;  // Virtual key.
		    typedef UINT mod_type;  // Standard Windows modifier type for storing MOD_CONTROL, MOD_WIN, MOD_ALT, MOD_SHIFT.
		*/

		protected internal abstract void LongOperationUpdate();

		protected internal abstract void LongOperationUpdateForSendKeys();

		protected internal abstract void Send(string keys);

		protected internal abstract void Send(Keys key);

		protected internal abstract void SendKeyEvent(KeyEventTypes aEventType, int aVK, int aSC = 0, IntPtr aTargetWindow = default, bool aDoKeyDelay = false, uint aExtraInfo = KeyIgnoreAllExceptModifier);

		protected abstract void Backspace(int n);

		//[Obsolete]//MATT
		//protected bool KeyReceived(Keys key, bool down)
		//{
		//  return KeyReceived(key, Letter(key).ToString(), down);
		//}

		//This function was from the IronAHK attempt to implement this hotkeys/strings. I am unsure if we're going to go with this style, or the direct port of the AHK code.
		//I lean more toward the latter because while being incredible verbose, we would have the comfort of knowing it was an exact duplication of the original functionality.//TODO
		/*
		    protected bool KeyReceived(Keys key, string typed, bool down)
		    {
		    if (Block)
		        return true;

		    var block = false;
		    var args = new KeyEventArgs(key, typed, down);
		    KeyEvent?.Invoke(this, args);

		    if (args.Block)
		    {
		        block = true;
		    }

		    if (args.Handled)
		    {
		        return block;
		    }

		    if (!Flow.Suspended)
		    {
		        pressed[key] = down;
		        var exec = new List<HotkeyDefinition>();

		        foreach (var hotkey in hotkeys)
		        {
		            var match = KeyMatch(hotkey.Keys & ~Keys.Modifiers, key) ||
		                        hotkey.Typed.Length != 0 && hotkey.Typed.Equals(typed, StringComparison.CurrentCultureIgnoreCase);
		            var up = hotkey.EnabledOptions.HasFlag(HotkeyDefinition.Options.Up);

		            if (hotkey.Enabled && match && HasModifiers(hotkey) && up != down)
		            {
		                exec.Add(hotkey);

		                if (!hotkey.EnabledOptions.HasFlag(HotkeyDefinition.Options.PassThrough))
		                    block = true;
		            }
		        }

		        if (exec.Any())//Most of the time, a hotkey will not be matched, so skip creating the thread to be more efficient.
		        {
		            new Thread(delegate ()
		            {
		                foreach (var hotkey in exec)
		                {
		                    PriorHotkeyTime = CurrentHotkeyTime;
		                    CurrentHotkeyTime = Environment.TickCount;
		                    PriorHotkey = CurrentHotkey;
		                    CurrentHotkey = hotkey.ToString();

		                    if (hotkey.Condition())
		                        _ = hotkey.Proc.Call(new object[] { });
		                }
		            }).Start();
		        }
		    }

		    if (!down)
		        return block;

		    if (hotstrings.Count > 0)
		    {
		        if (key == Keys.Back && history.Length > 0)
		        {
		            _ = history.Remove(history.Length - 1, 1);
		            return block;
		        }

		        switch (key)
		        {
		            case Keys.Left:
		            case Keys.Right:
		            case Keys.Down:
		            case Keys.Up:
		            case Keys.Next:
		            case Keys.Prior:
		            case Keys.Home:
		            case Keys.End:
		                history.Length = 0;
		                break;

		            //Original would break when these were pressed, but would not check on subsequent keys whether these were still pressed.
		            //This caused the bug of ignoring the hotstring when a user selected all with Ctrl+A then started typing.
		            //case Keys.Alt:
		            //case Keys.LMenu:
		            //case Keys.RMenu:
		            //case Keys.LControlKey:
		            //case Keys.RControlKey:
		            //case Keys.LShiftKey:
		            //case Keys.RShiftKey:
		            //  break;

		            default:
		                if (!pressed[Keys.Alt]
		                        && !pressed[Keys.LMenu]
		                        && !pressed[Keys.RMenu]
		                        && !pressed[Keys.Menu]
		                        && !pressed[Keys.LControlKey]
		                        && !pressed[Keys.RControlKey]
		                        && !pressed[Keys.Control]
		                        && !pressed[Keys.LWin]
		                        && !pressed[Keys.RWin]
		                        //&& !pressed[Keys.LShiftKey]
		                        //&& !pressed[Keys.RShiftKey]
		                   )
		                {
		                    var d = retention - history.Length;//Should take shift into acct here and insert CAPS letter, but not the shift.//MATT

		                    if (d < 0)
		                        _ = history.Remove(history.Length + d, -d);

		                    _ = history.Append(typed);
		                }

		                //else
		                //{
		                //  Console.WriteLine($"Modifier was pressed while also pressing {key} so not adding to history");
		                //}
		                break;
		        }
		    }

		    if (Flow.Suspended)
		        return block;

		    //var expand = new List<HotstringDefinition>();//Make a class variable instead.//MATT
		    expand.Clear();//Reuse for efficiency.
		    var dt = DateTime.Now;
		    var endstr = string.Empty;

		    foreach (var hotstring in hotstrings)//Seems very inneficient, maybe use a dkt?//MATT
		    {
		        if (hotstring.Enabled && HasConditions(hotstring, out endstr))
		        {
		            expand.Add(hotstring);
		            //if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.Reset))//Do it below.//MATT
		            //history.Length = 0;
		            break;//Why would we ever want more than one hotstring?//MATT
		        }
		    }

		    var dt2 = DateTime.Now;
		    //Console.WriteLine($"Finding a hotstring took {(dt2 - dt).TotalMilliseconds} ms");
		    var trigger = history.Length > 0 ? history[history.Length - 1].ToString() : null;

		    foreach (var hotstring in expand)
		    {
		        block = true;
		        //Putting this in a thread seems to be a hacky way of solving a particular problem.
		        //If a hotstring is a function style hotstring, and that function shows a message box, and the trigger is a space,
		        //the message box will be shown, but that space will be sent to it, and the space acts like a click on the ok button
		        //and dismisses the message box right after it is shown.
		        //As an alternative, we're trying to just launch all GUI components in their own thread using Task.Factory.StartNew().
		        //new Thread(delegate ()//Need to know why this was in a thread. When would it ever make sense to do that? Should check the original.//MATT
		        {
		            PriorHotstringTime = CurrentHotstringTime;
		            CurrentHotstringTime = Environment.TickCount;
		            PriorHotstring = CurrentHotstring;
		            CurrentHotstring = hotstring.ToString();
		            var length = hotstring.Sequence.Length;
		            var auto = hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.AutoTrigger);
		            var notautoandtrigger = !auto&& trigger != null;
		            //Console.WriteLine($"bef processing hotstring: \"{history}\"");

		            //This code doesn't even have access to the output string, but it sorely needs it. Fix it.//MATT
		            if (auto)
		                length--;

		            if (length > 0)//Roughly try to mimic the code from CollectHotstring() in AHK. Need to be testing the replacement str, not the sequence str.//MATT
		            {
		                if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.Backspace))
		                {
		                    Backspace(length);

		                    // UNDONE: hook on Windows captures triggering key and blocks it, but X11 allows it through and needs an extra backspace
		                    if (!auto&& Environment.OSVersion.Platform != PlatformID.Win32NT)
		                        Backspace(1);
		                }

		                _ = history.Clear();

		                if (notautoandtrigger)
		                {
		                    _ = history.Append(trigger);
		                }

		                //var n = length + 1;
		                //_ = history.Remove(history.Length - n, n);
		            }
		            else if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.Backspace))
		            {
		            }

		            if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.Reset))
		            {
		                _ = history.Clear();
		            }

		            //Need a way to put the space back into the history here for most options.//TODO
		            //hotstring.Proc();//MATT new object[] { });
		            //hotstring.Proc(new object[] { endstr, ApplyCase(endstr, hotstring.Replacement) });//Casing works, but we'll need another mode to support it, such as C2,3,4 etc...//MATT
		            hotstring.Proc(new object[] { endstr, hotstring.Replacement });//MATT new object[] { });

		            //Need to support the 'Z' replace option here, which just clears the history.

		            if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.OmitEnding))
		            {
		                if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.Backspace) &&
		                        !hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.AutoTrigger))
		                {
		                    //_ = history.Remove(history.Length - 1, 1);//AHK doesn't seem to care about this.//MATT
		                    //Backspace(1);
		                }
		            }
		            else if (notautoandtrigger && hotstring.Replacement != string.Empty)
		                SendMixed(trigger);

		            //Console.WriteLine($"aft processing hotstring: \"{history}\"");
		        }//).Start();
		    }

		    return block;
		    }
		*/

		protected abstract void RegisterHook();

		/*  private bool HasConditions(HotstringDefinition hotstring, out string endstr)
		    {
		    var histcopy = history.ToString();//Why do we want a copy for *every* hotstring in *every* keypress? Seems terribly inefficient.//MATT
		    endstr = string.Empty;

		    if (histcopy.Length == 0)
		        return false;

		    var compare = hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.CaseSensitive) ?
		                  StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
		    var x = histcopy.Length - hotstring.Sequence.Length - 1;

		    if (hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.AutoTrigger))
		    {
		        if (!histcopy.EndsWith(hotstring.Sequence, compare))
		            return false;

		        endstr = histcopy.Substring(x + 1, hotstring.Sequence.Length);
		    }
		    else
		    {
		        if (histcopy.Length < hotstring.Sequence.Length + 1)
		            return false;

		        if (hotstring.EndChars.IndexOf(histcopy[histcopy.Length - 1]) == -1)
		            return false;

		        endstr = histcopy.Substring(x--, hotstring.Sequence.Length);//Might be able to use EndsWith to avoid a copy.//MATT

		        if (!endstr.Equals(hotstring.Sequence, compare))
		            return false;
		    }

		    if (!hotstring.EnabledOptions.HasFlag(HotstringDefinition.Options.Nested))
		        if (x > -1)
		        {
		            //Console.WriteLine($"{endstr} -{histcopy[x]}-");
		            if (char.IsLetterOrDigit(histcopy[x]))
		                return false;
		        }

		    return true;
		    }*/

		private bool HasModifiers(HotkeyDefinition hotkey)
		{
			if (hotkey.Extra != Keys.None && !pressed[hotkey.Extra])
				return false;

			if (hotkey.EnabledOptions.HasFlag(HotkeyDefinition.Options.IgnoreModifiers))
				return true;

			bool[,] modifiers =
			{
				{ (hotkey.Keys & Keys.Alt) == Keys.Alt, pressed[Keys.Alt] || pressed[Keys.LMenu] || pressed[Keys.RMenu], (hotkey.Keys & Keys.LMenu) == Keys.LMenu },
				{ (hotkey.Keys & Keys.Control) == Keys.Control, pressed[Keys.Control] || pressed[Keys.LControlKey] || pressed[Keys.RControlKey], (hotkey.Keys & Keys.ControlKey) == Keys.ControlKey },
				{ (hotkey.Keys & Keys.Shift) == Keys.Shift, pressed[Keys.Shift] || pressed[Keys.LShiftKey] || pressed[Keys.RShiftKey], (hotkey.Keys & Keys.ShiftKey) == Keys.ShiftKey }
			};

			for (var i = 0; i < 3; i++)
				if ((modifiers[i, 0] && !modifiers[i, 1]) || (modifiers[i, 1] && !modifiers[i, 0] && !modifiers[i, 2]))
					return false;

			return true;
		}

		private bool KeyMatch(Keys expected, Keys received)
		{
			expected &= ~Keys.Modifiers;
			received &= ~Keys.Modifiers;

			if (expected == received)
				return true;

			switch (expected)
			{
				case Keys.ControlKey:
					return received == Keys.LControlKey || received == Keys.RControlKey;

				case Keys.ShiftKey:
					return received == Keys.LShiftKey || received == Keys.RShiftKey;
					//Shouldn't alt be here, and maybe lwin and rwin?//MATT
			}

			return false;
		}

		private char Letter(Keys key)
		{
			// HACK: remove Keys translation (and the overload) since it should be passed from the native handler
			var caps = (key & Keys.Shift) == Keys.Shift || pressed[Keys.ShiftKey] || pressed[Keys.LShiftKey] || pressed[Keys.RShiftKey];
			key &= ~Keys.Modifiers;

			switch (key)
			{
				case Keys.Space: return ' ';

				case Keys.Enter: return '\n';
			}

			var letter = key.ToString();

			if (!caps)
				letter = letter.ToLower();

			return letter.Length == 1 ? letter[0] : (char)0;
		}

		/// <summary>
		/// Raised when a Key is pressed/released
		/// </summary>
		public event EventHandler<KeyEventArgs> KeyEvent;
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