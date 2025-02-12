x := true
y := unset

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

x := 123

if (not (x is null))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (not (x is unset))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (not (x = null))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (not (x == null))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
