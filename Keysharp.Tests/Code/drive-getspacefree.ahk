#if WINDOWS
	val := DriveGetSpaceFree("C:\")
#else
	val := DriveGetSpaceFree("/dev/sda")
#endif
			
if (val > 10)
 	FileAppend, "pass", "*"
else
  	FileAppend, "fail", "*"