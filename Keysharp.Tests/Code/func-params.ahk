x := 1
y := 2
z := 3

func(a, b, c)
{
global x := a
global y := b
global z := c
}

func(11, 22, 33)

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
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 22)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *
