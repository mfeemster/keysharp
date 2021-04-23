;#Include %A_ScriptDir%/header.ahk

x := "test"
y := [ 116, 0, 101, 0, 115, 0, 116, 0 ]
z := StrGet(y)

if (x = z)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
y := [ 116, 101, 115, 116 ]
z := StrGet(y, null, "us-ascii")

if (x = z)
	FileAppend, pass, *
else
	FileAppend, fail, *

z := StrGet(y, 2, "us-ascii")

if (z = "te")
	FileAppend, pass, *
else
	FileAppend, fail, *

z := StrGet(y, , "us-ascii")

if (x = z)
	FileAppend, pass, *
else
	FileAppend, fail, *
