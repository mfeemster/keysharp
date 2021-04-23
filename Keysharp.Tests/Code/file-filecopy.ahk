; #Include %A_ScriptDir%/header.ahk
			
if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

path := "../../../Keysharp.Tests/Code/"
DirCreate("./FileCopy")
dir := path . "DirCopy"
FileCopy(dir . "/file1.txt", "./FileCopy/file1.txt")

if (FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *
	
FileDelete("./FileCopy/file1.txt")

if (!FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)
	
DirCreate("./FileCopy")
FileCopy(dir . "/*.txt", "./FileCopy")

if (FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

DirCreate("./FileCopy")
FileCopy(dir . "/*.txt", "./FileCopy/*.*")

if (FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

DirCreate("./FileCopy")
FileCopy(dir . "/*.txt", "./FileCopy/*.bak")

if (FileExist("./FileCopy/file1.bak"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file2.bak"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/file3.bak"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

DirCreate("./FileCopy")
FileCopy(dir . "/*.txt", "./FileCopy/*")

if (FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *


if (!FileExist("./FileCopy/file3.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

DirCreate("./FileCopy")
FileCopy(dir . "/*.txt", "./FileCopy/*.")

if (FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/file3.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

DirCreate("./FileCopy")
FileCopy(dir . "/*.txt", "./NonExistentDir/*")

if (!FileExist("./FileCopy/NonExistentDir/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/NonExistentDir/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/NonExistentDir/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (!FileExist("./FileCopy/NonExistentDir/file3.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)

DirCreate("./FileCopy")
FileCopy(dir . "/*", "./FileCopy/*")

if (FileExist("./FileCopy/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileCopy/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileCopy"))
	DirDelete("./FileCopy", true)