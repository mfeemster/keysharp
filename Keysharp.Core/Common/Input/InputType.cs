using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Threading;
using Keysharp.Core.Windows;
using Keysharp.Scripting;

namespace Keysharp.Core.Common.Input
{
	internal class InputType//Need to finish implementing this class. This is also very Windows specific, and needs to eventually be made into a common base with derived OS specific classes.//TODO
	{
		internal static System.Windows.Forms.Timer inputTimer;
		internal bool BackspaceIsUndo = true;
		internal string buffer = "";

		// Stores the user's actual input.
		//internal int BufferLength; // The current length of what the user entered.
		internal int BufferLengthMax = 1023;

		internal bool CaseSensitive;
		internal bool EndCharMode;

		// The maximum allowed length of the input.
		internal string EndChars = "";

		// A string of characters that should terminate the input.
		internal uint EndCharsMax;

		internal bool EndingBySC;
		internal char EndingChar;
		internal int EndingMatchIndex;
		internal int EndingMods;

		// Whether the Ending key was one handled by VK or SC.
		internal bool EndingRequiredShift;

		internal int EndingSC;
		internal int EndingVK;
		internal bool FindAnywhere;
		internal int[] KeySC = new int[HookThread.SC_ARRAY_COUNT];

		// The hook puts the terminating key into one of these if that's how it was terminated.
		// Whether the key that terminated the input was one that needed the SHIFT key.
		internal int[] KeyVK = new int[HookThread.VK_ARRAY_COUNT];

		// Current size of EndChars buffer. (probably not needed)//TODO
		internal List<string> match = new List<string>();

		internal string MatchBuf = "";

		// The is the buffer whose contents are pointed to by the match array.
		internal uint MatchBufSize;

		// Array of strings, each string is a match-phrase which if entered, terminates the input.
		internal uint MatchCount;

		// The number of strings currently in the array.
		internal uint MatchCountMax;

		internal int MinSendLevel;
		internal bool NotifyNonText;
		internal InputType Prev;
		internal InputObject ScriptObject;
		internal InputStatusType Status = InputStatusType.Off;

		// The maximum number of strings that the match array can contain.
		// The capacity of the above buffer.
		internal int Timeout;

		internal DateTime TimeoutAt;

		// The minimum SendLevel that can be captured by this input (0 allows all).
		internal bool TranscribeModifiedKeys; // Whether the input command will attempt to transcribe modified keys such as ^c.

		internal bool VisibleText, VisibleNonText = true;
		// A sparse array of key flags by VK.
		// A sparse array of key flags by SC.

		internal InputType(InputObject io, string aOptions, string aEndKeys, string aMatchList)
		{
			ScriptObject = io;
			ParseOptions(aOptions);
			SetKeyFlags(aEndKeys);
			match = aMatchList.SplitWithDelimiter(Core.Comma, true);
		}

		internal void CollectChar(string ch, int charCount)
		{
			for (var i = 0; i < charCount; ++i)
			{
				if (EndChars.IndexOf(ch[i], CaseSensitive ? StringComparison.CurrentCulture : StringComparison.OrdinalIgnoreCase) != -1)
				{
					EndByChar(ch[i]);
					return;
				}

				if (buffer.Length == BufferLengthMax)
				{
					if (buffer.Length == 0) // For L0, collect nothing but allow OnChar, etc.
						return;

					break;
				}

				buffer += ch[i];
			}

			// Check if the buffer now matches any of the key phrases, if there are any:
			if (FindAnywhere)
			{
				if (CaseSensitive)
				{
					for (var i = 0; i < match.Count; ++i)
					{
						if (buffer.IndexOf(match[i], StringComparison.CurrentCulture) != -1)
						{
							EndByMatch(i);
							return;
						}
					}
				}
				else // Not case sensitive.
				{
					for (var i = 0; i < MatchCount; ++i)
					{
						if (buffer.IndexOf(match[i], StringComparison.OrdinalIgnoreCase) != -1)
						{
							EndByMatch(i);
							return;
						}
					}
				}
			}
			else // Exact match is required
			{
				if (CaseSensitive)
				{
					for (var i = 0; i < MatchCount; ++i)
					{
						if (string.Compare(buffer, match[i], StringComparison.CurrentCulture) == 0)
						{
							EndByMatch(i);
							return;
						}
					}
				}
				else // Not case sensitive.
				{
					for (var i = 0; i < MatchCount; ++i)
					{
						// v1.0.43.03: Changed to locale-insensitive search.  See similar v1.0.43.03 comment above for more details.
						if (string.Compare(buffer, match[i], StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							EndByMatch(i);
							return;
						}
					}
				}
			}

			// Otherwise, no match found.
			if (buffer.Length >= BufferLengthMax)
				EndByLimit();
		}

		internal void EndByChar(char ch)
		{
			EndingChar = ch;
			// The other EndKey related fields are ignored when Char is non-zero.
			EndByReason(InputStatusType.TerminatedByEndKey);
		}

		internal void EndByKey(int vk, int sc, bool bySC, bool requiredShift)
		{
			EndingVK = vk;
			EndingSC = sc;
			EndingBySC = bySC;
			EndingRequiredShift = requiredShift;
			EndingChar = (char)0; // Must be zero if the above are to be used.
			EndByReason(InputStatusType.TerminatedByEndKey);
		}

		internal void EndByLimit() => EndByReason(InputStatusType.LimitReached);

		internal void EndByMatch(int matchIndex)
		{
			EndingMatchIndex = matchIndex;
			EndByReason(InputStatusType.TerminatedByMatch);
		}

		internal void EndByTimeout() => EndByReason(InputStatusType.TimedOut);

		internal string GetEndReason(ref string keyBuf)
		{
			if (Script.HookThread is Keysharp.Core.Common.Threading.HookThread hook && hook.kbdMsSender != null)
			{
				switch (Status)
				{
					case InputStatusType.TimedOut:
						return "Timeout";

					case InputStatusType.TerminatedByMatch:
						return "Match";

					case InputStatusType.TerminatedByEndKey:
					{
						var keyName = keyBuf;

						if (keyName == null)
							return "EndKey";

						if (EndingChar != '\0')
						{
							keyName = EndingChar.ToString();
						}
						else if (EndingRequiredShift)
						{
							// Since the only way a shift key can be required in our case is if it's a key whose name
							// is a single char (such as a shifted punctuation mark), use a diff. method to look up the
							// key name based on fact that the shift key was down to terminate the input.  We also know
							// that the key is an EndingVK because there's no way for the shift key to have been
							// required by a scan code based on the logic (above) that builds the end_key arrays.
							// MSDN: "Typically, ToAscii performs the translation based on the virtual-key code.
							// In some cases, however, bit 15 of the uScanCode parameter may be used to distinguish
							// between a key press and a key release. The scan code is used for translating ALT+
							// number key combinations.
							var state = new byte[256];
							var sb = new StringBuilder();
							state[(int)Keys.ShiftKey] |= 0x80; // Indicate that the neutral shift key is down for conversion purposes.
							var active_window_keybd_layout = hook.kbdMsSender.GetFocusedKeybdLayout(IntPtr.Zero);
							var count = WindowsAPI.ToUnicodeEx((uint)EndingVK, (uint)hook.MapVkToSc(EndingVK), state // Nothing is done about ToAsciiEx's dead key side-effects here because it seems to rare to be worth it (assuming its even a problem).
															   , sb, 2, Script.menuIsVisible != MenuType.None ? 1u : 0u, active_window_keybd_layout); // v1.0.44.03: Changed to call ToAsciiEx() so that active window's layout can be specified (see hook.cpp for details).
							keyName = keyName.Substring(0, count);
						}
						else
						{
							keyName = "";

							if (EndingBySC)
								keyName = hook.SCtoKeyName(EndingSC, false);

							if (keyName?.Length == 0)
								keyName = hook.VKtoKeyName(EndingVK, !EndingBySC);

							if (keyName?.Length == 0)
								keyName = "sc" + EndingSC.ToString("X3");
						}

						keyBuf = keyName;
						return "EndKey";
					}

					case InputStatusType.LimitReached:
						return "Max";

					case InputStatusType.Off:
						return "Stopped";
				}
			}

			return "";
		}

		internal bool InProgress() => Status == InputStatusType.InProgress;

		internal InputType InputRelease()
		{
			var ht = Script.HookThread;

			// Input should already have ended prior to this function being called.
			// Otherwise, removal of aInput from the chain will end input collection.
			if (Script.input == this)
			{
				Script.input = Prev;
			}
			else
			{
				for (var input = Script.input; ; input = input.Prev)
				{
					if (input == null)
						return null; // aInput is not valid (faked AHK_INPUT_END message?) or not active.

					if (input.Prev == this)
					{
						input.Prev = Prev;
						break;
					}
				}
			}

			// Ensure any pending use of aInput by the hook is finished.
			ht.WaitHookIdle();
			Prev = null;

			if (ScriptObject != null)
			{
				HotkeyDefinition.MaybeUninstallHook();

				if (ScriptObject.OnEnd != null)
					return this; // Return for caller to call OnEnd and Release.

				//Original called Release() on ScriptObject, and the comments specifically differentiate between that and setting to null.
				//So just don't do anything here.
				// The following is not done because this Release() is only to counteract an AddRef() in
				// InputStart().  ScriptObject != NULL indicates this input_type is actually embedded in
				// the InputObject and as such the link should never be broken until both are deleted.
				//aInput->ScriptObject = NULL;
				Script.ExitIfNotPersistent(Flow.ExitReasons.Exit); // In case this InputHook was the only thing keeping the script running.
			}

			return null;
		}

		internal void InputStart()
		{
			// Set or update the timeout timer if needed.  The timer proc takes care to end
			// only those inputs which are due, and will reset or kill the timer as needed.
			if (Timeout > 0)
				SetTimeoutTimer();

			Prev = Script.input;
			Start();
			Script.input = this; // Signal the hook to start the input.
			HotkeyDefinition.InstallKeybdHook(); // Install the hook (if needed).
		}

		internal bool IsInteresting(ref KBDLLHOOKSTRUCT ev)
		{
			char? ch = null;
			return MinSendLevel == 0 ? true : KeyboardMouseSender.HotInputLevelAllowsFiring(MinSendLevel - 1, (uint)ev.dwExtraInfo, ref ch);
		}

		internal void ParseOptions(string options)
		{
			for (var i = 0; i < options.Length; i++)
			{
				switch (char.ToUpper(options[i]))
				{
					case 'B':
						BackspaceIsUndo = false;
						break;

					case 'C':
						CaseSensitive = true;
						break;

					case 'I':
						MinSendLevel = (options[i + 1] <= '9' && options[i + 1] >= '0') ? options[i + 1].ParseInt(false).Value : 1;
						break;

					case 'M':
						TranscribeModifiedKeys = true;
						break;

					case 'L':
						// Use atoi() vs. ATOI() to avoid interpreting something like 0x01C as hex
						// when in fact the C was meant to be an option letter:
						BufferLengthMax = options[i + 1].ParseInt(false).Value;

						if (BufferLengthMax < 0)
							BufferLengthMax = 0;

						break;

					case 'T':
						var sub = options.Substring(i + 1);
						Timeout = sub.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? sub.ParseInt().Value : (int)(options[i + 1].ParseDouble() * 1000);
						break;

					case 'V':
						VisibleText = true;
						VisibleNonText = true;
						break;

					case '*':
						FindAnywhere = true;
						break;

					case 'E':
						// Interpret single-character keys as characters rather than converting them to VK codes.
						// This tends to work better when using multiple keyboard layouts, but changes behavior:
						// for instance, an end char of "." cannot be triggered while holding Alt.
						EndCharMode = true;
						break;
				}
			}
		}

		internal void SetKeyFlags(string keys, bool endKeyMode = true, int flagsRemove = 0, int flagsAdd = HookThread.END_KEY_ENABLED)
		{
			//While this may have been easier and more concise to do C# style, there are extremely hard to decipher details in the original which make exact duplication
			//of the behavior very unlikely. So it's copied verbatim and ported to ensure consistent functionality.
			int? modifiersLR = 0;
			int keyTextLength;
			var singleCharCount = 0;
			//TCHAR* end_pos, single_char_string[2];
			var endPos = 0;
			var singleCharString = "";
			var endcharMode = endKeyMode && EndCharMode;
			var vk = 0;
			var sc = 0;
			var vkByNumber = false;
			bool? scByNumber = false;
			var ht = Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;//This should always be non-null if any hotkeys/strings are present.

			for (var i = 0; i < keys.Length; ++i) // This a modified version of the processing loop used in SendKeys().
			{
				vk = 0; // Set default.  Not strictly necessary but more maintainable.
				singleCharString = "";  // Set default as "this key name is not a single-char string".
				var ch = keys[i];
				var sub = keys.Substring(i + 1);

				switch (ch)
				{
					case '}': continue;  // Important that these be ignored.

					case '{':
					{
						endPos = sub.IndexOf('}');

						if (endPos == -1)
							continue;  // Do nothing, just ignore the unclosed '{' and continue.

						if ((keyTextLength = endPos - i - 1) == 0)
						{
							if (sub.Length > 1 && sub[1] == '}') // The string "{}}" has been encountered, which is interpreted as a single "}".
							{
								++endPos;
								keyTextLength = 1;
							}
							else // Empty braces {} were encountered.
								continue;  // do nothing: let it proceed to the }, which will then be ignored.
						}

						if (keyTextLength == 1) // A single-char key name, such as {.} or {{}.
						{
							if (endcharMode) // Handle this single-char key name by char code, not by VK.
							{
								// Although it might be sometimes useful to treat "x" as a character and "{x}" as a key,
								// "{{}" and "{}}" can't be included without the extra braces.  {vkNN} can still be used
								// to handle the key by VK instead of by character.
								singleCharCount++;
								continue; // It will be processed by another section.
							}

							singleCharString = keys[i + 1].ToString(); // Only used when vk != 0.
						}

						//end_pos = '\0';  // temporarily terminate the string here.
						scByNumber = false; // Set default.
						modifiersLR = 0;  // Init prior to below.
						// Handle the key by VK if it was given by number, such as {vk26}.
						// Otherwise, for any key name which has a VK shared by two possible SCs
						// (such as Up and NumpadUp), handle it by SC so it's identified correctly.
						var nextkey = keys[i + 1].ToString();
						vk = ht.TextToVK(nextkey, ref modifiersLR, true, true, WindowsAPI.GetKeyboardLayout(0));

						if (vk != 0)
						{
							vkByNumber = sub.StartsWith("vk", StringComparison.OrdinalIgnoreCase);

							if (!vkByNumber && (sc = ht.MapVkToSc(vk, true)) != 0)
							{
								sc ^= 0x100; // Convert sc to the primary scan code, which is the one named by end_key.
								vk = 0; // Handle it only by SC.
							}
						}
						else
							// No virtual key, so try to find a scan code.
							sc = ht.TextToSC(nextkey, ref scByNumber);

						i = endPos;  // In prep for ++i at the top of the loop.
						break; // Break out of the switch() and do the vk handling beneath it (if there is a vk).
					}

					default:
						if (endcharMode)
						{
							singleCharCount++;
							continue; // It will be processed by another section.
						}

						singleCharString = ch.ToString();
						modifiersLR = 0;  // Init prior to below.
						vk = ht.TextToVK(singleCharString, ref modifiersLR, true, true, WindowsAPI.GetKeyboardLayout(0));
						vkByNumber = false;
						break;
				} // switch()

				if (vk != 0) // A valid virtual key code was discovered above.
				{
					// Insist the shift key be down to form genuinely different symbols --
					// namely punctuation marks -- but not for alphabetic chars.
					if (singleCharString.Length == 1 && endKeyMode && !char.IsLetter(singleCharString[0])) // v1.0.46.05: Added check for "*single_char_string" so that non-single-char strings like {F9} work as end keys even when the Shift key is being held down (this fixes the behavior to be like it was in pre-v1.0.45).
					{
						// Now we know it's not alphabetic, and it's not a key whose name
						// is longer than one char such as a function key or numpad number.
						// That leaves mostly just the number keys (top row) and all
						// punctuation chars, which are the ones that we want to be
						// distinguished between shifted and unshifted:
						if ((modifiersLR & (KeyboardMouseSender.MOD_LSHIFT | KeyboardMouseSender.MOD_RSHIFT)) != 0)
							KeyVK[vk] |= HookThread.END_KEY_WITH_SHIFT;
						else
							KeyVK[vk] |= HookThread.END_KEY_WITHOUT_SHIFT;
					}
					else
					{
						KeyVK[vk] = (KeyVK[vk] & ~flagsRemove) | flagsAdd;
						// Apply flag removal to this key's SC as well.  This is primarily
						// to support combinations like {All} +E, {LCtrl}{RCtrl} -E.
						int temp_sc;

						if (flagsRemove != 0 && !vkByNumber && (temp_sc = ht.MapVkToSc(vk)) != 0)
						{
							KeySC[temp_sc] &= ~flagsRemove; // But apply aFlagsAdd only by VK.
							// Since aFlagsRemove implies ScriptObject != NULL and !vk_by_number
							// was also checked, that implies vk_to_sc(vk, true) was already called
							// and did not find a secondary SC.
						}
					}
				}

				if (sc != 0 || scByNumber.IsTrue()) // Fixed for v1.1.33.02: Allow sc000 for setting/unsetting flags for any events that lack a scan code.
				{
					KeySC[sc] = (KeySC[sc] & ~flagsRemove) | flagsAdd;
				}
			} // for()

			if (singleCharCount != 0)  // See single_char_count++ above for comments.
			{
				for (var i = 0; i < keys.Length; ++i)
				{
					var ch = keys[i];

					if (ch == '{')
					{
						var sub = keys.Substring(i);
						endPos = sub.IndexOf('}');

						if (endPos != -1)
						{
							if (endPos == 1 && sub[1] == '}') // {}}
								endPos++;

							if (endPos == 2)
								EndChars += keys[1]; // Copy the single character from between the braces.

							i = endPos; // Skip '{key'.  Loop does ++src to skip the '}'.
						}
					}
					else if (ch == '}')// Otherwise, just ignore the '{'.
						continue;

					EndChars += ch;
				}
			}
			else if (endKeyMode) // single_char_count is false
			{
				EndChars = "";
			}
		}

		internal void SetTimeoutTimer()
		{
			var now = DateTime.Now;
			TimeoutAt = now.AddMilliseconds(Timeout);

			if (!Script.inputTimerExists || Timeout < (Script.inputTimeoutAt - now).TotalMilliseconds)
			{
				Script.inputTimeoutAt = TimeoutAt;

				if (inputTimer == null)
				{
					inputTimer = new System.Windows.Forms.Timer();
					inputTimer.Tick += InputTimer_Tick;
				}

				inputTimer.Interval = Timeout;
				inputTimer.Start();
				Script.inputTimerExists = true;
			}
		}

		internal void Start() => Status = InputStatusType.InProgress;

		internal void Stop() => EndByReason(InputStatusType.Off);

		private void EndByReason(InputStatusType aReason)
		{
			if (Script.HookThread is Keysharp.Core.Common.Threading.HookThread hook && hook.kbdMsSender != null)
				EndingMods = hook.kbdMsSender.modifiersLRLogical; // Not relevant to all end reasons, but might be useful anyway.

			Status = aReason;
			// It's done this way rather than calling InputRelease() directly...
			// ...so that we can rely on MsgSleep() to create a new thread for the OnEnd event.
			// ...because InputRelease() can't be called by the hook thread.
			// ...because some callers rely on the list not being broken by this call.
			//TODO
			//WindowsAPI.PostMessage(Keysharp.Scripting.Script.mainWindow.Handle, (uint)UserMessages.AHK_INPUT_END, new IntPtr(this), IntPtr.Zero);
			Script.mainWindow.CheckedBeginInvoke(() =>
			{
				if (InputRelease() is InputType it)
				{
					var so = it.ScriptObject;
					//Threads.LaunchInThread(so.OnEnd, new object[] { so });//Why would we invoke this on the main window then launch another thread?
					_ = so.OnEnd.Call(so);
					//Original called Release() on so, but unsure what the equivalent of that would be here.
				}
			});
		}

		private void InputTimer_Tick(object sender, EventArgs e)
		{
			inputTimer.Stop();
			var newTimerPeriod = 0;

			for (var input = Script.input; input != null; input = input.Prev)
			{
				if (input.Timeout != 0 && input.InProgress())
				{
					var timeLeft = (int)(input.TimeoutAt - DateTime.Now).TotalMilliseconds;

					if (timeLeft <= 0)
						input.EndByTimeout();
					else if (timeLeft < newTimerPeriod || newTimerPeriod == 0)
						newTimerPeriod = timeLeft;
				}
			}

			if (newTimerPeriod != 0)
			{
				inputTimer.Interval = newTimerPeriod;
				Script.inputTimeoutAt = DateTime.Now.AddMilliseconds(newTimerPeriod);
				inputTimer.Start();
			}
			else
			{
				if (Script.inputTimerExists)
				{
					inputTimer.Stop();
					Script.inputTimerExists = false;
				}
			}
		}
	}

	internal enum InputStatusType
	{
		Off,
		InProgress,
		TimedOut,
		TerminatedByMatch,
		TerminatedByEndKey,
		LimitReached
	}
}