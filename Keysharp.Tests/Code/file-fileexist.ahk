; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy/*.txt"

attr := FileExist(dir)

if ("A" == attr)
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"