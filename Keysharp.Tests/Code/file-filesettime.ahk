; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileSetTime"))
	DirDelete("./FileSetTime", true)

dir := "../../../Keysharp.Tests/Code/DirCopy"
DirCopy(dir, "./FileSetTime")

if (DirExist("./FileSetTime"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileSetTime/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileSetTime/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./FileSetTime/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileSetTime("20200101131415", "./FileSetTime/file1.txt", "m")
filetime := FileGetTime("./FileSetTime/file1.txt", "m")

if ("20200101131415" == filetime)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileSetTime("20200101131416", "./FileSetTime/file1.txt", "c")
filetime := FileGetTime("./FileSetTime/file1.txt", "c")

if ("20200101131416" == filetime)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

FileSetTime("20200101131417", "./FileSetTime/file1.txt", "a")
filetime := FileGetTime("./FileSetTime/file1.txt", "a")

if ("20200101131417" == filetime)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileSetTime"))
	DirDelete("./FileSetTime", true)
