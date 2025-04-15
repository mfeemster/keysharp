x := 1
y := 25
y--
z := y--
x++

If (x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 23)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := 1
++y

If (y = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "1"
++y

If (y = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "1"
y++

If (y = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x--

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "2"
x--

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

--y

If (y = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "2"
--y

If (y = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := y++

If (z = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := "0"
z := y++

If (z = 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := 2
z := --y

If (z = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := "2"
z := --y

If (z = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == "1")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := y%x%++

if (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 11
y11 := 123
z := ++y%x%

if (z == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 11
y11 := 123
z := y%x%--

if (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 11
y11 := 123
z := --y%x%

if (z == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
myfunc(xx)
{
	return xx
}

x := 11
y11 := 123
z := myfunc(y%x%++)

if (y%x% == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := myfunc(++y%x%)

if (y%x% == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (z == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := myfunc(y%x%--)

if (y%x% == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := myfunc(--y%x%)

if (y%x% == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (z == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

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
			global ct++
			return _c
		}
	}

	__New()
	{
		global _c := cclass()
	}
}

class cclass
{
	d := 1
}

a := aclass()
a.b.c.d++

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := aclass()
++a.b.c.d

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
a := aclass()
a.b.c.d--

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
a := aclass()
--a.b.c.d

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
a := aclass()
x := a.b.c.d++

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := aclass()
x := ++a.b.c.d

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := aclass()
x := a.b.c.d--

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := aclass()
x := --a.b.c.d

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
; Compound operator with prefix or postfix increment/decrement.

a := aclass()
x := 2
x *= a.b.c.d++

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
a := aclass()
x := 2
x *= ++a.b.c.d

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a := aclass()
x := 2
x *= a.b.c.d--

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
a := aclass()
x := 2
x *= --a.b.c.d

if (a.b.ct == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (a.b.c.d == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := 2
z *= y%x%++

if (z == 246)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := 2
z *= ++y%x%

if (z == 248)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 11
y11 := 123
z := 2
z *= y%x%--

if (z == 246)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 11
y11 := 123
z := 2
z *= --y%x%

if (z == 244)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y%x% == 122)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
arr := [1, 2, 3]
prev := arr[2]++

if (prev == 2 && arr[2] == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [1, 2, 3]
prev := arr[2]--

if (prev == 2 && arr[2] == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [1, 2, 3]
newval := ++arr[2]

if (newval == 3 && arr[2] == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [1, 2, 3]
newval := --arr[2]

if (newval == 1 && arr[2] == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map(x, 1)
prev := m[x]++

if (prev == 1 && m[x] == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map(x, 1)
prev := m[x]--

if (prev == 1 && m[x] == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map(x, 1)
prev := ++m[x]

if (prev == 2 && m[x] == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map(x, 1)
prev := --m[x]

if (prev == 0 && m[x] == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"