
b := "blue"
o := "ooo"
r := "red"
x := "xxx"
z := "zzz"

If not (StrCompare(o, b) > 0 and StrCompare(o, r) < 0)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If not (StrCompare(o, r) > 0 and StrCompare(o, b) < 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (StrCompare(o, x) > 0 and StrCompare(o, z) < 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (StrCompare(o, z) > 0 and StrCompare(o, x) < 0)
	FileAppend "pass", "*"	
else
	FileAppend "fail", "*"