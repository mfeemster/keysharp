;#Include %A_ScriptDir%/header.ahk

x := " test`t"
y := LTrim(x)

if (y = "test`t")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "test"
y := LTrim(x)

if (y = "test")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "`ttest "
y := LTrim(x)

if (y = "test ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "`ttest`t "
y := LTrim(x)

if (y = "test`t ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"