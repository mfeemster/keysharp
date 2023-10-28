;#Include %A_ScriptDir%/header.ahk

x := 123
y := String(x)

if (y = "123")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "123"
y := String(x)

if (y = "123")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1.234
y := String(x)

if (y = "1.234")
	FileAppend, pass, *
else
	FileAppend, fail, *
