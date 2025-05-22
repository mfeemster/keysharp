shell := ComObject("WScript.Shell")
exec := shell.Exec("Notepad.exe")
exec := shell.Run("Notepad.exe")

dict := ComObject("Scripting.Dictionary")

dict.Add("Name", "Alice")
dict.Add("Age", 30)
dict.Add("Country", "USA")

if (dict.Item("Name") == "Alice")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (dict.Item("Age") == 30)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (dict.Item("Country") == "USA")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (dict["Name"] == "Alice" && dict["Age"] == 30 && dict["Country"] == "USA")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

dict["Age"] := 50

if (dict.Item("Age") == 50 && dict["Age"] == 50)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

dict["newval"] := 75

if (dict.Item("newval") == 75 && dict["newval"] == 75)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"