#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon

#Requires AutoHotkey v2.1-alpha
#Include <assert>
#Import KS { CompileScript, Image, RealThread, StringBuffer, Task }

struct AB1 {
    #StructPack 1
    a : Int8
    b : Int32
}

struct AB2 {
    a : Int8
    b : Int32
}

; #StructPack 1 disables padding, so AB1 is 1 + 4 = 5 bytes.
ab := AB1()
Assert(!(ab.Size != 5), A_LineNumber)

; Without #StructPack, b is aligned to offset 4, so AB2 is 8 bytes.
Assert(!(AB2().Size != 8), A_LineNumber)

; ObjGetDataPtr / ObjGetDataSize mirror the struct's Ptr / Size.
Assert(!(ObjGetDataPtr(ab) != ab.Ptr), A_LineNumber)

Assert(!(ObjGetDataSize(ab) != 5), A_LineNumber)

; Only an externally backed instance can be rebound.
buf1 := Buffer(8, 0)
buf2 := Buffer(8, 0)
pt := AB1.At(buf1.Ptr)
ObjSetDataPtr(pt, buf2.Ptr)
Assert(!(pt.Ptr != buf2.Ptr), A_LineNumber)

try ObjSetDataPtr(AB1(), buf1.Ptr)
catch Error
    normalStructFailed := true

Assert(!(!IsSet(normalStructFailed)), A_LineNumber)

; Raw construction skips instance initialization and leaves external memory under the caller's ownership.
struct ExternalValue {
    Number : Int32 := 123
    __New() => ThrowUnexpectedInitialization()
    __Value {
        set => this.Number := value
    }
}
class ExternalObject {
    Initialized := 1
    __New() => ThrowUnexpectedInitialization()
    static Call() => ThrowUnexpectedInitialization()
    Method() => 42
}
ThrowUnexpectedInitialization() {
    throw Error("Unexpected initialization")
}
externalBuffer := Buffer(4, 0)
NumPut("Int", 42, externalBuffer)
external := ObjCreateWithDataPtr(ExternalValue, externalBuffer.Ptr)
Assert(external is ExternalValue, A_LineNumber)
AssertEq(ObjGetBase(external), ExternalValue.Prototype, A_LineNumber)
AssertEq(external.Number, 42, A_LineNumber)
AssertEq(ObjGetDataSize(external), 4, A_LineNumber)
external.Number := 91
AssertEq(NumGet(externalBuffer, "Int"), 91, A_LineNumber)
emptyExternal := ObjCreateWithDataPtr(ExternalValue, 0)
AssertEq(ObjGetDataPtr(emptyExternal), 0, A_LineNumber)
ObjSetDataPtr(emptyExternal, externalBuffer.Ptr)
AssertEq(emptyExternal.Number, 91, A_LineNumber)
ObjSetDataPtr(emptyExternal, buf2.Ptr)
emptyExternal.Number := 27
AssertEq(NumGet(buf2, "Int"), 27, A_LineNumber)
AssertEq(NumGet(externalBuffer, "Int"), 91, A_LineNumber)

rawObject := ObjCreateWithDataPtr(ExternalObject, 1)
Assert(rawObject is ExternalObject, A_LineNumber)
AssertEq(ObjGetBase(rawObject), ExternalObject.Prototype, A_LineNumber)
AssertEq(rawObject.Method(), 42, A_LineNumber)
Assert(!rawObject.HasOwnProp("Initialized"), A_LineNumber)
rawObject.Value := 9
AssertEq(rawObject.Value, 9, A_LineNumber)
AssertEq(ObjGetDataPtr(rawObject), 1, A_LineNumber)
AssertEq(ObjGetDataSize(rawObject), 0, A_LineNumber)
ObjSetDataPtr(rawObject, 0)
AssertEq(ObjGetDataPtr(rawObject), 0, A_LineNumber)
Throws(() => ObjSetBase(rawObject, ExternalObject.Prototype), A_LineNumber, ValueError)
AssertEq(ObjGetDataSize({}), 0, A_LineNumber)
AssertEq(ObjGetDataSize(ExternalValue.Prototype), 4, A_LineNumber)
originalStructPrototype := ExternalValue.Prototype
ExternalValue.Prototype := Object.Prototype
AssertEq(ObjGetBase(ExternalValue.At(externalBuffer.Ptr)), originalStructPrototype, A_LineNumber)
ExternalValue.Prototype := originalStructPrototype
Throws(() => ObjGetDataPtr({}), A_LineNumber, Error)
Throws(() => ObjSetDataPtr({}, 0), A_LineNumber, Error)
Throws(() => ObjCreateWithDataPtr({Prototype: Object.Prototype}, 0), A_LineNumber, Error)
Throws(() => ObjCreateWithDataPtr(ExternalObject, {}), A_LineNumber, TypeError)
originalExternalPrototype := ExternalObject.Prototype
ExternalObject.Prototype := RegExMatchInfo.Prototype
AssertEq(ObjGetBase(ObjCreateWithDataPtr(ExternalObject, 0)), originalExternalPrototype, A_LineNumber)
AssertEq(ObjGetBase(Class("DerivedExternal", ExternalObject).Prototype), originalExternalPrototype, A_LineNumber)
ExternalObject.Prototype := originalExternalPrototype
dynamicExternal := Class("ExternalDynamic")
dynamicExternalPrototype := dynamicExternal.Prototype
dynamicExternal.Prototype := RegExMatchInfo.Prototype
AssertEq(ObjGetBase(ObjCreateWithDataPtr(dynamicExternal, 0)), dynamicExternalPrototype, A_LineNumber)
fakeClass := Class()
fakeClass.Prototype := Object.Prototype
Throws(() => ObjCreateWithDataPtr(fakeClass, 0), A_LineNumber, Error)
for invalidClass in [Any, Primitive, Integer, String, Func, RegExMatchInfo, File, VarRef, Gui.Control, ScriptProcess, Task, Thread, RealThread]
    Throws(() => ObjCreateWithDataPtr(invalidClass, 0), A_LineNumber, Error)

rawArray := ObjCreateWithDataPtr(Array, 0)
Assert(rawArray is Array, A_LineNumber)
AssertEq(rawArray.Length, 0, A_LineNumber)
rawArray.Push(7)
AssertEq(rawArray[1], 7, A_LineNumber)
rawMap := ObjCreateWithDataPtr(Map, 0)
AssertEq(rawMap.Count, 0, A_LineNumber)
rawMap["Key"] := 8
AssertEq(rawMap["Key"], 8, A_LineNumber)
rawBuffer := ObjCreateWithDataPtr(Buffer, externalBuffer.Ptr)
AssertEq(rawBuffer.Size, 0, A_LineNumber)
AssertEq(rawBuffer.Ptr, 0, A_LineNumber)
AssertEq(ObjGetDataPtr(rawBuffer), externalBuffer.Ptr, A_LineNumber)
rawInput := ObjCreateWithDataPtr(InputHook, 0)
AssertEq(rawInput.InProgress, false, A_LineNumber)
Assert(ObjCreateWithDataPtr(Class, 0) is Class, A_LineNumber)
Assert(ObjCreateWithDataPtr(ArgumentError, 0) is ArgumentError, A_LineNumber)

for pointerBacked in [rawObject, rawArray, rawMap, rawBuffer, ObjCreateWithDataPtr(StringBuffer, 0), ObjCreateWithDataPtr(Image.Path, 0)]
    Throws(() => pointerBacked.Clone(), A_LineNumber, TypeError)
AssertEq({Value: 3}.Clone().Value, 3, A_LineNumber)
AssertEq([4].Clone()[1], 4, A_LineNumber)
AssertEq(Map("Key", 5).Clone()["Key"], 5, A_LineNumber)

; Address checks depend on the layout, and only raw construction permits a null address with fields.
for address in [0, 1, 65535]
    Throws(() => Int32.At(address), A_LineNumber, ValueError)
for address in [1, 65535]
    Throws(() => ObjCreateWithDataPtr(Int32, address), A_LineNumber, ValueError)
for address in [0, 1, 65535, -1] {
    AssertEq(ObjGetDataPtr(Struct.At(address)), address, A_LineNumber)
    AssertEq(ObjGetDataPtr(ObjCreateWithDataPtr(Struct, address)), address, A_LineNumber)
}
AssertEq(ObjGetDataPtr(Int32.At(-1)), -1, A_LineNumber)
AssertEq(ObjGetDataPtr(ObjCreateWithDataPtr(Int32, -1)), -1, A_LineNumber)
Throws(() => Struct.At.Call(Object, 0), A_LineNumber, Error)

; Rebinding preserves embedded objects already read from the original storage.
struct ExternalNested {
    Child : ExternalValue
}
for nested in [ExternalNested.At(externalBuffer.Ptr), ObjCreateWithDataPtr(ExternalNested, externalBuffer.Ptr)] {
    child := nested.Child
    ObjSetDataPtr(nested, buf2.Ptr)
    AssertEq(nested.Child, child, A_LineNumber)
    AssertEq(ObjGetDataPtr(child), externalBuffer.Ptr, A_LineNumber)
    ObjSetDataPtr(nested, 0)
    AssertEq(nested.Child, child, A_LineNumber)
    nested.Child := 47
    AssertEq(child.Number, 47, A_LineNumber)
}

; A union block overlaps its members at the first one's offset, so the union is as large as its largest member. A
; union block holding a property body is still an ordinary property named union. [v2.1-alpha.34+]
struct TaggedValue {
    Tag : UInt32
    union {
        AsInt : UInt32
        AsFloat : Float32, AsByte : UInt8
    }
    After : UInt8
}
tagged := TaggedValue()
AssertEq(TaggedValue.Prototype.Size, 12, A_LineNumber)
tagged.AsInt := 0x3F800000
AssertEq(tagged.AsFloat, 1.0, A_LineNumber)
AssertEq(tagged.AsByte, 0, A_LineNumber)
tagged.After := 9
AssertEq(NumGet(tagged, 8, "uchar"), 9, A_LineNumber)
struct InitializedUnion {
    Head : UInt8
    union {
        Wide : Int64
        Narrow : UInt16 := 7
    }
}
AssertEq(InitializedUnion.Prototype.Size, 16, A_LineNumber)
AssertEq(InitializedUnion().Wide, 7, A_LineNumber)
class UnionProperty {
    union {
        get => 42
    }
}
AssertEq(UnionProperty().union, 42, A_LineNumber)
; Neither an untyped declaration nor a nested union block is a typed property declaration.
UnionErrors(Body) {
    errors := ""
    for message in CompileScript("#NoTrayIcon`nstruct S {`nunion {`n" Body "`n}`n}`n").Errors
        errors .= message
    return errors
}
Assert(InStr(UnionErrors("X := 1"), "may contain only typed property declarations"), A_LineNumber)
Assert(InStr(UnionErrors("union {`nA : Int32`n}"), "may contain only typed property declarations"), A_LineNumber)

FileAppend "pass", "*"
