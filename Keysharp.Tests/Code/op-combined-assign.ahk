x := 10
x += 100

if (x = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x += "100"

if (x = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x += "0x64"

if (x = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x += -100

if (x = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x += "-100"

if (x = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x += "-0x64"

if (x = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x -= 100

if (x = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x -= "100"

if (x = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x -= "0x64"

if (x = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x -= -100

if (x = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x -= "-100"

if (x = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x -= "-0x64"

if (x = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x *= 100

if (x = 1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x *= "100"

if (x = 1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x *= "0x64"

if (x = 1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x *= -100

if (x = -1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x *= "-100"

if (x = -1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x *= "-0x64"

if (x = -1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x /= 100

if (x = 0.1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x /= "100"

if (x = 0.1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x /= "0x64"

if (x = 0.1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x /= -100

if (x = -0.1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x /= "-100"

if (x = -0.1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x /= "-0x64"

if (x = -0.1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x //= 100

if (x = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x //= "100"

if (x = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
x //= "0x64"

if (x = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 5
x //= -2

if (x = -2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 5
x //= "-2"

if (x = -2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 5
x //= "-0x02"

if (x = -2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "first"
x .= "second"

if (x = "firstsecond")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x |= 2

if (x = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x |= "2"

if (x = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x |= "0x2"

if (x = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x &= 2

if (x = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x &= "2"

if (x = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x &= "0x2"

if (x = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x ^= 2

if (x = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x ^= "2"

if (x = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x ^= "0x2"

if (x = 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x :=
x += 1

if (x = 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 3
x :=
x += "1"

if (x = 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 3
x :=
x += "0x1"

if (x = 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 8
x >>= 2

if (x = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 8
x >>= "2"

if (x = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 8
x >>= "0x02"

if (x = 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x <<= 2

if (x = 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x <<= "2"

if (x = 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
x <<= "0x2"

if (x = 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -1
x >>>= 1

if (x == 0x7fffffffffffffff)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -1
x >>>= "1"

if (x == 0x7fffffffffffffff)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -1
x >>>= "0x1"

if (x == 0x7fffffffffffffff)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; The parser takes special action for combined assignments on properties, so ensure they work here.
m := Map()
m.Default := 4
m.Default += 123

if (m.Default == 127)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

m.Default -= 123

if (m.Default == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

m.Default *= 123

if (m.Default == 492)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

m.Default //= 123

if (m.Default == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

m.Default &= 123

if (m.Default == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

m.Default |= 123

if (m.Default == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; Special care is taken in the parser to not call the code before the last property more than once, so ensure that works here.

x := 0

emptyfunc()
{
	global x
	x++
}

a := {b:1}

(1 ? (emptyfunc(), a) : (emptyfunc(), a)).b *= 2

if (x == 1) ; Ensure emptyfunc() was only called once.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class aclass
{
	b := bclass()
}

class bclass
{
	_c := 0
	ct := 0

	c
	{
		get
		{
			this.ct += 1
			return this._c
		}
	}

	__New()
	{
		this._c := cclass()
	}
}

class cclass
{
	d := 1
}

a := aclass()
a.b.c.d *= 2

if (a.b.ct == 1) ; Ensure a.b.c was only called once.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := aclass()
a.b.c.d++

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := aclass()
++a.b.c.d

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
a := aclass()
a.b.c.d--

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
		
a := aclass()
--a.b.c.d

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
a := aclass()
x := a.b.c.d++

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := aclass()
x := ++a.b.c.d

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := aclass()
x := a.b.c.d--

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := aclass()
x := --a.b.c.d

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
; Compound operator with prefix or postfix increment/decrement.

a := aclass()
x := 2
x *= a.b.c.d++

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
a := aclass()
x := 2
x *= ++a.b.c.d

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := aclass()
x := 2
x *= a.b.c.d--

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
a := aclass()
x := 2
x *= --a.b.c.d

if (a.b.ct == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (a.b.c.d == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := 2
z *= y%x%++

if (z == 246)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := 2
z *= ++y%x%

if (z == 248)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 124)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := 11
y11 := 123
z := 2
z *= y%x%--

if (z == 246)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 11
y11 := 123
z := 2
z *= --y%x%

if (z == 244)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y%x% == 122)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"