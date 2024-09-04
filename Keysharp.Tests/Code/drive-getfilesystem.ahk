#if WINDOWS
	val := DriveGetFileSystem("C:\")
#else
	val := DriveGetFileSystem("/dev/sda")
#endif
			
if (val == "NTFS" || val == "FAT32" || val == "FAT" || val == "CDFS" || val == "UDF" || val == "udev")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
