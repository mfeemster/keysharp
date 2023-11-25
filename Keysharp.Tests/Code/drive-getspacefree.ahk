; #Include %A_ScriptDir%/header.ahk

val := DriveGetSpaceFree("C:\")
			
if (val > 10)
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"