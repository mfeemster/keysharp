

x := "test"
y := StrLen(x)

if (y = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := ""
y := StrLen(x)

if (y = 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"