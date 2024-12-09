desktop := GetDesktopWindow()
buf := Buffer(16, 0)
DllCall("user32.dll\GetWindowRect", "ptr", desktop, "ptr", buf)
l := NumGet(buf, 0, "UInt")
t := NumGet(buf, 4, "UInt")
r := NumGet(buf, 8, "UInt")
b := NumGet(buf, 12, "UInt")
	
if (r > 0 && b > 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

str := "lower"
len := StrLen(str)
strbuf := StringBuffer(str)
DllCall("user32.dll\CharUpperBuff", "ptr", strbuf, "UInt", len)

if (strbuf == StrUpper(str))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


DetectHiddenWindows True
pid := ""
Run("notepad.exe", "", "max", &pid)
ProcessWait(pid)
ProcessSetPriority("H", pid)
Sleep(2000)

if DllCall("IsWindowVisible", "Ptr", WinExist("Untitled - Notepad"))
{
	ProcessClose(pid)
	ProcessWaitClose(pid)
	FileAppend, "pass", "*"
}
else
	FileAppend, "fail", "*"

ZeroPaddedNumber := Buffer(20)
DllCall("wsprintf", "Ptr", ZeroPaddedNumber, "Str", "%010d", "Int", 432, "Cdecl")
str := StrGet(ZeroPaddedNumber)
fmtstr := Format(str, "0:D10")

if (str == "0000000432" && str == fmtstr)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

freq := 0
CounterBefore := 0
CounterAfter := 0

DllCall("QueryPerformanceFrequency", "Int64*", &freq)
DllCall("QueryPerformanceCounter", "Int64*", &CounterBefore)
Sleep(1000)
DllCall("QueryPerformanceCounter", "Int64*", &CounterAfter)
elapsed := (CounterAfter - CounterBefore) / freq * 1000

if (elapsed > 900 && elapsed < 1200)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

freq := 0
CounterBefore := 0
CounterAfter := 0
mh := DllCall("GetModuleHandle", "Str", "kernel32", "Ptr")
qpf := DllCall("GetProcAddress", "Ptr", mh, "AStr", "QueryPerformanceFrequency", "Ptr")
qpc := DllCall("GetProcAddress", "Ptr", mh, "AStr", "QueryPerformanceCounter", "Ptr")

DllCall(qpf, "Int64*", freq)
DllCall(qpc, "Int64*", counterbefore)
Sleep(1000)
DllCall(qpc, "Int64*", counterafter)
elapsed := (CounterAfter - CounterBefore) / freq * 1000

if (elapsed > 900 && elapsed < 1200)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

mh := DllCall("GetModuleHandle", "Str", "kernel32", "Ptr")
MulDivProc := DllCall("GetProcAddress", "Ptr", mh, "AStr", "MulDiv", "Ptr")
result := DllCall(MulDivProc, "Int", 3, "Int", 4, "Int", 3)

if (result == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
str := "hello"
DllCall("msvcrt.dll\_wcsrev", "Str", str)

if (str == "olleh")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
str2 := "world"
DllCall("msvcrt.dll\_wcsrev", "Str", &str2)

if (str2 == "dlrow")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

code := Buffer(64)
NumPut(
	'Int64', 0x10ec8348e5894855, 'Int64', 0x00fc45c7104d8948,
	'Int64', 0xfc458304eb000000, 'Int64', 0x8d489848fc458b01,
	'Int64', 0x014810458b480014, 'Int64', 0x75c0856600b70fd0,
	'Int64', 0x10c48348fc458be4, 'Int64', 0xc35d,
	code
)

if (!DllCall("VirtualProtect", "Ptr", code, "Ptr", code.Size, "UInt", 0x40, "UInt*", &OldProtect := 0, "UInt"))
	throw Error("Failed to mark MCL memory as executable")

val := DllCall(code, "Str", "Hello", "Cdecl Int")

if (val == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"