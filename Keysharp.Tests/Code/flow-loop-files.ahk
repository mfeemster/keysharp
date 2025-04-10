#Include %A_ScriptDir%/header.ahk

y = 5
x = 0

Loop, %y%
{
	If A_Index =2
		Continue
	x := x + A_Index
	If A_Index =4
		Break
}

If x =8
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x = 0
y := ""

Loop, %y%
{
	x++
}

If x = 0
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x = 0
y = 0

Loop, %y% ; this is a comment
{
	x++
}

If x = 0
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"