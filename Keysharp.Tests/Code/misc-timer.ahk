x := 0

fo1 := FuncObj("TimerHandler")
SetTimer(fo1, 1000)

TimerHandler()
{
global
	x++

	if (x == 5)
	{
		SetTimer(fo1, 0)
	}
}

Sleep(6000)

if (x == 5)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := 0
fo1 := FuncObj("TimerHandler2")
SetTimer(fo1, 1000)

TimerHandler2(thefo)
{
global x
	x++
	SetTimer(thefo, 0)

}

Sleep(3000)

if (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

ExitApp() ; Needed because it sees timer as a persistent function, so it opens the standard GUI.