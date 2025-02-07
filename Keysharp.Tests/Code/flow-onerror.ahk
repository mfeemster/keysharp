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

x := 0

fo1 := Func("TimerHandler")
SetTimer(fo1, 100)

TimerHandler()
{
global
	x++

	if (x == 1)
	{
		SetTimer(fo1, 0)
	}

	Exit()
	x := 123
}

Sleep(1000)

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

OnError("LogError3", 0)
ExitApp()