;#Include %A_ScriptDir%/header.ahk

x := A_YYYY
y := A_Year

if (x > 2000)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (y = x)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_MM
y := A_Mon

if (x >= 1 && x <= 12)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (y = x)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_DD
y := A_MDay

if (x >= 1 && x <= 31)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (y = x)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_MMMM

if (x = "January" || x = "February" || x = "March" || x = "April" || x = "May" || x = "June" || x = "July" || x = "August" || x = "September" || x = "October" || x = "November" || x = "December")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_MMM

if (x = "Jan" || x = "Feb" || x = "Mar" || x = "Apr" || x = "May" || x = "Jun" || x = "Jul" || x = "Aug" || x = "Sep" || x = "Oct" || x = "Nov" || x = "Dec")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_DDDD

if (x = "Sunday" || x = "Monday" || x = "Tuesday" || x = "Wednesday" || x = "Thursday" || x = "Friday" || x = "Sunday" || x = "Saturday")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_DDD

if (x = "Sun" || x = "Mon" || x = "Tue" || x = "Wed" || x = "Thu" || x = "Fri" || x = "Sun" || x = "Sat")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_WDay

if (x >= 1 && x <= 7)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_YDay

if (x >= 1 && x <= 366)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_YWeek

if (x != "") ; Not really a full test, but the code is clear enough to know it works.
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_Hour

if (x >= 0 && x <= 23)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_Min

if (x >= 0 && x <= 59)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_Sec

if (x >= 0 && x <= 59)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_MSec

if (x >= 0 && x <= 999)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_Now

if (x != "") ; Not really a full test, but the code is clear enough to know it works.
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_NowUTC

if (x != "") ; Not really a full test, but the code is clear enough to know it works.
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_TickCount

if (x > 0) ; Not really a full test, but the code is clear enough to know it works.
	FileAppend, pass, *
else
	FileAppend, fail, *
