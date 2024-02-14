

PI = 3.1415926535897931

if (-11.548739357257746 == Sinh(-1 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (-2.3012989023072947 == Sinh(-0.5 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Sinh(0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (0 == Sinh(-0))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (2.3012989023072947 == Sinh(0.5 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (11.548739357257746 == Sinh(1 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (4.107983493619838 == Sinh(0.675 * PI))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"