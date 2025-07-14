; #Include %A_ScriptDir%/header.ahk

x := 0xAA

if (x = 170)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

x := 0xBB

if (x = 187)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

x := 0xCC

if (x = 204)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

x := 0xDD

if (x = 221)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

x := 0xDd

if (x = 221)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"