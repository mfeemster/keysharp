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
			HotstringManager.ClearHotstrings();
			_ = Keyboard.Hotstring("Reset");

			foreach (var hotstring in hotstrings)
			{
				var splits = hotstring.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				var split0 = splits[0].Substring(splits[0].IndexOf('(') + 1).Trim('"');
				var split3 = splits[3].Trim('"');
				hs1 = HotstringManager.AddHotstring(split0, null, splits[2].Trim('"'), split3, splits[4].Trim('"'), false);
				Debug.WriteLine(split0);

				if (!split0.Contains('*'))
					val = split3 + " ";
				else
					val = split3;

				HotstringManager.AddChars(val);
				hs2 = HotstringManager.MatchHotstring();//Test as is.
				Assert.AreEqual(hs1, hs2);
				//
				_ = Keyboard.Hotstring("Reset");
				HotstringManager.AddChars(Guid.NewGuid() + " " + val);//Test with text before it.
				hs2 = HotstringManager.MatchHotstring();
				Assert.AreEqual(hs1, hs2);
				_ = Keyboard.Hotstring("Reset");
				hs2 = HotstringManager.MatchHotstring();
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
			//End char required.
			var newVal = false;
			var origVal = Accessors.A_DefaultHotstringEndCharRequired;
			Assert.AreEqual(origVal, !newVal);
			var oldVal = Keyboard.Hotstring("*:");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringEndCharRequired);
			Assert.AreEqual(Accessors.A_DefaultHotstringEndCharRequired, newVal);
			Assert.AreEqual(null, oldVal);
			//Case sensitivity.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("C");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringCaseSensitive);
			Assert.AreEqual(Accessors.A_DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual(null, oldVal);
			//Case sensitivity restore to default.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("C0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringCaseSensitive);
			Assert.AreEqual(Accessors.A_DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual(null, oldVal);
			//Inside word.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringDetectWhenInsideWord;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("?");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDetectWhenInsideWord);
			Assert.AreEqual(Accessors.A_DefaultHotstringDetectWhenInsideWord, newVal);
			Assert.AreEqual(null, oldVal);
			//Automatic backspacing off.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("B0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoBackspace);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual(null, oldVal);
			//Automatic backspacing back on.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("B");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoBackspace);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual(null, oldVal);
			//Do not conform to typed case.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringConformToCase;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("C1");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringConformToCase);
			Assert.AreEqual(Accessors.A_DefaultHotstringConformToCase, newVal);
			Assert.AreEqual(null, oldVal);
			//Omit ending character.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("O");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringOmitEndChar);
			Assert.AreEqual(Accessors.A_DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual(null, oldVal);
			//Restore ending character.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("O0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringOmitEndChar);
			Assert.AreEqual(Accessors.A_DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual(null, oldVal);
			//Exempt from suspend.
			newVal = true;
			origVal = Accessors.A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("S");
			Assert.AreNotEqual(origVal, Accessors.A_SuspendExempt.Ab());
			Assert.AreEqual(Accessors.A_SuspendExempt.Ab(), newVal);
			Assert.AreEqual(null, oldVal);
			//Remove suspend exempt.
			newVal = false;
			origVal = Accessors.A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("S0");
			Assert.AreNotEqual(origVal, Accessors.A_SuspendExempt.Ab());
			Assert.AreEqual(Accessors.A_SuspendExempt.Ab(), newVal);
			Assert.AreEqual(null, oldVal);
			//Reset on trigger.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("Z");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoReset);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoReset, newVal);
			Assert.AreEqual(null, oldVal);
			//Restore reset on trigger.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keyboard.Hotstring("Z0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoReset);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoReset, newVal);
			Assert.AreEqual(null, oldVal);
			//Send replacement text raw.
			var newMode = SendRawModes.Raw.ToString();
			var origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw.ToString());
			oldVal = Keyboard.Hotstring("R");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw.ToString();
			origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.Raw.ToString());
			oldVal = Keyboard.Hotstring("R0");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Send replacement text mode.
			newMode = SendRawModes.RawText.ToString();
			origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw.ToString());
			oldVal = Keyboard.Hotstring("T");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw.ToString();
			origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.RawText.ToString());
			oldVal = Keyboard.Hotstring("T0");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Key delay.
			var newInt = 42;
			var origInt = Accessors.A_DefaultHotstringKeyDelay;
			Assert.AreEqual(origInt, 0);
			oldVal = Keyboard.Hotstring($"K{newInt}");
			Assert.AreNotEqual(origInt, Accessors.A_DefaultHotstringKeyDelay);
			Assert.AreEqual(Accessors.A_DefaultHotstringKeyDelay, newInt);
			Assert.AreEqual(null, oldVal);
			//Priority.
			newInt = 42;
			origInt = Accessors.A_DefaultHotstringPriority;
			Assert.AreEqual(origInt, 0);
			oldVal = Keyboard.Hotstring($"P{newInt}");
			Assert.AreNotEqual(origInt, Accessors.A_DefaultHotstringPriority);
			Assert.AreEqual(Accessors.A_DefaultHotstringPriority, newInt);
			Assert.AreEqual(null, oldVal);
			//Send mode Event.
			var newSendMode = SendModes.Event.ToString();
			var origSendMode = Accessors.A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Input.ToString());
			oldVal = Keyboard.Hotstring("SE");
			Assert.AreNotEqual(origSendMode, Accessors.A_DefaultHotstringSendMode);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual(null, oldVal);
			//Send mode Play.
			newSendMode = SendModes.Play.ToString();
			origSendMode = Accessors.A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Event.ToString());
			oldVal = Keyboard.Hotstring("SP");
			Assert.AreNotEqual(origSendMode, Accessors.A_DefaultHotstringSendMode);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual(null, oldVal);
			//Send mode Input.
			newSendMode = SendModes.Input.ToString();
			origSendMode = Accessors.A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Play.ToString());
			oldVal = Keyboard.Hotstring("SI");
			Assert.AreNotEqual(origSendMode, Accessors.A_DefaultHotstringSendMode);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, SendModes.InputThenPlay.ToString());//InputThenPlay gets used when Input is specified. See HotstringDefinition.ParseOptions().
			Assert.AreEqual(null, oldVal);
			//Try changing multiple options at once.
			//First reset everything back to the default state.
			_ = Keyboard.Hotstring("*0");
			origVal = Accessors.A_DefaultHotstringEndCharRequired;
			Assert.AreEqual(origVal, true);
			_ = Keyboard.Hotstring("C0");
			origVal = Accessors.A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("?0");
			origVal = Accessors.A_DefaultHotstringDetectWhenInsideWord;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("B");
			origVal = Accessors.A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, true);
			_ = Keyboard.Hotstring("O0");
			origVal = Accessors.A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("S0");
			origVal = Accessors.A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("Z0");
			origVal = Accessors.A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, false);
			_ = Keyboard.Hotstring("R0");
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, SendRawModes.NotRaw.ToString());
			_ = Keyboard.Hotstring("T0");
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, SendRawModes.NotRaw.ToString());
			_ = Keyboard.Hotstring("K-1");
			Assert.AreEqual(Accessors.A_DefaultHotstringKeyDelay, -1L);
			_ = Keyboard.Hotstring("P-1");
			Assert.AreEqual(Accessors.A_DefaultHotstringPriority, -1L);
			_ = Keyboard.Hotstring("SI");
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, SendModes.InputThenPlay.ToString());
			//Now test a multi-option string.
			_ = Keyboard.Hotstring("*?CB0OSZRK123P10");
			Assert.AreEqual(Accessors.A_DefaultHotstringEndCharRequired, false);
			Assert.AreEqual(Accessors.A_DefaultHotstringDetectWhenInsideWord, true);
			Assert.AreEqual(Accessors.A_DefaultHotstringCaseSensitive, true);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoBackspace, false);
			Assert.AreEqual(Accessors.A_DefaultHotstringOmitEndChar, true);
			Assert.AreEqual(Accessors.A_SuspendExempt, true);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoReset, true);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, SendRawModes.Raw.ToString());
			Assert.AreEqual(Accessors.A_DefaultHotstringKeyDelay, 123L);
			Assert.AreEqual(Accessors.A_DefaultHotstringPriority, 10L);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void ChangeEndChars()
		{
			var newVal = "newendchars";
			var origVal = Accessors.A_DefaultHotstringEndChars;
			Assert.AreEqual(origVal, "-()[]{}:;'\"/\\,.?!\r\n \t");
			var oldVal = Keyboard.Hotstring("EndChars", newVal);
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringEndChars);
			Assert.AreEqual(Accessors.A_DefaultHotstringEndChars, newVal);
			Assert.AreEqual(origVal, oldVal);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void CreateHotstring()
		{
			//Can't seem to simulate uppercase here, so we can't test case sensitive hotstrings.
			btwtyped = false;
			HotstringManager.ClearHotstrings();
			_ = Keyboard.Hotstring("Reset");
			_ = HotstringManager.AddHotstring("::btw", Functions.FuncObj("label_9F201721", null), ":btw", "btw", "", false);
			HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
			Assert.IsTrue(Accessors.A_KeybdHookInstalled == 1);
			Assert.IsTrue(Accessors.A_MouseHookInstalled == 1);//Because there is a hotstring and mouse reset is true by default, the mouse hook gets installed.
			Script.SimulateKeyPress((uint)Keys.B);
			Script.SimulateKeyPress((uint)Keys.T);
			Script.SimulateKeyPress((uint)Keys.W);
			Script.SimulateKeyPress((uint)Keys.Enter);
			Thread.Sleep(2000);
			Assert.AreEqual(btwtyped, true);
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void HotstringDirectives()
		{
			Assert.IsTrue(TestScript("hotstring-directives", false));
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void HotstringParsing()
		{
			Assert.IsTrue(TestScript("hotkey-hotstring-parsing", false));
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void HotstringParsing2()
		{
			var filename = string.Format("hotstring-parsing2", Path.DirectorySeparatorChar);
			_ = TestScript(filename, false);
			//After the script exits, the hotstrings are still kept in memory in the global list.
			//So query them below to ensure they were properly parsed.
			_ = Keyboard.Hotstring("Reset");
			HotstringManager.AddChars("btw ");
			var hs = HotstringManager.MatchHotstring();
			Assert.AreEqual(hs.Name, "::btw");
			Assert.AreEqual(hs.Replacement, "by the way");
			_ = Keyboard.Hotstring("Reset");
			//
			HotstringManager.AddChars("1 ");
			hs = HotstringManager.MatchHotstring();
			Assert.AreEqual(hs.Name, "::1");
			Assert.AreEqual(hs.Replacement, ":2");
			_ = Keyboard.Hotstring("Reset");
			//
			HotstringManager.AddChars("3 ");
			hs = HotstringManager.MatchHotstring();
			Assert.AreEqual(hs.Name, "::3");
			Assert.AreEqual(hs.Replacement, "::4");
			_ = Keyboard.Hotstring("Reset");
			//
			HotstringManager.AddChars("5: ");
			hs = HotstringManager.MatchHotstring();
			Assert.AreEqual(hs.Name, "::5:");
			Assert.AreEqual(hs.Replacement, "6");
			_ = Keyboard.Hotstring("Reset");
			//
			HotstringManager.AddChars("7: ");
			hs = HotstringManager.MatchHotstring();
			Assert.AreEqual(hs.Name, "::7:");
			Assert.AreEqual(hs.Replacement, ":8");
			_ = Keyboard.Hotstring("Reset");
			HotstringManager.ClearHotstrings();
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
			HotstringManager.AddChars("asdf");
			var origVal = HotstringManager.CurrentInputBuffer;
			Assert.AreEqual(origVal, "asdf");
			origVal = Keyboard.Hotstring("Reset") as string;
			Assert.AreEqual(origVal, "asdf");
			var newVal = HotstringManager.CurrentInputBuffer;
			Assert.AreNotEqual(origVal, newVal);
			Assert.AreEqual(newVal, "");
		}

		[Test, Category("Hotstring"), NonParallelizable]
		public void ResetOnMouseClick()
		{
			var newVal = false;
			var origVal = Accessors.A_HotstringNoMouse;
			Assert.AreEqual(origVal, false);
			var oldVal = Keyboard.Hotstring("MouseReset", newVal);
			Assert.AreNotEqual(origVal, Accessors.A_HotstringNoMouse);
			Assert.AreEqual(Accessors.A_HotstringNoMouse, !newVal);
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
            HotstringManager.ClearHotstrings();
        }
	}
}