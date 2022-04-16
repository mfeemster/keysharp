;#Include %A_ScriptDir%/header.ahk

x := 10
x += 100

if (x = 110)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x += -100

if (x = -90)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x -= 100

if (x = -90)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x -= -100

if (x = 110)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x *= 100

if (x = 1000)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x *= -100

if (x = -1000)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x /= 100

if (x = 0.1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x /= -100

if (x = -0.1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 10
x //= 100

if (x = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 5
x //= -2

if (x = -2)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "first"
x .= "second"

if (x = "firstsecond")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
x |= 2

if (x = 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
x &= 2

if (x = 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
x ^= 2

if (x = 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

x :=
x += 1

if (x = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 8
x >>= 2

if (x = 2)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 1
x <<= 2

if (x = 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := -1
x >>>= 1

if (x == 0x7fffffffffffffff)
	FileAppend, pass, *
else
	FileAppend, fail, *

; The parser takes special action for combined assignments on properties, so ensure they work here.
m := Map()
m.Default := 4
m.Default += 123

if (m.Default == 127)
	FileAppend, pass, *
else
	FileAppend, fail, *

m.Default -= 123

if (m.Default == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

m.Default *= 123

if (m.Default == 492)
	FileAppend, pass, *
else
	FileAppend, fail, *

m.Default //= 123

if (m.Default == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

m.Default &= 123

if (m.Default == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

m.Default |= 123

if (m.Default == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *