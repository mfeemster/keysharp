

x := 1
y:=2
z := x + y

If x != 1
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If y!=2
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
if z!=	3
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If x = 1
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If y = 2
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If z = 3
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x != 1)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If (y!=2)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
if (z!=	3)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If (x = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z = 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1 + 2

If (x != 3)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := -1 + -2

If (x != -3)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"