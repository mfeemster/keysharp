; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileCreateShortcut"))
	DirDelete("./FileCreateShortcut", true)

if (FileExist("./testshortcut.lnk"))
	FileDelete("./testshortcut.lnk")

path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy"
DirCopy(dir, "./FileCreateShortcut/")
	
if (FileExist("./fileappend.txt"))
	FileDelete("./fileappend.txt")
	
FileCreateShortcut("./FileCreateShortcut/file1.txt", "./testshortcut.lnk", "", "", "TestDescription", "../../../Keysharp.ico", "")

if (FileExist("./testshortcut.lnk"))
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

if (DirExist("./FileCreateShortcut"))
	DirDelete("./FileCreateShortcut", true)

if (FileExist("./testshortcut.lnk"))
	FileDelete("./testshortcut.lnk")
