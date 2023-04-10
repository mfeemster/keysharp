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
	a := 321
	static b := 654
	c := 999
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
	FileAppend, pass, *
else
	FileAppend, fail, *

If (testsubclassobj is testclass)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (testsubclassobj is testsubclass)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testclassobj.a

If (val == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testsubclassobj.a

If (val == 321)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testclassobj.b

If (val == 456)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testsubclass.b

If (val == 654)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testclass.c

If (val == 888)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testsubclassobj.c

If (val == 999)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testclass.d

If (val == 1000)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testsubclass.d

If (val == 2000)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
testsubclassobj.setbasea()

val := testsubclassobj.getbasea()

If (val == 500)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testsubclassobj.a

If (val == 321)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
testsubclassobj.super.a := 777

val := testsubclassobj.getbasea()

If (val == 777)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := testsubclassobj.a

If (val == 321)
	FileAppend, pass, *
else
	FileAppend, fail, *

classname := testclassobj.__Class

If (classname == "testclass")
	FileAppend, pass, *
else
	FileAppend, fail, *

classname := testsubclassobj.__Class

If (classname == "testsubclass")
	FileAppend, pass, *
else
	FileAppend, fail, *

testsubclassobj.a := ""
testsubclassobj.super.a := ""

testsubclassobj.subcasesensitivefunc()

if (testsubclassobj.super.a == 999)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (testsubclassobj.a == 1212)
	FileAppend, pass, *
else
	FileAppend, fail, *

testclass.c := ""
testsubclass.subcasesensitivefuncstatic()

if (testclass.c == 3131)
	FileAppend, pass, *
else
	FileAppend, fail, *

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
	FileAppend, pass, *
else
	FileAppend, fail, *

If (classname is MyArray)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := classname[100]

If (val == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

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
	FileAppend, pass, *
else
	FileAppend, fail, *

If (classname is MyMap)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := classname[100]

If (val == 321)
	FileAppend, pass, *
else
	FileAppend, fail, *

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
	FileAppend, pass, *
else
	FileAppend, fail, *

obj := subarr2()

If (obj is subarr2 && obj is subarr1 && obj is Array)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := obj[999]

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

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
	FileAppend, pass, *
else
	FileAppend, fail, *

val := obj[999]

If (val == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

testclass.c := 101
myfunc := FuncObj("basecasesensitivefuncstatic", testsubclassobj)

myfunc()

if (testclass.c == 3131)
	FileAppend, pass, *
else
	FileAppend, fail, *

testclass.c := 101
myfunc := FuncObj("subcasesensitivefuncstatic", testsubclassobj)

myfunc()

if (testclass.c == 3131)
	FileAppend, pass, *
else
	FileAppend, fail, *

testsubclassobj.a := 0
myfunc := FuncObj("basecasesensitivefunc", testsubclassobj)
myfunc()

if (testsubclassobj.a == 1212)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
testsubclassobj.a := 0
myfunc := FuncObj("SubCaseSensitiveFunc", testsubclassobj)
myfunc()

if (testsubclassobj.a == 1212)
	FileAppend, pass, *
else
	FileAppend, fail, *