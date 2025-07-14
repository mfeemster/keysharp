; #Include %A_ScriptDir%/header.ahk

dir := "../../../Keysharp.Tests/Code/DirCopy/file1.txt"
size := FileGetSize(dir)

if (size == 14)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

size := FileGetSize(dir, "k")

if (size == 0)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"
	
size := FileGetSize(dir, "m")

if (size == 0)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"
	
size := FileGetSize(dir, "g")

if (size == 0)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"
	
size := FileGetSize(dir, "t")

if (size == 0)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"