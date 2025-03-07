x := 10
y := 20
z := 30

x++, y++, z++

if (x = 11)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (y = 21)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (z = 31)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
