;#Include %A_ScriptDir%/header.ahk
b = blue
o = ooo
r = red
x = xxx
z = zzz

If o between %b% and %r%
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If o between %r% and %b%
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If o between %x% and %z%
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If o between %z% and %x%
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"	