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

; Same, but with the first string being a C# style version strings with 4 numbers.
if (VerCompare(" 1.20.0.1", "<1.30") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0.1 ", "<=1.30") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0.1", " >1.30") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0.1", " >=1.30 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0.1", " =1.30 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0.1 ", " =1.20.0 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0.1 ", " >1.20.0 ") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; With the second being such.
if (VerCompare(" 1.20.0", "<1.30.0.1") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0 ", "<=1.30.0.1") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", " >1.30.0.1") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0", " >=1.30.0.1 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0", " =1.30.0.1 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0 ", " =1.20.0.0 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0 ", " <1.20.0.1 ") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; With both.
if (VerCompare(" 1.20.0.0", "<1.30.0.1") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0.0 ", "<=1.30.0.1") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0.0", " >1.30.0.1") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare("1.20.0.0", " >=1.30.0.1 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0.0", " =1.30.0.1 ") = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0.0 ", " =1.20.0.0 ") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (VerCompare(" 1.20.0.0 ", " <1.20.0.1 ") = 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; SemVer style.
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