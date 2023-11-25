; #Include %A_ScriptDir%/header.ahk

x := "one"
m := { %x% : 1, "two" : 2, "three" : 3 }
val := m[x]

if (val = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := "one"
y := 1
m := { %x% : y, "two" : 2, "three" : 3 }
val := m[x]

if (val = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := { "one" : 1, "two" : 2, "three" : 3 }
val := m["one"]

if (val = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := { one : 1, two : 2, three : 3 }
val := m["one"]

if (val = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := m.two

if (val = 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
val := m.three

if (val = 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

str := m.ToString()

if (str == '{"one": 1, "two": 2, "three": 3}')
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m := Map(123, 456, "two", 2, "three", 3 )
val := m[123]

if (val = 456)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m := Map(123.111, 456, "two", 2, "three", 3)
val := m[123.111]

if (val = 456)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map(123.111, 456.222, "two", 2, "three", 3)
val := m[123.111]

if (val = 456.222)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map(0xFEED, 0xF00D, "two", 2, "three", 3)
val := m[0xFEED]

if (val = 0xF00D)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
str1 := "one"
str2 := "two"
str3 := "three"

m := { %str1% : 1, %str2% : 2, %str3% : 3 }
val := m[str1]
val := m.one

if (val = 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := { "one" : 1, "two" : 2, "three" : 3 }
val := m.Has("two")

if (val == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m2 := m.Clone()
len := m2.Count

if (len == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := m.Delete("one")

if (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.Clear()
val := m.Count

if (val == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.Set("one", 1, "two", 2, "three", 3)
val := m.Count

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.Clear()
m.Set("one", 1, "two", 2, "three", 3, "fourbad")
val := m.Count

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.Clear()
arr := ["one", 1, "two", 2, "three", 3]
m.Set(arr)
val := m.Count

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

v1 := m["one"]
v2 := m["two"]
v3 := m["three"]

if (v1 == 1 && v2 == 2 && v3 == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map()
val := m.Count

if (val == 0)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m := Map("one", 1, "two", 2, "three", 3)
val := m.Count

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map( ["one", 1, "two", 2, "three", 3] )
val := m.Count

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m1 := { one : 1, two : 2, three : 3 }
m2 := { four : 4, five : 5, six : 6 }
m3 := { seven : 7, eight : 8, nine : 9 }

m := { %m1% : "mapone", %m2% : "maptwo", %m3% : "mapthree" }

val := m.Count

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m.Delete(m2)

val := m.Count

if (val == 2)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := m.Get(m1)

if (val == "mapone")
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := m.Get(m2, 123)

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

b := false

try
{
	val := m.Get(m2)
}
catch
{
	b := true
}

if (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.Default := 555

val := m.Get(m2)

if (val == 555)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (m.Has(m1))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (!m.Has(m2))
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m := Map()
m.CaseSense := "off"
m.Set("one", 1, "two", 2, "three", 3)

val := m.Has("ONE")

if (val == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := Map()
m.CaseSense := "on"
m.Set("one", 1, "two", 2, "three", 3)

val := m.Has("ONE")

if (val == false)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

b := false

try
{
	m.CaseSense := "off"
}
catch
{
	b := true
}

if (b == true)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.Capacity := 1000
val := m.Capacity

if (val >= 1000) ; Capacity will internall be made to be at least as big as we specified.
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m := { one : 1, two : 2, three : 3 }

if (m.one == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.one := 123

if (m.one == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (m["one"] == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := { one : [1, 1, 1], two : [2, 2, 2], three : [3, 3, 3] }
val := m["one"][1]

if (val == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
m["one"][1] := 123

if (m["one"][1] == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
if (m.one[1] == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := m["one"][1]

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := { one : [[1, 1, 1], [2, 2, 2], [3, 3, 3]] }
val := m["one"][3][1]

if (val == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m.one[3][1] := 123

if (m["one"][3][1] == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

val := m.one[3][1]

if (val == 123)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := {
	one : 1
}

if (m.one == 1)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m := {
	one : { oneone : 11 }
}

if (m.one.oneone == 11)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
