;#Include %A_ScriptDir%/header.ahk

x := A_Space
y := A_Tab

if (x = " ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y = "`t")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
