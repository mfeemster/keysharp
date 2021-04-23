; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/DirCopy/file1.txt"
splitpath := SplitPath(path)
filename := splitpath.OutFileName
ext := splitpath.OutExtension
namenoext := splitpath.OutNameNoExt

; Can't really to drive or path, because it differs by OS.

if (filename == "file1.txt")
 	FileAppend, pass, *
else
  	FileAppend, fail, *
	
if (ext == "txt")
 	FileAppend, pass, *
else
  	FileAppend, fail, *
	
if (namenoext == "file1")
 	FileAppend, pass, *
else
  	FileAppend, fail, *