; #Include %A_ScriptDir%/header.ahk

arr := [10, 20, 30]
arr2 := [10, 20, 30]

if (arr = arr2)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr == arr2)
	FileAppend, fail, *
else
	FileAppend, pass, *

if (arr[1] = 10)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr[2] = 20)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr[3] = 30)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr[-1] == 30)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr[-2] == 20)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr[-3] == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

x := arr[1]

if (x = 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

len := arr.Length

if (len == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
str := arr.ToString()

if (str == "[10, 20, 30]")
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.Length += 123
len := arr.Length

if (len == 126)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := Array()

arr.Push(10)
arr.Push(20)
arr.Push(30)

if (arr[1] = 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.InsertAt(1, 100)

if (arr[1] = 100)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.InsertAt(1, [ 200 ])

if (arr[1] = [ 200 ])
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr.InsertAt(1, 300, 400, 500)

if (arr[1] = 300)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[2] = 400)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[3] = 500)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr.InsertAt(1, 600, [601, 602, 603], 700)

if (arr[1] = 600)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr[2] = [601, 602, 603])
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[3] = 700)
	FileAppend, pass, *
else
	FileAppend, fail, *

; 
; if (arr[3] = 602)
; 	FileAppend, pass, *
; else
; 	FileAppend, fail, *
; 	
; if (arr[4] = 603)
; 	FileAppend, pass, *
; else
; 	FileAppend, fail, *
; 	
; if (arr[3] = 700)
; 	FileAppend, pass, *
; else
; 	FileAppend, fail, *

arr := Array()

arr.Push(10)
arr.Push(20)
arr.Push(30)

has1 := arr.Has(1)
 
if (has1 = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

has1 := arr.Has(2)
 
if (has1 = true)
	FileAppend, pass, *
else
	FileAppend, fail, *

has1 := arr.Has(3)
 
if (has1 = true)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
has1 := arr.Has(4)
 
if (has1 = false)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr.InsertAt(4, 100)

if (arr[4] = 100)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr.RemoveAt(4)
len := arr.Length

if (len == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
val := arr.Pop()
len := arr.Length

if (len == 2 && val == 30)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
val := arr.Delete(2)
len := arr.Length

if (len == 2 && val == 20)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.Capacity := 200
cap := arr.Capacity

if (cap == 200)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
cap := arr.Capacity

if (cap == 200)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := Array(400, 500, 2, 1000, 10000)
minin := arr.MinIndex()
maxin := arr.MaxIndex()

if (minin == 2 && maxin == 10000)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
arr := Array(1, 2, 3)
arr2 := arr.Clone()

if (arr2[1] == 1)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr2[2] == 2)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
if (arr2[3] == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.Clear()
len := arr.Length

if (len == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := [ "hello" ]
x := arr[1] .= "world"

if (arr[1] == "helloworld")
	FileAppend, pass, *
else
	FileAppend, fail, *

if (x == "helloworld")
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := [10, 20, 30, 40]
i := arr.IndexOf(30)

if (i == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

i := arr.IndexOf(20, 3)

if (i == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

i := arr.IndexOf(40, -1)

if (i == 4)
	FileAppend, pass, *
else
	FileAppend, fail, *

i := arr.IndexOf(40, -2)

if (i == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *

lam := (x) => Mod(x, 5) == 0
arr := [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
filtered := arr.Filter(lam)

if (filtered.Length == 2 && filtered[1] == 5 && filtered[2] == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

filtered := arr.Filter(lam, 6)

if (filtered.Length == 1 && filtered[1] == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

lam := (x) => Mod(x, 5) == 0
arr := [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
filtered := arr.Filter(lam, -2)

if (filtered.Length == 1 && filtered[1] == 5)
	FileAppend, pass, *
else
	FileAppend, fail, *

lam := (x, i) => Mod(x, 5) == 0 && i == x
arr := [10, 20, 30, 40, 5, 60, 70, 80, 90, 10]
filtered := arr.Filter(lam)

if (filtered.Length == 2 && filtered[1] == 5 && filtered[2] == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

filtered := arr.Filter(lam, 6)

if (filtered.Length == 1 && filtered[1] == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

filtered := arr.Filter(lam, -1)

if (filtered.Length == 2 && filtered[1] == 10 && filtered[2] == 5)
	FileAppend, pass, *
else
	FileAppend, fail, *

filtered := arr.Filter(lam, -6)

if (filtered.Length == 1 && filtered[1] == 5)
	FileAppend, pass, *
else
	FileAppend, fail, *

lam := (x, i) => Mod(x, 5) == 0 && i == x
arr := [10, 20, 30, 40, 5, 60, 70, 80, 90, 10]
index := arr.FindIndex(lam)

if (index == 5)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr := [10, 20, 30, 40, 5, 60, 70, 80, 90, 10]
index := arr.FindIndex(lam, 6)

if (index == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

index := arr.FindIndex(lam, -1)

if (index == 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

index := arr.FindIndex(lam, -2)

if (index == 5)
	FileAppend, pass, *
else
	FileAppend, fail, *

str := arr.Join()

if (str == "10,20,30,40,5,60,70,80,90,10")
	FileAppend, pass, *
else
	FileAppend, fail, *

str := arr.Join("-")

if (str == "10-20-30-40-5-60-70-80-90-10")
	FileAppend, pass, *
else
	FileAppend, fail, *

lam := (x, i) => x * i
arr := [10, 20, 30]
arr2 := arr.MapTo(lam)

if (arr2.Length == 3 && arr2[1] == 10 && arr2[2] == 40 && arr2[3] == 90)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr2 := arr.MapTo(lam, 2)

if (arr2.Length == 2 && arr2[1] == 40 && arr2[2] == 90)
	FileAppend, pass, *
else
	FileAppend, fail, *

lam := (l, r) => l < r ? -1 : (l > r ? 1 : 0)
arr := [99, 3, 100, -5, -5, 0]
arr.Sort(lam)
sorted := [-5, -5, 0, 3, 99, 100]

if (arr = sorted)
	FileAppend, pass, *
else
	FileAppend, fail, *
