;#Include %A_ScriptDir%/header.ahk

ct := MonitorGetCount()
names := ""

loop ct
	names .= MonitorGetName(A_Index)
	
if (names != "")
	FileAppend, pass, *
else
	FileAppend, fail, *