;#Include %A_ScriptDir%/header.ahk
b = blue
o = ooo
r = red
x = xxx
z = zzz

If o not between %b% and %r%
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If o not between %r% and %b%
	FileAppend, pass, *
else
	FileAppend, fail, *

If o not between %x% and %z%
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If o not between %z% and %x%
	FileAppend, pass, *	
else
	FileAppend, fail, *