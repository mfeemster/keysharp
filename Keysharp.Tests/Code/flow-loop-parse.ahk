#Include %A_ScriptDir%/header.ahk

z := ""

Loop Parse "hello"
{
	z .= A_LoopField
}

If (z == "hello")
	FileAppend, pass, *
else
	FileAppend, fail, *

z := ""

Loop Parse "hello" {
	z .= A_LoopField
}

If (z == "hello")
	FileAppend, pass, *
else
	FileAppend, fail, *

z := ""

Loop Parse "hello", , "l"
{
	z .= A_LoopField
}

If (z == "heo")
	FileAppend, pass, *
else
	FileAppend, fail, *

z := ""

Loop Parse "hello", , "l" {
	z .= A_LoopField
}

If (z == "heo")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "hello"
z := ""

Loop Parse x
{
	z .= A_LoopField
}

If (z == "hello")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "hello"
z := ""

Loop Parse %x%
{
	z .= A_LoopField
}

If (z == "hello")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "hel,lo"
z := ""

Loop Parse x, ","
{
	z .= A_LoopField
}

If (z == "hello")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "hel,lo"
z := ""

Loop Parse x, ",", "l"
{
	z .= A_LoopField
}

If (z == "heo")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "hel,lo"
y := ","
z := ""

Loop Parse x, y, "l"
{
	z .= A_LoopField
}

If (z == "heo")
	FileAppend, pass, *
else
	FileAppend, fail, *

v := "l"
x := "hel,lo"
y := ","
z := ""

Loop Parse x, y, v
{
	z .= A_LoopField
}

If (z == "heo")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := """first field"",SecondField,""the word """"special"""" is quoted literally"",,""last field, has literal comma"""
z := ""

Loop Parse x, "csv"
{
	z .= A_LoopField
}

If (z == "first fieldSecondFieldthe word ""special"" is quoted literallylast field, has literal comma")
	FileAppend, pass, *
else
	FileAppend, fail, *
