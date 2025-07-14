x := ""

If (x != "")
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If (x = "")
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

if (null = x)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (null == x)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x != null)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (x !== null)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

if (null != x)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
if (null !== x)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
