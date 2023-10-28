;#Include %A_ScriptDir%/header.ahk

; Can't really test if some of these properties have "valid" values. So at least just test if they can be compiled properly in a script.

x := A_WorkingDir

if (x != "")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_ScriptName

if (x == "props-script-properties.ahk")
	FileAppend, pass, *
else
	FileAppend, fail, *
x := A_ScriptFullPath

if (x != "")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_ScriptDir

if (x != "")
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_ScriptHwnd

if (x = 0) ; No window in this test, so HWND will be 0.
	FileAppend, pass, *
else
	FileAppend, fail, *


x := A_LineNumber ; This line must start at 40 for the test to pass, so nothing above it can ever be moved.

if (x = 40)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_LineNumber

if (x = 47)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := A_LineNumber

if (x = 54)
	FileAppend, pass, *
else
	FileAppend, fail, *

; End line tests. Anything after this can be moved.

x := A_LineFile

if (x = A_ScriptFullPath) ; These two are always the same except for when the latter is in an include file.
	FileAppend, pass, *
else
	FileAppend, fail, *

myfunc()
{
	y := A_ThisFunc

	if (y == "myfunc")
		FileAppend, pass, *
	else
		FileAppend, fail, *
}

myfunc()

if (A_IsUnicode == true)
	FileAppend, pass, *
else
	FileAppend, fail, *