

ct := MonitorGetCount()
names := ""

loop ct
	names .= MonitorGetName(A_Index)
	
if (names != "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"