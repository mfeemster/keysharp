;#Include %A_ScriptDir%/header.ahk

x = 11
y11 = 123
z := y%x%

If (z == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (y11 == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == y11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x == 11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x != y11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x != y%x%)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z != x)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == y%x%)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (123 == y%x%)
	FileAppend, pass, *
else
	FileAppend, fail, *

target := 42
second := "target"
val := %second%

if (second == "target")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (val == 42)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (%second% == 42)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := "y"
y11 := 123
z := %x%11

If (z == %x%11)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (z == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (y11 == 123)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z == y11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x == "y")
	FileAppend, pass, *
else
	FileAppend, fail, *

If (x != y11)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (z != x)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
If (123 == %x%11)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := [10, 20, 30]
suffix := "gth"
val := arr.Len%suffix%

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
suffix := "Length"
val := arr.%suffix%

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

suffix := "gth"
val := arr.len%suffix%

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
suffix := "length"
val := arr.%suffix%

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

prefix := "Len"
val := arr.%prefix%gth

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

prefix := "len"
val := arr.%prefix%Gth

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
suffix := "gth"
val := arr.%prefix%%suffix%

If (val == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

suffix := "city"
arr.Capa%suffix% := 1000

If (arr.Capacity == 1000)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (arr.Capa%suffix% == 1000)
	FileAppend, pass, *
else
	FileAppend, fail, *

prefix := "capa"
arr.%prefix%city := 2000

If (arr.Capacity == 2000)
	FileAppend, pass, *
else
	FileAppend, fail, *

If (arr.%prefix%City == 2000)
	FileAppend, pass, *
else
	FileAppend, fail, *

MyArray1 := 10
MyArray2 := 20
MyArray3 := 30
x := 0

Loop 3
    x += MyArray%A_Index%

If (x == 60)
	FileAppend, pass, *
else
	FileAppend, fail, *