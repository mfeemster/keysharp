#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon
#Import Ks { Input }
#Include <assert>

Hook := Input.OnDeviceChange((Hook, Change, Device) => 0)
try {
    Assert(Hook is Input.DeviceHook, A_LineNumber)
    Assert(Hook.InProgress, A_LineNumber)
    AssertEq(Input.Hooks.Length, 1, A_LineNumber)
    AssertEq(Input.Hooks[1], Hook, A_LineNumber)
    Hook.Stop()
    Assert(!Hook.InProgress, A_LineNumber)
    AssertEq(Hook.EndReason, "Stopped", A_LineNumber)
    AssertEq(Input.Hooks.Length, 0, A_LineNumber)
    Hook.Start()
    Assert(Hook.InProgress, A_LineNumber)
} finally {
    Hook.Stop()
}

FileAppend "pass", "*"
