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
