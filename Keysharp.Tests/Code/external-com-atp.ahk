#NoTrayIcon
#ErrorStdOut
#Warn All, StdOut
#Include <assert>
#Import Ks { A_KsCorePath }

SplitPath(A_KsCorePath,, &RuntimeDir)
shared := {Values: ["One", "Two", "Three"], Result: ""}
NumPut("Int64", 0x20400, "Int64", 0x46000000000000c0, IID_IDispatch := Buffer(16))
lResult := DllCall("OleAcc\LresultFromObject", "Ptr", IID_IDispatch, "Ptr", 0, "Ptr", ObjPtr(shared), "Ptr")
exec := ComObject("WScript.Shell").Exec('"' RuntimeDir '\Keysharp.exe" *')
try {
    exec.StdIn.Write(Format("
(
#NoTrayIcon
#ErrorStdOut
#Warn All, StdOut
    NumPut("Int64", 0x20400, "Int64", 0x46000000000000c0, IID_IDispatch := Buffer(16))
    DllCall("OleAcc\ObjectFromLresult", "Ptr", {}, "Ptr", IID_IDispatch, "Ptr", 0, "Ptr*", &pointer := 0)
    shared := ComValue(9, pointer), result := ""
    for index, value in shared.Values.__Enum()
        result .= index ":" value "|"
    for index, value in shared.Values.__Enum(2)
        result .= index ":" value "|"
    for value in shared.Values.__Enum()
        result .= value "|"
    shared.Result := result
)", lResult))
    exec.StdIn.Close
    started := A_TickCount
    while exec.Status = 0 {
        if A_TickCount - started > 30000
            throw Error("Enumeration subprocess timed out.")
        Sleep 10
    }
    diagnostics := exec.StdOut.ReadAll() exec.StdErr.ReadAll()
    if exec.ExitCode != 0 || diagnostics != ""
        throw Error("Enumeration subprocess failed (exit " exec.ExitCode "). " diagnostics)
    AssertEq(shared.Result, "1:One|2:Two|3:Three|1:One|2:Two|3:Three|One|Two|Three|", A_LineNumber)
} finally {
    if exec.Status = 0
        exec.Terminate()
}
FileAppend "pass", "*"
