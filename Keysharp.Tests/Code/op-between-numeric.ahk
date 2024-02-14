

x = 1

If x between 0 and 2
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If x between 2 and 0
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If x between 2 and 3
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If x between 0.9 and 1.1
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If x between 1.1 and 0.9
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If x between 0.5 and 0.8
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If x between -1 and 2
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If x between 2 and -1
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If x between -3 and -2
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If x between -2 and -3
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
If x between -0.9 and 1.1
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If x between 1.1 and -0.9
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If x between -0.5 and -0.8
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"