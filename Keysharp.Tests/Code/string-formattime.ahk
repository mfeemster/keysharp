

x := "20200704070809"
y := FormatTime(x, "d")

if (y = "4")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

y := FormatTime(x, "dd")

if (y = "04")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "ddd")

if (y = "Sat")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "dddd")

if (y = "Saturday")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "M")

if (y = "7")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "MM")

if (y = "07")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "yyyy")

if (y = "2020")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "shortdate")

if (y = "7/4/2020")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "LongDate")

if (y = "Saturday, July 4, 2020")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "'Date:' yyyyMMMMdddd")

if (y = "Date: 2020JulySaturday")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "'Date:' yyyyMMMMdddd ''''")

if (y = "Date: 2020JulySaturday '")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
y := FormatTime(x, "'Date:' yyyyMMMMdddd `"''`"")

if (y = "Date: 2020JulySaturday '")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"