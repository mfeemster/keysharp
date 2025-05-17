#if WINDOWS

using static Keysharp.Core.Common.Keyboard.KeyboardMouseSender;
using static Keysharp.Core.Common.Keyboard.KeyboardUtils;
using static Keysharp.Core.Common.Keyboard.ScanCodes;
using static Keysharp.Core.Common.Keyboard.VirtualKeys;
using static Keysharp.Core.Windows.WindowsAPI;

namespace Keysharp.Core.Windows
{
	/// <summary>
	/// Concrete implementation of HookThread for the Windows platfrom.
	/// Once we figure out how to wire up a hook on linux, we need to go through every method here and move any that are not
	/// windows-specific into the base class to reduce duplication.
	/// Of course leave any windows-specific methods here.
	/// </summary>
	internal class WindowsHookThread : HookThread
	{
		private readonly LowLevelKeyboardProc kbdHandlerDel;
		private readonly LowLevelMouseProc mouseHandlerDel;
		private bool pendingDeadKeyInvisible;
		private List<DeadKeyRecord> pendingDeadKeys = [];
		private StaThreadWithMessageQueue thread;
		private bool uwpAppFocused;
		private IntPtr uwpHwndChecked = IntPtr.Zero;
		internal WindowsHookThread()
		{
			keyToSc = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)//Unsure if these are cross platform or not.//TODO
			{
				{"NumpadEnter", NumpadEnter},
				{"Delete", Delete},
				{"Del", Delete},
				{"Insert", Insert},
				{"Ins", Insert},
				//{"Clear", SC_CLEAR},  // Seems unnecessary because there is no counterpart to the Numpad5 clear key?
				{"Up", Up},
				{"Down", Down},
				{"Left", Left},
				{"Right", Right},
				{"Home", Home},
				{"End", End},
				{"PgUp", PgUp},
				{"PgDn", PgDn}
			};
			keyToScAlt = keyToSc.GetAlternateLookup<ReadOnlySpan<char>>();
			keyToVk = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
			{
				{"Numpad0", VK_NUMPAD0},
				{"Numpad1", VK_NUMPAD1},
				{"Numpad2", VK_NUMPAD2},
				{"Numpad3", VK_NUMPAD3},
				{"Numpad4", VK_NUMPAD4},
				{"Numpad5", VK_NUMPAD5},
				{"Numpad6", VK_NUMPAD6},
				{"Numpad7", VK_NUMPAD7},
				{"Numpad8", VK_NUMPAD8},
				{"Numpad9", VK_NUMPAD9},
				{"NumpadMult", VK_MULTIPLY},
				{"NumpadDiv", VK_DIVIDE},
				{"NumpadAdd", VK_ADD},
				{"NumpadSub", VK_SUBTRACT},
				// , {("NumpadEnter"), VK_RETURN},  // Must do this one via scan code, see below for explanation.
				{"NumpadDot", VK_DECIMAL},
				{"Numlock", VK_NUMLOCK},
				{"ScrollLock", VK_SCROLL},
				{"CapsLock", VK_CAPITAL},
				{"Escape", VK_ESCAPE},  // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				{"Esc", VK_ESCAPE},
				{"Tab", VK_TAB},
				{"Space", VK_SPACE},
				{"Backspace", VK_BACK}, // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				{"BS", VK_BACK},
				// These keys each have a counterpart on the number pad with the same VK.  Use the VK for these,
				// since they are probably more likely to be assigned to hotkeys (thus minimizing the use of the
				// keyboard hook, and use the scan code (SC) for their counterparts.  UPDATE: To support handling
				// these keys with the hook (i.e. the sc_takes_precedence flag in the hook), do them by scan code
				// instead.  This allows Numpad keys such as Numpad7 to be differentiated from NumpadHome, which
				// would otherwise be impossible since both of them share the same scan code (i.e. if the
				// sc_takes_precedence flag is set for the scan code of NumpadHome, that will effectively prevent
				// the hook from telling the difference between it and Numpad7 since the hook is currently set
				// to handle an incoming key by either vk or sc, but not both.
				// For VKs with multiple SCs, such as VK_RETURN, the keyboard hook is made mandatory unless the
				// user specifies the VK by number.  This ensures that Enter:: and NumpadEnter::, for example,
				// only fire when the appropriate key is pressed.
				{"Enter", VK_RETURN},  // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				// See g_key_to_sc for why these Numpad keys are handled here:
				{"NumpadDel", VK_DELETE},
				{"NumpadIns", VK_INSERT},
				{"NumpadClear", VK_CLEAR},  // same physical key as Numpad5 on most keyboards?
				{"NumpadUp", VK_UP},
				{"NumpadDown", VK_DOWN},
				{"NumpadLeft", VK_LEFT},
				{"NumpadRight", VK_RIGHT},
				{"NumpadHome", VK_HOME},
				{"NumpadEnd", VK_END},
				{"NumpadPgUp", VK_PRIOR},
				{"NumpadPgDn", VK_NEXT},
				{"PrintScreen", VK_SNAPSHOT},
				{"CtrlBreak", VK_CANCEL},  // Might want to verify this, and whether it has any peculiarities.
				{"Pause", VK_PAUSE}, // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				{"Help", VK_HELP},  // VK_HELP is probably not the extended HELP key.  Not sure what this one is.
				{"Sleep", VK_SLEEP},
				{"AppsKey", VK_APPS},
				{"LControl", VK_LCONTROL}, // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				{"RControl", VK_RCONTROL}, // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				{"LCtrl", VK_LCONTROL}, // Abbreviated versions of the above.
				{"RCtrl", VK_RCONTROL}, //
				{"LShift", VK_LSHIFT},
				{"RShift", VK_RSHIFT},
				{"LAlt", VK_LMENU},
				{"RAlt", VK_RMENU},
				// These two are always left/right centric and I think their vk's are always supported by the various
				// Windows API calls, unlike VK_RSHIFT, etc. (which are seldom supported):
				{"LWin", VK_LWIN},
				{"RWin", VK_RWIN},
				{"Control", VK_CONTROL}, // So that VKtoKeyName() delivers consistent results, always have the preferred name first.
				{"Ctrl", VK_CONTROL},  // An alternate for convenience.
				{"Alt", VK_MENU},
				{"Shift", VK_SHIFT},
				{"F1", VK_F1},
				{"F2", VK_F2},
				{"F3", VK_F3},
				{"F4", VK_F4},
				{"F5", VK_F5},
				{"F6", VK_F6},
				{"F7", VK_F7},
				{"F8", VK_F8},
				{"F9", VK_F9},
				{"F10", VK_F10},
				{"F11", VK_F11},
				{"F12", VK_F12},
				{"F13", VK_F13},
				{"F14", VK_F14},
				{"F15", VK_F15},
				{"F16", VK_F16},
				{"F17", VK_F17},
				{"F18", VK_F18},
				{"F19", VK_F19},
				{"F20", VK_F20},
				{"F21", VK_F21},
				{"F22", VK_F22},
				{"F23", VK_F23},
				{"F24", VK_F24},
				// Mouse buttons:
				{"LButton", VK_LBUTTON},
				{"RButton", VK_RBUTTON},
				{"MButton", VK_MBUTTON},
				{"XButton1", VK_XBUTTON1},
				{"XButton2", VK_XBUTTON2},
				// Custom/fake VKs for use by the mouse hook:
				{"WheelDown", VK_WHEEL_DOWN},
				{"WheelUp", VK_WHEEL_UP},
				{"WheelLeft", VK_WHEEL_LEFT},
				{"WheelRight", VK_WHEEL_RIGHT},
				{"Browser_Back", VK_BROWSER_BACK},
				{"Browser_Forward", VK_BROWSER_FORWARD},
				{"Browser_Refresh", VK_BROWSER_REFRESH},
				{"Browser_Stop", VK_BROWSER_STOP},
				{"Browser_Search", VK_BROWSER_SEARCH},
				{"Browser_Favorites", VK_BROWSER_FAVORITES},
				{"Browser_Home", VK_BROWSER_HOME},
				{"Volume_Mute", VK_VOLUME_MUTE},
				{"Volume_Down", VK_VOLUME_DOWN},
				{"Volume_Up", VK_VOLUME_UP},
				{"Media_Next", VK_MEDIA_NEXT_TRACK},
				{"Media_Prev", VK_MEDIA_PREV_TRACK},
				{"Media_Stop", VK_MEDIA_STOP},
				{"Media_Play_Pause", VK_MEDIA_PLAY_PAUSE},
				{"Launch_Mail", VK_LAUNCH_MAIL},
				{"Launch_Media", VK_LAUNCH_MEDIA_SELECT},
				{"Launch_App1", VK_LAUNCH_APP1},
				{"Launch_App2", VK_LAUNCH_APP2}
				// Probably safest to terminate it this way, with a flag value.  (plus this makes it a little easier
				// to code some loops, maybe).  Can also calculate how many elements are in the array using sizeof(array)
				// divided by sizeof(element).  UPDATE: Decided not to do this in case ever decide to sort this array; don't
				// want to rely on the fact that this will wind up in the right position after the sort (even though it
				// should):
				//, {(""), 0}
			};
			keyToVkAlt = keyToVk.GetAlternateLookup<ReadOnlySpan<char>>();
			var foundesc = false;

			foreach (var kv in keyToVk)//Make the reverse dictionary so it's quickerr to look up without requiring a full loop to compare each key value.
			{
				if (foundesc)
					vkToKey[kv.Value] = kv.Key;
				else if (kv.Key == "Esc")//Do not add Esc, because Escape will take its place.
					foundesc = true;
				else
					vkToKey[kv.Value] = kv.Key;
			}

			kbdMsSender = new WindowsKeyboardMouseSender();
			kbdHandlerDel = new LowLevelKeyboardProc(LowLevelKeybdHandler);
			mouseHandlerDel = new LowLevelMouseProc(LowLevelMouseHandler);
		}

		public override void SimulateKeyPress(uint key)
		{
			var kbdStruct = new KBDLLHOOKSTRUCT()
			{
				vkCode = key,
				scanCode = MapVkToSc(key),
				flags = 0,
				time = (uint)DateTime.UtcNow.Ticks,
				dwExtraInfo = 0
			};
			_ = LowLevelKeybdHandler(0, new IntPtr(256), ref kbdStruct);
		}

		internal override void AddRemoveHooks(HookType hooksToBeActive, bool changeIsTemporary = false)
		{
			var hooksActiveOrig = GetActiveHooks();

			if (hooksToBeActive == hooksActiveOrig) // It's already in the right state.
				return;

			if (hooksActiveOrig == HookType.None) // Neither hook is active now but at least one will be or the above would have returned.
			{
				// Assert: sThreadHandle should be NULL at this point.  The only way this isn't true is if
				// a previous call to AddRemoveHooks() timed out while waiting for the hook thread to exit,
				// which seems far too rare to add extra code for.
				// CreateThread() vs. _beginthread():
				// It's not necessary to link to the multi-threading C runtime (which bloats the code by 3.5 KB
				// compressed) as long as the new thread doesn't call any C-library functions that aren't thread-safe
				// (in addition to the C functions that obviously use static data, calls to things like malloc(),
				// new, and other memory management functions probably aren't thread-safe unless the multi-threaded
				// library is used). The memory leak described in MSDN for ExitThread() applies only to the
				// multi-threaded libraries (multiple sources confirm this), so it isn't a concern either.
				// That's true even if the program is linked against the multi-threaded DLLs (MSVCRT.dll) rather
				// than the libraries (e.g. for a minimum-sized SC.bin file), as confirmed by the following quotes:
				// "This applies only to the static-link version of the runtime. For this and other reasons, I
				// *highly* recommend using the DLL runtime, which lets you use CreateThread() without prejudice.
				// Confirmation from MSDN: "Another work around is to link the *executable* to the CRT in a *DLL*
				// instead of the static CRT."
				//
				// The hooks are designed to make minimal use of C-library calls, currently calling only things
				// like memcpy() and strlen(), which are thread safe in the single-threaded library (according to
				// their source code).  However, the hooks may indirectly call other library functions via calls
				// to KeyEvent() and other functions, which has already been reviewed for thread-safety but needs
				// to be kept in mind as changes are made in the future.
				//
				// CreateThread's second parameter is the new thread's initial stack size. The stack will grow
				// automatically if more is needed, so it's kept small here to greatly reduce the amount of
				// memory used by the hook thread.  The XP Task Manager's "VM Size" column (which seems much
				// more accurate than "Mem Usage") indicates that a new thread consumes 28 KB + its stack size.
				if (!changeIsTemporary) // Caller has ensured that thread already exists when aChangeIsTemporary==true.
				{
					Start();

					if (channelThreadID != 0)
					{
					}
					// The above priority level seems optimal because if some other process has high priority,
					// the keyboard and mouse hooks will still take precedence, which avoids the mouse cursor
					// and keystroke lag that would otherwise occur (confirmed through testing).  Due to their
					// return-ASAP nature, the hooks are an ideal candidate for almost-realtime priority because
					// they run only rarely and only for tiny bursts of time.
					// Note that the above must also be done in such a way that it works on NT4, which doesn't support
					// below-normal and above-normal process priorities, nor perhaps other aspects of priority.
					// So what is the actual priority given to the hooks by the OS?  Assuming that the script's
					// process is set to NORMAL_PRIORITY_CLASS (which is the default), the following applies:
					// First of all, a definition: "base priority" is the actual/net priority of the thread.
					// It determines how the OS will schedule a thread relative to all other threads on the system.
					// So in a sense, if you look only at base priority, the thread's process's priority has no
					// bearing on how the thread will get scheduled (except to the extent that it contributes
					// to the calculation of the base priority itself).  Here are some common base priorities
					// along with where the hook priority (15) fits in:
					// 7 = NORMAL_PRIORITY_CLASS process + THREAD_PRIORITY_NORMAL thread.
					// 9 = NORMAL_PRIORITY_CLASS process + THREAD_PRIORITY_HIGHEST thread.
					// 13 = HIGH_PRIORITY_CLASS process + THREAD_PRIORITY_NORMAL thread.
					// 15 = (ANY)_PRIORITY_CLASS process + THREAD_PRIORITY_TIME_CRITICAL thread. <-- Seems like the optimal compromise.
					// 15 = HIGH_PRIORITY_CLASS process + THREAD_PRIORITY_HIGHEST thread.
					// 24 = REALTIME_PRIORITY_CLASS process + THREAD_PRIORITY_NORMAL thread.
					else // Failed to create thread.  Seems to rare to justify the display of an error.
					{
						FreeHookMem(); // If everything's designed right, there should be no hooks now (even if there is, they can't be functional because their thread is nonexistent).
						return;
					}
				}
			}

			//else there is at least one hook already active, which guarantees that the hook thread exists (assuming
			// everything is designed right).
			// Above has ensured that the hook thread now exists, so send it the status-change message.
			// Post the AHK_CHANGE_HOOK_STATE message to the new thread to put the right hooks into effect.
			// If both hooks are to be deactivated, AHK_CHANGE_HOOK_STATE also causes the hook thread to exit.
			// PostThreadMessage() has been observed to fail, such as when a script replaces a previous instance
			// of itself via #SingleInstance.  I think this happens because the new thread hasn't yet had a
			// chance to create its message queue via GetMessage().  So rather than using something like
			// WaitForSingleObject() -- which might not be reliable due to split-second timing of when the
			// queue actually gets created -- just keep retrying until time-out or PostThreadMessage() succeeds.
			//var ksmsg = new KeysharpMsg()
			//{
			//  message = (uint)UserMessages.AHK_CHANGE_HOOK_STATE,
			//  wParam = new IntPtr((uint)hooksToBeActive),
			//  lParam = new IntPtr(changeIsTemporary ? 0 : 1/*Flipped on purpose*/)
			//};
			//
			//for (var i = 0; i < 50 && !channel.Writer.TryWrite(ksmsg); ++i)
			//  System.Threading.Thread.Sleep(10); // Should never execute if thread already existed before this function was called.
			//Pulled from msg queue thread
			var problemActivatingHooks = ChangeHookState(hooksToBeActive, changeIsTemporary);
			//
			//
			//for (var i = 0; i < 50 && !PostThreadMessage(hookThreadID, (uint)UserMessages.AHK_CHANGE_HOOK_STATE, new UIntPtr((uint)hooksToBeActive), new IntPtr(changeIsTemporary ? 1 : 0/*Flipped on purpose*/)); ++i)
			//System.Threading.Thread.Sleep(10); // Should never execute if thread already existed before this function was called.
			// Above: Sleep(10) seems better than Sleep(0), which would max the CPU while waiting.
			// MUST USE Sleep vs. MsgSleep, otherwise an infinite recursion of ExitApp is possible.
			// This can be reproduced by running a script consisting only of the line #InstallMouseHook
			// and then exiting via the tray menu.  I tried fixing it in TerminateApp with the following,
			// but it's just not enough.  So rather than spend a long time on it, it's fixed directly here:
			// Because of the below, our callers must NOT assume that an exit will actually take place.
			//static is_running = false;
			//if (is_running)
			//  return OK;
			//is_running = true; // Since we're exiting, there should be no need to set it to false further below.
			// If it times out I think it's realistically impossible that the new thread really exists because
			// if it did, it certainly would have had time to execute GetMessage() in all but extreme/theoretical
			// cases.  Therefore, no thread check/termination attempt is done.  Alternatively, a check for
			// GetExitCodeThread() could be done followed by closing the handle and setting it to NULL, but once
			// again the code size doesn't seem worth it for a situation that is probably impossible.
			//
			// Also, a timeout itself seems too rare (perhaps even impossible) to justify a warning dialog.
			// So do nothing, which retains the current values of g_KeybdHook and g_MouseHook.
			// For safety, serialize the termination of the hook thread so that this function can't be called
			// again by the main thread before the hook thread has had a chance to exit in response to the
			// previous call.  This improves reliability, especially by ensuring a clean exit (if our caller
			// is about to exit the app via exit(), which otherwise might not cleanly close all threads).
			// UPDATE: Also serialize all changes to the hook status so that our caller can rely on the new
			// hook state being in effect immediately.  For example, the Input command installs the keyboard
			// hook and it's more maintainable if we ensure the status is correct prior to returning.
			//DateTime startTime;
			//bool problemActivatingHooks;
			//for (problemActivatingHooks = false, startTime = DateTime.UtcNow; ;) // For our caller, wait for hook thread to update the status of the hooks.
			{
				//Wait till we get confirmation back that the action completed.
				//if (PeekMessage(out msg, IntPtr.Zero, (uint)UserMessages.AHK_CHANGE_HOOK_STATE, (uint)UserMessages.AHK_CHANGE_HOOK_STATE, PM_REMOVE))
				//if (ksmsg.completed)
				{
					if (hooksToBeActive != HookType.None) // Wait for the hook thread to activate the specified hooks.
					{
						//if (ksmsg.wParam != IntPtr.Zero) // The hook thread indicated failure to activate one or both of the hooks.
						if (problemActivatingHooks) // The hook thread indicated failure to activate one or both of the hooks.
						{
							// This is done so that the MsgBox warning won't be shown until after these loops finish,
							// which seems safer to prevent any parts of the script from running as a result
							// the MsgBox pumping hotkey messages and such, which could result in a script
							// subroutine launching while we're in here:
							//problemActivatingHooks = true;
							if (GetActiveHooks() == HookType.None && !changeIsTemporary) // The failure is such that no hooks are now active, and thus (due to the mode) the hook thread will exit.
							{
								// Convert this loop into the mode that waits for the hook thread to exit.
								// This allows the thread handle to be closed and the memory to be freed.
								hooksToBeActive = 0;
								//continue;
							}

							// It failed but one hook is still active, or the change is temporary.  Either way,
							// we're done waiting.  Fall through to "break" below.
						}

						//else it successfully changed the state.
						// In either case, we're done waiting:
						//break;
						//else no AHK_CHANGE_HOOK_STATE message has arrived yet, so keep waiting until it does or timeout occurs.
					}
					else // The hook thread has been asked to deactivate both hooks.
					{
						if (changeIsTemporary) // The thread will not terminate in this mode, it will just remove its hooks.
						{
							//if (GetActiveHooks() == HookType.None) // The hooks have been deactivated.
							//  break; // Don't call FreeHookMem() because caller doesn't want that when aChangeIsTemporary==true.
						}
						else // Wait for the thread to terminate.
						{
							//GetExitCodeThread(new IntPtr(hookThreadID), out exit_code);
							if (IsReadThreadCompleted()) // The hook thread is now gone.
							{
								channelReadThread = null;
								channelThreadID = 0;
								FreeHookMem();//There should be no hooks now (even if there is, they can't be functional because their thread is nonexistent).
								//break;
							}
						}
					}
				}
				//When stepping through code, this timeout will be exceeded, thus preventing the code above from running sometimes.
				//So do not timeout when debugging.
				//if (!Debugger.IsAttached)
				//{
				//  if ((DateTime.UtcNow - startTime).TotalMilliseconds > 1000)//Original did 500ms, increase to 1s just to be safe.
				//      break;
				//}
				// v1.0.43: The following sleeps for 0 rather than some longer time because:
				// 1) In nearly all cases, this loop should do only one iteration because a Sleep(0) should guarantee
				//    that the hook thread will get a timeslice before our thread gets another.  In fact, it might not
				//    do any iterations if the system preempts the main thread immediately when a message is posted to
				//    a higher priority thread (especially one in its own process).
				// 2) SendKeys()'s SendInput mode relies on fast removal of hook to prevent a 10ms or longer delay before
				//    the keystrokes get sent.  Such a delay would be quite undesirable in cases where response time is
				//    critical, such as in games.
				// Testing shows that removing the Sleep() entirely does not help performance.  The following was measured
				// when the CPU was under heavy load from a cpu-maxing utility:
				//   Loop 10  ; Keybd hook must be installed for this test to be meaningful.
				//      SendInput {Shift}
				//System.Threading.Thread.Sleep(0); // Not MsgSleep (see the "Sleep(10)" above for why).
			}

			// If the above loop timed out without the hook thread exiting (if it was asked to exit), sThreadHandle
			// is left as non-NULL to reflect this condition.

			// In case mutex create/open/close can be a high-overhead operation, do it only when the hook isn't
			// being quickly/temporarily removed then added back again.
			if (!changeIsTemporary)
			{
				if (HasKbdHook() && ((int)hooksActiveOrig & (int)HookType.Keyboard) == 0) // The keyboard hook has been newly added.
				{
					keybdMutex = new Mutex(false, KeybdMutexName, out _); // Create-or-open this mutex and have it be unowned.
				}
				else if (!HasKbdHook() && ((int)hooksActiveOrig & (int)HookType.Keyboard) != 0)  // The keyboard hook has been newly removed.
				{
					keybdMutex?.Close();
					keybdMutex = null;
				}

				if (HasKbdHook() && ((int)hooksActiveOrig & (int)HookType.Mouse) == 0) // The mouse hook has been newly added.
				{
					mouseMutex = new Mutex(false, MouseMutexName); // Create-or-open this mutex and have it be unowned.
				}
				else if (!HasKbdHook() && ((int)hooksActiveOrig & (int)HookType.Mouse) != 0)  // The mouse hook has been newly removed.
				{
					mouseMutex?.Close();
					mouseMutex = null;
				}
			}

			// For maintainability, it seems best to display the MsgBox only at the very end.
			if (problemActivatingHooks)
			{
				// Prevent hotkeys and other subroutines from running (which could happen via MsgBox's message pump)
				// to avoid the possibility that the script will continue to call this function recursively, resulting
				// in an infinite stack of MsgBoxes. This approach is similar to that used in Hotkey::Perform()
				// for the A_MaxHotkeysPerInterval warning dialog:
				script.FlowData.allowInterruption = false;
				// Below is a generic message to reduce code size.  Failure is rare, but has been known to happen when
				// certain types of games are running).
				_ = MessageBox.Show("Warning: The keyboard and/or mouse hook could not be activated; some parts of the script will not function.");//AHK has its own MsgBox() function which does things differently. Will need to see if we need to do all of that.
				script.FlowData.allowInterruption = true;
			}
		}

		/// <summary>
		/// Always use the parameter vk rather than event.vkCode because the caller or caller's caller
		/// might have adjusted vk, namely to make it a left/right specific modifier key rather than a
		/// neutral one.
		/// </summary>
		internal long AllowIt(IntPtr hook, int code, long param, ref KBDLLHOOKSTRUCT kbd, ref MSDLLHOOKSTRUCT mouse, uint vk, uint sc,
							  bool keyUp, ulong extraInfo, CollectInputState state, KeyHistoryItem keyHistoryCurr,
							  uint hotkeyIDToPost, HotkeyVariant variant)
		{
			HotstringDefinition hsOut = null;
			var caseConformMode = CaseConformModes.None;
			var endChar = (char)0;
			var hm = script.HotstringManager;

			// Prevent toggleable keys from being toggled (if the user wanted that) by suppressing it.
			// Seems best to suppress key-up events as well as key-down, since a key-up by itself,
			// if seen by the system, doesn't make much sense and might have unwanted side-effects
			// in rare cases (e.g. if the foreground app takes note of these types of key events).
			// Don't do this for ignored keys because that could cause an endless loop of
			// numlock events due to the keybd events that SuppressThisKey sends.
			// It's a little more readable and comfortable not to rely on short-circuit
			// booleans and instead do these conditions as separate IF statements.
			if (hook == mouseHook)
			{
				// Since a mouse button that is physically down is not necessarily logically down -- such as
				// when the mouse button is a suppressed hotkey -- only update the logical state (which is the
				// state the OS believes the key to be in) when this event is non-supressed (i.e. allowed to
				// go to the system):
				//Only for FUTURE_USE_MOUSE_BUTTONS_LOGICAL
				// THIS ENTIRE SECTION might never be necessary if it's true that GetAsyncKeyState() and
				// GetKeyState() can retrieve the logical mouse button state on Windows NT/2000/XP, which are
				// the only OSes that matter for this purpose because the hooks aren't supported on Win9x.
				//KBDLLHOOKSTRUCT& event = *(PMSDLLHOOKSTRUCT) lParam;  // For convenience, maintainability, and possibly performance.
				//
				//switch (wParam)
				//{
				//  case WM_LBUTTONUP: g_mouse_buttons_logical &= ~MK_LBUTTON; break;
				//
				//  case WM_RBUTTONUP: g_mouse_buttons_logical &= ~MK_RBUTTON; break;
				//
				//  case WM_MBUTTONUP: g_mouse_buttons_logical &= ~MK_MBUTTON; break;
				//
				//  // WM_NCXBUTTONUP is a click in the non-client area of a window.  MSDN implies this message can be
				//  // received by the mouse hook  but it seems doubtful because its counterparts, such as WM_NCLBUTTONUP,
				//  // are apparently never received:
				//  case WM_NCXBUTTONUP:
				//  case WM_XBUTTONUP:
				//      g_mouse_buttons_logical &= ~(   (HIWORD(event.mouseData)) == XBUTTON1 ? MK_XBUTTON1 : MK_XBUTTON2   );
				//      break;
				//
				//  case WM_LBUTTONDOWN: g_mouse_buttons_logical |= MK_LBUTTON; break;
				//
				//  case WM_RBUTTONDOWN: g_mouse_buttons_logical |= MK_RBUTTON; break;
				//
				//  case WM_MBUTTONDOWN: g_mouse_buttons_logical |= MK_MBUTTON; break;
				//
				//  case WM_NCXBUTTONDOWN:
				//  case WM_XBUTTONDOWN:
				//      g_mouse_buttons_logical |= (HIWORD(event.mouseData) == XBUTTON1) ? MK_XBUTTON1 : MK_XBUTTON2;
				//      break;
				//}
			}
			else // Our caller is the keyboard hook.
			{
				var isIgnored = IsIgnored(kbd.dwExtraInfo);

				if (!isIgnored)
				{
					var forceToggle = kvk[vk].ToggleVal(vk);

					if (forceToggle != null) // Key is a toggleable key.
						if (forceToggle != ToggleValueType.Neutral) // Prevent toggle.
							return SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIDToPost, variant);
				}

				if ((hm.enabledCount > 0 && !isIgnored) || script.input != null)
					if (!CollectInput(ref kbd, vk, sc, keyUp, isIgnored, state, keyHistoryCurr, ref hsOut, ref caseConformMode, ref endChar)) // Key should be invisible (suppressed).
						return SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIDToPost, variant, hsOut, caseConformMode, endChar);

				// Do this here since the above "return SuppressThisKey" will have already done it in that case.
				UpdateKeybdState(ref kbd, vk, sc, keyUp, false);

				// UPDATE: The Win-L and Ctrl-Alt-Del workarounds below are still kept in effect in spite of the
				// anti-stick workaround done via GetModifierLRState().  This is because ResetHook() resets more
				// than just the modifiers and physical key state, which seems appropriate since the user might
				// be away for a long period of time while the computer is locked or the security screen is displayed.
				// Win-L uses logical keys, unlike Ctrl-Alt-Del which uses physical keys (i.e. Win-L can be simulated,
				// but Ctrl-Alt-Del must be physically pressed by the user):
				if (vk == 'L' && !keyUp && (kbdMsSender.modifiersLRLogical == MOD_LWIN  // i.e. *no* other keys but WIN.
											|| kbdMsSender.modifiersLRLogical == MOD_RWIN || kbdMsSender.modifiersLRLogical == (MOD_LWIN | MOD_RWIN)))
				{
					// Since the user has pressed Win-L with *no* other modifier keys held down, and since
					// this key isn't being suppressed (since we're here in this function), the computer
					// is about to be locked.  When that happens, the hook is apparently disabled or
					// deinstalled until the user logs back in.  Because it is disabled, it will not be
					// notified when the user releases the LWIN or RWIN key, so we should assume that
					// it's now not in the down position.  This avoids it being thought to be down when the
					// user logs back in, which might cause hook hotkeys to accidentally fire.
					// Update: I've received an indication from a single Win2k user (unconfirmed from anyone
					// else) that the Win-L hotkey doesn't work on Win2k.  AutoIt3 docs confirm this.
					// Thus, it probably doesn't work on NT either.  So it's been changed to happen only on XP:
					ResetHook(true); // We already know that *only* the WIN key is down.
					// Above will reset g_PhysicalKeyState, especially for the windows keys and the 'L' key
					// (in our case), in preparation for re-logon:
				}

				// Although the delete key itself can be simulated (logical or physical), the user must be physically
				// (not logically) holding down CTRL and ALT for the ctrl-alt-del sequence to take effect,
				// which is why g_modifiersLR_physical is used vs. kbdMsSender.modifiersLR_logical (which is used above since
				// it's different).  Also, this is now done for XP -- in addition to NT4 & Win2k -- in case XP is
				// configured to display the NT/2k style security window instead of the task manager.  This is
				// probably very common because whenever the welcome screen is disabled, that's the default behavior?:
				// Control Panel > User Accounts > Use the welcome screen for fast and easy logon
				if ((vk == VK_DELETE || vk == VK_DECIMAL) && !keyUp         // Both of these qualify, see notes.
						// Below: At least one CTRL key is physically down.  physical and ctrlaltdel_mask are combined
						// because ctrlaltdel_mask excludes fake LCtrl (from AltGr) but might not be tracked as reliably.
						&& (kbdMsSender.modifiersLRPhysical & kbdMsSender.modifiersLRCtrlAltDelMask & (MOD_LCONTROL | MOD_RCONTROL)) != 0
						&& (kbdMsSender.modifiersLRPhysical & (MOD_LALT | MOD_RALT)) != 0        // At least one ALT key is physically down.
						&& (kbdMsSender.modifiersLRPhysical & (MOD_LSHIFT | MOD_RSHIFT)) == 0)// Neither shift key is phys. down (WIN is ok).
				{
					// Similar to the above case except for Windows 2000.  I suspect it also applies to NT,
					// but I'm not sure.  It seems safer to apply it to NT until confirmed otherwise.
					// Note that Ctrl-Alt-Delete works with *either* delete key, and it works regardless
					// of the state of Numlock (at least on XP, so it's probably that way on Win2k/NT also,
					// though it would be nice if this too is someday confirmed).  Here's the key history
					// someone for when the pressed ctrl-alt-del and then pressed esc to dismiss the dialog
					// on Win2k (Win2k invokes a 6-button dialog, with choices such as task manager and lock
					// workstation, if I recall correctly -- unlike XP which invokes task mgr by default):
					// A4  038      d   21.24   Alt
					// A2  01D      d   0.00    Ctrl
					// A2  01D      d   0.52    Ctrl
					// 2E  053      d   0.02    Num Del         <-- notice how there's no following up event
					// 1B  001      u   2.80    Esc             <-- notice how there's no preceding down event
					// Other notes: On XP at least, shift key must not be down, otherwise Ctrl-Alt-Delete does
					// not take effect.  Windows key can be down, however.
					// Since the user will be gone for an unknown amount of time, it seems best just to reset
					// all hook tracking of the modifiers to the "up" position.  The user can always press them
					// down again upon return.  It also seems best to reset both logical and physical, just for
					// peace of mind and simplicity:
					ResetHook(true);
					// The above will also reset g_PhysicalKeyState so that especially the following will not
					// be thought to be physically down:CTRL, ALT, and DEL keys.  This is done in preparation
					// for returning from the security screen.  The neutral keys (VK_MENU and VK_CONTROL)
					// must also be reset -- not just because it's correct but because CollectInput() relies on it.
				}

				// Bug-fix for v1.0.20: The below section was moved out of LowLevelKeybdProc() to here because
				// altTabMenuIsVisible should not be set to true prior to knowing whether the current tab-down
				// event will be suppressed.  This is because if it is suppressed, the menu will not become visible
				// after all since the system will never see the tab-down event.
				// Having this extra check here, in addition to the other(s) that set altTabMenuIsVisible to be
				// true, allows AltTab and ShiftAltTab hotkeys to function even when the AltTab menu was invoked by
				// means other than an AltTabMenu or AltTabAndMenu hotkey.  The alt-tab menu becomes visible only
				// under these exact conditions, at least under WinXP:
				if (vk == VK_TAB && !keyUp && !altTabMenuIsVisible
						&& (kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT)) != 0 // At least one ALT key is down.
						&& (kbdMsSender.modifiersLRLogical & (MOD_LCONTROL | MOD_RCONTROL)) == 0) // Neither CTRL key is down.
					altTabMenuIsVisible = true;

				uint modLR;

				if ((modLR = kvk[vk].asModifiersLR) != 0) // It's a modifier key.
				{
					// Don't do it this way because then the alt key itself can't be reliably used as "AltTabMenu"
					// (due to ShiftAltTab causing altTabMenuIsVisible to become false):
					//if (   altTabMenuIsVisible && !((kbdMsSender.modifiersLR_logical & MOD_LALT) || (kbdMsSender.modifiersLR_logical & MOD_RALT))
					//  && !(aKeyUp && keyHistoryCurr.event_type == 'h')   )  // In case the alt key itself is "AltTabMenu"
					if (altTabMenuIsVisible && // Release of Alt key (the check above confirmed it is a modifier):
							(keyUp && (vk == VK_LMENU || vk == VK_RMENU || vk == VK_MENU))
							// In case the alt key itself is "AltTabMenu":
							&& keyHistoryCurr.eventType != 'h' && keyHistoryCurr.eventType != 's')
						// It's important to reset in this case because if altTabMenuIsVisible were to
						// stay true and the user presses ALT in the future for a purpose other than to
						// display the Alt-tab menu, we would incorrectly believe the menu to be displayed:
						altTabMenuIsVisible = false;

					if (!keyUp) // Key-down.
					{
						// undisguisedMenuInEffect can be true or false prior to this.
						//  LAlt (true) + LWin = both disguised (false).
						//  LWin (true) + LAlt = both disguised (false).
						if ((modLR & (MOD_LWIN | MOD_RWIN)) != 0)
							undisguisedMenuInEffect = (kbdMsSender.modifiersLRLogical & ~(MOD_LWIN | MOD_RWIN)) == 0; // If any other modifier is down, disguise is already in effect.
						else if ((modLR & (MOD_LALT | MOD_RALT)) != 0)
							undisguisedMenuInEffect = (kbdMsSender.modifiersLRLogical & (MOD_LCONTROL | MOD_RCONTROL)) == 0; // If Ctrl is down (including if this Alt is AltGr), disguise is already in effect.
						else // Shift or Ctrl: pressing either serves to disguise any previous Alt or Win.
							undisguisedMenuInEffect = false;
					}
					else if (disguiseNextMenu)
					{
						// If a menu key is still physically down (or down due to an explicit Send, such as a remapping),
						// keep watching until it is released so that if key-repeat puts it back into effect, it will be
						// disguised again.  _non_ignored is used to ignore temporary modifier changes made during a
						// Send which aren't explicit, such as `Send x` temporarily releasing LWin/RWin.  Without this,
						// something like AppsKey::RWin would not work well with other hotkeys which Send.
						// v1.1.27.01: This section now also handles Ctrl-up and Shift-up events, which not only fail to
						// disguise Win but actually cause the Start menu to immediately appear even though the Win key
						// has not been released.  This only occurs if it was not already disguised by the Ctrl/Shift down
						// event; i.e. when an isolated Ctrl/Shift up event is received without a corresponding down event.
						// "Physical" events of this kind can be sent by the system when switching from a window with UK
						// layout to a window with US layout.  This is likely related to the UK layout having AltGr.
						// v1.1.33.03: This is now applied to LAlt/RAlt, to fix issues with hotkeys like !WheelUp:: in
						// programs with non-standard handling of the Alt key, such as Firefox.
						if ((kbdMsSender.modifiersLRLogicalNonIgnored & (MOD_LWIN | MOD_RWIN | MOD_LALT | MOD_RALT)) == 0)
						{
							if ((modLR & (MOD_LCONTROL | MOD_RCONTROL | MOD_LSHIFT | MOD_RSHIFT)) != 0)
							{
								// v1.1.27.01: Since this key being released is Ctrl/Shift and Win is not down, this must
								// be in combination with Alt, which can be disguised by this event.  By contrast, if the
								// Win key was down and undisguisedMenuInEffect == true (meaning there was no Ctrl/Shift
								// down event prior to this up event), this event needs to be disguised for the reason
								// described above.
								undisguisedMenuInEffect = false;
							}

							disguiseNextMenu = false;
						}

						// Since the below call to KeyEvent() calls the keybd hook recursively, a quick down-and-up
						// is all that is necessary to disguise the key.  This is because the OS will see that the
						// keystroke occurred while ALT or WIN is still down because we haven't done CallNextHookEx() yet.
						if (undisguisedMenuInEffect)
							kbdMsSender.SendKeyEventMenuMask(KeyEventTypes.KeyDownAndUp); // This should also cause undisguisedMenuInEffect to be reset.
					}
					else // A modifier key was released and sDisguiseNextMenu was false.
					{
						// Now either no menu keys are down or they have been disguised by this keyup event.
						// Key-repeat may put the menu key back into effect, but that will be detected above.
						undisguisedMenuInEffect = false;
					}
				} // It's a modifier key.
				else // It's not a modifier key.
				{
					// Any key press or release serves to disguise the menu key.
					undisguisedMenuInEffect = false;
				}
			} // Keyboard vs. mouse hook.

			// Since above didn't return, this keystroke is being passed through rather than suppressed.
			if (hm.hsResetUponMouseClick && (vk == VK_LBUTTON || vk == VK_RBUTTON)) // v1.0.42.03
			{
				hm.ClearBuf();
			}

			// In case CallNextHookEx() is high overhead or can sometimes take a long time to return,
			// call it before posting the messages.  This solves conditions in which the main thread is
			// able to launch a script subroutine before the hook thread can finish updating its key state.
			// Search on AHK_HOOK_HOTKEY in this file for more comments.
			var resultToReturn = CallNextHookEx(hook, code, new IntPtr(param), ref kbd);
			SendHotkeyMessages(keyUp, extraInfo, keyHistoryCurr, hotkeyIDToPost, variant, hsOut, caseConformMode, endChar);
			return resultToReturn.ToInt64();
		}

		/// <summary>
		/// Caller must verify that aWhichHook and aWhichHookAlways accurately reflect the hooks that should
		/// be active when we return.  For example, the caller must have already taken into account which
		/// hotkeys/hotstrings are suspended, disabled, etc.
		///
		/// Caller should always be the main thread, never the hook thread.
		/// One reason is that this function isn't thread-safe.  Another is that new/delete/malloc/free
		/// themselves might not be thread-safe when the single-threaded CRT libraries are in effect
		/// (not using multi-threaded libraries due to a 3.5 KB increase in compressed code size).
		///
		/// The input params are unnecessary because could just access directly by using Hotkey::shk[].
		/// But aHK is a little more concise.
		/// aWhichHookAlways was added to force the hooks to be installed (or stay installed) in the case
		/// of #InstallKeybdHook and #InstallMouseHook.  This is so that these two commands will always
		/// still be in effect even if hotkeys are suspended, so that key history can still be monitored via
		/// the hooks.
		/// Returns the set of hooks that are active after processing is complete.
		/// </summary>
		internal override void ChangeHookState(List<HotkeyDefinition> hks, HookType whichHook, HookType whichHookAlways)
		{
			// Determine the set of hooks that should be activated or deactivated.
			var hooksToBeActive = whichHook | whichHookAlways; // Bitwise union.

			if (hooksToBeActive == 0) // No need to check any further in this case.  Just remove all hooks.
			{
				AddRemoveHooks(0); // Remove all hooks.
				return;
			}

			// Even if hooks_to_be_active indicates no change to hook status, we still need to continue in case
			// this is a suspend or unsuspend operation.  In both of those cases, though the hook(s)
			// may already be active, the hotkey configuration probably needs to be updated.
			// Related: Even if aHK_count is zero, still want to install the hook(s) whenever
			// aWhichHookAlways specifies that they should be.  This is done so that the
			// #InstallKeybdHook and #InstallMouseHook directives can have the hooks installed just
			// for use with something such as the KeyHistory feature, or for Hotstrings, Numlock AlwaysOn,
			// the Input command, and possibly others.

			// Now we know that at least one of the hooks is a candidate for activation.
			// Set up the arrays process all of the hook hotkeys even if the corresponding hook won't
			// become active (which should only happen if g_IsSuspended is true
			// and it turns out there are no suspend-hotkeys that are handled by the hook).

			// These arrays are dynamically allocated so that memory is conserved in cases when
			// the user doesn't need the hook at all (i.e. just normal registered hotkeys).
			// This is a waste of memory if there are no hook hotkeys, but currently the operation
			// of the hook relies upon these being allocated, even if the arrays are all clean
			// slates with nothing in them (it could check if the arrays are NULL but then the
			// performance would be slightly worse for the "average" script).  Presumably, the
			// caller is requesting the keyboard hook with zero hotkeys to support the forcing
			// of Num/Caps/ScrollLock always on or off (a fairly rare situation, probably):
			if (kvk == null || kvk.Length == 0)  // Since it's an initialized global, this indicates that all 4 objects are not yet allocated.
			{
				var kbd = script.KeyboardData;
				kvk = new KeyType[VK_ARRAY_COUNT];
				ksc = new KeyType[SC_ARRAY_COUNT];

				for (var i = 0u; i < kvk.Length; i++)
					kvk[i] = new KeyType(i);

				for (var i = 0u; i < ksc.Length; i++)
					ksc[i] = new KeyType(i);

				kvkm = new uint[KVKM_SIZE];
				kscm = new uint[KSCM_SIZE];
				hotkeyUp = new List<uint>(hks.Count);

				for (var i = 0; i < hks.Count; i++)
					hotkeyUp.Add(0);

				// Below is also a one-time-only init:
				// This attribute is exists for performance reasons (avoids a function call in the hook
				// procedure to determine this value):
				kvk[VK_CONTROL].asModifiersLR = MOD_LCONTROL | MOD_RCONTROL;
				kvk[VK_LCONTROL].asModifiersLR = MOD_LCONTROL;
				kvk[VK_RCONTROL].asModifiersLR = MOD_RCONTROL;
				kvk[VK_MENU].asModifiersLR = MOD_LALT | MOD_RALT;
				kvk[VK_LMENU].asModifiersLR = MOD_LALT;
				kvk[VK_RMENU].asModifiersLR = MOD_RALT;
				kvk[VK_SHIFT].asModifiersLR = MOD_LSHIFT | MOD_RSHIFT;
				kvk[VK_LSHIFT].asModifiersLR = MOD_LSHIFT;
				kvk[VK_RSHIFT].asModifiersLR = MOD_RSHIFT;
				kvk[VK_LWIN].asModifiersLR = MOD_LWIN;
				kvk[VK_RWIN].asModifiersLR = MOD_RWIN;
				// This is a bit iffy because it's far from certain that these particular scan codes
				// are really modifier keys on anything but a standard English keyboard.  However,
				// long years of use haven't shown this to be a problem, and there are certainly other
				// parts of the code that do not support custom layouts remapping the modifier keys.
				ksc[LControl].asModifiersLR = MOD_LCONTROL;
				ksc[RControl].asModifiersLR = MOD_RCONTROL;
				ksc[LAlt].asModifiersLR = MOD_LALT;
				ksc[RAlt].asModifiersLR = MOD_RALT;
				ksc[LShift].asModifiersLR = MOD_LSHIFT;
				ksc[RShift].asModifiersLR = MOD_RSHIFT;
				ksc[LWin].asModifiersLR = MOD_LWIN;
				ksc[RWin].asModifiersLR = MOD_RWIN;
				// Use the address rather than the value, so that if the global var's value
				// changes during runtime, ours will too:
				kvk[VK_SCROLL].forceToggle = kbd.toggleStates;
				kvk[VK_CAPITAL].forceToggle = kbd.toggleStates;
				kvk[VK_NUMLOCK].forceToggle = kbd.toggleStates;
			}

			// Init only those attributes which reflect the hotkey's definition, not those that reflect
			// the key's current status (since those are initialized only if the hook state is changing
			// from OFF to ON (later below):

			foreach (var k in kvk)
				k.ResetKeyTypeAttrib();

			foreach (var k in ksc)
				k.ResetKeyTypeAttrib();// Note: ksc not kvk.

			// Indicate here which scan codes should override their virtual keys:
			foreach (var kv in keyToSc)
				if (kv.Value > 0 && kv.Value <= ksc.Length)
					ksc[kv.Value].scTakesPrecedence = true;

			// These have to be initialized with element value INVALID.
			System.Array.Fill(kvkm, HotkeyDefinition.HOTKEY_ID_INVALID);
			System.Array.Fill(kscm, HotkeyDefinition.HOTKEY_ID_INVALID);

			for (var i = 0; i < hotkeyUp.Count; i++)
				hotkeyUp[i] = HotkeyDefinition.HOTKEY_ID_INVALID;

			KeyType thisKey;
			var shk = script.HotkeyData.shk;
			var hkSorted = new List<HkSortedType>(shk.Count);

			for (var i = 0; i < hks.Count; ++i)
			{
				var hk = hks[i];

				// If it's not a hook hotkey (e.g. it was already registered with RegisterHotkey() or it's a joystick
				// hotkey) don't process it here.  Similarly, if g_IsSuspended is true, we won't include it unless it's
				// exempt from suspension:
				if (!HotkeyDefinition.HK_TYPE_IS_HOOK(hk.type)
						|| (A_IsSuspended && !hk.IsExemptFromSuspend())
						|| hk.IsCompletelyDisabled()) // Listed last for short-circuit performance.
					continue;

				// Rule out the possibility of obnoxious values right away, preventing array-out-of bounds, etc.:
				if ((hk.vk == 0 && hk.sc == 0) || hk.vk > VK_MAX || hk.sc > SC_MAX)
					continue;

				if (hk.vk == 0)
				{
					// scan codes don't need something like the switch stmt below because they can't be neutral.
					// In other words, there's no scan code equivalent for something like VK_CONTROL.
					// In addition, SC_LCONTROL, for example, doesn't also need to change the kvk array
					// for VK_LCONTROL because the hook knows to give the scan code precedence, and thus
					// look it up only in the ksc array in that case.
					thisKey = ksc[hk.sc];
					// For some scan codes this was already set above.  But to support explicit scan code hotkeys,
					// such as "SC102::MsgBox", make sure it's set for every hotkey that uses an explicit scan code.
					thisKey.scTakesPrecedence = true;
				}
				else
				{
					thisKey = kvk[hk.vk];

					// Keys that have a neutral as well as a left/right counterpart must be
					// fully initialized since the hook can receive the left, the right, or
					// the neutral (neutral only if another app calls KeyEvent(), probably).
					// There are several other switch stmts in this function like the below
					// that serve a similar purpose.  The alternative to doing all these
					// switch stmts is to always translate left/right vk's (whose sc's don't
					// take precedence) in the KeyboardProc() itself.  But that would add
					// the overhead of a switch stmt to *every* keypress ever made on the
					// system, so it seems better to set up everything correctly here since
					// this init section is only done once.
					switch (hk.vk)
					{
						case VK_MENU:
							// It's not strictly necessary to init all of these, since the
							// hook currently never handles VK_RMENU, for example, by its
							// vk (it uses sc instead).  But it's safest to do all of them
							// in case future changes ever ruin that assumption:
							kvk[VK_LMENU].usedAsSuffix = true;
							kvk[VK_RMENU].usedAsSuffix = true;
							ksc[LAlt].usedAsSuffix = true;
							ksc[RAlt].usedAsSuffix = true;

							if (hk.keyUp) // Fix for v1.1.07.03: Set only if true in case there was already an "up" hotkey.
							{
								kvk[VK_LMENU].usedAsKeyUp = true;
								kvk[VK_RMENU].usedAsKeyUp = true;
								ksc[LAlt].usedAsKeyUp = true;
								ksc[RAlt].usedAsKeyUp = true;
							}

							break;

						case VK_SHIFT:
							// The neutral key itself is also set to be a suffix further below.
							kvk[VK_LSHIFT].usedAsSuffix = true;
							kvk[VK_RSHIFT].usedAsSuffix = true;
							ksc[LShift].usedAsSuffix = true;
							ksc[RShift].usedAsSuffix = true;

							if (hk.keyUp) // Fix for v1.1.07.03: Set only if true in case there was already an "up" hotkey.
							{
								kvk[VK_LSHIFT].usedAsKeyUp = true;
								kvk[VK_RSHIFT].usedAsKeyUp = true;
								ksc[LShift].usedAsKeyUp = true;
								ksc[RShift].usedAsKeyUp = true;
							}

							break;

						case VK_CONTROL:
							kvk[VK_LCONTROL].usedAsSuffix = true;
							kvk[VK_RCONTROL].usedAsSuffix = true;
							ksc[LControl].usedAsSuffix = true;
							ksc[RControl].usedAsSuffix = true;

							if (hk.keyUp) // Fix for v1.1.07.03: Set only if true in case there was already an "up" hotkey.
							{
								kvk[VK_LCONTROL].usedAsKeyUp = true;
								kvk[VK_RCONTROL].usedAsKeyUp = true;
								ksc[LControl].usedAsKeyUp = true;
								ksc[RControl].usedAsKeyUp = true;
							}

							break;
							// Later might want to add cases for VK_LCONTROL and such, but for right now,
							// these keys should never come up since they're done by scan code?
					}
				}

				thisKey.usedAsSuffix = true;
				var hotkeyIdWithFlags = hk.id;

				if (hk.keyUp)
				{
					thisKey.usedAsKeyUp = true;
					hotkeyIdWithFlags |= HotkeyDefinition.HOTKEY_KEY_UP;
				}

				var hkIsCustomCombo = hk.modifierVK != 0 || hk.modifierSC != 0;

				// If this is a naked (unmodified) modifier key, make it a prefix if it ever modifies any
				// other hotkey.  This processing might be later combined with the hotkeys activation function
				// to eliminate redundancy / improve efficiency, but then that function would probably need to
				// init everything else here as well:
				if (thisKey.asModifiersLR != 0 && hk.modifiersConsolidatedLR == 0 && !hkIsCustomCombo
						&& (hk.noSuppress & HotkeyDefinition.AT_LEAST_ONE_VARIANT_HAS_TILDE) == 0) // v1.0.45.02: ~Alt, ~Control, etc. should fire upon press-down, not release (broken by 1.0.44's PREFIX_FORCED, but I think it was probably broken in pre-1.0.41 too).
					SetModifierAsPrefix(hk.vk, hk.sc);

				if (hkIsCustomCombo)
				{
					if (hk.modifierVK != 0)
					{
						if (kvk[hk.modifierVK].asModifiersLR != 0)
							// The hotkey's ModifierVK is itself a modifier.
							SetModifierAsPrefix(hk.modifierVK, 0, true);
						else
						{
							kvk[hk.modifierVK].usedAsPrefix = KeyType.PREFIX_ACTUAL;

							if ((hk.noSuppress & HotkeyDefinition.NO_SUPPRESS_PREFIX) != 0)
								kvk[hk.modifierVK].noSuppress |= HotkeyDefinition.AT_LEAST_ONE_COMBO_HAS_TILDE;
						}
					}
					else //if (hk.mModifierSC)
					{
						if (ksc[hk.modifierSC].asModifiersLR != 0)  // Fixed for v1.0.35.13 (used to be kvk vs. ksc).
							// The hotkey's ModifierSC is itself a modifier.
							SetModifierAsPrefix(0, hk.modifierSC, true);
						else
						{
							ksc[hk.modifierSC].usedAsPrefix = KeyType.PREFIX_ACTUAL;

							if ((hk.noSuppress & HotkeyDefinition.NO_SUPPRESS_PREFIX) != 0)
								ksc[hk.modifierSC].noSuppress |= HotkeyDefinition.AT_LEAST_ONE_COMBO_HAS_TILDE;

							// For some scan codes this was already set above.  But to support explicit scan code prefixes,
							// such as "SC118 & SC122::MsgBox", make sure it's set for every prefix that uses an explicit
							// scan code:
							ksc[hk.modifierSC].scTakesPrecedence = true;
						}
					}

					// Insert this hotkey at the front of the linked list of hotkeys which use this suffix key.
					hk.nextHotkey = thisKey.firstHotkey;
					thisKey.firstHotkey = hk.id;
					continue;
				}

				// At this point, since the above didn't "continue", this hotkey is one without a ModifierVK/SC.
				// Put it into a temporary array, which will be later sorted:
				var hkst = new HkSortedType
				{
					idWithFlags = hk.hookAction != 0 ? hk.hookAction : hotkeyIdWithFlags,
					vk = hk.vk,
					sc = hk.sc,
					modifiers = hk.modifiers,
					modifiersLR = hk.modifiersLR,
					allowExtraModifiers = hk.allowExtraModifiers
				};
				hkSorted.Add(hkst);
			}

			if (hkSorted.Count != 0)
			{
				// It's necessary to get them into this order to avoid problems that would be caused by
				// AllowExtraModifiers:
				hkSorted.Sort(HkSortedType.SortMostGeneralBeforeLeast);
				// For each hotkey without a ModifierVK/SC (which override normal modifiers), expand its modifiers and
				// modifiersLR into its column in the kvkm or kscm arrays.
				uint modifiers, modifiersMerged;
				uint modifiersLRExcluded;
				uint modifiersLR;  // Don't make this modLR_type to avoid integer overflow, since it's a loop-counter.
				bool prevHkIsKeyUp, thisHkIsKeyUp;
				int prevHkId, thisHkId;

				for (var i = 0; i < hkSorted.Count; ++i)
				{
					var thisHk = hkSorted[i];
					thisHkIsKeyUp = (thisHk.idWithFlags & HotkeyDefinition.HOTKEY_KEY_UP) != 0;
					thisHkId = (int)thisHk.idWithFlags & HotkeyDefinition.HOTKEY_ID_MASK;

					if (thisHkId <= HotkeyDefinition.HOTKEY_ID_MAX) // It's a valid ID and not an ALT_TAB action.
					{
						// Insert this hotkey at the front of the list of hotkeys that use this suffix key.
						// This enables fallback between overlapping hotkeys, such as LCtrl & a, <^+a, ^+a.
						thisKey = thisHk.vk != 0 ? kvk[thisHk.vk] : ksc[thisHk.sc];
						// Insert after any custom combos.
						ref var first = ref thisKey.firstHotkey;

						while (first != HotkeyDefinition.HOTKEY_ID_INVALID && (hks[(int)first].modifierVK != 0 || hks[(int)first].modifierSC != 0))
							first = ref hks[(int)first].nextHotkey;

						hks[thisHkId].nextHotkey = first;
						first = (uint)thisHkId;
					}

					modifiersMerged = thisHk.modifiers;

					if (thisHk.modifiersLR != 0)
						modifiersMerged |= ConvertModifiersLR(thisHk.modifiersLR);

					// Fixed for v1.1.27.00: Calculate the modifiersLR bits which are NOT allowed to be set.
					// This fixes <^A erroneously taking over <>^A, and reduces the work that must be done
					// on each iteration of the loop below.
					modifiersLRExcluded = thisHk.allowExtraModifiers ? 0
										  : ~(thisHk.modifiersLR | ConvertModifiers(thisHk.modifiers));

					for (modifiersLR = 0; modifiersLR <= MODLR_MAX; ++modifiersLR)  // For each possible LR value.
					{
						if ((modifiersLR & modifiersLRExcluded) != 0) // Checked first to avoid the ConvertModifiersLR call in many cases.
							continue;

						modifiers = ConvertModifiersLR(modifiersLR);

						// Below is true if modifiersLR is a superset of i's modifier value.  In other words,
						// modifiersLR has the minimum required keys.  It may also have some extraneous keys,
						// but only if they were not excluded by the check above, in which case they are allowed.
						if (modifiersMerged != (modifiers & modifiersMerged))
							continue;

						// In addition to the above, modifiersLR must also have the *specific* left or right keys
						// found in i's modifiersLR.  In other words, i's modifiersLR must be a perfect subset
						// of modifiersLR:
						if (thisHk.modifiersLR != 0) // make sure that any more specific left/rights are also present.
							if (thisHk.modifiersLR != (modifiersLR & thisHk.modifiersLR))
								continue;

						// scan codes don't need the switch() stmt below because, for example,
						// the hook knows to look up left-control by only SC_LCONTROL, not VK_LCONTROL.
						var doCascade = thisHk.vk != 0;
						// If above didn't "continue", modifiersLR is a valid hotkey combination so set it as such:
						ref var itsTableEntry = ref (thisHk.vk != 0 ? ref Kvkm(modifiersLR, thisHk.vk) : ref Kscm(modifiersLR, thisHk.sc));

						if (itsTableEntry == HotkeyDefinition.HOTKEY_ID_INVALID) // Since there is no ID currently in the slot, key-up/down doesn't matter.
						{
							itsTableEntry = thisHk.idWithFlags;
						}
						else
						{
							prevHkId = (int)(itsTableEntry & HotkeyDefinition.HOTKEY_ID_MASK);

							if (thisHkId >= shk.Count || prevHkId >= shk.Count) // AltTab hotkey.
								continue; // Exclude AltTab hotkeys since hotkey_up[] and shk[] can't be used.

							prevHkIsKeyUp = (itsTableEntry & HotkeyDefinition.HOTKEY_KEY_UP) != 0;

							if (thisHkIsKeyUp && !prevHkIsKeyUp) // Override any existing key-up hotkey for this down hotkey ID, e.g. "LButton Up" takes precedence over "*LButton Up".
							{
								var prevHk = shk[prevHkId];

								// v1.1.33.03: Since modifiers aren't checked when hotkey_to_fire_upon_release is used
								// to fire a key-up hotkey, avoid setting setting this_hk as prev_hk's up hotkey when:
								//   a) prev_hk permits modifiers that this_hk does not permit (i.e. requires to be up).
								//   b) this_hk requires modifiers that prev_hk does not require (i.e. might not be pressed).
								//
								//  a up::    ; Doesn't permit any modifiers.
								//  *a::      ; Permits all modifiers, so shouldn't necessarily fire "a up".
								//  <^b up::  ; Doesn't permit RCtrl.
								//  ^b::      ; Permits RCtrl, so shouldn't necessarily fire "<^b up".
								//  *^c up::  ; Requires Ctrl.
								//  *+c::     ; Doesn't require Ctrl, so shouldn't necessarily fire "^c up".
								//
								// Note that prev_hk.mModifiersConsolidatedLR includes all LR modifiers that CAN be down,
								// but some might not be required, so might not be down (e.g. ^b has MOD_LCTRL|MOD_RCTRL).
								// However, if either LCTRL or RCTRL is set there, we know CTRL will be down, so the result
								// of ConvertModifiersLR() tells us which neutral modifiers will definitely be down.
								// prev_hk.mModifiers is checked first to avoid the function call where possible.
								if (((prevHk.allowExtraModifiers ? MODLR_MAX : prevHk.modifiersConsolidatedLR) & modifiersLRExcluded) == 0
										&& (thisHk.modifiersLR & ~prevHk.modifiersLR) == 0
										&& ((thisHk.modifiers & ~prevHk.modifiers) == 0
											|| (thisHk.modifiers & ~ConvertModifiersLR(prevHk.modifiersConsolidatedLR)) == 0))
								{
									hotkeyUp[prevHkId] = thisHk.idWithFlags;
									doCascade = false;  // Every place the down-hotkey ID already appears, it will point to this same key-up hotkey.
								}
								else
								{
									// v1.1.33.03: Override the lower-priority key-down hotkey which was already present.
									// Hotkey::FindPairedHotkey will be used to locate a key-down hotkey to fire based on
									// current modifier state.
									itsTableEntry = thisHk.idWithFlags;
								}
							}
							else
							{
								uint newUpId;

								if (!thisHkIsKeyUp && prevHkIsKeyUp)
									// Swap them so that the down-hotkey is in the main array and the up in the secondary:
									newUpId = itsTableEntry;
								else if (prevHkIsKeyUp || hotkeyUp[thisHkId] != HotkeyDefinition.HOTKEY_ID_INVALID)
									// Both are key-up hotkeys, or this_hk already has a key-up hotkey, in which case it
									// isn't overwritten since there's no guarantee the new one is more appropriate, and
									// it can cause the effect of swapping hotkey_up[] between two values repeatedly.
									newUpId = HotkeyDefinition.HOTKEY_ID_INVALID;
								else // Both are key-down hotkeys.
									// Fix for v1.0.40.09: Also copy the previous hotkey's corresponding up-hotkey (if any)
									// so that this hotkey will have that same one.  This also solves the issue of a hotkey
									// such as "^!F1" firing twice (once for down and once for up) when "*F1" and "*F1 up"
									// are both hotkeys.  Instead, the "*F1 up" hotkey should fire upon release of "^!F1"
									// so that the behavior is consistent with the case where "*F1" isn't present as a hotkey.
									// This fix doesn't appear to break anything else, most notably it still allows a hotkey
									// such as "^!F1 up" to take precedence over "*F1 up" because in such a case, this
									// code would never have executed because prev_hk_is_key_up would be true but
									// this_hk_is_key_up would be false.  Note also that sort_most_general_before_least()
									// has put key-up hotkeys after their key-down counterparts in the list.
									// v1.1.33.03: Without this "^!F1" won't fire twice, but it also won't fire "*F1 up".
									newUpId = hotkeyUp[prevHkId];

								if (newUpId != HotkeyDefinition.HOTKEY_ID_INVALID)
								{
									var new_up_hk = shk[(int)newUpId & HotkeyDefinition.HOTKEY_ID_MASK];

									// v1.1.33.03: Since modifiers aren't checked when hotkey_to_fire_upon_release is used
									// to fire a key-up hotkey, avoid setting setting new_up_hk as this_hk's up hotkey when:
									//   a) this_hk permits modifiers that new_up_hk does not.
									//   b) new_up_hk requires modifiers that this_hk does not.
									//
									//  <^a up::  ; Does not permit RCtrl.
									//  ^a::      ; Permits RCtrl, so shouldn't necessarily fire "<^a up".
									//  *!1 up::  ; Requires Alt.
									//  *<^1::    ; Doesn't require Alt, so shouldn't necessarily fire "*!1 up".
									//
									// ~i_modifiersLR_excluded already accounts for this_hk.AllowExtraModifiers.
									//if (  !(modLR_type)(~i_modifiersLR_excluded & (new_up_hk.mAllowExtraModifiers ? 0 : ~new_up_hk.mModifiersConsolidatedLR))  )
									if ((new_up_hk.allowExtraModifiers || (~modifiersLRExcluded & ~new_up_hk.modifiersConsolidatedLR) == 0)
											&& (new_up_hk.modifiers & ~modifiersMerged) == 0 && (new_up_hk.modifiersLR & ~thisHk.modifiersLR) == 0)
										hotkeyUp[thisHkId] = newUpId;
								}

								// Either both are key-up hotkeys or both are key-down hotkeys.  this overrides prev.
								itsTableEntry = thisHk.idWithFlags;
							}
						}

						if (doCascade)
						{
							if (thisHk.vk == VK_MENU || thisHk.vk == VK_LMENU)
							{
								Kvkm(modifiersLR, VK_LMENU) = thisHk.idWithFlags;
								Kscm(modifiersLR, LAlt) = thisHk.idWithFlags;
							}

							if (thisHk.vk == VK_MENU || thisHk.vk == VK_RMENU)
							{
								Kvkm(modifiersLR, VK_RMENU) = thisHk.idWithFlags;
								Kscm(modifiersLR, RAlt) = thisHk.idWithFlags;
							}

							if (thisHk.vk == VK_SHIFT || thisHk.vk == VK_LSHIFT)
							{
								Kvkm(modifiersLR, VK_LSHIFT) = thisHk.idWithFlags;
								Kscm(modifiersLR, LShift) = thisHk.idWithFlags;
							}

							if (thisHk.vk == VK_SHIFT || thisHk.vk == VK_RSHIFT)
							{
								Kvkm(modifiersLR, VK_RSHIFT) = thisHk.idWithFlags;
								Kscm(modifiersLR, RShift) = thisHk.idWithFlags;
							}

							if (thisHk.vk == VK_CONTROL || thisHk.vk == VK_LCONTROL)
							{
								Kvkm(modifiersLR, VK_LCONTROL) = thisHk.idWithFlags;
								Kscm(modifiersLR, LControl) = thisHk.idWithFlags;
							}

							if (thisHk.vk == VK_CONTROL || thisHk.vk == VK_RCONTROL)
							{
								Kvkm(modifiersLR, VK_RCONTROL) = thisHk.idWithFlags;
								Kscm(modifiersLR, RControl) = thisHk.idWithFlags;
							}
						} // if (do_cascade)
					}
				}
			}

			// Support "Control", "Alt" and "Shift" as suffix keys by appending their lists of
			// custom combos to the lists used by their left and right versions.  This avoids the
			// need for the hook to detect these keys and perform a search through a second list.
			// This must be done after all custom combos have been processed above, since they
			// might be defined in any order, but the neutral hotkeys must be placed last.
			if (kvk[VK_SHIFT].usedAsSuffix) // Skip the following unless Shift, LShift or RShift was used as a suffix.
				LinkKeysForCustomCombo(VK_SHIFT, VK_LSHIFT, VK_RSHIFT);

			if (kvk[VK_CONTROL].usedAsSuffix)
				LinkKeysForCustomCombo(VK_CONTROL, VK_LCONTROL, VK_RCONTROL);

			if (kvk[VK_MENU].usedAsSuffix)
				LinkKeysForCustomCombo(VK_MENU, VK_LMENU, VK_RMENU);

			// Add or remove hooks, as needed.  No change is made if the hooks are already in the correct state.
			AddRemoveHooks(hooksToBeActive);
		}

		internal override uint CharToVKAndModifiers(char ch, ref uint? modifiersLR, IntPtr keybdLayout, bool enableAZFallback = false)
		// If non-NULL, pModifiersLR contains the initial set of modifiers provided by the caller, to which
		// we add any extra modifiers required to realize aChar.
		{
			// For v1.0.25.12, it seems best to avoid the many recent problems with linefeed (`n) being sent
			// as Ctrl+Enter by changing it to always send a plain Enter, just like carriage return (`r).
			if (ch == '\n')
				return VK_RETURN;

			// Otherwise:
			var modPlusVk = VkKeyScanEx(ch, keybdLayout); // v1.0.44.03: Benchmark shows that VkKeyScanEx() is the same speed as VkKeyScan() when the layout has been pre-fetched.
			var vk = (uint)(modPlusVk & 0xFF);
			var keyscanModifiers = (char)((modPlusVk >> 8) & 0xFF);

			if (keyscanModifiers == -1 && vk == 0xFF) // No translation could be made.
			{
				if (!(enableAZFallback && Strings.Cisalpha(ch)))
					return 0;

				// v1.1.27.00: Use the A-Z fallback; assume the user means vk41-vk5A, since these letters
				// are commonly used to describe keyboard shortcuts even when these vk codes are actually
				// mapped to other characters.  Our callers should pass false for aEnableAZFallback if
				// they require a strict printable character.keycode mapping, such as for sending text.
				vk = char.ToUpper(ch);
				keyscanModifiers = (char)(Strings.Cisupper(ch) ? 0x01 : 0); // It's debatable whether the user intends this to be Shift+letter; this at least makes `Send ^A` consistent across (most?) layouts.
			}

			if ((keyscanModifiers & 0x38) != 0) // "The Hankaku key is pressed" or either of the "Reserved" state bits (for instance, used by Neo2 keyboard layout).
				return 0;// Callers expect failure in this case so that a fallback method can be used.

			// For v1.0.35, pModifiersLR was changed to modLR vs. mod so that AltGr keys such as backslash and
			// '{' are supported on layouts such as German when sending to apps such as Putty that are fussy about
			// which ALT key is held down to produce the character.  The following section detects AltGr by the
			// assuming that any character that requires both CTRL and ALT (with optional SHIFT) to be held
			// down is in fact an AltGr key (I don't think there are any that aren't AltGr in this case, but
			// confirmation would be nice).

			// The win docs for VkKeyScan() are a bit confusing, referring to flag "bits" when it should really
			// say flag "values".  In addition, it seems that these flag values are incompatible with
			// MOD_ALT, MOD_SHIFT, and MOD_CONTROL, so they must be translated:
			if (modifiersLR != null) // The caller wants this info added to the output param.
			{
				// Best not to reset this value because some callers want to retain what was in it before,
				// merely merging these new values into it:
				//*pModifiers = 0;
				if ((keyscanModifiers & 0x06) == 0x06) // 0x06 means "requires/includes AltGr".
				{
					// v1.0.35: The critical difference below is right vs. left ALT.  Must not include MOD_LCONTROL
					// because simulating the RAlt keystroke on these keyboard layouts will automatically
					// press LControl down.
					modifiersLR |= MOD_RALT;
				}
				else // Do normal/default translation.
				{
					// v1.0.40: If caller-supplied modifiers already include the right-side key, no need to
					// add the left-side key (avoids unnecessary keystrokes).
					if ((keyscanModifiers & 0x02) != 0 && (modifiersLR & (MOD_LCONTROL | MOD_RCONTROL)) == 0)
						modifiersLR |= MOD_LCONTROL; // Must not be done if requires_altgr==true, see above.

					if ((keyscanModifiers & 0x04) != 0 && (modifiersLR & (MOD_LALT | MOD_RALT)) == 0)
						modifiersLR |= MOD_LALT;
				}

				// v1.0.36.06: Done unconditionally because presence of AltGr should not preclude the presence of Shift.
				// v1.0.40: If caller-supplied modifiers already contains MOD_RSHIFT, no need to add LSHIFT (avoids
				// unnecessary keystrokes).
				if ((keyscanModifiers & 0x01) != 0 && ((modifiersLR & (MOD_LSHIFT | MOD_RSHIFT)) == 0))
					modifiersLR |= MOD_LSHIFT;
			}

			return vk;
		}

		internal unsafe bool CollectHotstring(ref KBDLLHOOKSTRUCT ev, char[] ch, int charCount, IntPtr activeWindow,
											  KeyHistoryItem keyHistoryCurr, ref HotstringDefinition hsOut, ref CaseConformModes caseConformMode, ref char endChar)
		{
			var suppressHotstringFinalChar = false; // Set default.
			var hm = script.HotstringManager;

			if (activeWindow != hsHwnd)
			{
				// Since the buffer tends to correspond to the text to the left of the caret in the
				// active window, if the active window changes, it seems best to reset the buffer
				// to avoid misfires.
				hsHwnd = activeWindow;
				hm.ClearBuf();
			}
			else if (hm.hsBuf.Count > 90)
				hm.hsBuf.RemoveRange(0, 45);

			hm.hsBuf.Add(ch[0]);

			if (charCount > 1)
				// MSDN: "This usually happens when a dead-key character (accent or diacritic) stored in the
				// keyboard layout cannot be composed with the specified virtual key to form a single character."
				hm.hsBuf.Add(ch[1]);

			if (hm.MatchHotstring() is HotstringDefinition hs)
			{
				int cpcaseStart, cpcaseEnd;
				int caseCapableCharacters;
				bool firstCharWithCaseIsUpper, firstCharWithCaseHasGoneBy;
				var hsBufSpan = (ReadOnlySpan<char>)CollectionsMarshal.AsSpan(hm.hsBuf);
				var hsLength = hsBufSpan.Length;
				var hsBufCountm1 = hsLength - 1;
				var hsBufCountm2 = hsLength - 2;
				var hasEndChar = hm.defEndChars.Contains(hsBufSpan[hsBufCountm1]);

				if (HotInputLevelAllowsFiring(hs.inputLevel, ev.dwExtraInfo, ref keyHistoryCurr.eventType))
				{
					// Since default KeyDelay is 0, and since that is expected to be typical, it seems
					// best to unconditionally post a message rather than trying to handle the backspacing
					// and replacing here.  This is because a KeyDelay of 0 might be fairly slow at
					// sending keystrokes if the system is under heavy load, in which case we would
					// not be returning to our caller in a timely fashion, which would case the OS to
					// think the hook is unresponsive, which in turn would cause it to timeout and
					// route the key through anyway (testing confirms this).
					if (!hs.conformToCase)
					{
						caseConformMode = CaseConformModes.None;
					}
					else
					{
						// Find out what case the user typed the string in so that we can have the
						// replacement produced in similar case:
						cpcaseEnd = hsLength;

						if (hs.endCharRequired)
							--cpcaseEnd;

						// Bug-fix for v1.0.19: First find out how many of the characters in the abbreviation
						// have upper and lowercase versions (i.e. exclude digits, punctuation, etc):
						for (caseCapableCharacters = 0, firstCharWithCaseIsUpper = firstCharWithCaseHasGoneBy = false
								, cpcaseStart = cpcaseEnd - hs.str.Length
								; cpcaseStart < cpcaseEnd; ++cpcaseStart)
						{
							char chStart = hsBufSpan[cpcaseStart];

							if (char.IsLower(chStart) || char.IsUpper(chStart)) // A case-capable char.
							{
								if (!firstCharWithCaseHasGoneBy)
								{
									firstCharWithCaseHasGoneBy = true;

									if (char.IsUpper(chStart))
										firstCharWithCaseIsUpper = true; // Override default.
								}

								++caseCapableCharacters;
							}
						}

						if (caseCapableCharacters == 0) // All characters in the abbreviation are caseless.
							caseConformMode = CaseConformModes.None;
						else if (caseCapableCharacters == 1)
							// Since there is only a single character with case potential, it seems best as
							// a default behavior to capitalize the first letter of the replacement whenever
							// that character was typed in uppercase.  The behavior can be overridden by
							// turning off the case-conform mode.
							caseConformMode = firstCharWithCaseIsUpper ? CaseConformModes.FirstCap : CaseConformModes.None;
						else // At least two characters have case potential. If all of them are upper, use ALL_CAPS.
						{
							if (!firstCharWithCaseIsUpper) // It can't be either FIRST_CAP or ALL_CAPS.
							{
								caseConformMode = CaseConformModes.None;
							}
							else // First char is uppercase, and if all the others are too, this will be ALL_CAPS.
							{
								caseConformMode = CaseConformModes.FirstCap; // Set default.

								// Bug-fix for v1.0.19: Changed !IsCharUpper() below to IsCharLower() so that
								// caseless characters such as the @ symbol do not disqualify an abbreviation
								// from being considered "all uppercase":
								for (cpcaseStart = cpcaseEnd - hs.str.Length; cpcaseStart < cpcaseEnd; ++cpcaseStart)
									if (char.IsLower(hsBufSpan[cpcaseStart])) // Use IsCharLower to better support chars from non-English languages.
										break; // Any lowercase char disqualifies CASE_CONFORM_ALL_CAPS.

								if (cpcaseStart == cpcaseEnd) // All case-possible characters are uppercase.
									caseConformMode = CaseConformModes.AllCaps;

								//else leave it at the default set above.
							}
						}
					}

					if (hs.doBackspace || hs.omitEndChar && hs.endCharRequired) // Fix for v1.0.37.07: Added hs.mOmitEndChar so that B0+O will omit the ending character.
					{
						// Have caller suppress this final key pressed by the user, since it would have
						// to be backspaced over anyway.  Even if there is a visible Input command in
						// progress, this should still be okay since the input will still see the key,
						// it's just that the active window won't see it, which is okay since once again
						// it would have to be backspaced over anyway.  UPDATE: If an Input is in progress,
						// it should not receive this final key because otherwise the hotstring's backspacing
						// would backspace one too few times from the Input's point of view, thus the input
						// would have one extra, unwanted character left over (namely the first character
						// of the hotstring's abbreviation).  However, this method is not a complete
						// solution because it fails to work under a situation such as the following:
						// A hotstring script is started, followed by a separate script that uses the
						// Input command.  The Input script's hook will take precedence (since it was
						// started most recently), thus when the Hotstring's script's hook does sends
						// its replacement text, the Input script's hook will get a hold of it first
						// before the Hotstring's script has a chance to suppress it.  In other words,
						// The Input command will capture the ending character and then there will
						// be insufficient backspaces sent to clear the abbreviation out of it.  This
						// situation is quite rare so for now it's just mentioned here as a known limitation.
						suppressHotstringFinalChar = true;
					}

					// Post the message rather than sending it, because Send would need
					// SendMessageTimeout(), which is undesirable because the whole point of
					// making this hook thread separate from the main thread is to have it be
					// maximally responsive (especially to prevent mouse cursor lag).
					// Put the end char in the LOWORD and the case_conform_mode in the HIWORD.
					// Casting to UCHAR might be necessary to avoid problems when MAKELONG
					// casts a signed char to an unsigned WORD.
					// UPDATE: In v1.0.42.01, the message is posted later (by our caller) to avoid
					// situations in which the message arrives and is processed by the main thread
					// before we finish processing the hotstring's final keystroke here.  This avoids
					// problems with a script calling GetKeyState() and getting an inaccurate value
					// because the hook thread is either pre-empted or is running in parallel
					// (multiprocessor) and hasn't yet returned 1 or 0 to determine whether the final
					// keystroke is suppressed or passed through to the active window.
					// UPDATE: In v1.0.43, the ending character is not put into the Lparam when
					// hs.mDoBackspace is false.  This is because:
					// 1) When not backspacing, it's more correct that the ending character appear where the
					//    user typed it rather than appearing at the end of the replacement.
					// 2) Two ending characters would appear in pre-1.0.43 versions: one where the user typed
					//    it and one at the end, which is clearly incorrect.
					hsOut = hs;
					endChar = hs.endCharRequired ? hsBufSpan[hsBufCountm1] : (char)0;

					// Clean up.
					// The keystrokes to be sent by the other thread upon receiving the message prepared above
					// will not be received by this function because:
					// 1) CollectInput() is not called for simulated keystrokes.
					// 2) The keyboard hook is absent during a SendInput hotstring.
					// 3) The keyboard hook does not receive SendPlay keystrokes (if hotstring is of that type).
					// Consequently, the buffer should be adjusted below to ensure it's in the right state to work
					// in situations such as the user typing two hotstrings consecutively where the ending
					// character of the first is used as a valid starting character (non-alphanumeric) for the next.
					if (!string.IsNullOrEmpty(hs.replacement))
					{
						// Since the buffer no longer reflects what is actually on screen to the left
						// of the caret position (since a replacement is about to be done), reset the
						// buffer, except for any end-char (since that might legitimately form part
						// of another hot string adjacent to the one just typed).  The end-char
						// sent by DoReplace() won't be captured (since it's "ignored input", which
						// is why it's put into the buffer manually here):
						if (hs.endCharRequired)
							hm.hsBuf.RemoveRange(0, hm.hsBuf.Count - 1);
						else
							hm.ClearBuf();
					}
					else if (hs.doBackspace)
					{
						// It's *not* a replacement, but we're doing backspaces, so adjust buf for backspaces
						// and the fact that the final char of the HS (if no end char) or the end char
						// (if end char required) will have been suppressed and never made it to the
						// active window.  A simpler way to understand is to realize that the buffer now
						// contains (for recognition purposes, in its right side) the hotstring and its
						// end char (if applicable), so remove both:
						hm.hsBuf.RemoveRange(hm.hsBuf.Count - hs.str.Length, hs.str.Length);

						if (hs.endCharRequired)
							hm.hsBuf.RemoveAt(hm.hsBuf.Count - 1);
					}

					// v1.0.38.04: Fixed the following mDoReset section by moving it beneath the above because
					// the above relies on the fact that the buffer has not yet been reset.
					// v1.0.30: mDoReset was added to prevent hotstrings such as the following
					// from firing twice in a row, if you type 11 followed by another 1 afterward:
					//:*?B0:11::
					//MsgBox,0,test,%A_ThisHotkey%,1 ; Show which key was pressed and close the window after a second.
					//return
					// There are probably many other uses for the reset option (albeit obscure, but they have
					// been brought up in the forum at least twice).
					if (hs.doReset)
						hm.ClearBuf(); // Further below, the buffer will be terminated to reflect this change.
				}//for each hotstring for this letter.
			}//if hotstring buffer not empty.

			return !suppressHotstringFinalChar;
		}

		internal bool CollectInput(ref KBDLLHOOKSTRUCT ev, uint vk, uint sc, bool keyUp, bool isIgnored
								   , CollectInputState state, KeyHistoryItem keyHistoryCurr, ref HotstringDefinition hsOut
								   , ref CaseConformModes caseConformMode, ref char endChar
								  )
		// Caller is responsible for having initialized aHotstringWparamToPost to HOTSTRING_INDEX_INVALID.
		// Returns true if the caller should treat the key as visible (non-suppressed).
		// Always use the parameter vk rather than event.vkCode because the caller or caller's caller
		// might have adjusted vk, namely to make it a left/right specific modifier key rather than a
		// neutral one.
		{
			if (!state.earlyCollected && !EarlyCollectInput(ref ev, vk, sc, keyUp, isIgnored, state, keyHistoryCurr))
				return false;

			if (keyUp)
				return CollectKeyUp(ref ev, vk, sc, false);

			int charCount = state.charCount;
			var activeWindow = state.activeWindow;
			var activeWindowKeybdLayout = state.keyboardLayout;
			var ch = state.ch;
			var sb = new StringBuilder(8);
			var hm = script.HotstringManager;
			var hsBuf = hm.hsBuf;

			if (!CollectInputHook(ref ev, vk, sc, ch, charCount, false))
				return false; // Suppress.

			// Hotstrings monitor neither ignored input nor input that is invisible due to suppression by
			// the Input command.  One reason for not monitoring ignored input is to avoid any chance of
			// an infinite loop of keystrokes caused by one hotstring triggering itself directly or
			// indirectly via a different hotstring:
			if (hm.enabledCount != 0 && !isIgnored)
			{
				switch (vk)
				{
					case VK_LEFT:
					case VK_RIGHT:
					case VK_DOWN:
					case VK_UP:
					case VK_NEXT:
					case VK_PRIOR:
					case VK_HOME:
					case VK_END:

						// Reset hotstring detection if the user seems to be navigating within an editor.  This is done
						// so that hotstrings do not fire in unexpected places.
						if (hsBuf.Count > 0)
							hm.ClearBuf();

						break;

					case VK_BACK:

						// v1.0.21: Only true (unmodified) backspaces are recognized by the below.  Another reason to do
						// this is that ^backspace has a native function (delete word) different than backspace in many editors.
						// Fix for v1.0.38: Below now uses kbdMsSender.modifiersLR_logical vs. physical because it's the logical state
						// that determines whether the backspace behaves like an unmodified backspace.  This solves the issue
						// of the Input command collecting simulated backspaces as real characters rather than recognizing
						// them as a means to erase the previous character in the buffer.
						if (kbdMsSender.modifiersLRLogical == 0 && hsBuf.Count > 0)
							hsBuf.RemoveAt(hsBuf.Count - 1);

						// Fall through to the check below in case this {BS} completed a dead key sequence.
						break;
				}

				if (charCount > 0
						&& !CollectHotstring(ref ev, ch, charCount, activeWindow, keyHistoryCurr,
											 ref hsOut, ref caseConformMode, ref endChar))
				{
					var ignored = new StringBuilder(8);

					if (state.used_dead_key_non_destructively)
					{
						// There's still a dead key in the keyboard layout's internal buffer, and it's supposed to apply to
						// this keystroke which we're suppressing.  Flush it out, otherwise a hotstring like the following
						// would insert an extra accent character:
						//   :*:jsá::jsmith@somedomain.com
						_ = ignored.Clear();
						ignored.Capacity = 8;

						while (ToUnicodeOrAsciiEx(VK_DECIMAL, 0, physicalKeyState, ignored, 1, activeWindowKeybdLayout) == -1) ;
					}

					return false; // Suppress.
				}
			}

			return true; // Visible.
		}

		internal bool CollectInputHook(ref KBDLLHOOKSTRUCT ev, uint vk, uint sc, char[] ch, int charCount, bool early)
		{
			var input = script.input;

			for (; input != null; input = input.prev)
			{
				if (!(input.BeforeHotkeys == early && input.IsInteresting(ev.dwExtraInfo) && input.InProgress()))
					continue;

				var keyFlags = input.keyVK[vk] | input.keySC[sc];
				// aCharCount is negative for dead keys, which are treated as text but not collected.
				var treatAsText = charCount != 0 && (keyFlags & INPUT_KEY_IGNORE_TEXT) == 0;
				var collectChars = treatAsText && charCount > 0;
				// Determine visibility based on options and whether the key produced text.
				// Negative aCharCount (dead key) is treated as text in this context.
				bool visible;

				if ((keyFlags & INPUT_KEY_VISIBILITY_MASK) != 0)
					visible = (keyFlags & INPUT_KEY_VISIBLE) != 0;
				else if (kvk[vk].asModifiersLR != 0 || kvk[vk].forceToggle != null)
					visible = true; // Do not suppress modifiers or toggleable keys unless specified by KeyOpt().
				else
					visible = treatAsText ? input.visibleText : input.visibleNonText;

				if ((keyFlags & END_KEY_ENABLED) != 0) // A terminating keystroke has now occurred unless the shift state isn't right.
				{
					var end_if_shift_is_down = (keyFlags & END_KEY_WITH_SHIFT) != 0;
					var end_if_shift_is_not_down = (keyFlags & END_KEY_WITHOUT_SHIFT) != 0;
					var shift_is_down = (kbdMsSender.modifiersLRLogical & (MOD_LSHIFT | MOD_RSHIFT)) != 0;

					if (shift_is_down ? end_if_shift_is_down : end_if_shift_is_not_down)
					{
						// The shift state is correct to produce the desired end-key.
						input.EndByKey(vk, sc, input.keySC[sc] != 0 && (sc != 0 || input.keyVK[vk] == 0), shift_is_down && !end_if_shift_is_not_down);

						if (!visible)
							break;

						continue;
					}
				}

				// Collect before backspacing, so if VK_BACK was preceded by a dead key, we delete it instead of the
				// previous char.  For example, {vkDE}{BS} on the US-Intl layout produces '\b (but we discarded \b).
				if (collectChars)
					input.CollectChar(new string(ch), charCount);

				// Fix for v2.0: Shift is allowed as it generally has no effect on the native function of Backspace.
				// This is probably connected with the fact that Shift+BS is also transcribed to `b, which we don't want.
				if (vk == VK_BACK && input.backspaceIsUndo
						&& (kbdMsSender.modifiersLRLogical & ~(MOD_LSHIFT | MOD_RSHIFT)) == 0)
				{
					if (input.buffer.Length != 0)
						input.buffer = input.buffer.Substring(0, input.buffer.Length - 1);

					if ((keyFlags & INPUT_KEY_VISIBILITY_MASK) == 0)// If +S and +V haven't been applied to Backspace...
						visible = input.visibleText; // Override VisibleNonText.

					// Fall through to the check below in case this {BS} completed a dead key sequence.
				}

				if (input.notifyNonText)
				{
					// These flags enable key-up events to be classified as text or non-text based on
					// whether key-down produced text.
					if (treatAsText)
						input.keyVK[vk] |= INPUT_KEY_IS_TEXT;
					else
						input.keyVK[vk] &= ~INPUT_KEY_IS_TEXT; // In case keyboard layout has changed or similar.
				}

				// Posting the notifications after CollectChar() might reduce the odds of a race condition.
				if (((keyFlags & INPUT_KEY_NOTIFY) != 0 || (input.notifyNonText && !treatAsText))
						&& input.scriptObject != null && input.scriptObject.OnKeyDown != null)
				{
					// input is passed because the alternative would require the main thread to
					// iterate through the Input chain and determine which ones should be notified.
					// This would mean duplicating much of the logic that's used here, and would be
					// complicated by the possibility of an Input being terminated while OnKeyDown
					// is being executed (and thereby breaking the list).
					// This leaves room only for the bare essential parameters: aVK and aSC.
					_ = channel.Writer.TryWrite(new KeysharpMsg()
					{
						message = (uint)UserMessages.AHK_INPUT_KEYDOWN,
						obj = input,
						lParam = new IntPtr(vk),
						wParam = new IntPtr(sc)
					});
					//PostMessage(Keysharp.Scripting.Script.MainWindowHandle, (uint)UserMessages.AHK_INPUT_KEYDOWN, input, (uint)((sc << 16) | vk));
				}

				// Seems best to not collect dead key chars by default; if needed, OnDeadChar
				// could be added, or the script could mark each dead key for OnKeyDown.
				if (collectChars && input.scriptObject != null && input.scriptObject.OnChar != null)
				{
					_ = channel.Writer.TryWrite(new KeysharpMsg()
					{
						message = (uint)UserMessages.AHK_INPUT_CHAR,
						obj = input,
						lParam = new IntPtr(ch[0]),
						wParam = ch.Length > 1 ? new IntPtr(ch[1]) : IntPtr.Zero
					});
					//PostMessage(Keysharp.Scripting.Script.MainWindowHandle, (uint)UserMessages.AHK_INPUT_CHAR, input, (uint)((ch[1] << 16) | ch[0]));
				}

				if (!visible)
				{
					if (charCount < 0 && treatAsText && input.InProgress())
					{
						// This dead key is being treated as text but will be suppressed, so to get the correct
						// result, we will need to replay the dead key sequence when the next key is collected.
						pendingDeadKeyInvisible = true;
					}

					break;
				}
			}

			if (input != null) // Early break (invisible input).
			{
				if (sc != 0)
					input.keySC[sc] |= INPUT_KEY_DOWN_SUPPRESSED;
				else
					input.keyVK[vk] |= INPUT_KEY_DOWN_SUPPRESSED;

				return false;
			}

			return true;
		}

		internal bool CollectKeyUp(ref KBDLLHOOKSTRUCT ev, uint vk, uint sc, bool early)
		// Caller is responsible for having initialized aHotstringWparamToPost to HOTSTRING_INDEX_INVALID.
		// Returns true if the caller should treat the key as visible (non-suppressed).
		// Always use the parameter vk rather than event.vkCode because the caller or caller's caller
		// might have adjusted vk, namely to make it a left/right specific modifier key rather than a
		// neutral one.
		{
			for (var input = script.input; input != null; input = input.prev)
			{
				if (input.BeforeHotkeys == early && input.IsInteresting(ev.dwExtraInfo) && input.InProgress())
				{
					if (input.scriptObject != null && input.scriptObject.OnKeyUp != null
							&& (((input.keySC[sc] | input.keyVK[vk]) & INPUT_KEY_NOTIFY) != 0
								|| (input.notifyNonText && (input.keyVK[vk] & INPUT_KEY_IS_TEXT) == 0)))
					{
						_ = channel.Writer.TryWrite(new KeysharpMsg()
						{
							message = (uint)UserMessages.AHK_INPUT_KEYUP,
							obj = input,
							lParam = new IntPtr(vk),
							wParam = new IntPtr(sc)
						});
					}

					if ((input.keySC[sc] & INPUT_KEY_DOWN_SUPPRESSED) != 0)
					{
						input.keySC[sc] &= ~INPUT_KEY_DOWN_SUPPRESSED;
						return false;
					}

					if ((input.keyVK[vk] & INPUT_KEY_DOWN_SUPPRESSED) != 0)
					{
						input.keyVK[vk] &= ~INPUT_KEY_DOWN_SUPPRESSED;
						return false;
					}
				}
			}

			return true;
		}

		internal override uint ConvertMouseButton(ReadOnlySpan<char> buf, bool allowWheel = true)
		{
			if (buf.Length == 0 || buf.StartsWith("Left", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("L", StringComparison.OrdinalIgnoreCase))
				return VK_LBUTTON; // Some callers rely on this default when buf is empty.

			if (buf.StartsWith("Right", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("R", StringComparison.OrdinalIgnoreCase)) return VK_RBUTTON;

			if (buf.StartsWith("Middle", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("M", StringComparison.OrdinalIgnoreCase)) return VK_MBUTTON;

			if (buf.StartsWith("X1", StringComparison.OrdinalIgnoreCase)) return VK_XBUTTON1;

			if (buf.StartsWith("X2", StringComparison.OrdinalIgnoreCase)) return VK_XBUTTON2;

			if (allowWheel)
			{
				if (buf.StartsWith("WheelUp", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("WU", StringComparison.OrdinalIgnoreCase)) return VK_WHEEL_UP;

				if (buf.StartsWith("WheelDown", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("WD", StringComparison.OrdinalIgnoreCase)) return VK_WHEEL_DOWN;

				// Lexikos: Support horizontal scrolling in Windows Vista and later.
				if (buf.StartsWith("WheelLeft", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("WL", StringComparison.OrdinalIgnoreCase)) return VK_WHEEL_LEFT;

				if (buf.StartsWith("WheelRight", StringComparison.OrdinalIgnoreCase) || buf.StartsWith("WR", StringComparison.OrdinalIgnoreCase)) return VK_WHEEL_RIGHT;
			}

			return 0;
		}

		internal bool EarlyCollectInput(ref KBDLLHOOKSTRUCT ev, uint vk, uint sc, bool keyUp, bool isIgnored
										, CollectInputState state, KeyHistoryItem keyHistoryCurr)
		// Returns true if the caller should treat the key as visible (non-suppressed).
		// Always use the parameter aVK rather than event.vkCode because the caller or caller's caller
		// might have adjusted aVK, such as to make it a left/right specific modifier key rather than a
		// neutral one. On the other hand, event.scanCode is the one we need for ToUnicodeEx() calls.
		{
			state.earlyCollected = true;
			state.used_dead_key_non_destructively = false;
			state.charCount = 0;

			if (keyUp && !CollectKeyUp(ref ev, vk, sc, true))
				return false;

			// The checks above suppress key-up if key-down was suppressed and the Input is still active.
			// Otherwise, avoid suppressing key-up since it may result in the key getting stuck down.
			// At the very least, this is needed for cases where a user presses a #z hotkey, for example,
			// to initiate an Input.  When the user releases the LWIN/RWIN key during the input, that
			// up-event should not be suppressed otherwise the modifier key would get "stuck down".
			if (keyUp)
				return true;

			var transcribeKey = true;

			// Don't unconditionally transcribe modified keys such as Ctrl-C because calling ToAsciiEx() on
			// some such keys (e.g. Ctrl-LeftArrow or RightArrow if I recall correctly), disrupts the native
			// function of those keys.  That is the reason for the existence of the
			// g_input.TranscribeModifiedKeys option.
			// Fix for v1.0.38: Below now uses kbdMsSender.modifiersLR_logical vs. g_modifiersLR_physical because
			// it's the logical state that determines what will actually be produced on the screen and
			// by ToAsciiEx() below.  This fixes the Input command to properly capture simulated
			// keystrokes even when they were sent via hotkey such #c or a hotstring for which the user
			// might still be holding down a modifier, such as :*:<t>::Test (if '>' requires shift key).
			// It might also fix other issues.
			if ((kbdMsSender.modifiersLRLogical & ~(MOD_LSHIFT | MOD_RSHIFT)) != 0 // At least one non-Shift modifier is down (Shift may also be down).
					&& !((kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT)) != 0 && (kbdMsSender.modifiersLRLogical & (MOD_LCONTROL | MOD_RCONTROL)) != 0))
			{
				// Since in some keybd layouts, AltGr (Ctrl+Alt) will produce valid characters (such as the @ symbol,
				// which is Ctrl+Alt+Q in the German/IBM layout and Ctrl+Alt+2 in the Spanish layout), an attempt
				// will now be made to transcribe all of the following modifier combinations:
				// - Anything with no modifiers at all.
				// - Anything that uses ONLY the shift key.
				// - Anything with Ctrl+Alt together in it, including Ctrl+Alt+Shift, etc. -- but don't do
				//   "anything containing the Alt key" because that causes weird side-effects with
				//   Alt+LeftArrow/RightArrow and maybe other keys too).
				// Older comment: If any modifiers except SHIFT are physically down, don't transcribe the key since
				// most users wouldn't want that.  An additional benefit of this policy is that registered hotkeys will
				// normally be excluded from the input (except those rare ones that have only SHIFT as a modifier).
				// Note that ToAsciiEx() will translate ^i to a tab character, !i to plain i, and many other modified
				// letters as just the plain letter key, which we don't want.
				for (var input = script.input; ; input = input.prev)
				{
					if (input == null) // No inputs left, and none were found that meet the conditions below.
					{
						transcribeKey = false;
						break;
					}

					// Transcription is done only once for all layers, so do this if any layer requests it:
					if (input.transcribeModifiedKeys && input.InProgress() && input.IsInteresting(ev.dwExtraInfo))
						break;
				}
			}

			// v1.1.28.00: active_window is set to the focused control, if any, so that the hotstring buffer is reset
			// when the focus changes between controls, not just between windows.
			// v1.1.28.01: active_window is left as the active window; the above is not done because it disrupts
			// hotstrings when the first keypress causes a change in focus, such as to enter editing mode in Excel.
			// See Get_active_window_keybd_layout macro definition for related comments.
			var activeWindow = GetForegroundWindow(); // Set default in case there's no focused control.
			var tempzero = IntPtr.Zero;
			var activeWindowKeybdLayout = script.PlatformProvider.Manager.GetKeyboardLayout(script.WindowProvider.Manager.GetFocusedCtrlThread(ref tempzero, activeWindow));
			state.activeWindow = activeWindow;
			state.keyboardLayout = activeWindowKeybdLayout;

			// SUMMARY OF DEAD KEY ISSUE:
			// Calling ToUnicodeEx() with conventional parameters disrupts the entry of dead keys in two different ways:
			//  1) Passing a dead key buffers it within the keyboard layout's internal state.
			//  2) Passing a live key removes any pending dead key from the keyboard layout's internal state.
			// In either case, the state is then incorrect for the active window's own call to ToUnicodeEx(), so it ends
			// up with something like "e" or "''e" instead of "é".  Originally this was solved by reinserting the pending
			// dead key (first by re-sending the dead key, then later by calling ToUnicodeEx()), but now we use a special
			// combination of parameters to avoid changing the state where possible.

			// Univeral Windows Platform apps apparently have their own handling for dead keys:
			//  - On some OS versions, dead key followed by Esc produces Chr(27), unlike non-UWP apps.
			//  - Pressing a dead key in a UWP app does not leave it in the keyboard layout's buffer,
			//    so to get the correct result here we must translate the dead key again, first.
			//  - Pressing a non-dead key disregards any dead key which was placed into the buffer by
			//    calling ToUnicodeEx, and it is left in the buffer.  To get the correct result for the
			//    next call, the dead key must NOT be left in the buffer.
			//  - Chained dead keys reportedly do not work even without AutoHotkey interfering, but in
			//    case that's fixed (and for simplicity), our translation assumes that it will work.
			// Note that this still applies to some apps on Windows 11 22H2 (such as Feedback Hub) but
			// does not apply to newer apps based on WinUI, such as the Photos app.
			if (uwpHwndChecked != activeWindow)
			{
				uwpHwndChecked = activeWindow;
				var className = new StringBuilder(32);
				_ = GetClassName(activeWindow, className, 32);
				uwpAppFocused = string.Compare(className.ToString(), "ApplicationFrameWindow", true) == 0;
			}

			int charCount;
			var ch = new char[3];
			var sb = new StringBuilder(8);

			if (vk == VK_PACKET)
			{
				// VK_PACKET corresponds to a SendInput event with the KEYEVENTF_UNICODE flag.
				charCount = 1;// SendInput only supports a single 16-bit character code.
				ch[0] = (char)ev.scanCode; // No translation needed.
			}
			else if (transcribeKey && vk != VK_MENU)
			{
				var keyState = new byte[physicalKeyState.Length];
				bool interfere = pendingDeadKeys.Count > 0 && (pendingDeadKeyInvisible || uwpAppFocused);

				if (interfere)
				{
					// Either an invisible InputHook is in progress or there is a UWP app focused.  In either case, the dead key
					// was only recorded by us and wasn't retained by the keyboard layout's internal state, so we need to "replay"
					// the sequence to set things up for conversion of the new key.
					for (int i = 0; i < pendingDeadKeys.Count; ++i)
					{
						var dead_key = pendingDeadKeys[i];
						AdjustKeyState(keyState, dead_key.modLR);
						keyState[VK_CAPITAL] = (byte)dead_key.caps;
						_ = sb.Clear();
						sb.Capacity = 8;
						_ = ToUnicodeOrAsciiEx(dead_key.vk, dead_key.sc, keyState, sb, 0, activeWindowKeybdLayout);
						ch = sb.ToString().ToCharArray();
					}
				}

				// The documentation for ToAsciiEx is incomplete, but recent documentation for ToUnicodeEx shows the meaning of
				// the flags: 0x1 = Alt+Numpad key combinations are not handled (but the flag doesn't prevent Alt-up itself from
				// being processed), 0x2 = handle key break events (key-up).  We fake key-up to avoid changing the dead key state
				// (it stands to reason that key-down normally causes a change in state, so the corresponding key-up wouldn't).
				// We must avoid passing VK_MENU with KBDBREAK (0x8000) because that disrupts any ongoing Alt+Numpad entry.
				// Note that Windows 10 v1607 supports flag 0x4 to avoid changing the keyboard state, but there seems to be no
				// benefit; in particular, the Alt+Numpad state is still affected.
				// Credit to Ilya Zakharevich for pointing out this method @ https://stackoverflow.com/a/78173420/894589
				var flags = interfere ? 1u : 3u;
				var scanCode = ev.scanCode | (interfere ? 0u : 0x8000u);
				// Provide the correct logical modifier and CapsLock state for any translation below.
				AdjustKeyState(keyState, kbdMsSender.modifiersLRLogical);
				keyState[VK_CAPITAL] = (byte)(IsKeyToggledOn(VK_CAPITAL) ? 1 : 0);
				_ = sb.Clear();
				sb.Capacity = 8;
				charCount = ToUnicodeOrAsciiEx(vk, scanCode, keyState, sb, flags, activeWindowKeybdLayout);

				if (charCount == 0 && (kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT)) != 0 && (kbdMsSender.modifiersLRLogical & (MOD_LCONTROL | MOD_RCONTROL)) == 0u && !interfere)
				{
					// Apparently, ToUnicodeEx ignores the Alt in Alt and Alt+Shift combinations only if the key-up bit is not set.
					// For consistency with prior versions (and Win, but not Ctrl/Shift), let the Alt state be ignored under these
					// conditions.  transcribe_key and modifier state checked above imply that the M option was used.
					keyState[VK_MENU] = 0;
					_ = sb.Clear();
					sb.Capacity = 8;
					charCount = ToUnicodeOrAsciiEx(vk, scanCode, keyState, sb, flags, activeWindowKeybdLayout);
				}

				if (charCount <= 0 && interfere) // A key with no text translation, or possibly a chained dead key (if < 0).
				{
					// Flush the dead key which was buffered either by the ToUnicodeEx call above or the dead key loop further up.
					var ignored = new StringBuilder(8);

					// Michael S. Kaplan blogged that he would explain in a later post why he used VK_SPACE to clear the buffer,
					// but then changed to using VK_DECIMAL and apparently never explained either choice.  Still, VK_DECIMAL
					// seems like a safe choice for clearing the state; probably any key which produces text will work, but
					// the loop is needed in case of an unconventional layout which makes VK_DECIMAL itself a dead key.
					while (ToUnicodeOrAsciiEx(VK_DECIMAL, 0, keyState, ignored, flags, activeWindowKeybdLayout) == -1) ;
				}

				if (charCount > 0)
				{
					state.used_dead_key_non_destructively = pendingDeadKeys.Count > 0 && !interfere;
					pendingDeadKeys.Clear();
					pendingDeadKeyInvisible = false;
				}
				else if (charCount < 0 && pendingDeadKeys.Count < 3)
				{
					// Record this dead key so that we can reproduce the sequence when needed.
					var deadKey = new DeadKeyRecord()
					{
						vk = vk,
						sc = ev.scanCode,
						modLR = kbdMsSender.modifiersLRLogical,
						caps = keyState[VK_CAPITAL]
					};
					pendingDeadKeys.Add(deadKey);
				}

				ch = sb.ToString().ToCharArray();

				if ((kbdMsSender.modifiersLRLogical & (MOD_LCONTROL | MOD_RCONTROL)) == 0) // i.e. must not replace '\r' with '\n' if it is the result of Ctrl+M.
				{
					if (ch.Length > 0)
						if (ch[0] == '\r')  // Translate \r to \n since \n is more typical and useful in Windows.
							ch[0] = '\n';

					if (ch.Length > 1)
						if (ch[1] == '\r')  // But it's never referred to if byte_count < 2
							ch[1] = '\n';
				}
			}
			else
			{
				charCount = 0;
			}

			// If Backspace is pressed after a dead key, ch[0] is the "dead" char and ch[1] is '\b'.
			// Testing shows that this can be handled a number of ways (we only support 1 & 2):
			// 1. Insert ch[0] and then apply backspacing.  This is subtly different from doing nothing
			//    in that if there is a selection, it is deleted.  This appears to be how Edit controls
			//    behave on Windows 11 22H2, 10 22H2 and 7 (in a VM).
			// 2. UWP apps perform backspacing and discard the pending dead key.
			//    (VS2022 does as well, but we don't do anything to support that.)
			// 3. VS2015 performs backspacing and leaves the dead key in the buffer.
			// 4. MarkdownPad 2 prints the dead char as if Space was pressed, and does no backspacing.
			// 5. In 2019, Lexikos noted that Win32 apps performed backspacing and THEN inserted ch[0].
			//    This might have only applied to Windows 10 builds around that time.
			if (vk == VK_BACK && charCount > 0)
			{
				if (uwpAppFocused)
				{
					charCount = 0;
					pendingDeadKeys.Clear();
				}
				else // Assume standard Win32 behavior as described above.
					charCount--;// Remove '\b' to simplify the backspacing and collection stages.
			}

			state.ch = ch;
			state.charCount = charCount;

			if (!CollectInputHook(ref ev, vk, sc, ch, charCount, true))
				return false; // Suppress.

			return true;//Visible.
		}

		internal override object Invoke(Func<object> f) => thread?.Invoke(() => f());

		internal override bool IsHookThreadRunning() => thread != null && !thread.IsDisposed();

		internal override bool IsHotstringWordChar(char ch)
		// Returns true if aChar would be part of a word if followed by a word char.
		// aChar itself may be a word char or a nonspacing mark which combines with
		// the next character (the first character of a potential hotstring match).
		{
			// IsCharAlphaNumeric is used for simplicity and to preserve old behavior
			// (with the only exception being the one added below), in case it's what
			// users have come to expect.  Note that checking for C1_ALPHA or C3_ALPHA
			// and C1_DIGIT is not equivalent: Michael S. Kaplan wrote that the real
			// conditions are "(C1_ALPHA && ! (C3_HIRAGANA | C3_KATAKANA) || C1_DIGIT)" -- https://web.archive.org/web/20130627015450/http://blogs.msdn.com/b/michkap/archive/2007/06/19/3396819.aspx
			if (IsCharAlphaNumeric(ch))
				return true;

			var char_type = new ushort[1];

			if (GetStringTypeEx(0, CT_CTYPE3, ch.ToString(), 1, char_type))//Ignore locale for unicode by passing 0.
			{
				// Nonspacing marks combine with the following character, so would visually
				// appear to be part of the word.  This should fix detection of words beginning
				// with or containing Arabic nonspacing diacritics, for example.
				if ((char_type[0] & C3_NONSPACING) != 0)
					return true;
			}

			return false;
		}

		internal override bool IsKeyDown(uint vk) => (GetKeyState((int)vk) & 0x8000) != 0;

		internal override bool IsKeyDownAsync(uint vk) => (GetAsyncKeyState((int)vk) & 0x8000) != 0;

		internal override bool IsKeyToggledOn(uint vk) => (GetKeyState((int)vk) & 0x01) != 0;

		internal override bool IsMouseVK(uint vk)
		{
			return vk >= VK_LBUTTON && vk <= VK_XBUTTON2 && vk != VK_CANCEL
				   || vk >= VK_NEW_MOUSE_FIRST && vk <= VK_NEW_MOUSE_LAST;
		}

		internal override bool IsWheelVK(uint vk) => vk >= VK_WHEEL_LEFT&& vk <= VK_WHEEL_UP;

		/// <summary>
		/// Always use the parameter vk rather than event.vkCode because the caller or caller's caller
		/// might have adjusted vk, namely to make it a left/right specific modifier key rather than a
		/// neutral one.
		/// Will need to figure out how to manage fake shift keyup when using shift and numpad (it makes shift not normally apply).
		/// </summary>
		/// <param name="eventFlags"></param>
		/// <param name="vk"></param>
		/// <param name="keyUp"></param>
		/// <returns></returns>
		internal bool KeybdEventIsPhysical(uint eventFlags, uint vk, bool keyUp)
		{
			// MSDN: "The keyboard input can come from the local keyboard driver or from calls to the keybd_event
			// function. If the input comes from a call to keybd_event, the input was "injected"".
			// My: This also applies to mouse events, so use it for them too:
			if ((eventFlags & LLKHF_INJECTED) != 0)
				return false;

			// So now we know it's a physical event.  But certain SHIFT key-down events are driver-generated.
			// We want to be able to tell the difference because the Send command and other aspects
			// of keyboard functionality need us to be accurate about which keys the user is physically
			// holding down at any given time:
			if ((vk == VK_LSHIFT || vk == VK_RSHIFT) && !keyUp)
			{
				// If the corresponding mask bit is set, the key was temporarily "released" by the system
				// as part of translating a shift-numpad combination to its unshifted counterpart, and this
				// event is the fake key-down which follows the release of the numpad key.  The system uses
				// standard scancodes for this specific case, not SC_FAKE_LSHIFT or SC_FAKE_RSHIFT.
				if ((kbdMsSender.modifiersLRNumpadMask & (vk == VK_LSHIFT ? MOD_LSHIFT : MOD_RSHIFT)) != 0)
					return false;
			}

			// Otherwise, it's physical.
			// v1.0.42.04:
			// The time member of the incoming event struct has been observed to be wrongly zero sometimes, perhaps only
			// for AltGr keyboard layouts that generate LControl events when RAlt is pressed (but in such cases, I think
			// it's only sometimes zero, not always).  It might also occur during simulation of Alt+Numpad keystrokes
			// to support {Asc NNNN}.  In addition, SendInput() is documented to have the ability to set its own timestamps;
			// if it's callers put in a bad timestamp, it will probably arrive here that way too.  Thus, use GetTickCount().
			// More importantly, when a script or other application simulates an AltGr keystroke (either down or up),
			// the LControl event received here is marked as physical by the OS or keyboard driver.  This is undesirable
			// primarily because it makes g_TimeLastInputPhysical inaccurate, but also because falsely marked physical
			// events can impact the script's calls to GetKeyState("LControl", "P"), etc.
			script.timeLastInputPhysical = script.timeLastInputKeyboard = DateTime.UtcNow;
			return true;
		}

		/// <summary>
		/// v1.0.38.06: The keyboard and mouse hooks now call this common function to reduce code size and improve
		/// maintainability.  The code size savings as of v1.0.38.06 is 3.5 KB of uncompressed code, but that
		/// savings will grow larger if more complexity is ever added to the hooks.
		/// </summary>
		internal IntPtr LowLevelCommon(IntPtr hook, int code, long wParam, ref KBDLLHOOKSTRUCT kbd, ref MSDLLHOOKSTRUCT mouse, uint vk, uint sc, bool keyUp, ulong extraInfo, uint eventFlags)
		{
			var hotkeyIdToPost = HotkeyDefinition.HOTKEY_ID_INVALID; // Set default.
			var isIgnored = IsIgnored(extraInfo);
			var collectInputState = new CollectInputState()
			{
				earlyCollected = false
			};
			// The following is done for more than just convenience.  It solves problems that would otherwise arise
			// due to the value of a global var such as KeyHistoryNext changing due to the reentrancy of
			// this procedure.  For example, a call to KeyEvent() in here would alter the value of
			// KeyHistoryNext, in most cases before we had a chance to finish using the old value.  In other
			// words, we use an automatic variable so that every instance of this function will get its
			// own copy of the variable whose value will stay constant until that instance returns:
			KeyHistoryItem keyHistoryCurr; // Must not be static (see above).  Serves as a storage spot for a single keystroke in case key history is disabled.

			if (keyHistory == null)
			{
				keyHistoryCurr = new KeyHistoryItem();  // Having a non-NULL pKeyHistoryCurr simplifies the code in other places.
			}
			else
			{
				var kh = keyHistory;//In case it's being updated on the channel thread.
				keyHistoryCurr = kh.NextItem();
				keyHistoryCurr.vk = vk; // aSC is done later below.
				keyHistoryCurr.keyUp = keyUp;
				var fore_win = GetForegroundWindow();

				if (fore_win != IntPtr.Zero)
				{
					if (fore_win != kh.HistoryHwndPrev)
					{
						// The following line is commented out in favor of the one beneath it (seem below comment):
						//GetWindowText(fore_win, pKeyHistoryCurr.target_window, sizeof(pKeyHistoryCurr.target_window));
						//PostMessage(Keysharp.Scripting.Script.MainWindowHandle, (uint)UserMessages.AHK_GETWINDOWTEXT, keyHistoryCurr.targetWindow, fore_win);
						var wnd = Control.FromHandle(fore_win) is Control ctrl ? ctrl : script.mainWindow;

						if (wnd != null)
						{
							wnd.CheckedBeginInvoke(() =>
							{
								keyHistoryCurr.targetWindow = wnd.Text;
							}, false, false);
						}

						// v1.0.44.12: The reason for the above is that clicking a window's close or minimize button
						// (and possibly other types of title bar clicks) causes a delay for the following window, at least
						// when XP Theme (but not classic theme) is in effect:
						//#InstallMouseHook
						//Gui, +AlwaysOnTop
						//Gui, Show, w200 h100
						//return
						// The problem came about from the following sequence of events:
						// 1) User clicks the one of the script's window's title bar's close, minimize, or maximize button.
						// 2) WM_NCLBUTTONDOWN is sent to the window's window proc, which then passes it on to
						//    DefWindowProc or DefDlgProc, which then apparently enters a loop in which no messages
						//    (or a very limited subset) are pumped.
						// 3) If anyone sends a message to that window (such as GetWindowText(), which sends a message
						//    in cases where it doesn't have the title pre-cached), the message will not receive a reply
						//    until after the mouse button is released.
						// 4) But the hook is the very thing that's supposed to release the mouse button, and it can't
						//    until a reply is received.
						// 5) Thus, a deadlock occurs.  So after a short but noticeable delay, the OS sees the hook as
						//    unresponsive and bypasses it, sending the click through normally, which breaks the deadlock.
						// 6) A similar situation might arise when a right-click-down is sent to the title bar or
						//    sys-menu-icon.
						//
						// SOLUTION:
						// Post the message to our main thread to have it do the GetWindowText call.  That way, if
						// the target window is one of the main thread's own window's, there's no chance it can be
						// in an unresponsive state like the deadlock described above.  In addition, do this for ALL
						// windows because its simpler, more maintainable, and especially might solve other hook
						// performance problems if GetWindowText() has other situations where it is slow to return
						// (which seems likely).
						// Although the above solution could create rare situations where there's a lag before window text
						// is updated, that seems unlikely to be common or have significant consequences.  Furthermore,
						// it has the advantage of improving hook performance by avoiding the call to GetWindowText (which
						// incidentally might solve hotkey lag problems that have been observed while the active window
						// is momentarily busy/unresponsive -- but maybe not because the main thread would then be lagged
						// instead of the hook thread, which is effectively the same result from user's POV).
						// Note: It seems best not to post the message to the hook thread because if LButton is down,
						// the hook's main event loop would be sending a message to an unresponsive thread (our main thread),
						// which would create the same deadlock.
						// ALTERNATE SOLUTIONS:
						// - #1: Avoid calling GetWindowText at all when LButton or RButton is in a logically-down state.
						// - Same as #1, but do so only if one of the main thread's target windows is known to be in a tight loop (might be too unreliable to detect all such cases).
						// - Same as #1 but less rigorous and more catch-all, such as by checking if the active window belongs to our thread.
						// - Avoid calling GetWindowText at all upon release of LButton.
						// - Same, but only if the window to have text retrieved belongs to our process.
						// - Same, but only if the mouse is inside the close/minimize/etc. buttons of the active window.
					}
					else // i.e. where possible, avoid the overhead of the call to GetWindowText().
						keyHistoryCurr.targetWindow = "";
				}
				else
					keyHistoryCurr.targetWindow = "N/A";// Due to AHK_GETWINDOWTEXT, this could collide with main thread's writing to same string; but in addition to being extremely rare, it would likely be inconsequential.

				kh.HistoryHwndPrev = fore_win;  // Updated unconditionally in case fore_win is NULL.
			}

			// Keep the following flush with the above to indicate that they're related.
			// The following is done even if key history is disabled because firing a wheel hotkey via PostMessage gets
			// the notch count from pKeyHistoryCurr.sc.
			if (vk == (uint)Keys.Packet) // Win2k/XP: VK_PACKET is used to send Unicode characters as if they were keystrokes.  sc is a 16-bit character code in that case.
			{
				sc = 0; // This held a truncated character code, not to be mistaken for a real scan code.
				keyHistoryCurr.sc = kbd.scanCode; // Get the full character code.
				keyHistoryCurr.eventType = 'U'; // Give it a unique identifier even though it can be distinguished by the 4-digit "SC".  'U' vs 'u' to avoid confusion with 'u'=up.
				// Artificial character input via VK_PACKET isn't supported by hotkeys, since they always work via
				// keycode, but hotstrings and Input are supported via the macro below when #InputLevel is non-zero.
				// Must return now to avoid misinterpreting aSC as an actual scancode in the code below.
				return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
			}

			//else: Use usual modified value.
			keyHistoryCurr.sc = sc; // Will be zero if our caller is the mouse hook (except for wheel notch count).

			// After logging the wheel notch count (above), purify aSC for readability and maintainability.
			if (IsWheelVK(vk))
				sc = 0; // Also relied upon by sc_takes_precedence below.

			bool isArtificial;

			if (hook == mouseHook)
			{
				if (!(isArtificial = (eventFlags & LLMHF_INJECTED) != 0)) // It's a physical mouse event.
					physicalKeyState[vk] = (byte)(keyUp ? 0 : StateDown);
			}
			else // Keybd hook.
			{
				// Even if the below is set to false, the event might be reclassified as artificial later (though it
				// won't be logged as such).  See comments in KeybdEventIsPhysical() for details.
				isArtificial = (eventFlags & LLKHF_INJECTED) != 0; // LLKHF vs. LLMHF

				// Track physical state of keyboard & mouse buttons. Also, if it's a modifier, let another section
				// handle it because it's not as simple as just setting the value to true or false (e.g. if LShift
				// goes up, the state of VK_SHIFT should stay down if VK_RSHIFT is down, or up otherwise).
				// Also, even if this input event will wind up being suppressed (usually because of being
				// a hotkey), still update the physical state anyway, because we want the physical state to
				// be entirely independent of the logical state (i.e. we want the key to be reported as
				// physically down even if it isn't logically down):
				if (kvk[vk].asModifiersLR == 0 && KeybdEventIsPhysical(eventFlags, vk, keyUp))
					physicalKeyState[vk] = (byte)(keyUp ? 0 : StateDown);
			}

			// The following is done even if key history is disabled because altTabMenuIsVisible relies on it:
			keyHistoryCurr.eventType = isIgnored ? 'i' : (isArtificial ? 'a' : ' '); // v1.0.42.04: 'a' was added, but 'i' takes precedence over 'a'.

			// v2.1: Process any InputHooks which have the H option.  This requires translating the event to text, which
			// is done only once for each event.  If there are no InputHooks with the H option, the translation is done
			// later to avoid any change in behaviour compared to v2.0 (such as dead keys affecting the translation prior
			// to being suppressed by a hotkey), or not done at all if the event is suppressed by other means.
			if (hook == kbdHook && script.inputBeforeHotkeysCount > 0
					&& !EarlyCollectInput(ref kbd, vk, sc, keyUp, isIgnored, collectInputState, keyHistoryCurr))
				return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));

			// v1.0.43: Block the Win keys during journal playback to prevent keystrokes hitting the Start Menu
			// if the user accidentally presses one of those keys during playback.  Note: Keys other than Win
			// don't need to be blocked because the playback hook defers them until after playback.
			// Only block the down-events in case the user is physically holding down the key at the start
			// of playback but releases it during the Send (avoids Win key becoming logically stuck down).
			// This also serves to block Win shortcuts such as Win+R and Win+E during playback.
			// Also, it seems best to block artificial LWIN keystrokes too, in case some other script or
			// program tries to display the Start Menu during playback.
			if (blockWinKeys && (vk == VK_LWIN || vk == VK_RWIN) && !keyUp)
				return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));

			// v1.0.37.07: Cancel the alt-tab menu upon receipt of Escape so that it behaves like the OS's native Alt-Tab.
			// Even if is_ignored==true, it seems more flexible/useful to cancel the Alt-Tab menu upon receiving
			// an Escape keystroke of any kind.
			// Update: Must not handle Alt-Up here in a way similar to Esc-down in case the hook sent Alt-up to
			// dismiss its own menu. Otherwise, the shift key might get stuck down if Shift-Alt-Tab was in effect.
			// Instead, the release-of-prefix-key section should handle it via its checks of this_key.it_put_shift_down, etc.
			if (altTabMenuIsVisible && vk == VK_ESCAPE && !keyUp)
			{
				// When the alt-tab window is owned by the script (it is owned by csrss.exe unless the script
				// is the process that invoked the alt-tab window), testing shows that the script must be the
				// originator of the Escape keystroke.  Therefore, substitute a simulated keystroke for the
				// user's physical keystroke. It might be necessary to do this even if is_ignored==true because
				// a keystroke from some other script/process might not qualify as a valid means to cancel it.
				// UPDATE for v1.0.39: The escape handler below works only if the hook's thread invoked the
				// alt-tab window, not if the script's thread did via something like "Send {Alt down}{tab down}".
				// This is true even if the process ID is checked instead of the thread ID below.  I think this
				// behavior is due to the window obeying escape only when its own thread sends it.  This
				// is probably done to allow a program to automate the alt-tab menu without interference
				// from Escape keystrokes typed by the user.  Although this could probably be fixed by
				// sending a message to the main thread and having it send the Escape keystroke, it seems
				// best not to do this because:
				// 1) The ability to dismiss a script-invoked alt-tab menu with escape would vary depending on
				//    whether the keyboard hook is installed (i.e. it's inconsistent).
				// 2) It's more flexible to preserve the ability to protect the alt-tab menu from physical
				//    escape keystrokes typed by the user.  The script can simulate an escape key to explicitly
				//    close an alt-tab window it invoked (a simulated escape keystroke can apparently also close
				//    any alt-tab menu, even one invoked by physical keystrokes; but the converse isn't true).
				// 3) Lesser reason: Reduces code size and complexity.
				// UPDATE in 2019: Testing on Windows 7 and 10 indicate this does not apply to the more modern
				// versions of Alt-Tab, but it still applies if the classic Alt-Tab is restored via the registry.
				// However, on these OSes, the user is able to press Esc to dismiss our Alt-Tab.  Other scripts
				// (and presumably other processes) are *NOT* able to dismiss it by simulating Esc.
				IntPtr altTabWindow;

				if ((altTabWindow = FindWindow("#32771", null)) != IntPtr.Zero // There is an alt-tab window...
						&& GetWindowThreadProcessId(altTabWindow, out _) == mgr.CurrentThreadId()) // ...and it's owned by the hook thread (not the main thread).
				{
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, VK_ESCAPE);
					// By definition, an Alt key should be logically down if the alt-tab menu is visible (even if it
					// isn't, sending an extra up-event seems harmless).  Releasing that Alt key seems best because:
					// 1) If the prefix key that pushed down the alt key is still physically held down and the user
					//    presses a new (non-alt-tab) suffix key to form a hotkey, it avoids any alt-key disruption
					//    of things such as MouseClick that that subroutine might due.
					// 2) If the user holds down the prefix, presses Escape to dismiss the menu, then presses an
					//    alt-tab suffix, testing shows that the existing alt-tab logic here in the hook will put
					//    alt or shift-alt back down if it needs to.
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, (kbdMsSender.modifiersLRLogical & MOD_RALT) != 0 ? VK_RMENU : VK_LMENU);
					return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));// Testing shows that by contrast, the upcoming key-up on Escape doesn't require this logic.
				}

				// Otherwise, the alt-tab window doesn't exist or (more likely) it's owned by some other process
				// such as crss.exe.  Do nothing extra to avoid interfering with the native function of Escape or
				// any remappings or hotkeys assigned to Escape.  Also, do not set altTabMenuIsVisible to false
				// in any of the cases here because there is logic elsewhere in the hook that does that more
				// reliably; it takes into account things such as whether the Escape keystroke will be suppressed
				// due to being a hotkey).
			}

			// Pointer to the key record for the current key event.  Establishes this_key as an alias
			// for the array element in kvk or ksc that corresponds to the vk or sc, respectively.
			// I think the compiler can optimize the performance of reference variables better than
			// pointers because the pointer indirection step is avoided.  In any case, this must be
			// a true alias to the object, not a copy of it, because it's address (&this_key) is compared
			// to other addresses for equality further below.
			var scTakesPrecedence = ksc[sc].scTakesPrecedence;
			// Check hook type too in case a script every explicitly specifies scan code zero as a hotkey:
			var thisKey = (hook == kbdHook && scTakesPrecedence) ? ksc[sc] : kvk[vk];
			var thisKeyIndex = (hook == kbdHook && scTakesPrecedence) ? sc : vk;

			// Do this after above since AllowKeyToGoToSystem requires that sc be properly determined.
			// Another reason to do it after the above is due to the fact that KEY_PHYS_IGNORE permits
			// an ignored key to be considered physical input, which is handled above:
			if (isIgnored)
			{
				// This is a key sent by our own app that we want to ignore.
				// It's important never to change this to call the SuppressKey function because
				// that function would cause an infinite loop when the Numlock key is pressed,
				// which would likely hang the entire system:
				// UPDATE: This next part is for cases where more than one script is using the hook
				// simultaneously.  In such cases, it desirable for the KEYEVENT_PHYS() of one
				// instance to affect the down-state of the current prefix-key in the other
				// instances.  This check is done here -- even though there may be a better way to
				// implement it -- to minimize the chance of side-effects that a more fundamental
				// change might cause (i.e. a more fundamental change would require a lot more
				// testing, though it might also fix more things):
				if (extraInfo == KeyPhysIgnore && keyUp && prefixKey == thisKey)
				{
					thisKey.isDown = false;
					thisKey.downPerformedAction = false;  // Seems best, but only for PHYS_IGNORE.
					prefixKey = null;
				}

				return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
			}

			if (!keyUp) // Set defaults for this down event.
			{
				thisKey.hotkeyDownWasSuppressed = false;
				// Don't do the following because key-repeat should not prevent a previously-selected
				// key-up hotkey from executing (although it can still be overridden by selecting a
				// different key-up hotkey below).  If this was done, a key-down hotkey which puts a
				// modifier into effect would not allow the corresponding key-up to execute unless the
				// key is released prior to key-repeat, or the key-up hotkey explicitly allows it
				// (which would defeat the purpose of hotkey_to_fire_upon_release).
				//thisKey.hotkeyToFireUponRelease = HotkeyDefinition.HOTKEY_ID_INVALID;
				// Don't do the following because of the keyboard key-repeat feature.  In other words,
				// the NO_SUPPRESS_NEXT_UP_EVENT should stay pending even in the face of consecutive
				// down-events.  Even if it's possible for the flag to never be cleared due to never
				// reaching any of the parts that clear it (which currently seems impossible), it seems
				// inconsequential since by its very nature, this_key never consults the flag.
				// this_key.no_suppress &= ~NO_SUPPRESS_NEXT_UP_EVENT;
			}

			if (hook == mouseHook)
			{
				// If no vk, there's no mapping for this key, so currently there's no way to process it.
				if (vk == 0)
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

				// Also, if the script is displaying a menu (tray, main, or custom popup menu), always
				// pass left-button events through -- even if LButton is defined as a hotkey -- so
				// that menu items can be properly selected.  This is necessary because if LButton is
				// a hotkey, it can't launch now anyway due to the script being uninterruptible while
				// a menu is visible.  And since it can't launch, it can't do its typical "MouseClick
				// left" to send a true mouse-click through as a replacement for the suppressed
				// button-down and button-up events caused by the hotkey.  Also, for simplicity this
				// is done regardless of which modifier keys the user is holding down since the desire
				// to fire mouse hotkeys while a context or popup menu is displayed seems too rare.
				//
				// Update for 1.0.37.05: The below has been extended to look for menus beyond those
				// supported by g_MenuIsVisible, namely the context menus of a MonthCal or Edit control
				// (even the script's main window's edit control's context menu).  It has also been
				// extended to include RButton because:
				// 1) Right and left buttons may have been swapped via control panel to take on each others' functions.
				// 2) Right-click is a valid way to select a context menu items (but apparently not popup or menu bar items).
				// 3) Right-click should invoke another instance of the context menu (or dismiss existing menu, depending
				//    on where the click occurs) if user clicks outside of our thread's existing context menu.
				var menuHwnd = IntPtr.Zero;

				if ((vk == VK_LBUTTON || vk == VK_RBUTTON)
						&& (script.menuIsVisible != MenuType.None // Ordered for short-circuit performance.
							|| ((menuHwnd = FindWindow("#32768", null)) != IntPtr.Zero
								&& GetWindowThreadProcessId(menuHwnd, out _) == script.ProcessesData.MainThreadID))) // Don't call GetCurrentThreadId() because our thread is different than main's.
				{
					// Bug-fix for v1.0.22: If "LControl & LButton::" (and perhaps similar combinations)
					// is a hotkey, the foreground window would think that the mouse is stuck down, at least
					// if the user clicked outside the menu to dismiss it.  Specifically, this comes about
					// as follows:
					// The wrong up-event is suppressed:
					// ...because down_performed_action was true when it should have been false
					// ...because the while-menu-was-displayed up-event never set it to false
					// ...because it returned too early here before it could get to that part further below.
					thisKey.downPerformedAction = false; // Seems ok in this case to do this for both aKeyUp and !aKeyUp.
					thisKey.isDown = !keyUp;
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
				}
			} // Mouse hook.

			// Any key-down event (other than those already ignored and returned from,
			// above) should probably be considered an attempt by the user to use the
			// prefix key that's currently being held down as a "modifier".  That way,
			// if pPrefixKey happens to also be a suffix, its suffix action won't fire
			// when the key is released, which is probably the correct thing to do 90%
			// or more of the time.  But don't consider the modifiers themselves to have
			// been modified by a prefix key, since that is almost never desirable:
			if (prefixKey != null && prefixKey != thisKey && !keyUp // There is a prefix key being held down and the user has now pressed some other key.
					&& prefixKey.wasJustUsed != KeyType.AS_PASSTHROUGH_PREFIX // v1.1.34.02: Retain this value for prefix key-up.
					&& ((hook == kbdHook ? (thisKey.asModifiersLR == 0 ? 1 : 0) : prefixKey.asModifiersLR) != 0))//Use ^ xor to toggle.
				prefixKey.wasJustUsed = KeyType.AS_PREFIX; // Indicate that currently-down prefix key has been "used".

			// Formerly, the above was done only for keyboard hook, not the mouse.  This was because
			// most people probably would not want a prefix key's suffix-action to be stopped
			// from firing just because a non-hotkey mouse button was pressed while the key
			// was held down (i.e. for games).  But now a small exception to this has been made:
			// Prefix keys that are also modifiers (ALT/SHIFT/CTRL/WIN) will now not fire their
			// suffix action on key-up if they modified a mouse button event (since Ctrl-LeftClick,
			// for example, is a valid native action and we don't want to give up that flexibility).
			// WinAPI docs state that for both virtual keys and scan codes:
			// "If there is no translation, the return value is zero."
			// Therefore, zero is never a key that can be validly configured (and likely it's never received here anyway).
			// UPDATE: For performance reasons, this check isn't even done.  Even if sc and vk are both zero, both kvk[0]
			// and ksc[0] should have all their attributes initialized to FALSE so nothing should happen for that key
			// anyway.
			//if (!vk && !sc)
			//    return AllowKeyToGoToSystem;

			if (thisKey.usedAsPrefix == 0 && !thisKey.usedAsSuffix)
			{
				// Fix for v1.1.31.02: This is done regardless of used_as to ensure it doesn't get "stuck down"
				// when a custom combination hotkey Suspends itself, thereby causing used_as to be reset to false.
				// Fix for v1.1.31.03: Done conditionally because its previous value is used below.  This affects
				// modifier keys as hotkeys, such as Shift::MsgBox.
				thisKey.isDown = !keyUp;
				return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
			}

			var hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID; // Set default.
			HotkeyVariant firingIsCertain = null;
			uint hotkeyIdTemp; // For informal/temp storage of the ID-without-flags.
			bool fireWithNoSuppress = false; // Set default.
			bool downPerformedAction = false, wasDownBeforeUp = false;

			if (keyUp)
			{
				// Save prior to reset.  These var's should only be used further below in conjunction with aKeyUp
				// being TRUE.  Otherwise, their values will be unreliable (refer to some other key, probably).
				wasDownBeforeUp = thisKey.isDown;
				downPerformedAction = thisKey.downPerformedAction;  // Save prior to reset below.
				// Reset these values in preparation for the next call to this procedure that involves this key:
				thisKey.downPerformedAction = false;

				if (thisKey.hotkeyToFireUponRelease != HotkeyDefinition.HOTKEY_ID_INVALID)
				{
					hotkeyIdWithFlags = thisKey.hotkeyToFireUponRelease;
					// The line below is done even though the down-event also resets it in case it is ever
					// possible for keys to generate multiple consecutive key-up events (faulty or unusual keyboards?)
					thisKey.hotkeyToFireUponRelease = HotkeyDefinition.HOTKEY_ID_INVALID;
				}

				// v1.1.34.01: Use up the no-suppress ticket early for simplicity and maintainability.  Its value
				// might not be used further below, but in any case the ticket shouldn't be applied to any event
				// after this one.
				if ((thisKey.noSuppress & HotkeyDefinition.NO_SUPPRESS_NEXT_UP_EVENT) != 0)
				{
					fireWithNoSuppress = true;
					thisKey.noSuppress &= ~HotkeyDefinition.NO_SUPPRESS_NEXT_UP_EVENT; // This ticket has been used up, so remove it.
				}
			}

			thisKey.isDown = !keyUp;
			var modifiersWereCorrected = false;

			if (hook == kbdHook)
			{
				// The below was added to fix hotkeys that have a neutral suffix such as "Control & LShift".
				// It may also fix other things and help future enhancements:
				if (thisKey.asModifiersLR != 0)
				{
					// The neutral modifier "Win" is not currently supported.
					kvk[VK_CONTROL].isDown = kvk[VK_LCONTROL].isDown || kvk[VK_RCONTROL].isDown;
					kvk[VK_MENU].isDown = kvk[VK_LMENU].isDown || kvk[VK_RMENU].isDown;
					kvk[VK_SHIFT].isDown = kvk[VK_LSHIFT].isDown || kvk[VK_RSHIFT].isDown;
					// No longer possible because vk is translated early on from neutral to left-right specific:
					// I don't think these ever happen with physical keyboard input, but it might with artificial input:
					//case VK_CONTROL: kvk[sc == SC_RCONTROL ? VK_RCONTROL : VK_LCONTROL].is_down = !aKeyUp; break;
					//case VK_MENU: kvk[sc == SC_RALT ? VK_RMENU : VK_LMENU].is_down = !aKeyUp; break;
					//case VK_SHIFT: kvk[sc == SC_RSHIFT ? VK_RSHIFT : VK_LSHIFT].is_down = !aKeyUp; break;
				}
			}
			else // Mouse hook
			{
				// If the mouse hook is installed without the keyboard hook, update kbdMsSender.modifiersLR_logical
				// manually so that it can be referred to by the mouse hook after this point:
				if (kbdHook == IntPtr.Zero)
				{
					kbdMsSender.modifiersLRLogical = kbdMsSender.modifiersLRLogicalNonIgnored = kbdMsSender.GetModifierLRState(true);
					modifiersWereCorrected = true;
				}
			}

			uint modifiersLRnew;
			var toggleVal = thisKey.ToggleVal(thisKeyIndex);
			var thisToggleKeyCanBeToggled = toggleVal != null && toggleVal.Value == ToggleValueType.Neutral; // Relies on short-circuit boolean order.
			var shk = script.HotkeyData.shk;

			// Prior to considering whether to fire a hotkey, correct the hook's modifier state.
			// Although this is rarely needed, there are times when the OS disables the hook, thus
			// it is possible for it to miss keystrokes.  This should be done before pPrefixKey is
			// consulted below as pPrefixKey itself might be corrected if it is a standard modifier.
			// See comments in GetModifierLRState() for more info:
			if (!modifiersWereCorrected)
			{
				modifiersWereCorrected = true;
				_ = kbdMsSender.GetModifierLRState(true);
			}

			///////////////////////////////////////////////////////////////////////////////////////
			// CASE #1 of 4: PREFIX key has been pressed down.  But use it in this capacity only if
			// no other prefix is already in effect or if this key isn't a suffix.  Update: Or if
			// this key-down is the same as the prefix already down, since we want to be able to
			// a prefix when it's being used in its role as a modified suffix (see below comments).
			///////////////////////////////////////////////////////////////////////////////////////
			if (thisKey.usedAsPrefix != 0 && !keyUp && (prefixKey == null || !thisKey.usedAsSuffix || thisKey == prefixKey))
			{
				// v1.0.41: Even if this prefix key is non-suppressed (passes through to active window),
				// still call PrefixHasNoEnabledSuffixes() because don't want to overwrite the old value of
				// pPrefixKey (see comments in "else" later below).
				// v1.0.44: Added check for PREFIX_ACTUAL so that a PREFIX_FORCED prefix will be considered
				// a prefix even if it has no suffixes.  This fixes an unintentional change in v1.0.41 where
				// naked, neutral modifier hotkeys Control::, Alt::, and Shift:: started firing on press-down
				// rather than release as intended.  The PREFIX_FORCED facility may also provide the means to
				// introduce a new hotkey modifier such as an "up2" keyword that makes any key into a prefix
				// key even if it never acts as a prefix for other keys, which in turn has the benefit of firing
				// on key-up, but only if the no other key was pressed while the user was holding it down.
				bool suppressThisPrefix = (thisKey.noSuppress & HotkeyDefinition.AT_LEAST_ONE_COMBO_HAS_TILDE) == 0; // Set default.
				bool hasNoEnabledSuffixes;

				if (!(hasNoEnabledSuffixes = (thisKey.usedAsPrefix == KeyType.PREFIX_ACTUAL)
											 && HotkeyDefinition.PrefixHasNoEnabledSuffixes(scTakesPrecedence ? sc : vk, scTakesPrecedence, ref suppressThisPrefix)))
				{
					// This check is necessary in cases such as the following, in which the "A" key continues
					// to repeat because pressing a mouse button (unlike pressing a keyboard key) does not
					// stop the prefix key from repeating:
					// $a::send, a
					// a & lbutton::
					if (thisKey != prefixKey)
					{
						// Override any other prefix key that might be in effect with this one, in case the
						// prior one, due to be old for example, was invalid somehow.  UPDATE: It seems better
						// to leave the old one in effect to support the case where one prefix key is modifying
						// a second one in its role as a suffix.  In other words, if key1 is a prefix and
						// key2 is both a prefix and a suffix, we want to leave key1 in effect as a prefix,
						// rather than key2.  Hence, a null-check was added in the above if-stmt:
						prefixKey = thisKey;
						// It should be safe to init this because even if the current key is repeating,
						// it should be impossible to receive here the key-downs that occurred after
						// the first, because there's a return-on-repeat check farther above (update: that check
						// is gone now).  Even if that check weren't done, it's safe to reinitialize this to zero
						// because on most (all?) keyboards & OSs, the moment the user presses another key while
						// this one is held down, the key-repeating ceases and does not resume for
						// this key (though the second key will begin to repeat if it too is held down).
						// In other words, the fear that this would be wrongly initialized and thus cause
						// this prefix's suffix-action to fire upon key-release seems unfounded.
						// It seems easier (and may perform better than alternative ways) to init this
						// here rather than say, upon the release of the prefix key:
						thisKey.wasJustUsed = 0; // Init to indicate it hasn't yet been used in its role as a prefix.
					}
				}

				//else this prefix has no enabled suffixes, so its role as prefix is also disabled.
				// Therefore, don't set pPrefixKey to this_key because don't want the following line
				// (in another section) to execute when a suffix comes in (there may be other reasons too,
				// such as not wanting to lose track of the previous prefix key in cases where the user is
				// holding down more than one prefix):
				// pPrefixKey.was_just_used = AS_PREFIX

				if (thisKey.usedAsSuffix) // v1.0.41: Added this check to avoid doing all of the below when unnecessary.
				{
					// This new section was added May 30, 2004, to fix scenarios such as the following example:
					// a & b::Msgbox a & b
					// $^a::MsgBox a
					// Previously, the ^a hotkey would only fire on key-up (unless it was registered, in which
					// case it worked as intended on the down-event).  When the user presses A, it's okay (and
					// probably desirable) to have recorded that event as a prefix-key-down event (above).
					// But in addition to that, we now check if this is a normal, modified hotkey that should
					// fire now rather than waiting for the key-up event.  This is done because it makes sense,
					// it's more correct, and also it makes the behavior of a hooked ^a hotkey consistent with
					// that of a registered ^a.
					// non_ignored is always used when considering whether a key combination is in place to
					// trigger a hotkey:
					modifiersLRnew = kbdMsSender.modifiersLRLogicalNonIgnored;

					if (thisKey.asModifiersLR != 0) // This will always be false if our caller is the mouse hook.
						// Hotkeys are not defined to modify themselves, so look for a match accordingly.
						modifiersLRnew &= ~thisKey.asModifiersLR;

					// This prefix key's hotkey needs to be checked even if it will ultimately fire only on release.
					// If suppress_this_prefix == false, this prefix key's key-down hotkey should fire immediately.
					// If suppress_this_prefix == true, its final value can only be confirmed by verifying whether
					// this prefix key's hotkey has the no-suppress prefix (which should cause the hotkey to fire
					// immediately and not be suppressed).
					// This prefix key's hotkey should also be fired immediately if there are any modifiers down.
					// Check hook type too in case a script ever explicitly specifies scan code zero as a hotkey:
					//if (modifiersLRnew != 0 || hasNoEnabledSuffixes || (thisKey.noSuppress & HotkeyDefinition.NO_SUPPRESS_PREFIX) != 0)
					//{
					// Check hook type too in case a script every explicitly specifies scan code zero as a hotkey:
					hotkeyIdWithFlags = (hook == kbdHook && scTakesPrecedence)
										? Kscm(modifiersLRnew, sc) : Kvkm(modifiersLRnew, vk);
					hotkeyIdTemp = hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_ID_MASK;

					if (HotkeyDefinition.IsAltTab(hotkeyIdTemp))
					{
						hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID; // Let it be rediscovered when the key is released.
					}
					else if (hotkeyIdWithFlags != HotkeyDefinition.HOTKEY_ID_INVALID)
					{
						if (!suppressThisPrefix) // v1.1.34.02: Retain this as a flag for key-up.
							thisKey.wasJustUsed = KeyType.AS_PASSTHROUGH_PREFIX;

						if (suppressThisPrefix && modifiersLRnew == 0) // So far, it looks like the prefix should be suppressed.
						{
							char? ch = null;
							firingIsCertain = HotkeyDefinition.CriterionFiringIsCertain(ref hotkeyIdWithFlags, keyUp, extraInfo, ref fireWithNoSuppress, ref ch);

							if (firingIsCertain == null || !fireWithNoSuppress) // Hotkey is ineligible to fire or lacks the no-suppress prefix.
							{
								// Resetting the ID is necessary to avoid the following cases:
								//  1) A key-down hotkey which isn't eligible to fire prevents the prefix key from being suppressed.
								//  2) A key-down hotkey which isn't eligible to fire causes its key-up counterpart to fire even if
								//     the prefix key was used to activate a custom combo.
								//  3) A key-down hotkey without ~ fires immediately instead of on release.
								//  4) A key-up hotkey without ~ fires even if the prefix key was used to activate a custom combo.
								if (hotkeyIdWithFlags < shk.Count && hotkeyUp[(int)hotkeyIdWithFlags] != HotkeyDefinition.HOTKEY_ID_INVALID)
								{
									// This key-down hotkey has a key-up counterpart.
									fireWithNoSuppress = false; // Reset for the call below.
									var hkuwf = hotkeyUp[(int)hotkeyIdWithFlags];
									var firingUp = HotkeyDefinition.CriterionFiringIsCertain(ref hkuwf, keyUp, extraInfo, ref fireWithNoSuppress, ref ch);
									hotkeyUp[(int)hotkeyIdWithFlags] = hkuwf;

									if (!(firingUp != null && fireWithNoSuppress)) // Both key-down and key-up are either ineligible or lack the no-suppress prefix.
										hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID; // See comments above about resetting the ID.
									else if (firingIsCertain != null) // Both key-down and key-up are eligible, but key-down should be suppressed.
										fireWithNoSuppress = false; // For backward-compatibility, suppress the key-down but leave hotkey_id_with_flags set so it fires immediately.
									else // Key-down is not eligible, but key-up is.
									{
										firingIsCertain = firingUp;
										hotkeyIdWithFlags = hotkeyUp[(int)hotkeyIdWithFlags];
									}
								}
								else
									hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID; // See comments above about resetting the ID.
							}
						}

						if ((hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_KEY_UP) != 0) // And it's okay even if it's is HotkeyDefinition.HOTKEY_ID_INVALID.
						{
							// Queue it for later, which is done here rather than upon release of the key so that
							// the user can release the key's modifiers before releasing the key itself, which
							// is likely to happen pretty often. v1.0.41: This is done even if the hotkey is subject
							// to #HotIf because it seems more correct to check those criteria at the actual time
							// the key is released rather than now:
							thisKey.hotkeyToFireUponRelease = hotkeyIdWithFlags;
							hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID;
						}
						else if (hotkeyIdWithFlags < shk.Count)//Valid key-down hotkey.
						{
							thisKey.hotkeyToFireUponRelease = hotkeyUp[(int)hotkeyIdWithFlags];//Might assign HotkeyDefinition.HOTKEY_ID_INVALID.
							// Since this prefix key is being used in its capacity as a suffix instead,
							// hotkey_id_with_flags now contains a hotkey ready for firing later below.
							// v1.0.41: Above is done even if the hotkey is subject to #HotIf because:
							// 1) The down-hotkey's #HotIf criteria might be different from that of the up's.
							// 2) It seems more correct to check those criteria at the actual time the key is
							// released rather than now (and also probably reduces code size).
						}
					}

					// Alt-tab need not be checked here (like it is in the similar section below) because all
					// such hotkeys use (or were converted at load-time to use) a modifier_vk, not a set of
					// modifiers or modifierlr's.
				} // if (this_key.used_as_suffix)

				if (hotkeyIdWithFlags == HotkeyDefinition.HOTKEY_ID_INVALID)
				{
					if (hasNoEnabledSuffixes)
					{
						keyHistoryCurr.eventType = '#'; // '#' to indicate this prefix key is disabled due to #HotIf WinActive/Exist criterion.
					}

					// In this case, a key-down event can't trigger a suffix, so return immediately.
					// If our caller is the mouse hook, both of the following will always be false:
					// this_key.as_modifiersLR
					// this_toggle_key_can_be_toggled
					if (!suppressThisPrefix) // Only for this condition. Not needed for toggle keys and not wanted for modifiers as it would prevent menu suppression.
						thisKey.noSuppress |= HotkeyDefinition.NO_SUPPRESS_NEXT_UP_EVENT; // Since the "down" is non-suppressed, so should the "up".

					if (thisKey.asModifiersLR != 0 || !suppressThisPrefix || thisToggleKeyCanBeToggled)
						return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

					// Mark this key as having been suppressed.  This currently doesn't have any known effect
					// since the change to tilde (~) handling in v1.0.95 (commit 161162b8), but may in future.
					thisKey.hotkeyDownWasSuppressed = true;
					return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));
				}

				//else valid suffix hotkey has been found; this will now fall through to Case #4 by virtue of aKeyUp==false.
			}

			//////////////////////////////////////////////////////////////////////////////////
			// CASE #2 of 4: SUFFIX key (that's not a prefix, or is one but has just been used
			// in its capacity as a suffix instead) has been released.
			// This is done before Case #3 for performance reasons.
			//////////////////////////////////////////////////////////////////////////////////
			// v1.0.37.05: Added "|| down_performed_action" to the final check below because otherwise a
			// script such as the following would send two M's for +b, one upon down and one upon up:
			// +b::Send, M
			// b & z::return
			// I don't remember exactly what the "pPrefixKey != &this_key" check is for below, but it is kept
			// to minimize the chance of breaking other things:
			var fellThroughFromCase2 = false; // Set default.

			if (thisKey.usedAsSuffix && keyUp && (prefixKey != thisKey || downPerformedAction)) // Note: hotkey_id_with_flags might be already valid due to this_key.hotkey_to_fire_upon_release.
			{
				if (prefixKey == thisKey) // v1.0.37.05: Added so that scripts such as the example above don't leave pPrefixKey wrongly non-NULL.
					prefixKey = null;       // Also, it seems unnecessary to check this_key.it_put_alt_down and such like is done in Case #3.

				// If it did perform an action, suppress this key-up event.  Do this even
				// if this key is a modifier because it's previous key-down would have
				// already been suppressed (since this case is for suffixes that aren't
				// also prefixes), thus the key-up can be safely suppressed as well.
				// It's especially important to do this for keys whose up-events are
				// special actions within the OS, such as AppsKey, Lwin, and Rwin.
				// Toggleable keys are also suppressed here on key-up because their
				// previous key-down event would have been suppressed in order for
				// down_performed_action to be true.  UPDATE: Added handling for
				// NO_SUPPRESS_NEXT_UP_EVENT and also applied this next part to both
				// mouse and keyboard.
				// v1.0.40.01: It was observed that a hotkey that consists of a mouse button as a prefix and
				// a keyboard key as a suffix can cause sticking keys in rare cases.  For example, when
				// "MButton & LShift" is a hotkey, if you hold down LShift long enough for it to begin
				// auto-repeating then press MButton, the hotkey fires the next time LShift auto-repeats (since
				// pressing a mouse button doesn't stop a keyboard key from auto-repeating).  Fixing that type
				// of firing seems likely to break more things than it fixes.  But since it isn't fixed, when
				// the user releases LShift, the up-event is suppressed here, which causes the key to get
				// stuck down.  That could be fixed in the following ways, but all of them seem likely to break
				// more things than they fix, especially given the rarity that both a hotkey of this type would
				// exist and its mirror image does something useful that isn't a hotkey (for example, Shift+MButton
				// is a meaningful action in few if any applications):
				// 1) Don't suppress the physical release of a suffix key if that key is logically down (as reported
				//    by GetKeyState/GetAsyncKeyState): Seems too broad in scope because there might be cases where
				//    the script or user wants the key to stay logically down (e.g. Send {Shift down}{a down}).
				// 2) Same as #1 but limit the non-suppression to only times when the suffix key was logically down
				//    when its first qualified physical down-event came in.  This is definitely better but like
				//    #1, the uncertainty of breaking existing scripts and/or causing more harm than good seems too
				//    high.
				// 3) Same as #2 but limit it only to cases where the suffix key is a keyboard key and its prefix
				//    is a mouse key.  Although very selective, it doesn't mitigate the fact it might still do more
				//    harm than good and/or break existing scripts.
				// In light of the above, it seems best to keep this documented here as a known limitation for now.
				//
				// v1.0.28: The following check is done to support certain keyboards whose keys or scroll wheels
				// generate up events without first having generated any down-event for the key.  UPDATE: I think
				// this check is now also needed to allow fall-through in cases like "b" and "b up" both existing.
				if (!thisKey.usedAsKeyUp)
				{
					return (downPerformedAction && !fireWithNoSuppress) ?
						   new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null)) :
						   new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
				}

				//else continue checking to see if the right modifiers are down to trigger one of this
				// suffix key's key-up hotkeys.
				fellThroughFromCase2 = true;
			}

			//////////////////////////////////////////////
			// CASE #3 of 4: PREFIX key has been released.
			//////////////////////////////////////////////
			if (thisKey.usedAsPrefix != 0 && keyUp) // If these are true, hotkey_id_with_flags should be valid only by means of this_key.hotkey_to_fire_upon_release.
			{
				if (prefixKey == thisKey)
					prefixKey = null;

				// Else it seems best to keep the old one in effect.  This could happen, for example,
				// if the user holds down prefix1, holds down prefix2, then releases prefix1.
				// In that case, we would want to keep the most recent prefix (prefix2) in effect.
				// This logic would fail to work properly in a case like this if the user releases
				// prefix2 but still has prefix1 held down.  The user would then have to release
				// prefix1 and press it down again to get the hook to realize that it's in effect.
				// This seems very unlikely to be something commonly done by anyone, so for now
				// it's just documented here as a limitation.

				if (thisKey.itPutAltDown) // key pushed ALT down, or relied upon it already being down, so go up:
				{
					thisKey.itPutAltDown = false;
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_MENU);
				}

				if (thisKey.itPutShiftDown) // similar to above
				{
					thisKey.itPutShiftDown = false;
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_SHIFT);
				}

				if (thisToggleKeyCanBeToggled) // Always false if our caller is the mouse hook.
				{
					// It's done this way because CapsLock, for example, is a key users often
					// press quickly while typing.  I suspect many users are like me in that
					// they're in the habit of not having releasing the CapsLock key quite yet
					// before they resume typing, expecting it's new mode to be in effect.
					// This resolves that problem by always toggling the state of a toggleable
					// key upon key-down.  If this key has just acted in its role of a prefix
					// to trigger a suffix action, toggle its state back to what it was before
					// because the firing of a hotkey should not have the side-effect of also
					// toggling the key:
					// Toggle the key by replacing this key-up event with a new sequence
					// of our own.  This entire-replacement is done so that the system
					// will see all three events in the right order:
					if (thisKey.wasJustUsed == KeyType.AS_PREFIX_FOR_HOTKEY) // If this is true, it's probably impossible for hotkey_id_with_flags to be valid by means of this_key.hotkey_to_fire_upon_release.
					{
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk, sc, IntPtr.Zero, false, KeyPhysIgnore);// Mark it as physical for any other hook instances.
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDownAndUp, vk, sc);
						return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));
					}

					// Otherwise, if it was used to modify a non-suffix key, or it was just
					// pressed and released without any keys in between, don't suppress its up-event
					// at all.  UPDATE: Don't return here if it didn't modify anything because
					// this prefix might also be a suffix. Let later sections handle it then.
					if (thisKey.wasJustUsed == KeyType.AS_PREFIX)
						return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
				}
				else // It's not a toggleable key, or it is but it's being kept forcibly on or off.

					// Seems safest to suppress this key if the user pressed any non-modifier key while it
					// was held down.  As a side-effect of this, if the user holds down numlock, for
					// example, and then presses another key that isn't actionable (i.e. not a suffix),
					// the numlock state won't be toggled even it's normally configured to do so.
					// This is probably the right thing to do in most cases.
					// Older note:
					// In addition, this suppression is relied upon to prevent toggleable keys from toggling
					// when they are used to modify other keys.  For example, if "Capslock & A" is a hotkey,
					// the state of the Capslock key should not be changed when the hotkey is pressed.
					// Do this check prior to the below check (give it precedence).
					if (thisKey.wasJustUsed > 0  // AS_PREFIX or AS_PREFIX_FOR_HOTKEY.  v1.1.34.02: Excludes AS_PASSTHROUGH_PREFIX, which would indicate the prefix key's suffix hotkey should always fire.
							&& hotkeyIdWithFlags == HotkeyDefinition.HOTKEY_ID_INVALID) // v1.0.44.04: Must check this because this prefix might be being used in its role as a suffix instead.
					{
						if (thisKey.asModifiersLR != 0 // Always false if our caller is the mouse hook.
								|| fireWithNoSuppress) // Shouldn't be true unless it's a modifier, but seems safest to check anyway.
							return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));// Win/Alt will be disguised if needed.

						return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));
					}

				// v1.0.41: This spot cannot be reached when a disabled prefix key's up-action fires on
				// key-down instead (via Case #1).  This is because upon release, that prefix key would be
				// returned from in Case #2 (by virtue of its check of down_performed_action).

				// Since above didn't return, this key-up for this prefix key wasn't used in it's role
				// as a prefix.  If it's not a suffix, we're done, so just return.  Don't do
				// "DisguiseWinAlt" because we want the key's native key-up function to take effect.
				// Also, allow key-ups for toggleable keys that the user wants to be toggleable to
				// go through to the system, because the prior key-down for this prefix key
				// wouldn't have been suppressed and thus this up-event goes with it (and this
				// up-event is also needed by the OS, at least WinXP, to properly set the indicator
				// light and toggle state):
				if (!thisKey.usedAsSuffix)
					// If our caller is the mouse hook, both of the following will always be false:
					// this_key.as_modifiersLR
					// this_toggle_key_can_be_toggled
					return (thisKey.asModifiersLR != 0
							|| fireWithNoSuppress
							|| thisToggleKeyCanBeToggled) ?
						   new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null)) :
						   new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));

				// Since the above didn't return, this key is both a prefix and a suffix, but
				// is currently operating in its capacity as a suffix.
				// If this key wasn't thought to be down prior to this up-event, it's probably because
				// it is registered with another prefix by RegisterHotkey().  In this case, the keyup
				// should be passed back to the system rather than performing it's key-up suffix
				// action.  UPDATE: This can't happen with a low-level hook.  But if there's another
				// low-level hook installed that receives events before us, and it's not
				// well-implemented (i.e. it sometimes sends ups without downs), this check
				// may help prevent unexpected behavior.  UPDATE: The check "!this_key.used_as_key_up"
				// is now done too so that an explicit key-up hotkey can operate even if the key wasn't
				// thought to be down before. One thing this helps with is certain keyboards (e.g. some
				// Dells) that generate only up events for some of their special keys but no down events,
				// even when *no* keyboard management software is installed). Some keyboards also have
				// scroll wheels that generate a stream of up events in one direction and down in the other.
				if (!(wasDownBeforeUp || thisKey.usedAsKeyUp)) // Verified correct.
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

				//else v1.0.37.05: Since no suffix action was triggered while it was held down, fall through
				// rather than returning so that the key's own unmodified/naked suffix action will be considered.
				// For example:
				// a & b::
				// a::   // This fires upon release of "a".
			}

			////////////////////////////////////////////////////////////////////////////////////////////////////
			// CASE #4 of 4: SUFFIX key has been pressed down (or released if it's a key-up event, in which case
			// it fell through from CASE #3 or #2 above).  This case can also happen if it fell through from
			// case #1 (i.e. it already determined the value of hotkey_id_with_flags).
			////////////////////////////////////////////////////////////////////////////////////////////////////
			HotkeyDefinition foundHk = null; // Custom combo hotkey found by case #4.

			if (prefixKey != null && (!keyUp || thisKey.usedAsKeyUp) && hotkeyIdWithFlags == HotkeyDefinition.HOTKEY_ID_INVALID) // Helps performance by avoiding all the below checking.
			{
				// Action here is considered first, and takes precedence since a suffix's ModifierVK/SC should
				// take effect regardless of whether any win/ctrl/alt/shift modifiers are currently down, even if
				// those modifiers themselves form another valid hotkey with this suffix.  In other words,
				// ModifierVK/SC combos take precedence over normally-modified combos:
				for (hotkeyIdTemp = thisKey.firstHotkey; hotkeyIdTemp != HotkeyDefinition.HOTKEY_ID_INVALID;)
				{
					var this_hk = shk[(int)hotkeyIdTemp]; // hotkey_id_temp does not include flags in this case.

					if (!(this_hk.modifierVK != 0 || this_hk.modifierSC != 0))
						break; // Not a custom combo.

					hotkeyIdTemp = this_hk.nextHotkey;
					var thisModifierKey = this_hk.modifierVK != 0 ? kvk[this_hk.modifierVK] : ksc[this_hk.modifierSC];

					// The following check supports the prefix+suffix pairs that have both an up hotkey and a down,
					// such as:
					//a & b::     ; Down.
					//a & b up::  ; Up.
					//MsgBox %A_ThisHotkey%
					//return
					if (thisModifierKey.isDown) // A prefix key qualified to trigger this suffix is down.
					{
						if (this_hk.keyUp)
						{
							if (!keyUp) // Key-up hotkey but the event is a down-event.
							{
								// Queue the up-hotkey for later so that the user is free to release the
								// prefix key prior to releasing the suffix (which seems quite common and
								// thus desirable).  v1.0.41: This is done even if the hotkey is subject
								// to #HotIf because it seems more correct to check those criteria at the actual time
								// the key is released rather than now:
								thisKey.hotkeyToFireUponRelease = this_hk.id;

								if (foundHk != null) // i.e. a previous iteration already found the down-event to fire.
									break;

								//else continue searching for the down hotkey that goes with this up (if any).
							}
							else // this hotkey is qualified to fire.
							{
								foundHk = this_hk;
								break;
							}
						}
						else // This is a normal hotkey that fires on suffix-key-down.
						{
							if (!keyUp)
							{
								if (foundHk == null) // Use the first one found (especially important when both "a & Control" and "a & LControl" are present).
									foundHk = this_hk;

								// and continue searching for the up hotkey (if any) to queue up for firing upon the key's release).
							}

							//else this key-down hotkey can't fire because the current event is a up-event.
							// But continue searching for an up-hotkey in case this key is of the type that never
							// generates down-events (e.g. certain Dell keyboards).
						}
					} // qualified prefix is down
				} // for each prefix of this suffix

				if (foundHk != null)
				{
					// Update pPrefixKey, even though it was probably already done close to the top of the
					// function, just in case this hotkey uses a different prefix key (perhaps because there
					// is currently more than one prefix being held down).
					// Since the hook is now designed to receive only left/right specific modifier keys
					// -- never the neutral keys -- never indicate that a neutral prefix key is down because
					// then it would never be released properly by the other main prefix/suffix handling
					// cases of the hook.  Instead, always identify which prefix key (left or right) is
					// in effect:

					prefixKey = foundHk.modifierVK switch
				{
						VK_SHIFT => kvk[kvk[VK_RSHIFT].isDown ? VK_RSHIFT : VK_LSHIFT],
							VK_CONTROL => kvk[kvk[VK_RCONTROL].isDown ? VK_RCONTROL : VK_LCONTROL],
							VK_MENU => kvk[kvk[VK_RMENU].isDown ? VK_RMENU : VK_LMENU],
							0 => ksc[foundHk.modifierSC],
							_ => kvk[foundHk.modifierVK],
					};

					if (foundHk.hookAction != 0)
						hotkeyIdWithFlags = foundHk.hookAction;
					else
						hotkeyIdWithFlags = foundHk.id; // Flags not needed.

					hotkeyIdTemp = hotkeyIdWithFlags;
					// Let the section further below handle evaluating the hotkey's criterion, since it takes
					// care of determining suppression based on a key-down hotkey's key-up counterpart, etc.
				}

				// Alt-tab: Alt-tab actions that require a prefix key are handled directly here rather than via
				// posting a message back to the main window.  In part, this is because it would be difficult
				// to design a way to tell the main window when to release the alt-key.
				if (hotkeyIdTemp == HotkeyDefinition.HOTKEY_ID_ALT_TAB || hotkeyIdTemp == HotkeyDefinition.HOTKEY_ID_ALT_TAB_SHIFT)
				{
					if (prefixKey.wasJustUsed != KeyType.AS_PASSTHROUGH_PREFIX)
						prefixKey.wasJustUsed = KeyType.AS_PREFIX_FOR_HOTKEY;

					// Not sure if it's necessary to set this in this case.  Review.
					if (!keyUp)
						thisKey.downPerformedAction = true; // aKeyUp is known to be false due to an earlier check.

					if ((kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT)) == 0)  // Neither ALT key is down.
						// Note: Don't set the ignore-flag in this case because we want the hook to notice it.
						// UPDATE: It might be best, after all, to have the hook ignore these keys.  That's because
						// we want to avoid any possibility that other hotkeys will fire off while the user is
						// alt-tabbing (though we can't stop that from happening if they were registered with
						// RegisterHotkey).  In other words, since the
						// alt-tab window is in the foreground until the user released the substitute-alt key,
						// don't allow other hotkeys to be activated.  One good example that this helps is the case
						// where <key1> & rshift is defined as alt-tab but <key1> & <key2> is defined as shift-alt-tab.
						// In that case, if we didn't ignore these events, one hotkey might unintentionally trigger
						// the other.
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, VK_MENU);

					// And leave it down until a key-up event on the prefix key occurs.

					if ((vk == VK_LCONTROL || vk == VK_RCONTROL) && !keyUp)
						// Even though this suffix key would have been suppressed, it seems that the
						// OS's alt-tab functionality sees that it's down somehow and thus this is necessary
						// to allow the alt-tab menu to appear.  This doesn't need to be done for any other
						// modifier than Control, nor any normal key since I don't think normal keys
						// being in a down-state causes any problems with alt-tab:
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk, sc);

					// Update the prefix key's
					// flag to indicate that it was this key that originally caused the alt-key to go down,
					// so that we know to set it back up again when the key is released.  UPDATE: Actually,
					// it's probably better if this flag is set regardless of whether ALT is already down.
					// That way, in case it's state go stuck down somehow, it will be reset by an Alt-TAB
					// (i.e. alt-tab will always behave as expected even if ALT was down before starting).
					// Note: pPrefixKey must already be non-NULL or this couldn't be an alt-tab event:
					prefixKey.itPutAltDown = true;

					if (hotkeyIdTemp == HotkeyDefinition.HOTKEY_ID_ALT_TAB_SHIFT)
					{
						if ((kbdMsSender.modifiersLRLogical & (MOD_LSHIFT | MOD_RSHIFT)) == 0) // Neither SHIFT key is down.
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, VK_SHIFT);  // Same notes apply to this key.

						prefixKey.itPutShiftDown = true;
					}
					// And this may do weird things if VK_TAB itself is already assigned a as a naked hotkey, since
					// it will recursively call the hook, resulting in the launch of some other action.  But it's hard
					// to imagine someone ever reassigning the naked VK_TAB key (i.e. with no modifiers).
					// UPDATE: The new "ignore" method should prevent that.  Or in the case of low-level hook:
					// keystrokes sent by our own app by default will not fire hotkeys.  UPDATE: Even though
					// the LL hook will have suppressed this key, it seems that the OS's alt-tab menu uses
					// some weird method (apparently not GetAsyncState(), because then our attempt to put
					// it up would fail) to determine whether the shift-key is down, so we need to still do this:
					else if (hotkeyIdTemp == HotkeyDefinition.HOTKEY_ID_ALT_TAB) // i.e. it's not shift-alt-tab
					{
						// Force it to be alt-tab as the user intended.
						if ((vk == VK_LSHIFT || vk == VK_RSHIFT) && !keyUp)  // Needed.  See above comments. vk == VK_SHIFT not needed.
							// If a shift key is the suffix key, this must be done every time,
							// not just the first:
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk, sc);

						// UPDATE: Don't do "else" because sometimes the opposite key may be down, so the
						// below needs to be unconditional:
						//else

						// In the below cases, it's not necessary to put the shift key back down because
						// the alt-tab menu only disappears after the prefix key has been released (and
						// it's not realistic that a user would try to trigger another hotkey while the
						// alt-tab menu is visible).  In other words, the user will be releasing the
						// shift key anyway as part of the alt-tab process, so it's not necessary to put
						// it back down for the user here (the shift stays in effect as a prefix for us
						// here because it's sent as an ignore event -- but the prefix will be correctly
						// canceled when the user releases the shift key).
						if ((kbdMsSender.modifiersLRLogical & MOD_LSHIFT) != 0)
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_LSHIFT);

						if ((kbdMsSender.modifiersLRLogical & MOD_RSHIFT) != 0)
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_RSHIFT);
					}

					// Any down control key prevents alt-tab from working.  This is similar to
					// what's done for the shift-key above, so see those comments for details.
					if ((kbdMsSender.modifiersLRLogical & MOD_LCONTROL) != 0)
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_LCONTROL);

					if ((kbdMsSender.modifiersLRLogical & MOD_RCONTROL) != 0)
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_RCONTROL);

					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDownAndUp, VK_TAB);

					if (hotkeyIdTemp == HotkeyDefinition.HOTKEY_ID_ALT_TAB_SHIFT && prefixKey.itPutShiftDown
							&& ((vk >= VK_NUMPAD0 && vk <= VK_NUMPAD9) || vk == VK_DECIMAL)) // dual-state numpad key.
					{
						// In this case, if there is a numpad key involved, it's best to put the shift key
						// back up in between every alt-tab to avoid problems caused due to the fact that
						// the shift key being down would CHANGE the VK being received when the key is
						// released (due to the fact that SHIFT temporarily disables numlock).
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_SHIFT);
						prefixKey.itPutShiftDown = false;  // Reset for next time since we put it back up already.
					}

					keyHistoryCurr.eventType = 'h'; // h = hook hotkey (not one registered with RegisterHotkey)

					if (!keyUp)
						thisKey.hotkeyDownWasSuppressed = true;

					return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));
				} // end of alt-tab section.

				// Since above didn't return, this isn't a prefix-triggered alt-tab action (though it might be
				// a non-prefix alt-tab action, which is handled later below).
			} // end of section that searches for a suffix modified by the prefix that's currently held down.

			if (hotkeyIdWithFlags == HotkeyDefinition.HOTKEY_ID_INVALID)  // Since above didn't find a hotkey, check if modifiers+this_key qualifies a firing.
			{
				modifiersLRnew = kbdMsSender.modifiersLRLogicalNonIgnored;

				if (thisKey.asModifiersLR != 0)
					// Hotkeys are not defined to modify themselves, so look for a match accordingly.
					modifiersLRnew &= ~thisKey.asModifiersLR;

				// Check hook type too in case a script every explicitly specifies scan code zero as a hotkey:
				hotkeyIdWithFlags = (hook == kbdHook && scTakesPrecedence)
									? Kscm(modifiersLRnew, sc) : Kvkm(modifiersLRnew, vk);

				// Bug fix for v1.0.20: The below second attempt is no longer made if the current keystroke
				// is a tab-down/up  This is because doing so causes any naked TAB that has been defined as
				// a hook hotkey to incorrectly fire when the user holds down ALT and presses tab two or more
				// times to advance through the alt-tab menu.  Here is the sequence:
				// $TAB is defined as a hotkey in the script.
				// User holds down ALT and presses TAB two or more times.
				// The Alt-tab menu becomes visible on the first TAB keystroke.
				// The $TAB hotkey fires on the second keystroke because of the below (now-fixed) logic.
				// By the way, the overall idea behind the below might be considered faulty because
				// you could argue that non-modified hotkeys should never be allowed to fire while ALT is
				// down just because the alt-tab menu is visible.  However, it seems justified because
				// the benefit (which I believe was originally and particularly that an unmodified mouse button
				// or wheel hotkey could be used to advance through the menu even though ALT is artificially
				// down due to support displaying the menu) outweighs the cost, which seems low since
				// it would be rare that anyone would press another hotkey while they are navigating through
				// the Alt-Tab menu.
				if (hotkeyIdWithFlags == HotkeyDefinition.HOTKEY_ID_INVALID && altTabMenuIsVisible && vk != VK_TAB)
				{
					// Try again, this time without the ALT key in case the user is trying to
					// activate an alt-tab related key (i.e. a special hotkey action such as AltTab
					// that relies on the Alt key being logically but not physically down).
					modifiersLRnew &= ~(MOD_LALT | MOD_RALT);
					hotkeyIdWithFlags = (hook == kbdHook && scTakesPrecedence)
										? Kscm(modifiersLRnew, sc) : Kvkm(modifiersLRnew, vk);
					// Fix for v1.0.28: If the ID isn't an alt-tab type, don't consider it to be valid.
					// Someone pointed out that pressing Alt-Tab and then pressing ESC while still holding
					// down ALT fired the ~Esc hotkey even when it should just dismiss the alt-tab menu.
					// Note: Both of the below checks must be done because the high-order bits of the
					// hotkey_id_with_flags might be set to indicate no-suppress, etc:
					hotkeyIdTemp = hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_ID_MASK;

					if (!HotkeyDefinition.IsAltTab(hotkeyIdTemp))
						hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID; // Since it's not an Alt-tab action, don't fire this hotkey.
				}

				if ((hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_KEY_UP) != 0)
				{
					if (!keyUp) // Key-up hotkey but the event is a down-event.
					{
						// Fixed for v1.1.33.01: Any key-up hotkey already found by the custom combo section
						// should take precedence over this hotkey.  This fixes "a up::" erroneously taking
						// precedence over "b & a up::" when "a::" is not defined, which resulted in either
						// firing the wrong hotkey or firing the right hotkey but not suppressing the key.
						if (thisKey.hotkeyToFireUponRelease == HotkeyDefinition.HOTKEY_ID_INVALID)
							thisKey.hotkeyToFireUponRelease = hotkeyIdWithFlags; // See comments above in other occurrences of this line.

						// v1.1.33.03: ChangeHookState now avoids pairing an up hotkey with a more permissive
						// down hotkey; e.g. "<^a up" and "^a" won't be paired, since that would cause "<^a up"
						// to fire when RCtrl+A is pressed.  To support them both firing on LCtrl+A, this looks
						// for any key-down hotkey which might be elegible to fire.  It's okay if this hotkey
						// has no eligible variants, because Hotkey::CriterionFiringIsCertain will handle that.
						hotkeyIdWithFlags = HotkeyDefinition.FindPairedHotkey(thisKey.firstHotkey, kbdMsSender.modifiersLRLogicalNonIgnored, false);
					}

					//else hotkey_id_with_flags contains the up-hotkey that is now eligible for firing.
				}
				else if (hotkeyIdWithFlags != HotkeyDefinition.HOTKEY_ID_INVALID) // hotkey_id_with_flags is a valid key-down hotkey.
				{
					hotkeyIdTemp = hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_ID_MASK;

					if (keyUp)
					{
						// Even though the key is being released, a hotkey should fire unconditionally because
						// the only way we can reach this exact point for a non-key-up hotkey is when it fell
						// through from Case #3, in which case this hotkey_id_with_flags is implicitly a key-up
						// hotkey if there is no actual explicit key-up hotkey for it.  UPDATE: It is now possible
						// to fall through from Case #2, so that is checked below.
						if (hotkeyIdTemp < shk.Count && hotkeyUp[(int)hotkeyIdTemp] != HotkeyDefinition.HOTKEY_ID_INVALID) // Relies on short-circuit boolean order.
						{
							if (fellThroughFromCase2
									|| (firingIsCertain = HotkeyDefinition.CriterionFiringIsCertain(ref hotkeyIdWithFlags, keyUp, extraInfo, ref fireWithNoSuppress, ref keyHistoryCurr.eventType)) != null)
							{
								// The key-down hotkey isn't eligible for firing, so fall back to the key-up hotkey:
								hotkeyIdWithFlags = hotkeyUp[(int)hotkeyIdTemp];
							}

							//else: the key-down hotkey is eligible for firing, so leave hotkey_id_with_flags as-is
							// and SuppressThisKeyFunc() or AllowIt() will post both hotkey-down and hotkey-up,
							// allowing remappings and other hotkey down-up pairs to work.
						}
						else // Leave it at its former value unless case#2.  See comments above and below.

							// Fix for v1.0.44.09: Since no key-up counterpart was found above (either in hotkey_up[]
							// or via the HOTKEY_KEY_UP flag), don't fire this hotkey when it fell through from Case #2.
							// This prevents a hotkey like $^b from firing TWICE (once on down and again on up) when a
							// key-up hotkey with different modifiers also exists, such as "#b" and "#b up" existing with $^b.
							if (fellThroughFromCase2)
								hotkeyIdWithFlags = HotkeyDefinition.HOTKEY_ID_INVALID;
					}
					else // hotkey_id_with_flags contains the down-hotkey that is now eligible for firing. But check if there's an up-event to queue up for later.
						if (hotkeyIdTemp < shk.Count)
						{
							// Fixed for v1.1.33.01: Any key-up hotkey already found by the custom combo section
							// should take precedence over this hotkey.  This fixes "b & a up::" not suppressing
							// "a" when "a::" is defined but disabled by #If and "b & a::" is not defined.
							if (thisKey.hotkeyToFireUponRelease == HotkeyDefinition.HOTKEY_ID_INVALID)
								thisKey.hotkeyToFireUponRelease = hotkeyUp[(int)hotkeyIdTemp];
						}
				}

				// Check hotkey_id_with_flags again now that the above possibly changed it:
				if (hotkeyIdWithFlags == HotkeyDefinition.HOTKEY_ID_INVALID)
				{
					// Even though at this point this_key is a valid suffix, no actionable ModifierVK/SC
					// or modifiers were pressed down, so just let the system process this normally
					// (except if it's a toggleable key).  This case occurs whenever a suffix key (which
					// is also a prefix) is released but the key isn't configured to perform any action
					// upon key-release.  Currently, I think the only way a key-up event will result
					// in a hotkey action is for the release of a naked/modifierless prefix key.
					// Example of a configuration that would result in this case whenever Rshift alone
					// is pressed then released:
					// RControl & RShift = Alt-Tab
					// RShift & RControl = Shift-Alt-Tab
					if (keyUp)
						// These sequence is basically the same as the one used in Case #3
						// when a prefix key that isn't a suffix failed to modify anything
						// and was then released, so consider any modifications made here
						// or there for inclusion in the other one.  UPDATE: Since
						// the previous sentence is a bit obsolete, describe this better:
						// If it's a toggleable key that the user wants to allow to be
						// toggled, just allow this up-event to go through because the
						// previous down-event for it (in its role as a prefix) would not
						// have been suppressed:
						return (thisKey.asModifiersLR != 0
								// The following line was added for v1.0.37.02 to take into account key-up hotkeys,
								// the release of which should never be suppressed if it didn't actually fire the
								// up-hotkey (due to the wrong modifiers being down):
								|| thisKey.usedAsPrefix == 0
								|| fireWithNoSuppress
								// The order on this line important; it relies on short-circuit boolean:
								|| thisToggleKeyCanBeToggled) ?
							   new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null)) :
							   new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));

					// v1.0.37.02: Added !this_key.used_as_prefix for mouse hook too (see comment above).

					// For execution to have reached this point, the following must be true:
					// 1) aKeyUp==false
					// 2) this_key is not a prefix, or it is also a suffix but some other custom prefix key
					//    is being held down (otherwise, Case #1 would have returned).
					// 3) No hotkey is eligible to fire.
					// If this_key is a prefix under these conditions, there are some combinations that are
					// inconsistent with Case #1.  Case #1 would pass it through if it has no enabled suffixes,
					// or it's a modifier/toggleable key, but otherwise would suppress it.  By contrast, this
					// section would unconditionally pass through a prefix key if the user was already holding
					// another prefix key.  Just suppressing it doesn't seem useful since it still wouldn't
					// function as a prefix key (since case #1 didn't set pPrefixKey to this_key), and fixing
					// that would change the behaviour in ways that might be undesired, so it's left as is.
					if (thisKey.hotkeyToFireUponRelease == HotkeyDefinition.HOTKEY_ID_INVALID)
						return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

					char? ch = null;

					// Otherwise (v1.0.44): Since there is a hotkey to fire upon release (somewhat rare under these conditions),
					// check if any of its criteria will allow it to fire, and if so whether that variant is non-suppressed.
					// If it is, this down-even should be non-suppressed too (for symmetry).  This check isn't 100% reliable
					// because the active/existing windows checked by the criteria might change before the user actually
					// releases the key, but there doesn't seem any way around that.
					if (HotkeyDefinition.CriterionFiringIsCertain(ref thisKey.hotkeyToFireUponRelease // firing_is_certain==false under these conditions, so no need to check it.
							, true  // Always a key-up since it will fire upon release.
							, extraInfo // May affect the result due to #InputLevel.  Assume the key-up's SendLevel will be the same as the key-down.
							, ref fireWithNoSuppress, ref ch) == null)// fire_with_no_suppress is the value we really need to get back from it.
						fireWithNoSuppress = true; // Although it's not "firing" in this case; just for use below.

					thisKey.hotkeyDownWasSuppressed = !fireWithNoSuppress; // Fixed for v1.1.33.01: If this isn't set, the key-up won't be suppressed even after the key-down is.
					return fireWithNoSuppress ?
						   new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null)) :
						   new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));
				}

				//else an eligible hotkey was found.
			} // Final attempt to find hotkey based on suffix have the right combo of modifiers.

			// Since above didn't return, hotkey_id_with_flags is now a valid hotkey.  The only thing that can
			// stop it from firing now is CriterionFiringIsCertain().
			// v1.0.41: This must be done prior to the setting of sDisguiseNextMenu below.
			hotkeyIdTemp = hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_ID_MASK;

			if (hotkeyIdTemp < shk.Count// i.e. don't call the below for Alt-tab hotkeys and similar.
					&& firingIsCertain == null // i.e. CriterionFiringIsCertain() wasn't already called earlier.
					&& (firingIsCertain = HotkeyDefinition.CriterionFiringIsCertain(ref hotkeyIdWithFlags, keyUp, extraInfo, ref fireWithNoSuppress, ref keyHistoryCurr.eventType)) == null)
			{
				if (keyHistoryCurr.eventType == 'i') // This non-zero SendLevel event is being ignored due to #InputLevel, so unconditionally pass it through, like with is_ignored.
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

				// v1.1.08: Although the hotkey corresponding to this event is disabled, it may need to
				// be suppressed if it has a counterpart (key-down or key-up) hotkey which is enabled.
				// This can be broken down into two cases:
				//  1) This is a key-up event and the key-down event was already suppressed.
				//     Prior to v1.1.08, the key-up was passed through; this caused problems in a
				//     few specific cases, such as XButton1 and XButton2 (which act when released).
				//  2) This is a key-down event, but there is also a key-up hotkey which is enabled.
				//     In that case, the documentation indicates the key-down will be suppressed.
				//     Prior to v1.1.08, neither event was suppressed.
				if (keyUp)
					return thisKey.hotkeyDownWasSuppressed ?
						   new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null)) :
						   new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

				if (thisKey.hotkeyToFireUponRelease == HotkeyDefinition.HOTKEY_ID_INVALID)
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

				char? ch = null;
				// Otherwise, this is a key-down event with a corresponding key-up hotkey.
				fireWithNoSuppress = false; // Reset it for the check below.
				// This check should be identical to the section above dealing with hotkey_to_fire_upon_release:
				firingIsCertain = HotkeyDefinition.CriterionFiringIsCertain(ref thisKey.hotkeyToFireUponRelease // firing_is_certain==false under these conditions, so no need to check it.
								  , true  // Always a key-up since it will fire upon release.
								  , extraInfo // May affect the result due to #InputLevel.  Assume the key-up's SendLevel will be the same as the key-down.
								  , ref fireWithNoSuppress, ref ch); // fire_with_no_suppress is the value we really need to get back from it.

				if (firingIsCertain == null || fireWithNoSuppress)
				{
					thisKey.noSuppress |= HotkeyDefinition.NO_SUPPRESS_NEXT_UP_EVENT;
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));
				}

				// Both this down event and the corresponding up event should be suppressed.
				thisKey.hotkeyDownWasSuppressed = true;
				return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, null));
			}

			hotkeyIdTemp = hotkeyIdWithFlags & HotkeyDefinition.HOTKEY_ID_MASK; // Update in case CriterionFiringIsCertain() changed the naked/raw ID.

			// If prefixKey is part of the reason for this hotkey firing, update was_just_used
			// so that when the prefix key is released, it won't perform its key-up action.
			// To match the behaviour prior to v1.1.37, this is done on key-up for custom combos
			// but not standard hotkeys.  Note that if there are multiple key-up hotkeys with
			// different modifier combinations, the one that fires might depend on the modifier
			// state at the time the key was pressed, rather than when it was released.  In other
			// words, prefixKey may be unrelated to the key-up hotkey if it is a standard modifier.
			if (prefixKey != null && (foundHk != null || (prefixKey.asModifiersLR != 0 && !keyUp))
					&& prefixKey.wasJustUsed != KeyType.AS_PASSTHROUGH_PREFIX)
				prefixKey.wasJustUsed = KeyType.AS_PREFIX_FOR_HOTKEY;

			// Now above has ensured that everything is in place for an action to be performed.
			// Determine the final ID at this late stage to improve maintainability:
			var hotkeyIdToFire = hotkeyIdTemp;

			// Check if the WIN or ALT key needs to be masked:
			if ((kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT | MOD_LWIN | MOD_RWIN)) != 0 // ALT and/or WIN is down.
					&& !fireWithNoSuppress // This hotkey will be suppressed (hotkeys with ~no-suppress should not require masking).
					&& (undisguisedMenuInEffect || hook == mouseHook)) // Menu has not already been disguised (as tracked by the keyboard hook), or this is the mouse hook, which may require masking anyway.
			{
				// If only a windows key was held down to activate this hotkey, suppress the next win-up
				// event so that the Start Menu won't appear.  The appearance of the Start Menu would be
				// caused by the fact that the hotkey's suffix key was suppressed, therefore the OS doesn't
				// see that the WIN key "modified" anything while it was held down.
				// Although having other modifiers present prevents the Start Menu from appearing, that's
				// handled by later checks since the WIN key can auto-repeat, putting an unmodified WIN
				// back into effect after the other mods are released.  This only happens if the WIN key
				// is the most recently pressed physical key, such as if this hotkey is a mouse button.
				// When the user finally releases the WIN key, that release will be disguised if called
				// for by the logic below and in AllowIt().
				if ((kbdMsSender.modifiersLRLogical & (MOD_LWIN | MOD_RWIN)) != 0 // One or both are down and may require disguising.
						&& HotkeyDefinition.HotkeyRequiresModLR(hotkeyIdToFire, MOD_LWIN | MOD_RWIN) != 0) // Avoid masking hotkeys which could be intended to send {LWin up}, such as for AppsKey::RWin.
				{
					disguiseNextMenu = true;
					// An earlier stage has ensured that the keyboard hook is installed for suppression of LWin/RWin if
					// this is a mouse hotkey, because the sending of CTRL directly (here) would otherwise not suppress
					// the Start Menu (though it does supress menu bar activation for ALT hotkeys, as described below).
				}

				// For maximum reliability on the maximum range of systems, it seems best to do the above
				// for ALT keys also, to prevent them from invoking the icon menu or menu bar of the
				// foreground window (rarer than the Start Menu problem, above, I think).
				// Update for v1.0.25: This is usually only necessary for hotkeys whose only modifier is ALT.
				// For example, Shift-Alt hotkeys do not need it if Shift is pressed after Alt because Alt
				// "modified" the shift so the OS knows it's not a naked ALT press to activate the menu bar.
				// Conversely, if Shift is pressed prior to Alt, but released before Alt, I think the shift-up
				// counts as a "modification" and the same rule applies.  However, if shift is released after Alt,
				// that would activate the menu bar unless the ALT key is disguised below.  This issue does
				// not apply to the WIN key above because apparently it is disguised automatically
				// whenever some other modifier was involved with it in any way and at any time during the
				// keystrokes that comprise the hotkey.
				if (!disguiseNextMenu // It's not already going to be disguised due to the section above or a previous hotkey.
						&& (kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT)) != 0// If RAlt==AltGr, it should never need disguising, but in that case LCtrl is also down, so ActiveWindowLayoutHasAltGr() isn't checked.
						&& (kbdMsSender.modifiersLRLogical & (MOD_LCONTROL | MOD_RCONTROL)) != 0 // No need to mask if Ctrl is down (the key-repeat issue that affects the WIN key does not affect ALT).
						&& HotkeyDefinition.HotkeyRequiresModLR(hotkeyIdToFire, MOD_LALT | MOD_RALT) != 0) // Avoid masking hotkeys which could be intended to send {Alt up}, such as for AppsKey::Alt.
				{
					if (HasKbdHook())
						disguiseNextMenu = true;
					else
						// Since no keyboard hook, no point in setting the variable because it would never be acted upon.
						// Instead, disguise the key now with a CTRL keystroke. Note that this is not done for
						// mouse buttons that use the WIN key as a prefix because it does not work reliably for them
						// (i.e. sometimes the Start Menu appears, even if two CTRL keystrokes are sent rather than one).
						// Therefore, as of v1.0.25.05, mouse button hotkeys that use only the WIN key as a modifier cause
						// the keyboard hook to be installed.  This determination is made during the hotkey loading stage.
						kbdMsSender.SendKeyEventMenuMask(KeyEventTypes.KeyDownAndUp);
				}
			}

			//Had to pull this out of the case statement because it's not allowed to fall though.
			if (hotkeyIdToFire == HotkeyDefinition.HOTKEY_ID_ALT_TAB_MENU_DISMISS && !altTabMenuIsVisible)// This case must occur before HOTKEY_ID_ALT_TAB_MENU due to non-break.
				return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null)); // Let the key do its native function.

			switch (hotkeyIdToFire)
			{
				// else fall through to the next case.
				case HotkeyDefinition.HOTKEY_ID_ALT_TAB_MENU:  // These cases must occur before the Alt-tab ones due to conditional break.
				case HotkeyDefinition.HOTKEY_ID_ALT_TAB_AND_MENU:
				{
					var whichAltDown = 0u;

					if ((kbdMsSender.modifiersLRLogical & MOD_LALT) != 0)
						whichAltDown = VK_LMENU;
					else if ((kbdMsSender.modifiersLRLogical & MOD_RALT) != 0)
						whichAltDown = VK_RMENU;

					if (altTabMenuIsVisible)  // Can be true even if which_alt_down is zero.
					{
						if (hotkeyIdToFire != HotkeyDefinition.HOTKEY_ID_ALT_TAB_AND_MENU) // then it is MENU or DISMISS.
						{
							// Since it is possible for the menu to be visible when neither ALT
							// key is down, always send an alt-up event if one isn't down
							// so that the menu is dismissed as intended:
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, whichAltDown != 0 ? whichAltDown : VK_MENU);

							if (thisKey.asModifiersLR != 0 && vk != VK_LWIN && vk != VK_RWIN && !keyUp)
								// Something strange seems to happen with the foreground app
								// thinking the modifier is still down (even though it was suppressed
								// entirely [confirmed!]).  For example, if the script contains
								// the line "lshift::AltTabMenu", pressing lshift twice would
								// otherwise cause the newly-activated app to think the shift
								// key is down.  Sending an extra UP here seems to fix that,
								// hopefully without breaking anything else.  Note: It's not
								// done for Lwin/Rwin because most (all?) apps don't care whether
								// LWin/RWin is down, and sending an up event might risk triggering
								// the start menu in certain hotkey configurations.  This policy
								// might not be the right one for everyone, however:
								kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk); // Can't send sc here since it's not defined for the mouse hook.

							altTabMenuIsVisible = false;
							break;
						}

						// else HOTKEY_ID_ALT_TAB_AND_MENU, do nothing (don't break) because we want
						// the switch to fall through to the Alt-Tab case.
					}
					else // alt-tab menu is not visible
					{
						// Unlike CONTROL, SHIFT, AND ALT, the LWIN/RWIN keys don't seem to need any
						// special handling to make them work with the alt-tab features.
						var vkIsAlt = vk == VK_LMENU || vk == VK_RMENU;  // Translated & no longer needed: || vk == VK_MENU;
						var vkIsShift = vk == VK_LSHIFT || vk == VK_RSHIFT;  // || vk == VK_SHIFT;
						var vkIsControl = vk == VK_LCONTROL || vk == VK_RCONTROL;  // || vk == VK_CONTROL;
						var whichShiftDown = 0u;

						if ((kbdMsSender.modifiersLRLogical & MOD_LSHIFT) != 0)
							whichShiftDown = VK_LSHIFT;
						else if ((kbdMsSender.modifiersLRLogical & MOD_RSHIFT) != 0)
							whichShiftDown = VK_RSHIFT;
						else if (!keyUp && vkIsShift)
							whichShiftDown = vk;

						var whichControlDown = 0u;

						if ((kbdMsSender.modifiersLRLogical & MOD_LCONTROL) != 0)
							whichControlDown = VK_LCONTROL;
						else if ((kbdMsSender.modifiersLRLogical & MOD_RCONTROL) != 0)
							whichControlDown = VK_RCONTROL;
						else if (!keyUp && vkIsControl)
							whichControlDown = vk;

						var shift_put_up = false;

						if (whichShiftDown != 0)
						{
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, whichShiftDown);
							shift_put_up = true;
						}

						if (whichControlDown != 0)
						{
							// In this case, the control key must be put up because the OS, at least
							// WinXP, knows the control key is down even though the down event was
							// suppressed by the hook.  So put it up and leave it up, because putting
							// it back down would cause it to be down even after the user releases
							// it (since the up-event of a hotkey is also suppressed):
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, whichControlDown);
						}

						// Alt-tab menu is not visible, or was not made visible by us.  In either case,
						// try to make sure it's displayed:
						// Don't put alt down if it's already down, it might mess up cases where the
						// ALT key itself is assigned to be one of the alt-tab actions:

						if (vkIsAlt)
						{
							if (keyUp)
								// The system won't see it as down for the purpose of alt-tab, so remove this
								// modifier from consideration.  This is necessary to allow something like this
								// to work:
								// LAlt & WheelDown::AltTab
								// LAlt::AltTabMenu   ; Since LAlt is a prefix key above, it will be a key-up hotkey here.
								whichAltDown = 0;
							else // Because there hasn't been a chance to update kbdMsSender.modifiersLR_logical yet:
								whichAltDown = vk;
						}

						if (whichAltDown == 0)
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, VK_MENU);

						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDownAndUp, VK_TAB); // v1.0.28: KEYDOWNANDUP vs. KEYDOWN.

						// Only put it put it back down if it wasn't the hotkey itself, because
						// the system would never have known it was down because the down-event
						// on the hotkey would have been suppressed.  And since the up-event
						// will also be suppressed, putting it down like this would result in
						// it being permanently down even after the user releases the key!:
						if (shift_put_up && !vkIsShift) // Must do this regardless of the value of aKeyUp.
							kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, whichShiftDown);

						// Update: Can't do this one because going down on control will instantly
						// dismiss the alt-tab menu, which we don't want if we're here.
						//if (control_put_up && !vk_is_control) // Must do this regardless of the value of aKeyUp.
						//  KeyEvent(KEYDOWN, which_control_down);
						// At this point, the alt-tab menu has displayed and advanced by one icon
						// (to the next window in the z-order).  Rather than sending a shift-tab to
						// go back to the first icon in the menu, it seems best to leave it where
						// it is because usually the user will want to go forward at least one item.
						// Going backward through the menu is a lot more rare for most people.
						altTabMenuIsVisible = true;
						break;
					}
				}
				break;

				case HotkeyDefinition.HOTKEY_ID_ALT_TAB:
				case HotkeyDefinition.HOTKEY_ID_ALT_TAB_SHIFT:
				{
					// Since we're here, this ALT-TAB hotkey didn't have a prefix or it would have
					// already been handled and we would have returned above.  Therefore, this
					// hotkey is defined as taking effect only if the alt-tab menu is currently
					// displayed, otherwise it will just be passed through to perform it's native
					// function.  Example:
					// MButton::AltTabMenu
					// WheelDown::AltTab     ; But if the menu is displayed, the wheel will function normally.
					// WheelUp::ShiftAltTab  ; But if the menu is displayed, the wheel will function normally.
					if (!altTabMenuIsVisible)
						return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, null));

					// Unlike CONTROL, SHIFT, AND ALT, the LWIN/RWIN keys don't seem to need any
					// special handling to make them work with the alt-tab features.

					// Must do this to prevent interference with Alt-tab when these keys
					// are used to do the navigation.  Don't put any of these back down
					// after putting them up since that would probably cause them to become
					// stuck down due to the fact that the user's physical release of the
					// key will be suppressed (since it's a hotkey):
					if (!keyUp && (vk == VK_LCONTROL || vk == VK_RCONTROL || vk == VK_LSHIFT || vk == VK_RSHIFT))
						// Don't do the ALT key because it causes more problems than it solves
						// (in fact, it might not solve any at all).
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk); // Can't send sc here since it's not defined for the mouse hook.

					// Even when the menu is visible, it's possible that neither of the ALT keys
					// is down (such as if Ctrl+Alt+Tab was used, and perhaps other cases):
					if ((kbdMsSender.modifiersLRLogical & (MOD_LALT | MOD_RALT)) == 0// Neither ALT key is down
							|| (keyUp && (vk == VK_LMENU || vk == VK_RMENU))) // Or the suffix key for Alt-tab *is* an ALT key and it's being released: must push ALT down for upcoming TAB to work.
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, VK_MENU);

					// And never put it back up because that would dismiss the menu.
					// Otherwise, use keystrokes to navigate through the menu:
					var shiftPutDown = false;

					if (hotkeyIdToFire == HotkeyDefinition.HOTKEY_ID_ALT_TAB_SHIFT && (kbdMsSender.modifiersLRLogical & (MOD_LSHIFT | MOD_RSHIFT)) == 0) // Neither SHIFT key is down.
					{
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, VK_SHIFT);
						shiftPutDown = true;
					}

					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDownAndUp, VK_TAB);

					if (shiftPutDown)
						kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, VK_SHIFT);

					break;
				}

				default:
					// Notify the main thread (via its main window) of which hotkey has been pressed.
					// Post the message rather than sending it, because Send would need
					// SendMessageTimeout(), which is undesirable because the whole point of
					// making this hook thread separate from the main thread is to have it be
					// maximally responsive (especially to prevent mouse cursor lag).
					// v1.0.42: The hotkey variant is not passed via the message below because
					// upon receipt of the message, the variant is recalculated in case conditions
					// have changed between msg-post and arrival.  See comments in the message loop for details.
					// v1.0.42.01: the message is now posted at the latest possible moment to avoid
					// situations in which the message arrives and is processed by the main thread
					// before we finish processing the hotkey's final keystroke here.  This avoids
					// problems with a script calling GetKeyState() and getting an inaccurate value
					// because the hook thread is either pre-empted or is running in parallel
					// (multiprocessor) and hasn't yet returned 1 or 0 to determine whether the final
					// keystroke is suppressed or passed through to the active window.  Similarly, this solves
					// the fact that previously, g_PhysicalKeyState was not updated for modifier keys until after
					// the hotkey message was posted, which on some PCs caused the hotkey subroutine to see
					// the wrong key state via KeyWait (which defaults to detecting the physical key state).
					// For example, the following hotkeys would be a problem on certain PCs, presumably due to
					// split-second timing where the hook thread gets preempted and the main thread gets a
					// timeslice that allows it to launch a script subroutine before the hook can get
					// another timeslice to finish up:
					//$LAlt::
					//if not GetKeyState("LAlt", "P")
					//  ToolTip `nProblem 1`n
					//return
					//
					//~LControl::
					//if not (DllCall("GetAsyncKeyState", int, 0xA2) & 0x8000)
					//    ToolTip `nProblem 2`n
					//return
					hotkeyIdToPost = hotkeyIdToFire; // Set this only when it is certain that this ID should be sent to the main thread via msg.

					if (firingIsCertain.hotCriterion != null)
					{
						// To avoid evaluating the expression twice, indicate to the main thread that the appropriate variant
						// has already been determined, by packing the variant's index into the high word of the param:
						//hotkeyIdToPost |= (uint)(firingIsCertain.index << 16);
					}
					else
						firingIsCertain = null;

					// Otherwise CriterionFiringIsCertain() might have returned a global variant (not necessarily the one
					// that will actually fire), so if we ever decide to do the above for other criterion types rather than
					// just re-evaluating the criterion later, must make sure not to send the mIndex of a global variant.
					//if (firing_is_certain.mHotCriterion) // i.e. a specific variant has already been determined.
					break;
			}

			keyHistoryCurr.eventType = 'h'; // h = hook hotkey (not one registered with RegisterHotkey)

			if (thisToggleKeyCanBeToggled && keyUp && thisKey.usedAsPrefix != 0)
			{
				// In this case, since all the above conditions are true, the key-down
				// event for this key-up (which fired a hotkey) would not have been
				// suppressed.  Thus, we should toggle the state of the key back
				// the what it was before the user pressed it (due to the policy that
				// the natural function of a key should never take effect when that
				// key is used as a hotkey suffix).  You could argue that instead
				// of doing this, we should change *pForceToggle's value to make the
				// key untoggleable whenever it's both a prefix and a naked
				// (key-up triggered) suffix.  However, this isn't too much harder
				// and has the added benefit of allowing the key to be toggled if
				// a modifier is held down before it (e.g. alt-CapsLock would then
				// be able to toggle the CapsLock key):
				kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk, sc, IntPtr.Zero, false, KeyPhysIgnore);// Mark it as physical for any other hook instances.
				kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDownAndUp, vk, sc);
				return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, firingIsCertain));
			}

			if (keyUp)
			{
				if (thisKey.asModifiersLR != 0)
					// Since this hotkey is fired on a key-up event, and since it's a modifier, must
					// not suppress the key because otherwise the system's state for this modifier
					// key would be stuck down due to the fact that the previous down-event for this
					// key (which is presumably a prefix *and* a suffix) was not suppressed. UPDATE:
					// For v1.0.28, if the new field hotkey_down_was_suppressed is true, also suppress
					// this up event, one purpose of which is to allow a pair of remappings such
					// as the following to display the Start Menu (because otherwise the non-suppressed
					// Alt key events would prevent it):
					// *LAlt up::Send {LWin up}
					// *LAlt::Send {LWin down}
					return thisKey.hotkeyDownWasSuppressed ?
						   new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, firingIsCertain)) :
						   new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, firingIsCertain));

				if (fireWithNoSuppress) // Plus we know it's not a modifier since otherwise it would've returned above.
				{
					// Although it seems more sensible to suppress the key-up if the key-down was suppressed,
					// it probably does no harm to let the key-up pass through, and in this case, it's exactly
					// what the script is asking to happen (by prefixing the key-up hotkey with '~').
					// this_key.pForceToggle isn't checked because AllowIt() handles that.
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, firingIsCertain));
				} // No suppression.
			}
			else // Key Down
			{
				// Do this only for DOWN (not UP) events that triggered an action:
				thisKey.downPerformedAction = true;

				if (fireWithNoSuppress)
				{
					// Since this hotkey is firing on key-down but the user specified not to suppress its native
					// function, substitute an DOWN+UP pair of events for this event, since we want the
					// DOWN to precede the UP.  It's necessary to send the UP because the user's physical UP
					// will be suppressed automatically when this function is called for that event.
					// UPDATE: The below method causes side-effects due to the fact that it is simulated
					// input vs. physical input, e.g. when used with the Input command, which distinguishes
					// between "ignored" and physical input.  Therefore, let this down event pass through
					// and set things up so that the corresponding up-event is also not suppressed:
					//KeyEvent(KEYDOWNANDUP, aVK, aSC);
					// No longer relevant due to the above change:
					// Now let it just fall through to suppress this down event, because we can't use it
					// since doing so would result in the UP event having preceded the DOWN, which would
					// be the wrong order.
					thisKey.noSuppress |= HotkeyDefinition.NO_SUPPRESS_NEXT_UP_EVENT;
					return new IntPtr(AllowIt(hook, code, wParam, ref kbd, ref mouse, vk, sc, keyUp, extraInfo, collectInputState, keyHistoryCurr, hotkeyIdToPost, firingIsCertain));
				}
				// Fix for v1.1.37.02 and v2.0.6: The following is also done for LWin/RWin because otherwise,
				// the system does not generate WM_SYSKEYDOWN (or even WM_KEYDOWN) messages for combinations
				// that correspond to some global hotkeys, even though they aren't actually triggering global
				// hotkeys because the logical key state doesn't match.  For example, with LWin::Alt, LWin-T
				// would not activate the Tools menu on a menu bar.
				// Fixes for v1.1.37.02 and v2.0.8:
				//  1) Apply this to Ctrl hotkeys because otherwise, the OS thinks Ctrl is being held down
				//     and therefore translates Alt-key combinations to WM_KEYDOWN instead of WM_SYSKEYDOWN.
				//     (confirmed on Windows 7, but might not be necessary on Windows 11).
				//  2) Apply this to Shift as well for simplicity and consistency.  Although this hasn't been
				//     confirmed, it might be necessary for correct system handling in some cases, such as with
				//     certain language-switching hotkeys, IME or advanced keyboard layouts.
				//  3) Don't apply this if the modifier is logically down, since in that case the system *should*
				//     consider the key to be held down.  For example, pressing Ctrl+Alt should produce WM_KEYDOWN,
				//     but if the system thinks Ctrl has been released, it will instead produce WM_SYSKEYDOWN.
				//     This was confirmed necessary for LCtrl::Alt and LAlt::LCtrl to work correctly on Windows 7.
				else if ((thisKey.asModifiersLR & ~kbdMsSender.modifiersLRLogical) != 0)
				{
					// Fix for v1.1.26.01: Added KEY_BLOCK_THIS to suppress the Alt key-up, which fixes an issue
					// which could be reproduced as follows:
					//  - Test with an Alt-blocking hotkey such as LAlt::return or LAlt::LCtrl.
					//  - Open Notepad and alt-tab away using the other Alt key or a remapping such as LCtrl::LAlt.
					//  - Reactivate Notepad and note that the keyboard accelerators (underlined letters) are still
					//    visible in the menus (usually).
					//  - Press LAlt and the menus are activated once, even though LAlt is supposed to be blocked.
					// Additionally, a Windows 10 check was added because the original issue this workaround was
					// intended for doesn't appear to occur on Windows 10 (tested on 10.0.15063).  This check was
					// removed for v1.1.27.00 to ensure consistent behavior of AltGr hotkeys across OS versions.
					// (Sending RAlt up on a layout with AltGr causes the system to send LCtrl up.)
					// Testing on XP, Vista and 8.1 showed that the #LAlt issue below only occurred if the key-up
					// was allowed to pass through to the active window.  It appeared to be a non-issue on Win 10
					// even when the Alt key-up was passed through.
					// Fix for v1.0.34: For some reason, the release of the ALT key here causes the Start Menu
					// to appear instantly for the hotkey #LAlt (and probably #RAlt), even when the hotkey does
					// nothing other than return.  This seems like an OS quirk since it doesn't conform to any
					// known Start Menu activation sequence.  This happens only when neither Shift nor Control is
					// down.  To work around it, send the menu-suppressing Control keystroke here.  Another one
					// will probably be sent later when the WIN key is physically released, but it seems best
					// for simplicity and avoidance of side-effects not to make this one prevent that one.
					//if (   (kbdMsSender.modifiersLR_logical & (MOD_LWIN | MOD_RWIN))   // At least one WIN key is down.
					//  && !(kbdMsSender.modifiersLR_logical & (MOD_LSHIFT | MOD_RSHIFT | MOD_LCONTROL | MOD_RCONTROL))   ) // But no SHIFT or CONTROL key is down to help us.
					//  KeyEventMenuMask(KEYDOWNANDUP);
					// Since this is a hotkey that fires on ALT-DOWN and it's a normal (suppressed) hotkey,
					// send an up-event to "turn off" the OS's low-level handling for the alt key with
					// respect to having it modify keypresses.  For example, the following hotkeys would
					// fail to work properly without this workaround because the OS apparently sees that
					// the ALT key is physically down even though it is not logically down:
					// RAlt::Send f  ; Actually triggers !f, which activates the FILE menu if the active window has one.
					// RAlt::Send {PgDn}  ; Fails to work because ALT-PgDn usually does nothing.
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, vk, sc, IntPtr.Zero, false, KeyBlockThis);
				}
			}

			// Otherwise:
			if (!keyUp)
				thisKey.hotkeyDownWasSuppressed = true;

			return new IntPtr(SuppressThisKeyFunc(hook, ref kbd, vk, sc, keyUp, extraInfo, keyHistoryCurr, hotkeyIdToPost, firingIsCertain));
		}

		internal IntPtr LowLevelMouseHandler(int code, IntPtr param, ref MSDLLHOOKSTRUCT lParam)
		{
			// code != HC_ACTION should be evaluated PRIOR to considering the values
			// of wParam and lParam, because those values may be invalid or untrustworthy
			// whenever code < 0.
			if (code != HC_ACTION)
				return CallNextHookEx(mouseHook, code, param, ref lParam);

			// Make all mouse events physical to try to simulate mouse clicks in games that normally ignore
			// artificial input.
			//event.flags &= ~LLMHF_INJECTED;

			if ((lParam.flags & LLMHF_INJECTED) == 0) // Physical mouse movement or button action (uses LLMHF vs. LLKHF).
				script.timeLastInputPhysical = script.timeLastInputMouse = DateTime.UtcNow;

			// Above: Don't use event.time, mostly because SendInput can produce invalid timestamps on such events
			// (though in truth, that concern isn't valid because SendInput's input isn't marked as physical).
			// Another concern is the comments at the other update of "g_TimeLastInputPhysical" elsewhere in this file.
			// A final concern is that some drivers might be faulty and might not generate an accurate timestamp.
			var iwParam = param.ToInt32();

			if (iwParam == WM_MOUSEMOVE) // Only after updating for physical input, above, is this checked.
				return (script.KeyboardData.blockMouseMove && ((lParam.flags & LLMHF_INJECTED) == 0)) ? new IntPtr(1) : CallNextHookEx(mouseHook, code, param, ref lParam);

			// Above: In v1.0.43.11, a new mode was added to block mouse movement only since it's more flexible than
			// BlockInput (which keybd too, and blocks all mouse buttons too).  However, this mode blocks only
			// physical mouse movement because it seems most flexible (and simplest) to allow all artificial
			// movement, even if that movement came from a source other than an AHK script (such as some other
			// macro program).
			// MSDN: WM_LBUTTONDOWN, WM_LBUTTONUP, WM_MOUSEMOVE, WM_MOUSEWHEEL [, WM_MOUSEHWHEEL], WM_RBUTTONDOWN, or WM_RBUTTONUP.
			// But what about the middle button?  It's undocumented, but it is received.
			// What about doubleclicks (e.g. WM_LBUTTONDBLCLK): I checked: They are NOT received.
			// This is expected because each click in a doubleclick could be separately suppressed by
			// the hook, which would make it become a non-doubleclick.
			var vk = 0u;
			var sc = 0u; // To be overridden if this even is a wheel turn.
			var keyUp = true;  // Set default to safest value.

			switch (iwParam)
			{
				case WM_MOUSEWHEEL:
				case WM_MOUSEHWHEEL:
				{
					// v1.0.48: Lexikos: Support horizontal scrolling in Windows Vista and later.
					// MSDN: "A positive value indicates that the wheel was rotated forward, away from the user;
					// a negative value indicates that the wheel was rotated backward, toward the user. One wheel
					// click is defined as WHEEL_DELTA, which is 120."  Testing shows that on XP at least, the
					// abs(delta) is greater than 120 when the user turns the wheel quickly (also depends on
					// granularity of wheel hardware); i.e. the system combines multiple turns into a single event.
					var wheelDelta = Conversions.HighWord(lParam.mouseData);

					if (iwParam == WM_MOUSEWHEEL)
						vk = wheelDelta < 0 ? VK_WHEEL_DOWN : VK_WHEEL_UP;
					else
						vk = wheelDelta < 0 ? VK_WHEEL_LEFT : VK_WHEEL_RIGHT;

					sc = (uint)wheelDelta;
					keyUp = false; // Always consider wheel movements to be "key down" events.
				}
				break;

				case WM_LBUTTONUP:
					vk = VK_LBUTTON; break;

				case WM_RBUTTONUP:
					vk = VK_RBUTTON; break;

				case WM_MBUTTONUP:
					vk = VK_MBUTTON; break;

				case WM_NCXBUTTONUP:  // NC means non-client.
				case WM_XBUTTONUP:
					vk = Conversions.HighWord(lParam.mouseData) == XBUTTON1 ? VK_XBUTTON1 : VK_XBUTTON2; break;

				case WM_LBUTTONDOWN:
					vk = VK_LBUTTON; keyUp = false; break;

				case WM_RBUTTONDOWN:
					vk = VK_RBUTTON; keyUp = false; break;

				case WM_MBUTTONDOWN:
					vk = VK_MBUTTON; keyUp = false; break;

				case WM_NCXBUTTONDOWN:
				case WM_XBUTTONDOWN:
					vk = (Conversions.HighWord(lParam.mouseData) == XBUTTON1) ? VK_XBUTTON1 : VK_XBUTTON2; keyUp = false; break;
			}

			KBDLLHOOKSTRUCT tempstruct = default;
			return LowLevelCommon(mouseHook, code, iwParam, ref tempstruct, ref lParam, vk, sc, keyUp, lParam.dwExtraInfo.ToUInt64(), lParam.flags);
		}

		internal override uint MapScToVk(uint sc)
		{
			// aSC is actually a combination of the last byte of the keyboard make code combined with
			// 0x100 for the extended-key flag.  Although in most cases the flag corresponds to a prefix
			// byte of 0xE0, it seems it's actually set by the KBDEXT flag in the keyboard layout dll
			// (it's hard to find documentation).  A few keys have the KBDEXT flag inverted, which means
			// we can't tell reliably which scan codes really need the 0xE0 prefix, so just handle them
			// as special cases and hope that the flag never varies between layouts.
			// If this approach ever fails for custom layouts, some alternatives are:
			//  - Load the keyboard layout dll manually and check the scan code conversion tables for
			//    the presence of the KBDEXT flag.
			//  - Convert aSC and (aSC ^ 0x100), check the conversion of VK back to SC, and if it
			//    round-trips use that VK instead.
			// However, it seems that neither MSKLC nor KbdEdit provide a means to change the KBDEXT flag.
			// US layout: https://github.com/microsoft/Windows-driver-samples/blob/master/input/layout/kbdus/kbdus.c
			// Keyboard make codes: http://stanislavs.org/helppc/make_codes.html
			// More low-level keyboard details: https://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html#ss1.5
			switch (sc)
			{
				// RShift doesn't have the 0xE0 prefix but has KBDEXT.  The US layout sample says
				// "Right-hand Shift key must have KBDEXT bit set", so it's probably always set.
				// KbdEdit seems to follow this rule when VK_RSHIFT is assigned to a non-ext key.
				// It's definitely possible to assign RShift a different VK, but 1) it can't be
				// done with MSKLC, and 2) KbdEdit clears the ext flag (so aSC != SC_RSHIFT).
				case RShift:

				// NumLock doesn't have the 0xE0 prefix but has KBDEXT.  Actually pressing the key
				// will produce VK_PAUSE if CTRL is down, but with SC_NUMLOCK rather than SC_PAUSE.
				case NumpadLock:
					// These cases can be handled by adjusting aSC to reflect the fact that these
					// keys don't really have the 0xE0 prefix, and allowing MapVirtualKey() to be
					// called below in case they have been remapped.
					sc &= 0xFF;
					break;

				// Pause actually generates 0xE1,0x1D,0x45, or in other words, E1,LCtrl,NumLock.
				// kbd.h says "We must convert the E1+LCtrl to BREAK, then ignore the Numlock".
				// So 0xE11D maps to and from VK_PAUSE, and 0x45 is "ignored".  However, the hook
				// receives only 0x45, not 0xE11D (which I guess would be truncated to 0x1D/ctrl).
				// The documentation for KbdEdit also indicates the mapping of Pause is "hard-wired":
				// http://www.kbdedit.com/manual/low_level_edit_vk_mappings.html
				case Pause:
					return VK_PAUSE;
			}

			if ((sc & 0x100) != 0) // Our extended-key flag.
			{
				// Since it wasn't handled above, assume the extended-key flag corresponds to the 0xE0
				// prefix byte.  Passing 0xE000 should work on Vista and up, though it appears to be
				// documented only for MapVirtualKeyEx() as at 2019-10-26.  Details can be found in
				// archives of Michael Kaplan's blog (the original blog has been taken down):
				// https://web.archive.org/web/20070219075710/http://blogs.msdn.com/michkap/archive/2006/08/29/729476.aspx
				sc = 0xE000 | (sc & 0xFF);
			}

			return MapVirtualKey(sc, MAPVK_VSC_TO_VK_EX);
		}

		/// <summary>
		/// If caller passes true for aReturnSecondary, the "extended" scan code will be returned for
		/// virtual keys that have two scan codes and two names (if there's only one, callers rely on
		/// zero being returned).  In those cases, the caller may want to know:
		///  a) Whether the hook needs to be used to identify a hotkey defined by name.
		///  b) Whether InputHook should handle the keys by SC in order to differentiate.
		///  c) Whether to retrieve the key's name by SC rather than VK.
		/// In all of those cases, only keys that we've given multiple names matter.
		/// Custom layouts could assign some other VK to multiple SCs, but there would
		/// be no reason (or way) to differentiate them in this context.
		/// </summary>
		/// <param name="vk"></param>
		/// <param name="returnSecondary"></param>
		/// <returns></returns>
		internal override uint MapVkToSc(uint vk, bool returnSecondary = false)
		{
			// Try to minimize the number mappings done manually because MapVirtualKey is a more reliable
			// way to get the mapping if user has non-standard or custom keyboard layout.
			var sc = 0u;

			switch (vk)
			{
				// MapVirtualKey() returns 0xE11D, but we want the code normally received by the
				// hook (sc045).  See sc_to_vk() for more comments.
				case VK_PAUSE: sc = Pause; break;

				// PrintScreen: MapVirtualKey() returns 0x54, which is SysReq (produced by pressing
				// Alt+PrintScreen, but still maps to VK_SNAPSHOT).  Use sc137 for consistency with
				// what the hook reports for the naked keypress (and therefore what a hotkey is
				// likely to need).
				case VK_SNAPSHOT: sc = PrintScreen; break;

				// See comments in sc_to_vk().
				case VK_NUMLOCK: sc = NumpadLock; break;
			}

			if (sc != 0) // Above found a match.
				return returnSecondary ? 0 : sc; // Callers rely on zero being returned for VKs that don't have secondary SCs.

			if ((sc = MapVirtualKey(vk, MAPVK_VK_TO_VSC_EX)) == 0u)
				return 0; // Indicate "no mapping".

			if ((sc & 0xE000) != 0u) // Prefix byte E0 or E1 (but E1 should only be possible for Pause/Break, which was already handled above).
				sc = 0x0100 | (sc & 0xFF);

			switch (vk)
			{
				// The following virtual keys have more than one physical key, and thus more than one scan code.
				case VK_RETURN:
				case VK_INSERT:
				case VK_DELETE:
				case VK_PRIOR: // PgUp
				case VK_NEXT:  // PgDn
				case VK_HOME:
				case VK_END:
				case VK_UP:
				case VK_DOWN:
				case VK_LEFT:
				case VK_RIGHT:
					// This is likely to be incorrect for custom layouts where aVK is mapped to two SCs
					// that differ in the low byte.  There seems to be no simple way to fix that;
					// the complex ways would be:
					//  - Build our own conversion table by mapping all SCs to VKs (taking care to detect
					//    changes to the current keyboard layout).  Find the second SC that maps to aVK,
					//    or the first one with the 0xE000 flag.  However, there's no guarantee that it
					//    would correspond to NumpadEnter vs. Enter, or Insert vs. NumpadIns, for example.
					//  - Load the keyboard layout dll manually and search the SC-to-VK conversion tables.
					//    What we actually want is to differentiate Numpad keys from their non-Numpad
					//    counter-parts, and we can do that by checking for the KBDNUMPAD flag.
					// Custom layouts might cause these issues:
					//  - If the scan code of the secondary key is changed, the Hotkey control (and
					//    other sections that don't call this function) may return either "scXXX"
					//    or a name inconsistent with the key's current VK (but if it's a custom
					//    layout + standard keyboard, it should match the key's original function).
					//  - The Hotkey control assumes that the HOTKEYF_EXT flag corresponds to the
					//    secondary key, but either/both/neither could be extended on a custom layout.
					//    If it's both/neither, the control would give no way to distinguish.
					return returnSecondary ? sc | 0x0100 : sc; // Below relies on the fact that these cases return early.

				// See "case SC_RSHIFT:" in sc_to_vk() for comments.
				case VK_RSHIFT:
					sc |= 0x0100;
					break;
			}

			// Since above didn't return, if aReturnSecondary==true, return 0 to indicate "no secondary SC for this VK".
			return returnSecondary ? 0 : sc; // Callers rely on zero being returned for VKs that don't have secondary SCs.
		}

		internal override void ParseClickOptions(ReadOnlySpan<char> options, ref int x, ref int y, ref uint vk, ref KeyEventTypes eventType, ref long repeatCount, ref bool moveOffset)
		{
			// Set defaults for all output parameters for caller.
			x = CoordUnspecified;
			y = CoordUnspecified;
			vk = VK_LBUTTON;
			eventType = KeyEventTypes.KeyDownAndUp;
			repeatCount = 1L;
			moveOffset = false;
			uint temp_vk;

			foreach (Range r in options.SplitAny(SpaceTabComma))
			{
				var opt = options[r].Trim();

				if (opt.Length > 0)
				{
					// Parameters can occur in almost any order to enhance usability (at the cost of
					// slightly diminishing the ability to unambiguously add more parameters in the future).
					// Seems okay to support floats because ATOI() will just omit the decimal portion.
					if (double.TryParse(opt, NumberStyles.Float, Parser.inv, out var d))
					{
						var val = (int)d;

						// Any numbers present must appear in the order: X, Y, RepeatCount
						// (optionally with other options between them).
						if (x == CoordUnspecified) // This will be converted into repeat-count if it is later discovered there's no Y coordinate.
							x = val;
						else if (y == CoordUnspecified)
							y = val;
						else // Third number is the repeat-count (but if there's only one number total, that's repeat count too, see further below).
							repeatCount = val;
					}
					else // Mouse button/name and/or Down/Up/Repeat-count is present.
					{
						if ((temp_vk = ConvertMouseButton(opt, true)) != 0)
						{
							vk = temp_vk;
						}
						else
						{
							switch (char.ToUpper(opt[0]))
							{
								case 'D': eventType = KeyEventTypes.KeyDown; break;

								case 'U': eventType = KeyEventTypes.KeyUp; break;

								case 'R': moveOffset = true; break; // Since it wasn't recognized as the right mouse button, it must have other letters after it, e.g. Rel/Relative.
									// default: Ignore anything else to reserve them for future use.
							}
						}
					}
				}
			}

			if (x != CoordUnspecified && y == CoordUnspecified)
			{
				// When only one number is present (e.g. {Click 2}, it's assumed to be the repeat count.
				repeatCount = x;
				x = CoordUnspecified;
			}
			else if (x == CoordUnspecified && y == CoordUnspecified)//Neither was specified, so just use the cursor position.
			{
				var pos = Cursor.Position;
				x = pos.X;
				y = pos.Y;
			}
		}

		internal void ResetHook(bool allModifiersUp = false, HookType whichHook = HookType.Keyboard | HookType.Mouse, bool resetKVKandKSC = false)
		// Caller should ensure that aWhichHook indicates at least one of the hooks (not none).
		{
			if (prefixKey != null)
			{
				// Reset pPrefixKey only if the corresponding hook is being reset.  This fixes
				// custom combo mouse hotkeys breaking when the prefix key does something which
				// causes the keyboard hook to be reset, or vice versa.
				bool isMouseKey = IsMouseVK(prefixKey.Pos);

				if ((whichHook & (isMouseKey ? HookType.Mouse : HookType.Keyboard)) != 0)
					prefixKey = null;
			}

			if (((uint)whichHook & HookMouse) != 0)
			{
				// Initialize some things, a very limited subset of what is initialized when the
				// keyboard hook is installed (see its comments).  This is might not everything
				// we should initialize, so further study is justified in the future:
				// FUTURE_USE_MOUSE_BUTTONS_LOGICAL
				//              g_mouse_buttons_logical = 0;
				//#endif
				physicalKeyState[VK_LBUTTON] = 0;
				physicalKeyState[VK_RBUTTON] = 0;
				physicalKeyState[VK_MBUTTON] = 0;
				physicalKeyState[VK_XBUTTON1] = 0;
				physicalKeyState[VK_XBUTTON2] = 0;
				// These are not really valid, since they can't be in a physically down state, but it's
				// probably better to have a false value in them:
				physicalKeyState[VK_WHEEL_DOWN] = 0;
				physicalKeyState[VK_WHEEL_UP] = 0;
				// Lexikos: Support horizontal scrolling in Windows Vista and later.
				physicalKeyState[VK_WHEEL_LEFT] = 0;
				physicalKeyState[VK_WHEEL_RIGHT] = 0;

				if (resetKVKandKSC)
				{
					kvk[VK_LBUTTON].ResetKeyTypeState();
					kvk[VK_RBUTTON].ResetKeyTypeState();
					kvk[VK_MBUTTON].ResetKeyTypeState();
					kvk[VK_XBUTTON1].ResetKeyTypeState();
					kvk[VK_XBUTTON2].ResetKeyTypeState();
					kvk[VK_WHEEL_DOWN].ResetKeyTypeState();
					kvk[VK_WHEEL_UP].ResetKeyTypeState();
					// Lexikos: Support horizontal scrolling in Windows Vista and later.
					kvk[VK_WHEEL_LEFT].ResetKeyTypeState();
					kvk[VK_WHEEL_RIGHT].ResetKeyTypeState();
				}
			}

			if (((uint)whichHook & HookKeyboard) != 0)
			{
				// Doesn't seem necessary to ever init g_KeyHistory or g_KeyHistoryNext here, since they were
				// zero-filled on startup.  But we do want to reset the below whenever the hook is being
				// installed after a (probably long) period during which it wasn't installed.  This is
				// because we don't know the current physical state of the keyboard and such:
				kbdMsSender.modifiersLRPhysical = 0;  // Best to make this zero, otherwise keys might get stuck down after a Send.
				kbdMsSender.modifiersLRNumpadMask = 0;
				kbdMsSender.modifiersLRCtrlAltDelMask = 0;
				kbdMsSender.modifiersLRLogical = kbdMsSender.modifiersLRLogicalNonIgnored = (allModifiersUp ? 0 : kbdMsSender.GetModifierLRState(true));
				System.Array.Clear(physicalKeyState, 0, physicalKeyState.Length);
				disguiseNextMenu = false;
				undisguisedMenuInEffect = false;
				// On Windows Vista and later, this definitely only works if the classic alt-tab menu
				// has been restored via the registry.  A non-NULL result is probably only helpful for
				// enabling the Esc key workaround in the hook (even though that isn't as critical on
				// Windows 7 as it was on XP, since on 7 the user can dismiss it with physical Esc).
				// A NULL result is probably more common, such as if it's been a while since the hook
				// was removed (or Alt was released).  If the *classic* alt-tab menu isn't in use,
				// this at least serves to reset altTabMenuIsVisible to false:
				altTabMenuIsVisible = FindWindow("#32771", null) != IntPtr.Zero;
				pendingDeadKeys.Clear();
				pendingDeadKeyInvisible = false;
				script.HotstringManager.ClearBuf();
				hsHwnd = IntPtr.Zero; // It isn't necessary to determine the actual window/control at this point since the buffer is already empty.

				if (resetKVKandKSC)
				{
					for (var i = 0u; i < kvk.Length; ++i)
						if (!IsMouseVK(i))  // Don't do mouse VKs since those must be handled by the mouse section.
							kvk[i].ResetKeyTypeState();

					for (var i = 0; i < ksc.Length; ++i)
						ksc[i].ResetKeyTypeState();
				}
			}
		}

		internal void SetModifierAsPrefix(uint vk, uint sc, bool alwaysSetAsPrefix = false)
		// The caller has already ensured that vk and/or sc is a modifier such as VK_CONTROL.
		{
			if (vk != 0)
			{
				switch (vk)
				{
					case VK_MENU:
					case VK_SHIFT:
					case VK_CONTROL:

						// Since the user is configuring both the left and right counterparts of a key to perform a suffix action,
						// it seems best to always consider those keys to be prefixes so that their suffix action will only fire
						// when the key is released.  That way, those keys can still be used as normal modifiers.
						// UPDATE for v1.0.29: But don't do it if there is a corresponding key-up hotkey for this neutral
						// modifier, which allows a remap such as the following to succeed:
						// Control::Send {LWin down}
						// Control up::Send {LWin up}
						if (!alwaysSetAsPrefix)
						{
							var shk = script.HotkeyData.shk;

							for (var i = 0; i < shk.Count; ++i)
							{
								var h = shk[i];

								if (h.vk == vk && h.keyUp && h.modifiersConsolidatedLR == 0 && h.modifierVK == 0 && h.modifierSC == 0 && !h.IsCompletelyDisabled())
									return; // Since caller didn't specify aAlwaysSetAsPrefix==true, don't make this key a prefix.
							}
						}

						switch (vk)
						{
							case VK_MENU:
								kvk[VK_MENU].usedAsPrefix = KeyType.PREFIX_FORCED;
								kvk[VK_LMENU].usedAsPrefix = KeyType.PREFIX_FORCED;
								kvk[VK_RMENU].usedAsPrefix = KeyType.PREFIX_FORCED;
								ksc[LAlt].usedAsPrefix = KeyType.PREFIX_FORCED;
								ksc[RAlt].usedAsPrefix = KeyType.PREFIX_FORCED;
								break;

							case VK_SHIFT:
								kvk[VK_SHIFT].usedAsPrefix = KeyType.PREFIX_FORCED;
								kvk[VK_LSHIFT].usedAsPrefix = KeyType.PREFIX_FORCED;
								kvk[VK_RSHIFT].usedAsPrefix = KeyType.PREFIX_FORCED;
								ksc[LShift].usedAsPrefix = KeyType.PREFIX_FORCED;
								ksc[RShift].usedAsPrefix = KeyType.PREFIX_FORCED;
								break;

							case VK_CONTROL:
								kvk[VK_CONTROL].usedAsPrefix = KeyType.PREFIX_FORCED;
								kvk[VK_LCONTROL].usedAsPrefix = KeyType.PREFIX_FORCED;
								kvk[VK_RCONTROL].usedAsPrefix = KeyType.PREFIX_FORCED;
								ksc[LControl].usedAsPrefix = KeyType.PREFIX_FORCED;
								ksc[RControl].usedAsPrefix = KeyType.PREFIX_FORCED;
								break;
						}

						break;

					default:  // vk is a left/right modifier key such as VK_LCONTROL or VK_LWIN:
						if (alwaysSetAsPrefix)
							kvk[vk].usedAsPrefix = KeyType.PREFIX_ACTUAL;
						else if (HotkeyDefinition.FindHotkeyContainingModLR(kvk[vk].asModifiersLR) != null) // Fixed for v1.0.35.13 (used to be aSC vs. aVK).
							kvk[vk].usedAsPrefix = KeyType.PREFIX_ACTUAL;

						break;
						// else allow its suffix action to fire when key is pressed down,
						// under the fairly safe assumption that the user hasn't configured
						// the opposite key to also be a key-down suffix-action (but even
						// if the user has done this, it's an explicit override of the
						// safety checks here, so probably best to allow it).
				}

				return;
			}

			// Since above didn't return, using scan code instead of virtual key:
			if (alwaysSetAsPrefix)
				ksc[sc].usedAsPrefix = KeyType.PREFIX_ACTUAL;
			else if (HotkeyDefinition.FindHotkeyContainingModLR(ksc[sc].asModifiersLR) != null)
				ksc[sc].usedAsPrefix = KeyType.PREFIX_ACTUAL;
		}

		internal long SuppressThisKeyFunc(IntPtr hook, ref KBDLLHOOKSTRUCT lParam, uint vk, uint sc, bool keyUp, ulong extraInfo,
										  KeyHistoryItem keyHistoryCurr, uint hotkeyIDToPost, HotkeyVariant variant,
										  HotstringDefinition hs = null, CaseConformModes caseConformMode = CaseConformModes.None, char endChar = (char)0)
		// Always use the parameter vk rather than event.vkCode because the caller or caller's caller
		// might have adjusted vk, namely to make it a left/right specific modifier key rather than a
		// neutral one.
		{
			if (keyHistoryCurr.eventType == ' ') // then it hasn't been already set somewhere else
				keyHistoryCurr.eventType = 's';

			// This handles the troublesome Numlock key, which on some (most/all?) keyboards
			// will change state independent of the keyboard's indicator light even if its
			// keydown and up events are suppressed.  This is certainly true on the
			// MS Natural Elite keyboard using default drivers on WinXP.  SetKeyboardState()
			// doesn't resolve this, so the only alternative to the below is to use the
			// Win9x method of setting the Numlock state explicitly whenever the key is released.
			// That might be complicated by the fact that the unexpected state change described
			// here can't be detected by GetKeyboardState() and such (it sees the state indicated
			// by the numlock light on the keyboard, which is wrong).  In addition, doing it this
			// way allows Numlock to be a prefix key for something like Numpad7, which would
			// otherwise be impossible because Numpad7 would become NumpadHome the moment
			// Numlock was pressed down.  Note: this problem doesn't appear to affect Capslock
			// or Scrolllock for some reason, possibly hardware or driver related.
			// Note: the check for KEY_IGNORE isn't strictly necessary, but here just for safety
			// in case this is ever called for a key that should be ignored.  If that were
			// to happen and we didn't check for it, and endless loop of keyboard events
			// might be caused due to the keybd events sent below.
			if (hook == kbdHook)
			{
				var nl = (uint)Keys.NumLock;

				if (vk == nl && !keyUp && !IsIgnored(lParam.dwExtraInfo))
				{
					// This seems to undo the faulty indicator light problem and toggle
					// the key back to the state it was in prior to when the user pressed it.
					// Originally, I had two keydowns and before that some keyups too, but
					// testing reveals that only a single key-down is needed.  UPDATE:
					// It appears that all 4 of these key events are needed to make it work
					// in every situation, especially the case when ForceNumlock is on but
					// numlock isn't used for any hotkeys.
					// Note: The only side-effect I've discovered of this method is that the
					// indicator light can't be toggled after the program is exitted unless the
					// key is pressed twice:
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyUp, nl);
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDownAndUp, nl);
					kbdMsSender.SendKeyEvent(KeyEventTypes.KeyDown, nl);
				}

				UpdateKeybdState(ref lParam, vk, sc, keyUp, true);
			}

			// These should be posted only at the last possible moment before returning in order to
			// minimize the chance that the main thread will receive and process the message before
			// our thread can finish updating key states and other maintenance.  This has been proven
			// to be a problem on single-processor systems when the hook thread gets preempted
			// before it can return.  Apparently, the fact that the hook thread is much higher in priority
			// than the main thread is not enough to prevent the main thread from getting a timeslice
			// before the hook thread gets back another (at least on some systems, perhaps due to their
			// system settings of the same ilk as "favor background processes").
			SendHotkeyMessages(keyUp, extraInfo, keyHistoryCurr, hotkeyIDToPost, variant, hs, caseConformMode, endChar);
			return 1;
		}

		internal override bool SystemHasAnotherKeybdHook() => SystemHasAnotherHook(ref keybdMutex, KeybdMutexName);

		internal override bool SystemHasAnotherMouseHook() => SystemHasAnotherHook(ref mouseMutex, MouseMutexName);

		internal override uint TextToSC(ReadOnlySpan<char> text, ref bool? specifiedByNumber)
		{
			if (text.Length == 0)
				return 0u;

			if (keyToScAlt.TryGetValue(text, out var val))
				return val;

			// Do this only after the above, in case any valid key names ever start with SC:
			if (char.ToUpper(text[0]) == 'S' && char.ToUpper(text[1]) == 'C')
			{
				var s = text.Slice(2);
				var digits = 0;

				foreach (var ch in s)
					if (ch.IsHex())
						digits++;

				var ok = uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var ii);

				if (!ok || (2 + digits < text.Length))
					return 0; // Fixed for v1.1.27: Disallow any invalid suffix so that hotkeys like a::scb() are not misinterpreted as remappings.

				if (specifiedByNumber != null)
					specifiedByNumber = true; // Override caller-set default.

				return ii;
			}

			return 0u; // Indicate "not found".
		}

		/// <summary>
		/// Returns vk for key-down, negative vk for key-up, or zero if no translation.
		/// We also update whatever's in *pModifiers and *pModifiersLR to reflect the type of key-action
		/// specified in <aText>.  This makes it so that {altdown}{esc}{altup} behaves the same as !{esc}.
		/// Note that things like LShiftDown are not supported because: 1) they are rarely needed; and 2)
		/// they can be down via "lshift down".
		/// </summary>
		/// <param name="text"></param>
		/// <param name="eventType"></param>
		/// <param name="modifiersLR"></param>
		/// <param name="updatePersistent"></param>
		/// <returns></returns>
		internal override uint TextToSpecial(ReadOnlySpan<char> text, ref KeyEventTypes eventType, ref uint modifiersLR, bool updatePersistent)
		{
			if (text.StartsWith("ALTDOWN", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					if ((modifiersLR & (MOD_LALT | MOD_RALT)) == 0) // i.e. do nothing if either left or right is already present.
						modifiersLR |= MOD_LALT; // If neither is down, use the left one because it's more compatible.

				eventType = KeyEventTypes.KeyDown;
				return VK_MENU;
			}

			if (text.StartsWith("ALTUP", StringComparison.OrdinalIgnoreCase))
			{
				// Unlike for Lwin/Rwin, it seems best to have these neutral keys (e.g. ALT vs. LALT or RALT)
				// restore either or both of the ALT keys into the up position.  The user can use {LAlt Up}
				// to be more specific and avoid this behavior:
				if (updatePersistent)
					modifiersLR &= 0xF3;// ~(MOD_LALT | MOD_RALT);

				eventType = KeyEventTypes.KeyUp;
				return VK_MENU;
			}

			if (text.StartsWith("SHIFTDOWN", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					if ((modifiersLR & (MOD_LSHIFT | MOD_RSHIFT)) == 0) // i.e. do nothing if either left or right is already present.
						modifiersLR |= MOD_LSHIFT; // If neither is down, use the left one because it's more compatible.

				eventType = KeyEventTypes.KeyDown;
				return VK_SHIFT;
			}

			if (text.StartsWith("SHIFTUP", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					modifiersLR &= 0x49;// ~(MOD_LSHIFT | MOD_RSHIFT); // See "ALTUP" for explanation.

				eventType = KeyEventTypes.KeyUp;
				return VK_SHIFT;
			}

			if (text.StartsWith("CTRLDOWN", StringComparison.OrdinalIgnoreCase) || text.StartsWith("CONTROLDOWN", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					if ((modifiersLR & (MOD_LCONTROL | MOD_RCONTROL)) == 0) // i.e. do nothing if either left or right is already present.
						modifiersLR |= MOD_LCONTROL; // If neither is down, use the left one because it's more compatible.

				eventType = KeyEventTypes.KeyDown;
				return VK_CONTROL;
			}

			if (text.StartsWith("CTRLUP", StringComparison.OrdinalIgnoreCase) || text.StartsWith("CONTROLUP", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					modifiersLR &= 0xFC;// ~(MOD_LCONTROL | MOD_RCONTROL); // See "ALTUP" for explanation.

				eventType = KeyEventTypes.KeyUp;
				return VK_CONTROL;
			}

			if (text.StartsWith("LWINDOWN", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					modifiersLR |= MOD_LWIN;

				eventType = KeyEventTypes.KeyDown;
				return VK_LWIN;
			}

			if (text.StartsWith("LWINUP", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					modifiersLR &= 0xBF;// ~MOD_LWIN;

				eventType = KeyEventTypes.KeyUp;
				return VK_LWIN;
			}

			if (text.StartsWith("RWINDOWN", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					modifiersLR |= MOD_RWIN;

				eventType = KeyEventTypes.KeyDown;
				return VK_RWIN;
			}

			if (text.StartsWith("RWINUP", StringComparison.OrdinalIgnoreCase))
			{
				if (updatePersistent)
					modifiersLR &= 0x7F;// ~MOD_RWIN;

				eventType = KeyEventTypes.KeyUp;
				return VK_RWIN;
			}

			// Otherwise, leave aEventType unchanged and return zero to indicate failure:
			return 0;
		}

		/// <summary>
		/// If modifiers_p is non-NULL, place the modifiers that are needed to realize the key in there.
		/// e.g. M is really +m (shift-m), # is really shift-3.
		/// HOWEVER, this function does not completely overwrite the contents of pModifiersLR; instead, it just
		/// adds the required modifiers into whatever is already there.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="modifiersLR"></param>
		/// <param name="excludeThoseHandledByScanCode"></param>
		/// <param name="allowExplicitVK"></param>
		/// <param name="keybdLayout"></param>
		/// <returns></returns>
		internal override uint TextToVK(ReadOnlySpan<char> text, ref uint? modifiersLR, bool excludeThoseHandledByScanCode, bool allowExplicitVK, IntPtr keybdLayout)
		{
			if (text.Length == 0)
				return 0;

			if (keybdLayout == IntPtr.Zero)
				keybdLayout = script.PlatformProvider.Manager.GetKeyboardLayout(0);

			// Don't trim() aText or modify it because that will mess up the caller who expects it to be unchanged.
			// Instead, for now, just check it as-is.  The only extra whitespace that should exist, due to trimming
			// of text during load, is that on either side of the COMPOSITE_DELIMITER (e.g. " then ").

			if (text.Length == 1) // _tcslen(aText) == 1
				return CharToVKAndModifiers(text[0], ref modifiersLR, keybdLayout); // Making this a function simplifies things because it can do early return, etc.

			if (allowExplicitVK && char.ToUpper(text[0]) == 'V' && char.ToUpper(text[1]) == 'K')
			{
				var s = text.Slice(2);
				var digits = 0;

				foreach (var ch in s)
					if (ch.IsHex())
						digits++;

				var ok = uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var ii);
				return !ok || (2 + digits < text.Length) ? 0 : ii; // Fixed for v1.1.27: Disallow any invalid suffix so that hotkeys like a::vkb() are not misinterpreted as remappings.
			}

			if (keyToVkAlt.TryGetValue(text, out var val))
				return val;

			if (excludeThoseHandledByScanCode)
				return 0; // Zero is not a valid virtual key, so it should be a safe failure indicator.

			// Otherwise check if aText is the name of a key handled by scan code and if so, map that
			// scan code to its corresponding virtual key:
			bool? dummy = null;
			var sc = TextToSC(text, ref dummy);
			return sc != 0 ? MapScToVk(sc) : 0;
		}

		internal override bool TextToVKandSC(ReadOnlySpan<char> text, ref uint vk, ref uint sc, ref uint? modifiersLR, IntPtr keybdLayout)
		{
			if ((vk = TextToVK(text, ref modifiersLR, true, true, keybdLayout)) != 0)
			{
				sc = 0; // Caller should call vk_to_sc(aVK) if needed.
				return true;
			}

			bool? dummy = null;

			if ((sc = TextToSC(text, ref dummy)) != 0)
			{
				return true;// Leave aVK set to 0.  Caller should call sc_to_vk(aSC) if needed.
			}

			if (text.StartsWith("vk", StringComparison.OrdinalIgnoreCase)) // Could be vkXXscXXX, which TextToVK() does not permit in v1.1.27+.
			{
				var vkIndex = text.IndexOf("vk", StringComparison.OrdinalIgnoreCase);
				var scIndex = text.IndexOf("sc", StringComparison.OrdinalIgnoreCase);

				if (vkIndex == 0 && scIndex > 2)
				{
					var vkStart = vkIndex + 2;
					var vkSpan = text.Slice(vkStart, scIndex - vkStart);
					var scStart = scIndex;
					var scSpan = text.Slice(scStart + 2);

					if (uint.TryParse(vkSpan, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var t1) &&
							uint.TryParse(scSpan, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out var t2))
					{
						vk = t1;
						sc = t2;
						return true;
					}
				}
			}

			return false;
		}

		internal override void Unhook()
		{
			// PostQuitMessage() might be needed to prevent hang-on-exit.  Once this is done, no message boxes or
			// other dialogs can be displayed.  MSDN: "The exit value returned to the system must be the wParam
			// parameter of the WM_QUIT message."  In our case, PostQuitMessage() should announce the same exit code
			// that we will eventually call exit() with:
			//Original did these, but HookThread.Stop() will take care of it before this is called.
			//WindowsAPI.PostQuitMessage(exitCode);
			AddRemoveHooks(HookType.None); // Remove all hooks. By contrast, registered hotkeys are unregistered below.
			Unhook(script.playbackHook); // Would be unusual for this to be installed during exit, but should be checked for completeness.
			thread?.Dispose();
		}

		internal override void Unhook(nint hook)
		{
			if (hook != IntPtr.Zero)
				Invoke(() => _ = UnhookWindowsHookEx(hook));
		}

		/// <summary>
		/// Caller has ensured that vk has been translated from neutral to left/right if necessary.
		/// Always use the parameter vk rather than event.vkCode because the caller or caller's caller
		/// might have adjusted vk, namely to make it a left/right specific modifier key rather than a
		/// neutral one.
		/// </summary>
		internal void UpdateKeybdState(ref KBDLLHOOKSTRUCT ev, uint vk, uint sc, bool keyUp, bool isSuppressed)
		{
			// If this function was called from SuppressThisKey(), these comments apply:
			// Currently SuppressThisKey is only called with a modifier in the rare case
			// when sDisguiseNextLWinUp/RWinUp is in effect.  But there may be other cases in the
			// future, so we need to make sure the physical state of the modifiers is updated
			// in our tracking system even though the key is being suppressed:
			uint modLR;

			if ((modLR = kvk[vk].asModifiersLR) != 0) // Update our tracking of LWIN/RWIN/RSHIFT etc.
			{
				// Caller has ensured that vk has been translated from neutral to left/right if necessary
				// (e.g. VK_CONTROL -> VK_LCONTROL). For this reason, always use the parameter vk rather
				// than the raw event.vkCode.
				// Below excludes KEY_IGNORE_ALL_EXCEPT_MODIFIER since that type of event shouldn't be ignored by
				// this function.  UPDATE: KEY_PHYS_IGNORE is now considered to be something that shouldn't be
				// ignored in this case because if more than one instance has the hook installed, it is
				// possible for kbdMsSender.modifiersLR_logical_non_ignored to say that a key is down in one instance when
				// that instance's kbdMsSender.modifiersLR_logical doesn't say it's down, which is definitely wrong.  So it
				// is now omitted below:
				var isNotIgnored = ev.dwExtraInfo != KeyIgnore;
				var isFakeShift = ev.scanCode == FakeLShift || ev.scanCode == FakeRShift;
				var isFakeCtrl = ev.scanCode == FakeLControl; // AltGr.
				var eventIsPhysical = !isFakeShift && KeybdEventIsPhysical(ev.flags, vk, keyUp);// For backward-compatibility, fake LCtrl is marked as physical.

				if (keyUp)
				{
					// Keep track of system-generated Shift-up events (as part of a workaround for
					// Shift becoming stuck due to interaction between Send and the system handling
					// of shift-numpad combinations).  Find "fake shift" for more details.
					if (isFakeShift)
						kbdMsSender.modifiersLRNumpadMask |= modLR;

					if (!isSuppressed)
					{
						kbdMsSender.modifiersLRLogical &= ~modLR;

						// Even if is_not_ignored == true, this is updated unconditionally on key-up events
						// to ensure that kbdMsSender.modifiersLR_logical_non_ignored never says a key is down when
						// kbdMsSender.modifiersLR_logical says its up, which might otherwise happen in cases such
						// as alt-tab.  See this comment further below, where the operative word is "relied":
						// "key pushed ALT down, or relied upon it already being down, so go up".  UPDATE:
						// The above is no longer a concern because KeyEvent() now defaults to the mode
						// which causes our var "is_not_ignored" to be true here.  Only the Send command
						// overrides this default, and it takes responsibility for ensuring that the older
						// comment above never happens by forcing any down-modifiers to be up if they're
						// not logically down as reflected in kbdMsSender.modifiersLR_logical.  There's more
						// explanation for kbdMsSender.modifiersLR_logical_non_ignored in keyboard_mouse.h:
						if (isNotIgnored)
							kbdMsSender.modifiersLRLogicalNonIgnored &= ~modLR;
					}

					if (eventIsPhysical) // Note that ignored events can be physical via KEYEVENT_PHYS()
					{
						kbdMsSender.modifiersLRPhysical &= ~modLR;
						physicalKeyState[vk] = 0;

						if (!isFakeCtrl)
							kbdMsSender.modifiersLRCtrlAltDelMask &= ~modLR;

						// If a modifier with an available neutral VK has been released, update the state
						// of the neutral VK to be that of the opposite key (the one that wasn't released):
						switch (vk)
						{
							case VK_LSHIFT: physicalKeyState[VK_SHIFT] = physicalKeyState[VK_RSHIFT]; break;

							case VK_RSHIFT: physicalKeyState[VK_SHIFT] = physicalKeyState[VK_LSHIFT]; break;

							case VK_LCONTROL: physicalKeyState[VK_CONTROL] = physicalKeyState[VK_RCONTROL]; break;

							case VK_RCONTROL: physicalKeyState[VK_CONTROL] = physicalKeyState[VK_LCONTROL]; break;

							case VK_LMENU: physicalKeyState[VK_MENU] = physicalKeyState[VK_RMENU]; break;

							case VK_RMENU: physicalKeyState[VK_MENU] = physicalKeyState[VK_LMENU]; break;
						}
					}

					kbdMsSender.modifiersLRLastPressed = 0;
				}
				else // Modifier key was pressed down.
				{
					kbdMsSender.modifiersLRNumpadMask &= ~modLR;

					if (!isSuppressed)
					{
						kbdMsSender.modifiersLRLogical |= modLR;

						if (isNotIgnored)
							kbdMsSender.modifiersLRLogicalNonIgnored |= modLR;
					}

					if (eventIsPhysical)
					{
						kbdMsSender.modifiersLRPhysical |= modLR;
						physicalKeyState[vk] = StateDown;

						if (!isFakeCtrl)
							kbdMsSender.modifiersLRCtrlAltDelMask |= modLR;

						// If a modifier with an available neutral VK has been pressed down (unlike LWIN & RWIN),
						// update the state of the neutral VK to be down also:
						switch (vk)
						{
							case VK_LSHIFT:
							case VK_RSHIFT: physicalKeyState[VK_SHIFT] = StateDown; break;

							case VK_LCONTROL:
							case VK_RCONTROL: physicalKeyState[VK_CONTROL] = StateDown; break;

							case VK_LMENU:
							case VK_RMENU: physicalKeyState[VK_MENU] = StateDown; break;
						}
					}

					// See comments in GetModifierLRState() for details about the following.
					kbdMsSender.modifiersLRLastPressed = modLR;
					kbdMsSender.modifiersLRLastPressedTime = DateTime.UtcNow;
				}
			} // vk is a modifier key.
		}

		/// <summary>
		/// Given a VK code, returns the character that an unmodified keypress would produce
		/// on the given keyboard layout.  Defaults to the script's own layout if omitted.
		/// Using this rather than MapVirtualKey() fixes some inconsistency that used to
		/// exist between 'A'-'Z' and every other key.
		/// </summary>
		/// <param name="vk"></param>
		/// <param name="keybdLayout"></param>
		/// <returns></returns>
		internal override char VKtoChar(uint vk, IntPtr keybdLayout)
		{
			if (keybdLayout == IntPtr.Zero)
				keybdLayout = script.PlatformProvider.Manager.GetKeyboardLayout(0);

			// MapVirtualKeyEx() always produces 'A'-'Z' for those keys regardless of keyboard layout,
			// but for any other keys it produces the correct results, so we'll use it:
			if (vk > 'Z' || vk < 'A')
				return (char)MapVirtualKeyEx(vk, MAPVK_VK_TO_CHAR, keybdLayout);

			// For any other keys,
			var ch = new StringBuilder();
			var chNotUsed = new StringBuilder();
			var keyState = new byte[256];
			var deadChar = (char)0;
			int n;

			// If there's a pending dead-key char in aKeybdLayout's buffer, it would modify the result.
			// We don't want that to happen, so as a workaround we pass a key-code which doesn't combine
			// with any dead chars, and will therefore pull it out.  VK_DECIMAL is used because it is
			// almost always valid; see http://www.siao2.com/2007/10/27/5717859.aspx
			if (ToUnicodeOrAsciiEx(VK_DECIMAL, 0, keyState, ch, 0, keybdLayout) == 2)
			{
				// Save the char to be later re-injected.
				deadChar = ch.ToString()[0];
			}

			// Retrieve the character that corresponds to aVK, if any.
			n = ToUnicodeOrAsciiEx(vk, 0, keyState, ch, 0, keybdLayout);

			if (n < 0) // aVK is a dead key, and we've just placed it into aKeybdLayout's buffer.
			{
				// Flush it out in the same manner as before (see above).
				_ = ToUnicodeOrAsciiEx(VK_DECIMAL, 0, keyState, chNotUsed, 0, keybdLayout);
			}

			if (deadChar != (char)0)
			{
				// Re-inject the dead-key char so that user input is not interrupted.
				// To do this, we need to find the right VK and modifier key combination:
				uint? modLR = 0u;
				var dead_vk = CharToVKAndModifiers(deadChar, ref modLR, keybdLayout);

				if (dead_vk != 0)
				{
					AdjustKeyState(keyState, modLR.Value);
					_ = ToUnicodeOrAsciiEx(dead_vk, 0, keyState, chNotUsed, 0, keybdLayout);
				}

				//else: can't do it.
			}

			// ch[0] is set even for n < 0, but might not be for n == 0.
			return n != 0 ? ch.ToString()[0] : (char)0;
		}

		internal override void WaitHookIdle()
		// Wait until the hook has reached a known idle state (i.e. finished any processing
		// that it was in the middle of, though it could start something new immediately after).
		{
			//Make sure this is not called within the channel thread because it would deadlock if so.
			if (channelThreadID != mgr.CurrentThreadId() && IsReadThreadRunning())
			{
				hookSynced = false;

				if (channel.Writer.TryWrite(new KeysharpMsg()
			{
				message = (uint)UserMessages.AHK_HOOK_SYNC
				}))
				{
					while (!hookSynced)
						Flow.SleepWithoutInterruption();
				}
			}
		}

		protected internal override void DeregisterHooks()
		{
			if (IsReadThreadRunning())
			{
				_ = ChangeHookState(HookType.None, false);
				//var ksmsg = new KeysharpMsg()
				//{
				//  message = (uint)UserMessages.AHK_CHANGE_HOOK_STATE
				//};
				//
				//if (channel.Writer.TryWrite(ksmsg))
				//{
				//  var startTime = DateTime.UtcNow;
				//
				//  while ((DateTime.UtcNow - startTime).TotalMilliseconds < 1000 && !ksmsg.completed)
				//      System.Threading.Thread.Sleep(100); // Should never execute if thread already existed before this function was called.
				//}
			}
		}

		protected internal override void Start()
		{
			//if (IsReadThreadRunning())
			//  Stop();

			//If it's running there is no reason to start it again.
			if (IsReadThreadRunning())
				return;

			running = true;
			channelThreadID = 0;
			thread = new StaThreadWithMessageQueue();
			//This is a consolidation of the main windows proc, message sleep and the thread which they keyboard hook is created on.
			//Unsure how much of this is windows specific or can be cross platform. Will need to determine when we begin linux work.//TODO
			//If Start() is called while this thread is already running, the foreach will exit, and thus the previous thread will exit.
			channelReadThread = Task.Factory.StartNew(async () =>
			{
				try
				{
					Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;//AHK Sets this to critical which seems extreme.
					var reader = channel.Reader;
					channelThreadID = mgr.CurrentThreadId();

					await foreach (var item in reader.ReadAllAsync())//This should be totally reworked to use object types/casting rather than packing all manner of obscure meaning into bits and bytes of wparam and lparam.
						//while (true)
					{
						//if (!reader.TryRead(out var item))
						//{
						//  Flow.Sleep(10L);
						//  continue;
						//}
						//var item = await reader.ReadAsync();
						//var theasyncfunc = async () =>
						var criterion_found_hwnd = IntPtr.Zero;
						channelThreadID = mgr.CurrentThreadId();

						if (item is KeysharpMsg msg)
						{
							if (msg.message == WM_QUIT)//Needed to be pulled out of the case statement because it uses fallthrough logic which isn't allowed in C#.
								// After this message, fall through to the next case below so that the hooks will be removed before
								// exiting this thread.
								msg.wParam = IntPtr.Zero; // Indicate to AHK_CHANGE_HOOK_STATE that both hooks should be deactivated.

							var wParamVal = msg.wParam.ToInt64();
							var lParamVal = msg.lParam.ToInt64();
							// ********
							// NO BREAK IN ABOVE, FALL INTO NEXT CASE:
							// ********
							var tv = script.Threads.GetThreadVariables();
							tv.WaitForCriticalToFinish();//Must wait until the previous critical task finished before proceeding.

							switch (msg.message)
							{
								case (uint)UserMessages.AHK_CHANGE_HOOK_STATE: // No blank line between this in the above to indicate fall-through.
									// In this case, wParam contains the bitwise set of hooks that should be active.
									/*
									    problem_activating_hooks = false;

									    if ((wParamVal & (long)HookType.Keyboard) != 0) // Activate the keyboard hook (if it isn't already).
									    {
									    if (kbdHook == IntPtr.Zero)
									    {
									        // v1.0.39: Reset *before* hook is installed to avoid any chance that events can
									        // flow into the hook prior to the reset:
									        if (msg.lParam != IntPtr.Zero) // Sender of msg. is signaling that reset should be done.
									            ResetHook(false, HookType.Keyboard, true);

									        if ((kbdHook = SetWindowsHookEx(WH_KEYBOARD_LL,
									                                        LowLevelKeybdHandler,
									                                        GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0)) == IntPtr.Zero)
									            problem_activating_hooks = true;
									    }
									    }
									    else // Caller specified that the keyboard hook is to be deactivated (if it isn't already).
									    if (kbdHook != IntPtr.Zero)
									        if (UnhookWindowsHookEx(kbdHook))
									            kbdHook = IntPtr.Zero;

									    if ((wParamVal & (long)HookType.Mouse) != 0) // Activate the mouse hook (if it isn't already).
									    {
									    if (mouseHook == IntPtr.Zero)
									    {
									        if (msg.lParam != IntPtr.Zero) // Sender of msg. is signaling that reset should be done.
									            ResetHook(false, HookType.Mouse, true);

									        if ((mouseHook = SetWindowsHookEx(WH_MOUSE_LL,
									                                          mouseHandlerDel,
									                                          GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0)) == IntPtr.Zero)
									            problem_activating_hooks = true;
									    }
									    }
									    else // Caller specified that the mouse hook is to be deactivated (if it isn't already).
									    if (mouseHook != IntPtr.Zero)
									        if (WindowsAPI.UnhookWindowsHookEx(mouseHook))
									            mouseHook = IntPtr.Zero;

									    // Upon failure, don't display MsgBox here because although MsgBox's own message pump would
									    // service the hook that didn't fail (if it's active), it's best to avoid any blocking calls
									    // here so that this event loop will continue to run.  For example, the script or OS might
									    // ask this thread to terminate, which it couldn't do cleanly if it was in a blocking call.
									    // Instead, send a reply back to the caller.
									    // It's safe to post directly to thread because the creator of this thread should be
									    // explicitly waiting for this message (so there's no chance that a MsgBox msg pump
									    // will discard the message unless the caller has timed out, which seems impossible
									    // in this case).
									    if (wParamVal != 0) // The caller wants a reply only when it didn't ask us to terminate via deactivating both hooks.
									    msg.wParam = new IntPtr(problem_activating_hooks ? 1 : 0);

									    //PostThreadMessage(Processes.MainThreadID, (uint)UserMessages.AHK_CHANGE_HOOK_STATE, new UIntPtr(problem_activating_hooks ? 1u : 0u), IntPtr.Zero);
									    msg.completed = true;

									    //else this is WM_QUIT or the caller wanted this thread to terminate.  Send no reply.

									    // If caller passes true for msg.lParam, it wants a permanent change to hook state; so in that case, terminate this
									    // thread whenever neither hook is no longer present.
									    if (lParamVal != 0 && kbdHook == IntPtr.Zero && mouseHook == IntPtr.Zero) // Both hooks are inactive (for whatever reason).
									    return; // Thread is no longer needed. The "return" automatically calls ExitThread().

									    // 1) Due to this thread's non-GUI nature, there doesn't seem to be any need to call
									    // the somewhat mysterious PostQuitMessage() here.
									    // 2) For thread safety and maintainability, it seems best to have the caller take
									    // full responsibility for freeing the hook's memory.
									*/
									break;

								case (uint)UserMessages.AHK_HOOK_SYNC:
									hookSynced = true;
									break;

								case (uint)UserMessages.AHK_HOOK_SET_KEYHISTORY:
									keyHistory = new KeyHistory((int)wParamVal);
									break;

								//These were taken from MsgSleep().
								case (uint)UserMessages.AHK_HOTSTRING:
									if (msg.obj is HotstringMsg hmsg)
									{
										var hs = hmsg.hs;

										if (hs.hotCriterion != null)
										{
											// For details, see comments in the hotkey section of this switch().
											criterion_found_hwnd = new IntPtr(HotkeyDefinition.HotCriterionAllowsFiring(hs.hotCriterion, hs.Name));

											if (criterion_found_hwnd.ToInt64() == 0L)
												// Hotstring is no longer eligible to fire even though it was when the hook sent us
												// the message.  Abort the firing even though the hook may have already started
												// executing the hotstring by suppressing the final end-character or other actions.
												// It seems preferable to abort midway through the execution than to continue sending
												// keystrokes to the wrong window, or when the hotstring has become suspended.
												continue;

											if (!(string.Compare(hs.hotCriterion.Name, "HotIfWinNotActivePrivate", true) == 0 || string.Compare(hs.hotCriterion.Name, "HotIfWinNotExistPrivate", true) == 0))
												criterion_found_hwnd = IntPtr.Zero;
											else if (hs.HotIfRequiresEval())
												criterion_found_hwnd = script.hotExprLFW;// For #if WinExist(WinTitle) and similar.
										}
										else // No criterion, so it's a global hotstring.  It can always fire, but it has no "last found window".
											criterion_found_hwnd = IntPtr.Zero;

										// Do a simple replacement for the hotstring if that's all that's called for.
										// Don't create a new quasi-thread or any of that other complexity done further
										// below.  But also do the backspacing (if specified) for a non-autoreplace hotstring,
										// even if it can't launch due to MaxThreads, MaxThreadsPerHotkey, or some other reason:
										//Any key sending must be on the main thread else keys will come in out of order.
										//Does only the backspacing if it's not an auto-replace hotstring.
										script.mainWindow.CheckedInvoke(() => hs.DoReplace(hmsg.caseMode, hmsg.endChar), true);

										if (string.IsNullOrEmpty(hs.replacement))
										{
											// Otherwise, continue on and let a new thread be created to handle this hotstring.
											// Since this isn't an auto-replace hotstring, set this value to support
											// the built-in variable A_EndChar:
											_ = hs.PerformInNewThreadMadeByCaller(criterion_found_hwnd, hmsg.endChar.ToString());
										}
										else
											continue;
									}

									break;

								case WM_HOTKEY://Some hotkeys are handled directly by windows using WndProc(), others, such as those with left/right modifiers, are handled directly by us.
								case (uint)UserMessages.AHK_HOOK_HOTKEY://Some hotkeys are handled directly by windows using WndProc(), others, such as those with left/right modifiers, are handled directly by us.
								{
									script.HookThread.kbdMsSender.ProcessHotkey((int)wParamVal, (int)lParamVal, msg.obj as HotkeyVariant, msg.message);
									break;
								}

								//case (uint)UserMessages.AHK_HOTSTRING: // Added for v1.0.36.02 so that hotstrings work even while an InputBox or other non-standard msg pump is running.
								//case (uint)UserMessages.AHK_CLIPBOARD_CHANGE: //Probably not needed because we handle OnClipboardChange() differently. Added for v1.0.44 so that clipboard notifications aren't lost while the script is displaying a MsgBox or other dialog.
								case (uint)UserMessages.AHK_INPUT_END:

									// If the following facts are ever confirmed, there would be no need to post the message in cases where
									// the MsgSleep() won't be done:
									// 1) The mere fact that any of the above messages has been received here in MainWindowProc means that a
									//    message pump other than our own main one is running (i.e. it is the closest pump on the call stack).
									//    This is because our main message pump would never have dispatched the types of messages above because
									//    it is designed to fully handle then discard them.
									// 2) All of these types of non-main message pumps would discard a message with a NULL hwnd.
									//
									// One source of confusion is that there are quite a few different types of message pumps that might
									// be running:
									// - InputBox/MsgBox, or other dialog
									// - Popup menu (tray menu, popup menu from Menu command, or context menu of an Edit/MonthCal, including
									//   our main window's edit control g_hWndEdit).
									// - Probably others, such as ListView marquee-drag, that should be listed here as they are
									//   remembered/discovered.
									//
									// Due to maintainability and the uncertainty over backward compatibility (see comments above), the
									// following message is posted even when INTERRUPTIBLE==false.
									// Post it with a NULL hwnd (update: also for backward compatibility) to avoid any chance that our
									// message pump will dispatch it back to us.  We want these events to always be handled there,
									// where almost all new quasi-threads get launched.  Update: Even if it were safe in terms of
									// backward compatibility to change NULL to gHwnd, testing shows it causes problems when a hotkey
									// is pressed while one of the script's menus is displayed (at least a menu bar).  For example:
									// *LCtrl::Send {Blind}{Ctrl up}{Alt down}
									// *LCtrl up::Send {Blind}{Alt up}
									//PostMessage(NULL, iMsg, wParam, lParam);
									//
									//if (IsInterruptible())
									//  MsgSleep(-1, RETURN_AFTER_MESSAGES_SPECIAL_FILTER);
									//else let the other pump discard this hotkey event since in most cases it would do more harm than good
									// (see comments above for why the message is posted even when it is 90% certain it will be discarded
									// in all cases where MsgSleep isn't done).
									//return 0;
									if (tv.priority == 0 && script.Threads.AnyThreadsAvailable())
									{
										if (msg.obj is InputType it
												&& it.InputRelease() is InputType inputHook
												&& inputHook.scriptObject is InputObject so)
										{
											if (so.OnEnd is IFuncObj ifo)
											{
												script.Threads.LaunchInThread(0, false, false, ifo, [so], true);
											}
										}
										else
											continue;
									}
									else
										//continue;
										continue;

									break;

								case (uint)UserMessages.AHK_INPUT_KEYDOWN:
								case (uint)UserMessages.AHK_INPUT_CHAR:
								case (uint)UserMessages.AHK_INPUT_KEYUP:
								{
									InputType input_hook;
									var inputHookParam = msg.obj as InputType;

									for (input_hook = script.input; input_hook != null && input_hook != inputHookParam; input_hook = input_hook.prev)
									{
									}

									if (input_hook == null)
										continue;

									if ((msg.message == (uint)UserMessages.AHK_INPUT_KEYDOWN ? input_hook.scriptObject.OnKeyDown
											: msg.message == (uint)UserMessages.AHK_INPUT_KEYUP ? input_hook.scriptObject.OnKeyUp
											: input_hook.scriptObject.OnChar) is IFuncObj ifo
											&& script.Threads.AnyThreadsAvailable())
									{
										var args = msg.message == (uint)UserMessages.AHK_INPUT_CHAR ?//AHK_INPUT_CHAR passes the chars as a string, whereas the rest pass them individually.
												   new object[] { input_hook.scriptObject, new string(new char[] { (char)lParamVal, (char)wParamVal }) }
												   : [input_hook.scriptObject, lParamVal, wParamVal];
										script.Threads.LaunchInThread(0, false, false, ifo, args, true);
									}
									else
										continue;
								}
								break;

								default:
									break;
							}

							//This is not going to work. It's always going to comapre against this queue thread's priority, not the priority of whichever hotkey/string is currently executing.
							//None of this will work until we implement real threads.
							//TODO
							//if (priority < (long)Accessors.A_Priority)
							//  continue;
							//Original tries to do some type of thread init here.//TOOD
							script.lastPeekTime = DateTime.UtcNow;
						}
					}

					System.Diagnostics.Debug.WriteLine("Exiting reader channel.");
				}
				catch (Exception ex)
				{
					_ = Debug.OutputDebug($"Windows hook thread exited unexpectedly: {ex}");
				}
				finally
				{
					running = false;
					Thread.CurrentThread.Priority = ThreadPriority.Normal;
				}
			}
			//Unsure if this will work or is needed here.
			//,CancellationToken.None, TaskCreationOptions.None,
			//SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current
													 );

			while (channelThreadID == 0)//Give it some time to startup before proceeding.
				Thread.Sleep(10);
		}

		protected internal override void Stop()
		{
			thread?.Dispose();
			base.Stop();
		}

		private bool ChangeHookState(HookType hooksToBeActive, bool changeIsTemporary)//This is going to be a problem if it's ever called to re-add a hook from another thread because only the main gui thread has a message loop.//TODO
		{
			var problem_activating_hooks = false;
			Func<object> func = () =>
			{
				if (((uint)hooksToBeActive & (uint)HookType.Keyboard) != 0) // Activate the keyboard hook (if it isn't already).
				{
					if (kbdHook == IntPtr.Zero)
					{
						// v1.0.39: Reset *before* hook is installed to avoid any chance that events can
						// flow into the hook prior to the reset:
						if (!changeIsTemporary) // Sender of msg. is signaling that reset should be done.
							ResetHook(false, HookType.Keyboard, true);

						//Note that in AHK, LowLevelKeybdHandler() is called for every keystroke and runs in its own thread.
						//No matter what we do in C#, it can and must only run on the main window thread.
						//This could potentially be a problem with all of the intricate code that takes special action depending
						//on the thread that a particular function is being called from.
						if ((kbdHook = SetWindowsHookEx(WH_KEYBOARD_LL,
														kbdHandlerDel,//This must be a class member or else it will go out of scope and cause the program to crash unpredictably.
														//GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0)) == IntPtr.Zero)
														Process.GetCurrentProcess().MainModule.BaseAddress, 0)) == IntPtr.Zero)
							problem_activating_hooks = true;
					}
				}
				else // Caller specified that the keyboard hook is to be deactivated (if it isn't already).
					if (HasKbdHook())
						if (UnhookWindowsHookEx(kbdHook) || GetLastError() == ERROR_INVALID_HOOK_HANDLE)// Check last error in case the OS has already removed the hook.
							kbdHook = IntPtr.Zero;

				if (((uint)hooksToBeActive & (uint)HookType.Mouse) != 0) // Activate the mouse hook (if it isn't already).
				{
					if (mouseHook == IntPtr.Zero)
					{
						if (!changeIsTemporary) // Sender of msg. is signaling that reset should be done.
							ResetHook(false, HookType.Mouse, true);

						if ((mouseHook = SetWindowsHookEx(WH_MOUSE_LL,
														  mouseHandlerDel,
														  //GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0)) == IntPtr.Zero)
														  Process.GetCurrentProcess().MainModule.BaseAddress, 0)) == IntPtr.Zero)
							problem_activating_hooks = true;
					}
				}
				else // Caller specified that the mouse hook is to be deactivated (if it isn't already).
					if (mouseHook != IntPtr.Zero)
						if (UnhookWindowsHookEx(mouseHook) || GetLastError() == ERROR_INVALID_HOOK_HANDLE)// Check last error in case the OS has already removed the hook.
							mouseHook = IntPtr.Zero;

				return "";
			};
			//Any modifications to the hooks must be done on the main thread else Windows will internally ignore them.
			//We assume that if the main window does not exist yet, then this code is running within the part of main() that happens
			//before Application.Run().
			//This must use the context, rather than the main window, because this could happen on a script containing InputHook,
			//which are not persistent, and thus no main window will be present.
			//kbMouseContext will be null if the hooks are run on the main thread.
			//mainContext be null during unit tests.
			//if (Script.kbMouseContext == null && Processes.mainContext != null)
			//  Processes.mainContext.Send(new SendOrPostCallback((obj) => func()), null);
			//else
			//  func();
			Invoke(func);
			//Keysharp.Scripting.Script.mainWindow.CheckedInvoke(func, true);
			return problem_activating_hooks;
		}

		private IntPtr LowLevelKeybdHandler(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
		{
			if (code != HC_ACTION)  // MSDN docs specify that both LL keybd & mouse hook should return in this case.
				return CallNextHookEx(kbdHook, code, wParam, ref lParam);

			var wParamVal = wParam.ToInt64();

			// Change the event to be physical if that is indicated in its dwExtraInfo attribute.
			// This is done for cases when the hook is installed multiple times and one instance of
			// it wants to inform the others that this event should be considered physical for the
			// purpose of updating modifier and key states:
			if (lParam.dwExtraInfo == KeyPhysIgnore)
				lParam.flags &= ~LLKHF_INJECTED;
			else if (lParam.dwExtraInfo == KeyBlockThis)
				return new IntPtr(1);

			// Make all keybd events physical to try to fool the system into accepting CTRL-ALT-DELETE.
			// This didn't work, which implies that Ctrl-Alt-Delete is trapped at a lower level than
			// this hook (folks have said that it's trapped in the keyboard driver itself):
			//event.flags &= ~LLKHF_INJECTED;
			// Note: Some scan codes are shared by more than one key (e.g. Numpad7 and NumpadHome).  This is why
			// the keyboard hook must be able to handle hotkeys by either their virtual key or their scan code.
			// i.e. if sc were always used in preference to vk, we wouldn't be able to distinguish between such keys.
			var keyUp = wParamVal == WM_KEYUP || wParamVal == WM_SYSKEYUP;
			var vk = lParam.vkCode;
			var sc = lParam.scanCode;

			//if (vk == 'B')
			//{
			//  int xx = 123;
			//}

			//if (vk == 'b')
			//{
			//  int xx = 123;
			//}

			//if (code != 0 && ((lParam.vkCode & VK_LSHIFT) == VK_LSHIFT || (lParam.vkCode & VK_RSHIFT) == VK_RSHIFT))
			//if (wParamVal > 0 && lParam.flags > 0 && lParam.vkCode != 0xA0)// (IsKeyDown(VK_LSHIFT) || IsKeyDown(VK_RSHIFT)))
			//{
			//  Console.WriteLine("shift");
			//}

			if (vk != 0 && sc == 0) // Might happen if another app calls keybd_event with a zero scan code.
				sc = MapVkToSc(vk);

			// MapVirtualKey() does *not* include 0xE0 in HIBYTE if key is extended.  In case it ever
			// does in the future (or if event.scanCode ever does), force sc to be an 8-bit value
			// so that it's guaranteed consistent and to ensure it won't exceed SC_MAX (which might cause
			// array indexes to be out-of-bounds).  The 9th bit is later set to 1 if the key is extended:
			sc &= 0xFF;

			// Change sc to be extended if indicated.  But avoid doing so for VK_RSHIFT, which is
			// apparently considered extended by the API when it shouldn't be.  Update: Well, it looks like
			// VK_RSHIFT really is an extended key, at least on WinXP (and probably be extension on the other
			// NT based OSes as well).  What little info I could find on the 'net about this is contradictory,
			// but it's clear that some things just don't work right if the non-extended scan code is sent.  For
			// example, the shift key will appear to get stuck down in the foreground app if the non-extended
			// scan code is sent with VK_RSHIFT key-up event:
			if ((lParam.flags & LLKHF_EXTENDED) != 0) // && vk != VK_RSHIFT)
				sc |= 0x100;

			// The below must be done prior to any returns that indirectly call UpdateKeybdState() to update
			// modifier state.
			// Update: It seems best to do the below unconditionally, even if the OS is Win2k or WinXP,
			// since it seems like this translation will add value even in those cases:
			// To help ensure consistency with Windows XP and 2k, for which this hook has been primarily
			// designed and tested, translate neutral modifier keys into their left/right specific VKs,
			// since beardboy's testing shows that NT4 receives the neutral keys like Win9x does:
			switch (vk)
			{
				case VK_SHIFT: vk = (sc == RShift) ? VK_RSHIFT : VK_LSHIFT; break;

				case VK_CONTROL: vk = (sc == RControl) ? VK_RCONTROL : VK_LCONTROL; break;

				case VK_MENU: vk = (sc == RAlt) ? VK_RMENU : VK_LMENU; break;
			}

			if (lParam.scanCode == FakeLControl &&  kbdMsSender.altGrExtraInfo != 0 && (lParam.flags & LLKHF_INJECTED) != 0)
			{
				// This LCtrl is a result of sending RAlt, which hasn't been received yet.
				// Override dwExtraInfo, though it will only affect this hook instance.
				lParam.dwExtraInfo = kbdMsSender.altGrExtraInfo;
			}

			MSDLLHOOKSTRUCT tempstruct = default;
			return LowLevelCommon(kbdHook, code, wParamVal, ref lParam, ref tempstruct, vk, sc, keyUp, lParam.dwExtraInfo, lParam.flags);
		}

		private void SendHotkeyMessages(bool keyUp, ulong extraInfo, KeyHistoryItem keyHistoryCurr, uint hotkeyIDToPost, HotkeyVariant variant, HotstringDefinition hs, CaseConformModes caseConformMode, char endChar)
		{
			if (hotkeyIDToPost != HotkeyDefinition.HOTKEY_ID_INVALID)
			{
				var inputLevel = InputLevelFromInfo(extraInfo);
				_ = channel.Writer.TryWrite(new KeysharpMsg()
				{
					message = (uint)UserMessages.AHK_HOOK_HOTKEY,
					wParam = new IntPtr(hotkeyIDToPost),//Would be so much better to eventually pass around object references rather than array indexes.//TODO
					lParam = new IntPtr(MakeLong((short)keyHistoryCurr.sc, (short)inputLevel)),
					obj = variant
				});

				if (keyUp && hotkeyUp[(int)hotkeyIDToPost & HotkeyDefinition.HOTKEY_ID_MASK] != HotkeyDefinition.HOTKEY_ID_INVALID)
				{
					// This is a key-down hotkey being triggered by releasing a prefix key.
					// There's also a corresponding key-up hotkey, so fire it too:
					_ = channel.Writer.TryWrite(new KeysharpMsg()
					{
						message = (uint)UserMessages.AHK_HOOK_HOTKEY,
						wParam = new IntPtr(hotkeyUp[(int)hotkeyIDToPost & HotkeyDefinition.HOTKEY_ID_MASK]),
						lParam = new IntPtr(MakeLong((short)keyHistoryCurr.sc, (short)inputLevel))
						//Do not pass the variant.
					});
				}
			}

			if (hs != null)
			{
				_ = channel.Writer.TryWrite(new KeysharpMsg()
				{
					message = (uint)UserMessages.AHK_HOTSTRING,
					obj = new HotstringMsg()
					{
						hs = hs,
						caseMode = caseConformMode,
						endChar = endChar
					}
				});
			}
		}

		//protected internal override void DeregisterMouseHook()
		//{
		//  _ = WindowsAPI.UnhookWindowsHookEx(mouseHook);
		//}
		private bool SystemHasAnotherHook(ref Mutex existingMutex, string name)
		{
			if (existingMutex != null)
				existingMutex.Close(); // But don't set it to NULL because we need its value below as a flag.

			var mutex = new Mutex(false, name, out var _); // Create() vs. Open() has enough access to open the mutex if it exists.
			var last_error = GetLastError();

			// Don't check g_KeybdHook because in the case of aChangeIsTemporary, it might be NULL even though
			// we want a handle to the mutex maintained here.
			if (existingMutex != null) // It was open originally, so update the handle the the newly opened one.
				existingMutex = mutex;
			else if (mutex != null) // Keep it closed because the system tracks how many handles there are, deleting the mutex when zero.
				mutex.Close();  // This facilitates other instances of the program getting the proper last_error value.

			return last_error == ERROR_ALREADY_EXISTS;
		}

		private class DeadKeyRecord
		{
			internal uint caps;
			internal uint modLR;
			internal uint sc;
			internal uint vk;
		}
	}

	internal enum GuiEventKinds
	{ GUI_EVENTKIND_EVENT = 0, GUI_EVENTKIND_NOTIFY, GUI_EVENTKIND_COMMAND }

	internal enum GuiEventTypes
	{
		GUI_EVENT_NONE  // NONE must be zero for any uses of ZeroMemory(), synonymous with false, etc.
		, GUI_EVENT_DROPFILES, GUI_EVENT_CLOSE, GUI_EVENT_ESCAPE, GUI_EVENT_RESIZE, GUI_EVENT_CONTEXTMENU
		, GUI_EVENT_WINDOW_FIRST = GUI_EVENT_DROPFILES, GUI_EVENT_WINDOW_LAST = GUI_EVENT_CONTEXTMENU
		, GUI_EVENT_CONTROL_FIRST
		, GUI_EVENT_CHANGE = GUI_EVENT_CONTROL_FIRST
		, GUI_EVENT_CLICK, GUI_EVENT_DBLCLK, GUI_EVENT_COLCLK
		, GUI_EVENT_ITEMCHECK, GUI_EVENT_ITEMSELECT, GUI_EVENT_ITEMFOCUS, GUI_EVENT_ITEMEXPAND
		, GUI_EVENT_ITEMEDIT
		, GUI_EVENT_FOCUS, GUI_EVENT_LOSEFOCUS
		, GUI_EVENT_NAMED_COUNT

		// The rest don't have explicit names in GUI_EVENT_NAMES:
		, GUI_EVENT_WM_COMMAND = GUI_EVENT_NAMED_COUNT
	};
}

#endif