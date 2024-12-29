

x := 1

If !(x > 0 and x < 2)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If !(x > 2 and x < 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If !(x > 2 and x < 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If !(x > 0.9 and x < 1.1)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If !(x > 1.1 and x < 0.9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If !(x > 0.5 and x < 0.8)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (x > -1 and x < 2)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If not (x > 2 and x < -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (x > -3 and x < -2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (x > -2 and x < -3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (x > -0.9 and x < 1.1)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If !(x > 1.1 and x < -0.9)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If ! (x > -0.5 and x < -0.8)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"