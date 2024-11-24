if (-1 == Float(-1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (1 == Float(1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-2.1 == Float(-2.1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Float(0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Float(-0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0.5 == Float(0.5))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (1.000001 == Float(1.000001))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

b := false

try
{
	Float("asdf")
}
catch
{
	b := true
}
	
if (b)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
