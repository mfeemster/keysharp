;#Include %A_ScriptDir%/header.ahk

x = hello
y = %x% world

If x != hello
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If y != hello world
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If y = hello world
	FileAppend, pass, *
else
	FileAppend, fail, *