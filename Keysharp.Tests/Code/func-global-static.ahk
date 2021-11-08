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
	static z := 33
	static zz := initfunc(1, 2), zzz := initfunc(3, 4) * 2

	If (z == 33)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	If (zz == 3)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	If (zzz == 14)
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
	static z

	if (z == "")
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
	static x := 111
	y := 22
	static z := 333, zz := initfunc(5, 6), zzz := initfunc(7, 8) * 2

	if (x == 111)
		FileAppend, pass, *
	else
		FileAppend, fail, *

	if (z == 333)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	If (zz == 11)
		FileAppend, pass, *
	else
		FileAppend, fail, *
	
	If (zzz == 30)
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
	static x
	static y
	static z

	x := 11
	y := 22
	z := 33

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