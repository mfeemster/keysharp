

x := 1

If x > 0 and x < 2
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > 2 and x < 0
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > 2 and x < 3
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > 0.9 and x < 1.1
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > 1.1 and x < 0.9
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > 0.5 and x < 0.8
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > -1 and x < 2
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > 2 and x < -1
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > -3 and x < -2
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > -2 and x < -3
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If x > -0.9 and x < 1.1
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If x > 1.1 and x < -0.9
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If x > -0.5 and x < -0.8
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"