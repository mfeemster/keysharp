; #Include %A_ScriptDir%/header.ahk

val := DriveGetSerial("C:\")
			
if (val > 1)
 	FileAppend, pass, *
else
  	FileAppend, fail, *