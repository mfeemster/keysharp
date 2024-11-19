class testclass
{
	a := ""
	b := ""
	c := ""
	
	__New(_a, _b, _c)
	{
		global
		a := _a
		b := _b
		c := _c
	}
}

class testsubclass extends testclass
{
	x := ""
	y := ""
	z := ""
	zz := ""
	
	__New(p1, p2, p3, p4)
	{
		global
		super.__New(p1, p2, p3)
		x := p1 * 10
		y := p2 * 10
		z := p3 * 10
		zz := p4 * 10
	}
}

testclassobj := testclass(1, 2, 3)
testsubclassobj := testsubclass(1, 2, 3, 4)

val := testclassobj.a

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclassobj.b

If (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testclassobj.c

If (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testsubclassobj.a

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.b

If (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testsubclassobj.c

If (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testsubclassobj.x

If (val == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.y

If (val == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testsubclassobj.z

If (val == 30)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.zz

If (val == 40)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class testclassnoargs
{
	a := ""
	b := ""
	c := ""
	
	__New()
	{
		global
		a := 1
		b := 2
		c := 3
	}
}

class testsubclassfourargs extends testclassnoargs
{
	x := ""
	y := ""
	
	__New(p1, p2)
	{
		global
		super.__New()
		x := p1 * 10
		y := p2 * 10
	}
}

testclassobj := testclassnoargs()
testsubclassobj := testsubclassfourargs(1, 2)

val := testclassobj.a

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclassobj.b

If (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testclassobj.c

If (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.x

If (val == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.y

If (val == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
class testclassthreeargs
{
	a := 1
	b := 2
	c := 3
	
	__New(_a, _b, _c)
	{
		global
		a := _a
		b := _b
		c := _c
	}
}

class testsubclassnoargs extends testclassthreeargs
{
	x := ""
	y := ""
	
	__New()
	{
		global
		super.__New()
		x := 100
		y := 200
	}
}

testclassobj := testclassthreeargs(4, 5, 6)
testsubclassobj := testsubclassnoargs()

val := testclassobj.a

If (val == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclassobj.b

If (val == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testclassobj.c

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.x

If (val == 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.y

If (val == 200)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.a

If (val == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.b

If (val == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.c

If (val == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testsubclassobj := testsubclassnoargs(7, 8, 9) ; No constructor parameters defined in the subclass, so just forward them to the base.

val := testsubclassobj.a

If (val == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.b

If (val == 8)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.c

If (val == 9)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
