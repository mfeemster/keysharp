x := 1
y := 2
z := 3

initfunc(a, b)
{
	return a + b
}

func()
{
	x := 11
	y := 22
	z := 33
	
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
}

func()

If (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := 100
b := 200
c := 300

func2()
{
	local a := 111
	local b := 222
	local c := 333

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

	a := 11
	b := 22
	c := 33

	If (a == 11)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (b == 22)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (c == 33)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

func2()

If (a == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (b == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (c == 300)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := 100
b := 200
c := 300

func3()
{
	local a := 444, b := 555, c := 666

	If (a == 444)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (b == 555)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (c == 666)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	a := 777
	b := 888
	c := 999

	If (a == 777)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (b == 888)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	If (c == 999)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

func3()

If (a == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (b == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (c == 300)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := fa() . fb() . fc("fa()")

if (x == "l!fa()z")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

fa()
{
	return "l"
}

fb(x := "")
{
;	return x
	return "!" . x
}

fc(x, y := "z")
{
	return x . y
}

x := 1
y := 2
z := 3

func4()
{
	local x := initfunc(1, 2), y := initfunc(3, 4) * 2, z := initfunc(5, 6) * 3

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

func4()

If (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (z == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"