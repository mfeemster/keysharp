using Keysharp.Internals.Input;

namespace Keysharp.Tests;

[TestFixture, NonParallelizable, Category("Internal"), Category("Curated")]
public class InputDeviceTests : TestRunner
{
	[TestCase(17L)]
	[TestCase(4294967313L)]
	[TestCase(-17L)]
	public void Descriptor(long id)
	{
		var device = new Ks.Input(new InputDeviceInfo(id, "Keyboard", "Test keyboard", 0x46D, 0xC52B, "test-id"));
		Assert.That(Script.GetPropertyValue(device, "Id"), Is.EqualTo(id));
		Assert.That(Script.GetPropertyValue(device, "Kind"), Is.EqualTo("Keyboard"));
		Assert.That(Script.GetPropertyValue(device, "Name"), Is.EqualTo("Test keyboard"));
		Assert.That(Script.GetPropertyValue(device, "VendorId"), Is.EqualTo(0x46DL));
		Assert.That(Script.GetPropertyValue(device, "ProductId"), Is.EqualTo(0xC52BL));
		Assert.That(Script.GetPropertyValue(device, "HardwareId"), Is.EqualTo("test-id"));
		_ = Assert.Throws<KeysharpException>(() => Script.SetPropertyValue(device, "Id", 99L));
		Assert.That(device.Id, Is.EqualTo(id));
	}

	[TestCase("Joy1")]
	[TestCase("WheelUp")]
	public void InvalidDeviceKey(string name)
	{
		var device = new Ks.Input(new InputDeviceInfo(17, "Keyboard"));
		Assert.That(Assert.Throws<KeysharpException>(() => device.GetKeyState(name)).UserError, Is.InstanceOf<ValueError>());
	}

#if WINDOWS
	[Test]
	public void NativeDevices() => Assert.That(TestScript("input-devices", false), Is.True);

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
}
