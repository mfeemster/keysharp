

x := true
y := false

If (x or y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x or y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x or y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x or y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((x or y) = false)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := 1
y := 0

If (x or y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x or y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x or y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x or y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((x or y) = false)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := 1.234
y := 5.678

If (x or y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x or y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x or y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x or y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((x or y) = false)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	

; Now do again with ||

x := true
y := false

If (x || y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x || y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x || y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x || y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((x || y) = false)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := 1
y := 0

If (x || y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x || y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x || y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x || y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((x || y) = false)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := 1.234
y := 5.678

If (x || y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x || y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x || y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x || y))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((x || y) = false)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

A := "", B := False, C := 0, D := "String", E := 20 ; At least one operand is truthy. All operands up until D (including) will be evaluated
x := A || B || C || D || ++E ; The first truthy operand is returned ("String"). E is not evaluated and is never incremented

if (x == "String")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (E == 20)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

A := "", B := False, C := 0 ; All operands are falsey and will be evaluated
x := A || B || C ; The last falsey operand is returned (0)

if (x == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"