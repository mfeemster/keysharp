class testclass
{
	a := 123
	b := 456
	static c := 888
	static d := 1000

	BaseCaseSensitiveFunc()
	{
		global a := 999
		this.a := 1212
	}
	
	static BaseCaseSensitiveFuncStatic()
	{
		global c := 3131
	}
}

class testsubclass extends testclass
{
	_a := 321

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

	static b := 654

	_c := 999

	c
	{
		get
		{
			return _c
		}

		set
		{
			global _c := value
		}
	}

	static d := 2000
	
	setbasea()
	{
		super.a := 500
	}

	GetBasea()
	{
		return super.a
	}

	SubCaseSensitiveFunc()
	{
		basecasesensitivefunc()
	}
	
	static SubCaseSensitiveFuncStatic()
	{
		testclass.basecasesensitivefuncstatic()
	}
}

testclassobj := testclass()
testsubclassobj := testsubclass()

If (testclassobj is testclass)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (testsubclassobj is testclass)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (testsubclassobj is testsubclass)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclassobj.a

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.a

If (val == 321)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclassobj.b

If (val == 456)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclass.b

If (val == 654)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclass.c

If (val == 888)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.c

If (val == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testclass.d

If (val == 1000)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclass.d

If (val == 2000)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
testsubclassobj.setbasea()

val := testsubclassobj.getbasea()

If (val == 500)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.a

If (val == 321)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
testsubclassobj.super.a := 777

val := testsubclassobj.getbasea()

If (val == 777)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := testsubclassobj.a

If (val == 321)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

classname := testclassobj.__Class

If (classname == "testclass")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

classname := testsubclassobj.__Class

If (classname == "testsubclass")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testsubclassobj.a := ""
testsubclassobj.super.a := ""

testsubclassobj.subcasesensitivefunc()

if (testsubclassobj.super.a == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (testsubclassobj.a == 1212)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testclass.c := ""
testsubclass.subcasesensitivefuncstatic()

if (testclass.c == 3131)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class MyArray extends Array
{
	__Item[index]
	{
		get
		{
			return 123
		}
	}
}

classname := MyArray()

If (classname is Array)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (classname is MyArray)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := classname[100]

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class MyMap extends Map
{
	__Item[index]
	{
		get
		{
			return 321
		}
	}
}

classname := MyMap()

If (classname is Map)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (classname is MyMap)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := classname[100]

If (val == 321)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class base1
{
	__Item[index]
	{
		get
		{
			return 1
		}
	}
}

class sub1 extends base1
{
	__Item[index]
	{
		get
		{
			return 2
		}
	}
}

class subarr1 extends Array
{
}

class subarr2 extends subarr1
{
	__Item[index]
	{
		get
		{
			return 3
		}
	}
}

obj := sub1()
val := obj[999]

If (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

obj := subarr2()

If (obj is subarr2 && obj is subarr1 && obj is Array)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := obj[999]

If (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class submap1 extends Map
{
}

class submap2 extends submap1
{
	__Item[index]
	{
		get
		{
			return 4
		}
	}
}

obj := submap2()

If (obj is submap2 && obj is submap1 && obj is Map)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := obj[999]

If (val == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testclass.c := 101
myfunc := Func("basecasesensitivefuncstatic", testsubclassobj)

myfunc()

if (testclass.c == 3131)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testclass.c := 101
myfunc := Func("subcasesensitivefuncstatic", testsubclassobj)

myfunc()

if (testclass.c == 3131)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

testsubclassobj.a := 0
myfunc := Func("basecasesensitivefunc", testsubclassobj)
myfunc()

if (testsubclassobj.a == 1212)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
testsubclassobj.a := 0
myfunc := Func("SubCaseSensitiveFunc", testsubclassobj)
myfunc()

if (testsubclassobj.a == 1212)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myarrayclass1 extends Array
{
	__item[k1]
	{
		get
		{
			return 1
		}
		
	}
}

mac := myarrayclass1()
mac.Push(123)
val := mac[1]

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

mac[1] := 999
val := mac[1]

If (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myarrayclass2 extends Array
{
	__item[p1]
	{
		set
		{
			super[p1] := value
		}
	}
}

mac := myarrayclass2()
mac.Push(123)
val := mac[1]

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

mac[1] := 999
val := mac[1]

If (val == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class mymapclass1 extends map
{
	__item[a]
	{
		get
		{
			return super[a]
		}

		set
		{
			super[a] := value
		}
	}

	__item[A, B] ;Change case on purpose.
	{
		get
		{
			return super[a + b]
		}

		set
		{
			super[a + b] := a + b
		}
	}


	__item[a, b, c]
	{
		get
		{
			return super[a + b + c]
		}

		set
		{
			super[a + b + c] := a + b + c
		}
	}
}

mmp := mymapclass1()

mmp["asdf"] := 123
val := mmp["asdf"]

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

mmp[1, 2] := 123
val := mmp[1, 2]

If (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

mmp[1, 2, 3] := 123
val := mmp[1, 2, 3]

If (val == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
class myarrayclass3 extends Array
{
	__item[p1*]
	{
		set
		{
			temp := 0

			for n in p1
			{
				temp += n
			}

			super[p1[1]] := temp + value
		}
	}
}

mac := myarrayclass3()
mac.Push(1)
mac[1, 2, 3, 4] := 100
val := mac[1]

if (val == 110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myarrayclass4 extends Array
{
	__item[p1*]
	{
		get
		{
			temp := 0

			for n in p1
			{
				temp += n
			}

			return temp
		}
	}
}

mac := myarrayclass4()
val := mac[1, 2, 3, 4]

if (val == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myarrayclass5 extends Array
{
	__item[p1, p2, p3*]
	{
		get
		{
			temp := p1 + p2

			for n in p3
			{
				temp += n
			}

			return temp
		}
	}
}

mac := myarrayclass5()
val := mac[1, 2, 3, 4]

if (val == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

	
class myarrayclass6 extends Array
{
	; Special test which references a property defined in the base built-in Array type.
	; The reason this is special is that it must be properly cased by the parser to compile.
	doublecount
	{
		get
		{
			global
			return count * 2 ; Meant to refer to the base Array.Count property.
		}
	}
}

mac := myarrayclass6(1, 2, 3, 4)
val := mac.doublecount

if (val == 8)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class myinitclass
{
	p1 := 123
}

mic := myinitclass()
val := mic.p1

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
return123func()
{
	return 123
}

class myfuncinitclass
{
	p1 := return123func()
}

mic := myfuncinitclass()
val := mic.p1

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class mybaseclass
{
	x := 100

	basefunc()
	{
		global x := 123
	}
}

class mysubclass extends mybaseclass
{
	basefunc()
	{
		global
		super.basefunc()
		x++
	}
}

msc := mysubclass()
msc.basefunc()
val := msc.x

if (val == 124)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

msc := mysubclass()
msc.super.basefunc()
val := msc.x

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Test subclasses that derive from built in types and access the base properties before either class is fully initialized.
; This ensures the initialization chain of __Init() and __New() work properly.

class bigarr extends Array
{
	Capacity := 10000
}

mybigarr := bigarr(1, 2, 3)

If (mybigarr is Array)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (mybigarr is bigarr)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (mybigarr.Capacity == 10000)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (mybigarr[1] == 1 && mybigarr[2] == 2 && mybigarr[3] == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
class Mapi extends Map {
	CaseSense := false
	DerivedDefault := ""

	__New()
	{
		global
		DerivedDefault := Default
	}
}

cim := Mapi("a", 1, "B", 2)

if (cim is Map)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (cim is Mapi)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (cim["A"] == 1 && cim["b"] == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
class dupepropsbase
{
	a := 123
}

class dupepropssub extends dupepropsbase
{
	_a := 999

	a
	{
		get => _a
		set => _a := value
	}

	getglobala()
	{
		global
		return a
	}

	getthisa()
	{
		return this.a
	}

	getsupera()
	{
		return super.a
	}
}

classobj := dupepropssub()

if (classobj.a == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (classobj.super.a == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (classobj.getglobala() == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (classobj.getthisa() == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (classobj.getsupera() == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
