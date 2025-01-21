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
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testclassobj.b

If (val == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testclassobj.c

If (val == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testsubclassobj.a

If (val == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.b

If (val == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testsubclassobj.c

If (val == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testsubclassobj.x

If (val == 10)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.y

If (val == 20)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testsubclassobj.z

If (val == 30)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.zz

If (val == 40)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

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
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testclassobj.b

If (val == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testclassobj.c

If (val == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.x

If (val == 10)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.y

If (val == 20)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

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
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testclassobj.b

If (val == 5)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := testclassobj.c

If (val == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.x

If (val == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.y

If (val == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.a

If (val == unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.b

If (val == unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.c

If (val == unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

testsubclassobj := testsubclassnoargs(7, 8, 9) ; No constructor parameters defined in the subclass, so just forward them to the base.

val := testsubclassobj.a

If (val == 7)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.b

If (val == 8)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testsubclassobj.c

If (val == 9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class class1
{
	sum := 0

	__New(args*)
	{
		global sum
		local temp := 0

		for n in args
		{
			temp += n
		}

		sum := temp
	}
}

arr := [1, 2, 3]
c1 := class1()

if (c1.sum == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
c1 := class1(arr*)

if (c1.sum == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c1 := class1(1, arr*)
		
if (c1.sum == 7)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
c1 := class1(1, 2, arr*)

if (c1.sum == 9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c1 := ""

class class2
{
	sum := 0

	__New(*)
	{
		global sum
		local temp := 0

		for n in args
		{
			temp += n
		}

		sum := temp
	}
}

c2 := class2()

if (c2.sum == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c2 := class2(1, 2, 3)

if (c2.sum == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
c2 := class2(arr*)

if (c2.sum == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
c2 := class2(1, 2, arr*)

if (c2.sum == 9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c2 := ""

class class3
{
	sum := 0

	__New(theparams*)
	{
		global sum
		local temp := 0

		for n in theparams
		{
			temp += n
		}

		sum := temp
	}
}

c3 := class3(1, 2, 3)

if (c3.sum == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c3 := class3(arr*)

if (c3.sum == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c3 := class3(1, 2, arr*)

if (c3.sum == 9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c3 := ""

class class4
{
	sum := 0

	__New(p1, p2, theparams*)
	{
		global sum
		local temp := p1 + p2

		if (theparams != unset)
		{
			for n in theparams
			{
				temp += n
			}
		}

		sum := temp
	}
}

c4 := class4(1, 2)

if (c4.sum == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

c4 := class4(1, 2, arr*)

if (c4.sum == 9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class class5
{
	func(a := "`r", b := "`n", c := "`t")
	{
		return a . b . c
	}
}

c5 := class5()
val := c5.func()

if (val == "`r`n`t")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"