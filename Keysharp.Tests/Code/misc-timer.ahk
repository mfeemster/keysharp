x := 0

fo1 := FuncObj("TimerHandler")
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

Sleep(2000)

if (x == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

; For some reason, ExitApp() causes the unit test to think it failed. So you need to manually close the main window for this test for it to be considered a success.
; ExitApp(0) ; Needed because it sees timer as a persistent function, so it opens the standard GUI.