x := 1
y := 2
z := 3

func()
{
x := 11
y := 22
z := 33
}

func()

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

a := 100
b := 200
c := 300

func2()
{
local a := 111
local b := 222
local c := 333
}

func2()

If (a == 100)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (b == 200)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (c == 300)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := fa() . fb() . fc("fa()")

if (x == "l!fa()z")
	FileAppend, pass, *
else
	FileAppend, fail, *

fa()
{
	return "l"
}

fb(x)
{
;	return x
	return "!" . x
}

fc(x, y := "z")
{
	return x . y
}
