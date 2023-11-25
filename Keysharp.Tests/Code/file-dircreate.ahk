; #Include %A_ScriptDir%/header.ahk

if (DirExist("./DirCreate"))
	DirDelete("./DirCreate", true)

dir := "./DirCreate/SubDir1/SubDir2/SubDir3"
DirCreate(dir)
	
if (DirExist("./DirCreate"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (DirExist("./DirCreate/SubDir1"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (DirExist("./DirCreate/SubDir1/SubDir2"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (DirExist("./DirCreate/SubDir1/SubDir2/SubDir3"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (DirExist("./DirCreate"))
	DirDelete("./DirCreate", true)

if (DirExist("./DirCreate"))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"