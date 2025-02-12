x := "`""

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(34))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '"'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(34))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "``"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(0x60))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '""'

if (x.Length == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(34) && x[2] == Chr(34))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`''

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(0x27))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "`n"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(10))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "`r"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(13))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`n'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(10))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`r'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(13))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "`s"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == " ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "`b"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(8))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`s'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == " ")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`b'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(8))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "`v"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(11))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := "`f"

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(12))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`v'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(11))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := '`f'

if (x.Length == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (x[1] == Chr(12))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; There was once a bug where certain verbatim strings being passed as function arguments wasn't parsing.
strargfunc(x)
{
    return x
}

strargfunc2(x, y)
{
    return x
}

value := 123
xx := strargfunc('"' value "'")

if (xx == "`"123'" )
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

xx := ""
xx := strargfunc2('"' value "'", -1)

if (xx == "`"123'" )
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"