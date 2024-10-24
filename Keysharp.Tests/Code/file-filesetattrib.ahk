; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileSetAttrib"))
	DirDelete("./FileSetAttrib", true)

dir := "../../../Keysharp.Tests/Code/DirCopy"
DirCreate("./FileSetAttrib")
DirCopy(dir, "./FileSetAttrib", true)

if (DirExist("./FileSetAttrib"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileSetAttrib/file1.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileSetAttrib/file2.txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (FileExist("./FileSetAttrib/file3txt"))
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

dir := "./FileSetAttrib"
attr := FileGetAttrib(dir)

if (attr == "D")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

dir := "./FileSetAttrib/file1.txt"
attr := FileGetAttrib(dir)

#if WINDOWS
	if (attr == "A")
#else
	if (attr == "N")
#endif
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileSetAttrib("r", dir)
attr := FileGetAttrib(dir)

if (attr == "R")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileSetAttrib("-r", dir)
attr := FileGetAttrib(dir)

if (attr == "N")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileSetAttrib("^r", dir)
attr := FileGetAttrib(dir)

if (attr == "R")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

FileSetAttrib("^r", dir)
attr := FileGetAttrib(dir)

if (attr == "N")
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"

if (DirExist("./FileSetAttrib"))
	DirDelete("./FileSetAttrib", true)