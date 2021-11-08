; #Include %A_ScriptDir%/header.ahk

EnvUpdate

if (A_ErrorLevel == 0) 
	FileAppend, pass, *
else
	FileAppend, fail, *