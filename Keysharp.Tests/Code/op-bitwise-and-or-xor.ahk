x := 1
y := 2
z := x & y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x1"
y := "2"
z := x & y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	x := 1.234
	y := 2.456
	z := x & y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	z := x | y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	z := x ^ y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	x := "1.234"
	y := 2.456
	z := x & y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	z := x | y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

b := false

try
{
	z := x ^ y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -1
y := -2
z := x & y

if (z = -2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-1"
y := "-0x2"
z := x & y

if (z = -2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 0
z := x & y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x1"
y := "0"
z := x & y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0
y := 0
z := x & y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0"
y := "0x0"
z := x & y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x | y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

z := x ^ y

if (z = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"