x := 1
y := 2
z := 3

initfunc(a, b)
{
	return a + b
}

func(a, b, c)
{
	global x := a
	global y := b
	global z := c
}

func(11, 22, 33)

If (x == 11)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 22)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z == 33)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

func(initfunc(1, 2), initfunc(3, 4) * 2, initfunc(5, 6) * 3)

If (x == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 14)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z == 33)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"