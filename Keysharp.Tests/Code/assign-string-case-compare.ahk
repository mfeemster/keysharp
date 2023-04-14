;#Include %A_ScriptDir%/header.ahk

x := "HeLlO WoRlD"

If (x == "HeLlO WoRlD")
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x == "hello world")
	FileAppend, fail, *
else
	FileAppend, pass, *

If (x = "HeLlO WoRlD")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (x = "hello world")
	FileAppend, pass, *
else
	FileAppend, fail, *


If (!(x == "HeLlO WoRlD"))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!(x == "hello world"))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x = "HeLlO WoRlD"))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If (!(x = "hello world"))
	FileAppend, fail, *
else
	FileAppend, pass, *