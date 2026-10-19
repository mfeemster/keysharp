using System;

namespace Keysharp.Components.Input
{
	/// <summary>Receives serialized input transitions. Returning true suppresses the transition.</summary>
	/// <remarks>Messages, virtual keys and flags use Windows WM_*, VK_*, LLKHF_* and LLMHF_* values.
	/// Scan codes use set-1 on Windows, evdev on Linux and kVK on macOS. Zero VK requests
	/// translation; zero time/device means unknown. int.MinValue coordinates mean no cursor position.
	/// A false return without CallNextHook leaves forwarding to the provider.
	/// The provider serializes KeyboardEvent, MouseEvent and ResetState with one another.</remarks>
	public interface IHookHost
	{
		bool KeyboardEvent(uint Message, uint VirtualKey, uint ScanCode, uint Flags, uint Time, ulong ExtraInfo, long DeviceId);
		bool MouseEvent(uint Message, int X, int Y, uint MouseData, uint Flags, uint Time, ulong ExtraInfo,
			long DeviceId, int DeltaX, int DeltaY);
		void CaptureLost(string Message);
		/// <summary>Forgets tracked state after a capture gap, before reporting resumed input.</summary>
		void ResetState();
	}

	/// <summary>Supplies keyboard and mouse transitions to the script's hook processing.</summary>
	/// <remarks>SetHooks must not wait for a host callback; false preserves the previous selection.
	/// CallNextHook forwards the current transition synchronously and promptly, without waiting on the receiver.
	/// On Windows, report physical input only; native hooks observe injected input. On Linux and macOS,
	/// report the observed stream, marking injected input with its LLKHF/LLMHF flag.
	/// The provider owns forwarding and suppression, including any OS effects of a suppressed transition.
	/// Only report input belonging to the script's session and desktop.
	/// Report capture/session gaps through ResetState before resuming input.</remarks>
	public interface IHookProvider : IDisposable
	{
		bool SetHooks(bool Keyboard, bool Mouse, IHookHost Host, out string Message);
		void CallNextHook();
	}

	/// <summary>Describes the device IDs supplied by a hook provider.</summary>
	public interface IInputDeviceProvider
	{
		InputDeviceInfo[] GetDevices();
		bool TryGetDeviceInfo(long DeviceId, string Kind, out InputDeviceInfo Device);
	}

	/// <summary>An immutable description whose ID is scoped to its provider.</summary>
	public sealed class InputDeviceInfo
	{
		public long Id { get; }
		public string Kind { get; }
		public string Name { get; }
		public uint VendorId { get; }
		public uint ProductId { get; }
		public string HardwareId { get; }

		public InputDeviceInfo(long Id, string Kind, string Name = "", uint VendorId = 0, uint ProductId = 0, string HardwareId = "")
		{
			if (Kind is not ("Keyboard" or "Mouse"))
				throw new ArgumentException("Kind must be Keyboard or Mouse.", nameof(Kind));
			this.Id = Id;
			this.Kind = Kind;
			this.Name = Name ?? "";
			this.VendorId = VendorId;
			this.ProductId = ProductId;
			this.HardwareId = HardwareId ?? "";
		}
	}
}
