;#Include %A_ScriptDir%/header.ahk

x := 10
y := x / 10

if (y = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
y := x / 2.5

if (y = 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 3
y := x / 2

if (y = 1.5)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := x // 3

if (y = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := x // -3

if (y = -1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := 0
res := false

try
{
	z := x / y
}
catch (ZeroDivisionError as exc)
{
	res := true
}

if (res == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := 0
res := false

try
{
	z := x // y
}
catch (ZeroDivisionError as exc)
{
	res := true
}

if (res == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
y := 1.234
res := false

try
{
	z := x // y
}
catch (TypeError as exc)
{
	res := true
}

if (res == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5.123
y := 2
res := false

try
{
	z := x // y
}
catch (TypeError as exc)
{
	res := true
}

if (res == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5.123
y := 2.456
res := false

try
{
	z := x // y
}
catch (TypeError as exc)
{
	res := true
}

if (res == true)
	FileAppend, pass, *
else
	FileAppend, fail, *
