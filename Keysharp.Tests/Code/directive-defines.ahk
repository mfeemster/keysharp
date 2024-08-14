x := 10

#if WINDOWS
	x *= 2
#endif

if (x == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10

#if LINUX
	x *= 2
#endif

#if WINDOWS
	if (x == 10)
#elif LINUX
	if (x == 20)
#endif
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10

#if 1
	x := 100
#else
	x := 200
#endif

if (x == 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10

#if 0
	x := 100
#else
	x := 200
#endif

if (x == 200)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
