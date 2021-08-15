;#Include %A_ScriptDir%/header.ahk

x := "this is a string"
y := " and another string"
z := x . y

If (z = "this is a string and another string")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 123
y := 456
z := x . y

If (z = "123456")
	FileAppend, pass, *
else
	FileAppend, fail, *

z := ""
z := x y

If (z = "123456")
	FileAppend, pass, *
else
	FileAppend, fail, *

z = The number is %x%

If (z = "The number is 123")
	FileAppend, pass, *
else
	FileAppend, fail, *


z := "The number is " . x * 10

If (z = "The number is 1230")
	FileAppend, pass, *
else
	FileAppend, fail, *


z := "The number is"
. " another line"

If (z = "The number is another line")
	FileAppend, pass, *
else
	FileAppend, fail, *
