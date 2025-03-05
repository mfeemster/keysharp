x := 1
y := 1

If (x < y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (1 < 1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (1 > 1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!("1" < 1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!("0x1" > "1"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1
y := 2

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -1
y := -1

If (x < y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (-1 < -1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (-1 > -1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!("-1" < -1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!("-0x1" > "-1"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -1
y := 2

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := -2
y := -1

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1.234
y := 1.234

If (x < y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (1.234 < 1.234)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (1.234 > 1.234)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!("1.234" < 1.234))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!("1.234" > "1.234"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1.234
y := 2.456

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -1.234
y := -1.234

If (x < y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (-1.234 < -1.234)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (-1.234 > -1.234)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!("-1.234" < -1.234))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!("-1.234" > "-1.234"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -1.234
y := 2.456

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -2.234
y := -1.456

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0

If (x < y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (0 < 0)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (0 > 0)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!("0" < 0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!("0" > "0"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0
y := 1

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "a"
y := "a"

If (x < y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "a"
y := "b"

If (x < y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x > y)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x < y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x > y))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"