shell := ComObject("WScript.Shell")
exec := shell.Exec("Notepad.exe")
exec := shell.Run("Notepad.exe")

dict := ComObject("Scripting.Dictionary")

dict.Add("Name", "Alice")
dict.Add("Age", 30)
dict.Add("Country", "USA")

if (dict.Item("Name") == "Alice")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (dict.Item("Age") == 30)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (dict.Item("Country") == "USA")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (dict["Name"] == "Alice" && dict["Age"] == 30 && dict["Country"] == "USA")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dict["Age"] := 50

if (dict.Item("Age") == 50 && dict["Age"] == 50)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dict["newval"] := 75

if (dict.Item("newval") == 75 && dict["newval"] == 75)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
=======
shell := ComObject("WScript.Shell")
exec := shell.Exec("Notepad.exe")
exec := shell.Run("Notepad.exe")

dict := ComObject("Scripting.Dictionary")

dict.Add("Name", "Alice")
dict.Add("Age", 30)
dict.Add("Country", "USA")

if (dict.Item("Name") == "Alice")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if dict.Exists("Age")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (dict.Item("Age") == 30)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
if (dict.Item("Country") == "USA")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (dict["Name"] == "Alice" && dict["Age"] == 30 && dict["Country"] == "USA")
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dict["Age"] := 50

if (dict.Item("Age") == 50 && dict["Age"] == 50)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dict["newval"] := 75

if (dict.Item("newval") == 75 && dict["newval"] == 75)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

totalKeys := ""
totalVals := ""

for key in dict.Keys
{
	totalKeys .= key
	totalVals .= dict.Item(key)
}

if totalKeys == "NameAgeCountrynewval"
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if totalVals == "Alice50USA75"
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

dict.Remove("Country")

if (dict.Count == 3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

SIZEOF_VARIANT := 8 + (2 * A_PtrSize)
var := Buffer(SIZEOF_VARIANT, 0)

cv := ComValue(0x4000 | 0x3, var.Ptr)
cv[] := 5

if (cv[] == 5)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (NumGet(var, 0, "int") == 5)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

cv := ComValue(0x4000 | 0xB, var.Ptr)
cv[] := true

if (cv[] == true)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if (NumGet(var, 0, "int") == 65535)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

cv := ComValue(0x4000 | 0xC, var.Ptr) ; VT_VARIANT
cv[] := 5

if (cv[] == 5)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (NumGet(var, 0, "int") == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

script := "
(
	x := ComObjActive("{6B39CAA1-A320-4CB0-8DB4-352AA81E460E}")
	x.1 := x.Count == 4
	x.Delete(3)
)"

; As of 05/2025 this is still quite limited since Map doesn't implement Item nor Enum directly
m := Map(1, "a", "2", "b", 3, 1, 4, 2)
m.1 := 0
ObjRegisterActive(m, "{6B39CAA1-A320-4CB0-8DB4-352AA81E460E}")
pi := RunScript(script,,, "Keysharp.exe")

if m.1
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

if !m.Has(3)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

ObjRegisterActive(obj, CLSID, Flags:=0) {
	static cookieJar := Map()
	if (!CLSID) {
		if (cookie := cookieJar.Remove(obj)) != ""
			DllCall("oleaut32\RevokeActiveObject", "uint", cookie, "ptr", 0)
		return
	}
	if cookieJar.Has(obj)
		throw Error("Object is already registered", -1)
	_clsid := Buffer(16, 0)
	if (hr := DllCall("ole32\CLSIDFromString", "wstr", CLSID, "ptr", _clsid)) < 0
		throw Error("Invalid CLSID", -1, CLSID)
	hr := DllCall("oleaut32\RegisterActiveObject", "ptr", ObjPtr(obj), "ptr", _clsid, "uint", Flags, "uint*", &cookie:=0, "uint")
	if hr < 0
		throw Error(format("Error 0x{:x}", hr), -1)
	cookieJar[obj] := cookie
}
>>>>>>> e3a6df06efb2a8f333c58e214e8dc50dea820a90
