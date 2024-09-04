#define SOMETHING
#define SOMETHING_UNDERSCORE

x := 10

#if WINDOWS
	x *= 2
#endif

#if WINDOWS
	if (x == 20)
#elif LINUX
	if (x == 10)
#endif
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

x := 10

#if (((WINDOWS || LINUX) && 0))
	x *= 2
#endif

if (x == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10

#if (((WINDOWS || LINUX) && 1))
	x *= 2
#endif

if (x == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; False outer with true inner.
x := 10

#if LINUX
	#if LINUX
		x := 20
	#else
		x := 1
	#endif
#endif

#if WINDOWS
	if (x == 10)
#elif LINUX
	if (x == 20)
#endif
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; True outer with false inner.
x := 10

#if WINDOWS
	#if LINUX
		x := 20
	#else
		x := 1
	#endif
#endif

#if WINDOWS
	if (x == 1)
#elif LINUX
	if (x == 10)
#endif
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

str := ""

#if WINDOWS
	#if WINDOWS
		str .= "windows"
	#elif LINUX
		str .= "linux"
	#else
		str .= "unknown"
	#endif
#elif LINUX
	str .= "linux"
#else
	str .= "unknown"
#endif

#if WINDOWS
	if (str == "windows")
#elif LINUX
	if (str == "linux")
#endif
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

str := ""

#if !WINDOWS
    str := "not windows"
#elif !LINUX
    str := "not linux"
#else
	str := "not unknown"
#endif

#if WINDOWS
	if (str == "not linux")
#else
	if (str == "not windows")
#endif
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10

#if (SOMETHING)
	x *= 2
#endif

if (x == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10

#if !(SOMETHING)
	x *= 2
#endif

if (x == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 10

#if SOMETHING_UNDERSCORE
	x *= 2
#endif

if (x == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Test undefining something that has been predefined.
x := false

#undef SOMETHING

#if SOMETHING
	x := true
#endif

if (!x)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := false

#define SOMETHING

#if SOMETHING
	x := true
#endif

if (x)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
