; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileRecycleEmpty"))
	DirDelete("./FileRecycleEmpty", true)

dir := "../../../Keysharp.Tests/Code/DirCopy"
DirCreate("./FileRecycleEmpty")
FileCopy(dir . "/*", "./FileRecycleEmpty/")

if (FileExist("./FileRecycleEmpty/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileRecycleEmpty/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileRecycleEmpty/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileRecycle("./FileRecycleEmpty/*")

if (!FileExist("./FileRecycleEmpty/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (!FileExist("./FileRecycleEmpty/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (!FileExist("./FileRecycleEmpty/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileRecycleEmpty()

if (DirExist("./FileRecycleEmpty"))
	DirDelete("./FileRecycleEmpty", true)

DirCreate("./FileRecycleEmpty")
FileCopy(dir . "/*", "./FileRecycleEmpty/")

if (FileExist("./FileRecycleEmpty/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileRecycleEmpty/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileRecycleEmpty/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileRecycle("./FileRecycleEmpty/*")

if (!FileExist("./FileRecycleEmpty/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (!FileExist("./FileRecycleEmpty/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (!FileExist("./FileRecycleEmpty/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileRecycleEmpty("C:\")

if (DirExist("./FileRecycleEmpty"))
	DirDelete("./FileRecycleEmpty", true)