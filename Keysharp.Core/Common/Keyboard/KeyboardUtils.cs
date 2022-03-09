namespace Keysharp.Core.Common.Keyboard
{
	internal static class KeyboardUtils
	{
		internal static string[] SEND_MODES = { "Event", "Input", "Play", "InputThenPlay" }; // Must match the SendModes enum.
		internal static uint MakeLong(short lowPart, short highPart) => (((ushort)lowPart) | (uint)(highPart << 16));
	}

	internal enum KeyDownTypes
	{
		Temp = 0,
		Persistent,
		Remap
	}

	internal enum HookType
	{
		None = 0,
		Keyboard = 1,
		Mouse = 2,
		Fail =  0xFF
	}

	internal enum KeyEventTypes
	{
		KeyDown,
		KeyUp,
		KeyDownAndUp
	}

	internal enum KeyStateTypes//For use with GetKeyJoyState(), etc.
	{
		Logical,
		Physical,
		Toggle
	}

	// FAIL = 0 to remind that FAIL should have the value zero instead of something arbitrary
	// because some callers may simply evaluate the return result as true or false
	// (and false is a failure):
	public enum ResultType
	{
		Fail = 0, Ok, Warn = Ok, CriticalError// Some things might rely on OK==1 (i.e. boolean "true")
		, ConditionTrue, ConditionFalse
		, LoopBreak, LoopContinue
		, EarlyReturn, EarlyExit// EARLY_EXIT needs to be distinct from FAIL for ExitApp() and AutoExecSection().
		, FailOrOk// For LineError/RuntimeError, error is continuable.
	}

	internal enum SendModes
	{
		Event,// Event must be zero.
		Input,
		Play,
		InputThenPlay,
		Invalid
	}

	// In above, SM_INPUT falls back to SM_EVENT when the SendInput mode would be defeated by the presence
	// of a keyboard/mouse hooks in another script (it does this because SendEvent is superior to a
	// crippled/interruptible SendInput due to SendEvent being able to dynamically adjust to changing
	// conditions [such as the user releasing a modifier key during the Send]).  By contrast,
	// SM_INPUT_FALLBACK_TO_PLAY falls back to the SendPlay mode.  SendInput has this extra fallback behavior
	// because it's likely to become the most popular sending method.

	internal enum SendRawModes
	{
		NotRaw,
		Raw,
		RawText
	}

	// These are used for things that can be turned on, off, or left at a
	// neutral default value that is neither on nor off.  INVALID must
	// be zero:
	internal enum ToggleValueType
	{
		Invalid = 0,
		On,
		Off,
		AlwaysOn,
		AlwaysOff,
		Toggle,
		Neutral,
		Send,
		Mouse,
		SendAndMouse,
		Default,
		MouseMove,
		MouseMoveOff
	}

	internal enum MenuType
	{
		None = 0,//None must be zero.
		Popup,
		Bar
	}

	internal enum MessageMode { WaitForMessages, ReturnAfterMessages, ReturnAfterMessagesSpecialFilter };

	internal enum Actions
	{
		// Seems best to make ACT_INVALID zero so that it will be the ZeroMemory() default within
		// any POD structures that contain an action_type field:
		ACT_INVALID = 0  // These should both be zero for initialization and function-return-value purposes.
		, ACT_ASSIGNEXPR
		, ACT_EXPRESSION
		// Keep ACT_BLOCK_BEGIN as the first "control flow" action, for range checks with ACT_FIRST_CONTROL_FLOW:
		, ACT_BLOCK_BEGIN, ACT_BLOCK_END
		, ACT_STATIC
		, ACT_HOTKEY_IF // Must be before ACT_FIRST_COMMAND.
		, ACT_EXIT // Used both with BIF_PerformAction and AddLine(), but excluded from the "named" range below so the function is preferred.
		, ACT_IF, ACT_FIRST_NAMED_ACTION = ACT_IF
		, ACT_ELSE
		, ACT_LOOP, ACT_LOOP_FILE, ACT_LOOP_REG, ACT_LOOP_READ, ACT_LOOP_PARSE
		, ACT_FOR, ACT_WHILE, ACT_UNTIL // Keep LOOP, FOR, WHILE and UNTIL together and in this order for range checks in various places.
		, ACT_BREAK, ACT_CONTINUE // Keep ACT_FOR..ACT_CONTINUE together for ACT_EXPANDS_ITS_OWN_ARGS.
		, ACT_GOTO
		, ACT_FIRST_JUMP = ACT_BREAK, ACT_LAST_JUMP = ACT_GOTO // Actions which accept a label name.
		, ACT_RETURN
		, ACT_TRY, ACT_CATCH, ACT_FINALLY, ACT_THROW // Keep TRY, CATCH and FINALLY together and in this order for range checks.
		, ACT_SWITCH, ACT_CASE // Keep ACT_TRY..ACT_CASE together for ACT_EXPANDS_ITS_OWN_ARGS.
		, ACT_LAST_NAMED_ACTION = ACT_CASE
		// ================================================================================
		// All others are not included in g_act, and are only used with BIF_PerformAction:
		// ================================================================================
		, ACT_EXITAPP
		, ACT_TOOLTIP, ACT_TRAYTIP
		, ACT_RUNAS, ACT_DOWNLOAD
		, ACT_SEND, ACT_SENDTEXT, ACT_SENDINPUT, ACT_SENDPLAY, ACT_SENDEVENT
		, ACT_SENDMODE, ACT_SENDLEVEL, ACT_COORDMODE, ACT_SETDEFAULTMOUSESPEED
		, ACT_CLICK, ACT_MOUSEMOVE, ACT_MOUSECLICK, ACT_MOUSECLICKDRAG
		, ACT_SLEEP
		, ACT_CRITICAL, ACT_THREAD
		, ACT_WINMINIMIZEALL, ACT_WINMINIMIZEALLUNDO
		// Keep rarely used actions near the bottom for parsing/performance reasons:
		, ACT_GROUPADD, ACT_GROUPDEACTIVATE, ACT_GROUPCLOSE
		, ACT_SOUNDBEEP, ACT_SOUNDPLAY
		, ACT_FILEDELETE, ACT_FILERECYCLE, ACT_FILERECYCLEEMPTY
		, ACT_FILEINSTALL, ACT_FILECOPY, ACT_FILEMOVE, ACT_DIRCOPY, ACT_DIRMOVE
		, ACT_DIRCREATE, ACT_DIRDELETE
		, ACT_FILESETATTRIB, ACT_FILESETTIME
		, ACT_SETWORKINGDIR, ACT_FILECREATESHORTCUT
		, ACT_INIWRITE, ACT_INIDELETE
		, ACT_OUTPUTDEBUG
		, ACT_SETKEYDELAY, ACT_SETMOUSEDELAY, ACT_SETWINDELAY, ACT_SETCONTROLDELAY
		, ACT_SUSPEND, ACT_PAUSE
		, ACT_BLOCKINPUT
		, ACT_SETNUMLOCKSTATE, ACT_SETSCROLLLOCKSTATE, ACT_SETCAPSLOCKSTATE
		, ACT_KEYHISTORY, ACT_LISTLINES, ACT_LISTVARS, ACT_LISTHOTKEYS
		, ACT_EDIT, ACT_RELOAD
		, ACT_SHUTDOWN
	};
}