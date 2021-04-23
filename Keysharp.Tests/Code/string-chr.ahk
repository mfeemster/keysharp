;#Include %A_ScriptDir%/header.ahk

x := Chr(116)

if (x = "t")
	FileAppend, pass, *
else
	FileAppend, fail, *