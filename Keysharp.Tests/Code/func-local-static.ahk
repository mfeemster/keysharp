#ErrorStdOut
#Warn All, StdOut
#Warn LocalSameAsGlobal, Off
#NoTrayIcon
#Include <assert>

initfunc(a, b)
{
	return a + b
}

func()
{
	x := 1
	y := 2
	static Z := 3

	AssertEq(x, 1, A_LineNumber)

	AssertEq(y, 2, A_LineNumber)
	
	AssertEq(z, 3, A_LineNumber)
}

func()

x := 1
y := 2
z := 3

func2()
{
	static
	local x := 22, y := initfunc(10, 20)

	AssertEq(x, 22, A_LineNumber)
	
	AssertEq(y, 30, A_LineNumber)
}

func2()

AssertEq(x, 1, A_LineNumber)

AssertEq(y, 2, A_LineNumber)

AssertEq(z, 3, A_LineNumber)

x := 1
y := 2
z := 3

func3()
{
	static x := 111
	y := 22
	static z := 333, ZZ := initfunc(1, 2)

	AssertEq(x, 111, A_LineNumber)

	AssertEq(y, 22, A_LineNumber)

	AssertEq(z, 333, A_LineNumber)
	
	AssertEq(zz, 3, A_LineNumber)
}

func3()

AssertEq(x, 1, A_LineNumber)

AssertEq(y, 2, A_LineNumber)

AssertEq(z, 3, A_LineNumber)

x := 1
y := 2
z := 3

func4()
{
	static
	local x := 11, y := 22, z := 33, zz := initfunc(5, 6)

	AssertEq(x, 11, A_LineNumber)

	AssertEq(y, 22, A_LineNumber)

	AssertEq(z, 33, A_LineNumber)
	
	AssertEq(zz, 11, A_LineNumber)

	x := 111, y := 222, z := 333, zz := initfunc(7, 8)

	AssertEq(x, 111, A_LineNumber)

	AssertEq(y, 222, A_LineNumber)

	AssertEq(z, 333, A_LineNumber)
	
	AssertEq(zz, 15, A_LineNumber)
}

func4()

AssertEq(x, 1, A_LineNumber)

AssertEq(y, 2, A_LineNumber)

AssertEq(z, 3, A_LineNumber)

reffunc1(&a)
{
	a := 100
}

func5()
{
	static x := 111
	reffunc1(&x := 0)
	
	AssertEq(x, 100, A_LineNumber)
}

func5()

class class1 {
	func6() {
		static i := 2
		return ++i
	}
}

func6() {
	static i := 2
	return ++i
}

t1 := class1()
x := t1.func6()
AssertEq(x, 3, A_LineNumber)

x := unset
x := t1.func6()
AssertEq(x, 4, A_LineNumber)

t2 := class1()
x := unset
x := t2.func6()
AssertEq(x, 5, A_LineNumber)

x := unset
x := t2.func6()
AssertEq(x, 6, A_LineNumber)

x := unset
x := func6()
AssertEq(x, 3, A_LineNumber)

; A static initializer that throws runs again on the next call.
attempts := 0

StaticRetry() {
	static value := Initial()
	return value
}

Initial() {
	global attempts
	if (++attempts = 1)
		throw Error("first attempt")
	return "second"
}

try StaticRetry()
AssertEq(StaticRetry(), "second", A_LineNumber)

; A declaration's initializers run together: when a later one throws, the whole declaration runs again next time, as
; AutoHotkey removes a declaration only once it has completed.
lineRuns := 0

StaticLineRetry() {
	static first := CountLineRun(), second := Initial()
	return first "," second
}

CountLineRun() {
	global lineRuns
	return ++lineRuns
}

attempts := 0
try StaticLineRetry()
AssertEq(StaticLineRetry(), "2,second", A_LineNumber)
AssertEq(StaticLineRetry(), "2,second", A_LineNumber)

; Each declaration of a static runs once, as AutoHotkey permits declaring one again.
StaticRedeclared() {
	static log := AppendLog("a")
	static log := AppendLog("b")
	return log
}

AppendLog(text) {
	static all := ""
	return all .= text
}

AssertEq(StaticRedeclared(), "ab", A_LineNumber)
AssertEq(StaticRedeclared(), "ab", A_LineNumber)

; An initializer calling its own function runs again in that call, as its declaration has not completed yet.
StaticRecursive(depth) {
	static value := (depth < 2 ? StaticRecursive(depth + 1) : 0) + 1
	return value
}

AssertEq(StaticRecursive(0), 3, A_LineNumber)
AssertEq(StaticRecursive(0), 3, A_LineNumber)

StaticFromParam(n) {
	static first := n * 10
	return first
}

AssertEq(StaticFromParam(1), 10, A_LineNumber)
AssertEq(StaticFromParam(2), 10, A_LineNumber)

Inner_1() {
	static first := [1]
	return first[1]
}
FlagScope() {
	Inner() {
		static second := [2]
		return second[1]
	}
	return Inner()
}
AssertEq(Inner_1(), 1, A_LineNumber)
AssertEq(FlagScope(), 2, A_LineNumber)

; A method get_x and the getter of a property x keep their own statics.
class StaticPair {
	get_x() {
		static a := [1]
		return a[1]
	}

	x {
		get {
			static a := [2]
			return a[1]
		}
	}
}

pair := StaticPair()
AssertEq(pair.get_x(), 1, A_LineNumber)
AssertEq(pair.x, 2, A_LineNumber)

; AutoHotkey assigns every initializer of a static declaration, constants included, when execution first reaches it, and
; again next time until the declaration has run to its end.
Fresh() => "fresh"
Redeclared() {
	static log := Fresh()
	static log := 5
	return log
}

AssertEq(Redeclared(), 5, A_LineNumber)
AssertEq(Redeclared(), 5, A_LineNumber)

Thrower() {
	static calls := 0
	if ++calls = 1
		throw Error("first")
	return calls
}

Retry() {
	try {
		static a := 1, b := Thrower()
	}
	a += 10
	return a
}

AssertEq(Retry(), 11, A_LineNumber)
AssertEq(Retry(), 11, A_LineNumber)
AssertEq(Retry(), 21, A_LineNumber)

EarlyRead() {
	r := IsSet(x)
	static x := 5
	return r
}

AssertEq(EarlyRead(), 0, A_LineNumber)
AssertEq(EarlyRead(), 1, A_LineNumber)

DefaultSeesStatic(value := IsSet(saved)) {
	static saved := 5
	return value
}
AssertEq(DefaultSeesStatic(), 0, A_LineNumber)
AssertEq(DefaultSeesStatic(), 1, A_LineNumber)

FileAppend "pass", "*"
