using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	/// <summary>
	/// All hotstring tests must be run sequentially, hence the usage of lock (syncroot).
	/// </summary>
	public partial class HotstringTests : TestRunner
	{
		private static bool btwtyped = false;

		public static object label_9F201721(params object[] args)
		{
			btwtyped = true;
			return string.Empty;
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void AutoCorrect()
		{
			var val = "";
			HotstringDefinition hs1, hs2;
			var filename = string.Format("..{0}..{0}..{0}Keysharp.Tests{0}HotstringTests.txt", Path.DirectorySeparatorChar);
			var hotstrings = File.ReadLines(filename);
			var delimiters = new char[] { ',' };
			hsm.ClearHotstrings();
			hsm.RestoreDefaults(true);
			_ = Keyboard.Hotstring("Reset");

			foreach (var hotstring in hotstrings)
			{
				var splits = hotstring.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				var split0 = splits[0].Substring(splits[0].IndexOf('(') + 1).Trim('"');
				var split3 = splits[3].Trim('"');
				hs1 = (HotstringDefinition)Keysharp.Core.Common.Keyboard.HotstringManager.AddHotstring(split0, null, splits[2].Trim('"'), split3, splits[4].Trim('"'), false);
				//System.Diagnostics.Debug.WriteLine(split0);

				if (!split0.Contains('*'))
					val = split3 + " ";
				else
					val = split3;

				hsm.AddChars(val);
				hs2 = hsm.MatchHotstring();//Test as is.
				Assert.AreEqual(hs1, hs2);
				//
				_ = Keyboard.Hotstring("Reset");
				hsm.AddChars(Guid.NewGuid() + " " + val);//Test with text before it.
				hs2 = hsm.MatchHotstring();
				Assert.AreEqual(hs1, hs2);
				_ = Keyboard.Hotstring("Reset");
				hs2 = hsm.MatchHotstring();
				Assert.AreEqual(null, hs2);
				//Need to ensure the other tests with ? and * work.
				var opts = split0.Substring(1, split0.IndexOf(':', 1) - 1);
				var newOptsName = ":*B0OSZRK123P10:" + split3;//Change options except for ? and C.

				//Still need to do the rest of the autocorrect file here.//TODO
				if (opts.Contains('?'))
					_ = Keyboard.Hotstring("?");
				else
					_ = Keyboard.Hotstring("?0");

				if (opts.Contains('C'))
					_ = Keyboard.Hotstring("C");
				else
					_ = Keyboard.Hotstring("C0");

				var found = Keyboard.Hotstring(newOptsName) as HotstringDefinition;
				Assert.IsNotNull(found);
				Assert.AreEqual(found.EndCharRequired, false);
				Assert.AreEqual(found.DoBackspace, false);
				Assert.AreEqual(found.OmitEndChar, true);
				Assert.AreEqual(found.SuspendExempt, true);
				Assert.AreEqual(found.DoReset, true);
				Assert.AreEqual(found.SendRaw, SendRawModes.Raw);
				Assert.AreEqual(found.KeyDelay, 123L);
				Assert.AreEqual(found.Priority, 10L);
				_ = Keyboard.Hotstring("?0");
				_ = Keyboard.Hotstring("C0");
			}
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void ChangeDefaultOptions()
		{
			hsm.RestoreDefaults(true);
			//End char required.
			var newVal = false;
			var origVal = A_DefaultHotstringEndCharRequired;
			Assert.AreEqual(origVal, !newVal);
			var oldVal = Keyboard.Hotstring("*:");
			Assert.AreNotEqual(origVal, A_DefaultHotstringEndCharRequired);
			Assert.AreEqual(A_DefaultHotstringEndCharRequired, newVal);
			Assert.AreEqual("", oldVal);
			//Case sensitivity.
			newVal = true;
			origVal = A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("C");
			Assert.AreNotEqual(origVal, A_DefaultHotstringCaseSensitive);
			Assert.AreEqual(A_DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual("", oldVal);
			//Case sensitivity restore to default.
			newVal = false;
			origVal = A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("C0");
			Assert.AreNotEqual(origVal, A_DefaultHotstringCaseSensitive);
			Assert.AreEqual(A_DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual("", oldVal);
			//Inside word.
			newVal = true;
			origVal = A_DefaultHotstringDetectWhenInsideWord;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("?");
			Assert.AreNotEqual(origVal, A_DefaultHotstringDetectWhenInsideWord);
			Assert.AreEqual(A_DefaultHotstringDetectWhenInsideWord, newVal);
			Assert.AreEqual("", oldVal);
			//Automatic backspacing off.
			newVal = false;
			origVal = A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("B0");
			Assert.AreNotEqual(origVal, A_DefaultHotstringDoBackspace);
			Assert.AreEqual(A_DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual("", oldVal);
			//Automatic backspacing back on.
			newVal = true;
			origVal = A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("B");
			Assert.AreNotEqual(origVal, A_DefaultHotstringDoBackspace);
			Assert.AreEqual(A_DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual("", oldVal);
			//Do not conform to typed case.
			newVal = false;
			origVal = A_DefaultHotstringConformToCase;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("C1");
			Assert.AreNotEqual(origVal, A_DefaultHotstringConformToCase);
			Assert.AreEqual(A_DefaultHotstringConformToCase, newVal);
			Assert.AreEqual("", oldVal);
			//Omit ending character.
			newVal = true;
			origVal = A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("O");
			Assert.AreNotEqual(origVal, A_DefaultHotstringOmitEndChar);
			Assert.AreEqual(A_DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual("", oldVal);
			//Restore ending character.
			newVal = false;
			origVal = A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("O0");
			Assert.AreNotEqual(origVal, A_DefaultHotstringOmitEndChar);
			Assert.AreEqual(A_DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual("", oldVal);
			//Exempt from suspend.
			newVal = true;
			origVal = A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("S");
			Assert.AreNotEqual(origVal, A_SuspendExempt.Ab());
			Assert.AreEqual(A_SuspendExempt.Ab(), newVal);
			Assert.AreEqual("", oldVal);
			//Remove suspend exempt.
			newVal = false;
			origVal = A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("S0");
			Assert.AreNotEqual(origVal, A_SuspendExempt.Ab());
			Assert.AreEqual(A_SuspendExempt.Ab(), newVal);
			Assert.AreEqual("", oldVal);
			//Reset on trigger.
			newVal = true;
			origVal = A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("Z");
			Assert.AreNotEqual(origVal, A_DefaultHotstringDoReset);
			Assert.AreEqual(A_DefaultHotstringDoReset, newVal);
			Assert.AreEqual("", oldVal);
			//Restore reset on trigger.
			newVal = false;
			origVal = A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("Z0");
			Assert.AreNotEqual(origVal, A_DefaultHotstringDoReset);
			Assert.AreEqual(A_DefaultHotstringDoReset, newVal);
			Assert.AreEqual("", oldVal);
			//Send replacement text raw.
			var newMode = SendRawModes.Raw.ToString();
			var origMode = A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw.ToString());
			oldVal = Keyboard.Hotstring("R");
			Assert.AreNotEqual(origMode, A_DefaultHotstringSendRaw);
			Assert.AreEqual(A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual("", oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw.ToString();
			origMode = A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.Raw.ToString());
			oldVal = Keyboard.Hotstring("R0");
			Assert.AreNotEqual(origMode, A_DefaultHotstringSendRaw);
			Assert.AreEqual(A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual("", oldVal);
			//Send replacement text mode.
			newMode = SendRawModes.RawText.ToString();
			origMode = A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw.ToString());
			oldVal = Keyboard.Hotstring("T");
			Assert.AreNotEqual(origMode, A_DefaultHotstringSendRaw);
			Assert.AreEqual(A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual("", oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw.ToString();
			origMode = A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.RawText.ToString());
			oldVal = Keyboard.Hotstring("T0");
			Assert.AreNotEqual(origMode, A_DefaultHotstringSendRaw);
			Assert.AreEqual(A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual("", oldVal);
			//Key delay.
			var newInt = 42;
			var origInt = A_DefaultHotstringKeyDelay;
			Assert.AreEqual(origInt, 0);
			oldVal = Keyboard.Hotstring($"K{newInt}");
			Assert.AreNotEqual(origInt, A_DefaultHotstringKeyDelay);
			Assert.AreEqual(A_DefaultHotstringKeyDelay, newInt);
			Assert.AreEqual("", oldVal);
			//Priority.
			newInt = 42;
			origInt = A_DefaultHotstringPriority;
			Assert.AreEqual(origInt, 0);
			oldVal = Keyboard.Hotstring($"P{newInt}");
			Assert.AreNotEqual(origInt, A_DefaultHotstringPriority);
			Assert.AreEqual(A_DefaultHotstringPriority, newInt);
			Assert.AreEqual("", oldVal);
			//Send mode Event.
			var newSendMode = SendModes.Event.ToString();
			var origSendMode = A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Input.ToString());
			oldVal = Keyboard.Hotstring("SE");
			Assert.AreNotEqual(origSendMode, A_DefaultHotstringSendMode);
			Assert.AreEqual(A_DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual("", oldVal);
			//Send mode Play.
			newSendMode = SendModes.Play.ToString();
			origSendMode = A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Event.ToString());
			oldVal = Keyboard.Hotstring("SP");
			Assert.AreNotEqual(origSendMode, A_DefaultHotstringSendMode);
			Assert.AreEqual(A_DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual("", oldVal);
			//Send mode Input.
			newSendMode = SendModes.Input.ToString();
			origSendMode = A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Play.ToString());
			oldVal = Keyboard.Hotstring("SI");
			Assert.AreNotEqual(origSendMode, A_DefaultHotstringSendMode);
			Assert.AreEqual(A_DefaultHotstringSendMode, SendModes.InputThenPlay.ToString());//InputThenPlay gets used when Input is specified. See HotstringDefinition.ParseOptions().
			Assert.AreEqual("", oldVal);
			//Try changing multiple options at once.
			//First reset everything back to the default state.
			_ = Keyboard.Hotstring("*0");
			origVal = A_DefaultHotstringEndCharRequired;
			Assert.AreEqual(origVal, true);
			_ = Keyboard.Hotstring("C0");
			origVal = A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("?0");
			origVal = A_DefaultHotstringDetectWhenInsideWord;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("B");
			origVal = A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, true);
			_ = Keyboard.Hotstring("O0");
			origVal = A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("S0");
			origVal = A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("Z0");
			origVal = A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("R0");
			Assert.AreEqual(A_DefaultHotstringSendRaw, SendRawModes.NotRaw.ToString());
			_ = Keyboard.Hotstring("T0");
			Assert.AreEqual(A_DefaultHotstringSendRaw, SendRawModes.NotRaw.ToString());
			_ = Keyboard.Hotstring("K-1");
			Assert.AreEqual(A_DefaultHotstringKeyDelay, -1L);
			_ = Keyboard.Hotstring("P-1");
			Assert.AreEqual(A_DefaultHotstringPriority, -1L);
			_ = Keyboard.Hotstring("SI");
			Assert.AreEqual(A_DefaultHotstringSendMode, SendModes.InputThenPlay.ToString());
			//Now test a multi-option string.
			_ = Keyboard.Hotstring("*?CB0OSZRK123P10");
			Assert.AreEqual(A_DefaultHotstringEndCharRequired, false);
			Assert.AreEqual(A_DefaultHotstringDetectWhenInsideWord, true);
			Assert.AreEqual(A_DefaultHotstringCaseSensitive, true);
			Assert.AreEqual(A_DefaultHotstringDoBackspace, false);
			Assert.AreEqual(A_DefaultHotstringOmitEndChar, true);
			Assert.AreEqual(A_SuspendExempt, true);
			Assert.AreEqual(A_DefaultHotstringDoReset, true);
			Assert.AreEqual(A_DefaultHotstringSendRaw, SendRawModes.Raw.ToString());
			Assert.AreEqual(A_DefaultHotstringKeyDelay, 123L);
			Assert.AreEqual(A_DefaultHotstringPriority, 10L);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void ChangeEndChars()
		{
			hsm.RestoreDefaults(true);
			var newVal = "newendchars";
			var origVal = A_DefaultHotstringEndChars;
			Assert.AreEqual(origVal, "-()[]{}:;'\"/\\,.?!\r\n \t");
			var oldVal = Keyboard.Hotstring("EndChars", newVal);
			Assert.AreNotEqual(origVal, A_DefaultHotstringEndChars);
			Assert.AreEqual(A_DefaultHotstringEndChars, newVal);
			Assert.AreEqual(origVal, oldVal);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void CreateHotstring()
		{
			//Can't seem to simulate uppercase here, so we can't test case sensitive hotstrings.
			btwtyped = false;
			hsm.ClearHotstrings();
			hsm.RestoreDefaults(true);
			_ = Keyboard.Hotstring("Reset");
			_ = Keysharp.Core.Common.Keyboard.HotstringManager.AddHotstring("::btw", Functions.Func("label_9F201721", null), ":btw", "btw", "", false);
			_ = HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
			Assert.IsTrue(A_KeybdHookInstalled == 1L);//Will fail if system has another hook, so exit your scripts before running this.
			Assert.IsTrue(A_MouseHookInstalled == 1L);//Because there is a hotstring and mouse reset is true by default, the mouse hook gets installed.
			s.SimulateKeyPress((uint)Keys.B);
			s.SimulateKeyPress((uint)Keys.T);
			s.SimulateKeyPress((uint)Keys.W);
			s.SimulateKeyPress((uint)Keys.Enter);
			Thread.Sleep(2000);
			Assert.AreEqual(btwtyped, true);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void GetKey()
		{
			Assert.IsTrue(Keysharp.Core.Keyboard.GetKeySC("Esc") == 1L);
			Assert.IsTrue(Keysharp.Core.Keyboard.GetKeyVK("Esc") == 27L);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void HotstringDirectives()
		{
			Assert.IsTrue(TestScript("hotstring-directives", false));
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void HotstringParsing()
		{
			var trigger = "^;";
			var hk = EscapeHotkeyTrigger(trigger);
			Assert.AreEqual("^;", hk);
			//
			trigger = "`;";
			hk = EscapeHotkeyTrigger(trigger);
			Assert.AreEqual(";", hk);
			//
			trigger = ":";
			hk = EscapeHotkeyTrigger(trigger);
			Assert.AreEqual(":", hk);
			//
			trigger = "`";
			hk = EscapeHotkeyTrigger(trigger);
			Assert.AreEqual("`", hk);
			//
			trigger = "``";
			hk = EscapeHotkeyTrigger(trigger);
			Assert.AreEqual("`", hk);
			//
			trigger = "+`";
			hk = EscapeHotkeyTrigger(trigger);
			Assert.AreEqual("+`", hk);
			//
			Assert.IsTrue(TestScript("hotkey-hotstring-parsing", false));

			string EscapeHotkeyTrigger(ReadOnlySpan<char> s)
			{
				var escaped = false;
				var sb = new StringBuilder(s.Length);
				char ch = (char)0;

				for (var i = 0; i < s.Length; ++i)
				{
					ch = s[i];
					escaped = i == 0 && ch == '`';

					if (!escaped)
						sb.Append(ch);
				}

				if (escaped)
					sb.Append(ch);

				return sb.ToString();
			}
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void HotstringParsing2()
		{
			var filename = "hotstring-parsing2";
			_ = TestScript(filename, false);
			//After the script exits, the hotstrings are still kept in memory in the global list.
			//So query them below to ensure they were properly parsed.
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("bitw ");
			var hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::bitw");
			Assert.AreEqual(hs.Replacement, "biggest in the world");
			_ = Keyboard.Hotstring("Reset");
			//
			hsm.AddChars("1 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::1");
			Assert.AreEqual(hs.Replacement, ":2");
			_ = Keyboard.Hotstring("Reset");
			//
			hsm.AddChars("3 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::3");
			Assert.AreEqual(hs.Replacement, "::4");
			_ = Keyboard.Hotstring("Reset");
			//
			hsm.AddChars("5: ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::5:");
			Assert.AreEqual(hs.Replacement, "6");
			_ = Keyboard.Hotstring("Reset");
			//
			hsm.AddChars("7: ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::7:");
			Assert.AreEqual(hs.Replacement, ":8");
			_ = Keyboard.Hotstring("Reset");
			//
			var val = "Any text between the top and bottom parentheses is treated literally.\r\nBy default" +
					  ", the hard carriage return (Enter) between the previous line and this one is als" +
					  "o preserved.\r\n    By default, the indentation (tab) to the left of this line is " +
					  "preserved.";
			hsm.AddChars("text1 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::text1");
			Assert.AreEqual(hs.Replacement, val);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf1 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, ":X:mf1");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf2 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, ":X:mf2");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf3 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, ":X:mf3");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf4 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::mf4");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf5 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::mf5");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf6 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::mf6");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf7 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, ":X:mf7");
			Assert.AreEqual(hs.Replacement, null);
			//
			_ = Keyboard.Hotstring("Reset");
			hsm.AddChars("mf8 ");
			hs = hsm.MatchHotstring();
			Assert.AreEqual(hs.Name, "::mf8");
			Assert.AreEqual(hs.Replacement, null);
			//
			hsm.ClearHotstrings();
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void InputHookOptions()
		{
			var ih = Input.InputHook("B C H I10 M L1 T2 V * E");
			Assert.AreEqual(ih.BackspaceIsUndo, false);
			Assert.AreEqual(ih.CaseSensitive, true);
			Assert.AreEqual(ih.BeforeHotkeys, true);
			Assert.AreEqual(ih.MinSendLevel, 10u);
			Assert.AreEqual(ih.TranscribeModifiedKeys, true);
			Assert.AreEqual(ih.BufferLengthMax, 1);
			Assert.AreEqual(ih.Timeout, 2);
			Assert.AreEqual(ih.VisibleText, true);
			Assert.AreEqual(ih.VisibleNonText, true);
			Assert.AreEqual(ih.FindAnywhere, true);
			Assert.AreEqual(ih.EndCharMode, true);
			//
			ih = Input.InputHook("BCHI10ML123T2V*E");
			Assert.AreEqual(ih.BackspaceIsUndo, false);
			Assert.AreEqual(ih.CaseSensitive, true);
			Assert.AreEqual(ih.BeforeHotkeys, true);
			Assert.AreEqual(ih.MinSendLevel, 10u);
			Assert.AreEqual(ih.TranscribeModifiedKeys, true);
			Assert.AreEqual(ih.BufferLengthMax, 123);
			Assert.AreEqual(ih.Timeout, 2);
			Assert.AreEqual(ih.VisibleText, true);
			Assert.AreEqual(ih.VisibleNonText, true);
			Assert.AreEqual(ih.FindAnywhere, true);
			Assert.AreEqual(ih.EndCharMode, true);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void ResetInputBuffer()
		{
			hsm.AddChars("asdf");
			var origVal = hsm.CurrentInputBuffer;
			Assert.AreEqual(origVal, "asdf");
			origVal = Keyboard.Hotstring("Reset") as string;
			Assert.AreEqual(origVal, "asdf");
			var newVal = hsm.CurrentInputBuffer;
			Assert.AreNotEqual(origVal, newVal);
			Assert.AreEqual(newVal, "");
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void ResetOnMouseClick()
		{
			hsm.RestoreDefaults(true);
			var newVal = false;
			var origVal = A_DefaultHotstringNoMouse;
			Assert.AreEqual(origVal, false);
			var oldVal = Keyboard.Hotstring("MouseReset", newVal);
			Assert.AreNotEqual(origVal, A_DefaultHotstringNoMouse);
			Assert.AreEqual(A_DefaultHotstringNoMouse, !newVal);
			Assert.AreEqual(origVal.Ab(), !oldVal.Ab());
			//Reset to what it was for the sake of other tests in this class.
			_ = Keyboard.Hotstring("MouseReset", true);
		}

		[SetUp, Category("Hotstring")]
		public void Setup()
		{
			_ = Keyboard.Hotstring("*0");
			_ = Keyboard.Hotstring("C0");
			_ = Keyboard.Hotstring("?0");
			_ = Keyboard.Hotstring("B");
			_ = Keyboard.Hotstring("O0");
			_ = Keyboard.Hotstring("R0");
			_ = Keyboard.Hotstring("T0");
			_ = Keyboard.Hotstring("S0");
			//_ = Keyboard.Hotstring("SI");
			_ = Keyboard.Hotstring("Z0");
			_ = Keyboard.Hotstring("K0");
			_ = Keyboard.Hotstring("P0");
			_ = Keyboard.Hotstring("EndChars", "-()[]{}:;'\"/\\,.?!\r\n \t");
			hsm.RestoreDefaults(true);
			hsm.ClearHotstrings();
		}
	}
}