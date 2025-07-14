; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileRecycle"))
	DirDelete("./FileRecycle", true)

DirCreate("./FileRecycle")
dir := "../../../Keysharp.Tests/Code/DirCopy"

FileCopy(dir . "/*", "./FileRecycle/")

if (FileExist("./FileRecycle/file1.txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (FileExist("./FileRecycle/file2.txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (FileExist("./FileRecycle/file3txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

FileRecycle("./FileRecycle/file1.txt")

if (!FileExist("./FileRecycle/file1.txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (FileExist("./FileRecycle/file2.txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (FileExist("./FileRecycle/file3txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

FileRecycle("./FileRecycle/*.txt")

if (!FileExist("./FileRecycle/file2.txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (FileExist("./FileRecycle/file3txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

FileRecycle("./FileRecycle/*")

if (!FileExist("./FileRecycle/file3txt"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (DirExist("./FileRecycle"))
	DirDelete("./FileRecycle", true)