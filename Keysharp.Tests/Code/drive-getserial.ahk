#if WINDOWS
	val := DriveGetSerial("C:\")
			
	if (val > 1)
#else
	val := DriveGetSerial("/dev/sda")
			
	if (val >= 0)
#endif
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"