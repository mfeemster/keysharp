; #Include %A_ScriptDir%/header.ahk

if (DirExist("./DirExist"))
	DirDelete("./DirExist", true)

path := "../../../Keysharp.Tests/Code/"
dir := "./DirExist/SubDir1/SubDir2/SubDir3"
DirCreate(dir)

if (DirExist("./DirExist"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (DirExist("./DirExist/SubDir1"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (DirExist("./DirExist/SubDir1/SubDir2"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (DirExist("./DirExist/SubDir1/SubDir2/SubDir3"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := DirExist(dir)

if (val == "D")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dir := path . "DirCopy/file1.txt"

if (FileExist(dir))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#if WINDOWS
	if (DirExist(dir) == "A")
#else
	if (DirExist(dir) == "N")
#endif
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dir := path . "DirCopy/file2.txt"

if (FileExist(dir))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#if WINDOWS
	if (DirExist(dir) == "A")
#else
	if (DirExist(dir) == "N")
#endif
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dir := path . "DirCopy/file3txt"

if (FileExist(dir))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#if WINDOWS
	if (DirExist(dir) == "A")
#else
	if (DirExist(dir) == "N")
#endif
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"