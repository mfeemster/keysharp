#if WINDOWS
	val := DriveGetCapacity("C:\")
#else
	val := DriveGetCapacity("/dev/sda")
#endif
			
if (val > 1000)
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"