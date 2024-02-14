

x := "ALL CAPS"
y := StrUpper(x)

if (y = "ALL CAPS")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "AlL CaPs"
y := StrUpper(x)

if (y = "ALL CAPS")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "all caps"
y := StrUpper(x)

if (y = "ALL CAPS")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := ""
y := StrUpper(x)

if (y = "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "ALL CAPS"
y := StrTitle(x)

if (y = "ALL CAPS")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "all caps"
y := StrTitle(x)

if (y = "All Caps")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "All Caps"
y := StrTitle(x)

if (y = "All Caps")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"