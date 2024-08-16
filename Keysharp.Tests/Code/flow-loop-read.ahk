x := ""

; MsgBox(A_WorkingDir)

Loop Read "../../../Keysharp.Tests/Code/test-text-file.txt"
{
	x .= A_LoopReadLine
}

If (x == "this is line 1another lineline 3")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := ""
FileDelete "../../../Keysharp.Tests/Code/test-text-file-out.txt"

Loop Read "../../../Keysharp.Tests/Code/test-text-file.txt", "../../../Keysharp.Tests/Code/test-text-file-out.txt" ; this is a comment
{
	y := Random()
	x .= A_LoopReadLine
	x .= y
	z := A_LoopReadLine
	z .= y
	FileAppend(z)
}

z := ""

Loop Read  "../../../Keysharp.Tests/Code/test-text-file-out.txt" ; another comment
{
	z.= A_LoopReadLine
}

If (x == z)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"