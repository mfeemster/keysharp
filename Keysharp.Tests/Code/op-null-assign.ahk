x := 123
y := x ?? ""

if (y = 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

func(p)
{
	if ((p ?? 456) == 456)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

func()

z :=
y := z ?? 456

if (y = 456)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

tot := 0

func2(a, b, c)
{
	global tot
	tot += a(1)
	tot += b()
	tot += c(3)
}

func2((o) => o ?? 11, (o) => o ?? 22, (o) => o ?? 33)

If (tot == 26)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 123
yy :=
m := { one : x ?? 456,  two : yy ?? 789}

if (m.one = 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (m.two = 789)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"