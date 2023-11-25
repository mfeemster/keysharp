d1 := "20050126"
d2 := "20040126"
val := DateDiff(d1, d2, "days")

if (val == 366)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230110"
d2 := "20230115"
val := DateDiff(d2, d1, "days")

if (val == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateDiff(d1, d2, "days")

if (val == -5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
d1 := "2023021002"
d2 := "2023021001"
val := DateDiff(d1, d2, "h")

if (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
val := DateDiff(d2, d1, "h")

if (val == -1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "202302100230"
d2 := "202302100225"
val := DateDiff(d1, d2, "m")

if (val == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateDiff(d2, d1, "m")

if (val == -5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

d1 := "20230210023015"
d2 := "20230210022510"
val := DateDiff(d1, d2, "s")

if (val == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := DateDiff(d2, d1, "s")

if (val == -5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"