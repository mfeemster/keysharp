﻿a := 0
Object.Prototype.DefineProp("protoCall", {call:(*) {
    global a := 1
    }
})
({}.protoCall())

if (a == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

b := 0
Object.Prototype.DefineProp("protoGet", {get:(*) => 1})
b := {}.protoGet

if (b == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

b := 0
Object.Prototype.DefineProp("protoValue", {value:1})
b := {}.protoValue

if (b == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

class Test {
    HasOwnProp(*) => 1 
    protoGet {
        get => 2
    }
}

class TestExtend extends Test {
    protoValue := 2
}

o := TestExtend()

b := 0
b := o.HasOwnProp("test")
if (b == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

o.base := Object.Prototype
b := o.HasOwnProp("test")
if (b == 0)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if (Type(o) = "Object")
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

a := 0
o.protoCall()
if (a == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

b := 0
b := o.protoGet
if (b == 1)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

b := 0
b := o.protoValue
if (b == 2)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

c := Class()

if (c.Base == Class.Prototype)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if (c.Base.Base == Object.Prototype)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if (c.Base.Base.Base == Any.Prototype)
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

t := Test()

if (t.Base.__Class == "Test")
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"


if Object.Base == Any
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if Any.Base == Class.Prototype
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"

if Class.Base == Object
    FileAppend "pass", "*"
else
    FileAppend "fail", "*"