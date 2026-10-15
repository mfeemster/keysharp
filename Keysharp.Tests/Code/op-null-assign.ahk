#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon
#Include <assert>

x := 123
y := x ?? ""

Assert(y = 123, A_LineNumber)

nafunc(p?)
{
	AssertEq((p ?? 456), 456, A_LineNumber)
}

nafunc(unset)

z :=
y := z ?? 456

Assert(y = 456, A_LineNumber)

tot := 0

nafunc2(a, b, c)
{
	global tot
	tot += a(1)
	tot += b()
	tot += c(3)
}

nafunc2((o) => o ?? 11, (o?) => o ?? 22, (o?) => o ?? 33)

AssertEq(tot, 26, A_LineNumber)

x := 123
yy := unset
m := { one : x ?? 456,  two : yy ?? 789}

Assert(m.one = 123, A_LineNumber)

Assert(m.two = 789, A_LineNumber)

x := 123

AssertEq((x ?? 456), 123, A_LineNumber)

x := unset
x ??= Array()

Assert(x is Array, A_LineNumber)

; Optional chaining tests
optObj := unset
optVal := optObj?.prop
Assert(!IsSet(optVal), A_LineNumber)

optObj := { prop: { inner: 7 } }
optVal := optObj?.prop.inner
Assert(optVal = 7, A_LineNumber)

optObj := { prop: unset }
optVal := optObj?.prop?.inner
Assert(!IsSet(optVal), A_LineNumber)

; v2.1-alpha.30 removed `a?.[i]` and `a?.()` in favour of `(a?)[i]` and `(a?)()`.
optArr := [10, 20]
optVal := (optArr?)[2]
Assert(optVal = 20, A_LineNumber)

optArr := unset
optVal := (optArr?)[1]
Assert(!IsSet(optVal), A_LineNumber)

optCount := 0
Bump()
{
	global optCount
	optCount += 1
	return 42
}

class OptClass
{
	M(x)
	{
		return x + 1
	}
}

optObj := unset
optVal := optObj?.M(Bump())
Assert(optCount = 0 && !IsSet(optVal), A_LineNumber)

optObj := OptClass()
optVal := optObj?.M(Bump())
Assert(optCount = 1 && optVal = 43, A_LineNumber)

optObj := unset
optVal := optObj?.prop ?? 55
Assert(optVal = 55, A_LineNumber)

; Optional chaining with ternary + coalesce (both branches)
optObj := unset
dummyObj := { prop: 1 }
optVal := (true ? optObj?.prop : dummyObj?.prop) ?? 66
Assert(optVal = 66, A_LineNumber)

optVal := (false ? dummyObj?.prop : optObj?.prop) ?? 77
Assert(optVal = 77, A_LineNumber)

; v2.1-alpha.29: the maybe operator short-circuits most operators, so an unset operand
; propagates out of the whole expression without evaluating the rest of it.
sideEffects := 0

Bump2()
{
	global sideEffects
	return ++sideEffects
}

missing := unset

optVal := missing?.value + Bump2()
Assert(!IsSet(optVal) && sideEffects = 0, A_LineNumber)

optVal := -(missing?) + Bump2()
Assert(!IsSet(optVal) && sideEffects = 0, A_LineNumber)

optVal := (missing?) ? Bump2() : Bump2()
Assert(!IsSet(optVal) && sideEffects = 0, A_LineNumber)

optCallable := unset
optVal := (optCallable?)(Bump2())
Assert(!IsSet(optVal) && sideEffects = 0, A_LineNumber)

optIndexable := unset
optVal := (optIndexable?)[Bump2()]
Assert(!IsSet(optVal) && sideEffects = 0, A_LineNumber)

; An assignment through an unset target is skipped entirely, value expression included.
missing?.value := Bump2()
Assert(sideEffects = 0, A_LineNumber)

; v2.1-alpha.30: `x?.%y%` is valid syntax, not a load-time error.
memberName := "value"
optVal := missing?.%memberName%
Assert(!IsSet(optVal), A_LineNumber)

; Short-circuiting must not swallow a set operand: the same forms still compute normally.
present := { value: 5 }
optVal := present?.value + Bump2()
Assert(optVal = 6 && sideEffects = 1, A_LineNumber)

; The access before ?. may be missing or return no value.
class OptHolder
{
	Stored := 0
	Prop => unset
	Present => { Inner: 9 }
	Items[Index] => unset
	__Item[Index] => unset
	NoValue() => unset
	Counted
	{
		get
		{
			global sideEffects
			sideEffects += 1
			return this
		}
	}
	Writable[Index]
	{
		get
		{
			global sideEffects
			sideEffects += 1
			return this.Stored
		}
		set => this.Stored := Value
	}
	Failure
	{
		get
		{
			throw ValueError("getter")
		}
	}
}

OptNoValue() => unset

holder := OptHolder()
holder?.NoValue()
optVal := holder.Prop?.Inner
Assert(!IsSet(optVal), A_LineNumber)
optVal := holder.Missing?.Inner
Assert(!IsSet(optVal), A_LineNumber)
memberName := "Missing"
optVal := holder.%memberName%?.Inner
Assert(!IsSet(optVal), A_LineNumber)
optNumber := 5
optVal := optNumber.Missing?.Inner
Assert(!IsSet(optVal), A_LineNumber)
optVal := holder[1]?.Inner
Assert(!IsSet(optVal), A_LineNumber)
optVal := holder.Items[1]?.Inner
Assert(!IsSet(optVal), A_LineNumber)
optVal := OptNoValue()?.Inner
Assert(!IsSet(optVal), A_LineNumber)
optVal := holder?.NoValue()?.Inner
Assert(!IsSet(optVal), A_LineNumber)
Assert(!IsSet(holder?.Prop?), A_LineNumber)
Assert(!IsSet(holder?.NoValue()?), A_LineNumber)
AssertEq((holder?.Prop?) ?? 12, 12, A_LineNumber)
AssertEq(holder.Present?.Inner ?? 0, 9, A_LineNumber)
AssertEq((holder.Missing?.Inner == 9) ?? "short", "short", A_LineNumber)

before := sideEffects
AssertEq(holder.Counted?.Present.Inner ?? 0, 9, A_LineNumber)
AssertEq(sideEffects, before + 1, A_LineNumber)

before := sideEffects
holder?.Writable[Bump2()] := 7
AssertEq(holder.Stored, 7, A_LineNumber)
AssertEq(sideEffects, before + 1, A_LineNumber)
memberName := "Writable"
holder.Counted?.%memberName%[Bump2()] += 1
AssertEq(holder.Stored, 8, A_LineNumber)
AssertEq(sideEffects, before + 4, A_LineNumber)
holder.Missing?.Writable[Bump2()] := Bump2()
holder.Missing?.%memberName%[Bump2()] += Bump2()
AssertEq(sideEffects, before + 4, A_LineNumber)

; The rest of the chain, its arguments and an assigned value are skipped.
before := sideEffects
optVal := holder.Prop?.Inner(Bump2())
holder.Missing?.Inner := Bump2()
holder.Prop?.Inner += Bump2()
Assert(!IsSet(optVal) && sideEffects = before, A_LineNumber)

; Only the access the maybe operator follows is lenient.
Throws(() => holder.Missing.Inner?.Value, A_LineNumber, PropertyError)
Throws(() => holder.Missing[1]?.Inner, A_LineNumber, PropertyError)
Throws(() => (holder?.Prop)?.Inner, A_LineNumber, UnsetError)
Throws(() => holder?.Missing, A_LineNumber, PropertyError)
Throws(() => holder?.Prop, A_LineNumber, UnsetError)
Throws(() => holder?.Items[1], A_LineNumber, UnsetError)
Throws(() => IsSet(holder?.Prop), A_LineNumber, UnsetError)
Throws(() => holder?.Prop ?? 12, A_LineNumber, UnsetError)
Throws(() => holder.Failure?.Inner, A_LineNumber, ValueError)

FileAppend "pass", "*"
