;#Include %A_ScriptDir%/header.ahk

if (-1 == Ceil(-1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-2 == Ceil(-2.1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Ceil(0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Ceil(-0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (2 == Ceil(1.000001))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
