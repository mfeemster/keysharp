; #Include %A_ScriptDir%/header.ahk

x := "one"
arr := { x : 1, "two" : 2, "three" : 3 }
val := arr[x]

if (val = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "one"
y := 1
arr := { x : y, "two" : 2, "three" : 3 }
val := arr[x]

if (val = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := { "one" : 1, "two" : 2, "three" : 3 }
val := arr["one"]

if (val = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := { 123 : 456, "two" : 2, "three" : 3 }
val := arr[123]

if (val = 456)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := { 123.111 : 456, "two" : 2, "three" : 3 }
val := arr[123.111]

if (val = 456)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := { 123.111 : 456.222, "two" : 2, "three" : 3 }
val := arr[123.111]

if (val = 456.222)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := { 0xFEED : 0xF00D, "two" : 2, "three" : 3 }
val := arr[0xFEED]

if (val = 0xF00D)
	FileAppend, pass, *
else
	FileAppend, fail, *

str1 := "one"
str2 := "two"
str3 := "three"

arr := { (str1) : 1, (str2) : 2, (str3) : 3 }
val := arr[str1]

if (val = 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := { "one" : 1, "two" : 2, "three" : 3 }
val := arr.Has("two")

if (val == true)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr2 := arr.Clone()
len := arr2.Count

if (len == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := arr.Delete("one")

if (val == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.Clear()
val := arr.Count

if (val == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.Set("one", 1, "two", 2, "three", 3)
val := arr.Count

if (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.Clear()
arr.Set("one", 1, "two", 2, "three", 3, "fourbad")
val := arr.Count

if (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := Map()
val := arr.Count

if (val == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr := Map("one", 1, "two", 2, "three", 3)
val := arr.Count

if (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := Map( ["one", 1, "two", 2, "three", 3] )
val := arr.Count

if (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
