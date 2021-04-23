;#Include %A_ScriptDir%/header.ahk

x := Ord("t")

if (x = 116)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := Ord("te", 2)
			
if (x = 101)
	FileAppend, pass, *
else
	FileAppend, fail, *
