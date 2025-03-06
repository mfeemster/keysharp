x := 2
y := 2
z := x**y

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != 4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

z := 2**2

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := "2"**2

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := "0x2"**"0x2"

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; This works differently than standard AHK. Here, - applies to just the first term, but in standard AHK, it would apply to the result.
; So this will give the same result as above.
z := -x**y

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != 4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
; Should give the same result as - with no parens.
z := -(x)**y

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != 4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

; Should give the same result as - with no parens.
z := (-x)**y

If (z = 4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != 4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"
	
; To apply the - to the result, parens are needed.
z := -(x**y)

If (z = -4)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != -4)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

; Now do float with int
x := 0.5
y := 2
z := x**y

If (z = 0.25)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != 0.25)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

z := 0.5**2

If (z = 0.25)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 2
y := 0.5
z := x**y

If (z = "1.414214")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != "1.414214")
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := 2
y := 0.5
z := 2**0.5

If (z = "1.414214")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Now do float with float
x := 0.5
y := 0.5
z := x**y

If (z = "0.707107")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (z != "0.707107")
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

x := 0.5
y := 0.5
z := 0.5**0.5

If (z = "0.707107")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"