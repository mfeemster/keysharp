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

	__Item[X] ;Change case on purpose.
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

if (ObjHasOwnProp(testclassobj, "__Item") && testclassobj.HasOwnProp("__Item"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := testclassobj.a

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testclassobj.a := 999

val := testclassobj.a

If (val == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclass.b

If (val == 555)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclassobj[3]

If (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testclassobj[3] := 100
val := testclassobj[3]

If (val == 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

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

if (ObjHasOwnProp(otb, "__Item") && otb.HasOwnProp("__Item"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
otb[999] := 123
val := otb[777]

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
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

if (!ObjHasOwnProp(ptt, "__Item") && !ptt.HasOwnProp("__Item"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
ptt.xprop := 123
val := ptt.xprop

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Ensure the special super property is properly implemented.
x := 0

class Test1 extends Test2 {
	Meth1()
	 {
		global x++
		return super.Meth1()
	}
}

class Test2 extends Test3 {
	Meth1()
	{
		global x++
		return super.Meth1()
	}
}

class Test3 {
	Meth1()
	{
		global x
		return x++
	}
}

t1 := test1()
y := t1.Meth1()

if (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (!ObjHasOwnProp(t1, "__Item") && !t1.HasOwnProp("__Item"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"