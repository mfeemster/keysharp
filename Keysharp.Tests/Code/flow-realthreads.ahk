lockit := ""
tharr := []
tharr.Length := 100
tot := 0

rtAddTot(o)
{
	global tot
	tot += o
}

rtfunc1(obj)
{
	LockRun(lockit, (o) => rtAddTot(o), obj)
}

fo := Func("rtfunc1")

Loop 100
{
	tharr[A_Index] := StartRealThread(fo, A_Index).ContinueWith(fo, 1)
}

Loop 100
{
	tharr[A_Index].Wait()
}

tharr.Clear()

If tot == 5150
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

tharr := []
tharr.Length := 100
tot := 0

rtSumTot()
{
	ct := 0
    
	Loop 100
	{
		ct++
	}

	return ct
}

rtfunc2()
{
	return rtSumTot()
}

fo := Func("rtfunc2")

Loop 100
{
	tharr[A_Index] := StartRealThread(fo)
}

Loop 100
{
	tot += tharr[A_Index].Wait()
}

tharr.Clear()

If tot == 10000
	FileAppend "pass", "*"
else
	FileAppend, "fail", "*"

CoordMode "Mouse", "Screen"

StartRealThread(RealThreadEntry)

cb2 := CallbackCreate(SetCoordModeMouseClient)
Loop 10000 {
	DllCall(cb2)
}

if A_CoordModeMouse = "Screen"
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

RealThreadEntry() {
	CoordMode "Mouse", "Screen"
	cb1 := CallbackCreate(SetCoordModeMouseWindow)

	Loop 10000 {
		DllCall(cb1)
	}
}

SetCoordModeMouseWindow() {
	if A_CoordModeMouse = "Client" {
		FileAppend "fail", "*"
		ExitApp()
	}

	CoordMode "Mouse", "Window"
}

SetCoordModeMouseClient() {
	if A_CoordModeMouse = "Window" {
		FileAppend "fail", "*"
		ExitApp()
	}

	CoordMode "Mouse", "Client"
}