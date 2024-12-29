

x := 1
y := 0
z := 2
a := 2
b := 3
c := 0.9
d := 1.1
e := 0.5
f := 0.8
g := -1
h := 2
i := -3
j := -2
k := -0.9
l := -0.5
m := -0.8

If not (x > y and x < z)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If not (x > z and x < y)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (x > a and x < b)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (x > c and x < d)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If not (x > d and x < c)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (x > e and x < f)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (x > g and x < h)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If not (x > h and x < g)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (x > i and x < j)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (x > j and x < i)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"
	
If not (x > k and x < d)
	FileAppend "fail", "*"
else
	FileAppend "pass", "*"

If not (x > d and x < k)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"

If not (x > l and x < m)
	FileAppend "pass", "*"
else
	FileAppend "fail", "*"