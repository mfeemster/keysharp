class testclass
{
	a := 123
	b := 456
	static c := 888
	static d := 1000
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

	getbasea()
	{
		return super.a
	}
}

testclassobj := testclass()
testsubclassobj := testsubclass()

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
val := obj[999]

If (val == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *