using Keysharp.Core;
using Keysharp.Core.Common.Keyboard;
using Keysharp.Core.Common.Threading;
using Keysharp.Scripting;
using NUnit.Framework;

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
			var origVal = HotstringDefinition.DefaultHotstringEndCharRequired;
			Assert.AreEqual(origVal, !newVal);
			var oldVal = Keysharp.Core.Keyboard.Hotstring("*:");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringEndCharRequired);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringEndCharRequired, newVal);
			Assert.AreEqual(null, oldVal);
			//Case sensitivity.
			newVal = true;
			origVal = HotstringDefinition.DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("C");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringCaseSensitive);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual(null, oldVal);
			//Case sensitivity restore to default.
			newVal = false;
			origVal = HotstringDefinition.DefaultHotstringCaseSensitive;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("C0");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringCaseSensitive);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringCaseSensitive, newVal);
			Assert.AreEqual(null, oldVal);
			//Inside word.
			newVal = true;
			origVal = HotstringDefinition.DefaultHotstringDetectWhenInsideWord;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("?");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringDetectWhenInsideWord);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringDetectWhenInsideWord, newVal);
			Assert.AreEqual(null, oldVal);
			//Automatic backspacing off.
			newVal = false;
			origVal = HotstringDefinition.DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("B0");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringDoBackspace);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual(null, oldVal);
			//Automatic backspacing back on.
			newVal = true;
			origVal = HotstringDefinition.DefaultHotstringDoBackspace;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("B");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringDoBackspace);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringDoBackspace, newVal);
			Assert.AreEqual(null, oldVal);
			//Do not conform to typed case.
			newVal = false;
			origVal = HotstringDefinition.DefaultHotstringConformToCase;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("C1");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringConformToCase);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringConformToCase, newVal);
			Assert.AreEqual(null, oldVal);
			//Omit ending character.
			newVal = true;
			origVal = HotstringDefinition.DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("O");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringOmitEndChar);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual(null, oldVal);
			//Restore ending character.
			newVal = false;
			origVal = HotstringDefinition.DefaultHotstringOmitEndChar;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("O0");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringOmitEndChar);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringOmitEndChar, newVal);
			Assert.AreEqual(null, oldVal);
			//Exempt from suspend.
			newVal = true;
			origVal = HotstringDefinition.DefaultHotstringSuspendExempt;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("S");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringSuspendExempt);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSuspendExempt, newVal);
			Assert.AreEqual(null, oldVal);
			//Remove suspend exempt.
			newVal = false;
			origVal = HotstringDefinition.DefaultHotstringSuspendExempt;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("S0");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringSuspendExempt);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSuspendExempt, newVal);
			Assert.AreEqual(null, oldVal);
			//Reset on trigger.
			newVal = true;
			origVal = HotstringDefinition.DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("Z");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringDoReset);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringDoReset, newVal);
			Assert.AreEqual(null, oldVal);
			//Restore reset on trigger.
			newVal = false;
			origVal = HotstringDefinition.DefaultHotstringDoReset;
			Assert.AreEqual(origVal, !newVal);
			oldVal = Keysharp.Core.Keyboard.Hotstring("Z0");
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringDoReset);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringDoReset, newVal);
			Assert.AreEqual(null, oldVal);
			//Send replacement text raw.
			var newMode = SendRawModes.Raw;
			var origMode = HotstringDefinition.DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw);
			oldVal = Keysharp.Core.Keyboard.Hotstring("R");
			Assert.AreNotEqual(origMode, HotstringDefinition.DefaultHotstringSendRaw);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw;
			origMode = HotstringDefinition.DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.Raw);
			oldVal = Keysharp.Core.Keyboard.Hotstring("R0");
			Assert.AreNotEqual(origMode, HotstringDefinition.DefaultHotstringSendRaw);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Send replacement text mode.
			newMode = SendRawModes.RawText;
			origMode = HotstringDefinition.DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.NotRaw);
			oldVal = Keysharp.Core.Keyboard.Hotstring("T");
			Assert.AreNotEqual(origMode, HotstringDefinition.DefaultHotstringSendRaw);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Restore replacement text mode.
			newMode = SendRawModes.NotRaw;
			origMode = HotstringDefinition.DefaultHotstringSendRaw;
			Assert.AreEqual(origMode, SendRawModes.RawText);
			oldVal = Keysharp.Core.Keyboard.Hotstring("T0");
			Assert.AreNotEqual(origMode, HotstringDefinition.DefaultHotstringSendRaw);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendRaw, newMode);
			Assert.AreEqual(null, oldVal);
			//Key delay.
			var newInt = 42;
			var origInt = HotstringDefinition.DefaultHotstringKeyDelay;
			Assert.AreEqual(origInt, 0);
			oldVal = Keysharp.Core.Keyboard.Hotstring($"K{newInt}");
			Assert.AreNotEqual(origInt, HotstringDefinition.DefaultHotstringKeyDelay);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringKeyDelay, newInt);
			Assert.AreEqual(null, oldVal);
			//Priority.
			newInt = 42;
			origInt = HotstringDefinition.DefaultHotstringPriority;
			Assert.AreEqual(origInt, 0);
			oldVal = Keysharp.Core.Keyboard.Hotstring($"P{newInt}");
			Assert.AreNotEqual(origInt, HotstringDefinition.DefaultHotstringPriority);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringPriority, newInt);
			Assert.AreEqual(null, oldVal);
			//Send mode Event.
			var newSendMode = SendModes.Event;
			var origSendMode = HotstringDefinition.DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Input);
			oldVal = Keysharp.Core.Keyboard.Hotstring("SE");
			Assert.AreNotEqual(origSendMode, HotstringDefinition.DefaultHotstringSendMode);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual(null, oldVal);
			//Send mode Play.
			newSendMode = SendModes.Play;
			origSendMode = HotstringDefinition.DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Event);
			oldVal = Keysharp.Core.Keyboard.Hotstring("SP");
			Assert.AreNotEqual(origSendMode, HotstringDefinition.DefaultHotstringSendMode);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendMode, newSendMode);
			Assert.AreEqual(null, oldVal);
			//Send mode Input.
			newSendMode = SendModes.Input;
			origSendMode = HotstringDefinition.DefaultHotstringSendMode;
			Assert.AreEqual(origSendMode, SendModes.Play);
			oldVal = Keysharp.Core.Keyboard.Hotstring("SI");
			Assert.AreNotEqual(origSendMode, HotstringDefinition.DefaultHotstringSendMode);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringSendMode, SendModes.InputThenPlay);//InputThenPlay gets used when Input is specified. See HotstringDefinition.ParseOptions().
			Assert.AreEqual(null, oldVal);
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void ChangeEndChars()
		{
			var newVal = "newendchars";
			var origVal = HotstringDefinition.DefaultHotstringEndChars;
			Assert.AreEqual(origVal, "-()[]{}:;'\"/\\,.?!\r\n \t");
			var oldVal = Keysharp.Core.Keyboard.Hotstring("EndChars", newVal);
			Assert.AreNotEqual(origVal, HotstringDefinition.DefaultHotstringEndChars);
			Assert.AreEqual(HotstringDefinition.DefaultHotstringEndChars, newVal);
			Assert.AreEqual(origVal, oldVal);
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void ChangeHotstringReplacement()
		{
			//HotstringDefinition.AddHotstring("::btw", null, ":btw", "btw", "by the way", false);
			//var tester = new HotstringDefinitionTester("tester", "");
			//tester.AddChars("asdf");
			//var origVal = HotstringDefinition.CurrentInputBuffer;
			//Assert.AreEqual(origVal, "asdf");
			//origVal = Keysharp.Core.Keyboard.Hotstring("Reset") as string;
			//Assert.AreEqual(origVal, "asdf");
			//var newVal = HotstringDefinition.CurrentInputBuffer;
			//Assert.AreNotEqual(origVal, newVal);
			//Assert.AreEqual(newVal, "");
		}

		[NonParallelizable]
		[Test, Category("Hotstring")]
		public void CreateHotstring()
		{
			//Can't seem to simulate uppercase here, so we can't test case sensitive hotstrings.
			_ = Keysharp.Core.Keyboard.Hotstring("Reset");
			_ = Keysharp.Core.Common.Keyboard.HotstringDefinition.AddHotstring("::btw", new FuncObj("label_9F201721", null), ":btw", "btw", "", false);
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