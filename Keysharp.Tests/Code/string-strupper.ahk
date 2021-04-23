;#Include %A_ScriptDir%/header.ahk

x := "ALL CAPS"
y := StrUpper(x)

if (y = "ALL CAPS")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "AlL CaPs"
y := StrUpper(x)

if (y = "ALL CAPS")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "all caps"
y := StrUpper(x)

if (y = "ALL CAPS")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := ""
y := StrUpper(x)

if (y = "")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "ALL CAPS"
y := StrUpper(x, "T")

if (y = "ALL CAPS")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "all caps"
y := StrUpper(x, "T")

if (y = "All Caps")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "All Caps"
y := StrUpper(x, "T")

if (y = "All Caps")
	FileAppend, pass, *
else
	FileAppend, fail, *