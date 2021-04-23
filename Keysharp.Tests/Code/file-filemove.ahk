; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileMove"))
	DirDelete("./FileMove", true)

if (DirExist("./FileMove2"))
	DirDelete("./FileMove2", true)

DirCreate("./FileMove")
path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy"
FileCopy(dir . "/*", "./FileMove/")
	
if (DirExist("./FileMove"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileMove/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileMove/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileMove/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

DirCreate("./FileMove2")

if (DirExist("./FileMove2"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileMove("./FileMove/*", "./FileMove2")

if (!FileExist("./FileMove/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileMove/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileMove/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileMove2/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileMove2/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileMove2/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileMove"))
	DirDelete("./FileMove", true)

if (DirExist("./FileMove2"))
	DirDelete("./FileMove2", true)