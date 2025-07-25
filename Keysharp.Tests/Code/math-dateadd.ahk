d1 := "20040126000000"
d2 := "20050126000000"
val := DateAdd(d1, 366, "days")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateAdd(d2, -366, "days")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "2023021002"
d2 := "20230210070000"
val := DateAdd(d1, 5, "h")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210020000"
val := DateAdd(d2, -5, "h")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "202302100225"
d2 := "20230210023000"
val := DateAdd(d1, 5, "m")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210022500"
val := DateAdd(d2, -5, "m")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210022510"
d2 := "20230210022515"
val := DateAdd(d1, 5, "s")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateAdd(d2, -5, "s")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210022500"
d2 := "20230210022530"
val := DateAdd(d1, 0.5, "m")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateAdd(d2, -0.5, "m")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210020000"
d2 := "20230210023000"
val := DateAdd(d1, 0.5, "h")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateAdd(d2, -0.5, "h")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20040126000000"
d2 := "20040126120000"
val := DateAdd(d1, 0.5, "d")

if (d2 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateAdd(d2, -0.5, "d")

if (d1 == val)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210023015.100"
d2 := "20230210023015.500"
val := DateAdd(d1, 400, "l")

if (val == d2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
d1 := "20230210023015.100"
d2 := "20230210023016.100"
val := DateAdd(d1, 1, "s")

if (val == d2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"