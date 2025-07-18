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

If (true or false = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(("true" or false) = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (not (("true" or "false") = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

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
	
If ((1 or 0) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(("1" or 0) = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (not (("1" or "0x0") = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1.234
y := 5.678

If (x or y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x or y) = x)
	FileAppend "pass", "*"
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

If ((1.234 or 5.678) = 1.234)
	FileAppend "pass", "*"
else
	FileAppend, "fail", "*"

If (!(("1.234" or 5.678) = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (not (("1.234" or "5.678") = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

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

If (true || false = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(("true" || false) = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (not (("true" || "false") = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

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

If ((1 || 0) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(("1" || 0) = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (not (("1" || "0x0") = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 1.234
y := 5.678

If (x || y)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x || y) = x)
	FileAppend "pass", "*"
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

If ((1.234 || 5.678) = 1.234)
	FileAppend "pass", "*"
else
	FileAppend, "fail", "*"

If (!(("1.234" || 5.678) = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
If (not (("1.234" || "5.678") = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

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

evalfunc(p1)
{
	return p1
}

val := evalfunc(0 || 2)

if (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := evalfunc("0" || 2)

if (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := evalfunc("0" || "0x2")

if (val == "0x2")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := evalfunc(x := 0 || 2)

if (val == 2 && x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := evalfunc(x := "0" || 2)

if (val == 2 && x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := evalfunc(x := "0" || "0x2")

if (val == "0x2" && x == "0x2")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if ((0 || 2) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (("0" || 2) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (("0x0" || 2) == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (("0x0" || "0x2") == "0x2")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if ((x := 0 || 2) == 2 && x == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := evalfunc(x := "" || false || 0 || 123)

if (val == 123 && x == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (("" || "false" || 0 || 123) == "false")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (("" || "false" || 0 || 123) == false)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

a := unset
try {
  if (!a)
    FileAppend "fail", "*"
  else
    FileAppend "fail", "*"
} catch {
  FileAppend "pass", "*"
}