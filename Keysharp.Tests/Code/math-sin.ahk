

PI = 3.1415926535897931

if (-1.2246467991473532E-16 == Sin(-1 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-1 == Sin(-0.5 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Sin(0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Sin(-0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (1 == Sin(0.5 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (1.2246467991473532E-16 == Sin(1 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0.8526401643540923 == Sin(0.675 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
