;#Include %A_ScriptDir%/header.ahk

x := 1
y := x

If (x = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (x != 1)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (y = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (y != 1)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (x != y)
	FileAppend, fail, *
else
	FileAppend, pass, *