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
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class enumclass
{
	arr := [1, 2, 3]

	__Enum(ct)
	{
		return arr.__Enum(ct)
	}
}

gval := 0
testclassobj := enumclass()

for i,v in testclassobj
{
	gval += v
}

If (gval == 6)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class subenumclass extends enumclass
{
	subarr := [4, 5, 6]

	__Enum(ct)
	{
		return subarr.__Enum(ct)
	}
}

gval := 0
testclassobj := subenumclass()

for i,v in testclassobj
{
	gval += v
}

If (gval == 15)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class testclass2
{
	a := 1
	b := 2
	c := 3
}

testclassobj := testclass2()
cloneobj := testclassobj.Clone()

If (cloneobj.a == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (cloneobj.b == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (cloneobj.c == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class testclass3 {
	static Call(a) {
		return a * 10
	}
}

val := testclass3(10)

If (val == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"


val := TestWithCustomStaticCall() ; internally calls the custom Call() to return 123 instead of a new object

if (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := TestWithCustomStaticCall.Call() ; also returns 123

if (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; class with one custom static Call() method which replaces the default one.
; this prevents an instance of this class from every being created.
class TestWithCustomStaticCall
{
	static Call()
	{
		return 123
	}
}

class TestWithCustomInstanceCall
{
	Call()
	{
		return 123
	}
}

obj := TestWithCustomInstanceCall() ; creates an instance of the class.

if (obj is TestWithCustomInstanceCall)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := obj.Call() ; intelligent enough to resolve to the instance Call() to return 123, instead of the default static one.

if (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"