x := 1
y := 2
z := 3

varfunc1(*)
{
	global x := true
}

x := false
varfunc1()

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfunc1("firstparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfunc1("firstparam", "secondparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfunc1("firstparam", ,"thirdparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfo1 := Func("varfunc1")

x := false
varfo1()

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfo1("firstparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfo1("firstparam", "secondparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false
varfo1("firstparam", ,"thirdparam")

If (x == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfuncimplicit(*)
{
	temp := 0

	for n in args
	{
		temp += args[A_Index]
	}

	return temp
}

arr := [1, 2, 3]
val := varfuncimplicit(arr*)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := varfuncimplicit()

If (val == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := Func("varfuncimplicit")
val := fo(arr*)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := Func("varfuncimplicit")
val := fo()

If (val == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc2(p1, theparams*)
{
	temp := p1

	for n in theparams
	{
		temp += theparams[A_Index]
	}

	return temp
}

val := varfunc2(1, 2, 3)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc3(p1, theparams*)
{
	temp := p1

	for n in theparams
	{
		temp += n
	}

	return temp
}

val := varfunc3(1, 2, 3)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc4(*)
{
	return args[1] + args[2] + args[3]
}

val := varfunc3(1, 2, 3)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr := [1, 2, 3]
val := varfunc3(1, arr*)

If (val == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := varfunc4(arr*)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc5(p1, p2, theparams*)
{
	temp := p1 + p2

	for n in theparams
	{
		temp += n
	}

	return temp
}

val := varfunc5(1, 2, arr*)

If (val == 9)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := Func("varfunc3")
val := fo(1, arr*)

If (val == 7)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := Func("varfunc4")
val := fo(arr*)

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo := Func("varfunc5")
val := fo(1, 2, arr*)

If (val == 9)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

varfunc6(args*)
{
	local temp := 0

	for n in args
	{
		temp += n
	}

	return temp
}

arr := [1, 2, 3]
; Test dynamic call passing two non variadic args plus a variadic arg passed to a func that takes one variadic param.
val := Func("varfunc6").Call(1, 2, arr*)

If (val == 9)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; This tests the proper casting of a variadic argument to object, so that it can be properly passed to a non variadic function.
first(args*)
{
	second(args)
	Func(second).Call(args) ; This should not create a local variable named second, because it's not a ref or assign.
}

second(args)
{
	If (args[1] == "hello")
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

first("hello")

; Test constructing a map with the last parameter being an arry with the spread operator.
arr := ["one", 1, "two", 2]
m := Map("three", 3, arr*)

if (m["one"] == 1 &&
	m["two"] == 2 &&
	m["three"] == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"