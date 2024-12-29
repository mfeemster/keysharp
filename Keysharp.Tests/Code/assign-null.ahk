x := ""

If (x != "")
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If (x = "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := unset
/*
THIS IS NOT VALID AHK SYNTAX
x :=

If (x != "")
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
*/
	
If (x = "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is null)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x = null)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == null)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 123
x := unset

if (x is unset)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x is null)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x = null)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == null)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"