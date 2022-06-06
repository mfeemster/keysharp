; #Include %A_ScriptDir%/header.ahk

if (DirExist("./DirDelete"))
	DirDelete("./DirDelete", true)

dir := "./DirDelete/SubDir1/SubDir2/SubDir3"
DirCreate(dir)

if (DirExist("./DirDelete"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirDelete/SubDir1"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirDelete/SubDir1/SubDir2"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirDelete/SubDir1/SubDir2/SubDir3"))
	FileAppend, pass, *
else
	FileAppend, fail, *

try
{
	DirDelete("./DirDelete")
}
catch
{
}

if (DirExist("./DirDelete"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirDelete/SubDir1"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirDelete/SubDir1/SubDir2"))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (DirExist("./DirDelete/SubDir1/SubDir2/SubDir3"))
	FileAppend, pass, *
else
	FileAppend, fail, *

DirDelete("./DirDelete", true)

if (DirExist("./DirDelete"))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
if (DirExist("./DirDelete/SubDir1"))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
if (DirExist("./DirDelete/SubDir1/SubDir2"))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
if (DirExist("./DirDelete/SubDir1/SubDir2/SubDir3"))
	FileAppend, fail, *
else
	FileAppend, pass, *