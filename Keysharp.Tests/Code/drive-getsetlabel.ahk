; #Include %A_ScriptDir%/header.ahk

val := DriveGetStatus("C:\")

origlabel := DriveGetLabel("C:\")
DriveSetLabel("C:\", "a test label")
newlabel := DriveGetLabel("C:\")
			
if (newlabel == "a test label")
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

DriveSetLabel("C:\", origlabel)
newlabel := DriveGetLabel("C:\")

if (origlabel = newlabel)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"