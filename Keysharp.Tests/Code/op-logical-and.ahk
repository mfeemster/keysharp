x := true
y := false

If (x and y = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (x and y = true)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (!((x and y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (not ((x and y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!((x and y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If (not ((x and y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (true and false = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!(("true" and false) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (not (("true" and "false") = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"


x := 1
y := 0

If ((x and y) = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If ((x and y) = true)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (!((x and y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (not ((x and y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!((x and y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If (not ((x and y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If ((1 and 0) = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!(("1" and 0) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (not (("1" and "0x0") = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1.234
y := 5.678

If ((x and y) = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If ((x and y) = true)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (!((x and y) = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (not ((x and y) = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!((x and y) = y))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (not ((x and y) = y))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If ((1.234 and 5.678) = 5.678)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!(("1.234" and 5.678) = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If (not (("1.234" and "5.678") = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

; Now do again with &&

x := true
y := false

If ((x && y) = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If ((x && y) = true)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (!((x && y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (not ((x && y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!((x && y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (not ((x && y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If ((true && false) = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!(("true" && false) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (not (("true" && "false") = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1
y := 0

If ((x && y) = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If ((x && y) = true)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (!((x && y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (not ((x && y) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!((x && y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (not ((x && y) = false))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"
	
If ((1 && 0) = false)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!(("1" && 0) = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (not (("1" && "0x0") = true))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 1.234
y := 5.678

If ((x && y) = y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If ((x && y) = false)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (!((x && y) = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (not ((x && y) = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!((x && y) = y))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If (not ((x && y) = y))
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If ((1.234 && 5.678) = 5.678)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (!(("1.234" && 5.678) = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If (not (("1.234" && "5.678") = false))
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

A := 1, B := {}, C := 20, D := True, E := "String" ; All operands are truthy and will be evaluated
x := A && B && C && D && E ; The last truthy operand is returned ("String")

if (x == "String")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
A := 1, B := "", C := 0, D := False, E := "String" ; B is falsey, C and D are false
x := A && B && ++C && D && E ; The first falsey operand is returned (""). C, D and E are not evaluated and C is never incremented

if (x == "")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (C == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

evalfunc(p1)
{
	return p1
}

val := evalfunc(1 && 2)

if (val == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc("1" && 2)

if (val == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc("1" && "0x2")

if (val == "0x2")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc(x := 1 && 2)

if (val == 2 && x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc(x := "1" && 2)

if (val == 2 && x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc(x := "0x1" && 2)

if (val == 2 && x == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if ((1 && 2) == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (("1" && 2) == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (("0x1" && 2) == 2)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (("0x1" && "0x2") == "0x2")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc(1 && true && 20 && "true")

if (val == "true")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := evalfunc(x := "1" && true && 20 && "true")

if (val == "true" && x == "true")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (x := ("1" && true && "0x20" && "true") == "true" && x == "true")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

val := evalfunc(1 && true && 20 && "true" && 0)

if (val == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
val := evalfunc(x := 1 && true && 20 && "true" && 0)

if (val == 0 && x == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if ((1 && true && "20" && "true" && 0) == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"