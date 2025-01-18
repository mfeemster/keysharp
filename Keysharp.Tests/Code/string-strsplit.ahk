

x := "a,b,c,d"
y := StrSplit(x, ",")
exp := [ "a", "b", "c", "d" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "abcd"
y := StrSplit(x)

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "	a, b,c ,d	"
y := StrSplit(x, ",", "`t ")

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "	a, b-c _d	"
y := StrSplit(x, [ ",", "-", "_" ], "`t ")

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "abcd"
y := StrSplit(x, , , 1)
exp := [ "abcd" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := StrSplit(x, , , 2)
exp := [ "a", "bcd" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := StrSplit(x, , , 3)
exp := [ "a", "b", "cd" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := StrSplit(x, , , 4)
exp := [ "a", "b", "c", "d" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

y := StrSplit(x, , , 5)
exp := [ "a", "b", "c", "d" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "a,b,c,d"
y := StrSplit(x, ",", , 3)
exp := [ "a", "b", "c,d" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "	a, b-c _d	"
y := StrSplit(x, [ ",", "-", "_" ], "`t ", 3)
exp := [ "a", "b", "c _d" ]

if (exp = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"