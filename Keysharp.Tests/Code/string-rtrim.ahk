;#Include %A_ScriptDir%/header.ahk

x := " test`t"
y := RTrim(x)

if (y = " test")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "test"
y := RTrim(x)

if (y = "test")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "`ttest "
y := RTrim(x)

if (y = "`ttest")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
x := "`ttest`t "
y := RTrim(x)

if (y = "`ttest")
	FileAppend, pass, *
else
	FileAppend, fail, *