

x := "abcdefghijkl"
y := SubStr(x)

if (x = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := SubStr(x, 1, 1)

if ("a" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := SubStr(x, 1, 5)

if ("abcde" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := SubStr(x, 1, 11)

if ("abcdefghijk" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := SubStr(x, 1, -1)

if ("abcdefghijk" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := SubStr(x, 1, -11)

if ("a" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 1, -12)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 1, -13)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 4, -3)

if ("defghi" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 6, -6)

if ("f" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 7, -6)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 7, -7)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 0)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, StrLen(x) + 1)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 2, 1)

if ("b" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 2, 1)

if ("b" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 4, 3)

if ("def" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 10, 3)

if ("jkl" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, 12, 1)

if ("l" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -1)

if ("l" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5)

if ("hijkl" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -12)

if (x = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := SubStr(x, -13)

if (x = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5, 5)

if ("hijkl" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5, 3)

if ("hij" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5, -3)

if ("hi" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5, -5)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5, -6)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := SubStr(x, -5, -13)

if ("" = y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"