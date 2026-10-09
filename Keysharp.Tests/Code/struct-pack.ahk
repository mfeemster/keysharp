#NoTrayIcon

#Requires AutoHotkey v2.1-alpha
#Include <assert>
#Import KS { CompileScript }

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

; Since alpha.27, only a boxed pointer created by Struct.At can be rebound.
buf1 := Buffer(8, 0)
buf2 := Buffer(8, 0)
pt := AB1.At(buf1.Ptr)
ObjSetDataPtr(pt, buf2.Ptr)
Assert(!(pt.Ptr != buf2.Ptr), A_LineNumber)

try ObjSetDataPtr(AB1(), buf1.Ptr)
catch Error
    normalStructFailed := true

Assert(!(!IsSet(normalStructFailed)), A_LineNumber)

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
