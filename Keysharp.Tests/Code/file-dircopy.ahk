; #Include %A_ScriptDir%/header.ahk

if (DirExist("./DirCopy2"))
	DirDelete("./DirCopy2", true)

DirCopy("../../../Keysharp.Tests/Code/DirCopy", "./DirCopy2")
	
if (DirExist("./DirCopy2"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./DirCopy2/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./DirCopy2/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./DirCopy2/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (DirExist("./DirCopy2"))
	DirDelete("./DirCopy2", true)

if (DirExist("./DirCopy2"))
 	FileAppend, "fail", "*"
else
  	FileAppend, "pass", "*"