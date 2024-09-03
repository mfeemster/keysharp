; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/DirCopy/file1.txt"
filename := 
dir :=
ext :=
drive := 
namenoext := ""
SplitPath(path, &filename, &dir, &ext, &namenoext, &drive)

if (filename == "file1.txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (ext == "txt")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (namenoext == "file1")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

#if WINDOWS
	if (StrLower("D:\Dev\keysharp\Keysharp.Tests\Code\DirCopy") == StrLower(dir))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	if (StrLower("D:\") == StrLower(drive))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
#else
	if (StrLower("/home/" . A_UserName . "/Dev/Keysharp/Keysharp.Tests/Code/DirCopy") == StrLower(dir))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	if ("/" == StrLower(drive))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

#endif