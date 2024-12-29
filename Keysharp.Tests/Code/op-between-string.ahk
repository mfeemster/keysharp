
o := "ooo"

If StrCompare(o, "blue") > 0 and StrCompare(o, "red") < 0
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If StrCompare(o, "red") > 0 and StrCompare(o, "blue") < 0
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If StrCompare(o, "xxx") > 0 and StrCompare(o, "zzz") < 0
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If StrCompare(o, "zzz") > 0 and StrCompare(o, "xxx") < 0
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"	