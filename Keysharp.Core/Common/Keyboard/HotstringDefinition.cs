using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Keysharp.Core.Common.Threading;
using Keysharp.Scripting;
using static Keysharp.Core.Misc;

namespace Keysharp.Core.Common.Keyboard
{
	public class HotstringDefinition
	{
		internal const int HOTSTRING_BLOCK_SIZE = 1024;
		internal const int HS_BUF_DELETE_COUNT = HS_BUF_SIZE / 2;
		internal const int HS_BUF_SIZE = (MAX_HOTSTRING_LENGTH * 2) + 10;
		internal const int HS_MAX_END_CHARS = 100;
		internal const int HS_SUSPENDED = 0x01;
		internal const int HS_TEMPORARILY_DISABLED = 0x04;
		internal const int HS_TURNED_OFF = 0x02;
		internal const int MAX_HOTSTRING_LENGTH = 40;
		internal const string MAX_HOTSTRING_LENGTH_STR = "40";      // Hard to imagine a need for more than this, and most are only a few chars long.
		internal static string defEndChars = "-()[]{}:;'\"/\\,.?!\r\n \t";//Should this be a platform specific newline instead of \r\n?//TODO
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

		//internal static Dictionary<string, List<HotstringDefinition>> shsDkt = new Dictionary<string, List<HotstringDefinition>>(StringComparer.OrdinalIgnoreCase);     //Should probably eventually make this a dictionary of some sort to avoid iterating over the whole list on every keypress.//TODO
		internal bool caseSensitive, conformToCase, doBackspace, omitEndChar, endCharRequired
		, detectWhenInsideWord, doReset, suspendExempt, constructedOK;

		internal uint existingThreads, maxThreads;
		internal IFuncObj funcObj;
		internal IFuncObj hotCriterion;
		internal uint inputLevel;
		internal int priority, keyDelay;
		internal SendModes sendMode;
		internal Keysharp.Core.Common.Keyboard.SendRawModes sendRaw;
		internal string str, replacement;
		internal int suspended;
		protected internal static List<char> hsBuf = new List<char>(256);

		[PublicForTestOnly]
		public static string CurrentInputBuffer => new string(hsBuf.ToArray());

		[PublicForTestOnly]
		public bool Enabled { get; set; }

		[PublicForTestOnly]
		public Options EnabledOptions { get; set; }

		[PublicForTestOnly]
		public string Name { get; set; }

		[PublicForTestOnly]
		public string Replacement { get; set; } = string.Empty;

		[PublicForTestOnly]
		public string Sequence { get; }

		public HotstringDefinition(string sequence, string replacement)
		{
			Sequence = sequence;
			Replacement = replacement;
			//EndChars = defEndChars;
		}

		internal HotstringDefinition(string _name, IFuncObj _funcObj, string _options, string _hotstring, string _replacement
									 , bool _hasContinuationSection, int _suspend)

		{
			funcObj = _funcObj;
			hotCriterion = Threads.GetThreadVariables().hotCriterion;
			suspended = _suspend;
			maxThreads = Accessors.A_MaxThreadsPerHotkey.Aui();  // The value of g_MaxThreadsPerHotkey can vary during load-time.
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
			inputLevel = (uint)Accessors.A_InputLevel;
			suspendExempt = Accessors.A_SuspendExempt.Ab();
			constructedOK = false;
			var unusedX = false; // do not assign  mReplacement if execute_action is true.
			ParseOptions(_options, ref priority, ref keyDelay, ref sendMode, ref caseSensitive, ref conformToCase, ref doBackspace
						 , ref omitEndChar, ref sendRaw, ref endCharRequired, ref detectWhenInsideWord, ref doReset, ref unusedX, ref suspendExempt);
			str = _hotstring;
			Name = _name;

			if (!string.IsNullOrEmpty(_replacement))
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
		public static ResultType AddHotstring(string _name, IFuncObj _funcObj, string _options, string _hotstring
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

		public override string ToString() => Name;

		internal static string ClearBuf()
		{
			var str = new string(hsBuf.ToArray());
			hsBuf.Clear();
			return str;
		}

		internal static HotstringDefinition FindHotstring(string _hotstring, bool _caseSensitive, bool _detectWhenInsideWord, IFuncObj _hotCriterion)
		{
			foreach (var hs in shs)
				if (hs.CompareHotstring(_hotstring, _caseSensitive, _detectWhenInsideWord, _hotCriterion))
					return hs;

			return null;
		}

		internal static void ParseOptions(string _options, ref int _priority, ref int _keyDelay, ref SendModes _sendMode
										  , ref bool _caseSensitive, ref bool _conformToCase, ref bool _doBackspace, ref bool _omitEndChar, ref Keysharp.Core.Common.Keyboard.SendRawModes _sendRaw
										  , ref bool _endCharRequired, ref bool _detectWhenInsideWord, ref bool _doReset, ref bool _executeAction, ref bool _suspendExempt)
		{
			// In this case, colon rather than zero marks the end of the string.  However, the string
			// might be empty so check for that too.  In addition, this is now called from
			// IsDirective(), so that's another reason to check for normal string termination.
			for (var i = 0; i < _options.Length && _options[i] != ':'; i++)
			{
				var ch = char.ToUpper(_options[i]);
				var next = _options.Length > i ? _options.AsSpan(i + 1) : "";

				switch (ch)
				{
					case '*':
						_endCharRequired = next.Length > 0 && next[0] == '0';
						break;

					case '?':
						_detectWhenInsideWord = next.Length == 0 || next[0] != '0';
						break;

					case 'B':
						_doBackspace = next.Length == 0 || next[0] != '0';
						break;

					case 'C':
						if (next.Length == 0)// treat as plain "C"
						{
							_conformToCase = false;  // No point in conforming if its case sensitive.
							_caseSensitive = true;
						}
						else if (next[0] == '0') // restore both settings to default.
						{
							_conformToCase = true;
							_caseSensitive = false;
						}
						else if (next[0] == '1')
						{
							_conformToCase = false;
							_caseSensitive = false;
						}

						break;

					case 'O':
						_omitEndChar = next.Length == 0 || next[0] != '0';
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
						_sendRaw = (next.Length == 0 || next[0] != '0') ? SendRawModes.Raw : SendRawModes.NotRaw;
						break;

					case 'T':
						_sendRaw = (next.Length == 0 || next[0] != '0') ? SendRawModes.RawText : SendRawModes.NotRaw;
						break;

					case 'S':
					{
						var tempch = (char)0;

						if (next.Length > 0)
						{
							tempch = char.ToUpper(next[0]);
							++i; // Skip over S's sub-letter (if any) to exclude it from  further consideration.
						}

						switch (tempch)
						{
							// There is no means to choose SM_INPUT because it seems too rarely desired (since auto-replace
							// hotstrings would then become interruptible, allowing the keystrokes of fast typists to get
							// interspersed with the replacement text).
							case 'I': _sendMode = SendModes.InputThenPlay; break;

							case 'E': _sendMode = SendModes.Event; break;

							case 'P': _sendMode = SendModes.Play; break;

							default: _suspendExempt = tempch != '0'; break;
						}
					}
					break;

					case 'Z':
						_doReset = next.Length == 0 || next[0] != '0';
						break;

					case 'X':
						_executeAction = next.Length == 0 || next[0] != '0';
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

		internal bool AnyThreadsAvailable() => existingThreads < maxThreads;

		internal bool CompareHotstring(string _hotstring, bool _caseSensitive, bool _detectWhenInsideWord, IFuncObj _hotCriterion)
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
			string sendBuf;
			var ht = Keysharp.Scripting.Script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			if (doBackspace)
			{
				var backspaceCount = str.Length;

				for (var i = 0; i < str.Length; i++)
					if (char.IsSurrogatePair(str, i))
					{
						i++;
						backspaceCount--;
					}

				// Subtract 1 from backspaces because the final key pressed by the user to make a
				// match was already suppressed by the hook (it wasn't sent through to the active
				// window).  So what we do is backspace over all the other keys prior to that one,
				// put in the replacement text (if applicable), then send the EndChar through
				// (if applicable) to complete the sequence.

				if (!endCharRequired)
					--backspaceCount;

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
			var tv = Threads.GetThreadVariables();
			var oldDelay = tv.keyDelay;
			var oldPressDuration = tv.keyDuration;
			var oldDelayPlay = tv.keyDelayPlay;
			var oldPressDurationPlay = tv.keyDurationPlay;
			var oldSendLevel = tv.sendLevel;
			tv.keyDelay = keyDelay; // This is relatively safe since SendKeys() normally can't be interrupted by a new thread.
			tv.keyDuration = -1L;   // Always -1, since Send command can be used in body of hotstring to have a custom press duration.
			tv.keyDelayPlay = -1L;
			tv.keyDurationPlay = keyDelay; // Seems likely to be more useful (such as in games) to apply mKeyDelay to press duration rather than above.
			// Setting the SendLevel to 0 rather than this->mInputLevel since auto-replace hotstrings are used for text replacement rather than
			// key remapping, which means the user almost always won't want the generated input to trigger other hotkeys or hotstrings.
			// Action hotstrings (not using auto-replace) do get their thread's SendLevel initialized to the hotstring's InputLevel.
			tv.sendLevel = 0u;

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
			tv.keyDelay = oldDelay;
			tv.keyDuration = oldPressDuration;
			tv.keyDelayPlay = oldDelayPlay;
			tv.keyDurationPlay = oldPressDurationPlay;
			tv.sendLevel = oldSendLevel;
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
		internal ResultType PerformInNewThreadMadeByCaller(IntPtr hwndCritFound, string endChar)
		{
			if (!Threads.AnyThreadsAvailable())//First test global thread count.
				return ResultType.Fail;

			if (!AnyThreadsAvailable())//Then test local thread count.
				return ResultType.Fail;

			var tv = Threads.GetThreadVariables();

			if (priority < tv.priority)//Finally, test priority.
				return ResultType.Fail;

			VariadicFunction vf = (o) =>
			{
				object ret = null;
				var tv = Threads.GetThreadVariables();
				tv.sendLevel = inputLevel;
				tv.hwndLastUsed = hwndCritFound;
				tv.hotCriterion = hotCriterion;// v2: Let the Hotkey command use the criterion of this hotstring by default.

				try
				{
					ret = funcObj.Call(o);
				}
				catch (Error ex)
				{
					_ = Keysharp.Core.Dialogs.MsgBox($"Exception thrown during hotstring handler.\n\n{ex}", null, (int)MessageBoxIcon.Hand);
				}

				_ = Interlocked.Decrement(ref existingThreads);
				return ret;
			};

			try
			{
				//var tsk = await Threads.LaunchInThread(priority, false, false, vf, new object[] { Name });
				// See Hotkey::Perform() for details about this.  For hot strings -- which also use the
				// g_script.mThisHotkeyStartTime value to determine whether g_script.mThisHotkeyModifiersLR
				// is still timely/accurate -- it seems best to set to "no modifiers":
				KeyboardMouseSender.thisHotkeyModifiersLR = 0;
				Accessors.A_EndChar = endCharRequired ? endChar : ""; // v1.0.48.04: Explicitly set 0 when hs->mEndCharRequired==false because LOWORD is used for something else in that case.
				Script.SetHotNamesAndTimes(Name);
				_ = Interlocked.Increment(ref existingThreads);//This is the thread count for this particular hotstring only.
				Threads.LaunchInThread(priority, false, false, vf, new object[] { Name });
			}
			catch (Error ex)
			{
				_ = Keysharp.Core.Dialogs.MsgBox($"Exception thrown during hotstring handler.\n\n{ex}", null, (int)MessageBoxIcon.Hand);
			}

			return ResultType.Ok;
		}

		[Flags]
		public enum Options
		{ None = 0, AutoTrigger = 1, Nested = 2, Backspace = 4, CaseSensitive = 8, OmitEnding = 16, Raw = 32, Reset = 64 }
	}

	internal enum CaseConformModes
	{ None, AllCaps, FirstCap };
}