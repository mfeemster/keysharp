x := 1
y := 2
z := 3

initfunc(a, b)
{
	return a + b
}

func()
{
	global x := 11, y := 22
	local z := 33

	If (x == 11)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (y == 22)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (z == 33)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	z := 44

	If (z == 44)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func()

If (x == 11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 22)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
y := 2
z := 3

func2()
{
	global x := 11
	global y := 22
	local z := 33

	If (x == 11)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (y == 22)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (z == 33)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func2()

If (x == 11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 22)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
y := 2
z := 3

func3()
{
	global
	y := 22
	local x := 11, z := 33

	If (x == 11)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (y == 22)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (z == 33)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func3()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 22)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
y := 2
z := 3

func4()
{
	global
	y := initfunc(1, 2)
	local x := initfunc(3, 4) * 2, z := initfunc(5, 6) * 3

	If (x == 14)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (y == 3)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (z == 33)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func4()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

funcretval(xx)
{
	return xx
}

func5()
{
	global y := funcretval(x) ; Since x is a function argument it should not create it and instead use the global.
}

x := 123
y := 0

func5()

If (y == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *
