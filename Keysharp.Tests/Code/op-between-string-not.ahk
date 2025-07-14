
o := "ooo"

If not (StrCompare(o, "blue") > 0 and StrCompare(o, "red") < 0)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If not (StrCompare(o, "red") > 0 and StrCompare(o, "blue") < 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (StrCompare(o, "xxx") > 0 and StrCompare(o, "zzz") < 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (StrCompare(o, "zzz") > 0 and StrCompare(o, "xxx") < 0)
	FileAppend "pass", "*"	
else
	FileAppend "fail", "*"