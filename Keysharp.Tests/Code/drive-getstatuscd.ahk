; #Include %A_ScriptDir%/header.ahk

val := DriveGetStatusCD("C:\\")
			
if (val == "error")
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"