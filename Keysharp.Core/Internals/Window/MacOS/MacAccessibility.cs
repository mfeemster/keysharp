using Keysharp.Builtins;
#if OSX
using System.Runtime.InteropServices;
using static Keysharp.Internals.AppleEvents.CF;

namespace Keysharp.Internals.Window.MacOS
{
	internal static partial class MacAccessibility
	{
		private const int kCFNumberSInt32Type = 3;
		private const int kAXErrorSuccess = 0;
		private const int kAXErrorInvalidUIElement = -25202;
		private const int kAXErrorCannotComplete = -25204;
		private const int kAXValueCGPointType = 1;
		private const int kAXValueCGSizeType = 2;
		private const int kAXValueCGRectType = 3;
		private const int kAXValueCFRangeType = 4;
		private const uint kCGHIDEventTap = 0;
		private const float WindowMessagingTimeout = 0.2f;
		private const int WindowStateTransitionTimeout = 2500;
		// An application answers no accessibility request while AppKit animates one of its windows, about 540 ms for a
		// minimize or restore. A query that times out for a reason this script did not cause holds it off for about as long.
		private const int WindowAnimationMs = 750;
		private const int UnresponsiveBackoffMs = 500;
		// Queries fall back to the state last seen instead of waiting; a write to a busy application only has to be
		// delivered, because AppKit applies it once the animation ends.
		private const float QueryMessagingTimeout = 0.05f;
		private const float BusyWriteTimeout = 0.02f;

		private enum CGEventType : uint
		{
			LeftMouseDown = 1,
			LeftMouseUp = 2,
			RightMouseDown = 3,
			RightMouseUp = 4,
			OtherMouseDown = 25,
			OtherMouseUp = 26
		}

		private enum CGMouseButton : uint
		{
			Left = 0,
			Right = 1,
			Center = 2,
			Extra1 = 3,
			Extra2 = 4
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CGPointD
		{
			public double X;
			public double Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CGSizeD
		{
			public double Width;
			public double Height;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CGRectD
		{
			public CGPointD Origin;
			public CGSizeD Size;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CFRangeNative
		{
			public nint Location;
			public nint Length;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CGPointNative
		{
			public double X;
			public double Y;

			public CGPointNative(double x, double y)
			{
				X = x;
				Y = y;
			}
		}

			private static readonly nint attrWindows = CreateCFString("AXWindows");
			private static readonly nint attrFocusedApplication = CreateCFString("AXFocusedApplication");
			private static readonly nint attrFocusedWindow = CreateCFString("AXFocusedWindow");
			private static readonly nint attrFocusedUIElement = CreateCFString("AXFocusedUIElement");
			private static readonly nint attrSelectedTextRange = CreateCFString("AXSelectedTextRange");
			private static readonly nint attrBoundsForRange = CreateCFString("AXBoundsForRange");
			private static readonly nint attrWindow = CreateCFString("AXWindow");
			private static readonly nint attrTitle = CreateCFString("AXTitle");
			private static readonly nint attrPosition = CreateCFString("AXPosition");
			private static readonly nint attrSize = CreateCFString("AXSize");
			private static readonly nint attrMinimized = CreateCFString("AXMinimized");
			private static readonly nint attrFullScreen = CreateCFString("AXFullScreen");
			private static readonly nint attrFullScreenButton = CreateCFString("AXFullScreenButton");
			private static readonly nint attrCloseButton = CreateCFString("AXCloseButton");

		private static readonly nint actionRaise = CreateCFString("AXRaise");
		private static readonly nint actionClose = CreateCFString("AXClose");
		private static readonly nint actionPress = CreateCFString("AXPress");
		private static readonly nint cfBoolTrue = ResolvePointerConstant("kCFBooleanTrue");
		private static readonly nint cfBoolFalse = ResolvePointerConstant("kCFBooleanFalse");
		private static readonly nint axTrustedCheckOptionPrompt = ResolveAppServicesPointerSymbol("kAXTrustedCheckOptionPrompt");
		private static readonly nint windowMetadataAttributes = CFArrayCreate(0, [attrTitle, attrPosition, attrSize], 3, 0);
		private static readonly nint windowStateAttributes = CFArrayCreate(0, [attrMinimized, attrFullScreen], 2, 0);

		private static int loggedTrustFailure;
		private static int loggedListenFailure;
		private static int loggedPostFailure;
		private static int loggedScreenFailure;
		private static int promptedTrust;
		private static int promptedListen;
		private static int promptedPost;
		private static int promptedScreen;
		private static bool listenGranted, postGranted, screenGranted;

			[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
			private static partial nint AXUIElementCreateApplication(int pid);

		// Private, but the only exact map from an accessibility window to its CGWindowID, and what window managers
		// rely on. Where it is missing, a window is matched by frame and title instead.
		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices", EntryPoint = "_AXUIElementGetWindow")]
		private static partial int AXUIElementGetWindow(nint element, out uint windowId);
		private static int windowIdUnavailable;

			[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
			private static partial nint AXUIElementCreateSystemWide();

			[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
			private static partial int AXUIElementCopyAttributeValue(nint element, nint attribute, out nint value);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial int AXUIElementCopyMultipleAttributeValues(nint element, nint attributes, uint options, out nint values);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial int AXUIElementGetPid(nint element, out int pid);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial nint AXValueGetTypeID();

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial int AXUIElementCopyParameterizedAttributeValue(nint element, nint parameterizedAttribute,
			nint parameter, out nint value);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial int AXUIElementSetAttributeValue(nint element, nint attribute, nint value);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial int AXUIElementIsAttributeSettable(nint element, nint attribute, [MarshalAs(UnmanagedType.I1)] out bool settable);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial int AXUIElementPerformAction(nint element, nint action);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool AXIsProcessTrustedWithOptions(nint options);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool CGPreflightListenEventAccess();

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool CGRequestListenEventAccess();

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool CGPreflightPostEventAccess();

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool CGRequestPostEventAccess();

		[LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool CGPreflightScreenCaptureAccess();

		[LibraryImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool CGRequestScreenCaptureAccess();

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool AXValueGetValue(nint value, int theType, out CGPointD point);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool AXValueGetValue(nint value, int theType, out CGSizeD size);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool AXValueGetValue(nint value, int theType, out CGRectD rect);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		[return: MarshalAs(UnmanagedType.I1)]
		private static partial bool AXValueGetValue(nint value, int theType, out CFRangeNative range);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices", EntryPoint = "AXValueCreate")]
		private static partial nint AXValueCreatePoint(int theType, in CGPointD point);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices", EntryPoint = "AXValueCreate")]
		private static partial nint AXValueCreateSize(int theType, in CGSizeD size);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices", EntryPoint = "AXValueCreate")]
		private static partial nint AXValueCreateRange(int theType, in CFRangeNative range);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial nint CGEventCreateMouseEvent(nint source, CGEventType mouseType, CGPointNative mouseCursorPosition, CGMouseButton mouseButton);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial void CGEventPost(uint tap, nint @event);

		[LibraryImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
		private static partial void CGEventSetIntegerValueField(nint @event, uint field, long value);

		internal sealed class WindowElement : IDisposable
		{
			internal nint Element { get; private set; }
			internal MacNativeWindow Info { get; }

			internal WindowElement(nint element, MacNativeWindow info)
			{
				Element = element;
				Info = info;
			}

			public void Dispose()
			{
				if (Element != 0)
				{
					CFRelease(Element);
					Element = 0;
				}
			}
		}

		/// <summary>
		/// What a script knows about foreign windows: each window's exact accessibility element, its last confirmed
		/// state and the state a request of this script has yet to reach, and the applications busy animating a window,
		/// which answer nothing until they finish.
		/// </summary>
		internal sealed class WindowRegistry
		{
			private sealed class KnownWindow
			{
				internal nint Element;
				internal FormWindowState? State, Requested;
			}

			private readonly Dictionary<(int, uint), KnownWindow> windows = new();
			private readonly Dictionary<int, long> busyUntil = new();
			private bool closed;

			internal bool IsBusy(int pid)
			{
				lock (windows)
					return busyUntil.TryGetValue(pid, out var until) && Environment.TickCount64 < until;
			}

			// `queued` starts the period when the one already known ends, for a request AppKit runs after the current
			// animation.
			internal void MarkBusy(int pid, int milliseconds, bool queued = false)
			{
				lock (windows)
				{
					var now = Environment.TickCount64;
					var known = busyUntil.GetValueOrDefault(pid);
					// A request from an earlier busy period has played out, whether or not a query confirmed it.
					if (known <= now)
						foreach (var (key, window) in windows)
							if (key.Item1 == pid)
								window.Requested = null;
					busyUntil[pid] = Math.Max(known, (queued ? Math.Max(known, now) : now) + milliseconds);
				}
			}

			// Retained for the caller, or 0.
			internal nint RetainElement(MacNativeWindow info)
			{
				lock (windows)
					return windows.GetValueOrDefault((info.OwnerPid, info.WindowNumber)) is { } known && known.Element != 0
						? CFRetain(known.Element) : 0;
			}

			internal void Confirm(MacNativeWindow info, FormWindowState state, nint element)
			{
				lock (windows)
					if (Get(info) is { } known)
					{
						known.State = state;
						known.Requested = null;
						if (element != 0 && known.Element == 0)
							known.Element = CFRetain(element);
					}
			}

			internal void Request(MacNativeWindow info, FormWindowState state)
			{
				lock (windows)
					if (Get(info) is { } known)
						known.Requested = state;
			}

			// The state last confirmed, or with `pending` the state a request already made will reach.
			internal bool TryGetState(MacNativeWindow info, bool pending, out FormWindowState state)
			{
				lock (windows)
				{
					var known = windows.GetValueOrDefault((info.OwnerPid, info.WindowNumber));
					var value = pending ? known?.Requested ?? known?.State : known?.State;
					state = value.GetValueOrDefault();
					return value.HasValue;
				}
			}

			internal void Forget(MacNativeWindow info, nint element)
			{
				lock (windows)
					if (windows.GetValueOrDefault((info.OwnerPid, info.WindowNumber)) is { } known && known.Element == element)
					{
						known.Element = 0;
						CFRelease(element);
					}
			}

			internal void Close()
			{
				lock (windows)
				{
					closed = true;
					Release();
				}
			}

			// CGWindowIDs are never reused, so a full registry is emptied rather than pruned; live windows are learned again.
			private KnownWindow Get(MacNativeWindow info)
			{
				if (closed)
					return null;
				if (!windows.TryGetValue((info.OwnerPid, info.WindowNumber), out var known))
				{
					if (windows.Count >= 256)
						Release();
					windows[(info.OwnerPid, info.WindowNumber)] = known = new KnownWindow();
				}
				return known;
			}

			private void Release()
			{
				foreach (var known in windows.Values)
					if (known.Element != 0)
						CFRelease(known.Element);
				windows.Clear();
			}
		}

		private static WindowRegistry Windows => Script.TheScript.MacWindows;

		/// <summary>
		/// The accessibility element of a window, found afresh once its application is not busy. A request that only
		/// has to be delivered (<paramref name="request"/>) takes the element already known for a busy application,
		/// which cannot list its windows until its animation ends. <paramref name="waitForWindow"/> also waits for a
		/// window the application has yet to list, as AX and window-server frames can disagree mid-animation.
		/// </summary>
		internal static WindowElement ResolveWindowElement(MacNativeWindow info, string operation, bool prompt = true,
			bool waitForWindow = false, bool request = false)
		{
			if (!EnsureAccessibilityAccess(operation, prompt))
				return null;
			if (request && Windows.IsBusy(info.OwnerPid) && Windows.RetainElement(info) is var known && known != 0)
				return new WindowElement(known, info);
			if (Windows.IsBusy(info.OwnerPid))
				_ = Flow.WaitUntil(() => !Windows.IsBusy(info.OwnerPid), WindowStateTransitionTimeout, 10);
			if (TryFindWindowElement(info, out var element, out var status, out _))
				return new WindowElement(element, info);
			if (!waitForWindow && status != kAXErrorCannotComplete)
				return null;

			var live = info;
			_ = Flow.WaitUntil(() => !MacNativeWindows.TryGetWindowInfo((nint)info.WindowNumber, out live)
				|| live.OwnerPid != info.OwnerPid || TryFindWindowElement(live, out element, out status, out _)
				|| !waitForWindow && status != kAXErrorCannotComplete,
				WindowStateTransitionTimeout, 25);
			return element != 0 ? new WindowElement(element, live) : null;
		}

		internal static bool TryActivateWindow(WindowElement window)
		{
			if (window == null)
				return false;

			if (TryGetWindowState(window, out var state) && state == FormWindowState.Minimized
				&& !TrySetWindowState(window, FormWindowState.Normal))
				return false;

			return MacNativeWindows.ActivateAppByPid(window.Info.OwnerPid) && TryRaiseWindow(window);
		}

		internal static bool TryRaiseWindow(WindowElement window)
			=> window != null && AXUIElementPerformAction(window.Element, actionRaise) == kAXErrorSuccess;

		internal static bool TrySetWindowTitle(WindowElement window, string title)
		{
			if (window == null || !IsAttributeSettable(window.Element, attrTitle))
				return false;

			var titleRef = CreateString(title);
			if (titleRef == 0)
				return false;

			try
			{
				return AXUIElementSetAttributeValue(window.Element, attrTitle, titleRef) == kAXErrorSuccess;
			}
			finally
			{
				CFRelease(titleRef);
			}
		}

		internal static bool TryCloseWindow(WindowElement window)
			=> window != null && (AXUIElementPerformAction(window.Element, actionClose) == kAXErrorSuccess
				|| TryPressButton(window.Element, attrCloseButton));

		internal static bool TryGetWindowState(MacNativeWindow info, out FormWindowState state)
		{
			state = FormWindowState.Normal;
			if (!EnsureAccessibilityAccess("query window state", prompt: false))
				return false;

			// Asking a busy application would stall the caller until its animation ends, so what was last seen stands.
			if (Windows.IsBusy(info.OwnerPid))
				return Windows.TryGetState(info, pending: false, out state);

			var element = Windows.RetainElement(info);
			var cached = element != 0;
			var exact = cached;
			if (!cached && !TryFindWindowElement(info, out element, out var findStatus, out exact, QueryMessagingTimeout))
				return findStatus == kAXErrorCannotComplete && Windows.TryGetState(info, pending: false, out state);

			using var window = new WindowElement(element, info);
			_ = AXUIElementSetMessagingTimeout(element, QueryMessagingTimeout);
			var read = TryGetWindowState(window, out state, out var status);
			_ = AXUIElementSetMessagingTimeout(element, WindowMessagingTimeout);
			if (read)
				Windows.Confirm(info, state, exact ? element : 0);
			else if (cached && status == kAXErrorInvalidUIElement)
			{
				// A window shown again after being ordered out has a new element under the same CGWindowID.
				Windows.Forget(info, element);
				return TryGetWindowState(info, out state);
			}

			return read || status == kAXErrorCannotComplete && Windows.TryGetState(info, pending: false, out state);
		}

		internal static bool TryGetWindowState(WindowElement window, out FormWindowState state)
			=> TryGetWindowState(window, out state, out _);

		private static bool TryGetWindowState(WindowElement window, out FormWindowState state, out int status)
		{
			state = FormWindowState.Normal;
			status = kAXErrorSuccess;
			if (window == null)
				return false;

			// Both attributes share one round trip to the owning application.
			status = AXUIElementCopyMultipleAttributeValues(window.Element, windowStateAttributes, 0, out var values);
			if (status == kAXErrorCannotComplete)
				Windows.MarkBusy(window.Info.OwnerPid, UnresponsiveBackoffMs);
			try
			{
				if (status != kAXErrorSuccess || values == 0 || CFArrayGetCount(values) != 2)
					return false;

				var knowsMinimized = TryReadBool(CFArrayGetValueAtIndex(values, 0), out var minimized);
				var knowsFullScreen = TryReadBool(CFArrayGetValueAtIndex(values, 1), out var full);
				if (knowsMinimized && minimized)
					state = FormWindowState.Minimized;
				else if (knowsFullScreen && full)
					state = FormWindowState.Maximized;

				return knowsMinimized || knowsFullScreen;
			}
			finally
			{
				if (values != 0)
					CFRelease(values);
			}
		}

		// Without `waitForChange` an ordinary minimize or restore is only requested; full-screen changes always wait,
		// because each step needs the one before it to finish.
		internal static bool TrySetWindowState(WindowElement window, FormWindowState state, bool waitForChange = true)
		{
			if (window == null)
				return false;

			FormWindowState current;
			// A busy application cannot be asked, but a request to it builds on the one it is still carrying out.
			if (!waitForChange && Windows.IsBusy(window.Info.OwnerPid))
			{
				// With nothing known, the request is delivered rather than dropped.
				if (!Windows.TryGetState(window.Info, pending: true, out current))
					current = state == FormWindowState.Minimized ? FormWindowState.Normal : FormWindowState.Minimized;
			}
			else if (!TryGetWindowState(window, out current))
				return false;

			var owner = Script.TheScript;
			var restores = owner.MacFullScreenRestoreWindows;
			WindowElement previous;
			lock (restores)
				restores.Remove((window.Info.OwnerPid, window.Info.WindowNumber), out previous);
			using var saved = previous;
			var restoreFullScreen = current == FormWindowState.Minimized && saved != null && CFEqual(saved.Element, window.Element);

			if (state == FormWindowState.Minimized)
			{
				if (current == FormWindowState.Minimized)
				{
					if (restoreFullScreen)
						RememberFullScreenRestore(owner, window);
					return true;
				}
				if (current != FormWindowState.Maximized)
				{
					if (!waitForChange)
						return RequestMinimized(window, true);
					if (!IsAttributeSettable(window.Element, attrMinimized))
						return false;
					return ChangeMinimized(window, true) || ReportWindowStateFailure("minimize the window");
				}

				if (!TryReadBool(window.Element, attrMinimized, out _) || !TrySetFullScreen(window, false))
					return false;
				// Miniaturization can be ignored while AppKit finishes its full-screen animation.
				if (!CompleteFullScreenChange(window, false) || !ChangeMinimized(window, true))
				{
					_ = TryWriteBool(window.Element, attrMinimized, false);
					_ = TrySetFullScreen(window, true);
					return ReportWindowStateFailure("minimize the full-screen window");
				}
				RememberFullScreenRestore(owner, window);
				return true;
			}

			if (current == FormWindowState.Minimized && state == FormWindowState.Normal && !restoreFullScreen && !waitForChange)
				return RequestMinimized(window, false);

			if (current == FormWindowState.Minimized && !ChangeMinimized(window, false))
			{
				if (restoreFullScreen && TryReadBool(window.Element, attrMinimized, out var stillMinimized) && stillMinimized)
					RememberFullScreenRestore(owner, window);
				return ReportWindowStateFailure("unminimize the window");
			}
			if (state == FormWindowState.Maximized)
			{
				if (!TrySetFullScreen(window, true))
					return false;
				return CompleteFullScreenChange(window, true) || ReportWindowStateFailure("maximize the window");
			}
			if (restoreFullScreen)
			{
				if (!TrySetFullScreen(window, true) || !CompleteFullScreenChange(window, true))
				{
					if (TryReadBool(window.Element, attrFullScreen, out var fullScreen) && !fullScreen && ChangeMinimized(window, true))
						RememberFullScreenRestore(owner, window);
					return ReportWindowStateFailure("restore the minimized window to full-screen mode");
				}
				return true;
			}
			if (current != FormWindowState.Maximized)
				return true;
			if (!TrySetFullScreen(window, false))
				return false;
			return CompleteFullScreenChange(window, false) || ReportWindowStateFailure("leave full-screen mode");
		}

		private static bool ChangeMinimized(WindowElement window, bool minimized)
			=> Flow.WaitUntil(() => TryReadBool(window.Element, attrMinimized, out var current)
				&& (current == minimized || TryWriteBool(window.Element, attrMinimized, minimized)
					&& TryReadBool(window.Element, attrMinimized, out current) && current == minimized),
				WindowStateTransitionTimeout, 25);

		// The application animates the change and answers nothing meanwhile, so it is marked busy for that long.
		private static bool RequestMinimized(WindowElement window, bool minimized)
		{
			var pid = window.Info.OwnerPid;
			var busy = Windows.IsBusy(pid);
			if (!busy && minimized && !IsAttributeSettable(window.Element, attrMinimized))
				return false;
			if (busy)
				_ = AXUIElementSetMessagingTimeout(window.Element, BusyWriteTimeout);
			var status = AXUIElementSetAttributeValue(window.Element, attrMinimized, minimized ? cfBoolTrue : cfBoolFalse);
			if (busy)
				_ = AXUIElementSetMessagingTimeout(window.Element, WindowMessagingTimeout);

			if (status != kAXErrorSuccess && !(busy && status == kAXErrorCannotComplete))
				return ReportWindowStateFailure(minimized ? "minimize the window" : "restore the window", waitForChange: false);

			Windows.MarkBusy(pid, WindowAnimationMs, queued: busy);
			Windows.Request(window.Info, minimized ? FormWindowState.Minimized : FormWindowState.Normal);
			return true;
		}

		private static bool CompleteFullScreenChange(WindowElement window, bool fullScreen)
			=> Flow.WaitUntil(() => TryReadBool(window.Element, attrFullScreen, out var current)
				&& (current == fullScreen || IsAttributeSettable(window.Element, attrFullScreen)
					&& TryWriteBool(window.Element, attrFullScreen, fullScreen)
					&& TryReadBool(window.Element, attrFullScreen, out current) && current == fullScreen),
				WindowStateTransitionTimeout, 25);

		private static bool ReportWindowStateFailure(string operation, bool waitForChange = true)
		{
			var detail = waitForChange ? $" within {WindowStateTransitionTimeout} ms" : string.Empty;
			_ = Errors.OSErrorOccurredWithMessage($"macOS could not {operation}{detail}.");
			// Continuing after OSError must not make the caller report a second UnsupportedError.
			return true;
		}

		private static void RememberFullScreenRestore(Script owner, WindowElement window)
		{
			var restores = owner.MacFullScreenRestoreWindows;
			lock (restores)
			{
				if (owner.IsDisposed)
					return;
				if (restores.Remove((window.Info.OwnerPid, window.Info.WindowNumber), out var previous))
					previous.Dispose();
				restores[(window.Info.OwnerPid, window.Info.WindowNumber)] = new WindowElement(CFRetain(window.Element), window.Info);
			}
		}

		internal static void ForgetFullScreenRestore(Script owner, uint windowNumber, nint element)
		{
			if (owner == null)
				return;
			var restores = owner.MacFullScreenRestoreWindows;
			lock (restores)
				foreach (var entry in restores.Where(entry => entry.Key.WindowNumber == windowNumber
					&& CFEqual(entry.Value.Element, element)).ToArray())
				{
					restores.Remove(entry.Key);
					entry.Value.Dispose();
				}
		}

		internal static void ClearFullScreenRestores(Script owner)
		{
			var restores = owner.MacFullScreenRestoreWindows;
			lock (restores)
			{
				foreach (var window in restores.Values)
					window.Dispose();
				restores.Clear();
			}
		}

		internal static bool TrySetFullScreen(WindowElement window, bool on)
		{
			if (window == null)
				return false;

			var element = window.Element;
			var known = TryReadBool(element, attrFullScreen, out var current);
			if (known && current == on)
				return true;

			if (IsAttributeSettable(element, attrFullScreen) && TryWriteBool(element, attrFullScreen, on))
				return true;

			// The green button toggles, so press it only when the current direction is known.
			return known && current != on && TryPressButton(element, attrFullScreenButton);
		}

		private static bool TryPressButton(nint windowElement, nint buttonAttr)
		{
			if (!TryCopyAttributeValue(windowElement, buttonAttr, out var button))
				return false;

			try
			{
				return AXUIElementPerformAction(button, actionPress) == kAXErrorSuccess;
			}
			finally
			{
				CFRelease(button);
			}
		}

		internal static bool TryMoveResizeWindow(WindowElement window, Rectangle rect, bool setPosition, bool setSize)
		{
			if (window == null)
				return false;

			var element = window.Element;
			var ok = true;
			if (setPosition && IsAttributeSettable(element, attrPosition))
			{
				var point = new CGPointD { X = rect.X, Y = rect.Y };
				var value = AXValueCreatePoint(kAXValueCGPointType, in point);
				if (value == 0)
					ok = false;
				else
				{
					try { ok &= AXUIElementSetAttributeValue(element, attrPosition, value) == kAXErrorSuccess; }
					finally { CFRelease(value); }
				}
			}

			// A fixed-size window can still be moved; an unsettable size is not an error.
			if (setSize && rect.Width > 0 && rect.Height > 0 && IsAttributeSettable(element, attrSize))
			{
				var size = new CGSizeD { Width = rect.Width, Height = rect.Height };
				var value = AXValueCreateSize(kAXValueCGSizeType, in size);
				if (value == 0)
					ok = false;
				else
				{
					try { ok &= AXUIElementSetAttributeValue(element, attrSize, value) == kAXErrorSuccess; }
					finally { CFRelease(value); }
				}
			}

			return ok;
		}

		internal static bool TryClickWindow(WindowElement window, Point location, uint buttonNumber, int count)
		{
			if (window == null || !EnsurePostEventAccess("post mouse click", prompt: true))
				return false;

			if (buttonNumber < 1 || buttonNumber > 5)
			{
				_ = Errors.ValueErrorOccurred($"Invalid macOS mouse button '{buttonNumber}'. Expected 1, 2, 3, 4 or 5.");
				return false;
			}

			if (count <= 0)
				return true;

			if (!TryActivateWindow(window))
				return false;
			var info = window.Info;
			var point = new CGPointNative(info.Bounds.X + location.X, info.Bounds.Y + location.Y);
			var button = (CGMouseButton)(buttonNumber - 1);
			var downType = button == CGMouseButton.Left ? CGEventType.LeftMouseDown
				: button == CGMouseButton.Right ? CGEventType.RightMouseDown : CGEventType.OtherMouseDown;
			var upType = button == CGMouseButton.Left ? CGEventType.LeftMouseUp
				: button == CGMouseButton.Right ? CGEventType.RightMouseUp : CGEventType.OtherMouseUp;

			for (var i = 0; i < count; i++)
			{
				var down = CGEventCreateMouseEvent(0, downType, point, button);
				var up = CGEventCreateMouseEvent(0, upType, point, button);
				try
				{
					if (down == 0 || up == 0)
						return false;

					CGEventSetIntegerValueField(down, 1, i + 1L); // kCGMouseEventClickState
					CGEventSetIntegerValueField(up, 1, i + 1L);
					CGEventPost(kCGHIDEventTap, down);
					CGEventPost(kCGHIDEventTap, up);
				}
				finally
				{
					if (down != 0) CFRelease(down);
					if (up != 0) CFRelease(up);
				}
			}

			return true;
		}

		// The CG preflights cost about 10 ms of tccd IPC when another process is responsible for this one, so a grant is
		// kept for the life of the process; a denial is re-checked, so access granted later is seen. Accessibility trust
		// is answered in-process and needs no cache.
		private static bool Preflight(ref bool granted, Func<bool> check, bool refresh)
			=> !refresh && granted || (granted = check());

		internal static bool EnsureAccessibilityAccess(string operation, bool prompt = false)
		{
			if (AXIsProcessTrustedWithOptions(0))
				return true;

			if (prompt && Interlocked.Exchange(ref promptedTrust, 1) == 0)
			{
				try
				{
					var options = CreateAccessibilityPromptOptions();
					try
					{
						if (options != 0 && AXIsProcessTrustedWithOptions(options))
							return true;
					}
					finally
					{
						if (options != 0)
							CFRelease(options);
					}
				}
				catch
				{
				}

				if (Keysharp.Internals.Flow.WaitUntil(() => AXIsProcessTrustedWithOptions(0), 60_000, 500))
					return true;
			}

			if (Interlocked.Exchange(ref loggedTrustFailure, 1) == 0)
			{
				Diagnostics.Debug.WriteLine(
					$"macOS Accessibility permission is required for '{operation}'. " +
					"Grant access in System Settings -> Privacy & Security -> Accessibility, then restart the app.");
			}

			return false;
		}

		internal static bool EnsureInputMonitoringAccess(string operation, bool prompt = false, bool refresh = false)
		{
			if (Preflight(ref listenGranted, CheckListenAccess, refresh))
				return true;

			if (prompt && Interlocked.Exchange(ref promptedListen, 1) == 0)
			{
				try
				{
					_ = CGRequestListenEventAccess();
				}
				catch (EntryPointNotFoundException)
				{
					return true;
				}
				catch
				{
				}

				if (Keysharp.Internals.Flow.WaitUntil(CheckListenAccess, 60_000, 500))
					return true;
			}

			if (Interlocked.Exchange(ref loggedListenFailure, 1) == 0)
			{
				Diagnostics.Debug.WriteLine(
					$"macOS Input Monitoring permission is required for '{operation}'. " +
					"Grant access in System Settings -> Privacy & Security -> Input Monitoring, then restart the app.");
			}

			return false;
		}

		internal static bool EnsurePostEventAccess(string operation, bool prompt = false, bool refresh = false)
		{
			if (Preflight(ref postGranted, CheckPostAccess, refresh))
				return true;

			if (prompt && Interlocked.Exchange(ref promptedPost, 1) == 0)
			{
				try
				{
					_ = CGRequestPostEventAccess();
				}
				catch (EntryPointNotFoundException)
				{
					// Older macOS: this API may be unavailable; Accessibility trust is authoritative there.
					return AXIsProcessTrustedWithOptions(0);
				}
				catch
				{
				}

				if (Keysharp.Internals.Flow.WaitUntil(CheckPostAccess, 60_000, 500))
					return true;
			}

			if (Interlocked.Exchange(ref loggedPostFailure, 1) == 0)
			{
				Diagnostics.Debug.WriteLine(
					$"macOS synthetic input permission is required for '{operation}'. " +
					"Grant access in System Settings -> Privacy & Security -> Accessibility, then restart the app.");
			}

			return false;
		}

		internal static bool EnsureScreenCaptureAccess(string operation, bool prompt = false, bool refresh = false)
		{
			if (Preflight(ref screenGranted, CheckScreenCaptureAccess, refresh))
				return true;

			if (prompt && Interlocked.Exchange(ref promptedScreen, 1) == 0)
			{
				try
				{
					_ = CGRequestScreenCaptureAccess();
				}
				catch (EntryPointNotFoundException)
				{
					// Older macOS: no dedicated API; do not block here.
					return true;
				}
				catch
				{
				}

				if (Keysharp.Internals.Flow.WaitUntil(CheckScreenCaptureAccess, 60_000, 500))
					return true;
			}

			if (Interlocked.Exchange(ref loggedScreenFailure, 1) == 0)
			{
				Diagnostics.Debug.WriteLine(
					$"macOS Screen Recording permission is required for '{operation}'. " +
					"Grant access in System Settings -> Privacy & Security -> Screen Recording, then restart the app.");
			}

			return false;
		}

			private static bool CheckListenAccess()
			{
				try
				{
					return CGPreflightListenEventAccess();
			}
			catch (EntryPointNotFoundException)
			{
				// Older macOS: treat Accessibility trust as the closest equivalent.
				return AXIsProcessTrustedWithOptions(0);
			}
				catch
				{
					return false;
				}
			}

			internal static bool TryGetCaretScreenPosition(out int x, out int y)
			{
				x = 0;
				y = 0;

				if (!EnsureAccessibilityAccess("query caret position"))
					return false;

				var systemElement = AXUIElementCreateSystemWide();

				if (systemElement == 0)
					return false;

				try
				{
					if (!TryCopyAttributeValue(systemElement, attrFocusedUIElement, out var focusedElement))
						return false;

					try
					{
						if (!TryGetCaretRect(focusedElement, out var caret))
							return false;

						x = caret.X;
						y = caret.Y;
						return true;
					}
					finally
					{
						CFRelease(focusedElement);
					}
				}
				finally
				{
					CFRelease(systemElement);
				}
			}

			/// <summary>The caret (insertion point) rectangle of a text element, in screen coordinates. The element's
			/// selected text range is collapsed to a zero-length range at the insertion end, which makes
			/// AXBoundsForRange report the caret itself rather than the bounds of any selected text. Elements that
			/// aren't text — or that don't expose these attributes — return false rather than an empty rectangle.
			/// Shared by <see cref="TryGetCaretScreenPosition"/> (which asks the system-wide focused element) and the
			/// AXSelectedTextChanged observer behind <c>WinEvent.OnCaretMove</c> (which asks the notified element), so
			/// the query and the event always report the same position.</summary>
			internal static bool TryGetCaretRect(nint element, out Rectangle rect)
			{
				rect = Rectangle.Empty;

				if (element == 0 || !TryCopyAttributeValue(element, attrSelectedTextRange, out var selectedRangeValue))
					return false;

				try
				{
					if (!AXValueGetValue(selectedRangeValue, kAXValueCFRangeType, out CFRangeNative selectedRange))
						return false;
					if (selectedRange.Location < 0 || selectedRange.Length < 0
							|| selectedRange.Location > nint.MaxValue - selectedRange.Length)
						return false;

					// Ask for a zero-length range at the insertion end. AXBoundsForRange then
					// returns the caret rectangle instead of the bounds of selected text.
					selectedRange.Location += selectedRange.Length;
					selectedRange.Length = 0;
					var caretRangeValue = AXValueCreateRange(kAXValueCFRangeType, in selectedRange);

					if (caretRangeValue == 0)
						return false;

					try
					{
						if (!TryCopyParameterizedAttributeValue(element, attrBoundsForRange,
								caretRangeValue, out var boundsValue))
							return false;

						try
						{
							if (!AXValueGetValue(boundsValue, kAXValueCGRectType, out CGRectD bounds))
								return false;
							if (!double.IsFinite(bounds.Origin.X) || !double.IsFinite(bounds.Origin.Y)
									|| !double.IsFinite(bounds.Size.Width) || !double.IsFinite(bounds.Size.Height)
									|| bounds.Size.Width < 0 || bounds.Size.Height < 0)
								return false;

							var left = Math.Round(bounds.Origin.X);
							var top = Math.Round(bounds.Origin.Y);
							var width = Math.Round(bounds.Size.Width);
							var height = Math.Round(bounds.Size.Height);

							if (left < int.MinValue || left > int.MaxValue || top < int.MinValue || top > int.MaxValue
									|| width > int.MaxValue || height > int.MaxValue)
								return false;

							rect = new Rectangle((int)left, (int)top, (int)width, (int)height);
							return true;
						}
						finally
						{
							CFRelease(boundsValue);
						}
					}
					finally
					{
						CFRelease(caretRangeValue);
					}
				}
				finally
				{
					CFRelease(selectedRangeValue);
				}
			}

		internal static bool TryGetFocusedWindowHandle(out nint handle)
		{
			handle = 0;
			if (!EnsureAccessibilityAccess("query active window"))
				return false;

			var systemElement = AXUIElementCreateSystemWide();
			if (systemElement == 0)
				return false;

			try
			{
				if (!TryCopyAttributeValue(systemElement, attrFocusedApplication, out var appElement))
					return false;

				try
				{
					_ = AXUIElementSetMessagingTimeout(appElement, WindowMessagingTimeout);
					if (!TryCopyAttributeValue(appElement, attrFocusedWindow, out var focusedWindow))
						return false;

					try
					{
						if (!TryResolveWindowId(focusedWindow, out var id))
							return false;

						handle = (nint)id;
						return true;
					}
					finally
					{
						CFRelease(focusedWindow);
					}
				}
				finally
				{
					CFRelease(appElement);
				}
			}
			finally
			{
				CFRelease(systemElement);
			}
		}

			private static bool CheckPostAccess()
			{
				try
				{
					return CGPreflightPostEventAccess();
			}
			catch (EntryPointNotFoundException)
			{
				return AXIsProcessTrustedWithOptions(0);
			}
				catch
				{
					return false;
				}
			}

		private static bool CheckScreenCaptureAccess()
		{
			try
			{
				return CGPreflightScreenCaptureAccess();
			}
			catch (EntryPointNotFoundException)
			{
				return true;
			}
			catch
			{
				return false;
			}
		}

		// Success without a match means the application does not list the window, as with the Dock's. `exact` is false
		// for a frame and title match; `timeout` applies to every request made and stays on the returned element.
		private static bool TryFindWindowElement(MacNativeWindow info, out nint windowElement, out int status, out bool exact,
			float timeout = WindowMessagingTimeout)
		{
			windowElement = 0;
			exact = false;
			status = kAXErrorSuccess;
			var appElement = AXUIElementCreateApplication(info.OwnerPid);
			if (appElement == 0)
				return false;

			try
			{
				_ = AXUIElementSetMessagingTimeout(appElement, timeout);
				status = AXUIElementCopyAttributeValue(appElement, attrWindows, out var windowsArray);
				if (status == kAXErrorCannotComplete)
					Windows.MarkBusy(info.OwnerPid, UnresponsiveBackoffMs);
				if (status != kAXErrorSuccess || windowsArray == 0)
				{
					if (windowsArray != 0)
						CFRelease(windowsArray);
					return false;
				}

				try
				{
					var count = CFArrayGetCount(windowsArray);
					var match = new MacWindowMatch(info.Bounds, info.Title);
					for (nint i = 0; i < count; i++)
					{
						var entry = CFArrayGetValueAtIndex(windowsArray, i);
						if (entry == 0)
							continue;

						// AX timeouts belong to each object; the application's timeout does not cover its windows.
						_ = AXUIElementSetMessagingTimeout(entry, timeout);
						if (TryGetWindowId(entry, out var id))
						{
							if (id != info.WindowNumber)
								continue;

							windowElement = CFRetain(entry);
							exact = true;
							return windowElement != 0;
						}

						if (TryReadWindowMetadata(entry, out var bounds, out var title))
							match.Consider(entry, bounds, title);
					}

					if (match.Handle == 0)
						return false;

					windowElement = CFRetain(match.Handle);
					return windowElement != 0;
				}
				finally
				{
					CFRelease(windowsArray);
				}
			}
			finally
			{
				CFRelease(appElement);
			}
		}

		private static bool TryGetWindowId(nint element, out uint id)
		{
			id = 0;
			if (Volatile.Read(ref windowIdUnavailable) != 0)
				return false;

			try
			{
				return AXUIElementGetWindow(element, out id) == kAXErrorSuccess && id != 0;
			}
			catch (EntryPointNotFoundException)
			{
				Volatile.Write(ref windowIdUnavailable, 1);
				return false;
			}
		}

		private static bool TryResolveWindowId(nint windowElement, out uint id)
		{
			id = 0;
			if (windowElement == 0)
				return false;

			_ = AXUIElementSetMessagingTimeout(windowElement, WindowMessagingTimeout);
			if (TryGetWindowId(windowElement, out id))
				return true;

			if (AXUIElementGetPid(windowElement, out var pid) != kAXErrorSuccess || pid <= 0
				|| !TryReadWindowMetadata(windowElement, out var bounds, out var title))
				return false;

			return MacNativeWindows.TryMatchWindow(MacNativeWindows.Snapshot(), pid, bounds, title, out id);
		}

		private static bool TryReadWindowMetadata(nint element, out Rectangle bounds, out string title)
		{
			bounds = Rectangle.Empty;
			title = string.Empty;
			if (windowMetadataAttributes == 0)
				return false;

			var status = AXUIElementCopyMultipleAttributeValues(element, windowMetadataAttributes, 0, out var values);
			try
			{
				if (status != kAXErrorSuccess || values == 0 || CFArrayGetCount(values) != 3)
					return false;

				var titleValue = CFArrayGetValueAtIndex(values, 0);
				if (titleValue != 0 && CFGetTypeID(titleValue) == CFStringGetTypeID())
					title = ReadString(titleValue);

				var positionValue = CFArrayGetValueAtIndex(values, 1);
				var sizeValue = CFArrayGetValueAtIndex(values, 2);
				if (positionValue == 0 || sizeValue == 0
					|| CFGetTypeID(positionValue) != AXValueGetTypeID() || CFGetTypeID(sizeValue) != AXValueGetTypeID()
					|| !AXValueGetValue(positionValue, kAXValueCGPointType, out CGPointD position)
					|| !AXValueGetValue(sizeValue, kAXValueCGSizeType, out CGSizeD size)
					|| !double.IsFinite(position.X) || !double.IsFinite(position.Y)
					|| !double.IsFinite(size.Width) || !double.IsFinite(size.Height)
					|| position.X < int.MinValue || position.X > int.MaxValue
					|| position.Y < int.MinValue || position.Y > int.MaxValue
					|| size.Width <= 0 || size.Width > int.MaxValue || size.Height <= 0 || size.Height > int.MaxValue)
					return false;

				bounds = new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y),
					Convert.ToInt32(size.Width), Convert.ToInt32(size.Height));
				return true;
			}
			finally
			{
				if (values != 0)
					CFRelease(values);
			}
		}

		private static bool TryReadRect(nint windowElement, out Rectangle rect)
		{
			rect = Rectangle.Empty;
			if (!TryReadPoint(windowElement, attrPosition, out var x, out var y))
				return false;

			if (!TryReadSize(windowElement, attrSize, out var w, out var h))
				return false;

			rect = new Rectangle((int)x, (int)y, (int)w, (int)h);
			return true;
		}

		private static bool TryReadPoint(nint element, nint attr, out double x, out double y)
		{
			x = 0;
			y = 0;
			if (!TryCopyAttributeValue(element, attr, out var value))
				return false;

			try
			{
				if (!AXValueGetValue(value, kAXValueCGPointType, out CGPointD p))
					return false;

				x = p.X;
				y = p.Y;
				return true;
			}
			finally
			{
				CFRelease(value);
			}
		}

		private static bool TryReadSize(nint element, nint attr, out double width, out double height)
		{
			width = 0;
			height = 0;
			if (!TryCopyAttributeValue(element, attr, out var value))
				return false;

			try
			{
				if (!AXValueGetValue(value, kAXValueCGSizeType, out CGSizeD s))
					return false;

				width = s.Width;
				height = s.Height;
				return true;
			}
			finally
			{
				CFRelease(value);
			}
		}

		private static bool TryReadBool(nint element, nint attr, out bool value)
		{
			value = false;
			if (!TryCopyAttributeValue(element, attr, out var obj))
				return false;

			try
			{
				return TryReadBool(obj, out value);
			}
			finally
			{
				CFRelease(obj);
			}
		}

		private static bool TryReadBool(nint value, out bool result)
		{
			result = false;
			if (value == 0)
				return false;

			var typeId = CFGetTypeID(value);
			if (typeId == CFBooleanGetTypeID())
			{
				result = CFBooleanGetValue(value);
				return true;
			}

			if (typeId == CFNumberGetTypeID() && CFNumberGetValue(value, kCFNumberSInt32Type, out int number))
			{
				result = number != 0;
				return true;
			}

			return false;
		}

		private static bool TryWriteBool(nint element, nint attr, bool value)
		{
			var boolRef = value ? cfBoolTrue : cfBoolFalse;
			return boolRef != 0 && AXUIElementSetAttributeValue(element, attr, boolRef) == kAXErrorSuccess;
		}

		private static nint CreateCFString(string value)
		{
			try
			{
				return CreateString(value);
			}
			catch
			{
				return 0;
			}
		}

		private static nint ResolveAppServicesPointerSymbol(string symbolName)
		{
			if (!NativeLibrary.TryLoad("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices", out var appServices))
				return 0;

			try
			{
				if (!NativeLibrary.TryGetExport(appServices, symbolName, out var symbol) || symbol == 0)
					return 0;

				return Marshal.ReadIntPtr(symbol);
			}
			finally
			{
				NativeLibrary.Free(appServices);
			}
		}

		private static nint CreateAccessibilityPromptOptions()
		{
			if (axTrustedCheckOptionPrompt == 0 || cfBoolTrue == 0)
				return 0;

			try
			{
				return CFDictionaryCreate(0, [axTrustedCheckOptionPrompt], [cfBoolTrue], 1, 0, 0);
			}
			catch
			{
				return 0;
			}
		}

		private static bool TryCopyAttributeValue(nint element, nint attr, out nint value)
		{
			value = 0;

			if (AXUIElementCopyAttributeValue(element, attr, out value) == kAXErrorSuccess && value != 0)
				return true;

			if (value != 0)
				CFRelease(value);

			value = 0;
			return false;
		}

		private static bool TryCopyParameterizedAttributeValue(nint element, nint attr, nint parameter, out nint value)
		{
			value = 0;

			if (AXUIElementCopyParameterizedAttributeValue(element, attr, parameter, out value) == kAXErrorSuccess
					&& value != 0)
				return true;

			if (value != 0)
				CFRelease(value);

			value = 0;
			return false;
		}

		private static bool IsAttributeSettable(nint element, nint attr)
		{
			return AXUIElementIsAttributeSettable(element, attr, out var settable) == kAXErrorSuccess && settable;
		}
	}
}
#endif
