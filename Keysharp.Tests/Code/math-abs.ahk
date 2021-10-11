;#Include %A_ScriptDir%/header.ahk

if (1 == Abs(1))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := [ 1 ]

if (1 == Abs(x))
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -1 ]

if (1 == Abs(-1))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (1 == Abs(x))
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -9.81 ]

if (9.81 == Abs(-9.81))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (9.81 == Abs(x))
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ 0 ]

if (0 == Abs(0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Abs(x))
	FileAppend, pass, *
else
	FileAppend, fail, *

x := [ -0 ]

if (0 == Abs(-0))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (0 == Abs(x))
	FileAppend, pass, *
else
	FileAppend, fail, *
