val := SysGet(80)

if (val > 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := SysGet(43)

if (val > 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := SysGet(19)

if (val > 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := SysGet(75)

if (val > 0)
	FileAppend, pass, *
else
	FileAppend, fail, *