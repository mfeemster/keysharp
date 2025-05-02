val := ""
callback := CallbackCreate("TheFunc", "&")
DllCall(callback, "float", 10.5, "int64", 42)

TheFunc(args)
{
	global val
	val := NumGet(args, 0, "float") + NumGet(args, A_PtrSize, "int64")
}

if (val == 52.5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := ""
CallbackFree(callback)
callback := CallbackCreate("FuncNoParams")
DllCall(callback)

FuncNoParams()
{
	global val
	val := 123
}

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CallbackFree(callback)
EnumAddress := CallbackCreate("EnumWindowsProc")
DetectHiddenWindows(True)
ct := 0
DllCall("EnumWindows", "Ptr", EnumAddress, "Ptr", 0)

EnumWindowsProc(hwnd, lParam)
{
	global ct
	win_title := WinGetTitle(hwnd)
	win_class := WinGetClass(hwnd)
	ct++

	if (ct < 5) ; go through the first five windows
		return true
	else
		return false
}

if (ct == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

CallbackFree(EnumAddress)

args := []
Loop 32 {
	i := A_Index - 1, ret := -1
	if (i > 0) {
		args.Push("ptr", i)
	}
	cb := CallbackCreate(Variadic,, i)
	ret := DllCall(cb, args*)
	if (ret == i)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	CallbackFree(cb)
}

Variadic(args*) => args.Length ? args[args.Length] : 0