;
; Integers
;

x := 10 ; Positive int minus positive int.
y := x - 100

if (y = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Positive int minus negative int.
y := x - -100

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -10 ; Negative int minus positive int.
y := x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -10 ; Negative int minus negative int.
y := x - -100

if (y = 90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Negated positive int minus positive int.
y := -x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Positive int minus negated positive int.
y := 100 - -x

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0 ; Zero int minus positive non-zero int.
y := x - 100

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 100 ; Non-zero positive int minus zero int.
y := x - 0

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0 ; Zero int minus negative non-zero int.
y := x - -100

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -100 ; Non-zero negative int minus zero int.
y := x - 0

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0 ; Zero int minus zero int.
y := x - 0

if (y = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;
; Double
;

x := 10 ; Positive int minus positive double.
y := x - 100.0

if (y = -90.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Positive int minus negative double.
y := x - -100.0

if (y = 110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -10.0 ; Negative double minus positive int.
y := x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -10.0 ; Negative double minus negative int.
y := x - -100

if (y = 90.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Negated positive int minus positive double.
y := -x - 100.0

if (y = -110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10.0 ; Positive int minus negated positive double.
y := 100 - -x

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0 ; Zero int minus positive non-zero double.
y := x - 100.0

if (y = -100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 100.0 ; Non-zero positive double minus zero int.
y := x - 0

if (y = 100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0 ; Zero int minus negative non-zero double.
y := x - -100.0

if (y = 100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := -100.0 ; Non-zero negative double minus zero int.
y := x - 0

if (y = -100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 0.0 ; Zero double minus zero double.
y := x - 0.0

if (y = 0.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;
; Integer strings
;

x := "10" ; Positive int string minus positive int.
y := x - 100

if (y = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Positive int minus negative int string.
y := x - "-100"

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-10" ; Negative int string minus positive int.
y := x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-10" ; Negative int string minus negative int string.
y := x - "-100"

if (y = 90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "10" ; Negated positive int string minus positive int.
y := -x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "10" ; Positive int minus negated positive int string.
y := 100 - -x

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0" ; Zero int string minus positive non-zero int.
y := x - 100

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "100" ; Non-zero positive int string minus zero int.
y := x - 0

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0" ; Zero int string minus negative non-zero int.
y := x - -100

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-100" ; Non-zero negative int string minus zero int.
y := x - 0

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0" ; Zero int string minus zero int string.
y := x - "0"

if (y = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;
; Double strings
;

x := "10.0" ; Positive double string minus positive int.
y := x - 100

if (y = -90.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Positive int minus negative double string.
y := x - "-100.0"

if (y = 110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-10.0" ; Negative double string minus positive int.
y := x - 100

if (y = -110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-10.0" ; Negative double string minus negative double string.
y := x - "-100.0"

if (y = 90.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "10.0" ; Negated positive double string minus positive int.
y := -x - 100

if (y = -110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "10.0" ; Positive int minus negated positive double string.
y := 100 - -x

if (y = 110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0.0" ; Zero double string minus positive non-zero int.
y := x - 100

if (y = -100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "100.0" ; Non-zero positive double string minus zero int.
y := x - 0

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0.0" ; Zero double string minus negative non-zero int.
y := x - -100

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-100.0" ; Non-zero negative double string minus zero int.
y := x - 0

if (y = -100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0.0" ; Zero double string minus zero double string.
y := x - "0.0"

if (y = 0.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;
; Hex strings minus int and double
;

x := "0x0A" ; Positive hex string minus positive int.
y := x - 100

if (y = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Positive hex string minus positive double.
y := x - 100.0

if (y = -90.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10 ; Positive int minus negative hex string.
y := x - "-0x64"

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := 10.0 ; Positive double minus negative hex string.
y := x - "-0x64"

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x0A" ; Negative hex string minus positive int.
y := x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x0A" ; Negative hex string minus positive double.
y := x - 100.0

if (y = -110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x0A" ; Negative hex string minus negative int string.
y := x - "-100"

if (y = 90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "-0x0A" ; Negative hex string minus negative double string.
y := x - "-100.0"

if (y = 90.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Negated positive hex string minus positive int.
y := -x - 100

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "0x0A" ; Negated positive hex string minus positive double.
y := -x - 100.0

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Positive int minus negated positive hex string.
y := 100 - -x

if (y = 110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "0x0A" ; Positive double minus negated positive hex string.
y := 100.0 - -x

if (y = 110.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x00" ; Zero hex string minus positive non-zero int.
y := x - 100

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "0x00" ; Zero hex string minus positive non-zero double.
y := x - 100.0

if (y = -100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Non-zero positive hex string minus zero int.
y := x - 0

if (y = 10)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Non-zero positive hex string minus zero double.
y := x - 0.0

if (y = 10.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x00" ; Zero hex string minus negative non-zero int.
y := x - -100

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "0x00" ; Zero hex string minus negative non-zero double.
y := x - -100.0

if (y = 100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x64" ; Non-zero negative hex string minus zero int.
y := x - 0

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x64" ; Non-zero negative hex string minus zero double.
y := x - 0.0

if (y = -100.0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Float")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

;
; Hex strings
;

x := "0x0A" ; Positive hex string minus positive hex string.
y := x - "0x64"

if (y = -90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Positive hex string minus negative hex string.
y := x - "-0x64"

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x0A" ; Negative hex string minus positive hex string.
y := x - "0x64"

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x0A" ; Negative hex string minus negative hex string.
y := x - "-0x64"

if (y = 90)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
x := "0x0A" ; Negated positive hex string minus positive hex string.
y := -x - "0x64"

if (y = -110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Positive hex string minus negated positive hex string.
y := "0x64" - -x

if (y = 110)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x00" ; Zero hex string minus positive non-zero hex string.
y := x - "0x64"

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x0A" ; Non-zero positive hex string minus zero hex string.
y := x - "0x00"

if (y = 10)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x00" ; Zero hex string minus negative non-zero hex string.
y := x - "-0x64"

if (y = 100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "-0x64" ; Non-zero negative hex string minus zero hex string.
y := x - "0x00"

if (y = -100)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

x := "0x00" ; Zero hex string minus zero hex string.
y := x - "0x00"

if (y = 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (Type(y) = "Integer")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"