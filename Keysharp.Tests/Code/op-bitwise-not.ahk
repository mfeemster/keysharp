;#Include %A_ScriptDir%/header.ahk

x := 1
y := ~x

If (y == -2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := ~y

If (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := ~(-2)

If (z == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 5000000000
y := ~x

If (y = -5000000001)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -5000000000
y := ~x

If (y = 4999999999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -5000000000
y := ~x

If (y = 4999999999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

b := false

try
{
	x := 1.234
	y := ~x
}
catch (TypeError as exc)
{
	b := true
}

If (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

b := false

try
{
	x := -2.345
	y := ~x
}
catch (TypeError as exc)
{
	b := true
}

If (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
x := "asdf"
b := false

try
{
	y := ~x
}
catch (TypeError as exc)
{
	b := true
}

If (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
