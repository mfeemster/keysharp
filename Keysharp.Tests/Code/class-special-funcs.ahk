gval := 0

class testclass
{
	__New()
	{
		program.gval := 100
	}

	__Delete()
	{
		program.gval := 999
	}
}

testclassobj := testclass()

testclassobj := ""

while (gval != 999)
{
	Sleep(100)
	Collect()
}

If (gval == 999)
	FileAppend, pass, *
else
	FileAppend, fail, *

class enumclass
{
	arr := [1, 2, 3]

	__Enum(2)
	{
		return arr
	}
}

gval := 0
testclassobj := enumclass()

for i,v in testclassobj
{
	gval += v
}

If (gval == 6)
	FileAppend, pass, *
else
	FileAppend, fail, *

class subenumclass extends enumclass
{
	subarr := [4, 5, 6]

	__Enum(2)
	{
		return subarr
	}
}

gval := 0
testclassobj := subenumclass()

for i,v in testclassobj
{
	gval += v
}

If (gval == 15)
	FileAppend, pass, *
else
	FileAppend, fail, *

class testclass2
{
	a := 1
	b := 2
	c := 3
}

testclassobj := testclass2()
cloneobj := testclassobj.Clone()

If (cloneobj.a == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (cloneobj.b == 2)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (cloneobj.c == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *