;#Include %A_ScriptDir%/header.ahk

PI = 3.1415926535897931

if (1.2246467991473532E-16 == Tan(-1 * PI))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (-16331239353195370 == Tan(-0.5 * PI))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Tan(0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Tan(-0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (16331239353195370 == Tan(0.5 * PI))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (-1.2246467991473532E-16 == Tan(1 * PI))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (-1.63185168712879 == Tan(0.675 * PI))
	FileAppend, pass, *
else
	FileAppend, fail, *