class myclass
{
	x := 123
	t := true
	member1 := memberfunc(a, b) => a * b * 2
	member2 := (a, b) => a * b * 2
	member3 := a => a * 2
	member4 := a* => (a[1] + a[2]) * 2
	member5 := () => 123

	myprop
	{
		get => t
		? 1
		: 0

		set => global x := 10 * value
	}

	myclassfunc(a, b) => 10 * x * a * b
	
	callmember1(a, b)
	{
		return member1(a, b)
	}
}

myclassobj := myclass()
x := myclassobj.myprop

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

myclassobj.myprop := 10

If (myclassobj.x == 100)
	FileAppend, pass, *
else
	FileAppend, fail, *

myclassobj.x := 10
x := myclassobj.myclassfunc(10, 20)

If (x == 20000)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := myclassobj.callmember1(4, 5)

If (x == 40)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := myclassobj.member2(1, 2)

If (x == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := myclassobj.member3(3)

If (x == 6)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := myclassobj.member4(3, 4)

If (x == 14)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := myclassobj.member5()

If (x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

myfunc() => 123
x := myfunc()

If (x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

Sum(a, b) => a + b

x := Sum(1, 2)

If (x == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

Sum2(a, b, c*) => a + b + c[1] + c[2]

x := Sum2(1, 2, 3, 4)

If (x == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

double1 := doublefunc(a, b) => a * b * 2

x := double1(1, 2)

If (x == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

double2 := doublefunc2(a, b, c*) => a * b * c[1] * 2

x := double2(1, 2, 3)

If (x == 12)
	FileAppend, pass, *
else
	FileAppend, fail, *
		
myfunc2 := () => 123
x := myfunc2()

If (x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
myfunc3 := (a*) => a[1] * a[2] * 2
x := myfunc3(1, 2)

If (x == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

myfunc4 := a* => a[1] * a[2] * 2

x := myfunc4(3, 4)

If (x == 24)
	FileAppend, pass, *
else
	FileAppend, fail, *

myfunc5 := (a) => a * 2
x := myfunc5(3)

If (x == 6)
	FileAppend, pass, *
else
	FileAppend, fail, *

myfunc6 := a => a * 2

x := myfunc6(4)

If (x == 8)
	FileAppend, pass, *
else
	FileAppend, fail, *