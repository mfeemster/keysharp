#NoTrayIcon
#Include <assert>

x := ""

Assert(!(x != ""), A_LineNumber)
	
Assert(x = "", A_LineNumber)

x := 123
x := unset

Assert(x is unset, A_LineNumber)

Assert(x = unset, A_LineNumber)

Assert(x = unset, A_LineNumber)

Assert(unset = x, A_LineNumber)
	
Assert(unset = x, A_LineNumber)

Assert(!(x != unset), A_LineNumber)

Assert(!(x !== unset), A_LineNumber)

Assert(!(unset != x), A_LineNumber)
	
Assert(!(unset !== x), A_LineNumber)

; The maybe operator short-circuits from within a dynamic name: no property is invoked and no parameter list is
; evaluated when the name is unset. [v2.1-alpha.34+]
named := {X: 1, __Item: 9}
missingName := unset
evaluated := false
AssertEq(named.%missingName?% ?? "none", "none", A_LineNumber)
AssertEq(named.%missingName?%(evaluated := true) ?? "none", "none", A_LineNumber)
AssertEq(named.%missingName?%[evaluated := true] ?? "none", "none", A_LineNumber)
Assert(!evaluated, A_LineNumber)
AssertEq(%missingName?% ?? "none", "none", A_LineNumber)
presentName := "X"
AssertEq(named.%presentName?% ?? "none", 1, A_LineNumber)
order := ""
AssertEq((order .= "target", named).%(order .= " name", presentName)?%, 1, A_LineNumber)
AssertEq(order, "target name", A_LineNumber)

FileAppend "pass", "*"
