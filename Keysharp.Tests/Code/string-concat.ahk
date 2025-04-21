x := "this is a string"
y := " and another string"
z := x . y

If (z = "this is a string and another string")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 123
y := 456
z := x . y

If (z = "123456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := ""
z := x y

If (z = "123456")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := "The number is " . (x * 10)

If (z = "The number is 1230")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

z := "The number is"
. " another line"

If (z = "The number is another line")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

a .= "hello"

if (a = "hello")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"