desktop := GetDesktopWindow()
buf := Buffer(16, 0)
DllCall("user32.dll\GetWindowRect", "ptr", desktop, "ptr", buf)
l := NumGet(buf, 0, "UInt")
t := NumGet(buf, 4, "UInt")
r := NumGet(buf, 8, "UInt")
b := NumGet(buf, 12, "UInt")

if (r > 0 && b > 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

str := "lower"
len := StrLen(str)
strbuf := StringBuffer(str)
DllCall("user32.dll\CharUpperBuff", "ptr", strbuf, "UInt", len)

if (strbuf == StrUpper(str))
	FileAppend, pass, *
else
	FileAppend, fail, *