;#Include %A_ScriptDir%/header.ahk

if (-1.5707963267948966 == ASin(-1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-0.5235987755982989 == ASin(-0.5))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == ASin(0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0.5235987755982989 == ASin(0.5))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (1.5707963267948966 == ASin(1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0.74096470220302 == ASin(0.675))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"