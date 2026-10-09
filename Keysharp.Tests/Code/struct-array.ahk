#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon

#Requires AutoHotkey v2.1-alpha
#Include <assert>

struct X {
    y : Int32[10]
}

struct XX {
    items : X[2]
}

; A struct field of array type contributes its full array size, and exposes Length/element access.
z := X()
Assert(!(z.Size != 40 || z.y.Size != 40 || z.y.Length != 10), A_LineNumber)

z.y[1] := 11
z.y[10] := 99
Assert(!(z.y[1] != 11 || z.y[10] != 99), A_LineNumber)

Assert(!(!(z.y is Int32[10]) || (z.y is Int32[100])), A_LineNumber)

; Nested array of structs: XX has 2 X's, each 40 bytes => 80.
zz := XX()
Assert(!(zz.Size != 80), A_LineNumber)

; Class-level indexing of a struct class yields a fixed-size structured-array class.
arrCls := Int32[10]
inst := arrCls()

Assert(!(inst.Size != 40), A_LineNumber)

Assert(!(inst.Length != 10), A_LineNumber)

inst[1] := 100
inst[10] := 200
inst[-1] := 250   ; negative index counts from the end, so -1 is element 10

Assert(!(inst[1] != 100), A_LineNumber)

Assert(!(inst[10] != 250 || inst[-1] != 250), A_LineNumber)

; The array class has stable identity (cached), and differs by element type and length.
Assert(!(!(inst is Int32[10])), A_LineNumber)

Assert(!(inst is Int32[5]), A_LineNumber)

Assert(!(inst is Float32[10]), A_LineNumber)

; Out-of-bounds indices throw IndexError.
threw := false
try
    _ := inst[11]
catch IndexError
    threw := true
Assert(!(!threw), A_LineNumber)

threw := false
try
    _ := inst[0]
catch IndexError
    threw := true
Assert(!(!threw), A_LineNumber)

; Structured-array arguments pass their storage address, regardless of its size.
arrayAddressCb := CallbackCreate(address => address, "Fast", [IntPtr, IntPtr])
for arrayType in [Int8[2], Int32[2], Int32[10]] {
    nativeArray := arrayType()
    nativeArray[1] := 7
    AssertEq(DllCall(arrayAddressCb, arrayType, nativeArray, "Ptr"), nativeArray.Ptr, A_LineNumber)
}

Throws(() => DllCall(arrayAddressCb, Int32[2]), A_LineNumber, ValueError)
CallbackFree(arrayAddressCb)

writeArrayCb := CallbackCreate(address => NumPut("Int", 42, address), "Fast", [IntPtr, "void"])
DllCall(writeArrayCb, Int32[10], &outputArray, "void")
Assert(outputArray is Int32[10], A_LineNumber)
AssertEq(outputArray[1], 42, A_LineNumber)
CallbackFree(writeArrayCb)

; An array of exactly the pointer's target class decays to the address of its first element, both in a typed
; property, which then reads back as the same array, and in a DllCall argument. [v2.1-alpha.34+]
struct BytePointer {
    Target : UInt8.Ptr
}
bytes := UInt8[4]()
pointer := BytePointer()
pointer.Target := bytes
Assert(pointer.Target == bytes, A_LineNumber)
AssertEq(NumGet(pointer, "ptr"), bytes.Ptr, A_LineNumber)
Throws(() => pointer.Target := Int8[4](), A_LineNumber, TypeError)
pointer.Target := unset
AssertEq(pointer.Target ?? "none", "none", A_LineNumber)
pointer.Target := bytes
fillCb := CallbackCreate(address => NumPut("UChar", 65, address), "Fast", [IntPtr, "void"])
DllCall(fillCb, UInt8.Ptr, bytes, "void")
AssertEq(bytes[1], 65, A_LineNumber)
CallbackFree(fillCb)

FileAppend "pass", "*"
