;#Include %A_ScriptDir%/header.ahk

x := "a"
y := "a"

If (x = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x == y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x != y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x !== y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x = y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x == y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"	

x := "a"
y := "A"

If (x = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x == y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x != y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x !== y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x = y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x == y))
	FileAppend, "pass", "*"	
else
	FileAppend, "fail", "*"
