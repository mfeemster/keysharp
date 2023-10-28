initfunc(a, b)
{
	return a + b
}

func()
{
	x := 1
	y := 2
	static z := 3

	If (x == 1)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	If (y == 2)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	If (z == 3)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func()

x := 1
y := 2
z := 3

func2()
{
	static
	local x := 22, y := initfunc(10, 20)

	if (x == 22)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	if (y == 30)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func2()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 2)
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
	static x := 111
	y := 22
	static z := 333, zz := initfunc(1, 2)

	if (x == 111)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (y == 22)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (z == 333)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	if (zz == 3)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func3()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 2)
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
	static
	local x := 11, y := 22, z := 33, zz := initfunc(5, 6)

	if (x == 11)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (y == 22)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (z == 33)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	if (zz == 11)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	x := 111, y := 222, z := 333, zz := initfunc(7, 8)

	if (x == 111)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (y == 222)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (z == 333)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	if (zz == 15)
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

func4()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 2)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *