x := 1
y := 2
z := 3

func()
{
global x := 11, y := 22, z := 33
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

If (z == 33)
	FileAppend, pass, *
else
	FileAppend, fail, *

a := 100
b := 200
c := 300

func2()
{
global
a := 111
b := 222
c := 333
}

func2()

If (a == 111)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (b == 222)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (c == 333)
	FileAppend, pass, *
else
	FileAppend, fail, *
