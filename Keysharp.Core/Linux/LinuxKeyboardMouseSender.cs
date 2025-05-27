#if LINUX
namespace Keysharp.Core.Linux
{
	/// <summary>
	/// Concrete implementation of KeyboardMouseSender for the linux platfrom.
	/// </summary>
	internal class LinuxKeyboardMouseSender : Common.Keyboard.KeyboardMouseSender
	{
		private Dictionary<char, CachedKey> Cache;

		private StringBuilder Dummy = new StringBuilder();

		// Somehow needed to get strings from native X11
		private Dictionary<XKeys, Keys> Mapping;

		private XConnectionSingleton XConn;

		internal LinuxKeyboardMouseSender()
		{
		}

		public void SetupMapping()
		{
			Mapping = new Dictionary<XKeys, Keys>();
			Cache = new Dictionary<char, CachedKey>();
			Mapping.Add(XKeys.LeftAlt, Keys.LMenu);
			Mapping.Add(XKeys.RightAlt, Keys.RMenu);
			Mapping.Add(XKeys.LeftControl, Keys.LControlKey);
			Mapping.Add(XKeys.RightControl, Keys.RControlKey);
			Mapping.Add(XKeys.LeftSuper, Keys.LWin);
			Mapping.Add(XKeys.RightSuper, Keys.RWin);
			Mapping.Add(XKeys.LeftShift, Keys.LShiftKey);
			Mapping.Add(XKeys.RightShift, Keys.RShiftKey);
			Mapping.Add(XKeys.F1, Keys.F1);
			Mapping.Add(XKeys.F2, Keys.F2);
			Mapping.Add(XKeys.F3, Keys.F3);
			Mapping.Add(XKeys.F4, Keys.F4);
			Mapping.Add(XKeys.F5, Keys.F5);
			Mapping.Add(XKeys.F6, Keys.F6);
			Mapping.Add(XKeys.F7, Keys.F7);
			Mapping.Add(XKeys.F8, Keys.F8);
			Mapping.Add(XKeys.F9, Keys.F9);
			Mapping.Add(XKeys.F10, Keys.F10);
			// Missing: F11 (Caught by WM)
			Mapping.Add(XKeys.F12, Keys.F12);
			Mapping.Add(XKeys.Escape, Keys.Escape);
			Mapping.Add(XKeys.Tab, Keys.Tab);
			Mapping.Add(XKeys.CapsLock, Keys.CapsLock);
			Mapping.Add(XKeys.Tilde, Keys.Oemtilde);
			Mapping.Add(XKeys.Backslash, Keys.OemBackslash);
			Mapping.Add(XKeys.BackSpace, Keys.Back);
			Mapping.Add(XKeys.ScrollLock, Keys.Scroll);
			Mapping.Add(XKeys.Pause, Keys.Pause);
			Mapping.Add(XKeys.Insert, Keys.Insert);
			Mapping.Add(XKeys.Delete, Keys.Delete);
			Mapping.Add(XKeys.Home, Keys.Home);
			Mapping.Add(XKeys.End, Keys.End);
			Mapping.Add(XKeys.PageUp, Keys.PageUp);
			Mapping.Add(XKeys.PageDown, Keys.PageDown);
			Mapping.Add(XKeys.NumLock, Keys.NumLock);
			Mapping.Add(XKeys.SpaceBar, Keys.Space);
			Mapping.Add(XKeys.Return, Keys.Return);
			Mapping.Add(XKeys.Slash, Keys.OemQuestion);
			Mapping.Add(XKeys.Dot, Keys.OemPeriod);
			Mapping.Add(XKeys.Comma, Keys.Oemcomma);
			Mapping.Add(XKeys.Semicolon, Keys.OemSemicolon);
			Mapping.Add(XKeys.OpenSquareBracket, Keys.OemOpenBrackets);
			Mapping.Add(XKeys.CloseSquareBracket, Keys.OemCloseBrackets);
			Mapping.Add(XKeys.Up, Keys.Up);
			Mapping.Add(XKeys.Down, Keys.Down);
			Mapping.Add(XKeys.Right, Keys.Right);
			Mapping.Add(XKeys.Left, Keys.Left);
			// Not sure about these ....
			Mapping.Add(XKeys.Dash, Keys.OemMinus);
			Mapping.Add(XKeys.Equals, Keys.Oemplus);
			// No windows equivalent?
			Mapping.Add(XKeys.NumpadSlash, Keys.None);
			Mapping.Add(XKeys.NumpadAsterisk, Keys.None);
			Mapping.Add(XKeys.NumpadDot, Keys.None);
			Mapping.Add(XKeys.NumpadEnter, Keys.None);
			Mapping.Add(XKeys.NumpadPlus, Keys.None);
			Mapping.Add(XKeys.NumpadMinus, Keys.None);
			// Add keys to the cache that can not be looked up with XLookupKeysym
			// HACK: I'm not sure these will work on other keyboard layouts.
			Cache.Add('(', new CachedKey(XKeys.OpenParens, true));
			Cache.Add(')', new CachedKey(XKeys.CloseParens, true));
			Cache.Add('[', new CachedKey(XKeys.OpenSquareBracket, true));
			Cache.Add(']', new CachedKey(XKeys.CloseSquareBracket, true));
			Cache.Add('=', new CachedKey(XKeys.Equals, true));
			Cache.Add('-', new CachedKey(XKeys.Dash, true));
			Cache.Add('!', new CachedKey(XKeys.ExMark, true));
			Cache.Add('@', new CachedKey(XKeys.At, true));
			Cache.Add('#', new CachedKey(XKeys.Hash, true));
			Cache.Add('$', new CachedKey(XKeys.Dollar, true));
			Cache.Add('%', new CachedKey(XKeys.Percent, true));
			Cache.Add('^', new CachedKey(XKeys.Circumflex, true));
			Cache.Add('&', new CachedKey(XKeys.Ampersand, true));
			Cache.Add('*', new CachedKey(XKeys.Asterisk, true));
			Cache.Add(' ', new CachedKey(XKeys.SpaceBar, false));
		}

		internal override void CleanupEventArray(long finalKeyDelay)
		{ }

		internal override void DoMouseDelay()
		{ }

		internal override nint GetFocusedKeybdLayout(nint window) => 0;

		internal override uint GetModifierLRState(bool explicitlyGet = false) => 0;

		internal override void InitEventArray(int maxEvents, uint modifiersLR)
		{ }

		internal override string ModifiersLRToText(uint aModifiersLR) => "";

		internal override void MouseClick(uint vk, int x, int y, long repeatCount, long speed, KeyEventTypes eventType, bool moveOffset)
		{ }

		internal override void MouseClickDrag(uint vk, int x1, int y1, int x2, int y2, long speed, bool relative)
		{ }

		internal override void MouseEvent(uint eventFlags, uint data, int x = CoordUnspecified, int y = CoordUnspecified)
		{ }

		internal override void MouseMove(ref int x, ref int y, ref uint eventFlags, long speed, bool moveOffset)
		{ }

		internal override int PbEventCount() => 0;

		internal override void SendEventArray(ref long finalKeyDelay, uint modsDuringSend)
		{ }

		internal override void SendKey(uint vk, uint sc, uint modifiersLR, uint modifiersLRPersistent
									   , long repeatCount, KeyEventTypes eventType, uint keyAsModifiersLR, nint targetWindow
									   , int x = CoordUnspecified, int y = CoordUnspecified, bool moveOffset = false)
		{
		}

		internal override void SendKeyEventMenuMask(KeyEventTypes eventType, uint extraInfo = KeyIgnoreAllExceptModifier)
		{
		}

		internal override void SendKeys(string keys, SendRawModes sendRaw, SendModes sendModeOrig, nint targetWindow)
		{
		}

		internal override int SiEventCount() => 0;

		internal override ToggleValueType ToggleKeyState(uint vk, ToggleValueType toggleValue) => ToggleValueType.Invalid;

		protected internal override void LongOperationUpdate()
		{ }

		protected internal override void LongOperationUpdateForSendKeys()
		{
		}

		//protected internal override void Send(string sequence)
		//{
		//  foreach (var c in sequence)
		//  {
		//      var Key = LookupKeycode(c);

		//      // If it is an upper case character, hold the shift key...
		//      if (char.IsUpper(c) || Key.Shift)
		//          Xlib.XTestFakeKeyEvent(XConn.Handle, (uint)XKeys.LeftShift, true, 0);

		//      // Make sure the key is up before we press it again.
		//      // If X thinks this key is still down, nothing will happen if we press it.
		//      // Likewise, if X thinks that the key is up, this will do no harm.
		//      Xlib.XTestFakeKeyEvent(XConn.Handle, Key.Sym, false, 0);
		//      // Fake a key event. Note that some programs filter this kind of event.
		//      Xlib.XTestFakeKeyEvent(XConn.Handle, Key.Sym, true, 0);
		//      Xlib.XTestFakeKeyEvent(XConn.Handle, Key.Sym, false, 0);

		//      // ...and release it later on
		//      if (char.IsUpper(c) || Key.Shift)
		//          Xlib.XTestFakeKeyEvent(XConn.Handle, (uint)XKeys.LeftShift, false, 0);
		//  }
		//}

		//protected internal override void Send(Keys key)
		//{
		//  var vk = (uint)key;
		//  Xlib.XTestFakeKeyEvent(XConn.Handle, vk, true, 0);
		//  Xlib.XTestFakeKeyEvent(XConn.Handle, vk, false, 0);
		//}

		//protected override void DeregisterHook()
		//{
		//  // TODO disposal
		//}
		protected internal override void SendKeyEvent(KeyEventTypes eventType, uint vk, uint sc = 0u, nint targetWindow = default, bool doKeyDelay = false, uint extraInfo = KeyIgnoreAllExceptModifier)
		{
		}

		// Simulate a number of backspaces
		//protected override void Backspace(int length)
		//{
		//  for (var i = 0; i < length; i++)
		//  {
		//      Xlib.XTestFakeKeyEvent(XConn.Handle, (uint)XKeys.BackSpace, true, 0);
		//      Xlib.XTestFakeKeyEvent(XConn.Handle, (uint)XKeys.BackSpace, false, 0);
		//  }
		//}

		protected override void RegisterHook()
		{
			SetupMapping();
			XConn = XConnectionSingleton.GetInstance();
			XConn.OnEvent += HandleXEvent;
		}

		private void HandleXEvent(XEvent ev)
		{
			//if (ev.type == XEventName.KeyPress || ev.type == XEventName.KeyRelease)
			//  _ = KeyReceived(TranslateKey(ev), ev.type == XEventName.KeyPress);//Figure this out.//TODO
		}

		private CachedKey LookupKeycode(char code)
		{
			// If we have a cache value, return that
			if (Cache.TryGetValue(code, out var cached))
				return cached;

			// First look up the KeySym (XK_* in X11/keysymdef.h)
			var KeySym = Xlib.XStringToKeysym(code.ToString());
			// Then look up the appropriate KeyCode
			var KeyCode = Xlib.XKeysymToKeycode(XConn.Handle, KeySym);
			// Cache for later use
			var Ret = new CachedKey(KeyCode, false);
			Cache.Add(code, Ret);
			return Ret;
		}

		private Keys StringToWFKey(XEvent ev)
		{
			_ = Dummy.Remove(0, Dummy.Length);
			_ = Dummy.Append(" "); // HACK: Somehow necessary
			ev.KeyEvent.state = 16; // Repair any shifting applied by control

			if (Xlib.XLookupString(ref ev, Dummy, 10, 0, 0) != 0)
			{
				var Lookup = Dummy.ToString();

				if (Dummy.Length == 1 && char.IsLetterOrDigit(Dummy[0]))
					Lookup = Dummy[0].ToString().ToUpper();

				if (string.IsNullOrEmpty(Lookup.Trim())) return Keys.None;

				try
				{
					return (Keys)Enum.Parse(typeof(Keys), Lookup);
				}
				catch (ArgumentException)
				{
					// TODO
					Debug.OutputDebug("Warning, could not look up key: " + Lookup);
					return Keys.None;
				}
			}
			else return Keys.None;
		}

		private Keys TranslateKey(XEvent Event) => Mapping.TryGetValue(Event.KeyEvent.keycode, out var key) ? key : StringToWFKey(Event);

		private struct CachedKey
		{
			public bool Shift;
			public uint Sym;

			public CachedKey(uint sym, bool shift)
			{
				Sym = sym;
				Shift = shift;
			}

			public CachedKey(XKeys sym, bool shift)
				: this((uint)sym, shift)
			{
			}
		}
	}
}

#endif