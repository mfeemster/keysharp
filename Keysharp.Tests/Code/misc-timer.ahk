#MaxThreads 2

x := 0

fo1 := Func("TimerHandler")
SetTimer(fo1, 100)

TimerHandler(*)
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
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0
fo1 := Func("TimerHandler2")
SetTimer(fo1, 10)

TimerHandler2(*)
{
global x
	x++
	SetTimer(A_EventInfo, 0)

}

Sleep(100)

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

ExitApp()

x := 0

SetTimer(TimerHandler3, -10) ; Ensure only one timer gets created because the handler is cached.
SetTimer(TimerHandler3, -20)

TimerHandler3()
{
	global x
	x++
}

Sleep(50)

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0
SetTimer(TimerHandler3, -1) ; Ensure the timer is called immediately if the period is 1
Sleep(-1)

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0, doDelayEnd := 0
; Fill max threads with TimerHandler4, and ensure TimerHandler3 was queued
SetTimer(TimerHandler3, -100)
SetTimer(TimerHandler4, -1)
Sleep(-1)

TimerHandler4() {
	global
	Sleep(120)
	doDelayEnd := A_TickCount
}

if (A_TickCount == doDelayEnd)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"