#if WINDOWS
	val := DriveGetType("C:\")
#else
	val := DriveGetType("/dev/sda")
#endif

if (val == "Fixed" || val == "RAMDisk")
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"