;#Include %A_ScriptDir%/header.ahk

s := Format("{1}", 123)

if (s == "123")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
s := Format("{1}", 123.456)

if (s == "123.456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"