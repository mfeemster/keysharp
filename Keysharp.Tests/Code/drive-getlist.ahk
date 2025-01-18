val := DriveGetList()
			
#if WINDOWS
	if (SubStr(val, 1, 1) == "C")
#else
	if (SubStr(val, 1, 1) == "/")
#endif
 	FileAppend "pass", "*"
else
  	FileAppend "fail", "*"