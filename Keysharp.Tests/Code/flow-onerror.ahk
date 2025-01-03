OnError("LogError1")
OnError("LogError2")
OnError("LogError3")

LogError1(exception, mode) {
	global x++
}

LogError2(exception, mode) {
	global x++
}

LogError3(exception, mode) {
	global x++
	return -1
}

x := 0
WinActivate("C3D38B48-B165-4A69-9D8F-020DCD360712")

if (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

OnError("LogError1", 0)
OnError("LogError2", 0)

x := 0
WinActivate("C3D38B48-B165-4A69-9D8F-020DCD360712")

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

OnError("LogError3", 0)