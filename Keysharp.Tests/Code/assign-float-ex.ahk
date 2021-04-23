;#Include %A_ScriptDir%/header.ahk

x := 1.123

If (x = 1.123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x != 1.123)
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If x = 1.123
	FileAppend, pass, *
else
	FileAppend, fail, *

If x != 1.123
	FileAppend, fail, *
else
	FileAppend, pass, *