WM_MY_BROADCAST := DllCall("RegisterWindowMessage", "Str", "MyUniqueBroadcastMessage", "UInt")
HWND_BROADCAST := 0xFFFF
OnMessage(WM_MY_BROADCAST, HandleMyBroadcast)

HandleMyBroadcast(wParam, lParam, *) {
    global result++
}

result := 0
PostMessage(WM_MY_BROADCAST, 123, 456,, HWND_BROADCAST)
Sleep 200

if (result == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

result := 0
SendMessage(WM_MY_BROADCAST, 123, 456,, A_ScriptHwnd)

if (result == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"
