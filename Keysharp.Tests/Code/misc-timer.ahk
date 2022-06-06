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

MsgBox("Please show the window from the system tray by double clicking the icon, then closing the main window for the misc-timer unit test to complete.")