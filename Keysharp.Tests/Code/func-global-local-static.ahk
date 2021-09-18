x := 1
y := 2
z := 3

func()
{
global x := 11
local y := 22
static z := 33

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

func()

If (x == 11)
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
xx := ""

func2()
{
global x := 11, xx := 111
local y := 22, yy := 222
static z := 33, zz := 333

If (x == 11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (xx == 111)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 22)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (yy == 222)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == 33)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (zz == 333)
	FileAppend, pass, *
else
	FileAppend, fail, *
}

func2()

If (x == 11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (xx == 111)
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