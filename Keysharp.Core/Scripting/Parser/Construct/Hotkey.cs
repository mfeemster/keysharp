using System;
using System.CodeDom;
using System.Collections.Generic;
using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Windows;
using static Keysharp.Core.Core;

namespace Keysharp.Scripting
{
	public partial class Parser
	{
		private Dictionary<string, string> conditionIds;
		private string lastHotstringFunc = "";
		private string lastHotkeyFunc = "";
		private List<CodeMethodInvokeExpression> stackedHotstrings = new List<CodeMethodInvokeExpression>();
		private List<CodeMethodInvokeExpression> stackedHotkeys = new List<CodeMethodInvokeExpression>();

		private string HotkeyConditionId()
		{
			const string sep = ".";

			if (conditionIds == null)
			{
				conditionIds = new Dictionary<string, string>();
				conditionIds.Add(sep + sep + sep + sep + sep + sep + sep, string.Empty);
			}

			var criteria = string.Concat(
							   IfWinActive_WinTitle, sep, IfWinActive_WinText, sep,
							   IfWinExist_WinTitle, sep, IfWinExist_WinText, sep,
							   IfWinNotActive_WinTitle, sep, IfWinNotActive_WinText, sep,
							   IfWinNotExist_WinTitle, sep, IfWinNotExist_WinText);

			if (conditionIds.TryGetValue(criteria, out var s))
				return s;

			var id = InternalID;
			conditionIds.Add(criteria, id);
			return id;
		}

		private CodeMethodInvokeExpression ParseHotkey(List<CodeLine> lines, int index)
		{
			var codeLine = lines[index];
			var buf = codeLine.Code;
			var hotstringOptionsIndex = 0;
			var hotstringStartIndex = -1;
			var hotkeyFlagIndex = -1;
			var hotstringExecute = false;

			if (buf.Length > 1 && buf[0] == ':')
			{
				hotstringOptionsIndex = 1; // Point it to the hotstring's option letters, if any.

				if (buf[1] != ':')
				{
					// The following relies on the fact that options should never contain a literal colon.
					// ALSO, the following doesn't use IS_HOTSTRING_OPTION() for backward compatibility,
					// performance, and because it seems seldom if ever necessary at this late a stage.
					hotstringStartIndex = buf.IndexOf(HotkeyBound, hotstringOptionsIndex);

					if (hotstringStartIndex <= 0)
						hotstringStartIndex = -1; // Indicate that this isn't a hotstring after all.
					else
						++hotstringStartIndex; // Points to the hotstring itself.
				}
				else // Double-colon, so it's a hotstring if there's more after this (but this means no options are present).
					if (buf.Length > 2)
						hotstringStartIndex = 2;

				//else it's just a naked "::", which is considered to be an ordinary label whose name is colon.
			}

			int cp;

			if (hotstringStartIndex > 0)
			{
				// Check for 'X' option early since escape sequence processing depends on it.
				hotstringExecute = false;// g_HSSameLineAction;//This appeared to always be false in AHK.

				for (cp = hotstringOptionsIndex; cp < hotstringStartIndex; ++cp)
					if (char.ToUpper(buf[cp]) == 'X')
					{
						if (cp < buf.Length - 1)
							hotstringExecute = buf[cp + 1] != '0';
						else
							hotstringExecute = true;

						break;
					}

				// Find the hotstring's final double-colon by considering escape sequences from left to right.
				// This is necessary for it to handle cases such as the following:
				// ::abc```::::Replacement String
				// The above hotstring translates literally into "abc`::".
				// LPTSTR escaped_double_colon = NULL;
				for (cp = hotstringStartIndex; cp < buf.Length; ++cp)  // Increment to skip over the symbol just found by the inner for().
				{
					for (; cp < buf.Length && buf[cp] != Escape && buf[cp] != ':'; ++cp)// Find the next escape char or colon.
					{
					}

					if (cp >= buf.Length - 1) // end of string.
					{
						if (buf.Length > 1 && buf[buf.Length - 1] == '`' && buf[buf.Length - 2] != '`')
							buf = buf.Substring(0, buf.Length - 1);

						break;
					}

					var cp1 = cp + 1;

					if (buf[cp] == ':')
					{
						// v2: Use the first non-escaped double-colon, not the last, since it seems more likely
						// that the user intends to produce text with "::" in it rather than typing "::" to trigger
						// the hotstring, and generally the trigger should be short.  By contrast, the v1 policy
						// behaved inconsistently with an odd number of colons, such as:
						//   ::foo::::bar  ; foo:: -> bar
						//   ::foo:::bar   ; foo -> :bar
						if (hotkeyFlagIndex == -1 && buf[cp1] == ':') // Found a non-escaped double-colon, so this is the right one.
						{
							hotkeyFlagIndex = cp++;  // Increment to have loop skip over both colons.

							if (hotstringExecute)
								break; // Let ParseAndAddLine() properly handle any escape sequences.

							// else continue with the loop so that escape sequences in the replacement
							// text (if there is replacement text) are also translated.
						}

						// else just a single colon, or the second colon of an escaped pair (`::), so continue.
						continue;
					}

					var escChar = buf[cp1];

					switch (escChar)//Escaped chars.
					{
						// Only lowercase is recognized for these:
						case 'a': escChar = '\a'; break;  // alert (bell) character

						case 'b': escChar = '\b'; break;  // backspace

						case 'f': escChar = '\f'; break;  // formfeed

						case 'n': escChar = '\n'; break;  // newline

						case 'r': escChar = '\r'; break;  // carriage return

						case 't': escChar = '\t'; break;  // horizontal tab

						case 'v': escChar = '\v'; break;  // vertical tab

						case 's': escChar = ' '; break;   // space

						case '`': escChar = '`'; break;   // tick (indicated by double tick)
							// Otherwise, if it's not one of the above, the escape-char is considered to
							// mark the next character as literal, regardless of what it is. Examples:
							// `` -> `
							// `: -> : (so `::: means a literal : followed by hotkey_flag)
							// `; -> ;
							// `c -> c (i.e. unknown escape sequences resolve to the char after the `)
					}

					//Omit the escape char, and copy it's real char to the original string.
					buf = buf.Substring(0, cp) + escChar + buf.Substring(cp1 + 1);
					// v2: The following is not done because 1) it is counter-intuitive for ` to affect two
					// characters and 2) it hurts flexibility by preventing the escaping of a single colon
					// immediately prior to the double-colon, such as ::lbl`:::.  Older comment:
					// Since single colons normally do not need to be escaped, this increments one extra
					// for double-colons to skip over the entire pair so that its second colon
					// is not seen as part of the hotstring's final double-colon.  Example:
					// ::ahc```::::Replacement String
					//if (*cp == ':' && *cp1 == ':')
					//  ++cp;
				}

				if (hotkeyFlagIndex == -1)
					hotstringStartIndex = -1;  // Indicate that this isn't a hotstring after all.
			}

			if (hotstringStartIndex <= 0) // Not a hotstring (hotstring_start is checked *again* in case above block changed it; otherwise hotkeys like ": & x" aren't recognized).
			{
				hotkeyFlagIndex = buf.IndexOf(HotkeySignal);

				// Note that there may be an action following the HOTKEY_FLAG (on the same line).
				if (hotkeyFlagIndex != -1) // Find the first one from the left, in case there's more than 1.
				{
					if (hotkeyFlagIndex == 0 && buf[hotkeyFlagIndex + 2] == ':') // v1.0.46: Support ":::" to mean "colon is a hotkey".
						++hotkeyFlagIndex;

					// Above: Hotkeys like "^:::" and "l & :::" are not supported because: 1) some cases are
					// ambiguous, such as "^:::" legitimately remapping caret to colon; 2) retaining support
					// for colon as a remap target would require larger/more complicated code; 3) such hotkeys
					// are hard for a human to read/interpret.
					// v1.0.40: It appears to be a hotkey, but validate it as such before committing to processing
					// it as a hotkey.  If it fails validation as a hotkey, treat it as a command that just happens
					// to contain a double-colon somewhere.  This avoids the need to escape double colons in scripts.
					// Note: Hotstrings can't suffer from this type of ambiguity because a leading colon or pair of
					// colons makes them easier to detect.
					var temp = buf.OmitTrailingWhitespace(hotkeyFlagIndex); // For maintainability.
					var hotkeyValidity = HotkeyDefinition.TextInterpret(temp.TrimStart(Keysharp.Core.Core.SpaceTab), null);

					switch (hotkeyValidity)
					{
						case ResultType.Fail:
							hotkeyFlagIndex = -1; // It's not a valid hotkey, so indicate that it's a command (i.e. one that contains a literal double-colon, which avoids the need to escape the double-colon).
							break;

						case ResultType.ConditionFalse:
							return null;// FAIL; // It's an invalid hotkey and above already displayed the error message.
							//case CONDITION_TRUE:
							// It's a key that doesn't exist on the current keyboard layout.  Leave hotkey_flag set
							// so that the section below handles it as a hotkey.  This allows it to end the auto-exec
							// section and register the appropriate label even though it won't be an active hotkey.
					}
				}
			}

			if (hotkeyFlagIndex > 0) // It's a hotkey/hotstring label.
			{
				// Allow a current function if it is mLastHotFunc, this allows stacking,
				// x::          // mLastHotFunc created here
				// y::action    // parsing "y::" now.
				//We can nest functions in Keysharp, but they end up just being un-nested in the final gnerated code. So we allow them, but they don't do anything special.
				/*
				    if ((g->CurrentFunc && g->CurrentFunc != mLastHotFunc) || mClassObjectCount)
				    {
				    // The reason for not allowing hotkeys and hotstrings inside a function's body is that it
				    // would create a nested function that could be called without ever calling the outer function.
				    // If the hotkey function became a closure, activating the hotkey would at best merely raise an
				    // error since it would not be associated with any particular call to the outer function.
				    // Currently CreateHotFunc() isn't set up to permit it; e.g. it doesn't set mOuterFunc or
				    // mDownVar, so a hotkey inside a function would reset g->CurrentFunc to nullptr for the
				    // remainder of the outer function and would crash if the hotkey references any outer vars.
				    return ScriptError(_T("Hotkeys/hotstrings are not allowed inside functions or classes."), buf);
				    }
				*/
				var hotName = buf.Substring(0, hotkeyFlagIndex);
				var options = buf.Substring(hotstringOptionsIndex, hotkeyFlagIndex - hotstringOptionsIndex);
				var hotstring = hotstringStartIndex >= 0 ? buf.Substring(hotstringStartIndex, hotkeyFlagIndex - hotstringStartIndex) : "";
				hotkeyFlagIndex += HotkeySignal.Length;  // Now hotkey_flag is the hotkey's action, if any.
				var hotkeyUsesOtb = buf.AsSpan(hotkeyFlagIndex).TrimStart(SpaceTab).SequenceEqual("{");

				if (hotstringStartIndex == -1)
				{
					// Mustn't use ltrim(hotkey_flag) because that would cause buf.length to become incorrect:
					hotkeyFlagIndex = buf.FindFirstNotOf(SpaceTab, hotkeyFlagIndex);

					// Not done because Hotkey::TextInterpret() does not allow trailing whitespace:
					//rtrim(buf); // Trim the new substring inside of buf (due to temp termination). It has already been ltrimmed.

					// To use '{' as remap_dest, escape it!.
					if (hotkeyFlagIndex >= 0 &&
							hotkeyFlagIndex < buf.Length &&
							buf[hotkeyFlagIndex] == Escape &&
							buf[hotkeyFlagIndex + 1] == '{')
						hotkeyFlagIndex++;

					cp = hotkeyFlagIndex; // Set default, conditionally overridden below (v1.0.44.07).
					int remap_dest_vk;
					var temp = buf.Substring(hotkeyFlagIndex);//Default.
					int? modifiersLR = null;
					var ht = Keysharp.Scripting.Script.HookThread;

					// v1.0.40: Check if this is a remap rather than hotkey:
					if (!hotkeyUsesOtb
							&& hotkeyFlagIndex >= 0 && hotkeyFlagIndex < buf.Length - 1 // This hotkey's action is on the same line as its trigger definition.
							&& ((remap_dest_vk = buf[hotkeyFlagIndex + 1]) != 0 ? ht.TextToVK(
									temp = HotkeyDefinition.TextToModifiers(buf.Substring(hotkeyFlagIndex), null),
									ref modifiersLR, false, true, WindowsAPI.GetKeyboardLayout(0)) : 0xFF) != 0) // And the action appears to be a remap destination rather than a command.
						// For above:
						// Fix for v1.0.44.07: Set remap_dest_vk to 0xFF if hotkey_flag's length is only 1 because:
						// 1) It allows a destination key that doesn't exist in the keyboard layout (such as 6::ð in
						//    English).
						// 2) It improves performance a little by not calling c  except when the destination key
						//    might be a mouse button or some longer key name whose actual/correct VK value is relied
						//    upon by other places below.
						// Fix for v1.0.40.01: Since remap_dest_vk is also used as the flag to indicate whether
						// this line qualifies as a remap, must do it last in the statement above.  Otherwise,
						// the statement might short-circuit and leave remap_dest_vk as non-zero even though
						// the line shouldn't be a remap.  For example, I think a hotkey such as "x & y::return"
						// would trigger such a bug.
					{
						int remap_source_vk;
						string tempcp1, remap_source, remap_dest, remap_dest_modifiers; // Must fit the longest key name (currently Browser_Favorites [17]), but buffer overflow is checked just in case.
						bool remap_source_is_combo, remap_source_is_mouse, remap_dest_is_mouse, remap_keybd_to_mouse;
						// These will be ignored in other stages if it turns out not to be a remap later below:
						remap_source_vk = ht.TextToVK(tempcp1 = HotkeyDefinition.TextToModifiers(buf, null), ref modifiersLR, false, true, WindowsAPI.GetKeyboardLayout(0));//An earlier stage verified that it's a valid hotkey, though VK could be zero.
						remap_source_is_combo = tempcp1.IndexOf(HotkeyDefinition.COMPOSITE_DELIMITER) != -1;
						remap_source_is_mouse = ht.IsMouseVK(remap_source_vk);
						remap_dest_is_mouse = ht.IsMouseVK(remap_dest_vk);
						remap_keybd_to_mouse = !remap_source_is_mouse && remap_dest_is_mouse;
						remap_source = (remap_source_is_combo ? "" : "*") +// v1.1.27.01: Omit * when the remap source is a custom combo.
									   (tempcp1.Length == 1 && char.IsUpper(tempcp1[0]) ? "+" : "") +// Allow A::b to be different than a::b.
									   buf;// Include any modifiers too, e.g. ^b::c.

						if (temp[0] == '"' || temp[0] == Escape) // Need to escape these.
							remap_dest = $"{Escape}{temp[0]}";
						else
							remap_dest = temp;// But exclude modifiers here; they're wanted separately.

						remap_dest_modifiers = buf.Substring(hotkeyFlagIndex);

						if (remap_dest_modifiers.Length == 0
								&& (remap_dest_vk == WindowsAPI.VK_PAUSE)
								&& string.Compare(remap_dest, "Pause", true) == 0) // Specifically "Pause", not "vk13".
						{
							// In the unlikely event that the dest key has the same name as a command, disqualify it
							// from being a remap (as documented).
							// v1.0.40.05: If the destination key has any modifiers,
							// it is unambiguously a key name rather than a command.
						}
						else
						{
							// It is a remapping. Create one "down" and one "up" hotkey,
							// eg, "x::y" yields,
							// *x::
							// {
							// SetKeyDelay(-1), Send("{Blind}{y DownR}")
							// }
							// *x up::
							// {
							// SetKeyDelay(-1), Send("{Blind}{y Up}")
							// }
							// Using one line to facilitate code.
							// For remapping, decided to use a "macro expansion" approach because I think it's considerably
							// smaller in code size and complexity than other approaches would be.  I originally wanted to
							// do it with the hook by changing the incoming event prior to passing it back out again (for
							// example, a::b would transform an incoming 'a' keystroke into 'b' directly without having
							// to suppress the original keystroke and simulate a new one).  Unfortunately, the low-level
							// hooks apparently do not allow this.  Here is the test that confirmed it:
							// if (event.vkCode == 'A')
							// {
							//  event.vkCode = 'B';
							//  event.scanCode = 0x30; // Or use vk_to_sc(event.vkCode).
							//  return CallNextHookEx(g_KeybdHook, aCode, wParam, lParam);
							// }
							//Unsure exactly how we'll check for this, revisit later.//TODO
							if (lastHotstringFunc != null)
								// Checking this to disallow stacking, eg
								// x::
								// y::z
								// which would cause x:: to just do the "down"
								// part of y::z.
								return null;// ScriptError(ERR_HOTKEY_MISSING_BRACE);

							/*
							*/
							Func<string, ResultType> make_remap_hotkey = (string aKey) => //[&](LPTSTR aKey)
							{
								/*
								    if (!CreateHotFunc())
								    return ResultType.Fail;

								    hk = Hotkey::FindHotkeyByTrueNature(aKey, suffix_has_tilde, hook_is_mandatory);

								    if (hk)
								    {
								    if (!hk->AddVariant(mLastHotFunc, suffix_has_tilde))
								        return ResultType.Fail;
								    }
								    else if (!Hotkey::AddHotkey(mLastHotFunc, HK_NORMAL, aKey, suffix_has_tilde))
								    return ResultType.Fail;
								*/
								return ResultType.Ok;
							};

							// Start with the "down" hotkey:
							if (make_remap_hotkey(remap_source) == ResultType.Fail)
								return null;// ResultType.Fail;

							var remap_buf = $"{(remap_dest_is_mouse ? "SetMouseDelay" : "SetKeyDelay")}(-1)"; // Does NOT need to be "-1, -1" for SetKeyDelay (see below).

							//cp = remap_buf;
							// It seems unnecessary to set press-duration to -1 even though the auto-exec section might
							// have set it to something higher than -1 because:
							// 1) Press-duration doesn't apply to normal remappings since they use down-only and up-only events.
							// 2) Although it does apply to remappings such as a::B and a::^b (due to press-duration being
							//    applied after a change to modifier state), those remappings are fairly rare and supporting
							//    a non-negative-one press-duration (almost always 0) probably adds a degree of flexibility
							//    that may be desirable to keep.
							// 3) SendInput may become the predominant SendMode, so press-duration won't often be in effect anyway.
							// 4) It has been documented that remappings use the auto-execute section's press-duration.
							// The primary reason for adding Key/MouseDelay -1 is to minimize the chance that a one of
							// these hotkey threads will get buried under some other thread such as a timer, which
							// would disrupt the remapping if #MaxThreadsPerHotkey is at its default of 1.
							if (remap_keybd_to_mouse)
							{
								// Since source is keybd and dest is mouse, prevent keyboard auto-repeat from auto-repeating
								// the mouse button (since that would be undesirable 90% of the time).  This is done
								// by inserting a single extra IF-statement above the Send that produces the down-event:
								remap_buf += $"!GetKeyState(\"{remap_dest}\") &&";// Should be no risk of buffer overflow due to prior validation.
							}

							// Otherwise, remap_keybd_to_mouse==false.
							string blind_mods = "", next_blind_mod = "";
							var modchars = "!#^+";

							foreach (var ch in modchars)
							{
								var tempindex = remap_source.IndexOf(ch);

								if (tempindex != -1 && tempindex < modchars.Length - 1)//Exclude the last char for !:: and similar.
									next_blind_mod += ch;
							}

							//next_blind_mod = '\0';
							var extra_event = ""; // Set default.

							switch (remap_dest_vk)
							{
								case WindowsAPI.VK_LMENU:
								case WindowsAPI.VK_RMENU:
								case WindowsAPI.VK_MENU:
									switch (remap_source_vk)
									{
										case WindowsAPI.VK_LCONTROL:
										case WindowsAPI.VK_CONTROL:
											extra_event = "{LCtrl up}"; // Somewhat surprisingly, this is enough to make "Ctrl::Alt" properly remap both right and left control.
											break;

										case WindowsAPI.VK_RCONTROL:
											extra_event = "{RCtrl up}";
											break;
											// Below is commented out because its only purpose was to allow a shift key remapped to alt
											// to be able to alt-tab.  But that wouldn't work correctly due to the need for the following
											// hotkey, which does more harm than good by impacting the normal Alt key's ability to alt-tab
											// (if the normal Alt key isn't remapped): *Tab::Send {Blind}{Tab}
											//case VK_LSHIFT:
											//case VK_SHIFT:
											//  extra_event = "{LShift up}";
											//  break;
											//case VK_RSHIFT:
											//  extra_event = "{RShift up}";
											//  break;
									}

									break;
							}

							remap_buf += string.Format("Send(\"{{Blind{0}}}{1}{2}{{{3} DownR}}\")", blind_mods, extra_event, remap_dest_modifiers, remap_dest);//DownR vs. Down. See Send's DownR handler for details.
							Func<ResultType> define_remap_func = () =>
							{
								//if (!AddLine(ACT_BLOCK_BEGIN)
								//      || !ParseAndAddLine(remap_buf)
								//      || !AddLine(ACT_BLOCK_END))
								//  return FAIL;
								return ResultType.Ok;
							};

							if (define_remap_func() == ResultType.Fail) // the "down" function.
								return null;// ResultType.Fail;

							//
							// "Down" is finished, proceed with "Up":
							//
							remap_buf = $"{remap_source} up";// Key-up hotkey, e.g. *LButton up::

							if (make_remap_hotkey(remap_buf) == ResultType.Fail)
								return null;// ResultType.Fail;

							//Unlike the down-event above, remap_dest_modifiers is not included for the up-event; e.g. ^{b up} is inappropriate.
							//Using the full function names vs. Set%sDelay might help size due to string pooling.
							remap_buf = $"{(remap_dest_is_mouse ? "SetMouseDelay" : "SetKeyDelay")}(-1),Send(\"{{Blind}}{{{remap_dest} Up}}\")\n";

							if (define_remap_func() == ResultType.Fail) // define the "up" function.
								return null;// ResultType.Fail;

							//goto continue_main_loop;//TODO
							return null;
						}

						// Since above didn't goto this is not a remap after all:
					}
				}

				// else don't trim hotstrings since literal spaces in both substrings are significant.
				string SetLastHotstringFunc(string hotstringName) => lastHotstringFunc.Length == 0 ? (lastHotstringFunc = LabelMethodName(hotstringName)) : lastHotstringFunc;
				void ClearParserHotstringState()
				{
					lastHotstringFunc = "";//Don't clear replacement.
					stackedHotstrings.Clear();
				}
				string replacement;
				void ClearParserHotState()
				{
					replacement = "";
					lastHotkeyFunc = "";
					lastHotstringFunc = "";
					stackedHotkeys.Clear();
					stackedHotstrings.Clear();
				}

				if (hotstringStartIndex != -1)
				{
					if (hotstringStartIndex == 0)
					{
						// The following error message won't indicate the correct line number because
						// the hotstring (as a label) does not actually exist as a line.  But it seems
						// best to report it this way in case the hotstring is inside a #Include file,
						// so that the correct file name and approximate line number are shown:
						throw new ParseException($"This hotstring is missing its abbreviation: {buf}.");
					}

					// In the case of hotstrings, hotstring_start is the beginning of the hotstring itself,
					// i.e. the character after the second colon.  hotstring_options is the first character
					// in the options list (which is terminated by a colon).  hotkey_flag is blank or the
					// hotstring's auto-replace text or same-line action.
					// v1.0.42: Unlike hotkeys, duplicate hotstrings are not detected.  This is because
					// hotstrings are less commonly used and also because it requires more code to find
					// hotstring duplicates (and performs a lot worse if a script has thousands of
					// hotstrings) because of all the hotstring options.
					if (hotstringExecute && (hotkeyFlagIndex == -1 || hotkeyUsesOtb))
						// Do not allow execute option with blank line or OTB.
						// Without this check, this
						// :X:x::
						// {
						// }
						// would execute the block. But X is supposed to mean "execute this line".
						throw new ParseException("Expected single-line action.");

					if (hotkeyUsesOtb)
					{
						// Never use otb if text or raw mode is in effect for this hotstring.
						// Either explicitly or via #hotstring.
						var uses_text_or_raw_mode = HotstringDefinition.DefaultHotstringSendRaw != SendRawModes.NotRaw;

						for (var i = hotstringOptionsIndex; i < hotstringStartIndex; ++i)
						{
							switch (char.ToUpper(buf[i]))
							{
								case 'T':
								case 'R':
									uses_text_or_raw_mode = buf[i + 1] != '0';
									break;
							}
						}

						if (uses_text_or_raw_mode)
							hotkeyUsesOtb = false;
					}

					replacement = hotkeyFlagIndex >= 0 ? buf.Substring(hotkeyFlagIndex) : "";

					//All of this here needs work.//TODO
					// The hotstring never uses otb if it uses X or T options (either explicitly or via #hotstring).
					if (replacement.Length == 0 || hotkeyUsesOtb || hotstringExecute)
					{
						if (SetLastHotstringFunc(hotName).Length == 0)// It is not auto-replace
							return null;// ResultType.Fail;
					}
					else if (lastHotstringFunc.Length > 0 && replacement.Length > 0)//Can't have stacked auto replace hotstrings.
					{
						// It is autoreplace but an earlier hotkey or hotstring
						// is "stacked" above, treat it as and error as it doesn't
						// make sense. Otherwise one could write something like:
						//
						//    ::abc::
						//    ::def::text
						//    x::action
						//    which would work as,
						//    ::def::text
						//    ::abc::
						//    x::action
						// Note that if it is ":X:def::action" instead, we do not end up here and
						// "::abc::" will also trigger "action".
						throw new ParseException("Hotkey or hotstring is missing its opening brace.");
					}

					var funcname = "";
					var nextIndex = index + 1;

					if (hotstringExecute)
					{
						funcname = LabelMethodName(hotName);
						var method = LocalMethod(funcname);
						var expr = ParseMultiExpression(replacement, true);//Original appeard to just support one function call, but it seems easy enough to support multiple statements separated by commas. All vars will be created as global.
						method.Statements.AddRange(expr);
						methods.Add(method.Name, method);
						ClearParserHotstringState();
					}
					else if (nextIndex < lines.Count)//Merge this with detecting otb, and see how X fits into this above.//TODO
					{
						var nextLine = lines[nextIndex];
						var nextBuf = nextLine.Code;

						if (IsFunction(nextBuf, nextIndex + 1 < lines.Count ? lines[nextIndex + 1].Code : string.Empty))
						{
							funcname = ParseFunctionName(nextBuf);

							foreach (var stacked in stackedHotstrings)//Go back through all stacked hotstrings that were parsed, and reset their function object to the one that was just parsed.
								stacked.Parameters[1] = new CodeSnippetExpression($"new FuncObj(\"{funcname}\", null)");

							ClearParserHotstringState();
						}
						else
						{
							var expect = nextBuf.StartsWith(BlockOpen);

							if (hotkeyUsesOtb ^ expect)//This is a hotstring that performs a custom action, so treat the body of it as the start of a new function.
							{
								StartNewFunction();
								funcname = SetLastHotstringFunc(hotName);
								var method = LocalMethod(funcname);
								var block = new CodeBlock(nextLine, funcname, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
								block.Type = expect ? CodeBlock.BlockType.Expect : CodeBlock.BlockType.Within;
								_ = CloseTopSingleBlock();
								blocks.Push(block);
								methods.Add(method.Name, method);
								ClearParserHotstringState();
							}
							else if (replacement.Length == 0)//Check for stacked
							{
								if (IsHotstringLabel(nextBuf))
								{
									funcname = SetLastHotstringFunc(hotName);//Stacked, so set the current line as the name of the function that will be used for all stacked. If a named function is eventually seen, that name will be used for all stacked.
								}
								else if (nextBuf != "")//Read all lines until a return statement is found, then treat them all as a function by inserting braces and marking all variable references as global.
								{
									var existingLineCount = lines.Count;
									var np1 = nextIndex + 1;
									lines.Insert(nextIndex, new CodeLine(nextLine.FileName, np1, "{"));
									var linesAdded = lines.Count - existingLineCount;
									var i = np1;

									for (; i < lines.Count; i++)
									{
										lines[i].LineNumber += linesAdded;

										if (lines[i].Code.Trim() == "return")
											break;
									}

									i++;
									lines.Insert(i, new CodeLine(nextLine.FileName, i, "}"));

									for (; i < lines.Count; i++)
										lines[i].LineNumber = i + 1;

									StartNewFunction();
									funcname = SetLastHotstringFunc(hotName);
									var method = LocalMethod(funcname);
									var block = new CodeBlock(nextLine, funcname, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
									block.Type = CodeBlock.BlockType.Expect;
									_ = CloseTopSingleBlock();
									blocks.Push(block);
									methods.Add(method.Name, method);
									ClearParserHotstringState();
								}
							}
						}
					}

					Persistent = true;
					var hasContinuationSection = replacement.IndexOfAny(CrLf) != -1;//Not a perfect detection, but will be correct most of the time.
					var invoke = (CodeMethodInvokeExpression)InternalMethods.AddHotstring;
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(hotName));
					_ = invoke.Parameters.Add(funcname != "" ? new CodeSnippetExpression($"new FuncObj(\"{funcname}\", null)") : new CodePrimitiveExpression(null));
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(options));
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(hotstring));
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(hotstringExecute || hotkeyUsesOtb ? "" : replacement));
					_ = invoke.Parameters.Add(new CodePrimitiveExpression(hasContinuationSection));

					//If this hotstring did not define a handler, then we need to keep track of it, plus all subsequent hotstrings until we find a handler that is an explicit function name.
					//Then once found, that function name is retroactively set as the second argument for all of the stacked hostrings and the list of stacked hotstring is cleared (see above).
					if (replacement.Length == 0 && lastHotstringFunc.Length > 0)
						stackedHotstrings.Add(invoke);

					lastHotkeyFunc = "";//After adding a hotstring, we must clear any hotkey state.
					return invoke;
				}
				else // It's a hotkey vs. hotstring.
				{
					var hook_action = HotkeyDefinition.ConvertAltTab(buf.Substring(hotkeyFlagIndex), false);
					var suffixHasTilde = false;
					var hookIsMandatory = false;
					var funcname = "";
					var nextIndex = index + 1;
					replacement = !hotkeyUsesOtb && hotkeyFlagIndex >= 0 ? buf.Substring(hotkeyFlagIndex) : "";

					if (replacement.Length > 0)//For hotkeys, there is no 'X' for treating the replacement as an execute. Instead, if any replacement exists on the same line, it's an execute.
					{
						funcname = SetLastHotkeyFunc(hotName);
						var method = LocalMethod(funcname);
						var expr = ParseMultiExpression(replacement, true);//Original appeard to just support one function call, but it seems easy enough to support multiple statements separated by commas. All vars will be created as global.
						method.Statements.AddRange(expr);
						_ = method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "thishotkey"));
						methods.Add(method.Name, method);

						if (AddHotkeyMethodInvoke(buf, hotName, hook_action, replacement, ref suffixHasTilde, ref hookIsMandatory) is CodeMethodInvokeExpression cmie)
						{
							ClearParserHotState();
							return cmie;
						}
					}
					else if (nextIndex < lines.Count)
					{
						var nextLine = lines[nextIndex];
						var nextBuf = nextLine.Code;

						if (IsFunction(nextBuf, nextIndex + 1 < lines.Count ? lines[nextIndex + 1].Code : string.Empty))
						{
							lastHotkeyFunc = ParseFunctionName(nextBuf);

							foreach (var stacked in stackedHotkeys)//Go back through all stacked hotstrings that were parsed, and reset their function object to the one that was just parsed.
								stacked.Parameters[0] = new CodeSnippetExpression($"new FuncObj(\"{lastHotkeyFunc}\", null)");

							stackedHotkeys.Clear();

							if (nextBuf.RemoveAll(Spaces).Contains($"{lastHotkeyFunc}()"))//They didn't explicitly supply the ThisHotkey parameter, so add it.
								lines[nextIndex] = new CodeLine(nextLine.FileName, nextLine.LineNumber, nextBuf.Insert(nextBuf.IndexOf('(') + 1, "thishotkey"));

							if (AddHotkeyMethodInvoke(buf, hotName, hook_action, replacement, ref suffixHasTilde, ref hookIsMandatory) is CodeMethodInvokeExpression cmie)
							{
								ClearParserHotState();
								return cmie;
							}
						}
						else
						{
							var expect = nextBuf.StartsWith(BlockOpen);

							if (hotkeyUsesOtb ^ expect)//This is a hotstring that performs a custom action, so treat the body of it as the start of a new function.
							{
								StartNewFunction();
								funcname = SetLastHotkeyFunc(hotName);//Will not overwrite the previous function name if this is the end of a stacked list of hotkeys.
								var method = LocalMethod(funcname);
								currentFuncParams.Peek().Add("thishotkey");
								_ = method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "thishotkey"));
								var block = new CodeBlock(nextLine, funcname, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
								block.Type = expect ? CodeBlock.BlockType.Expect : CodeBlock.BlockType.Within;
								_ = CloseTopSingleBlock();
								blocks.Push(block);
								methods.Add(method.Name, method);

								if (AddHotkeyMethodInvoke(buf, hotName, hook_action, replacement, ref suffixHasTilde, ref hookIsMandatory) is CodeMethodInvokeExpression cmie)
								{
									ClearParserHotState();
									return cmie;
								}
							}
							else
							{
								if (IsHotkeyLabel(nextBuf))//Check for stacked.
								{
									funcname = SetLastHotkeyFunc(hotName);//If this is the first of a stack, set the function name to the hash of the first stacked item's name, else keep that name for subsequent stacked hotkeys.

									if (AddHotkeyMethodInvoke(buf, hotName, hook_action, replacement, ref suffixHasTilde, ref hookIsMandatory) is CodeMethodInvokeExpression cmie)
									{
										return cmie;//Do not clear state here, we want to save it for future stacked hotkeys.
									}
								}
								else if (nextBuf != "")//Read all lines until a return statement is found, then treat them all as a function by inserting braces and marking all variable references as global.
								{
									var existingLineCount = lines.Count;
									var np1 = nextIndex + 1;
									lines.Insert(nextIndex, new CodeLine(nextLine.FileName, np1, "{"));
									var linesAdded = lines.Count - existingLineCount;
									var i = np1;

									for (; i < lines.Count; i++)
									{
										lines[i].LineNumber += linesAdded;

										if (lines[i].Code.Trim() == "return")
											break;
									}

									i++;
									lines.Insert(i, new CodeLine(nextLine.FileName, i, "}"));

									for (; i < lines.Count; i++)
										lines[i].LineNumber = i + 1;

									StartNewFunction();
									funcname = SetLastHotkeyFunc(hotName);
									var method = LocalMethod(funcname);
									currentFuncParams.Peek().Add("thishotkey");
									_ = method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "thishotkey"));
									var block = new CodeBlock(nextLine, funcname, method.Statements, CodeBlock.BlockKind.Function, blocks.PeekOrNull());
									block.Type = CodeBlock.BlockType.Expect;
									_ = CloseTopSingleBlock();
									blocks.Push(block);
									methods.Add(method.Name, method);

									if (AddHotkeyMethodInvoke(buf, hotName, hook_action, replacement, ref suffixHasTilde, ref hookIsMandatory) is CodeMethodInvokeExpression cmie)
									{
										ClearParserHotState();
										return cmie;
									}
								}
							}
						}
					}
				}
			}

			return null;
		}

		private string SetLastHotkeyFunc(string hotkeyName) => lastHotkeyFunc.Length == 0 ? (lastHotkeyFunc = LabelMethodName(hotkeyName)) : lastHotkeyFunc;

		private CodeMethodInvokeExpression AddHotkeyMethodInvoke(string buf, string hotkeyName, uint hook_action, string replacement, ref bool suffixHasTilde, ref bool hookIsMandatory)
		{
			HotkeyDefinition hk;
			var suffix_has_tilde = false;//Need to figure out passing this in or what to do with them later.//TODO
			var hook_is_mandatory = false;

			if ((hk = HotkeyDefinition.FindHotkeyByTrueNature(buf, ref suffixHasTilde, ref hookIsMandatory)) != null) // Parent hotkey found.  Add a child/variant hotkey for it.
			{
				if (hook_action != 0) // suffix_has_tilde has always been ignored for these types (alt-tab hotkeys).
				{
					// Hotkey::Dynamic() contains logic and comments similar to this, so maintain them together.
					// An attempt to add an alt-tab variant to an existing hotkey.  This might have
					// merit if the intention is to make it alt-tab now but to later disable that alt-tab
					// aspect via the Hotkey cmd to let the context-sensitive variants shine through
					// (take effect).
					hk.hookAction = hook_action;
				}
				else
				{
					// Detect duplicate hotkey variants to help spot bugs in scripts.
					if (hk.FindVariant() != null) // See if there's already a variant matching the current criteria (suffix_has_tilde does not make variants distinct form each other because it would require firing two hotkey IDs in response to pressing one hotkey, which currently isn't in the design).
					{
						//mCurrLine = NULL;  // Prevents showing unhelpful vicinity lines.
						return null;// ScriptError(_T("Duplicate hotkey."), buf);
					}

					//if (set_last_hotfunc() == null)//TODO
					//return null;// ResultType.Fail;

					if (hk.AddVariant(new FuncObj(lastHotkeyFunc, null), suffix_has_tilde) == null)
						return null;// ScriptError(ERR_OUTOFMEM, buf);

					if (hook_is_mandatory || Keysharp.Scripting.Script.forceKeybdHook)
					{
						// Require the hook for all variants of this hotkey if any variant requires it.
						// This seems more intuitive than the old behavior, which required $ or #UseHook
						// to be used on the *first* variant, even though it affected all variants.
						hk.keybdHookMandatory = true;
					}
				}

				//Not sure if stackedHotkeys should be used here too?//TODO
				lastHotstringFunc = "";//After adding a hotkey, we must clear any hotstring state.
				Parser.Persistent = true;
				//Need to figure out what to return here, else it'll just return null below.//TODO
			}
			else // No parent hotkey yet, so create it.
			{
				if (hook_action != (uint)HotkeyTypeEnum.Normal && !string.IsNullOrEmpty(lastHotkeyFunc))
					// A hotkey is stacked above, eg,
					// x::
					// y & z::altTab
					// Not supported.
					return null; // ScriptError(ERR_HOTKEY_MISSING_BRACE);

				if (hook_action == (uint)HotkeyTypeEnum.Normal && string.IsNullOrEmpty(SetLastHotkeyFunc(hotkeyName)))
					return null;

				Persistent = true;
				var invoke = (CodeMethodInvokeExpression)InternalMethods.AddHotkey;
				_ = invoke.Parameters.Add(lastHotkeyFunc != "" ? new CodeSnippetExpression($"new FuncObj(\"{lastHotkeyFunc}\", null)") : new CodePrimitiveExpression(null));
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(hook_action));
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(hotkeyName));
				_ = invoke.Parameters.Add(new CodePrimitiveExpression(suffix_has_tilde));

				if (replacement.Length == 0 && lastHotkeyFunc.Length > 0)//Detect if this was part of a stack, in which case add this hotkey for later reference in case a named function handler is encountered.
					stackedHotkeys.Add(invoke);

				return invoke;
				//hk = HotkeyDefinition.AddHotkey(new FuncObj(lastHotFunc, null), hook_action, buf, suffix_has_tilde);
				//if (hk == null)
				//{
				//  if (hotkeyValidity != ResultType.ConditionTrue)
				//      return null;// ResultType.Fail; // It already displayed the error.
				//  // This hotkey uses a single-character key name, which could be valid on some other
				//  // keyboard layout.  Allow the script to start, but warn the user about the problem.
				//  // Note that this hotkey's label is still valid even though the hotkey wasn't created.
				//  if (!Keysharp.Scripting.Script.validateThenExit) // Current keyboard layout is not relevant in /validate mode.
				//      _ = Keysharp.Core.Dialogs.MsgBox($"Note: The hotkey {buf} will not be active because it does not exist in the current keyboard layout.");
				//}
			}

			return null;
		}
	}
}