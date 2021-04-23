;#Include %A_ScriptDir%/header.ahk

monget := MonitorGetWorkArea()

if (monget.Left >= 0 && monget.Right >= 0 && monget.Top >= 0 && monget.Bottom >= 0 && monget.N > 0)
	FileAppend, pass, *
else
  	FileAppend, fail, *