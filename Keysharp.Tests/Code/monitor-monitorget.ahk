;#Include %A_ScriptDir%/header.ahk

monget := MonitorGet()

if (monget.Left >= 0 && monget.Right >= 0 && monget.Top >= 0 && monget.Bottom >= 0 && monget.N > 0)
	FileAppend, pass, *
else
  	FileAppend, fail, *