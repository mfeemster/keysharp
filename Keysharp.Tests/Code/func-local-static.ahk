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
local x := 22

if (x == 22)
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
static z := 333

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
local x := 11, y := 22, z := 33

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