using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Keysharp.Core.Common.Joystick;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;//Code in Core probably shouldn't be referencing windows specific code.//MATT

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
		private static Core.GenericFunction keyCondition;
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
		public static void BlockInput(params object[] obj)//This is completely wrong, need to make compat with V2//TODO
		{
			var mode = obj.L().S1();
			var toggle = ConvertBlockInput(mode);

			switch (toggle)
			{
				case ToggleValueType.On:
					ScriptBlockInput(true);
					break;

				case ToggleValueType.Off:
					ScriptBlockInput(false);
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

		public static object CaretGetPos(params object[] obj)//Need to eventually figure out ref vars and making this cross platforom.//TODO
		{
			// I believe only the foreground window can have a caret position due to relationship with focused control.
			var targetWindow = WindowsAPI.GetForegroundWindow(); // Variable must be named targetwindow for ATTACH_THREAD_INPUT.

			if (targetWindow == IntPtr.Zero) // No window is in the foreground, report blank coordinate.
				return "";

			var h = WindowsAPI.GetWindowThreadProcessId(targetWindow, out var _);
			var info = GUITHREADINFO.Default;//Must be initialized this way because the size field must be populated.
			var result = WindowsAPI.GetGUIThreadInfo(h, out info) && info.hwndCaret != IntPtr.Zero;

			if (!result)
				return "";

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
			return new Rectangle(pt.X, pt.Y, 0, 0);
		}

		public static string GetKeyName(params object[] obj) => GetKeyNamePrivate(obj.L().S1(), 0) as string;

		public static long GetKeySC(params object[] obj) => (long)GetKeyNamePrivate(obj.L().S1(), 1);

		/// <summary>
		/// Unlike the GetKeyState command -- which returns D for down and U for up -- this function returns (1) if the key is down and (0) if it is up.
		/// If <paramref name="KeyName"/> is invalid, an empty string is returned.
		/// </summary>
		/// <param name="KeyName">Use autohotkey definition or virtual key starting from "VK"</param>
		/// <param name="Mode"></param>
		public static object GetKeyState(params object[] obj)
		{
			var (keyname, mode) = obj.L().S2();
			var ht = Keysharp.Scripting.Script.HookThread;
			Keysharp.Core.Common.Joystick.JoyControls joy;
			int? joystickid = 0;
			int? dummy = null;
			var vk = ht.TextToVK(keyname, ref dummy, false, true, WindowsAPI.GetKeyboardLayout(0));

			if (vk == 0)
			{
				if ((joy = (JoyControls)Joystick.ConvertJoy(keyname, ref joystickid)) == 0)
					throw new ValueError("Invalid value.");//It is neither a key name nor a joystick button/axis.

				return Joystick.ScriptGetJoyState(joy, joystickid.Value);
			}

			// Since above didn't return: There is a virtual key (not a joystick control).
			KeyStateTypes keystatetype;

			switch (char.ToUpper(mode[0])) // Second parameter.
			{
				case 'T': keystatetype = KeyStateTypes.Toggle; break; // Whether a toggleable key such as CapsLock is currently turned on.

				case 'P': keystatetype = KeyStateTypes.Physical; break; // Physical state of key.

				default: keystatetype = KeyStateTypes.Logical; break;
			}

			return ScriptGetKeyState(vk, keystatetype); // 1 for down and 0 for up.
		}

		public static long GetKeyVK(params object[] obj) => (long)GetKeyNamePrivate(obj.L().S1(), 2);

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
		/// <item><term>UseAccessors.A_ErrorLevel</term>: <description>skips the warning dialog and sets <see cref="Accessors.A_ErrorLevel"/> if there was a problem.</description></item>
		/// <item><term>On</term>: <description>the hotkey becomes enabled.</description></item>
		/// <item><term>Off</term>: <description>the hotkey becomes disabled.</description></item>
		/// <item><term>Toggle</term>: <description>the hotkey is set to the opposite state (enabled or disabled).</description></item>
		/// </list>
		/// </param>
		public static void Hotkey(params object[] obj)
		{
			var (keyname, label, options) = obj.L().S3();
			var win = -1;
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			switch (keyname.ToLowerInvariant())
			{
				case Core.Keyword_IfWinActive: win = 0; break;

				case Core.Keyword_IfWinExist: win = 1; break;

				case Core.Keyword_IfWinNotActive: win = 2; break;

				case Core.Keyword_IfWinNotExit: win = 3; break;
			}

			if (win != -1)
			{
				var cond = new string[4, 2];
				cond[win, 0] = label; // title
				cond[win, 1] = options; // text
				keyCondition = new Core.GenericFunction(delegate
				{
					return HotkeyPrecondition(cond);
				});
				return;
			}

			bool? enabled = true;

			foreach (var option in Options.ParseOptions(options))
			{
				switch (option.ToLowerInvariant())
				{
					case Core.Keyword_On: enabled = true; break;

					case Core.Keyword_Off: enabled = false; break;

					case Core.Keyword_Toggle: enabled = null; break;

					case Core.Keyword_UseErrorLevel: break;

					default:
						switch (option[0])
						{
							case 'B':
							case 'b':
							case 'P':
							case 'p':
							case 'T':
							case 't':
								break;

							default:
								Accessors.A_ErrorLevel = 10;
								break;
						}

						break;
				}
			}

			HotkeyDefinition key;

			try
			{
				key = HotkeyDefinition.Parse(keyname);
			}
			catch (ArgumentException)
			{
				Accessors.A_ErrorLevel = 2;
				return;
			}

			var id = keyname;
			key.Name = id;

			if (keyCondition != null)
				id += "_" + keyCondition.GetHashCode().ToString("X");

			if (hotkeys.ContainsKey(id))
			{
				hotkeys[id].Enabled = enabled == null ? !hotkeys[id].Enabled : enabled == true;

				switch (label.ToLowerInvariant())
				{
					case Core.Keyword_On: hotkeys[id].Enabled = true; break;

					case Core.Keyword_Off: hotkeys[id].Enabled = true; break;

					case Core.Keyword_Toggle: hotkeys[id].Enabled = !hotkeys[id].Enabled; break;
				}
			}
			else
			{
				var method = Reflections.FindLocalMethod(label);

				if (method == null)
				{
					Accessors.A_ErrorLevel = 1;
					return;
				}

				key.Proc = (Core.GenericFunction)Delegate.CreateDelegate(typeof(Core.GenericFunction), method);
				key.Precondition = keyCondition;
				hotkeys.Add(id, key);
				_ = kbdMouseSender.Add(key);
			}
		}

		public static object Hotstring(params object[] obj)
		{
			var (name, replacement, onoff) = obj.L().S1O2("", null, null);
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;
			var xOption = false;
			var action = replacement as string;

			if (string.Compare(name, "EndChars", true) == 0) // Equivalent to #Hotstring EndChars <action>
			{
				var old = HotstringDefinition.defEndChars;

				if (replacement is string s && s.Length > 0)
					HotstringDefinition.defEndChars = s;

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
				HotstringDefinition.hsBuf.Clear();
				return null;
			}
			else if (replacement == null && onoff == null && name.Length > 0 && name[0] != ':') //Check if only one param was passed. Equivalent to #Hotstring <name>.
			{
				// TODO: Build string of current options and return it?
				HotstringDefinition.ParseOptions(name, ref Keysharp.Scripting.Script.hsPriority, ref Keysharp.Scripting.Script.hsKeyDelay, ref Keysharp.Scripting.Script.hsSendMode, ref Keysharp.Scripting.Script.hsCaseSensitive
												 , ref Keysharp.Scripting.Script.hsConformToCase, ref Keysharp.Scripting.Script.hsDoBackspace, ref Keysharp.Scripting.Script.hsOmitEndChar, ref Keysharp.Scripting.Script.hsSendRaw, ref Keysharp.Scripting.Script.hsEndCharRequired
												 , ref Keysharp.Scripting.Script.hsDetectWhenInsideWord, ref Keysharp.Scripting.Script.hsDoReset, ref xOption, ref Keysharp.Scripting.Parser.SuspendExemptHS);
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
						hotstringStart = hotstringOptions.Substring(1); // Points to the hotstring itself.
				}
				else // Double-colon, so it's a hotstring if there's more after this (but this means no options are present).
					if (name.Length > 2)
						hotstringStart = name.Substring(2);

				//else it's just a naked "::", which is invalid.
			}

			if (hotstringStart.Length == 0)
				throw new ValueError("Hotstring definition did not contain a hotstring.");

			// Determine options which affect hotstring identity/uniqueness.
			var caseSensitive = Keysharp.Scripting.Script.hsCaseSensitive;
			var detectInsideWord = Keysharp.Scripting.Script.hsDetectWhenInsideWord;
			var un = false; var iun = 0; var sm = SendModes.Event; var sr = SendRawModes.NotRaw; // Unused.

			if (!string.IsNullOrEmpty(hotstringOptions))
				HotstringDefinition.ParseOptions(hotstringOptions, ref iun, ref iun, ref sm, ref caseSensitive, ref un, ref un, ref un, ref sr, ref un, ref detectInsideWord, ref un, ref xOption, ref un);

			//Core.HotFunction actionObj = null;
			IFuncObj ifunc = null;

			if (xOption)
			{
				if (!string.IsNullOrEmpty(action))//The string could be a replacement string, a function name, a function object, or an expression to execute.
				{
					if (Reflections.FindLocalMethod(action) is MethodInfo mi)
					{
						ifunc = new FuncObj(mi, null);
					}

					//if (method == null)
					//throw new ValueError($"Could not find hotstring method {action}().");
					//proc = (Core.GenericFunction)Delegate.CreateDelegate(typeof(Core.GenericFunction), method);
					//actionObj = (Core.HotFunction)Delegate.CreateDelegate(typeof(Core.HotFunction), method);//Will need to ultimately determine if we should use GenericFunction or HotFunction.
					// Otherwise, it's always replacement text (the 'X' option is ignored at runtime).
				}
				else if (replacement is IFuncObj fo)
					ifunc = fo;
			}

			var toggle = ToggleValueType.Neutral;

			if (onoff != null && (toggle = Keysharp.Core.Options.ConvertOnOffToggle(onoff)) == ToggleValueType.Invalid)
			{
				throw new ValueError($"Invalid value of {onoff} for parameter 3.");
			}

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
					{
						if (!string.IsNullOrEmpty(existing.replacement) && string.Compare(rep, existing.replacement) == 0)
						{
							// Caller explicitly passed the same string it already had, which might be common,
							// such as if a single Hotstring() call site is used to both create and update.
							newReplacement = existing.replacement; // Avoid reallocating it.
						}
						else
							newReplacement = rep;
					}

					existing.suspended |= HotstringDefinition.HS_TEMPORARILY_DISABLED;
					ht.WaitHookIdle();

					// At this point it is certain the hook thread is not in the middle of reading this
					// hotstring's other properties, such as mReplacement (which we may be about to free).
					if (newReplacement != existing.replacement)
						existing.replacement = newReplacement;

					if (ifunc != existing.funcObj)
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
					throw new ValueError("Nonexistent hotstring.");

				var initialSuspendState = (toggle == ToggleValueType.Off) ? HotstringDefinition.HS_TURNED_OFF : 0;

				if (Accessors.A_IsSuspended)
					initialSuspendState |= HotstringDefinition.HS_SUSPENDED;

				if (HotstringDefinition.AddHotstring(name, ifunc, hotstringOptions, hotstringStart, action, false, initialSuspendState) == ResultType.Fail)
					return 0;

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

		public static void HotstringFunc(params object[] obj)//Matt, probably don't need this.//TODO
		{
			var (sequence, replacement, options) = obj.L().S3();
			Core.GenericFunction proc;

			//Core.HotFunction proc;//MATT
			try
			{
				var method = Reflections.FindLocalMethod(replacement);

				if (method == null)
					throw new ArgumentNullException();

				proc = (Core.GenericFunction)Delegate.CreateDelegate(typeof(Core.GenericFunction), method);
				//proc = (Core.HotFunction)Delegate.CreateDelegate(typeof(Core.HotFunction), method);//MATT
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Accessors.A_ErrorLevel = 1;
				throw new ArgumentException();
			}

			//Figure out how to integrate, or remove this.//TODO
			//var opts = HotstringDefinition.ParseOptions(options);//Need to figure out how to call new constructor (and get rid of the old one)//TODO
			//var key = new HotstringDefinition(sequence, replacement) { Name = sequence, Enabled = true, EnabledOptions = opts };
			////hotstrings.Add(Sequence, key);//MATT
			//hotstrings[sequence] = key;
			//_ = keyboardHook.Add(key);
		}
		/// <summary>
		/// Creates a hotstring.
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="label"></param>
		/// <param name="options"></param>
		public static void HotstringLabel(params object[] obj)//Matt
		{
			var (sequence, label, options) = obj.L().S3();
			//Core.GenericFunction proc;
			Core.HotFunction proc;//MATT

			try
			{
				var method = Reflections.FindLocalMethod(label);

				if (method == null)
					throw new ArgumentNullException();

				//proc = (Core.GenericFunction)Delegate.CreateDelegate(typeof(Core.GenericFunction), method);
				proc = (Core.HotFunction)Delegate.CreateDelegate(typeof(Core.HotFunction), method);//MATT
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);//Probably shouldn't even be a try/catch. Let the parent handle it.//TODO
				Accessors.A_ErrorLevel = 1;
				throw new ArgumentException();
			}

			//Figure out how to integrate, or remove this.//TODO
			//var opts = HotstringDefinition.ParseOptions(options);//Need to figure out how to call new constructor (and get rid of the old one)//TODO
			//var key = new HotstringDefinition(sequence, string.Empty, proc) { Name = sequence, Enabled = true, EnabledOptions = opts };
			////hotstrings.Add(Sequence, key);//MATT
			//hotstrings[sequence] = key;
			//_ = keyboardHook.Add(key);
		}
		/// <summary>
		/// Waits for the user to type a string (not supported on Windows 9x: it does nothing).
		/// </summary>
		/// <param name="result">
		/// <para>The name of the variable in which to store the text entered by the user (by default, artificial input is also captured).</para>
		/// <para>If this and the other parameters are omitted, any Input in progress in another thread is terminated and its Accessors.A_ErrorLevel is set to the word NewInput. By contrast, the Accessors.A_ErrorLevel of the current command will be set to 0 if it terminated a prior Input, or 1 if there was no prior Input to terminate.</para>
		/// <para>OutputVar does not store keystrokes per se. Instead, it stores characters produced by keystrokes according to the active window's keyboard layout/language. Consequently, keystrokes that do not produce characters (such as PageUp and Escape) are not stored (though they can be recognized via the EndKeys parameter below).</para>
		/// <para>Whitespace characters such as TAB (`t) are stored literally. ENTER is stored as linefeed (`n).</para>
		/// </param>
		/// <param name="options">
		/// <para>A string of zero or more of the following letters (in any order, with optional spaces in between):</para>
		/// <para>B: Backspace is ignored. Normally, pressing backspace during an Input will remove the most recently pressed character from the end of the string. Note: If the input text is visible (such as in an editor) and the arrow keys or other means are used to navigate within it, backspace will still remove the last character rather than the one behind the caret (insertion point).</para>
		/// <para>C: Case sensitive. Normally, MatchList is not case sensitive (in versions prior to 1.0.43.03, only the letters A-Z are recognized as having varying case, not letters like ü/Ü).</para>
		/// <para>I: Ignore input generated by any AutoHotkey script, such as the SendEvent command. However, the SendInput and SendPlay methods are always ignored, regardless of this setting.</para>
		/// <para>L: Length limit (e.g. L5). The maximum allowed length of the input. When the text reaches this length, the Input will be terminated and Accessors.A_ErrorLevel will be set to the word Max unless the text matches one of the MatchList phrases, in which case Accessors.A_ErrorLevel is set to the word Match. If unspecified, the length limit is 16383, which is also the absolute maximum.</para>
		/// <para>M: Modified keystrokes such as Control-A through Control-Z are recognized and transcribed if they correspond to real ASCII characters. Consider this example, which recognizes Control-C:</para>
		/// <code>Transform, CtrlC, Chr, 3 ; Store the character for Ctrl-C in the CtrlC var.
		/// Input, OutputVar, L1 M
		/// if OutputVar = %CtrlC%
		/// MsgBox, You pressed Control-C.</code>
		/// <para>ExitAppNote: The characters Ctrl-A through Ctrl-Z correspond to Chr(1) through Chr(26). Also, the M option might cause some keyboard shortcuts such as Ctrl-LeftArrow to misbehave while an Input is in progress.</para>
		/// <para>T: Timeout (e.g. T3). The number of seconds to wait before terminating the Input and setting Accessors.A_ErrorLevel to the word Timeout. If the Input times out, OutputVar will be set to whatever text the user had time to enter. This value can be a floating point number such as 2.5.</para>
		/// <para>V: Visible. Normally, the user's input is blocked (hidden from the system). Use this option to have the user's keystrokes sent to the active window.</para>
		/// <para>*: Wildcard (find anywhere). Normally, what the user types must exactly match one of the MatchList phrases for a match to occur. Use this option to find a match more often by searching the entire length of the input text.</para>
		/// </param>
		/// <param name="endKeys">
		/// <para>A list of zero or more keys, any one of which terminates the Input when pressed (the EndKey itself is not written to OutputVar). When an Input is terminated this way, Accessors.A_ErrorLevel is set to the word EndKey followed by a colon and the name of the EndKey. Examples: <code>EndKey:.
		/// EndKey:Escape</code></para>
		/// <para>The EndKey list uses a format similar to the Send command. For example, specifying {Enter}.{Esc} would cause either ENTER, period (.), or ESCAPE to terminate the Input. To use the braces themselves as end keys, specify {{} and/or {}}.</para>
		/// <para>To use Control, Alt, or Shift as end-keys, specify the left and/or right version of the key, not the neutral version. For example, specify {LControl}{RControl} rather than {Control}.</para>
		/// <para>Although modified keys such as Control-C (^c) are not supported, certain keys that require the shift key to be held down -- namely punctuation marks such as ?!:@&amp;{} -- are supported in v1.0.14+.</para>
		/// <para>An explicit virtual key code such as {vkFF} may also be specified. This is useful in the rare case where a key has no name and produces no visible character when pressed. Its virtual key code can be determined by following the steps at the bottom fo the key list page.</para>
		/// </param>
		/// <param name="matchList">
		/// <para>A comma-separated list of key phrases, any of which will cause the Input to be terminated (in which case Accessors.A_ErrorLevel will be set to the word Match). The entirety of what the user types must exactly match one of the phrases for a match to occur (unless the * option is present). In addition, any spaces or tabs around the delimiting commas are significant, meaning that they are part of the match string. For example, if MatchList is "ABC , XYZ ", the user must type a space after ABC or before XYZ to cause a match.</para>
		/// <para>Two consecutive commas results in a single literal comma. For example, the following would produce a single literal comma at the end of string: "string1,,,string2". Similarly, the following list contains only a single item with a literal comma inside it: "single,,item".</para>
		/// <para>Because the items in MatchList are not treated as individual parameters, the list can be contained entirely within a variable. In fact, all or part of it must be contained in a variable if its length exceeds 16383 since that is the maximum length of any script line. For example, MatchList might consist of %List1%,%List2%,%List3% -- where each of the variables contains a large sub-list of match phrases.</para>
		/// </param>
		public static void Input(out string result, string options, string endKeys, string matchList)
		{
			result = null;
			var optsItems = new Dictionary<string, Regex>();
			var inputHandler = InputCommand.Instance;

			if (inputHandler.IsBusy)  // is there another Thread using this command already?
			{
				inputHandler.AbortCatching(); // force to stop it
			}

			inputHandler.Reset();
			optsItems.Add(Core.Keyword_LimitS, new Regex(Core.Keyword_LimitS + @"(\d*)"));
			optsItems.Add(Core.Keyword_TimeOutS, new Regex(Core.Keyword_TimeOutS + @"([\d|\.]*)"));
			optsItems.Add(Core.Keyword_BackSpaceS, new Regex("(" + Core.Keyword_BackSpaceS + ")"));
			optsItems.Add(Core.Keyword_IgnoreS, new Regex("(" + Core.Keyword_IgnoreS + ")"));
			optsItems.Add(Core.Keyword_ModifiedS, new Regex("(" + Core.Keyword_ModifiedS + ")"));
			optsItems.Add(Core.Keyword_VisibleS, new Regex("(" + Core.Keyword_VisibleS + ")"));
			optsItems.Add(Core.Keyword_FindAnyWhereS, new Regex("(\\" + Core.Keyword_FindAnyWhereS + ")"));
			var dicOptions = Options.ParseOptionsRegex(ref options, optsItems, true);

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_LimitS]))
			{
				try
				{
					var limit = int.Parse(dicOptions[Core.Keyword_LimitS]);
					inputHandler.KeyLimit = limit;
				}
				catch
				{
					inputHandler.KeyLimit = 0;
				}
			}

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_TimeOutS]))
			{
				try
				{
					var timeout = float.Parse(dicOptions[Core.Keyword_TimeOutS]);
					inputHandler.TimeOutVal = (int)(timeout * 1000);
				}
				catch
				{
					inputHandler.TimeOutVal = null;
				}
			}

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_BackSpaceS]))
			{
				inputHandler.IgnoreBackSpace = true;
			}

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_ModifiedS]))
			{
				inputHandler.RecognizeModifiedKeystrockes = true;
			}

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_IgnoreS]))
			{
				inputHandler.IgnoreIAGeneratedInput = true;
			}

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_VisibleS]))
			{
				inputHandler.Visible = true;
			}

			if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_FindAnyWhereS]))
			{
				inputHandler.FindAnyWhere = true;
			}

			// Parse EndKeys
			var endkeys = KeyParser.ParseKeyStream(endKeys);

			foreach (var key in endkeys)
				inputHandler.Endkeys.Add(key);

			// Parse MatchList
			foreach (var matchStr in matchList.Split(','))
				inputHandler.EndMatches.Add(matchStr);

			var abortReason = inputHandler.StartCatching();
			result = abortReason.CatchedText;
			Accessors.A_ErrorLevel = (int)abortReason.Reason;
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
		public static bool KeyWait(params object[] obj)
		{
			var (keyname, options) = obj.L().S2();
			bool waitIndefinitely;
			int sleepDuration;
			DateTime startTime;
			int vk; // For GetKeyState.
			IntPtr runningProcess; // For RUNWAIT
			uint exitCode; // For RUNWAIT
			// For FID_KeyWait:
			bool waitForKeyDown;
			KeyStateTypes keyStateType;
			var joy = JoyControls.Invalid;
			int? joystickId = 0;
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;
			int? modLR = null;

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
					System.Threading.Thread.Sleep(Keysharp.Scripting.Script.SLEEP_INTERVAL);
					//MsgSleep() might not even be needed if we use real threads.//TODO
					//if (Keysharp.Scripting.Script.MsgSleep(Keysharp.Scripting.Script.INTERVAL_UNSPECIFIED)) // INTERVAL_UNSPECIFIED performs better.
					//{
					//}
				}
				else // Done waiting.
					return false; // Since it timed out, we override the default with this.
			}

			/*
			    var keyWaitCommand = new KeyWaitCommand(keyboardHook);
			    var key = KeyParser.ParseKey(keyname);
			    var optsItems = new Dictionary<string, Regex>();
			    optsItems.Add(Core.Keyword_TimeOutS, new Regex(Core.Keyword_TimeOutS + @"([\d|\.]*)"));
			    optsItems.Add(Core.Keyword_DownS, new Regex("(" + Core.Keyword_DownS + ")"));
			    var dicOptions = Options.ParseOptionsRegex(ref options, optsItems, true);

			    if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_DownS]))
			    {
			    keyWaitCommand.TriggerOnKeyDown = true;
			    }

			    if (!string.IsNullOrEmpty(dicOptions[Core.Keyword_TimeOutS]))
			    {
			    try
			    {
			        var timeout = float.Parse(dicOptions[Core.Keyword_TimeOutS]);
			        keyWaitCommand.TimeOutVal = (int)(timeout * 1000);
			    }
			    catch
			    {
			        keyWaitCommand.TimeOutVal = null;
			    }
			    }

			    keyWaitCommand.Wait(key);*/
		}
		/// <summary>
		/// Sends simulated keystrokes and mouse clicks to the active window.
		/// </summary>
		/// <param name="Keys">The sequence of keys to send.</param>
		public static void Send(params object[] obj) => Keysharp.Scripting.Script.HookThread.kbdMsSender.SendMixed(obj.L().S1());
		public static void SendMode(params object[] obj) => Accessors.A_SendMode = obj.L().S1();
		public static void SetKeyDelay(params object[] obj)
		{
			var o = obj.L();
			var play = string.Empty;

			if (o.Count > 2 && o[2] is string s)//Need to convert this to our usual object parsing.//MATT
				play = s.ToLowerInvariant();

			var isPlay = play == "play";
			var del = isPlay ? Accessors.A_KeyDelayPlay : Accessors.A_KeyDelay;
			var dur = isPlay ? Accessors.A_KeyDurationPlay : Accessors.A_KeyDuration;

			if (o.Count > 0 && o[0] != null)
				del = Convert.ToInt64(o[0]);

			if (o.Count > 1 && o[1] != null)
				dur = Convert.ToInt64(o[1]);

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
		/// <summary>
		/// Sets the state of the NumLock, ScrollLock or CapsLock keys.
		/// </summary>
		/// <param name="Key">
		/// <list type="bullet">
		/// <item>NumLock</item>
		/// <item>ScrollLock</item>
		/// <item>CapsLock</item>
		/// </list>
		/// </param>
		/// <param name="Mode">
		/// <list type="bullet">
		/// <item><term>On</term></item>
		/// <item><term>Off</term></item>
		/// <item><term>AlwaysOn</term>: <description>forces the key to stay on permanently.</description></item>
		/// <item><term>AlwaysOn</term>: <description>forces the key to stay off permanently.</description></item>
		/// <item><term>(blank)</term>: turn off the <c>AlwaysOn</c> or <c>Off</c> states if present.</item>
		/// </list>
		/// </param>
		public static void SetLockState(params object[] obj)
		{
			var (key, mode) = obj.L().S2();
		}
		public static void SetStoreCapsLockMode(params object[] obj)
		{
			var s = obj.L().S1();

			if (s != "")
				Accessors.A_StoreCapsLockMode = s;
		}
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
		internal static string GetKeyNameHelper(int vk, int sc, string def)
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
				WindowsAPI.BlockInput(enable);

			blockInput = enable;
			return ResultType.Ok;//By design, it never returns FAIL.
		}
		/// <summary>
		///
		/// </summary>
		/// <param name="vk"></param>
		/// <param name="keyStateType"></param>
		/// <returns>true if down, else false</returns>
		internal static bool ScriptGetKeyState(int vk, KeyStateTypes keyStateType)
		{
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			if (vk == 0) // Assume "up" if indeterminate.
				return false;

			switch (keyStateType)
			{
				case KeyStateTypes.Toggle: // Whether a toggleable key such as CapsLock is currently turned on.
					return ht.IsKeyToggledOn(vk); // This also works for non-"lock" keys, but in that case the toggle state can be out of sync with other processes/threads.

				case KeyStateTypes.Physical: // Physical state of key.
					if (ht.IsMouseVK(vk)) // mouse button
					{
						return ht.mouseHook != IntPtr.Zero ? (ht.physicalKeyState[vk] & KeyboardMouseSender.StateDown) != 0 : ht.IsKeyDownAsync(vk); // mouse hook is installed, so use it's tracking of physical state.
					}
					else // keyboard
					{
						if (ht.kbdHook != IntPtr.Zero)
						{
							bool? dummy = null;

							// Since the hook is installed, use its value rather than that from
							// GetAsyncKeyState(), which doesn't seem to return the physical state.
							// But first, correct the hook modifier state if it needs it.  See comments
							// in GetModifierLRState() for why this is needed:
							if (ht.KeyToModifiersLR(vk, 0, ref dummy) != 0)    // It's a modifier.
								kbdMouseSender.GetModifierLRState(true); // Correct hook's physical state if needed.

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
			var vk = 0;
			var sc = 0;
			int? modLR = null;
			ht.TextToVKandSC(keyname, ref vk, ref sc, ref modLR, WindowsAPI.GetKeyboardLayout(0));//Need to make cross platform.

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
	}

	internal class ToggleStates
	{
		internal ToggleValueType forceCapsLock = ToggleValueType.Neutral;
		internal ToggleValueType forceNumLock = ToggleValueType.Neutral;
		internal ToggleValueType forceScrollLock = ToggleValueType.Neutral;
	}
}