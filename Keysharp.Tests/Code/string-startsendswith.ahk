;#Include %A_ScriptDir%/header.ahk

s := "This is a test STRING"

if (EndsWith(s, " STRING", true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (!EndsWith(s, " string", true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (EndsWith(s, " string", false))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (StartsWith(s, "This ", true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (!StartsWith(s, "this ", true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (StartsWith(s, "tHiS ", false))
	FileAppend, pass, *
else
	FileAppend, fail, *
	