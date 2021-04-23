;#Include %A_ScriptDir%/header.ahk

if (0 == Sqrt(0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1 == Sqrt(1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (2 == Sqrt(4))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (3 == Sqrt(9))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (6 == Sqrt(36))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (113 == Sqrt(12769))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (2.8284271247461903 == Sqrt(8))
	FileAppend, pass, *
else
	FileAppend, fail, *