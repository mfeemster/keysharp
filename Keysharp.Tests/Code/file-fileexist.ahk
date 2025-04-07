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

#if	WINDOWS
if (FileExist(A_MyDocuments) == "RD") ; Unsure what it is in linux.//TODO
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
#endif