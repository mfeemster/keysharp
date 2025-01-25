x := 0
y := 0
z := 0

func_bound(a, b, c)
{
	global x := a
	global y := b
	global z := c
}

fo := Func("func_bound")

If (fo.Name == "func_bound")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (fo.IsBuiltIn == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo.Call(1, 2, 3)

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

fo(1, ,3)

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

bf := fo.Bind(5, 6, 7)

bf()

If (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

bf := fo.Bind(5)

bf()

If (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

bf := fo.Bind(5, ,7)

bf.Call()

If (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

bf := fo.Bind(,123)

bf(1)

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

bf := fo.Bind(,,123)

bf(1,2)

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := Func(boundvarfunc0) ; Try without quotes.
bf := fo.Bind(10)
val := bf(20)

If (val == 30)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [1, 2, 3]
bf := fo.Bind(arr*)

val := bf()

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := BoundVarFunc0 ; Try referring to an improperly cased local function by name, without using Func().
bf := fo.Bind(10)
val := bf(20)

If (val == 30)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [1, 2, 3]
bf := fo.Bind(arr*)

val := bf()

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
boundvarfunc0(theparams*) ; Purposely define this *after* it's used above.
{
	temp := 0

	for n in theparams
	{
		temp += theparams[A_Index]
	}

	return temp
}

fo := String ; Try referring to a built-in function by name, without using Func().
val := fo(123)

If (val == "123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

boundvarfunc1(p1, theparams*)
{
	temp := p1

	for n in theparams
	{
		temp += theparams[A_Index]
	}

	return temp
}

fo := Func("boundvarfunc1")
bf := fo.Bind(10, 20)

val := bf(20)

If (val == 50)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := funcretcall(Func123)

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

funcretcall(xx)
{
	return xx()
}

func123()
{
	return 123
}

newfunc := true ? func123 : func456
val := newfunc()

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

newfunc := false ? func123 : Func456
val := newfunc()

If (val == 456)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

func456()
{
	return 456
}

arr1 := Array()
arr2 := [10, 20, 30]
funcadd := Func("Add", arr1)

funcadd(10)
funcadd(20)
funcadd(30)

if (arr1 == arr2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

arr1 := Array()
funcadd := Func("Push", arr1)

funcadd(10, 20, 30)

if (arr1 == arr2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

o := unset
pcount := ""
varfunc5(p1, pvar*)
{
	pcount := pvar.Length
}

func5 := Func("varfunc5")
boundfunc5 := func5.Bind(o)

pcount := 123
boundfunc5()

if (pcount == 0)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

pcount := 0
boundfunc5(1)

if (pcount == 1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

pcount := 0
boundfunc5(1, 2)

if (pcount == 2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

pcount := 0
boundfunc5(1, 2, 3)

if (pcount == 3)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

pcount := 0
arr := [1, 2, 3]
boundfunc5(arr*)

if (pcount == 3)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"