; #Include %A_ScriptDir%/header.ahk

x := EnvGet("PATH")

if (x != "") 
	FileAppend, pass, *
else
	FileAppend, fail, *

dummy := "dummynothing123"
x := EnvGet(dummy)

if (x == "") 
	FileAppend, pass, *
else
	FileAppend, fail, *