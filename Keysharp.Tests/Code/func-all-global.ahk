x := 1
y := 2
z := 3

initfunc(a, b)
{
	return a + b
}

func()
{
	global x := 11, y := 22, z := 33

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

	x := 101, y := 202, z := 303

	If (x == 101)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (y == 202)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (z == 303)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

func()

If (x == 101)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 202)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z == 303)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := 100
b := 200
c := 300

func2()
{
	global
	a := 111
	b := 222
	c := 333

	If (a == 111)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (b == 222)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (c == 333)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

func2()

If (a == 111)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (b == 222)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (c == 333)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

func3()
{
	global x := initfunc(1, 2), y := initfunc(3, 4) * 2, z := initfunc(5, 6) * 3

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
}

func3()

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