; #Include %A_ScriptDir%/header.ahk

val := DriveGetList()
			
if (SubStr(val, 1, 1) == "C")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"