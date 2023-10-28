;#Include %A_ScriptDir%/header.ahk

if (Round(3.14) == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
		
if (Round(3.14, 1) == 3.1)
	FileAppend, pass, *
else
	FileAppend, fail, *
		
if (Round(345, -1) == 350)
	FileAppend, pass, *
else
	FileAppend, fail, *
		
if (Round(345, -2) == 300)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Round(-345, -1) == -350)
	FileAppend, pass, *
else
	FileAppend, fail, *
		
if (Round(-345, -2) == -300)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (Round(-0, -2) == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *