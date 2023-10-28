x := 1
y := "x"

func()
{
	%y% := 123
}

func()

If (x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == "x")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 11
y11 := 123

func2()
{
	y%x% := 222
}

func2()

If (x == 11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y11 == 222)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "unc"
y := 0

myfunc()
{
	global y := 999
}

myf%x%()

If (y == 999)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "unc2"
y := 0

myfunc2(funcparam)
{
	global y := funcparam
}

myf%x%(123)

If (y == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "myfunc"
y := 0

%x%()

If (y == 999)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "myfunc2"
y := 0

%x%(123)

If (y == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *