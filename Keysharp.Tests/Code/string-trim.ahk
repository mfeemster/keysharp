;#Include %A_ScriptDir%/header.ahk

x := " test`t"
y := Trim(x)

if (y = "test")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "test"
y := Trim(x)

if (y = "test")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "`ttest "
y := Trim(x)

if (y = "test")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "`ttest`t "
y := Trim(x)

if (y = "test")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"