; #Include %A_ScriptDir%/header.ahk

y = 5
x = 0

Loop %y%
{
	If A_Index =2
		Continue
	x := x + A_Index
	If A_Index =4
		Break
}

If x =8
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x = 0

Loop %y% {
	If A_Index =2
		Continue
	x := x + A_Index
	If A_Index =4
		Break
}

If x =8
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


x = 0

Loop y
{
	If A_Index =2
		Continue
	x := x + A_Index
	If A_Index =4
		Break
}

If x =8
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
x = 0

Loop y {
	If A_Index =2
		Continue
	x := x + A_Index
	If A_Index =4
		Break
}

If x =8
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

Loop y
	x++

If x =5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x = 0

Loop y
	if (A_Index == 1)
		x++
	else
		x += 2

If x =9
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0
y := ""

Loop %y%
{
	x++
}

If x = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x = 0
y = 0

Loop %y%
{
	x++
}

If x = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop
{
	if (A_Index > 25)
		break
	
	x++
}

If x = 25
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop {
	if (A_Index > 25)
		break
	
	x++
}

If x = 25
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0

Loop 100
{
	if (A_Index > 25)
		break
	
	x++
}

If x = 25
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop (100)
{
	if (A_Index > 25)
		break
	
	x++
}

If x = 25
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop(100)
{
	if (A_Index > 25)
		break
	
	x++
}

If x = 25
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0

Loop 100 {
	if (A_Index > 25)
		break
	
	x++
}

If x = 25
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If A_Index = 0
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_ThisFunc != "TestFunc")
{
	x := 0
	y := 5
	z5 := 100
 
	Loop z%y% ; this is a comment
	{
		if (A_Index > 25)
			break
	
		x++
	}

	If x = 25
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If A_Index = 0
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	x := 0
	y := 5
	z5 := 100
 
	Loop z%y% { ; another comment
		if (A_Index > 25)
			break
	
		x++
	}

	If x = 25
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If A_Index = 0
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

x := 1
b := false

Loop 1
{
	x++
}
else
{
	b := true
}

If (b == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 1
b := false

Loop 0
	x++
else
{
	b := true
}

If (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

		
x := 0
y := 5

Loop y + 1 {
	x++
}

If (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0

Loop (y + 1) {
	x++
}

If (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop (y + 1)
	x++

If (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop y + 1
	x++

If (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

Loop 1 * 2 * 3
	x++

If (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"