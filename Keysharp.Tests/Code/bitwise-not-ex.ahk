;#Include %A_ScriptDir%/header.ahk

x := 1
y := ~x

If (y = 4294967294)
	FileAppend, pass, *
else
	FileAppend, fail, *

z := ~y

If (z = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

z := ~(-2)

If (z = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := 5000000000
y := ~x

If (y = -5000000001)
	FileAppend, pass, *
else
 	FileAppend, fail, *

x := -5000000000
y := ~x

If (y = 4999999999)
	FileAppend, pass, *
else
 	FileAppend, fail, *

x := -5000000000
y := ~x

If (y = 4999999999)
	FileAppend, pass, *
else
 	FileAppend, fail, *

x := 1.234
y := ~x

If (y = 4294967294)
	FileAppend, pass, *
else
 	FileAppend, fail, *

x := -2.345
y := ~x

If (y = 1)
	FileAppend, pass, *
else
 	FileAppend, fail, *
	
x := "asdf"
y := ~x

If (y = "asdf")
	FileAppend, pass, *
else
 	FileAppend, fail, *
	