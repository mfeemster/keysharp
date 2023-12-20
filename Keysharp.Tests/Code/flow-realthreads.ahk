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

Loop 100
{
	tharr[++thi] := StartRealThread("func1", A_Index)
}

thi := 0

Loop 100
{
	tharr[++thi].Wait()
}

tharr.Clear()

If tot == 5050
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"