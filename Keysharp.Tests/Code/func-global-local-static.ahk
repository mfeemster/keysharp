x := 1
y := 2
z := 3

initfunc(a, b)
{
	return a + b
}

func()
{
	global x := 11
	local y := 22
	static z := 33

	If (x == 11)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (y == 22)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (z == 33)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

func()

If (x == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3
xx := ""

func2()
{
	global x := 11, xx := 111
	local y := 22, yy := 222
	static z := 33, zz := 333

	If (x == 11)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (xx == 111)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (y == 22)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	If (yy == 222)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (z == 33)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (zz == 333)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

func2()

If (x == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (xx == 111)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3
xx := ""

func3()
{
	global x := initfunc(1, 2), xx := initfunc(3, 4) * 2
	local y := initfunc(5, 6), yy := initfunc(7, 8) * 3
	static z := initfunc(9, 10) * 4, zz := initfunc(11, 12) * 5

	If (x == 3)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (xx == 14)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (y == 11)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	If (yy == 45)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (z == 76)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	If (zz == 115)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

func3()

If (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (xx == 14)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"