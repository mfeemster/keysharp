;#Include %A_ScriptDir%/header.ahk

x := 2
y := x >> 1

if (y = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *