;#Include %A_ScriptDir%/header.ahk

if (-1 == Truncate(-1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (-4 == Truncate(-4.5))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Truncate(0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Truncate(-0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (2 == Truncate(2.5))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (1 == Truncate(1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (8 == Truncate(8.675))
	FileAppend, pass, *
else
	FileAppend, fail, *