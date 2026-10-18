#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon
#Import Ks { Input }
#Include <assert>

Throws(() => Input(), A_LineNumber, Error)
Devices := Input.Devices
Assert(Devices is Array, A_LineNumber)

for Device in Devices {
    Assert(Device is Input, A_LineNumber)
    Assert(Device.Kind == "Keyboard" || Device.Kind == "Mouse", A_LineNumber)
    AssertEq(Type(Device.Id), "Integer", A_LineNumber)
#if WINDOWS
    Assert(Device.Id != 0, A_LineNumber)
    Throws(() => Device.GetKeyState("A"), A_LineNumber, OSError)
    Throws(() => Device.GetKeyState("Joy1"), A_LineNumber, ValueError)
#endif
    AssertEq(Type(Device.Name), "String", A_LineNumber)
    AssertEq(Type(Device.VendorId), "Integer", A_LineNumber)
    AssertEq(Type(Device.ProductId), "Integer", A_LineNumber)
    AssertEq(Type(Device.HardwareId), "String", A_LineNumber)
    Throws(() => Device.Name := "Changed", A_LineNumber)
}

FileAppend "pass", "*"
