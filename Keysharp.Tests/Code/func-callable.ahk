x := 0
y := 0
z := 0

func_bound(a, b, c)
{
	global x := a
	global y := b
	global z := c
}

fo := func_bound

If (fo.Name = "func_bound")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (fo.IsBuiltIn == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

fo.Call(1, 2, 3)

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0
z := 0

fo(1, 2, 3)

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

class test1 {
	static Call() {
		global x := 1
	}
}

test1()

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


x := 0

class test2 {
	Call() {
		global x := 1
	}
}

t := test2()
t()

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"


call_callback(callback) {
	callback()
}

x := 0

call_callback(modify_x)

modify_x() {
	global x := 1
}

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

call_callback((*) => modify_x())

If (x == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

