; #Include %A_ScriptDir%/header.ahk

val := DriveGetType("C:\")
			
if (val == "Fixed")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"