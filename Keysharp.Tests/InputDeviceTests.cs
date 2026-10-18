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

#if WINDOWS
	[Test]
	public void NativeDevices() => Assert.That(TestScript("input-devices", false), Is.True);
#endif
}
