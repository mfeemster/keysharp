using Keysharp.Internals.Input;

namespace Keysharp.Tests;

[TestFixture, NonParallelizable, Category("Internal"), Category("Curated")]
public class InputDeviceTests : TestRunner
{
	[Test]
	public void Devices() => Assert.That(TestScript("input-devices", false), Is.True);

#if WINDOWS
	[Test]
	public void DeviceNotifications() => Assert.That(TestScript("input-device-changes", false), Is.True);
#endif

	[Test]
	public void InventoryChanges()
	{
		var mouse = new InputDeviceInfo(7, "Mouse", "Composite");
		var keyboard = new InputDeviceInfo(7, "Keyboard", "Composite");
		var removed = new InputDeviceInfo(8, "Mouse");
		var renamed = keyboard with { Name = "Renamed" };
		var added = new InputDeviceInfo(9, "Keyboard");
		var changes = InputEventManager.Classify([mouse, keyboard, removed],
			[mouse with { Source = new object() }, renamed, added]);
		Assert.That(changes, Is.EqualTo(new[] { ("Changed", renamed), ("Removed", removed), ("Added", added) }));
	}

	[Test]
	public void AnonymousDevicesAreNotMerged()
	{
		var device = new InputDeviceInfo(0, "Keyboard", "Keyboard");
		Assert.That(InputEventManager.Classify([device, device], [device]), Is.EqualTo(new[] { ("Removed", device) }));
	}

	[Test]
	public void MetadataChanges()
	{
		var device = new InputDeviceInfo(7, "Keyboard");
		InputDeviceInfo[] changed = [device with { Manufacturer = "Manufacturer" }, device with { Connection = "USB" },
			device with { Location = "Port 1" }, device with { IsInternal = false }, device with { VendorId = 0 },
			device with { ProductId = 0 }];
		foreach (var updated in changed)
			Assert.That(InputEventManager.Classify([device], [updated]), Is.EqualTo(new[] { ("Changed", updated) }));
	}

#if OSX
	[TestCase(0, 0u)]
	[TestCase(-100663296, 0xFA000000u)]
	[TestCase(-1, uint.MaxValue)]
	public void UnsignedRegistryNumbers(int value, uint expected)
	{
		var number = CFNumberCreate(0, 3, ref value);
		try { Assert.That(MacInputDevices.ReadUnsignedNumber(number), Is.EqualTo(expected)); }
		finally { Keysharp.Internals.AppleEvents.CF.CFRelease(number); }
	}

	[System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
	private static extern nint CFNumberCreate(nint allocator, nint type, ref int value);
#endif
}
