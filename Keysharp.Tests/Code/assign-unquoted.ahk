;#Include %A_ScriptDir%/header.ahk
x = hello

If x != hello
	FileAppend, fail, *
else
	FileAppend, pass, *
	
if x = hello
	FileAppend, pass, *
else
	FileAppend, fail, *