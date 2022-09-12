; #Include %A_ScriptDir%/header.ahk

if (DirExist("./DirExist"))
	DirDelete("./DirExist", true)

path := "../../../Keysharp.Tests/Code/"
dir := "./DirExist/SubDir1/SubDir2/SubDir3"
DirCreate(dir)

if (DirExist("./DirExist"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirExist/SubDir1"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirExist/SubDir1/SubDir2"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirExist/SubDir1/SubDir2/SubDir3"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
val := DirExist(dir)

if (val == "D")
	FileAppend, pass, *
else
	FileAppend, fail, *

dir := path . "DirCopy/file1.txt"

if (FileExist(dir))
	FileAppend, pass, *
else
	FileAppend, fail, *

if (DirExist(dir) == "A")
	FileAppend, pass, *
else
	FileAppend, fail, *

; val = Disk.DirExist(dir);
; Assert.AreEqual(val, "A");
; dir = string.Concat(path, "DirCopy/file2.txt");
; Assert.IsTrue(File.Exists(dir));
; val = Disk.DirExist(dir);
; Assert.AreEqual(val, "A");
; dir = string.Concat(path, "DirCopy/file3txt");
; Assert.IsTrue(File.Exists(dir));
; val = Disk.DirExist(dir);
; Assert.AreEqual(val, "A");