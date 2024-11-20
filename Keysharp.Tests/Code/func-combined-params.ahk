optreffunc(a?, &b)
{
	b := a
}

val1 := ""
val2 := ""
optreffunc(,&val2)

if (val2 == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val1 := ""
val2 := ""
fo := FuncObj("optreffunc")
fo(,&val2)

if (val2 == unset)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val1 := 123
val2 := ""
optreffunc(val1,&val2)

if (val2 == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val1 := 123
val2 := ""
fo(val1,&val2)

if (val2 == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val1 := ""
val2 := ""
optreffunc(,)

if (val2 == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val1 := ""
val2 := ""
fo(,)

if (val2 == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3

optrefvarfunc(a?, &b, c*)
{
	temp := a

    if (c != unset)
	{
		for n in c
		{
			temp += c[A_Index]
		}
	}

	b := temp
	return temp
}

val := optrefvarfunc(x, &y, z, z, z)

If (y == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (val == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3

fo := FuncObj("optrefvarfunc")
fo(x, &y, z, z, z)

If (y == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (val == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3

val := optrefvarfunc(x, &y,)

If (y == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2
z := 3

val := fo(x, &y,)

If (y == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"