

x := true
y :=

If (!x = false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (x != true)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!(x) = false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x) != true)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((!x) = false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (!(x = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If ((x != true))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (!y = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (y != true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (not x = false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (not x = true)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not (x) = false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (not (x) = true)
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If ((not x) = false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (not (x = false))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (not (x = true))
	FileAppend, "fail", "*"
else
	FileAppend, "pass", "*"

If (not y = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

If (not (y) = true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"