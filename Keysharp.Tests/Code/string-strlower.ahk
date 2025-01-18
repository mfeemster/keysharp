

x := "ALL CAPS"
y := StrLower(x)

if (y = "all caps")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "AlL CaPs"
y := StrLower(x)

if (y = "all caps")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "all caps"
y := StrLower(x)

if (y = "all caps")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := ""
y := StrLower(x)

if (y = "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "ALL CAPS"
y := StrTitle(x)

if (y = "ALL CAPS")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "all caps"
y := StrTitle(x)

if (y = "All Caps")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "All Caps"
y := StrTitle(x)

if (y = "All Caps")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"