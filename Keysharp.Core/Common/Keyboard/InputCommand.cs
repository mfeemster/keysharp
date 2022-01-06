using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Keysharp.Core.Common.Patterns;

namespace Keysharp.Core.Common.Keyboard
{
	// ToDo: Handle IgnoreIAGeneratedInput
	// ToDo: Test TimeOut
	// ToDo: Test Multithreaded Access to this Singleton

	internal class AbortInformation
	{
		public string CatchedText;
		public Keys EndKey;
		public AbortReason Reason;

		public AbortInformation()
		{
			Reason = AbortReason.Fail;
			EndKey = Keys.None;
			CatchedText = "";
		}

		public AbortInformation(AbortReason reason, Keys endkey, string catchedText)
		{
			Reason = reason;
			EndKey = endkey;
			CatchedText = catchedText;
		}
	}

	/// <summary>
	/// Input Command Handler (Singleton)
	/// </summary>
	internal class InputCommand : Singleton<InputCommand>//This class is probably no longer needed since we transcribed the functionality directly from AHK.//TODO
	{
		private System.Threading.Timer _timeoutTimer;
		private AbortReason abortReason = AbortReason.Fail;
		private string catchedText = "";
		private object catchingLock = new object();
		private Keys endKeyReason = Keys.None;
		private bool isCatching = false;
		private Keysharp.Core.Common.Keyboard.KeyboardMouseSender kbdMsSender;//This is conflicting with the other hook, need to remove.//TODO
		// as long as this Class depends on other non Singleton Objects,
		// it must be configured else where

		/// <summary>
		/// Is MatchList case sensitive?
		/// </summary>
		public bool CaseSensitive { get; set; } = false;

		/// <summary>
		/// A list of Keys which terminates the key catching
		/// </summary>
		public List<Keys> Endkeys { get; } = new List<Keys>();

		/// <summary>
		/// Strings which force to abort Logging
		/// </summary>
		public List<string> EndMatches { get; } = new List<string>();

		/// <summary>
		/// Normally, what the user types must exactly match one of the MatchList phrases for a match to occur.
		/// Use this option to find a match more often by searching the entire length of the input text.
		/// </summary>
		public bool FindAnyWhere { get; set; } = false;

		/// <summary>
		/// Backspace is ignored. Normally, pressing backspace during an Input will remove
		/// the most recently pressed character from the end of the string.
		///
		/// Note: If the input text is visible (such as in an editor) and the arrow keys or other
		/// means are used to navigate within it, backspace will still remove the last character
		/// rather than the one behind the caret (insertion point).
		/// </summary>
		public bool IgnoreBackSpace { get; set; } = false;

		/// <summary>
		/// Ignore Scripts own Send Input
		/// </summary>
		public bool IgnoreIAGeneratedInput { get; set; } = false;

		/// <summary>
		/// Is User Input catching running?
		/// </summary>
		public bool IsBusy
		{
			get
			{
				lock (catchingLock)
				{
					return isCatching;
				}
			}
		}

		/// <summary>
		/// Set the KeyboardHook to use to catch userinput
		/// </summary>
		public Keysharp.Core.Common.Keyboard.KeyboardMouseSender KbdMsSender
		{
			set
			{
				kbdMsSender = value ?? throw new ArgumentException("Attempted to assign a null Keyboard Mouse Sender.");
			}

			internal get { return kbdMsSender; }
		}

		/// <summary>
		/// Max Keys which are catched. 0 Means no Limit.
		/// </summary>
		public int KeyLimit { get; set; } = 0;

		/// <summary>
		/// Modified keystrokes such as Control-A through Control-Z are recognized and
		/// transcribed if they correspond to real ASCII characters.
		/// </summary>
		public bool RecognizeModifiedKeystrockes { get; set; } = false;

		/// <summary>
		/// The number of milliseconds to wait before terminating the Input and setting ErrorLevel to the word Timeout.
		/// If the Input times out, OutputVar will be set to whatever text the user had time to enter.
		///
		/// Null means that there is no TimeOut. (default)
		/// </summary>
		public int? TimeOutVal { get; set; } = null;

		/// <summary>
		/// Normally, the user's input is blocked (hidden from the system).
		/// Use this option to have the user's keystrokes sent to the active window.
		/// </summary>
		public bool Visible { get; set; } = false;

		public void AbortCatching()
		{
			abortReason = AbortReason.NewInput;
			CatchingDone();
		}

		public void Reset()
		{
			Visible = false;
			KeyLimit = 0;
			IgnoreBackSpace = false;
			RecognizeModifiedKeystrockes = false;
			catchedText = "";
			endKeyReason = Keys.None;
			abortReason = AbortReason.Fail;
			isCatching = false;
			catchingLock = new object();
			TimeOutVal = 0;
			IgnoreIAGeneratedInput = false;
			Endkeys.Clear();
			EndMatches.Clear();
			CaseSensitive = false;
			FindAnyWhere = false;

			if (_timeoutTimer != null)
				_timeoutTimer.Dispose();
		}

		/// <summary>
		/// Start Logging Text. Blocks calling Thread until user input meets an abort condition.
		/// </summary>
		/// <returns>Returns the catched Userinput</returns>
		public AbortInformation StartCatching()
		{
			lock (catchingLock)
			{
				if (isCatching)
				{
					throw new NotImplementedException("Pending Input interruption not implemented yet!");
				}

				kbdMsSender.KeyEvent += OnKeyPressedEvent;
				isCatching = true;

				if (TimeOutVal.HasValue)
				{
					if (_timeoutTimer != null)
						_timeoutTimer.Dispose();

					_timeoutTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTimoutTick));
					_ = _timeoutTimer.Change(TimeOutVal.Value, Timeout.Infinite);
				}
			}

			while (true)
			{
				lock (catchingLock)
				{
					if (!isCatching)
						break;
				}

				Application.DoEvents(); // This is necessary if the StartCatching Method gets called on the Main GUI Thread
				Thread.Sleep(2);
			}

			kbdMsSender.KeyEvent -= OnKeyPressedEvent; // we no longer need to get notified about keys...
			var ret = new AbortInformation(abortReason, endKeyReason, catchedText);
			return ret;
		}

		private void CatchingDone()
		{
			lock (catchingLock)
			{
				isCatching = false;
			}
		}

		private void OnKeyPressedEvent(object sender, KeyEventArgs e)
		{
			if (e.Block || e.Handled || !e.Down)
				return;

			lock (catchingLock)
			{
				if (!isCatching)
					return;
			}

			// Tell how to proceed with this key
			if (!Visible)
				e.Block = true;

			// Check for Post Abort Conditions
			if (PostAbortCondition(e))
			{
				CatchingDone();
				return;
			}

			// Handle Input
			if (e.Keys == Keys.Back)
			{
				if (!IgnoreBackSpace)
				{
					catchedText = catchedText.Substring(0, catchedText.Length - 2);
				}
			}
			else
			{
				catchedText += e.Typed;
			}

			// Check for Past Abort Conditions
			if (PastAbortCondition(e))
			{
				CatchingDone();
			}
		}

		private void OnTimoutTick(object state)
		{
			var timeoutTimer = (System.Threading.Timer)state;
			timeoutTimer.Dispose();
			CatchingDone();
		}

		/// <summary>
		/// Checks if we are done with logging.
		/// </summary>
		/// <returns></returns>
		private bool PastAbortCondition(KeyEventArgs e)
		{
			// Past Condition: Key Limit
			if (KeyLimit != 0)
			{
				if (catchedText.Length >= KeyLimit)
				{
					abortReason = AbortReason.Max;
					return true;
				}
			}

			var abort = false;

			// Past Condition Matchlist
			foreach (var match in EndMatches)
			{
				if (CaseSensitive)
				{
					if (!FindAnyWhere && match == catchedText)
						abort = true;

					if (FindAnyWhere && catchedText.Contains(match))
						abort = true;
				}
				else
				{
					if (!FindAnyWhere && match.Equals(catchedText, StringComparison.OrdinalIgnoreCase))
						abort = true;

					if (FindAnyWhere && catchedText.ToLowerInvariant().Contains(match.ToLowerInvariant()))
						abort = true;
				}

				if (abort)
					break;
			}

			if (abort)
				abortReason = AbortReason.Match;

			return abort;
		}

		/// <summary>
		/// Checks if we are done with logging.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool PostAbortCondition(KeyEventArgs e)
		{
			if (Endkeys.Contains(e.Keys))
			{
				endKeyReason = e.Keys;
				abortReason = AbortReason.EndKey;
				return true;
			}

			return false;
		}
	}

	/// <summary>
	/// Reason why Catching of Userinput was stopped
	/// </summary>
	internal enum AbortReason : int
	{
		Success = 0,
		Fail = 1,
		NewInput = 2,
		Max = 3,
		Timeout = 4,
		Match = 5,
		EndKey = 6,
	}
}