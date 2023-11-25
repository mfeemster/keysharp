;#Include %A_ScriptDir%/header.ahk

x = 1

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x != 1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
xneg = -1

If (xneg == -1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (xneg != -1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"