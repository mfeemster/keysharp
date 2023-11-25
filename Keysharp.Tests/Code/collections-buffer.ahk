;#Include %A_ScriptDir%/header.ahk

buf := Buffer(5, 10)

If (buf.Size == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
Loop (buf.Size)
{
	p := buf[A_Index]
	
	If (p == 10)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}

buf.Size := 10

If (buf.Size == 10)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
; Ensure original values were copied. Subsequent values are undefined.
Loop (5)
{
	p := buf[A_Index]
	
	If (p == 10)
		FileAppend, "pass", "*"
	else
		FileAppend, "fail", "*"
}