class testclass
{
	a := 0
	b := 0
}

o1 := testclass()
testfunc(o1) ; Class with declared members a, b.
o1 := { a : "" }
testfunc(o1) ; Class with dynamic member a.

o1 := testclass()
o1.DefineProp("a", { ; Define a dynamic property over a declared one of the same name.
		get: (this) => 123,
		set: (this, v) => this.b := v
	})

o1.a := 100
val := o1.a

If (val == 123)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (o1.b == 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1.DefineProp("a", { ; Redefine a dynamic property over previously declared dynamic property on a class with a declared property of the same name.
		set: (this, v) => this.b := v
	})

o1.a := 200

If (o1.a == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (o1.b == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

class extestclass extends testclass
{
}

eo1 := extestclass()
eo1.DefineProp("a", { ; Define dynamic property in a derived class where the base class has a declared property of the same name.
		set: (this, v) => this.b := v
	})

eo1.a := 200

If (eo1.a == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (eo1.b == 200)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := 100
o1 := testclass()
o1.DefineProp("a", { ; Define a dynamic Call property that takes a reference parameter, over a declared one of the same name.
		Call: (this, &v) => this.b := v := 999
	})

o1.a(&val)

If (val == 999)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1 := testclass()
o1.DefineProp("c", {
		value: 123
	})

o1.DefineProp("getsetprop", {
		get: (this) => 456,
		set: (this) => this.b := 789
	})
	
b := true

try
{
	val := o1.GetOwnPropDesc("a") ; Get a value property object for a declared property.

	if (val.Value == 0)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	o1.a := 999

	if (o1.GetOwnPropDesc("a").Value == o1.a && o1.a == 999)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	val := o1.GetOwnPropDesc("c") ; Get a value property object for a dynamic property.

	if (val.Value == 123)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	val := o1.GetOwnPropDesc("getsetprop") ; Get a get property object for a dynamic property. Note that get must be called like a method().

	if (val.get() == 456)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	val := o1.GetOwnPropDesc("getsetprop") ; Get a get property object for a dynamic property. Note that get must be called like a method().
	val.set()

	if (o1.b == 0) ; val was considered "this" inside of set(), so it never set o1.b.
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	if (ObjHasOwnProp(o1, "a")) ; 
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	if (o1.HasOwnProp("c"))
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}
catch
{
	b := false
}

if (b)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1 := { a : 1, c : 123 }
b := true

try
{
	val := o1.GetOwnPropDesc("a") ; Get a value property object for a declared property in an object literal.

	if (val.Value == 1)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	val := o1.GetOwnPropDesc("c")

	if (val.Value == 123)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	if (ObjHasOwnProp(o1, "a"))
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	if (o1.HasOwnProp("c"))
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}
catch
{
	b := false
}

if (b)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
o1 := testclass()

if (ObjOwnPropCount(o1) == 3) ; Count all declared properties.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (ObjHasOwnProp(o1, "a") && o1.HasOwnProp("a")) ; Call both forms of *HasOwnProp().
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
class extclass2 extends testclass
{
	c := 0
}

o1 := extclass2()

if (ObjOwnPropCount(o1) == 3) ; Count all declared properties in base and derived class.
	FileAppend "pass", "*"
if (ObjOwnPropCount(o1) == 5) ; Count all declared properties in base and derived class.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1.DefineProp("d", {
		call : () => 123
	})

if (ObjOwnPropCount(o1) == 6) ; Count all declared and dynamic properties in base and derived class.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (ObjHasOwnProp(o1, "a") && o1.HasOwnProp("b") && o1.HasOwnProp("c") && o1.HasOwnProp("d"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
	FileAppend "fail", "*"

o1 := { one : 1}

if (o1.OwnPropCount(o1) == 1) ; Count all declared properties in object literal.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (ObjHasOwnProp(o1, "One") && !o1.HasOwnProp("two"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1.DefineProp("d", {
		call : () => 123
	})
	
if (o1.OwnPropCount(o1) == 2) ; Count all declared and dynamic properties in object literal.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (ObjHasOwnProp(o1, "one") && o1.HasOwnProp("d"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1 := [1, 2, 3]

if (o1.OwnPropCount(o1) == 0 && ObjOwnPropCount(o1) == 0) ; Declared properties for built in types are not counted.
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (!ObjHasOwnProp(o1, "capacity") && !o1.HasOwnProp("Count") && !o1.HasOwnProp("Length"))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1 := testclass()
o1.DefineProp("c", { ; Dynamically property with getter and setter.
		get: (this) => 123,
		set: (this, v) => this.b := v
	})
o1.a := 100
o1.b := 200
b := false
i := 0

For Name, Value in o1.OwnProps() ; Enumerator inline with a for loop. Retrieve values is implicitly true because of two loop variables.
{
	if (name == "a" && value == 100)
		b := true
	else if (name == "b" && value == 200)
		b := true
	else if (name == "c" && value == 123)
		b := true
	else if (name == "super")
		b := true
	else
		b := false

	i++
}

If (b && i == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

o1.a := 100
o1.b := 200
b := false
i := 0
op := o1.OwnProps(true) ; Retrieve value must be specified.

For Name,Value in op ; Enumerator variable with a for loop.
{
	if (name == "a" && value == 100)
		b := true
	else if (name == "b" && value == 200)
		b := true
	else if (name == "c" && value == 123)
		b := true
	else if (name == "super")
		b := true
	else
		b := false

	i++
}

If (b && i == 4)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

i := 0
b := false
o1.a := 0
m := { one : 1, two : 2, three : (this) => o1.a := 123 }

for name in m.OwnProps() { ; Enumerator inline with a for loop, names only.
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
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

testfunc(testclassobj)
{
	testclassobj.DefineProp("prop", { ; Dynamically defined property with getter.
		get: (this) => 123
	})

	val := testclassobj.prop
	
	If (val == 123)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.DefineProp("prop", { ; Overwrite previous with dynamically defined property with getter and setter.
		get: (this) => 123,
		set: (this, v) => this.a := v
	})

	testclassobj.prop := 100
	val := testclassobj.a

	If (val == 100)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	testclassobj.DefineProp("prop", { ; Overwrite previous with dynamically defined property with getter, setter and call.
		get: (this) => 123,
		set: (this, v) => this.a := v,
		call: (this, p*) => this.a := p.Length
	})

	testclassobj.prop := 200
	val := testclassobj.a

	If (val == 200)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop()
	val := testclassobj.a

	If (val == 0)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.DefineProp("prop", { ; Overwrite previous with dynamically defined call.
		value: 123,
		call: (this, p*) => this.a := p.Length
	})

	val := testclassobj.prop

	If (val == 123)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop := "123"

	val := testclassobj.prop

	If (val == "123")
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
	
	testclassobj.prop(1, 2)

	If (testclassobj.a == 2)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop := (this, p*) => this.a := p.Length ; Overwrite previous with dynamically defined value property with direct fat arrow function assignment.
	testclassobj.prop()

	If (testclassobj.a == 0)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop(1, 2)

	If (testclassobj.a == 2)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop(1, 2, 3)

	If (testclassobj.a == 3)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.DefineProp("prop", { ; Overwrite previous with dynamically defined get property which returns another fat arrow function.
		get: (*) => ((this, p*) => this.a := p.Length)
	})

	testclassobj.prop() ; Retrieve value from get, which will be a FuncObj(), then call it using ().

	If (testclassobj.a == 0)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop(1)

	If (testclassobj.a == 1)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop(1, 2)

	If (testclassobj.a == 2)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"

	testclassobj.prop(1, 2, 3)

	If (testclassobj.a == 3)
		FileAppend "pass", "*"
	else
		FileAppend "fail", "*"
}
