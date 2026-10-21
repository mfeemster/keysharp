#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon
#Import Ks { Input }
#Include <assert>

Throws(() => Input(), A_LineNumber, Error)
#if WINDOWS || OSX
Devices := Input.Devices
Assert(Devices is Array, A_LineNumber)

for Device in Devices {
    Assert(Device is Input, A_LineNumber)
    Assert(Device.Kind == "Keyboard" || Device.Kind == "Mouse", A_LineNumber)
    Assert(Device.Id == "" || (Device.Id is Integer && Device.Id != 0), A_LineNumber)
    Throws(() => Device.GetKeyState("A"), A_LineNumber, OSError)
    AssertEq(Type(Device.Name), "String", A_LineNumber)
    Assert(Device.VendorId == "" || Type(Device.VendorId) == "Integer", A_LineNumber)
    Assert(Device.ProductId == "" || Type(Device.ProductId) == "Integer", A_LineNumber)
    AssertEq(Type(Device.HardwareId), "String", A_LineNumber)
    AssertEq(Type(Device.Manufacturer), "String", A_LineNumber)
    AssertEq(Type(Device.Connection), "String", A_LineNumber)
    AssertEq(Type(Device.Location), "String", A_LineNumber)
}
#endif

Registration := InputTestDescriptors()
Devices := Input.Devices
AssertEq(Devices.Length, 4, A_LineNumber)
AssertEq(Devices[2].Id, 4294967313, A_LineNumber)
AssertEq(Devices[2].VendorId, 0x46D, A_LineNumber)
AssertEq(Devices[2].ProductId, 0xC52B, A_LineNumber)
AssertEq(Devices[3].Id, -17, A_LineNumber)
CheckMetadata20(Devices[1], Devices[4])
CheckMetadata21(Devices[1], Devices[4])
Registration.Dispose()

CheckMetadata20(Known, Missing) {
    #Requires AutoHotkey v2.0
    for Property in ["Id", "Name", "VendorId", "ProductId", "HardwareId", "Manufacturer", "Connection", "Location", "IsInternal"] {
        Assert(HasProp(Missing, Property), A_LineNumber)
        AssertEq(Missing.%Property%, "", A_LineNumber)
    }
    CheckKnown(Known)
}

CheckMetadata21(Known, Missing) {
    #Requires AutoHotkey v2.1-alpha
    for Property in ["Id", "Name", "VendorId", "ProductId", "HardwareId", "Manufacturer", "Connection", "Location", "IsInternal"] {
        Assert(HasProp(Missing, Property), A_LineNumber)
        AssertEq(Missing.%Property% ?? "Unknown", "Unknown", A_LineNumber)
    }
    Throws(() => Missing.Manufacturer, A_LineNumber, UnsetError)
    AssertEq(Known.VendorId ?? "Unknown", 0, A_LineNumber)
    AssertEq(Known.ProductId ?? "Unknown", 0, A_LineNumber)
    AssertEq(Known.IsInternal ?? "Unknown", false, A_LineNumber)
    CheckKnown(Known)
}

CheckKnown(Device) {
    AssertEq(Device.Id, 17, A_LineNumber)
    AssertEq(Device.Kind, "Keyboard", A_LineNumber)
    AssertEq(Device.Name, "Test keyboard", A_LineNumber)
    AssertEq(Device.VendorId, 0, A_LineNumber)
    AssertEq(Device.ProductId, 0, A_LineNumber)
    AssertEq(Device.HardwareId, "test-id", A_LineNumber)
    AssertEq(Device.Manufacturer, "Test manufacturer", A_LineNumber)
    AssertEq(Device.Connection, "USB", A_LineNumber)
    AssertEq(Device.Location, "Port 1", A_LineNumber)
    AssertEq(Device.IsInternal, false, A_LineNumber)
    Throws(() => Device.Id := 99, A_LineNumber)
    Throws(() => Device.Name := "Changed", A_LineNumber)
    Throws(() => Device.GetKeyState("Joy1"), A_LineNumber, ValueError)
    Throws(() => Device.GetKeyState("WheelUp"), A_LineNumber, ValueError)
    AssertEq(Device.Id, 17, A_LineNumber)
}

#CSharp
public static object InputTestDescriptors()
    => Keysharp.Runtime.Script.TheScript.RegisterHookProvider(new MetadataProvider());

sealed class MetadataProvider : Keysharp.Components.Input.IHookProvider, Keysharp.Components.Input.IInputDeviceProvider
{
    public Keysharp.Components.Input.InputDeviceInfo[] GetDevices() => new[] {
        new Keysharp.Components.Input.InputDeviceInfo(17, "Keyboard", "Test keyboard", HardwareId: "test-id") {
            HasVendorId = true, HasProductId = true, Manufacturer = "Test manufacturer",
            Connection = "USB", Location = "Port 1", IsInternal = false
        },
        new Keysharp.Components.Input.InputDeviceInfo(4294967313, "Keyboard", VendorId: 0x46D, ProductId: 0xC52B),
        new Keysharp.Components.Input.InputDeviceInfo(-17, "Mouse"),
        new Keysharp.Components.Input.InputDeviceInfo(0, "Mouse")
    };
    public bool TryGetDeviceInfo(long DeviceId, string Kind, out Keysharp.Components.Input.InputDeviceInfo Device)
    {
        Device = null;
        return false;
    }
    public bool SetHooks(bool Keyboard, bool Mouse, Keysharp.Components.Input.IHookHost Host, out string Message)
    {
        Message = "";
        return false;
    }
    public void CallNextHook() { }
    public void Dispose() { }
}
#EndCSharp

FileAppend "pass", "*"
