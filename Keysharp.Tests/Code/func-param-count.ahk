ZeroParams() => 0

try {
    ZeroParams()
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    ZeroParams(0)
    FileAppend "fail", "*"
} catch {
    FileAppend "pass", "*"
}

OneParam(a) => 0

try {
    OneParam()
    FileAppend "fail", "*"
} catch {
    FileAppend "pass", "*"
}

try {
    OneParam(0)
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    OneParam(0, 0)
    FileAppend "fail", "*"
} catch {
    FileAppend "pass", "*"
}

try {
    OneParam(0, unset)
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

VariadicOneParam(a*) => 0

try {
    VariadicOneParam()
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    VariadicOneParam(0)
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    VariadicOneParam(0, 0)
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

class TestClass1 {
    __Item[a] {
        get => 0
        set => 0
    }
}

t1 := TestClass1()

try {
    a := t1[]
    FileAppend "fail", "*"
} catch {
    FileAppend "pass", "*"
}

try {
    a := t1[1]
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    a := t1[1, 2]
    FileAppend "fail", "*"
} catch {
    FileAppend "pass", "*"
}

class TestClass2 {
    __Item[a*] {
        get => 0
        set => 0
    }
}

t2 := TestClass2()

try {
    a := t2[]
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    a := t2[1]
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    a := t2[1, 2]
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

class TestClass3 {
    __Item[a, b*] {
        get => 0
        set => 0
    }
}

t3 := TestClass3()

try {
    a := t3[]
    FileAppend "fail", "*"
} catch {
    FileAppend "pass", "*"
}

try {
    a := t3[1]
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}

try {
    a := t3[1, 2]
    FileAppend "pass", "*"
} catch {
    FileAppend "fail", "*"
}