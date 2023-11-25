;#Include %A_ScriptDir%/header.ahk

if (-0.6931471805599453 == Ln(0.5))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (0 == Ln(1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-0.3930425881096072 == Ln(0.675))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
