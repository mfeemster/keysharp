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

If (!(x and y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (not (x and y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x and y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If (not (x and y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1
y := 0

If (x and y = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x and y = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!(x and y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (not (x and y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x and y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
If (not (x and y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

x := 1.234
y := 5.678

If (x and y = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x and y = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!(x and y = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not (x and y = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x and y = true))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x and y = true))
	FileAppend, fail, *
else
	FileAppend, pass, *
	

; Now do again with &&

x := true
y := false

If (x && y = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x && y = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!(x && y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not (x && y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x && y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x && y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
x := 1
y := 0

If (x && y = false)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x && y = true)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!(x && y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not (x && y = true))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x && y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x && y = false))
	FileAppend, fail, *
else
	FileAppend, pass, *
	
x := 1.234
y := 5.678

If (x && y = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x && y = false)
	FileAppend, fail, *
else
	FileAppend, pass, *

If (!(x && y = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (not (x && y = false))
	FileAppend, pass, *
else
	FileAppend, fail, *

If (!(x && y = true))
	FileAppend, fail, *
else
	FileAppend, pass, *

If (not (x && y = true))
	FileAppend, fail, *
else
	FileAppend, pass, *
