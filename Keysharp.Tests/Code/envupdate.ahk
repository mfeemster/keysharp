; #Include %A_ScriptDir%/header.ahk

EnvUpdate

if (ErrorLevel == 0) 
	FileAppend, pass, *
else
	FileAppend, fail, *