;#Include %A_ScriptDir%/header.ahk

x := 1
y := &x

if (x = y)
	FileAppend, fail, *
else
	FileAppend, pass, *