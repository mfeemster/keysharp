; #Include %A_ScriptDir%/header.ahk

dir := "../../../Keysharp.Tests/Code/DirCopy/file1.txt"
time := FileGetTime(dir)

if (StrLen(time) == 14)
 	FileAppend, pass, *
else
  	FileAppend, fail, *
