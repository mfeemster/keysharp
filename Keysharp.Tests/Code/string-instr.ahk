;#Include %A_ScriptDir%/header.ahk

x := "the string to searchz"
y := "the"
z := InStr(x, y)

if (z = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
y := "z"
z := InStr(x, y)

if (z = 21)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
y := "Z"
z := InStr(x, y, 1)

if (z = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

z := InStr(x, y, "on")

if (z = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := "g"
z := InStr(x, y, 0, 12)

if (z = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := "s"
z := InStr(x, y, 0, 1, 2)

if (z = 15)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := "z"
z := InStr(x, y, 0, -1)

if (z = 21)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := "h"
z := InStr(x, y, 0, -1, 2)

if (z = 2)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := "t"
z := InStr(x, y, 0, -1, 3)

if (z = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *