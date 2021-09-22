x := 1
y := "x"

func()
{
%y% := 123
}

func()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 123)
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