class myclass
{
	a := ""
	b :=
	c := "asdf"
	d := c
	x := 123
	y := this.x
}

classobj := myclass.Call()

If (classobj.a == "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (classobj.b == "asdf")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (classobj.d == "asdf")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (classobj.x == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (classobj.y == classobj.x)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

classobj.x := 456

If (classobj.x == 456)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

classobj2 := myclass.Call()

If (classobj2.x == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

classobj3 := myclass()

If (classobj3.x == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
a := 1

If (classobj.a == "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

classobj.a := 123

If (a == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; Test class members that are initialized using the value of other members.
; Purposely declare them in reverse alphabetical order to make sure they are
; created in the exact order specified.
class membersrefeachother
{
	zz := 8080
	ii := this.zz * 2
}

classobj := membersrefeachother()

if (classobj.zz == 8080)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (classobj.ii == 16160)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
