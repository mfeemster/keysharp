class myclass
{
	static a :=
	static b := ""
	static c := "asdf"
	static x := 123
	static y := x
}

classobj := myclass.Call()

If (myclass.a == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

If (myclass.b == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

If (myclass.c == "asdf")
	FileAppend, pass, *
else
	FileAppend, fail, *

If (myclass.x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (myclass.y == myclass.x)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
myclass.x := 456

If (myclass.x == 456)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (myclass.y == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
classobj2 := myclass.Call()

If (myclass.x == 456)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
classobj3 := myclass()

If (classobj3.x == 456)
	FileAppend, pass, *
else
	FileAppend, fail, *

a := 1

If (myclass.a == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

myclass.a := 123

If (a == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *