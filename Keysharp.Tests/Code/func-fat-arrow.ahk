class myclass
{
	x := 123
	t := true
	member1 := memberfunc(a, b) => a * b * 2
	member2 := (a, b) => a * b * 2
	member3 := a => a * 2
	member4 := a* => (a[1] + a[2]) * 2
	member5 := () => 123
	member6 := (*) => (args[1] + args[2]) * 2
	member7 := (a) => a * this.x
	member8 := (a) => this.x := 100, a * this.x

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

x := myclassobj.member6(3, 4)

If (x == 14)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
myclassobj.x := 123
x := myclassobj.member7(2)

If (x == 246)
	FileAppend, pass, *
else
	FileAppend, fail, *

myclassobj.x := 123
x := myclassobj.member8(2)

If (x == 200)
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

myfunc7 := (*) =>  args[1] * args[2] * 2
x := myfunc7(1, 2)

If (x == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 0
myfunc8 := () => x := 123
y := myfunc8()

If (x == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

myfunc9 := () => a := 123, b := 456, c := 789
x := myfunc9()

If (x == 789)
	FileAppend, pass, *
else
	FileAppend, fail, *

m := { two : (this) => 2 }
x := m.two()

If (x == 2)
	FileAppend, pass, *
else
	FileAppend, fail, *

m := { two : (this, a) => a * 2 }
x := m.two(5)

If (x == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

MultFunc(a, b)
{
	return a * b
}

m := { two : (this) => MultFunc(3, 4) }
x := m.two()

If (x == 12)
	FileAppend, pass, *
else
	FileAppend, fail, *

m := { two : (this, a) => a * MultFunc(3, 4) }
x := m.two(2)

If (x == 24)
	FileAppend, pass, *
else
	FileAppend, fail, *

x :=
m := { one : 1, two : (this, a) => a * MultFunc(3, 4) }
x := m.two(2)

If (x == 24)
	FileAppend, pass, *
else
	FileAppend, fail, *
		
m := {
one : 1,
two : (this, a) => a * MultFunc(3, 4),
three : (this, a) => a * 2
}

x := m.two(2) * m.three(3)

If (x == 144)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr := [
() => 1
, () => 2
, () => 3
]

x := arr[1]() * arr[2]() * arr[3]()

If (x == 6)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
b :=

AssignFunc(xx)
{
	global b := xx
}

AssignFunc(() => 1)
x := b()

If (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

m := { one : { oneone : 11, onef : (this, a) => a * 2 }, two : { twotwo : 22 }, three : { threethree : 33, threethreearr : [10, 20, 30 ] } }

x := m.one.onef(5)

If (x == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := m.one.onef(5) * m.two.twotwo

If (x == 220)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := m.one.onef(5) * m.two.twotwo * m.three.threethree

If (x == 7260)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := m.one.onef(5) * m.two.twotwo * m.three.threethree * m.three.threethreearr[3]

If (x == 217800)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := 5
m := { one : (this, &a) => a := a * 2 }
x := m.one(&val)

If (x == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (val == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *