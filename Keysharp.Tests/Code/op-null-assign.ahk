x := 123
y := x ?? ""

if (y = 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

nafunc(p?)
{
	if ((p ?? 456) == 456)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}

nafunc(unset)

z :=
y := z ?? 456

if (y = 456)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

tot := 0

nafunc2(a, b, c)
{
	global tot
	tot += a(1)
	tot += b()
	tot += c(3)
}

nafunc2((o) => o ?? 11, (o?) => o ?? 22, (o?) => o ?? 33)

If (tot == 26)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 123
yy := unset
m := { one : x ?? 456,  two : yy ?? 789}

if (m.one = 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (m.two = 789)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 123

if ((x ?? 456) == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := unset
x ??= Array()

if (x is Array)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"