#CLIPBOARDTIMEOUT 2000
#ERRORSTDOUT true
#USEHOOK true
#MAXTHREADS 100
#MAXTHREADSBUFFER 1
#MAXTHREADSPERHOTKEY 150
#NOTRAYICON
#SUSPENDEXEMPT 1
#WINACTIVATEFORCE

if (A_ClipboardTimeout == 2000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_ClipboardTimeout == 2000)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
				
#USEHOOK

if (A_UseHook)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
#USEHOOK 0

if (!A_UseHook)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#USEHOOK true

if (A_UseHook)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_MaxThreadsBuffer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
#MAXTHREADSBUFFER 0

if (!A_MaxThreadsBuffer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#MAXTHREADSBUFFER

if (A_MaxThreadsBuffer)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_MaxThreadsPerHotkey == 150)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#MAXTHREADSPERHOTKEY 300

if (A_MaxThreadsPerHotkey == 255)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_NoTrayIcon)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (A_SuspendExempt)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#SUSPENDEXEMPT 0

if (!A_SuspendExempt)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#SUSPENDEXEMPT

if (A_SuspendExempt)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (A_WinActivateForce)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#INPUTLEVEL 50

if (A_InputLevel == 50)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

#INPUTLEVEL

if (A_InputLevel == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"