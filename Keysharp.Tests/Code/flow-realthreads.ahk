thi := 0
lockit := ""
tharr := []
tharr.Length := 100
tot := 0

func1(obj)
{
	global tot
	LockObject(lockit, (o) => tot += o, obj)
}

Loop 100
{
	tharr[++thi] := StartRealThread("func1", A_Index)
}

thi := 0

Loop 100
{
	WaitRealThread(tharr[++thi])
}

tharr.Clear()

If tot == 5050
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"