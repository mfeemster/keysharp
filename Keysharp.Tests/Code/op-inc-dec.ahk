x := 1
y := 25
y--
z := y--
x++

If (x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 23)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := 1
++y

If (y = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := "1"
++y

If (y = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := "1"
y++

If (y = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x--

If (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "2"
x--

If (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

--y

If (y = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := "2"
--y

If (y = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := y++

If (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := "0"
z := y++

If (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := 2
z := --y

If (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
y := "2"
z := --y

If (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (y == "1")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := y%x%++

if (z == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := 11
y11 := 123
z := ++y%x%

if (z == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := 11
y11 := 123
z := y%x%--

if (z == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := 11
y11 := 123
z := --y%x%

if (z == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
myfunc(xx)
{
	return xx
}

x := 11
y11 := 123
z := myfunc(y%x%++)

if (y%x% == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (z == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := myfunc(++y%x%)

if (y%x% == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (z == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := myfunc(y%x%--)

if (y%x% == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (z == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := myfunc(--y%x%)

if (y%x% == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (z == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"