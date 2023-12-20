thi := 0
lockit := ""
tharr := []
tharr.Length := 100
tot := 0

AddTot(o)
{
	global tot
	tot += o
}

func1(obj)
{
	LockRun(lockit, (o) => AddTot(o), obj)
}

fo := FuncObj("func1")

Loop 100
{
	tharr[A_Index] := StartRealThread(fo, A_Index).ContinueWith(fo, 1)
}

thi := 0

Loop 100
{
	tharr[A_Index].Wait()
}

tharr.Clear()

If tot == 5150
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"