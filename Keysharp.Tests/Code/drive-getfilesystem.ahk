; #Include %A_ScriptDir%/header.ahk

val := DriveGetFileSystem("C:\")
			
if (val == "NTFS" || val == "FAT32" || val == "FAT" || val == "CDFS" || val == "UDF")
	FileAppend, pass, *
else
	FileAppend, fail, *
