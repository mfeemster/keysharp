

x := "a,b,c,d,e,f"
varct := ""
z := "abcdef"
y := StrReplace(x, ",")


if (y = z)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, ",", "")

if (y = "abcdef")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, ",", ".")

if (y = "a.b.c.d.e.f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, ",", ".", "On")

if (y = "a.b.c.d.e.f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, ",", ".", null, &varct)

if (y = "a.b.c.d.e.f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (varct = 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := StrReplace(x, ",", ".", , &varct, 3)

if (y = "a.b.c.d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (varct = 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := StrReplace(x, "")

if (y = "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "a", "A", 1)

if (y = "A,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "a", "A", "On")

if (y = "A,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "a", "A", true)

if (y = "A,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "A", "1", 0)

if (y = "1,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "A", "1", "Off")

if (y = "1,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "A", "1", false)

if (y = "1,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := StrReplace(x, "a", "A", "On", &varct, 9)
		
if (y = "A,b,c,d,e,f")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (varct = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"