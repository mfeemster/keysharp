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

func2(a, b, c := 123)
{
	global x := a
	global y := b
	global z := c
}

x := 1
y := 2
z := 3
func2(11, 22)

If (x == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 22)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 1
y := 2
z := 3
func2(,)

If (x == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3
func2(, 22, 33)

If (x == null)
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
	
x := 1
y := 2
z := 3
func2(11,,33)

If (x == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 33)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3
func2(11,)

If (x == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3
func2(11,,)

If (x == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3
func2(,22,)

If (x == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 22)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 1
y := 2
z := 3
func2(,,)

If (x == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == null)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := false

func3(a, b, c := unset)
{
	if (IsSet(c))
	{
		global x := true
	}
}

func3(,)

If (x == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false

func3(1,)

If (x == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false

func3(1, 2)

If (x == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false

func3(1, 2, 3)

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false

func3(,)

If (x == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false

func3(,,)

If (x == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; test variadic functions here.

varfunc1(*)
{
	global x := true
}

x := false
varfunc1()

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfunc1("firstparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfunc1("firstparam", "secondparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfunc1("firstparam", ,"thirdparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfo1 := FuncObj("varfunc1")

x := false
varfo1()

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfo1("firstparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfo1("firstparam", "secondparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfo1("firstparam", ,"thirdparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc2(p1, theparams*)
{
	temp := p1

	for n in theparams
	{
		temp += theparams[A_Index]
	}

	return temp
}

val := varfunc2(1, 2, 3)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc3(p1, theparams*)
{
	temp := p1

	for n in theparams
	{
		temp += n
	}

	return temp
}

val := varfunc3(1, 2, 3)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc4(*)
{
	return args[1] + args[2] + args[3]
}

val := varfunc3(1, 2, 3)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"