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

func(initfunc(1, 2), initfunc(3, 4) * 2, initfunc(5, 6) * 3)

If (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 14)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 33)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

myfunc(a, b, c)
{
	return a + b + c
}

myfunc(xx := 1, yy := 2, zz := 3)

if (xx == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (yy == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (zz == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := myfunc(xxx := 1, yyy := 2, zzz := 3)

if (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (xxx == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (yyy == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (zzz == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := myfunc((x4 := 3) / 2, (y4 := 2) + 2, (z4 := 1) * 2) ; The parsing is a little trickier in nested expressions as arguments.

if (x4 == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y4 == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (z4 == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (val == 7.5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Try nested arguments referring the global, local and static vars.

myfunc2(xx)
{
	return xx
}

x := 10

TestParamFunc() {
	global x--
	static yy := 123
	local ll := 8
	
	if (x == 9)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	val := myfunc2((x := 10) / 5)
	
	if (val == 2 && x == 10)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
		
	if (yy == 123)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	val := myfunc2((yy := 9) / 3)
	
	if (val == 3 && yy == 9)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	if (ll == 8)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	val := myfunc2((ll := 20) * 5)
	
	if (val == 100 && ll == 20)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

TestParamFunc()