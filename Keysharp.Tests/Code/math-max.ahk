;#Include %A_ScriptDir%/header.ahk
			
if (Max(-6, -6) == -6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Max(-6, -5) == -5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(-4.2, -5.0) == -4.2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(0, 0) == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(0, 1) == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(1, 1) == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(1.5, 2.3) == 2.3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(-1.0, "asdf") == "")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := [ -1.0, -0.5, 0, 0.5, 1, 0.675 ]

if (Max(x) == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := [ -1.0, -0.5, 0, 0.5, 1, 0.675, 2.0 ]

if (Max(x) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(-1.0, -0.5, 0, 0.5, 1, 0.675) == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (Max(-1.0, -0.5, 0, 0.5, 1, 0.675, 2.0) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
