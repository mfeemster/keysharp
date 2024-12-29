

x := 1
y := 25
y--
z := y--
x++

If (x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (x != 2)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

y := 1
++y

If (y = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y != 2)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

x--

If (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (x != 1)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

--y

If (y = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y != 1)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

z := y++

If (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z != 1)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

z := --y

If (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z != 1)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"