

if (-2 == Floor(-1.5))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (-1 == Floor(-1))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (-1 == Floor(-0.5))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (0 == Floor(0))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (0 == Floor(-0))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (0 == Floor(0.5))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (1 == Floor(1))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (1 == Floor(1.675))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
