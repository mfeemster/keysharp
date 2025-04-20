if (VerCompare("1.20.0", "1.3") == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", "<1.30") == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", "<=1.30") == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", ">1.30") == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", ">=1.30") == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", "=1.30") == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", "=1.20.0") == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (StrCompare("1.20.0", "1.3") == -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("2.0-a137", "2.0-a136") == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("2.0-a137", "2.0") == -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("10.2-beta.3", "10.2.0") == -1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"