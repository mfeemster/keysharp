; #Include %A_ScriptDir%/header.ahk

key := "dummynothing123"
s := "a test value"
EnvSet(key, s)
val := EnvGet(key)

if (val == s) 
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
EnvSet(key, null)
val := EnvGet(key)

if (val == "") 
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"