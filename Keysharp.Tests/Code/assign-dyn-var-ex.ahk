;#Include %A_ScriptDir%/header.ahk

x = 11
y11 = %x%
z := y%x%

If (z == y11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x == y11)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (z == x)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z != y11)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (x != y11)
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If (z != x)
	FileAppend, fail, *
else
	FileAppend, pass, *