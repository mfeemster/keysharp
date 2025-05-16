; #Include %A_ScriptDir%/header.ahk

x = 0

while true
{
	x++
	
	if (x > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


x = 0

while true {
	x++
	
	if (x > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while (true)
{
	x++
	
	if (A_Index > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while (true) {
	x++
	
	if (A_Index > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while 1
{
	x++
	
	if (x > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while 1 {
	x++
	
	if (x > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0
str := ""

while (str = "") {
	x++
	
	if (x > 4)
		break
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x = 0

while (x < 5)
{
	x++
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while (x < 5) {
	x++
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while x < 5
{
	x++
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x = 0

while x < 5 {
	x++
}

If x = 5
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (A_ThisFunc != "TestFunc")
{
	x := 0
	y := 5
	z5 := 100

	while z%y% ; this is a comment
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

	while z%y% { ; another comment
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

	while (z%y%) {
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

while (x  < 2) {
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

while (x < 1)
	x++
else
{
	b := true
}

If (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	