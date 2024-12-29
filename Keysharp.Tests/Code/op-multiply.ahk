

x := 10
y := x * 100

if (y = 1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -10
y := x * 100

if (y = -1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10
y := -x * 100

if (y = -1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -10
y := x * -100

if (y = 1000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := 10
y := x * 1.5

if (y = 15)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10.5
y := x * 1.5

if (y = 15.75)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0
y := x * 10

if (y = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0
y := x * 10.5

if (y = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0
y := x * 0

if (y = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	