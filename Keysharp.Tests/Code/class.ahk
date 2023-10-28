class myclass
{
	a := ""
	b :=
	c := "asdf"
	x := 123
	y := x
}

classobj := myclass.Call()

If (classobj.a == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

If (classobj.b == "")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (classobj.c == "asdf")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (classobj.x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (classobj.y == classobj.x)
	FileAppend, pass, *
else
	FileAppend, fail, *

classobj.x := 456

If (classobj.x == 456)
	FileAppend, pass, *
else
	FileAppend, fail, *

classobj2 := myclass.Call()

If (classobj2.x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

classobj3 := myclass()

If (classobj3.x == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
a := 1

If (classobj.a == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

classobj.a := 123

If (a == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *