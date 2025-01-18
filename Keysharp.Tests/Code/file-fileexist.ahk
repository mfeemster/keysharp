; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy/*.txt"

val := FileExist(dir)

#if WINDOWS
	if ("A" == val)
#else
	if ("N" == val)
#endif
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"