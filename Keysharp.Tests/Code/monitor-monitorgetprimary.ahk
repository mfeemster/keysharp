;#Include %A_ScriptDir%/header.ahk

monget := MonitorGetPrimary()

if (monget >= 0)
	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"