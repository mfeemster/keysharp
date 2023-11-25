;#Include %A_ScriptDir%/header.ahk

x := Ord("t")

if (x = 116)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := Ord("et")
			
if (x = 101)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
