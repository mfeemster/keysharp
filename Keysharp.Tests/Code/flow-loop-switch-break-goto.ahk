#ErrorStdOut
#Warn All, StdOut
#NoTrayIcon
#Include <assert>

x := 0

Loop 10
{
	x++
	If (A_Index == 5)
		goto l1
}

l1:

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

Loop 10
{
	x++
	If (A_Index == 5)
		break 1
}

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

looplabel1:
Loop 10
{
	x++
	If (A_Index == 5)
		break looplabel1
}

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

outerlooplabel1:
Loop 10
{
	x++
	If (A_Index == 5)
		break outerlooplabel1
}

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

outerlooplabel2:
Loop 10
{
	innerlooplabel2:
	Loop 10
	{
		x++
		If (A_Index == 5)
			break innerlooplabel2
	}
	
	x := 999
}

AssertEq(x, 999, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

outerlooplabel3:
Loop 10
{
	innerlooplabel3:
	Loop 10
	{
		x++
		If (A_Index == 5)
			break outerlooplabel3
	}
	
	x := 999
}

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

Loop 10
{
	Loop 10
	{
		x++
		If (A_Index == 5)
			break 2
	}
	
	x := 999
}

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

Loop 10
{
	Loop 10
	{
		x++
		If (A_Index == 5)
			goto l4
	}
	
	x := 999
}
l4:

AssertEq(x, 5, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0

Loop 10
{
	Loop 10
	{
		x++
		If (A_Index == 5)
			goto l5
	}
l5:
	x := 999
}

AssertEq(x, 999, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0
y := 123

Loop 10
{
	x++
	switch y
	{
		case 3:
			z := 3
		case 2:
			z := 2
		case 123:
			break
	}
}

AssertEq(x, 1, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0
y := 123

Loop 10
{
	Loop 10
	{
		x++
		switch y
		{
			case 3:
				z := 3
			case 2:
				z := 2
			case 123:
				break 2
		}
	}
}

AssertEq(x, 1, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

x := 0
y := 123

outerswitchlooplabel1:
Loop 10
{
	innerswitchlooplabel1:
	Loop 10
	{
		x++
		switch y
		{
			case 3:
				z := 3
			case 2:
				z := 2
			case 123:
				break outerswitchlooplabel1
		}
	}
	
	x := 999
}

AssertEq(x, 1, A_LineNumber)

AssertEq(A_Index, 0, A_LineNumber)

; A loop is named by every label of the run directly before it.
x := 0
stackedOuter:
stackedInner:
Loop 3
{
	Loop 3
	{
		x++
		break stackedOuter
	}
}

AssertEq(x, 1, A_LineNumber)

; A computed goto resolves its label when it runs, among the labels in blocks enclosing it.
ComputedGoto(name) {
	result := ""
	if (name != "")
		goto(name)
	result .= "a"
	first:
	result .= "b"
	Second:
	result .= "c"
	return result
}

AssertEq(ComputedGoto(""), "abc", A_LineNumber)
AssertEq(ComputedGoto("first"), "bc", A_LineNumber)
AssertEq(ComputedGoto("SECOND"), "c", A_LineNumber)
AssertError(() => ComputedGoto("nosuch"), "Error: Label not found in current scope. [nosuch]", A_LineNumber)

ComputedGotoInto(name) {
	if (name != "")
		goto(name)
	if (name = "") {
		inside:
		return 1
	}
	return 0
}

AssertEq(ComputedGotoInto(""), 1, A_LineNumber)
AssertError(() => ComputedGotoInto("inside"), "Error: A Goto must not jump into a block that doesn't enclose it. []", A_LineNumber)

ComputedGotoFinally(name, exitFinally := false) {
	result := ""
	try {
		goto(name)
	} finally {
		result .= "finally"
		if exitFinally
			Goto(name)
	}
	return "missed"
	done:
	return result
}

AssertEq(ComputedGotoFinally("DONE"), "finally", A_LineNumber)
AssertError(() => ComputedGotoFinally("done", true), "Error: Jumps cannot exit a FINALLY block. []", A_LineNumber)

; IsLabel looks among the labels of the function calling it, or of the auto-execute section outside any function.
AssertEq(IsLabel("l1") IsLabel("L1") IsLabel("InFunc") IsLabel("nosuch"), "1100", A_LineNumber)

LabelsOf() {
	InFunc:
	viaObject := IsLabel
	Inner() {
		InNested:
		return IsLabel("InNested") IsLabel("InFunc")
	}
	return IsLabel("InFunc") IsLabel("l1") viaObject("InFunc") Inner()
}

AssertEq(LabelsOf(), "10110", A_LineNumber)

FileAppend "pass", "*"
