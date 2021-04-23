;#Include %A_ScriptDir%/header.ahk

x := true
y := false

If (x or y)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x or y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x or y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x or y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If ((x or y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1
y := 0

If (x or y)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x or y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x or y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x or y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If ((x or y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1.234
y := 5.678

If (x or y)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x or y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x or y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x or y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If ((x or y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *
	

; Now do again with ||

x := true
y := false

If (x || y)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x || y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x || y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x || y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If ((x || y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1
y := 0

If (x || y)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x || y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x || y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x || y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If ((x || y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1.234
y := 5.678

If (x || y)
	FileAppend, pass, *
else
	FileAppend, fail, *

If ((x || y) = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x || y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x || y))
	FileAppend, fail, *
else
	FileAppend, pass, *

If ((x || y) = false)
	FileAppend, fail, *
else
	FileAppend, pass, *