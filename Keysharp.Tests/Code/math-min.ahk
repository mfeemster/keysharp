;#Include %A_ScriptDir%/header.ahk

if (Min(-6, -6) == -6)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (Min(-6, -5) == -6)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(-4.2, -5.0) == -5.0)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(0, 0) == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(0, 1) == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(1, 1) == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(1.5, 2.3) == 1.5)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(-1.0, "asdf") == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -1.0, -0.5, 0, 0.5, 1, 0.675 ]

if (Min(x) == -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 ]

if (Min(x) == -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(-1.0, -0.5, 0, 0.5, 1, 0.675) == -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Min(-1.0, -0.5, 0, 0.5, 1, 0.675, 2.0) == -1)
	FileAppend, pass, *
else
	FileAppend, fail, *
