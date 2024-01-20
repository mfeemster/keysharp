arr := [10, 20, 30]
x := 0

for (in arr)
	x++

if (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

for (i in arr)
	x += i
	
if (x == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0

for (i,v in arr)
{
	x += i
	y += v
}

if (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0

for (i,v in arr) {
	x += i
	y += v
}

if (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

for (,v in arr)
	x += v
	
if (x == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

for (i, in arr)
	x += i
	
if (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

for (, in arr)
	x++

if (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0

for (i1,v1 in arr)
{
	for (i2,v2 in arr)
	{
		for (i3,v3 in arr)
		{
			x += i3
			y += v3
		}
	}
}

if (x == 54)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 540)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0

for (i1,v1 in arr) {
	for (i2,v2 in arr) {
		for (i3,v3 in arr) {
			x += i3
			y += v3
		}
	}
}

if (x == 54)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 540)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

arr2 := [arr, arr, arr]
x := 0
y := 0
for (i1,v1 in arr2) ; Test double nested arrays.
{
	for (i2,v2 in v1)
	{
		x += i2
		y += v2
	}
}

if (x == 18)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 180)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

; Same tests, but for map.

m := Map(1, 10, 2, 20, 3, 30)
x := 0

for ( in m)
	x++

if (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0

for (i in m)
	x += i
	
if (x == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
		
x := 0
y := 0

for (i,v in m)
{
	x += i
	y += v
}

if (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0

for (,v in m)
	x += v
	
if (x == 60)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"
	
x := 0

for (i, in m)
	x += i
	
if (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0

for (, in m)
	x++

if (x == 3)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

x := 0
y := 0

for (i1,v1 in m)
{
	for (i2,v2 in m)
	{
		for (i3,v3 in m)
		{
			x += i3
			y += v3
		}
	}
}

if (x == 54)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 540)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

m2 := Map(1, m, 2, m, 3, m)

x := 0
y := 0
for (i1,v1 in m2) ; Test double nested maps.
{
	for (i2,v2 in v1)
	{
		x += i2
		y += v2
	}
}

if (x == 18)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

if (y == 180)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"

funcin()
{
	return [1, 2, 3]
}

x := 0

for w in funcin()
	x += w

if (x == 6)
	FileAppend, "pass", "*"
else
	FileAppend, "fail", "*"