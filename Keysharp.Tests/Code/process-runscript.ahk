info := RunScript("ExitApp(0)",,, "Keysharp.exe")
if (info.ExitCode == 0)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

info := ""
info := RunScript("ExitApp(1)",,, "Keysharp.exe")
if (info.ExitCode == 1)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

AsyncCallback(callbackinfo) {
	global result := callbackinfo.ExitCode
}
info := "", result := ""
info := RunScript("ExitApp(3)", AsyncCallback,, "Keysharp.exe")

while (!info.HasExited)
	Sleep 10

if (info.ExitCode == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (result == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

info := ""
script := "
(
	stdout := FileOpen("*", "r")
	stdin := FileOpen("*", "w")
	str := stdout.ReadLine()
	stdin.WriteLine(str str)
)"
info := RunScript(script, 1,, "Keysharp.exe")
info.StdIn.WriteLine("a")
while (!info.HasExited)
	Sleep 10

if (info.StdOut.Read(2) == "aa")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
