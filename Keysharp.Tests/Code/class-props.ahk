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
	FileAppend, pass, *
else
	FileAppend, fail, *

testclassobj.a := 999

val := testclassobj.a

If (val == 999)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testclass.b

If (val == 555)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testclassobj[3]

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

testclassobj[3] := 100
val := testclassobj[3]

If (val == 100)
	FileAppend, pass, *
else
	FileAppend, fail, *