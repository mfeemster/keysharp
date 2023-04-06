; #Include %A_ScriptDir%/header.ahk

if (DirExist("./FileGetShortcut"))
	DirDelete("./FileGetShortcut", true)

if (FileExist("./testshortcut.lnk"))
	FileDelete("./testshortcut.lnk")

path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy"
DirCopy(dir, "./FileGetShortcut/")
fullpath := FileDirName("./FileGetShortcut/file1.txt")

FileCreateShortcut("./FileGetShortcut/file1.txt", "./testshortcut.lnk", fullpath, "", "TestDescription", "../../../Keysharp.ico", "")

if (FileExist("./testshortcut.lnk"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

shortcut := FileGetShortcut("./testshortcut.lnk")

if (StrLower(FileFullPath("./FileGetShortcut/file1.txt")) == StrLower(shortcut.OutTarget))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (fullpath == shortcut.OutDir)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if ("TestDescription" == shortcut.OutDescription)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if ("" == shortcut.OutArgs)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileFullPath("../../../Keysharp.ico") == shortcut.OutIcon)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if ("0" == shortcut.OutIconNum)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if ("1" == shortcut.OutRunState)
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (DirExist("./FileGetShortcut"))
	DirDelete("./FileGetShortcut", true)

if (FileExist("./testshortcut.lnk"))
	FileDelete("./testshortcut.lnk")