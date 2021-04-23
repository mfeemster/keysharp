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

x := arr[1]

if (x = 10)
	FileAppend, pass, *
else
	FileAppend, fail, *

len := arr.Length()

if (len == 3)
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

if (arr[1] = 200)
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

if (arr[2] = 601)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[3] = 602)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[4] = 603)
	FileAppend, pass, *
else
	FileAppend, fail, *

if (arr[5] = 700)
	FileAppend, pass, *
else
	FileAppend, fail, *

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
len := arr.Length()

if (len == 3)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := arr.Pop()
len := arr.Length()

if (len == 2 && val == 30)
	FileAppend, pass, *
else
	FileAppend, fail, *

val := arr.Delete(2)
len := arr.Length()

if (len == 2 && val == 20)
	FileAppend, pass, *
else
	FileAppend, fail, *

arr.SetCapacity(200)
cap := arr.GetCapacity()

if (cap == 200)
	FileAppend, pass, *
else
	FileAppend, fail, *
	
cap := arr.Capacity()

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
len := arr.Length()

if (len == 0)
	FileAppend, pass, *
else
	FileAppend, fail, *
