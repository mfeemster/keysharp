;#Include %A_ScriptDir%/header.ahk
o = ooo

If o between blue and red
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If o between red and blue
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If o between xxx and zzz
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If o between zzz and xxx
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"	