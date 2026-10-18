#if WINDOWS
using Microsoft.Win32.SafeHandles;
#elif OSX
using static Keysharp.Internals.AppleEvents.CF;
#endif

namespace Keysharp.Internals
{
	internal sealed record InputDeviceInfo(long Id, string Kind, string Name = "", uint VendorId = 0, uint ProductId = 0, string HardwareId = "")
	{
		// Saved descriptors must not address IDs from a replacement input source.
		internal object Source { get; set; }
	}

	internal static partial class Platform
	{
		internal static InputDeviceInfo[] GetInputDevices(Script owner) =>
#if WINDOWS
			WindowsInputDevices.GetDevices();
#elif LINUX
			owner.LinuxServices.Input.GetDeviceDescriptors()
				?? throw new InvalidOperationException("Input device enumeration requires the keysharp-input service and access to device metadata.");
#elif OSX
			MacInputDevices.GetDevices();
#endif

		internal static IDisposable WatchInputDevices(Script owner, Action changed, Action failed) =>
#if WINDOWS
			WindowsInputDevices.Watch(changed);
#elif LINUX
			owner.LinuxServices.Input.WatchDevices(changed, failed);
#elif OSX
			MacInputDevices.Watch(changed, failed);
#endif
	}

#if WINDOWS
	internal static class WindowsInputDevices
	{
		private const uint DeviceNameCommand = 0x20000007, StringProperty = 1, StringListProperty = 7;
		private const uint HardwareIdProperty = 1, FriendlyNameProperty = 12, DescriptionProperty = 0;
		private const uint ShareReadWrite = 3, OpenExisting = 3;
		private const int InsufficientBuffer = 122;

		internal static IDisposable Watch(Action changed) => new DeviceWatcher(changed);

		private sealed class DeviceWatcher : IDisposable
		{
			private readonly Action changed;
			private readonly DeviceNotification callback;
			private nint keyboard, mouse;
			private int stopped;

			internal DeviceWatcher(Action changed)
			{
				this.changed = changed;
				callback = OnDeviceNotification;

				try
				{
					keyboard = Register(new Guid("884b96c3-56ef-11d1-bc8c-00a0c91405dd"));
					mouse = Register(new Guid("378de44c-56ef-11d1-bc8c-00a0c91405dd"));
				}
				catch { Dispose(); throw; }
			}

			public void Dispose()
			{
				if (Interlocked.Exchange(ref stopped, 1) != 0) return;
				// Unregistration waits for in-flight callbacks, which only queue inventory work.
				if (keyboard != 0) _ = CM_Unregister_Notification(keyboard);
				if (mouse != 0) _ = CM_Unregister_Notification(mouse);
				GC.KeepAlive(callback);
			}

			private nint Register(Guid classGuid)
			{
				var filter = new NotificationFilter { Size = (uint)Marshal.SizeOf<NotificationFilter>(), ClassGuid = classGuid };
				var result = CM_Register_Notification(in filter, 0, callback, out var notification);
				return result == 0 ? notification
					: throw new InvalidOperationException($"Windows input device notifications could not be registered (Configuration Manager error {result}).");
			}

			private uint OnDeviceNotification(nint notification, nint context, uint action, nint eventData, uint eventDataSize)
			{
				if (action is 0 or 1 && Volatile.Read(ref stopped) == 0)
					try { changed(); }
					catch (Exception error) { Diagnostics.Debug.WriteLine($"Input device notification failed: {error.Message}"); }
				return 0;
			}
		}

		internal static InputDeviceInfo[] GetDevices()
		{
			var devices = new List<InputDeviceInfo>();
			var deviceInfoSet = SetupDiCreateDeviceInfoList(0, 0);

			try
			{
				foreach (var device in GetRawDevices())
				{
					if (device.Kind > 1)
						continue;

					var kind = device.Kind == 1 ? "Keyboard" : "Mouse";
					var path = GetDevicePath(device.Handle);
					var name = "";
					var hardwareId = "";
					var deviceInfo = new DeviceInfoData { Size = (uint)Marshal.SizeOf<DeviceInfoData>() };
					var deviceInterface = new DeviceInterfaceData { Size = (uint)Marshal.SizeOf<DeviceInterfaceData>() };

					if (deviceInfoSet != -1 && path.Length != 0 && SetupDiOpenDeviceInterfaceW(deviceInfoSet, path, 0, ref deviceInterface))
					{
						// A null detail buffer still fills the devnode; the expected result is ERROR_INSUFFICIENT_BUFFER.
						// https://learn.microsoft.com/windows/win32/api/setupapi/nf-setupapi-setupdigetdeviceinterfacedetailw
						_ = SetupDiGetDeviceInterfaceDetailW(deviceInfoSet, ref deviceInterface, 0, 0, out _, ref deviceInfo);

						if (Marshal.GetLastPInvokeError() == InsufficientBuffer)
						{
							hardwareId = GetDeviceProperty(deviceInfoSet, ref deviceInfo, HardwareIdProperty, StringListProperty);
							name = GetDeviceProperty(deviceInfoSet, ref deviceInfo, FriendlyNameProperty, StringProperty);

							if (name.Length == 0)
								name = GetDeviceProperty(deviceInfoSet, ref deviceInfo, DescriptionProperty, StringProperty);
						}
					}

					var vendorId = ParseHardwareIdNumber(hardwareId, "VID_");
					var productId = ParseHardwareIdNumber(hardwareId, "PID_");

					if (path.Length != 0)
					{
						// Zero access allows HID metadata queries without reading or writing input reports.
						using var handle = new SafeFileHandle(Os.Windows.WindowsAPI.CreateFile(path, 0, ShareReadWrite, 0, OpenExisting, 0, 0), ownsHandle: true);
						var attributes = new HidAttributes { Size = (uint)Marshal.SizeOf<HidAttributes>() };

						if (!handle.IsInvalid && HidD_GetAttributes(handle, ref attributes))
						{
							vendorId = attributes.VendorId;
							productId = attributes.ProductId;
						}
					}

					devices.Add(new InputDeviceInfo(unchecked((long)(nuint)device.Handle), kind, name, vendorId, productId, hardwareId));
				}
			}
			finally
			{
				if (deviceInfoSet != -1)
					_ = SetupDiDestroyDeviceInfoList(deviceInfoSet);
			}

			return devices.ToArray();
		}

		private static RawDevice[] GetRawDevices()
		{
			var size = (uint)Marshal.SizeOf<RawDevice>();

			for (var attempt = 0; attempt < 3; attempt++)
			{
				uint count = 0;

				if (GetRawInputDeviceList(null, ref count, size) == uint.MaxValue)
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError(), "Windows input devices could not be enumerated.");

				if (count == 0)
					return [];

				if (count > 65536)
					throw new InvalidOperationException("Windows returned an invalid input device count.");

				var devices = new RawDevice[count];
				var read = GetRawInputDeviceList(devices, ref count, size);

				if (read != uint.MaxValue)
					return read == devices.Length ? devices : devices.AsSpan(0, (int)read).ToArray();

				if (Marshal.GetLastPInvokeError() != InsufficientBuffer)
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastPInvokeError(), "Windows input devices could not be enumerated.");
			}

			throw new InvalidOperationException("Windows input devices changed during enumeration. Try again.");
		}

		private static string GetDevicePath(nint device)
		{
			uint count = 0;

			if (GetRawInputDeviceInfoW(device, DeviceNameCommand, 0, ref count) == uint.MaxValue || count == 0 || count > 65536)
				return "";

			var text = new char[count];
			return GetRawInputDeviceInfoW(device, DeviceNameCommand, text, ref count) != uint.MaxValue
				? new string(text.AsSpan().TrimEnd('\0')) : "";
		}

		private static string GetDeviceProperty(nint set, ref DeviceInfoData info, uint property, uint expectedType)
		{
			_ = SetupDiGetDeviceRegistryPropertyW(set, ref info, property, out _, null, 0, out var needed);

			if (needed == 0 || needed > 65536 || (needed & 1) != 0)
				return "";

			var bytes = new byte[needed];

			if (!SetupDiGetDeviceRegistryPropertyW(set, ref info, property, out var type, bytes, needed, out var used)
				|| type != expectedType || used > needed || (used & 1) != 0)
				return "";

			var text = MemoryMarshal.Cast<byte, char>(bytes.AsSpan(0, (int)used));
			var end = text.IndexOf('\0');
			return new string(end < 0 ? text : text[..end]);
		}

		private static uint ParseHardwareIdNumber(ReadOnlySpan<char> hardwareId, ReadOnlySpan<char> token)
		{
			var start = hardwareId.IndexOf(token, StringComparison.OrdinalIgnoreCase);

			if (start <= 0 || hardwareId[start - 1] is not ('\\' or '&'))
				return 0;

			var digits = start + token.Length;
			var end = digits + 4;
			return end <= hardwareId.Length && (end == hardwareId.Length || hardwareId[end] == '&')
				&& uint.TryParse(hardwareId.Slice(digits, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value)
				? value : 0;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct HidAttributes
		{
			internal uint Size;
			internal ushort VendorId, ProductId, Version;
		}

		[DllImport("hid.dll", ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.U1)]
		private static extern bool HidD_GetAttributes(SafeFileHandle device, ref HidAttributes attributes);

		[StructLayout(LayoutKind.Sequential)]
		private struct RawDevice
		{
			internal nint Handle;
			internal uint Kind;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DeviceInfoData
		{
			internal uint Size;
			internal Guid ClassGuid;
			internal uint Instance;
			internal nint Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DeviceInterfaceData
		{
			internal uint Size;
			internal Guid ClassGuid;
			internal uint Flags;
			internal nint Reserved;
		}

		// The union also contains a 200-character UTF-16 device instance ID.
		[StructLayout(LayoutKind.Explicit, Size = 416)]
		private struct NotificationFilter
		{
			[FieldOffset(0)] internal uint Size;
			[FieldOffset(16)] internal Guid ClassGuid;
		}

		[UnmanagedFunctionPointer(CallingConvention.Winapi)]
		private delegate uint DeviceNotification(nint notification, nint context, uint action, nint eventData, uint eventDataSize);

		[DllImport("cfgmgr32.dll", ExactSpelling = true)]
		private static extern uint CM_Register_Notification(in NotificationFilter filter, nint context, DeviceNotification callback, out nint notification);

		[DllImport("cfgmgr32.dll", ExactSpelling = true)]
		private static extern uint CM_Unregister_Notification(nint notification);

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern uint GetRawInputDeviceList([Out] RawDevice[] devices, ref uint count, uint size);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		private static extern uint GetRawInputDeviceInfoW(nint device, uint command, nint data, ref uint size);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		private static extern uint GetRawInputDeviceInfoW(nint device, uint command, [Out] char[] data, ref uint size);

		[DllImport("setupapi.dll", ExactSpelling = true)]
		private static extern nint SetupDiCreateDeviceInfoList(nint classGuid, nint parent);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetupDiOpenDeviceInterfaceW(nint set, string path, uint flags, ref DeviceInterfaceData iface);

		[DllImport("setupapi.dll", ExactSpelling = true, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetupDiGetDeviceInterfaceDetailW(nint set, ref DeviceInterfaceData iface, nint detail, uint size, out uint needed, ref DeviceInfoData info);

		[DllImport("setupapi.dll", ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetupDiGetDeviceRegistryPropertyW(nint set, ref DeviceInfoData info, uint property, out uint type,
			[Out] byte[] buffer, uint bufferSize, out uint needed);

		[DllImport("setupapi.dll", ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetupDiDestroyDeviceInfoList(nint set);
	}
#elif OSX
	internal static class MacInputDevices
	{
		private const string IOKit = "/System/Library/Frameworks/IOKit.framework/IOKit";
		private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
		private const string LibSystem = "/usr/lib/libSystem.B.dylib";

		internal static IDisposable Watch(Action changed, Action failed) => new DeviceWatcher(changed, failed);

		private sealed class DeviceWatcher : IDisposable
		{
			private static readonly DispatchFunction drainQueue = _ => { };
			private readonly Action changed, failed;
			private readonly MatchingNotification callback;
			private nint port, queue;
			private uint arrivals, removals;
			private int stopped;

			internal DeviceWatcher(Action changed, Action failed)
			{
				this.changed = changed;
				this.failed = failed;
				callback = OnMatchingNotification;

				try
				{
					port = IONotificationPortCreate(0);
					queue = dispatch_queue_create("Keysharp input device notifications", 0);

					if (port == 0 || queue == 0 || !Register("IOServiceFirstMatch", out arrivals) || !Register("IOServiceTerminate", out removals))
						throw new InvalidOperationException("macOS input device notifications could not be registered.");

					// Emptying the initial iterators arms notifications without reporting existing devices as arrivals.
					_ = Drain(arrivals);
					_ = Drain(removals);
					IONotificationPortSetDispatchQueue(port, queue);
				}
				catch { Dispose(); throw; }
			}

			public void Dispose()
			{
				if (Interlocked.Exchange(ref stopped, 1) != 0) return;
				if (port != 0) IONotificationPortSetDispatchQueue(port, 0);
				// Native callbacks only queue work, so draining cannot wait for the event manager's lock.
				if (queue != 0) dispatch_sync_f(queue, 0, drainQueue);
				if (arrivals != 0) _ = IOObjectRelease(arrivals);
				if (removals != 0) _ = IOObjectRelease(removals);
				if (port != 0) IONotificationPortDestroy(port);
				if (queue != 0) dispatch_release(queue);
				GC.KeepAlive(callback);
			}

			private bool Register(string notification, out uint iterator)
			{
				iterator = 0;
				var matching = IOServiceMatching("IOHIDDevice");
				return matching != 0 && IOServiceAddMatchingNotification(port, notification, matching, callback, 0, out iterator) == 0;
			}

			private void OnMatchingNotification(nint context, uint iterator)
			{
				try
				{
					if (Drain(iterator) && Volatile.Read(ref stopped) == 0) changed();
				}
				catch (Exception error)
				{
					Diagnostics.Debug.WriteLine($"Input device notification failed: {error.Message}");
					try { if (Volatile.Read(ref stopped) == 0) failed(); }
					catch (Exception failure) { Diagnostics.Debug.WriteLine($"Input device failure notification failed: {failure.Message}"); }
				}
			}

			private static bool Drain(uint iterator)
			{
				var changed = false;
				uint device;

				while ((device = IOIteratorNext(iterator)) != 0)
				{
					changed = true;
					_ = IOObjectRelease(device);
				}

				return changed;
			}
		}

		internal static InputDeviceInfo[] GetDevices()
		{
			var devices = new List<InputDeviceInfo>();
			var matching = IOServiceMatching("IOHIDDevice");

			if (matching == 0)
				throw new InvalidOperationException("macOS input devices could not be enumerated.");

			var result = IOServiceGetMatchingServices(0, matching, out var iterator);

			if (result != 0)
				throw new InvalidOperationException($"macOS input devices could not be enumerated (IOKit error {result}).");

			try
			{
				uint device;

				while ((device = IOIteratorNext(iterator)) != 0)
				{
					try { Describe(device, devices); }
					finally { _ = IOObjectRelease(device); }
				}
			}
			finally
			{
				_ = IOObjectRelease(iterator);
			}

			return devices.ToArray();
		}

		private static void Describe(uint device, List<InputDeviceInfo> devices)
		{
			if (IORegistryEntryCreateCFProperties(device, out var properties, 0, 0) != 0 || properties == 0)
				return;

			try
			{
				if (CFGetTypeID(properties) != CFDictionaryGetTypeID())
					return;

				var keyboard = false;
				var mouse = false;
				ClassifyUsage(Number(properties, DeviceKeys.PrimaryPage), Number(properties, DeviceKeys.PrimaryUsage), ref keyboard, ref mouse);
				var pairs = CFDictionaryGetValue(properties, DeviceKeys.UsagePairs);

				// Usage pairs include every application collection, including both halves of a composite keyboard/mouse.
				// https://github.com/apple-oss-distributions/IOHIDFamily/blob/main/IOHIDFamily/IOHIDKeys.h
				if (pairs != 0 && CFGetTypeID(pairs) == CFArrayGetTypeID())
					for (nint index = 0, count = CFArrayGetCount(pairs); index < count; index++)
					{
						var pair = CFArrayGetValueAtIndex(pairs, index);

						if (pair != 0 && CFGetTypeID(pair) == CFDictionaryGetTypeID())
							ClassifyUsage(Number(pair, DeviceKeys.UsagePage), Number(pair, DeviceKeys.Usage), ref keyboard, ref mouse);
					}

				if (!keyboard && !mouse)
					return;

				var name = Text(properties, DeviceKeys.Product);
				var hardwareId = Text(properties, DeviceKeys.Serial);
				var vendorId = Number(properties, DeviceKeys.Vendor);
				var productId = Number(properties, DeviceKeys.ProductId);

				// Event taps have no device ID. SerialNumber is copied only when the HID service supplies a string.
				if (keyboard)
					devices.Add(new InputDeviceInfo(0, "Keyboard", name, vendorId, productId, hardwareId));

				if (mouse)
					devices.Add(new InputDeviceInfo(0, "Mouse", name, vendorId, productId, hardwareId));
			}
			finally
			{
				CFRelease(properties);
			}
		}

		private static void ClassifyUsage(uint usagePage, uint usage, ref bool keyboard, ref bool mouse)
		{
			if (usagePage != 1)
				return;

			keyboard |= usage is 6 or 7;
			mouse |= usage == 2;
		}

		private static uint Number(nint dictionary, nint key)
		{
			var value = CFDictionaryGetValue(dictionary, key);
			return value != 0 && CFGetTypeID(value) == CFNumberGetTypeID() && CFNumberGetValue(value, NumberSInt64Type, out long number)
				&& number is >= 0 and <= uint.MaxValue ? (uint)number : 0;
		}

		private static string Text(nint dictionary, nint key)
		{
			var value = CFDictionaryGetValue(dictionary, key);
			return value != 0 && CFGetTypeID(value) == CFStringGetTypeID() && CFStringGetLength(value) <= 65536 ? ReadString(value) : "";
		}

		private static class DeviceKeys
		{
			internal static readonly nint Product = CreateString("Product"), Vendor = CreateString("VendorID"), ProductId = CreateString("ProductID");
			internal static readonly nint Serial = CreateString("SerialNumber"), PrimaryPage = CreateString("PrimaryUsagePage"), PrimaryUsage = CreateString("PrimaryUsage");
			internal static readonly nint UsagePairs = CreateString("DeviceUsagePairs"), UsagePage = CreateString("DeviceUsagePage"), Usage = CreateString("DeviceUsage");
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void MatchingNotification(nint context, uint iterator);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void DispatchFunction(nint context);

		[DllImport(IOKit)]
		private static extern nint IONotificationPortCreate(uint mainPort);
		[DllImport(IOKit)]
		private static extern void IONotificationPortDestroy(nint port);
		[DllImport(IOKit)]
		private static extern void IONotificationPortSetDispatchQueue(nint port, nint queue);
		[DllImport(IOKit)]
		private static extern int IOServiceAddMatchingNotification(nint port, [MarshalAs(UnmanagedType.LPStr)] string notification,
			nint matching, MatchingNotification callback, nint context, out uint iterator);
		[DllImport(LibSystem)]
		private static extern nint dispatch_queue_create([MarshalAs(UnmanagedType.LPUTF8Str)] string label, nint attributes);
		[DllImport(LibSystem)]
		private static extern void dispatch_sync_f(nint queue, nint context, DispatchFunction work);
		[DllImport(LibSystem)]
		private static extern void dispatch_release(nint queue);

		[DllImport(IOKit)]
		private static extern nint IOServiceMatching([MarshalAs(UnmanagedType.LPStr)] string name);
		[DllImport(IOKit)]
		private static extern int IOServiceGetMatchingServices(uint mainPort, nint matching, out uint iterator);
		[DllImport(IOKit)]
		private static extern uint IOIteratorNext(uint iterator);
		[DllImport(IOKit)]
		private static extern int IOObjectRelease(uint device);
		[DllImport(IOKit)]
		private static extern int IORegistryEntryCreateCFProperties(uint device, out nint properties, nint allocator, uint options);
		[DllImport(CoreFoundation)]
		private static extern nint CFDictionaryGetTypeID();
		[DllImport(CoreFoundation)]
		private static extern nint CFDictionaryGetValue(nint dictionary, nint key);
		[DllImport(CoreFoundation)]
		[return: MarshalAs(UnmanagedType.I1)]
		private static extern bool CFNumberGetValue(nint number, nint kind, out long value);
		[DllImport(CoreFoundation)]
		private static extern nint CFArrayGetTypeID();
		[DllImport(CoreFoundation)]
		private static extern nint CFStringGetLength(nint text);
	}
#endif
}
