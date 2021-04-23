;#Include %A_ScriptDir%/header.ahk

x := "ALL CAPS"
y := StrLower(x)

if (y = "all caps")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "AlL CaPs"
y := StrLower(x)

if (y = "all caps")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "all caps"
y := StrLower(x)

if (y = "all caps")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := ""
y := StrLower(x)

if (y = "")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "ALL CAPS"
y := StrLower(x, "T")

if (y = "ALL CAPS")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "all caps"
y := StrLower(x, "T")

if (y = "All Caps")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "All Caps"
y := StrLower(x, "T")

if (y = "All Caps")
	FileAppend, pass, *
else
	FileAppend, fail, *