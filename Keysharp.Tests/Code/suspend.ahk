; #Include %A_ScriptDir%/header.ahk

Suspend, On

if (A_IsSuspended == true) 
	FileAppend, pass, *
else
	FileAppend, fail, *