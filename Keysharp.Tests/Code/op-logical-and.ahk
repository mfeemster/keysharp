;#Include %A_ScriptDir%/header.ahk

x := true
y := false

If (x and y = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x and y = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!((x and y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (not ((x and y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!((x and y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If (not ((x and y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1
y := 0

If ((x and y) = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x and y) = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!((x and y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (not ((x and y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!((x and y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If (not ((x and y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	

x := 1.234
y := 5.678

If ((x and y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x and y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!((x and y) = false))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (not ((x and y) = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!((x and y) = true))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not ((x and y) = true))
	FileAppend, fail, *
else
	FileAppend, pass, *

; Now do again with &&

x := true
y := false

If ((x && y) = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x && y) = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!((x && y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not ((x && y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!((x && y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not ((x && y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1
y := 0

If ((x && y) = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x && y) = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!((x && y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not ((x && y) = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!((x && y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not ((x && y) = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1.234
y := 5.678

If ((x && y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x && y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!((x && y) = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not ((x && y) = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!((x && y) = true))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not ((x && y) = true))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
A := 1, B := {}, C := 20, D := True, E := "String" ; All operands are truthy and will be evaluated
x := A && B && C && D && E ; The last truthy operand is returned ("String")

if (x == "String")
	FileAppend, pass, *
else
	FileAppend, fail, *
	
A := 1, B := "", C := 0, D := False, E := "String" ; B is falsey, C and D are false
x := A && B && ++C && D && E ; The first falsey operand is returned (""). C, D and E are not evaluated and C is never incremented

if (x == "")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (C == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *