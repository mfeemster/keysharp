class testclass
{
	a := 0
	b := 0
}

o1 := testclass()
testfunc(o1)
o1 := { a : "" }
testfunc(o1)

o1 := testclass()
o1.DefineProp("a", {
		get: (this) => 123,
		set: (this, v) => this.b := v
	})

o1.a := 100
val := o1.a

If (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (o1.b == 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1.DefineProp("a", {
		set: (this, v) => this.b := v
	})

o1.a := 200

If (o1.a == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (o1.b == 200)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

class extestclass extends testclass
{
}

eo1 := extestclass()
eo1.DefineProp("a", {
		set: (this, v) => this.b := v
	})

eo1.a := 200

If (eo1.a == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (eo1.b == 200)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := 100
o1 := testclass()
o1.DefineProp("a", {
		Call: (this, &v) => this.b := v := 999
	})
o1.a(&val)

If (val == 999)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1 := testclass()
o1.DefineProp("c", {
		value: 123
	})
b := true

try
{
	val := o1.GetOwnPropDesc("a")

	if (val == 0)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	o1.a := 999
	if (o1.GetOwnPropDesc("a") == o1.a && o1.a == 999)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	val := o1.GetOwnPropDesc("c")

	if (val.Count == 1)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	if (ObjHasOwnProp(o1, "a"))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	if (o1.HasOwnProp("c"))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}
catch
{
	b := false
}

if (b)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1 := { a : 1, c : 123 }
b := true

try
{
	val := o1.GetOwnPropDesc("a")

	if (val.Count == 1)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	val := o1.GetOwnPropDesc("c")

	if (val.Count == 1)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	if (ObjHasOwnProp(o1, "a"))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	if (o1.HasOwnProp("c"))
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}
catch
{
	b := false
}

if (b)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
class extclass2 extends testclass
{
	c := 0
}

o1 := testclass()

if (ObjOwnPropCount(o1) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (ObjHasOwnProp(o1, "a") && o1.HasOwnProp("a"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1 := extclass2()

if (ObjOwnPropCount(o1) == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1.DefineProp("d", {
		call : () => 123
	})

if (ObjOwnPropCount(o1) == 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (ObjHasOwnProp(o1, "a") && o1.HasOwnProp("b") && o1.HasOwnProp("c") && o1.HasOwnProp("d"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1 := { one : 1}

if (o1.OwnPropCount(o1) == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (ObjHasOwnProp(o1, "One") && !o1.HasOwnProp("two"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1.DefineProp("d", {
		call : () => 123
	})
	
if (o1.OwnPropCount(o1) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (ObjHasOwnProp(o1, "one") && o1.HasOwnProp("d"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1 := [1, 2, 3]

if (o1.OwnPropCount(o1) == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (!ObjHasOwnProp(o1, "capacity") && !o1.HasOwnProp("Count") && !o1.HasOwnProp("Length"))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1 := testclass()
o1.DefineProp("c", {
		get: (this) => 123,
		set: (this, v) => this.b := v
	})
o1.a := 100
o1.b := 200
b := false
i := 0

For Name, Value in o1.OwnProps()
{
	if (name == "a" && value == 100)
		b := true
	else if (name == "b" && value == 200)
		b := true
	else if (name == "c" && value == 123)
		b := true
	else
		b := false

	i++
}

If (b && i == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

o1.a := 100
o1.b := 200
b := false
i := 0
op := o1.OwnProps(true)

For Name,Value in op
{
	if (name == "a" && value == 100)
		b := true
	else if (name == "b" && value == 200)
		b := true
	else if (name == "c" && value == 123)
		b := true
	else
		b := false

	i++
}

If (b && i == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

i := 0
b := false
o1.a := 0
m := { one : 1, two : 2, three : (this) => o1.a := 123 }

for name in m.OwnProps() {
	if (name == "one")
		b := true
	else if (name == "two")
		b := true
	else if (name == "three")
		b := true
	else
		b := false

	i++
}

If (b && i == 3 && o1.a == 0) ; Ensure the last prop didn't get called.
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
testfunc(testclassobj)
{
	testclassobj.DefineProp("prop", {
		get: (this) => 123
	})

	val := testclassobj.prop

	If (val == 123)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.DefineProp("prop", {
		get: (this) => 123,
		set: (this, v) => this.a := v
	})

	testclassobj.prop := 100
	val := testclassobj.a

	If (val == 100)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
	
	testclassobj.DefineProp("prop", {
		get: (this) => 123,
		set: (this, v) => this.a := v,
		call: (this, p*) => this.a := p.Length
	})

	testclassobj.prop := 200
	val := testclassobj.a

	If (val == 200)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop()
	val := testclassobj.a

	If (val == 0)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.DefineProp("prop", {
		value: 123
	})

	val := testclassobj.prop

	If (val == 123)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop := "123"

	val := testclassobj.prop

	If (val == "123")
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	b := false

	try {
		testclassobj.prop(1, 2)
	}
	catch Error as e {
		b := true
	}

	If (b == true)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop := (this, p*) => this.a := p.Length
	testclassobj.prop()

	If (testclassobj.a == 0)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop(1, 2)

	If (testclassobj.a == 2)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop(1, 2, 3)

	If (testclassobj.a == 3)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.DefineProp("prop", {
		get: (*) => ((this, p*) => this.a := p.Length)
	})

	testclassobj.prop()

	If (testclassobj.a == 0)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop(1)

	If (testclassobj.a == 1)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop(1, 2)

	If (testclassobj.a == 2)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"

	testclassobj.prop(1, 2, 3)

	If (testclassobj.a == 3)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}