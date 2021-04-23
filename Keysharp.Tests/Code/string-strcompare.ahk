;#Include %A_ScriptDir%/header.ahk

x := "a"
y := "b"
z := StrCompare(x, y)

if (z = -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "a"
y := "a"
z := StrCompare(x, y)

if (z = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "b"
y := "a"
z := StrCompare(x, y)

if (z = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "a"
y := "B"
z := StrCompare(x, y)

if (z = -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "A"
y := "a"
z := StrCompare(x, y)

if (z = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "b"
y := "A"
z := StrCompare(x, y)

if (z = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "A"
y := "a"
z := StrCompare(x, y, 1)

if (z < 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "A11"
y := "A100"
z := StrCompare(x, y, 1)

if (z > 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "A11"
y := "A100"
z := StrCompare(x, y, "logical")

if (z < 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
	