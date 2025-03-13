a := 1
globalFatArrow := (*) => a := 2
globalFatArrow()

if (a == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := 1
globalFatArrowWithArg := (a) => a := 2
globalFatArrowWithArg(1)

if (a == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

a := 1
globalAnonFunc := (*) {
    a := 2
}
globalAnonFunc()

if (a == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

g()

g() {
    localClosure() {
        b := 2
    }

    b := 1
    localClosure()
    if (b == 2)
        FileAppend "pass", "*"
    else
        FileAppend "fail", "*"

    b := 1
    localAnonClosure := (*) => b := 2
    localAnonClosure()
    if (b == 2)
        FileAppend "pass", "*"
    else
        FileAppend "fail", "*"

    static localStaticClosure() {
        b := 2
    }

    b := 1
    localStaticClosure()
    if (b == 1)
        FileAppend "pass", "*"
    else
        FileAppend "fail", "*"

    closureLocalVar() {
        local b := 2
    }

    b := 1
    closureLocalVar()
    if (b == 1)
        FileAppend "pass", "*"
    else
        FileAppend "fail", "*"

    closureStaticVar() {
        static c := 2
    }

    static c := 1
    closureStaticVar()
    if (c == 1)
        FileAppend "pass", "*"
    else
        FileAppend "fail", "*"
}