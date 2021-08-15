;#Include %A_ScriptDir%/header.ahk

x := 2
y := x >> 1

if (y == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := -1
y := x >> 1

if (y == 0xffffffffffffffff)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := -1
y := x >>> 1

if (y == 0x7fffffffffffffff)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
y := 1
z := x >> y

if (z == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	x := 1
	y := x >> 1.2
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	x := 1.2
	y := x >> 1
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	x := 1.2
	y := 3.4
	z := x >> y
}
catch (TypeError as exc)
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	x := 1
	y := -1
	z := x >> y
}
catch (Error as exc)
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

b := false

try
{
	x := 1
	y := 64
	z := x >> y
}
catch (Error as exc)
{
	b := true
}

if (b == true)
	FileAppend, pass, *
else
	FileAppend, fail, *