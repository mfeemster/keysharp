;#Include %A_ScriptDir%/header.ahk

x := 10
y := x / 10

if (y = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
y := x / 2.5

if (y = 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 3
y := x / 2

if (y = 1.5)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := x // 3

if (y = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := x // -3

if (y = -2)
	FileAppend, pass, *
else
	FileAppend, fail, *