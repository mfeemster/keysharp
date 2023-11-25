; #Include %A_ScriptDir%/header.ahk
		
if (DirExist("./FileDelete"))
	DirDelete("./FileDelete", true)

dir := "../../../Keysharp.Tests/Code/DirCopy"

DirCopy(dir, "./FileDelete")
FileDelete("./FileDelete/*.txt")

if (DirExist("./FileDelete/"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"
	
if (!FileExist("./FileDelete/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (!FileExist("./FileDelete/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileDelete/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileDelete("./FileDelete/*")

if (!FileExist("./FileDelete/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"
	
if (DirExist("./FileDelete"))
	DirDelete("./FileDelete", true)