

PI = 3.1415926535897931

if (-1 == Cos(-1 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (6.123233995736766E-17 == Cos(-0.5 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (1 == Cos(0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (1 == Cos(-0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (6.123233995736766E-17 == Cos(0.5 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (-1 == Cos(1 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-0.5224985647159488 == Cos(0.675 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
