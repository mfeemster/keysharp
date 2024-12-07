namespace Keysharp.Core
{
	/// <summary>
	/// Public interface for InputHook-related functions.
	/// </summary>
	public static class Input
	{
		/// <summary>
		/// Creates an object which can be used to collect or intercept keyboard input.
		/// </summary>
		/// <param name="options">A string of zero or more of the following options (in any order, with optional spaces in between):
		/// B: Sets BackspaceIsUndo to 0 (false), which causes Backspace to be ignored.
		/// C: Sets CaseSensitive to 1 (true), making MatchList case-sensitive.
		/// I: Sets MinSendLevel to 1 or a given value, causing any input with send level below this value to be ignored.
		/// For example, I2 would ignore any input with a level of 0 (the default) or 1, but would capture input at level 2.
		/// L: Length limit (e.g. L5). The maximum allowed length of the input.
		/// When the text reaches this length, the Input is terminated and EndReason is set to the word Max (unless the text matches one of the MatchList phrases, in which case EndReason is set to the word Match).
		/// If unspecified, the length limit is 1023.
		/// M: Allows a greater range of modified keypresses to produce text.
		/// Normally, a key is treated as non-text if it is modified by any combination other than Shift, Ctrl+Alt (i.e. AltGr) or Ctrl+Alt+Shift (i.e. AltGr+Shift).
		/// This option causes translation to be attempted for other combinations of modifiers. Consider this example, which typically recognizes Ctrl+C:
		/// T: Sets Timeout (e.g. T3 or T2.5).
		/// V: Sets VisibleText and VisibleNonText to 1 (true).
		/// Normally, the user's input is blocked (hidden from the system).
		/// Use this option to have the user's keystrokes sent to the active window.
		/// *: Wildcard. Sets FindAnywhere to 1 (true), allowing matches to be found anywhere within what the user types.
		/// E: Handle single-character end keys by character code instead of by keycode.
		/// This provides more consistent results if the active window's keyboard layout is different to the script's keyboard layout.
		/// It also prevents key combinations which don't actually produce the given end characters from ending input; for example, if @ is an end key, on the US layout Shift+2 will trigger it but Ctrl+Shift+2 will not (if the E option is used).
		/// If the C option is also used, the end character is case-sensitive.
		/// </param>
		/// <param name="endKeys">A list of zero or more keys, any one of which terminates the Input when pressed (the end key itself is not written to the Input buffer). When an Input is terminated this way, EndReason is set to the word EndKey and EndKey is set to the name of the key.</param>
		/// <param name="matchList">A comma-separated list of key phrases, any of which will cause the Input to be terminated (in which case EndReason will be set to the word Match).</param>
		/// <returns>A newly created InputObject.</returns>
		public static InputObject InputHook(object options = null, object endKeys = null, object matchList = null) => new (options.As(), endKeys.As(), matchList.As());
	}
}