x := 1
y := 25
y--
z := y--
x++

If (x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 23)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := 1
++y

If (y = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x--

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

--y

If (y = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := y++

If (z = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := 2
z := --y

If (z = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"