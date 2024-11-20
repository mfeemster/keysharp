x := 1
y := 2
z := 3

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

funcdef1(p := '')
{
	if (p == "")
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

funcdef1()

funcdef2(p := '"')
{
	if (p == "`"")
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

funcdef2()

funcdef3(p := 'asdf')
{
	if (p == "asdf")
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

funcdef3()