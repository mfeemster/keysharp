; #Include %A_ScriptDir%/header.ahk

val := DriveGetCapacity("C:\")
			
if (val > 1000)
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"