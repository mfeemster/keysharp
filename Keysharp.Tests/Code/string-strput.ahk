;#Include %A_ScriptDir%/header.ahk

x := "test"
y := [ 116, 0, 101, 0, 115, 0, 116, 0 ]
z := StrPut(x)

if (y = z)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
y := [ 116, 101, 115, 116 ]
z := StrPut(x, , "UTF-8")

if (y = z)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := [ 116, 0, 101, 0 ]
z := StrPut(x, 2)

if (y = z)
	FileAppend, pass, *
else
	FileAppend, fail, *