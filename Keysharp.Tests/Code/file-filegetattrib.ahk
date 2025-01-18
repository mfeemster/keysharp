; #Include %A_ScriptDir%/header.ahk

path := "../../../Keysharp.Tests/Code/DirCopy"
val := FileGetAttrib(path)

if (val == "D")
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

val := FileGetAttrib(path . "/file1.txt")

#if WINDOWS
	if ("A" == val)
#else
	if ("N" == val)
#endif
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

val := FileGetAttrib(path . "/file2.txt")

#if WINDOWS
	if ("A" == val)
#else
	if ("N" == val)
#endif
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"

val := FileGetAttrib(path . "/file3txt")

#if WINDOWS
	if ("A" == val)
#else
	if ("N" == val)
#endif
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"