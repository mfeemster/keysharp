;#Include %A_ScriptDir%/header.ahk

x := "a,b,c,d,e,f"
y := StrReplace(x, ",")
z := "abcdef"
varct := ""

if (y = z)
	FileAppend, pass, *
else
	FileAppend, fail, *

y := StrReplace(x, ",", "")

if (y = "abcdef")
	FileAppend, pass, *
else
	FileAppend, fail, *

y := StrReplace(x, ",", ".")

if (y = "a.b.c.d.e.f")
	FileAppend, pass, *
else
	FileAppend, fail, *

y := StrReplace(x, ",", ".", "On")

if (y = "a.b.c.d.e.f")
	FileAppend, pass, *
else
	FileAppend, fail, *

y := StrReplace(x, ",", ".", null)

if (y = "a.b.c.d.e.f")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (y.Count = 5)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
y := StrReplace(x, ",", ".", null, 3)

if (y = "a.b.c.d,e,f")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (y.Count = 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
y := StrReplace(x, "")

if (y = "")
	FileAppend, pass, *
else
	FileAppend, fail, *

y := StrReplace(x, "a", "A", "On")

if (y = "A,b,c,d,e,f")
	FileAppend, pass, *
else
	FileAppend, fail, *

y := StrReplace(x, "a", "A", "On", 9)
		
if (y = "A,b,c,d,e,f")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (y.Count = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *