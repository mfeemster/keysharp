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
SetTimer(fo1, 1000)

TimerHandler2(thefo)
{
global x
	x++
	SetTimer(thefo, 0)

}

Sleep(2000)

if (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

ExitApp()