;#Include %A_ScriptDir%/header.ahk

monget := MonitorGet()

; Need to eventually make this work with ownprops like monget.Left

if (monget["Left"] >= 0 && monget["Right"] >= 0 && monget["Top"] >= 0 && monget["Bottom"] >= 0 && monget["N"] > 0)
	FileAppend, pass, *
else
  	FileAppend, fail, *