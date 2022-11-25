using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Keysharp.Core.Common.Threading;

namespace Keysharp.Core.Common.Keyboard
{
	public class HotstringDefinition
	{
		internal const int HOTSTRING_BLOCK_SIZE = 1024;
		internal const int HS_BUF_DELETE_COUNT = (HS_BUF_SIZE / 2);
		internal const int HS_BUF_SIZE = (MAX_HOTSTRING_LENGTH * 2 + 10);
		internal const int HS_MAX_END_CHARS = 100;
		internal const int HS_SUSPENDED = 0x01;
		internal const int HS_TEMPORARILY_DISABLED = 0x04;
		internal const int HS_TURNED_OFF = 0x02;
		internal const int MAX_HOTSTRING_LENGTH = 40;
		internal const string MAX_HOTSTRING_LENGTH_STR = "40";      // Hard to imagine a need for more than this, and most are only a few chars long.
		internal static string defEndChars = "-()[]{}:;'\"/\\,.?!\r\n \t";//Should this be a platform specific newline instead of \r\n? //Make this a static as the default.//MATT
		internal static uint enabledCount;      // Keep in sync with the above.
		internal static bool hsCaseSensitive;
		internal static bool hsConformToCase = true;
		internal static bool hsDetectWhenInsideWord;
		internal static bool hsDoBackspace = true;
		internal static bool hsDoReset;
		internal static bool hsEndCharRequired = true;
		internal static int hsKeyDelay;
		internal static bool hsOmitEndChar;
		internal static int hsPriority;
		internal static SendModes hsSendMode = SendModes.Input;
		internal static SendRawModes hsSendRaw = SendRawModes.NotRaw;
		internal static bool hsSuspendExempt;
		internal static List<HotstringDefinition> shs = new List<HotstringDefinition>(256);
		internal static Dictionary<string, List<HotstringDefinition>> sksDkt = new Dictionary<string, List<HotstringDefinition>>(StringComparer.OrdinalIgnoreCase);     //Should probably eventually make this a dictionary of some sort to avoid iterating over the whole list on every keypress.//TODO
		internal bool caseSensitive, conformToCase, doBackspace, omitEndChar, endCharRequired
		, detectWhenInsideWord, doReset, suspendExempt, constructedOK;
		internal uint existingThreads, maxThreads;
		internal IFuncObj funcObj;
		internal HotkeyCriterion hotCriterion;
		internal uint inputLevel;
		internal int priority, keyDelay;
		internal SendModes sendMode;
		internal Keysharp.Core.Common.Keyboard.SendRawModes sendRaw;
		internal string str, replacement;
		internal int suspended;
		protected internal static List<char> hsBuf = new List<char>(256);

		public static string CurrentInputBuffer => new string(hsBuf.ToArray());
		public static bool DefaultHotstringCaseSensitive => hsCaseSensitive;
		public static bool DefaultHotstringConformToCase => hsConformToCase;
		public static bool DefaultHotstringDetectWhenInsideWord => hsDetectWhenInsideWord;
		public static bool DefaultHotstringDoBackspace => hsDoBackspace;
		public static bool DefaultHotstringDoReset => hsDoReset;
		public static bool DefaultHotstringEndCharRequired => hsEndCharRequired;
		public static string DefaultHotstringEndChars => defEndChars;
		public static int DefaultHotstringKeyDelay => hsKeyDelay;
		public static bool DefaultHotstringOmitEndChar => hsOmitEndChar;
		public static int DefaultHotstringPriority => hsPriority;
		public static SendModes DefaultHotstringSendMode => hsSendMode;
		public static SendRawModes DefaultHotstringSendRaw => hsSendRaw;
		public static bool DefaultHotstringSuspendExempt => hsSuspendExempt;
		public bool Enabled { get; set; }
		public Options EnabledOptions { get; set; }
		public string Name { get; set; }
		public string Replacement { get; set; } = string.Empty;
		public string Sequence { get; }

		public HotstringDefinition(string sequence, string replacement)
		{
			Sequence = sequence;
			Replacement = replacement;
			//EndChars = defEndChars;
		}

		internal HotstringDefinition(string _name, /*Core.HotFunction*/IFuncObj _funcObj, string _options, string _hotstring, string _replacement
									 , bool _hasContinuationSection, int _suspend)

		{
			funcObj = _funcObj;
			hotCriterion = Keysharp.Scripting.Script.hotCriterion;
			suspended = _suspend;
			maxThreads = Keysharp.Scripting.Parser.MaxThreadsPerHotkey;  // The value of g_MaxThreadsPerHotkey can vary during load-time.
			priority = hsPriority;
			keyDelay = hsKeyDelay;
			sendMode = hsSendMode;  // And all these can vary too.
			caseSensitive = hsCaseSensitive;
			conformToCase = hsConformToCase;
			doBackspace = hsDoBackspace;
			omitEndChar = hsOmitEndChar;
			sendRaw = _hasContinuationSection ? Keysharp.Core.Common.Keyboard.SendRawModes.RawText : hsSendRaw;
			endCharRequired = hsEndCharRequired;
			detectWhenInsideWord = hsDetectWhenInsideWord;
			doReset = hsDoReset;
			inputLevel = Keysharp.Scripting.Script.inputLevel;
			suspendExempt = Keysharp.Scripting.Parser.SuspendExempt || DefaultHotstringSuspendExempt;
			constructedOK = false;
			var executeAction = false; // do not assign  mReplacement if execute_action is true.
			ParseOptions(_options, ref priority, ref keyDelay, ref sendMode, ref caseSensitive, ref conformToCase, ref doBackspace
						 , ref omitEndChar, ref sendRaw, ref endCharRequired, ref detectWhenInsideWord, ref doReset, ref executeAction, ref suspendExempt);
			str = _hotstring;
			Name = _name;

			if (!executeAction && !string.IsNullOrEmpty(_replacement))
				replacement = _replacement;
			else // Leave mReplacement NULL, but make this false so that the hook doesn't do extra work.
				conformToCase = false;

			constructedOK = true; // Done at the very end.
		}

		/// <summary>
		/// Returns OK or FAIL.
		/// Caller has ensured that aHotstringOptions is blank if there are no options.  Otherwise, aHotstringOptions
		/// should end in a colon, which marks the end of the options list.  aHotstring is the hotstring itself
		/// (e.g. "ahk"), which does not have to be unique, unlike aName, which was made unique by also including
		/// any options (e.g. ::ahk:: has a different aName than :c:ahk::).
		/// Caller has also ensured that aHotstring is not blank.
		/// </summary>
		public static ResultType AddHotstring(string _name, /*Core.HotFunction*/IFuncObj _funcObj, string _options, string _hotstring
											  , string _replacement, bool _hasContinuationSection, int _suspend = 0)
		{
			var hs = new HotstringDefinition(_name, _funcObj, _options, _hotstring, _replacement, _hasContinuationSection, _suspend);

			if (!hs.constructedOK)
				return ResultType.Fail;

			shs.Add(hs);

			if (!Keysharp.Scripting.Script.isReadyToExecute) // Caller is LoadIncludedFile(); allow BIF_Hotstring to manage this at runtime.
				++enabledCount; // This works because the script can't be suspended during startup (aSuspend is always FALSE).

			return ResultType.Ok;
		}

		public static void ClearHotstrings()
		{
			hsBuf.Clear();
			shs.Clear();
		}

		public void DefaultHotFunction(object[] o)
		{
			if (o.Length > 1 && o[0] is string typed && o[1] is string replace)
			{
				Console.WriteLine($"typed: \"{typed}\" sending: \"{replace}\"");
				Keysharp.Core.Keyboard.Send(replace);//Perhaps make use of the other fields later.//MATT
			}
		}

		public override string ToString() => Name;

		internal static string ClearBuf()
		{
			var str = new string(hsBuf.ToArray());
			hsBuf.Clear();
			return str;
		}

		internal static HotstringDefinition FindHotstring(string _hotstring, bool _caseSensitive, bool _detectWhenInsideWord, HotkeyCriterion _hotCriterion)
		{
			foreach (var hs in shs)//Would really prefer to put this in a dictionary.//TODO
			{
				if (hs.CompareHotstring(_hotstring, _caseSensitive, _detectWhenInsideWord, _hotCriterion))
					return hs;
			}

			return null;
		}

		internal static void ParseOptions(string _options, ref int _priority, ref int _keyDelay, ref SendModes _sendMode
										  , ref bool _caseSensitive, ref bool _conformToCase, ref bool _doBackspace, ref bool _omitEndChar, ref Keysharp.Core.Common.Keyboard.SendRawModes _sendRaw
										  , ref bool _endCharRequired, ref bool _detectWhenInsideWord, ref bool _doReset, ref bool _executeAction, ref bool _suspendExempt)
		{
			// In this case, colon rather than zero marks the end of the string.  However, the string
			// might be empty so check for that too.  In addition, this is now called from
			// IsDirective(), so that's another reason to check for normal string termination.
			var firstColon = _options.IndexOf(':');

			foreach (var split in firstColon == -1
					 ? _options.Split(Core.SpaceColon, StringSplitOptions.TrimEntries)
					 : _options.Substring(0, firstColon).Split(Core.SpaceColon, StringSplitOptions.TrimEntries))
			{
				if (split.Length == 0)
					continue;

				var next = split.Length > 1 ? split.Substring(1) : "";

				switch (char.ToUpper(split[0]))
				{
					case '*':
						_endCharRequired = next == "0";
						break;

					case '?':
						_detectWhenInsideWord = next != "0";
						break;

					case 'B':
						_doBackspace = next != "0";
						break;

					case 'C':
						if (next == "0") // restore both settings to default.
						{
							_conformToCase = true;
							_caseSensitive = false;
						}
						else if (next == "1")
						{
							_conformToCase = false;
							_caseSensitive = false;
						}
						else // treat as plain "C"
						{
							_conformToCase = false;  // No point in conforming if its case sensitive.
							_caseSensitive = true;
						}

						break;

					case 'O':
						_omitEndChar = next != "0";
						break;

					// For options such as K & P: Use atoi() vs. ATOI() to avoid interpreting something like 0x01C
					// as hex when in fact the C was meant to be an option letter:
					case 'K':
					{
						if (int.TryParse(next, out var val))
							_keyDelay = val;
					}
					break;

					case 'P':
					{
						if (int.TryParse(next, out var val))
							_priority = val;
					}
					break;

					case 'R':
						_sendRaw = (next != "0") ? SendRawModes.Raw : SendRawModes.NotRaw;
						break;

					case 'T':
						_sendRaw = (next != "0") ? SendRawModes.RawText : SendRawModes.NotRaw;
						break;

					case 'S':
					{
						switch (next.Length > 0 ? next[0] : (char)0)
						{
							// There is no means to choose SM_INPUT because it seems too rarely desired (since auto-replace
							// hotstrings would then become interruptible, allowing the keystrokes of fast typists to get
							// interspersed with the replacement text).
							case 'I': _sendMode = SendModes.InputThenPlay; break;

							case 'E': _sendMode = SendModes.Event; break;

							case 'P': _sendMode = SendModes.Play; break;

							default: _suspendExempt = next != "0"; break;
						}
					}
					break;

					case 'Z':
						_doReset = next != "0";
						break;

					case 'X':
						_executeAction = next != "0";
						break;
						// Otherwise: Ignore other characters, such as the digits that comprise the number after the P option.
				}
			}
		}

		internal static void SuspendAll(bool _suspend)
		{
			if (shs.Count < 1) // At least one part below relies on this check.
				return;

			int u;

			if (_suspend) // Suspend all those that aren't exempt.
			{
				// Recalculating sEnabledCount might perform better in the average case since most aren't exempt.
				for (u = 0, enabledCount = 0; u < shs.Count; ++u)
					if (shs[u].suspendExempt)
					{
						shs[u].suspended &= ~HS_SUSPENDED;

						if (shs[u].suspended == 0) // Not turned off.
							++enabledCount;
					}
					else
						shs[u].suspended |= HS_SUSPENDED;
			}
			else // Unsuspend all.
			{
				var previous_count = enabledCount;

				// Recalculating enabledCount is probably best since we otherwise need to both remove HS_SUSPENDED
				// and determine if the final suspension status has changed (i.e. no other bits were set).
				for (enabledCount = 0, u = 0; u < shs.Count; ++u)
				{
					shs[u].suspended &= ~HS_SUSPENDED;

					if (shs[u].suspended == 0) // Not turned off.
						++enabledCount;
				}

				// v1.0.44.08: Added the following section.  Also, the HS buffer is reset, but only when hotstrings
				// are newly enabled after having been entirely disabled.  This is because CollectInput() would not
				// have been called in a long time, making the contents of g_HSBuf obsolete, which in turn might
				// otherwise cause accidental firings based on old keystrokes coupled with new ones.
				if (previous_count == 0 && enabledCount > 0)
					hsBuf.Clear();
			}
		}

		internal bool CompareHotstring(string _hotstring, bool _caseSensitive, bool _detectWhenInsideWord, HotkeyCriterion _hotCriterion)
		{
			// hs.mEndCharRequired is not checked because although it affects the conditions for activating
			// the hotstring, ::abbrev:: and :*:abbrev:: cannot co-exist (the latter would always take over).
			return hotCriterion == _hotCriterion // Same #HotIf criterion.
				   && caseSensitive == _caseSensitive // ::BTW:: and :C:BTW:: can co-exist.
				   && detectWhenInsideWord == _detectWhenInsideWord // :?:ion:: and ::ion:: can co-exist.
				   && (_caseSensitive ? str == _hotstring : string.Compare(str, _hotstring, true) == 0);// :C:BTW:: and :C:btw:: can co-exist, but not ::BTW:: and ::btw::.
		}

		internal void DoReplace(CaseConformModes caseMode, char endChar)
		{
			var sb = new StringBuilder();//This might be able to be done more efficiently, but use sb unless performance issues show up.
			var startOfReplacement = 0;
			string sendBuf = "";
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			if (doBackspace)
			{
				// Subtract 1 from backspaces because the final key pressed by the user to make a
				// match was already suppressed by the hook (it wasn't sent through to the active
				// window).  So what we do is backspace over all the other keys prior to that one,
				// put in the replacement text (if applicable), then send the EndChar through
				// (if applicable) to complete the sequence.
				var backspaceCount = str.Length - 1;

				if (endCharRequired)
					++backspaceCount;

				for (var i = 0; i < backspaceCount; ++i)
				{
					_ = sb.Append('\b');  // Use raw backspaces, not {BS n}, in case the send will be raw.
					startOfReplacement++;
				}
			}

			if (!string.IsNullOrEmpty(replacement))
			{
				_ = sb.Append(replacement);

				if (caseMode == CaseConformModes.AllCaps)
				{
					sendBuf = sb.ToString().ToUpper();
					_ = sb.Clear();
					_ = sb.Append(sendBuf);
				}
				else if (caseMode == CaseConformModes.FirstCap)
				{
					var b = false;
					sendBuf = sb.ToString();
					_ = sb.Clear();

					for (var i = 0; i < sendBuf.Length; i++)
					{
						if (i < startOfReplacement)
							_ = sb.Append(sendBuf[i]);
						else if (b)
							_ = sb.Append(sendBuf[i]);
						else if (!b)
						{
							_ = sb.Append(char.ToUpper(sendBuf[i]));
							b = true;
						}
					}
				}

				if (!omitEndChar) // The ending character (if present) needs to be sent too.
				{
					// Send the final character in raw mode so that chars such as !{} are sent properly.
					// v1.0.43: Avoid two separate calls to SendKeys because:
					// 1) It defeats the uninterruptibility of the hotstring's replacement by allowing the user's
					//    buffered keystrokes to take effect in between the two calls to SendKeys.
					// 2) Performance: Avoids having to install the playback hook twice, etc.
					if (endCharRequired && endChar != 0) // Must now check mEndCharRequired because LOWORD has been overloaded with context-sensitive meanings.
					{
						// v1.0.43.02: Don't send "{Raw}" if already in raw mode!
						// v1.1.27: Avoid adding {Raw} if it gets switched on within the replacement text.
						if (sendRaw != 0 || replacement.Contains("{Raw}", StringComparison.OrdinalIgnoreCase) || replacement.Contains("{Text}", StringComparison.OrdinalIgnoreCase))
							_ = sb.Append(endChar);
						else
							_ = sb.Append(string.Format("{0}{1}", "{Raw}", endChar));
					}
				}
			}

			sendBuf = sb.ToString();

			if (sendBuf.Length == 0) // No keys to send.
				return;

			// For the following, mSendMode isn't checked because the backup/restore is needed to varying extents
			// by every mode.
			var oldDelay = Accessors.A_KeyDelay;
			var oldPressDuration = Accessors.A_KeyDuration;
			var oldDelayPlay = Accessors.A_KeyDelayPlay;
			var oldPressDurationPlay = Accessors.A_KeyDurationPlay;
			var oldSendLevel = Accessors.A_SendLevel;
			Accessors.A_KeyDelay = keyDelay; // This is relatively safe since SendKeys() normally can't be interrupted by a new thread.
			Accessors.A_KeyDuration = -1;   // Always -1, since Send command can be used in body of hotstring to have a custom press duration.
			Accessors.A_KeyDelayPlay = -1;
			Accessors.A_KeyDurationPlay = keyDelay; // Seems likely to be more useful (such as in games) to apply mKeyDelay to press duration rather than above.
			// Setting the SendLevel to 0 rather than this->mInputLevel since auto-replace hotstrings are used for text replacement rather than
			// key remapping, which means the user almost always won't want the generated input to trigger other hotkeys or hotstrings.
			// Action hotstrings (not using auto-replace) do get their thread's SendLevel initialized to the hotstring's InputLevel.
			Accessors.A_SendLevel = 0;

			// v1.0.43: The following section gives time for the hook to pass the final keystroke of the hotstring to the
			// system.  This is necessary only for modes other than the original/SendEvent mode because that one takes
			// advantage of the serialized nature of the keyboard hook to ensure the user's final character always appears
			// on screen before the replacement text can appear.
			// By contrast, when the mode is SendPlay (and less frequently, SendInput), the system and/or hook needs
			// another timeslice to ensure that AllowKeyToGoToSystem() actually takes effect on screen (SuppressThisKey()
			// doesn't seem to have this problem).
			if (!(doBackspace || omitEndChar) && sendMode != SendModes.Event) // The final character of the abbreviation (or its EndChar) was not suppressed by the hook.
				System.Threading.Thread.Sleep(0);

			kbdMouseSender.SendKeys(sendBuf, sendRaw, sendMode, IntPtr.Zero); // Send the backspaces and/or replacement.
			// Restore original values.
			Accessors.A_KeyDelay = oldDelay;
			Accessors.A_KeyDuration = oldPressDuration;
			Accessors.A_KeyDelayPlay = oldDelayPlay;
			Accessors.A_KeyDurationPlay = oldPressDurationPlay;
			Accessors.A_SendLevel = oldSendLevel;
		}

		internal void ParseOptions(string aOptions)
		{
			var unused_X_option = false;
			ParseOptions(aOptions, ref priority, ref keyDelay, ref sendMode, ref caseSensitive, ref conformToCase, ref doBackspace
						 , ref omitEndChar, ref sendRaw, ref endCharRequired, ref detectWhenInsideWord, ref doReset, ref unused_X_option, ref suspendExempt);
		}

		/// <summary>
		/// Returns OK or FAIL.  Caller has already ensured that the backspacing (if specified by mDoBackspace)
		/// has been done.  Caller must have already created a new thread for us, and must close the thread when
		/// we return.
		/// </summary>
		internal async Task<ResultType> PerformInNewThreadMadeByCaller()
		{
			if (existingThreads >= maxThreads)
				return ResultType.Fail;

			// See Hotkey::Perform() for details about this.  For hot strings -- which also use the
			// g_script.mThisHotkeyStartTime value to determine whether g_script.mThisHotkeyModifiersLR
			// is still timely/accurate -- it seems best to set to "no modifiers":
			KeyboardMouseSender.thisHotkeyModifiersLR = 0;
			++existingThreads;  // This is the thread count for this particular hotstring only.

			try
			{
				var tsk = await Threads.LaunchInThread(funcObj, new object[] { /*Keysharp.Scripting.Script.thisHotkeyName, */Name });//Only need to pass Name. thisHotkeyName was passed by the original just for debugging.
			}
			catch (Error ex)
			{
				_ = Keysharp.Core.Dialogs.MsgBox($"Exception thrown during hotstring handler.\n\n{ex}");
			}
			finally
			{
				--existingThreads;
			}

			return ResultType.Ok;
		}

		//Need key delay, text, priority, send style, execute (X), and make sure raw actually works.
		[Flags]
		public enum Options
		{ None = 0, AutoTrigger = 1, Nested = 2, Backspace = 4, CaseSensitive = 8, OmitEnding = 16, Raw = 32, Reset = 64 }
	}

	internal enum CaseConformModes
	{ None, AllCaps, FirstCap };
}