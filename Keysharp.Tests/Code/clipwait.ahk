; #Include %A_ScriptDir%/header.ahk

; Clipboard should have been set before this test script is run.

A_ErrorLevel := 0
ClipWait, , true

if (A_ErrorLevel = 0) ; Waited for any type indefinitely and successfully detected clipboard data because a bitmap was placed on the clipboard before this script was run.
	FileAppend, pass, *
else
	FileAppend, fail, *

A_ErrorLevel := 0
ClipWait, 0.5

if (A_ErrorLevel = 1) ; Waited specifically for text/file data for 0.5s and did not detect that kind of clipboard data because a bitmap was placed on the clipboard before this script was run.
	FileAppend, pass, *
else
	FileAppend, fail, *