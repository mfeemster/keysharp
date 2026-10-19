using Keysharp.Builtins;
using Keysharp.Components.Input;

namespace Keysharp.Internals.Input.Keyboard
{
	internal sealed class KeyState(Script script) : IDisposable
	{
		// A permanent marker prevents registration from racing past shutdown.
		private static readonly Registration retired = new(null, null);
		private Registration registration;

		internal bool? GetState(uint vk, KeyStateTypes type)
		{
			var current = Volatile.Read(ref registration);
			return current == null ? GetNativeState(vk, type) : GetCustomState(current, vk, type);
		}

		private bool? GetCustomState(Registration current, uint vk, KeyStateTypes type)
		{
			while (true)
			{
				if (current == retired) return null;
				var calls = Volatile.Read(ref current.calls);
				if (calls >= 0 && calls < int.MaxValue && Interlocked.CompareExchange(ref current.calls, calls + 1, calls) == calls)
					break;
				current = Volatile.Read(ref registration);
				if (current == null) return GetNativeState(vk, type);
			}

			try
			{
				var mode = type switch { KeyStateTypes.Physical => "P", KeyStateTypes.Toggle => "T", _ => "L" };
				if (current.provider.TryGetKeyState(vk, mode, out var down)) return down;
			}
			catch (Exception ex) when (ex is not KeysharpException and not Keysharp.Builtins.Flow.UserRequestedExitException)
			{
				_ = Errors.OSErrorOccurredWithMessage($"Key-state provider query failed: {ex.Message}");
				return null;
			}
			finally
			{
				if (Interlocked.Decrement(ref current.calls) == int.MinValue) current.Release();
			}

			return Volatile.Read(ref registration) == retired ? null : GetNativeState(vk, type);
		}

		internal IDisposable Register(IKeyStateProvider replacement)
		{
			ArgumentNullException.ThrowIfNull(replacement);
			ObjectDisposedException.ThrowIf(script.IsDisposed, script);
			var current = new Registration(this, replacement);
			var previous = Interlocked.CompareExchange(ref registration, current, null);

			if (previous != null)
			{
				ObjectDisposedException.ThrowIf(previous == retired, script);
				throw new InvalidOperationException("This script already has a key-state provider.");
			}
			return current;
		}

		public void Dispose()
		{
			var current = Interlocked.Exchange(ref registration, retired);
			if (current != retired) current?.Dispose();
		}

		private sealed class Registration(KeyState owner, IKeyStateProvider replacement) : IDisposable
		{
			internal IKeyStateProvider provider = replacement;
			// The sign bit retires admission; the remaining bits count queries still using the plugin.
			internal int calls;

			public void Dispose()
			{
				Interlocked.CompareExchange(ref owner.registration, null, this);
				// The last query releases the plugin, so shutdown never waits for a blocked driver call.
				if (Interlocked.Or(ref calls, int.MinValue) == 0) Release();
			}

			internal void Release()
			{
				var released = provider;
				provider = null;
				try { (released as IDisposable)?.Dispose(); }
				catch (Exception ex) { System.Diagnostics.Trace.WriteLine($"Key-state provider teardown failed: {ex}"); }
			}
		}

		internal bool GetNativeState(uint vk, KeyStateTypes keyStateType)
		{
			var ht = script.HookThread;
			var kbdMouseSender = ht.kbdMsSender;

			if (vk == 0)
				return false;

#if LINUX || OSX
			EnsureKeyStateQueryPermission(vk, keyStateType);
#endif
#if LINUX
			// Device snapshots represent overlapping holds on multiple keyboards or mice.
			if (keyStateType == KeyStateTypes.Physical
				&& (MouseUtils.IsMouseVK(vk)
					? Platform.Mouse.TryGetButtonStatePhysical(vk, out var isDown)
					: Platform.Keyboard.TryGetKeyStatePhysical(vk, out isDown)))
				return isDown;

			if (keyStateType != KeyStateTypes.Toggle)
				return HookThread.QueryKeyDownLogical(vk);
#endif

			switch (keyStateType)
			{
				case KeyStateTypes.Toggle:
					return ht.IsKeyToggledOn(vk);

				case KeyStateTypes.Physical:
				{
					// Physical queries must not install hooks; absent a hook, AHK falls back to OS state.
					var isMouse = MouseUtils.IsMouseVK(vk);

					if (isMouse ? ht.HasMouseHook() : ht.HasKbdHook())
					{
						if (!isMouse)
						{
							bool? dummy = null;

							// Correct tracked modifiers before reading their physical state.
							if (ht.KeyToModifiersLR(vk, 0, ref dummy) != 0)
								_ = kbdMouseSender.GetModifierLRState(true);

							if (vk < ht.physicalKeyState.Length)
								return (ht.physicalKeyState[vk] & KeyboardMouseSender.StateDown) != 0;

							return false;
						}

						return (ht.physicalKeyState[vk] & KeyboardMouseSender.StateDown) != 0;
					}

					// macOS can expose physical keyboard state without a process-local hook.
					if (!isMouse && Platform.Keyboard.TryGetKeyStatePhysical(vk, out var physicalDown))
						return physicalDown;

					break;
				}
			}

			return ht.IsKeyDownLogical(vk);
		}

#if LINUX || OSX
		private void EnsureKeyStateQueryPermission(uint vk, KeyStateTypes keyStateType)
		{
			// Modifier and lock-toggle snapshots do not require input monitoring access.
			var lockToggle = keyStateType == KeyStateTypes.Toggle
				&& vk is VirtualKeys.VK_CAPITAL or VirtualKeys.VK_NUMLOCK or VirtualKeys.VK_SCROLL;

			if (lockToggle || KeyboardUtils.IsModifierVk(vk))
				return;

#if LINUX
			var required = MouseUtils.IsMouseVK(vk)
				? Keysharp.Internals.Input.Linux.KeysharpInputClient.Operations.QueryPointerButtons
				: Keysharp.Internals.Input.Linux.KeysharpInputClient.Operations.QueryKeyState;

			if (script.LinuxServices.Input.HasInputOperation(required))
				return;
#endif
			_ = script.Permissions.EnsureCapabilities(
				inputMonitoring: true, inputControl: false, operation: "read input state");
		}
#endif
	}
}
