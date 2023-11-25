;#Include %A_ScriptDir%/header.ahk
o = ooo

If o not between blue and red
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If o not between red and blue
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If o not between xxx and zzz
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If o not between zzz and xxx
	FileAppend, "pass", "*"	
else
	FileAppend, "fail", "*"