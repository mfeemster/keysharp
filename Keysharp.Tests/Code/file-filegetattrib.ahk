; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/DirCopy"
attr := FileGetAttrib(path)

if (attr == "D")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

attr := FileGetAttrib(path . "/file1.txt")

if (attr == "A")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

attr := FileGetAttrib(path . "/file2.txt")

if (attr == "A")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

attr := FileGetAttrib(path . "/file3txt")

if (attr == "A")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"