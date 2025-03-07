;
; Integers
;

x := 10 ; Positive int plus positive int.
y := x + 100

if (y = 110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Positive int plus negative int.
y := x + -100

if (y = -90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -10 ; Negative int plus positive int.
y := x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -10 ; Negative int plus negative int.
y := x + -100

if (y = -110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Negated positive int plus positive int.
y := -x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Positive int plus negated positive int.
y := 100 + -x

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0 ; Zero int plus positive non-zero int.
y := x + 100

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 100 ; Non-zero positive int plus zero int.
y := x + 0

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0 ; Zero int plus negative non-zero int.
y := x + -100

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -100 ; Non-zero negative int plus zero int.
y := x + 0

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0 ; Zero int plus zero int.
y := x + 0

if (y = 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

;
; Double
;

x := 10 ; Positive int plus positive double.
y := x + 100.0

if (y = 110.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Positive int plus negative double.
y := x + -100.0

if (y = -90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -10.0 ; Negative double plus positive int.
y := x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -10.0 ; Negative double plus negative int.
y := x + -100

if (y = -110.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Negated positive int plus positive double.
y := -x + 100.0

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10.0 ; Positive int plus negated positive double.
y := 100 + -x

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0 ; Zero int plus positive non-zero double.
y := x + 100.0

if (y = 100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 100.0 ; Non-zero positive double plus zero int.
y := x + 0

if (y = 100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0 ; Zero int plus negative non-zero double.
y := x + -100.0

if (y = -100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := -100.0 ; Non-zero negative double plus zero int.
y := x + 0

if (y = -100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0.0 ; Zero double plus zero double.
y := x + 0.0

if (y = 0.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

;
; Integer strings
;

x := "10" ; Positive int string plus positive int.
y := x + 100

if (y = 110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Positive int plus negative int string.
y := x + "-100"

if (y = -90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-10" ; Negative int string plus positive int.
y := x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-10" ; Negative int string plus negative int string.
y := x + "-100"

if (y = -110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "10" ; Negated positive int string plus positive int.
y := -x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "10" ; Positive int plus negated positive int string.
y := 100 + -x

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0" ; Zero int string plus positive non-zero int.
y := x + 100

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "100" ; Non-zero positive int string plus zero int.
y := x + 0

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0" ; Zero int string plus negative non-zero int.
y := x + -100

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-100" ; Non-zero negative int string plus zero int.
y := x + 0

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0" ; Zero int string plus zero int string.
y := x + "0"

if (y = 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

;
; Double strings
;

x := "10.0" ; Positive double string plus positive int.
y := x + 100

if (y = 110.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Positive int plus negative double string.
y := x + "-100.0"

if (y = -90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-10.0" ; Negative double string plus positive int.
y := x + 100

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-10.0" ; Negative double string plus negative double string.
y := x + "-100.0"

if (y = -110.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "10.0" ; Negated positive double string plus positive int.
y := -x + 100

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "10.0" ; Positive int plus negated positive double string.
y := 100 + -x

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0.0" ; Zero double string plus positive non-zero int.
y := x + 100

if (y = 100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "100.0" ; Non-zero positive double string plus zero int.
y := x + 0

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0.0" ; Zero double string plus negative non-zero int.
y := x + -100

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
					
x := "-100.0" ; Non-zero negative double string plus zero int.
y := x + 0

if (y = -100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0.0" ; Zero double string plus zero double string.
y := x + "0.0"

if (y = 0.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

;
; Hex strings plus int and double
;

x := "0x0A" ; Positive hex string plus positive int.
y := x + 100

if (y = 110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Positive hex string plus positive double.
y := x + 100.0

if (y = 110.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10 ; Positive int plus negative hex string.
y := x + "-0x64"

if (y = -90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 10.0 ; Positive double plus negative hex string.
y := x + "-0x64"

if (y = -90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x0A" ; Negative hex string plus positive int.
y := x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x0A" ; Negative hex string plus positive double.
y := x + 100.0

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x0A" ; Negative hex string plus negative int string.
y := x + "-100"

if (y = -110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "-0x0A" ; Negative hex string plus negative double string.
y := x + "-100.0"

if (y = -110.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Negated positive hex string plus positive int.
y := -x + 100

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Negated positive hex string plus positive double.
y := -x + 100.0

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Positive int plus negated positive hex string.
y := 100 + -x

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "0x0A" ; Positive double plus negated positive hex string.
y := 100.0 + -x

if (y = 90.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x00" ; Zero hex string plus positive non-zero int.
y := x + 100

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "0x00" ; Zero hex string plus positive non-zero double.
y := x + 100.0

if (y = 100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Non-zero positive hex string plus zero int.
y := x + 0

if (y = 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Non-zero positive hex string plus zero double.
y := x + 0.0

if (y = 10.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x00" ; Zero hex string plus negative non-zero int.
y := x + -100

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "0x00" ; Zero hex string plus negative non-zero double.
y := x + -100.0

if (y = -100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x64" ; Non-zero negative hex string plus zero int.
y := x + 0

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x64" ; Non-zero negative hex string plus zero double.
y := x + 0.0

if (y = -100.0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Float")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

;
; Hex strings
;

x := "0x0A" ; Positive hex string plus positive hex string.
y := x + "0x64"

if (y = 110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Positive hex string plus negative hex string.
y := x + "-0x64"

if (y = -90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x0A" ; Negative hex string plus positive hex string.
y := x + "0x64"

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x0A" ; Negative hex string plus negative hex string.
y := x + "-0x64"

if (y = -110)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Negated positive hex string plus positive hex string.
y := -x + "0x64"

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Positive hex string plus negated positive hex string.
y := "0x64" + -x

if (y = 90)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x00" ; Zero hex string plus positive non-zero hex string.
y := x + "0x64"

if (y = 100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x0A" ; Non-zero positive hex string plus zero hex string.
y := x + "0x00"

if (y = 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x00" ; Zero hex string plus negative non-zero hex string.
y := x + "-0x64"

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "-0x64" ; Non-zero negative hex string plus zero hex string.
y := x + "0x00"

if (y = -100)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "0x00" ; Zero hex string plus zero hex string.
y := x + "0x00"

if (y = 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"