; #Include %A_ScriptDir%/header.ahk

x := 1
y := 20
Loop
	x *= 2
Until x > y

if (x == 32)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 20
Loop
	x *= 2
Until (x > y)

if (x == 32)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 20
Loop
{
	x *= 2

	if (Mod(x, 2) == 1)
		continue
}
Until (x > y)

if (x == 32)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
Loop
{
	x++
}
Until (A_Index == 5)

if (x == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1

while true
{
	x++
}
Until (A_Index == 5)

if (x == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 5
z := "y"

Loop %z%
{
	x++

	If A_Index = 10
		break
}
Until (A_Index == 5)

if (x == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 5
z := "y"

Loop %z%
{
	x++

	If A_Index = 5
		break
}
Until (A_Index == 10)

if (x == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 5
str := ""

Loop y
{
	x++

	If A_Index = 5
		break
}
Until (str != "")

if (x == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 20
z := 0

Loop ; this is a comment
{
	x *= 2

	Loop ; and another
		z++
	Until z > x
} ; more comments
Until x > y ; last comment

if (x == 32)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (z == 33)
	FileAppend "pass", "*"
else
	FileAppend, "fail", "*"

arr := [10, 20, 30]
x := 0

for in arr
{
	x++
}
until x > 1

if (x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
