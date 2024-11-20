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

varfo1 := FuncObj("varfunc1")

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