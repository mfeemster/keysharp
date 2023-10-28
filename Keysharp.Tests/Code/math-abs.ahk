;#Include %A_ScriptDir%/header.ahk

if (1 == Abs(1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1 == Abs(-1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (9.81 == Abs(-9.81))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Abs(0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Abs(-0))
	FileAppend, pass, *
else
	FileAppend, fail, *