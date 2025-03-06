x := 2
y := -2

If (y = -2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != -2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

y := -y

If (y = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != 2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

y := -(x * y)

If (y = -4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != -4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

y := y * -1

If (y = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != 4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

y := y / -1

If (y = -4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != -4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

y := -(y / -2)

If (y = -2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != -2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
y := -4 + 5 * -10

If (y = -54)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != -54)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

y := -2.5

If (y = -2.5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := 2.5
y := -y

If (y = -2.5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "2.5"
y := -y

If (y = -2.5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "-2.5"
y := -y

If (y = 2.5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "0x0A"
y := -y

If (y = -10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "-0x0A"
y := -y

If (y = 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"