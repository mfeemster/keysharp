#NoTrayIcon
#ErrorStdOut
#Warn All, StdOut
#Warn LocalSameAsGlobal, Off
#Include <assert>

ZeroParams() => 0

try {
    ZeroParams()
} catch {
    Assert(false, A_LineNumber)
}

Throws(() => ZeroParams(0), A_LineNumber)

OneParam(a) => 0

Throws(() => OneParam(), A_LineNumber)

try {
    OneParam(0)
} catch {
    Assert(false, A_LineNumber)
}

Throws(() => OneParam(0, 0), A_LineNumber)

try {
    OneParam(0, unset)
} catch {
    Assert(false, A_LineNumber)
}

VariadicOneParam(a*) => 0

try {
    VariadicOneParam()
} catch {
    Assert(false, A_LineNumber)
}

try {
    VariadicOneParam(0)
} catch {
    Assert(false, A_LineNumber)
}

try {
    VariadicOneParam(0, 0)
} catch {
    Assert(false, A_LineNumber)
}

class TestClass1 {
    __Item[a] {
        get => 0
        set => 0
    }
}

t1 := TestClass1()

Throws(() => (a := t1[]), A_LineNumber)

try {
    a := t1[1]
} catch {
    Assert(false, A_LineNumber)
}

Throws(() => (a := t1[1, 2]), A_LineNumber)

class TestClass2 {
    __Item[a*] {
        get => 0
        set => 0
    }
}

t2 := TestClass2()

try {
    a := t2[]
} catch {
    Assert(false, A_LineNumber)
}

try {
    a := t2[1]
} catch {
    Assert(false, A_LineNumber)
}

try {
    a := t2[1, 2]
} catch {
    Assert(false, A_LineNumber)
}

class TestClass3 {
    __Item[a, b*] {
        get => 0
        set => 0
    }
}

t3 := TestClass3()

Throws(() => (a := t3[]), A_LineNumber)

try {
    a := t3[1]
} catch {
    Assert(false, A_LineNumber)
}

try {
    a := t3[1, 2]
} catch {
    Assert(false, A_LineNumber)
}

; Generic delegate parameter-count boundaries.
SixteenParams(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16 := 2) => a1 + a16
f := SixteenParams
AssertEq(f(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), 3, A_LineNumber)
AssertEq(f.MaxParams, 16, A_LineNumber)

SeventeenParams(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17 := 2) => a1 + a17
f := SeventeenParams
AssertEq(f(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), 3, A_LineNumber)
AssertEq(f.MaxParams, 17, A_LineNumber)

; A variadic parameter may share its name with a local the compiler generates.
LineCount(line*) => line.Length
FrameCount(frame*) {
    return frame.Length
}
ScopeCount(scope*) {
    return %"scope"%.Length
}
class VariadicNames {
    Count(line*) => line.Length
    Values[items*] {
        get {
            KS_params_items := 10, KS_params_items_ := 20
            return KS_params_items + KS_params_items_ + items.Length
        }
        set => this.Last := value + items.Length
    }
    static __Item[items*] {
        set => this.StaticLast := value + items.Length
    }
}
ArrowLineCount() {
    inner := (line*) => line.Length
    return inner(1, 2, 3, 4)
}
AssertEq(LineCount(1, 2), 2, A_LineNumber)
AssertEq(FrameCount(1, 2, 3), 3, A_LineNumber)
AssertEq(ScopeCount(1), 1, A_LineNumber)
AssertEq(VariadicNames().Count(1, 2), 2, A_LineNumber)
AssertEq(ArrowLineCount(), 4, A_LineNumber)

PrefixCollision(KS_params_items, items*) => KS_params_items + items.Length
NestedVariadic(frame*) {
    KS_params_frame := 10
    inner(frame*) => KS_params_frame + frame.Length
    return frame.Length + inner(1, 2)
}
KeywordCount(params*) => params.Length
NamedTail(params_items*) => params_items.Length
MixedTail(MiXeD*) => MiXeD.Length
PrefixedTail(KS_items*) => KS_items.Length
#CSharp
public static object LegacyTail(params object[] KS_params_items) => (long)KS_params_items.Length;
#EndCSharp
AssertEq(PrefixCollision(10, 1, 2), 12, A_LineNumber)
AssertEq(NestedVariadic(1, 2, 3), 15, A_LineNumber)
AssertEq(KeywordCount(1, 2), 2, A_LineNumber)
AssertEq(FrameCount.Params[1].Name, "frame", A_LineNumber)
AssertEq(NamedTail.Params[1].Name, "params_items", A_LineNumber)
AssertEq(MixedTail.Params[1].Name, "MiXeD", A_LineNumber)
AssertEq(PrefixedTail(1, 2), 2, A_LineNumber)
AssertEq(PrefixedTail.Params[1].Name, "KS_items", A_LineNumber)
AssertEq(LegacyTail.Params[1].Name, "params_items", A_LineNumber)
AssertEq(LegacyTail(1, 2), 2, A_LineNumber)
variadic := VariadicNames()
AssertEq(variadic.Values[1, 2], 32, A_LineNumber)
variadic.Values[1, 2] := 7
AssertEq(variadic.Last, 9, A_LineNumber)
variadic.Values := 7
AssertEq(variadic.Last, 7, A_LineNumber)
VariadicNames[1, 2] := 7
AssertEq(VariadicNames.StaticLast, 9, A_LineNumber)
VariadicNames[] := 7
AssertEq(VariadicNames.StaticLast, 7, A_LineNumber)

FileAppend "pass", "*"
