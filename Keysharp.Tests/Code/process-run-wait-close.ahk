; #Include %A_ScriptDir%/header.ahk

pid := Run("notepad.exe", "", "max")
ProcessWait(pid)
ProcessSetPriority("H", pid)
exists := ProcessExist(pid)

if (exists != 0)
{
	Sleep(2000)
	ProcessClose(pid)
	ProcessWaitClose(pid)
}

Sleep(1000)
exists := ProcessExist("notepad.exe")

if (exists == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

pid := RunWait("notepad.exe", "", "max")
Sleep(1000)
exists := ProcessExist("notepad.exe")

if (exists == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *