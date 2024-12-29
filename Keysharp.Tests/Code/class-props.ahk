class testclass
{
	_a := 123
	static _b := 555
	arr := [1, 2, 3, 4, 5, 6]

	a
	{
		get
		{
			return _a
		}

		set
		{
			global _a := value
		}
	}

	static b
	{
		get
		{
			return _b
		}
	}

	__Item[x]
	{
		get
		{
			return arr[x]
		}

		set
		{
			arr[x] := value
		}
	}
}

testclassobj := testclass()
val := testclassobj.a

If (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

testclassobj.a := 999

val := testclassobj.a

If (val == 999)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testclass.b

If (val == 555)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := testclassobj[3]

If (val == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

testclassobj[3] := 100
val := testclassobj[3]

If (val == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class PropTestOTB
{
	x := 0
	__Item[name] {
		get {
		global
		return x
		}
		set {
		global
		x := value
		}
	}
}

otb := PropTestOTB()
otb[999] := 123
val := otb[777]

if (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
class PropTestThis
{
	x := 0
	xprop {
		get {
		global
		return x
		}
		set {
		this.x := value
		}
	}
}

ptt := PropTestThis()
ptt.xprop := 123
val := ptt.xprop

if (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
