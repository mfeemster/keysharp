; #Include %A_ScriptDir%/header.ahk

if (DirExist("./DirMove"))
	DirDelete("./DirMove", true)

if (DirExist("./DirCopy3"))
	DirDelete("./DirCopy3", true)

if (DirExist("./DirCopy3-rename"))
	DirDelete("./DirCopy3-rename", true)

path := "../../../Keysharp.Tests/Code/"
dir := path . "DirCopy"

DirCopy(dir, "./DirMove")
	
if (DirExist("./DirMove"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirMove/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirMove/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirMove/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

DirMove("./DirMove", "./DirCopy3")

if (!DirExist("./DirMove"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *
	
if (DirExist("./DirCopy3"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirCopy3/file1.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirCopy3/file2.txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirCopy3/file3txt"))
 	FileAppend, pass, *
else
  	FileAppend, fail, *

DirMove("./DirCopy3", "./DirCopy3") ;Both of these should just return, do nothing, and not throw because ./DirCopy3 exists.
DirMove("./DirCopy3", "./DirCopy3", 0)
DirCopy(dir, "./DirMove")
DirMove("./DirMove", "./DirCopy3", 1) ;Will copy into because ./DirCopy3 already exists.

if (DirExist("./DirCopy3/DirMove"))
	FileAppend, pass, *
else
  	FileAppend, fail, *

if (FileExist("./DirCopy3/DirMove/file1.txt"))
	FileAppend, pass, *
else
  	FileAppend, fail, *


if (FileExist("./DirCopy3/DirMove/file2.txt"))
	FileAppend, pass, *
else
  	FileAppend, fail, *


if (FileExist("./DirCopy3/DirMove/file3txt"))
	FileAppend, pass, *
else
  	FileAppend, fail, *
	
DirMove("./DirCopy3", "./DirCopy3-rename", "R")

if (DirExist("./DirMove"))
	DirDelete("./DirMove", true)

if (DirExist("./DirCopy3"))
	DirDelete("./DirCopy3", true)
	
if (DirExist("./DirCopy3-rename"))
	DirDelete("./DirCopy3-rename", true)