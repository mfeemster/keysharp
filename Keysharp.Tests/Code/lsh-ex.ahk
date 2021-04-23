;#Include %A_ScriptDir%/header.ahk

x := 1
y := x << 1

if (y = 2)
	FileAppend, pass, *
else
	FileAppend, fail, *