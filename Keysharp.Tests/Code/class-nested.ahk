#NoTrayIcon
#Include <assert>

class c1 {
    class c2 {
        test() {
            global a := 5
        }
        static test() {
            global b := 6
        }
        statictest() {
            global c := 7
        }
        static statictest() {
            global d := 8
        }
    }
    test() {
        global a := 1
    }
    static test() {
        global b := 2
    }
    statictest() {
        global c := 3
    }
    static statictest() {
        global d := 4
    }
}

a := 0, b := 0, c := 0, d := 0

c1().test()
Assert(a == 1 && b == 0 && c == 0 && d == 0, A_LineNumber)

c1.test()
Assert(a == 1 && b == 2 && c == 0 && d == 0, A_LineNumber)
c1().statictest()
Assert(a == 1 && b == 2 && c == 3 && d == 0, A_LineNumber)
c1.statictest()
Assert(a == 1 && b == 2 && c == 3 && d == 4, A_LineNumber)

a := 0, b := 0, c := 0, d := 0

c1.c2().test()
Assert(a == 5 && b == 0 && c == 0 && d == 0, A_LineNumber)
c1.c2.test()
Assert(a == 5 && b == 6 && c == 0 && d == 0, A_LineNumber)

c1.c2().statictest()
Assert(a == 5 && b == 6 && c == 7 && d == 0, A_LineNumber)

c1.c2.statictest()
Assert(a == 5 && b == 6 && c == 7 && d == 8, A_LineNumber)

; Type() names an instance by the __Class its class declares, so a nested class reports its full
; dotted path whether or not the instance carries own properties, and reassigning a Prototype's
; __Class renames every instance of it.
class outer {
    class empty {
    }
    class field {
        x := 1
    }
    class deep {
        class deeper {
        }
    }
}

class subofnested extends c1.c2 {
}

AssertEq(Type(outer.empty()), "outer.empty", A_LineNumber)
AssertEq(outer.empty().__Class, "outer.empty", A_LineNumber)
AssertEq(Type(outer.field()), "outer.field", A_LineNumber)
AssertEq(Type(outer.deep.deeper()), "outer.deep.deeper", A_LineNumber)
AssertEq(Type(c1.c2()), "c1.c2", A_LineNumber)
AssertEq(Type(subofnested()), "subofnested", A_LineNumber)
AssertEq(Type(outer.empty), "Class", A_LineNumber)
AssertEq(Type(outer.empty.Prototype), "Prototype", A_LineNumber)

renamed := outer.empty()
outer.empty.Prototype.__Class := "Override"
AssertEq(Type(renamed), "Override", A_LineNumber)
AssertEq(Type(outer.empty()), "Override", A_LineNumber)

FileAppend "pass", "*"
