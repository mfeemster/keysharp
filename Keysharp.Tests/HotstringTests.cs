using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace Keysharp.Tests
{
	/// <summary>
	/// All hotstring tests must be run sequentially, hence the usage of lock (syncroot).
	/// </summary>
	public partial class Scripting
	{
		private static bool btwtyped = false;

		public static object label_9F201721(params object[] args)
		{
			btwtyped = true;
			return string.Empty;
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void ChangeDefaultOptions()
		{
			//End char required.
			var newVal = false;
			var origVal = Accessors.A_DefaultHotstringEndCharRequired;
			Assert.AreEqual(origVal, !newVal);
			var oldVal = Keysharp.Core.Keyboard.Hotstring("*:");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringEndCharRequired);
			Assert.AreEqual(Accessors.A_DefaultHotstringEndCharRequired, newVal);
			Assert.AreEqual(null, oldVal);
			//Case sensitivity.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("C");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringCaseSensitive);
			Assert.AreEqual(Accessors.A_DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual(null, oldVal);
			//Case sensitivity restore to default.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("C0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringCaseSensitive);
			Assert.AreEqual(Accessors.A_DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual(null, oldVal);
			//Inside word.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringDetectWhenInsideWord;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("?");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDetectWhenInsideWord);
			Assert.AreEqual(Accessors.A_DefaultHotstringDetectWhenInsideWord, newVal);
			Assert.AreEqual(null, oldVal);
			//Automatic backspacing off.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("B0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoBackspace);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual(null, oldVal);
			//Automatic backspacing back on.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("B");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoBackspace);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual(null, oldVal);
			//Do not conform to typed case.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringConformToCase;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("C1");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringConformToCase);
			Assert.AreEqual(Accessors.A_DefaultHotstringConformToCase, newVal);
			Assert.AreEqual(null, oldVal);
			//Omit ending character.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("O");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringOmitEndChar);
			Assert.AreEqual(Accessors.A_DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual(null, oldVal);
			//Restore ending character.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("O0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringOmitEndChar);
			Assert.AreEqual(Accessors.A_DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual(null, oldVal);
			//Exempt from suspend.
			newVal = true;
			origVal = Accessors.A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("S");
			Assert.AreNotEqual(origVal, Accessors.A_SuspendExempt.Ab());
			Assert.AreEqual(Accessors.A_SuspendExempt.Ab(), newVal);
			Assert.AreEqual(null, oldVal);
			//Remove suspend exempt.
			newVal = false;
			origVal = Accessors.A_SuspendExempt.Ab();
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("S0");
			Assert.AreNotEqual(origVal, Accessors.A_SuspendExempt.Ab());
			Assert.AreEqual(Accessors.A_SuspendExempt.Ab(), newVal);
			Assert.AreEqual(null, oldVal);
			//Reset on trigger.
			newVal = true;
			origVal = Accessors.A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("Z");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoReset);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoReset, newVal);
			Assert.AreEqual(null, oldVal);
			//Restore reset on trigger.
			newVal = false;
			origVal = Accessors.A_DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("Z0");
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringDoReset);
			Assert.AreEqual(Accessors.A_DefaultHotstringDoReset, newVal);
			Assert.AreEqual(null, oldVal);
			//Send replacement text raw.
			var newMode = SendRawModes.Raw.ToString();
			var origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("R");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw.ToString();
			origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.Raw.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("R0");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Send replacement text mode.
			newMode = SendRawModes.RawText.ToString();
			origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("T");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw.ToString();
			origMode = Accessors.A_DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.RawText.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("T0");
			Assert.AreNotEqual(origMode, Accessors.A_DefaultHotstringSendRaw);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Key delay.
			var newInt = 42;
			var origInt = Accessors.A_DefaultHotstringKeyDelay;
			Assert.AreEqual(origInt, 0);
			oldVal = Keysharp.Core.Keyboard.Hotstring($"K{newInt}");
			Assert.AreNotEqual(origInt, Accessors.A_DefaultHotstringKeyDelay);
			Assert.AreEqual(Accessors.A_DefaultHotstringKeyDelay, newInt);
			Assert.AreEqual(null, oldVal);
			//Priority.
			newInt = 42;
			origInt = Accessors.A_DefaultHotstringPriority;
			Assert.AreEqual(origInt, 0);
			oldVal = Keysharp.Core.Keyboard.Hotstring($"P{newInt}");
			Assert.AreNotEqual(origInt, Accessors.A_DefaultHotstringPriority);
			Assert.AreEqual(Accessors.A_DefaultHotstringPriority, newInt);
			Assert.AreEqual(null, oldVal);
			//Send mode Event.
			var newSendMode = SendModes.Event.ToString();
			var origSendMode = Accessors.A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Input.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("SE");
			Assert.AreNotEqual(origSendMode, Accessors.A_DefaultHotstringSendMode);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual(null, oldVal);
			//Send mode Play.
			newSendMode = SendModes.Play.ToString();
			origSendMode = Accessors.A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Event.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("SP");
			Assert.AreNotEqual(origSendMode, Accessors.A_DefaultHotstringSendMode);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual(null, oldVal);
			//Send mode Input.
			newSendMode = SendModes.Input.ToString();
			origSendMode = Accessors.A_DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Play.ToString());
			oldVal = Keysharp.Core.Keyboard.Hotstring("SI");
			Assert.AreNotEqual(origSendMode, Accessors.A_DefaultHotstringSendMode);
			Assert.AreEqual(Accessors.A_DefaultHotstringSendMode, SendModes.InputThenPlay.ToString());//InputThenPlay gets used when Input is specified. See HotstringDefinition.ParseOptions().
			Assert.AreEqual(null, oldVal);
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void ChangeEndChars()
		{
			var newVal = "newendchars";
			var origVal = Accessors.A_DefaultHotstringEndChars;
			Assert.AreEqual(origVal, "-()[]{}:;'\"/\\,.?!\r\n \t");
			var oldVal = Keysharp.Core.Keyboard.Hotstring("EndChars", newVal);
			Assert.AreNotEqual(origVal, Accessors.A_DefaultHotstringEndChars);
			Assert.AreEqual(Accessors.A_DefaultHotstringEndChars, newVal);
			Assert.AreEqual(origVal, oldVal);
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void HotstringDirectives()
		{
			Assert.IsTrue(TestScript("hotstring-directives", false));
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void HotstringParsing()
		{
			Assert.IsTrue(TestScript("hotkey-hotstring-parsing", false));
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void CreateHotstring()
		{
			//Can't seem to simulate uppercase here, so we can't test case sensitive hotstrings.
			btwtyped = false;
			_ = Keysharp.Core.Keyboard.Hotstring("Reset");
			_ = Keysharp.Core.Common.Keyboard.HotstringDefinition.AddHotstring("::btw", Keysharp.Core.Misc.FuncObj("label_9F201721", null), ":btw", "btw", "", false);
			Keysharp.Core.Common.Keyboard.HotkeyDefinition.ManifestAllHotkeysHotstringsHooks();
			Keysharp.Scripting.Script.SimulateKeyPress((uint)System.Windows.Forms.Keys.B);
			Keysharp.Scripting.Script.SimulateKeyPress((uint)System.Windows.Forms.Keys.T);
			Keysharp.Scripting.Script.SimulateKeyPress((uint)System.Windows.Forms.Keys.W);
			Keysharp.Scripting.Script.SimulateKeyPress((uint)System.Windows.Forms.Keys.Enter);
			System.Threading.Thread.Sleep(2000);
			Assert.AreEqual(btwtyped, true);
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void ResetInputBuffer()
		{
			_ = Keysharp.Core.Keyboard.Hotstring("Reset");
			var tester = new HotstringDefinitionTester("tester", "");
			tester.AddChars("asdf");
			var origVal = HotstringDefinition.CurrentInputBuffer;
			Assert.AreEqual(origVal, "asdf");
			origVal = Keysharp.Core.Keyboard.Hotstring("Reset") as string;
			Assert.AreEqual(origVal, "asdf");
			var newVal = HotstringDefinition.CurrentInputBuffer;
			Assert.AreNotEqual(origVal, newVal);
			Assert.AreEqual(newVal, "");
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void ResetOnMouseClick()
		{
			var newVal = true;
			var origVal = Script.ResetUponMouseClick;
			Assert.AreEqual(origVal, false);
			var oldVal = Keysharp.Core.Keyboard.Hotstring("MouseReset", newVal);
			Assert.AreNotEqual(origVal, Script.ResetUponMouseClick);
			Assert.AreEqual(Script.ResetUponMouseClick, newVal);
			Assert.AreEqual(origVal, oldVal);
		}
	}

	internal class HotstringDefinitionTester : HotstringDefinition
	{
		public HotstringDefinitionTester(string sequence, string replacement)
			: base(sequence, replacement)
		{
		}

		public void AddChars(string s)
		{
			foreach (var ch in s)
				hsBuf.Add(ch);
		}
	}
}