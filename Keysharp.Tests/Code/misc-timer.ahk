x := 0

fo1 := Func("TimerHandler")
SetTimer(fo1, 100)

TimerHandler()
{
global
	x++

	if (x == 5)
	{
		SetTimer(fo1, 0)
	}
}

Sleep(1000)

if (x == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
fo1 := Func("TimerHandler2")
SetTimer(fo1, 500)

TimerHandler2()
{
global x
	x++
	SetTimer(A_EventInfo, 0)

}

Sleep(2000)

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

ExitApp()

x := 0

SetTimer(TimerHandler3, -100) ; Ensure only one timer gets created because the handler is cached.
SetTimer(TimerHandler3, -200)

TimerHandler3()
{
	global x
	x++
}

Sleep(1000)

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"